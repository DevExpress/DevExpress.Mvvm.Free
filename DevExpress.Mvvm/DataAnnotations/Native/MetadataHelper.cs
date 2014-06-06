using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevExpress.Mvvm.Native {
    public interface IAttributesProvider {
        IEnumerable<Attribute> GetAttributes(string propertyName);
    }
    internal interface IPropertyMetadataBuilder {
        IEnumerable<Attribute> Attributes { get; }
    }

    interface IAttributeProxy {
        Attribute CreateRealAttribute();
    }
    class PropertyMetadataStorage {
        Dictionary<Type, Attribute> attributes = new Dictionary<Type, Attribute>();
        List<Attribute> multipleAttributes = new List<Attribute>();
        internal void AddOrModifyAttribute<TAttribute>(Action<TAttribute> setAttributeValue = null) where TAttribute : Attribute, new() {
            Attribute attribute = attributes.GetOrAdd(typeof(TAttribute), () => new TAttribute());
            if(setAttributeValue != null)
                setAttributeValue((TAttribute)attribute);
        }
        internal void AddOrReplaceAttribute<TAttribute>(TAttribute attribute) where TAttribute : Attribute {
            attributes[typeof(TAttribute)] = attribute;
        }
        internal void AddAttribute(Attribute attribute) {
            multipleAttributes.Add(attribute);
        }
        internal IEnumerable<Attribute> GetAttributes() {
            return attributes.Values.Select(x => {
                if(x is IAttributeProxy)
                    return ((IAttributeProxy)x).CreateRealAttribute();
                return x;
            }).Concat(multipleAttributes);
        }
    }

    public static class MetadataHelper {
        #region
        class ExtenalMetadataAttributesProvider : IAttributesProvider {
            readonly Type metadataClassType;
            public ExtenalMetadataAttributesProvider(Type metadataClassType) {
                this.metadataClassType = metadataClassType;
            }
            IEnumerable<Attribute> IAttributesProvider.GetAttributes(string propertyName) {
                MemberInfo metadataProperty = (MemberInfo)metadataClassType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public) ?? metadataClassType.GetMethod(propertyName, BindingFlags.Instance | BindingFlags.Public);
                if(metadataProperty != null)
                    return metadataProperty.GetCustomAttributes(true).OfType<Attribute>();
                return new Attribute[0];
            }
        }
        class CompositeMetadataAttributesProvider : IAttributesProvider {
            readonly IAttributesProvider[] providers;
            public CompositeMetadataAttributesProvider(IAttributesProvider[] providers) {
                this.providers = providers;
            }
            IEnumerable<Attribute> IAttributesProvider.GetAttributes(string propertyName) {
                return providers.Select(x => x.GetAttributes(propertyName)).Where(x => x != null).SelectMany(x => x);
            }
        }
        #endregion

        const string BuildMetadataMethodName = "BuildMetadata";
        [ThreadStatic]
        static Dictionary<Type, IAttributesProvider> providers;
        static Dictionary<Type, IAttributesProvider> Providers { get { return providers ?? (providers = new Dictionary<Type, IAttributesProvider>()); } }

        internal static T GetAttribute<T>(MemberInfo member) where T : Attribute {
            return GetAllAttributes(member).FirstOrDefault(x => x is T) as T;
        }

        internal static Attribute[] GetAllAttributes(MemberInfo member) {
            return member.GetCustomAttributes(false).OfType<Attribute>().Concat(GetExtenalAndFluentAPIAttrbutes(member.ReflectedType, member.Name) ?? new Attribute[0]).ToArray();
        }

        public static IEnumerable<Attribute> GetExtenalAndFluentAPIAttrbutes(Type componentType, string propertyName) {
            return Providers.GetOrAdd(componentType, () => GetExtenalAndFluentAPIAttrbutesCore(componentType)).GetAttributes(propertyName);
        }
        static IAttributesProvider GetExtenalAndFluentAPIAttrbutesCore(Type componentType) {
            IEnumerable<IAttributesProvider> result = new IAttributesProvider[0];
            while(componentType != null) {
                Type metadataClassType = DataAnnotationsAttributeHelper.GetMetadataClassType(componentType);
                if(metadataClassType != null) {
                    result = result.Concat(GetAllMetadataAttributes(metadataClassType, componentType));
                }
                Type[] externalMetadataClassTypes = MetadataLocator.Default.With(x => x.GetMetadataTypes(GetTypeOrGenericTypeDefinition(componentType)));
                if(externalMetadataClassTypes != null) {
                    foreach(var externalMetadataClassType in externalMetadataClassTypes) {
                        result = result.Concat(GetAllMetadataAttributes(externalMetadataClassType, componentType));
                    }
                }
                GetFluentAPIAttributesFromStaticMethod(componentType, componentType).Do(x => result = result.Concat(new[] { x }));
                componentType = componentType.BaseType;
            }
            return new CompositeMetadataAttributesProvider(result.Where(x => x != null).ToArray());
        }
        static IEnumerable<IAttributesProvider> GetAllMetadataAttributes(Type metadataClassType, Type componentType) {
            if(componentType.IsGenericType && metadataClassType.IsGenericTypeDefinition) {
                metadataClassType = metadataClassType.MakeGenericType(componentType.GetGenericArguments());
            }
            yield return GetExternalMetadataAttributes(metadataClassType);
            yield return GetFluentAPIAttributes(metadataClassType, componentType);
            yield return GetFluentAPIAttributesFromStaticMethod(metadataClassType, componentType);
        }
        internal static Type GetTypeOrGenericTypeDefinition(Type componentType) {
            return componentType.IsGenericType ? componentType.GetGenericTypeDefinition() : componentType;
        }

        public static IEnumerable<Attribute> GetFluentAPIAttributes(Type metadataClassType, Type componentType, string propertyName) {
            return GetFluentAPIAttributes(metadataClassType, componentType).With(x => x.GetAttributes(propertyName));
        }
        static IAttributesProvider GetFluentAPIAttributes(Type metadataClassType, Type componentType) {
            bool isPublic = metadataClassType.IsPublic || metadataClassType.IsNestedPublic;
            if(metadataClassType.IsAbstract || !isPublic || metadataClassType.GetConstructor(new Type[0]) == null)
                return null;
            Type metadatProviderInterfaceType = metadataClassType.GetInterfaces().SingleOrDefault(x => {
                return x.GetGenericTypeDefinition() == typeof(IMetadataProvider<>) && x.GetGenericArguments().Single() == componentType;
            });
            if(metadatProviderInterfaceType == null)
                return null;
            IAttributesProvider result = CreateBuilder(componentType);
            object provider = Activator.CreateInstance(metadataClassType);
            metadatProviderInterfaceType.GetMethod(BuildMetadataMethodName, BindingFlags.Instance | BindingFlags.Public).Invoke(provider, new[] { result });
            return result;
        }
        static IAttributesProvider GetFluentAPIAttributesFromStaticMethod(Type metadataClassType, Type componentType) {
            MethodInfo buildMetadataMethod = GetBuildMetadataMethods(metadataClassType)
                .Where(x => GetBuildMetadataMethodEntityType(x) == componentType)
                .SingleOrDefault();
            if(buildMetadataMethod == null)
                return null;
            var key = new Tuple<Type, Type>(componentType, metadataClassType);
            IAttributesProvider result = CreateBuilder(componentType);
            buildMetadataMethod.Invoke(null, new[] { result });
            return result;
        }
        static IAttributesProvider GetExternalMetadataAttributes(Type metadataClassType) {
            return metadataClassType.GetMembers(BindingFlags.Public | BindingFlags.Instance).Any()
                ? new ExtenalMetadataAttributesProvider(metadataClassType)
                : null;
        }
        internal static Type GetBuildMetadataMethodEntityType(MethodInfo x) {
            return x.GetParameters().Single().ParameterType.GetGenericArguments().Single();
        }
        internal static IEnumerable<MethodInfo> GetBuildMetadataMethods(Type metadataClassType) {
            return metadataClassType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                            .Where(x => {
                                if(x.Name != BuildMetadataMethodName || x.ReturnType != typeof(void))
                                    return false;
                                if(x.GetParameters().Length != 1)
                                    return false;
                                var parameter = x.GetParameters().Single();
                                if(!parameter.ParameterType.IsGenericType || parameter.ParameterType.GetGenericTypeDefinition() != typeof(MetadataBuilder<>))
                                    return false;
                                return true;
                            });
        }
        static IAttributesProvider CreateBuilder(Type componentType) {
            return (IAttributesProvider)Activator.CreateInstance(typeof(MetadataBuilder<>).MakeGenericType(componentType));
        }
    }
}