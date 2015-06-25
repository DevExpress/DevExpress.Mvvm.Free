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
    class MemberMetadataStorage {
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
        #region external types
        class ExternalMetadataAttributesProvider : IAttributesProvider {
            readonly Type metadataClassType;
            public ExternalMetadataAttributesProvider(Type metadataClassType) {
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
            public static IAttributesProvider Create(IEnumerable<IAttributesProvider> providers) {
                return new CompositeMetadataAttributesProvider(providers.Where(x => x != null).ToArray());
            }
            readonly IAttributesProvider[] providers;
            CompositeMetadataAttributesProvider(IAttributesProvider[] providers) {
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
            return GetAllAttributes(member).OfType<T>().FirstOrDefault() as T;
        }

        internal static Attribute[] GetAllAttributes(MemberInfo member) {
            return member.GetCustomAttributes(false).OfType<Attribute>().Concat(GetExternalAndFluentAPIAttrbutes(member.ReflectedType, member.Name) ?? new Attribute[0]).ToArray();
        }

        public static IEnumerable<Attribute> GetExternalAndFluentAPIAttrbutes(Type componentType, string propertyName) {
            return Providers.GetOrAdd(componentType, () => GetExternalAndFluentAPIAttrbutesCore(componentType)).GetAttributes(propertyName);
        }
        public static IAttributesProvider GetAttrbutesProvider(Type componentType, IMetadataLocator locator) {
            return CompositeMetadataAttributesProvider.Create(GetMetadataTypes(locator, componentType).GetProviders(componentType));
        }
        static IAttributesProvider GetExternalAndFluentAPIAttrbutesCore(Type componentType) {
            IEnumerable<IAttributesProvider> result = new IAttributesProvider[0];
            foreach(var type in GetHierarchy(componentType)) {
                IEnumerable<Type> metadataClassType = DataAnnotationsAttributeHelper.GetMetadataClassType(type)
                    .Return(x => new[] { x }, () => Enumerable.Empty<Type>());
                IEnumerable<Type> externalMetadataClassTypes = GetMetadataTypes(MetadataLocator.Default, type);
                result = result.Concat(metadataClassType.Concat(externalMetadataClassTypes).GetProviders(type));
                GetFluentAPIAttributesFromStaticMethod(type, type).Do(x => result = result.Concat(new[] { x }));
            }
            return CompositeMetadataAttributesProvider.Create(result);
        }
        static IEnumerable<IAttributesProvider> GetProviders(this IEnumerable<Type> metadataTypes, Type type) {
            return metadataTypes.SelectMany(x => GetAllMetadataAttributes(x, type));

        }
        static IEnumerable<Type> GetMetadataTypes(this IMetadataLocator locator, Type type) {
            return locator.With(x => x.GetMetadataTypes(GetTypeOrGenericTypeDefinition(type)))
                                .Return(x => x, () => Enumerable.Empty<Type>());
        }

        static IEnumerable<Type> GetHierarchy(Type type) {
            while(type != null) {
                yield return type;
                type = type.BaseType;
            }
        }
        static IEnumerable<IAttributesProvider> GetAllMetadataAttributes(Type metadataClassType, Type componentType) {
            if(componentType.IsGenericType && metadataClassType.IsGenericTypeDefinition) {
                metadataClassType = metadataClassType.MakeGenericType(componentType.GetGenericArguments());
            }
            yield return GetExternalMetadataAttributes(metadataClassType);
            yield return GetFluentAPIAttributes(metadataClassType, componentType);
            yield return GetFluentAPIAttributesFromStaticMethod(metadataClassType, componentType);

            yield return GetExternalAndFluentAPIAttrbutesCore(metadataClassType);
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
                Type expectedProviderType = componentType.IsEnum ? typeof(IEnumMetadataProvider<>) : typeof(IMetadataProvider<>);
                return x.GetGenericTypeDefinition() == expectedProviderType && x.GetGenericArguments().Single() == componentType;
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
                ? new ExternalMetadataAttributesProvider(metadataClassType)
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
                                if(!parameter.ParameterType.IsGenericType ||
                                    (parameter.ParameterType.GetGenericTypeDefinition() != typeof(MetadataBuilder<>) && parameter.ParameterType.GetGenericTypeDefinition() != typeof(EnumMetadataBuilder<>)))
                                    return false;
                                return true;
                            });
        }
        static IAttributesProvider CreateBuilder(Type componentType) {
            return (IAttributesProvider)Activator.CreateInstance(GetMetadataBuilderType(componentType).MakeGenericType(componentType));
        }
        static Type GetMetadataBuilderType(Type componentType) {
            return componentType.IsEnum ? typeof(EnumMetadataBuilder<>) : typeof(MetadataBuilder<>);
        }
    }
}