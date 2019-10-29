using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

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
        #region MetadataLocator
        class ThreadSafeEnumerable<T> : IEnumerable<T> {
            readonly IEnumerable<T> list;

            public ThreadSafeEnumerable(IEnumerable<T> list) {
                this.list = list;
            }
            public IEnumerator<T> GetEnumerator() {
                List<T> listCopy;
                lock(list) {
                    listCopy = list.ToList();
                }
                return listCopy.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        static readonly List<Tuple<Type, Type>> internalMetadataProviders = new List<Tuple<Type, Type>>();
        internal static IEnumerable<Tuple<Type, Type>> InternalMetadataProviders { get { return new ThreadSafeEnumerable<Tuple<Type, Type>>(internalMetadataProviders); } }
        readonly static List<WeakReference> locators = new List<WeakReference>();
        internal static void RegisterLocator(MetadataLocator locator) {
            lock(locators) {
                locators.Add(new WeakReference(locator));
            }
        }
        static void UpdateLocators() {
            lock(locators) {
                var locatorsList = locators.ToList();
                for(int i = locatorsList.Count; --i >= 0;) {
                    var locator = (MetadataLocator)locatorsList[i].Target;
                    if(locator == null)
                        locators.RemoveAt(i);
                    else
                        locator.Update();
                }
            }
        }

        public static IEnumerable<Tuple<Type, Type>> GetMetadataInfoList(Type metadataType) {
            Func<Type, Type, bool> isMetadataProviderType =
                (t1, t2) => IsFilteringMetadataProviderType(t1, t2) || IsMetadataProviderType(t1, t2);
            Func<Type, Type, bool> isMetadataBuilderType =
                (t1, t2) => IsFilteringMetadataBuilderType(t1, t2) || IsMetadataBuilderType(t1, t2);
            var newInfoList1 = metadataType.GetInterfaces()
                .Where(x => isMetadataProviderType(x, null))
                .Select(x => {
                    return new Tuple<Type, Type>(
                        GetTypeOrGenericTypeDefinition(x.GetGenericArguments().Single()), metadataType);
                });
            var newInfoList2 = GetBuildMetadataStaticMethods(metadataType, null, isMetadataBuilderType)
                .Select(x => {
                    var buildMetadataMethodEntityType = x.GetParameters().Single().ParameterType.GetGenericArguments().Single();
                    return new Tuple<Type, Type>(
                        GetTypeOrGenericTypeDefinition(buildMetadataMethodEntityType), metadataType);
                });
            return newInfoList1.Concat(newInfoList2);
        }
        public static void AddMetadata(Type type, Type metadataType) {
            var tuple = new Tuple<Type, Type>(type, metadataType);
            AddMetadata(new[] { tuple });
        }
        public static void AddMetadata(Type metadataType) {
            AddMetadata(GetMetadataInfoList(metadataType));
        }
        public static void AddMetadata<T, TMetadata>() {
            AddMetadata(typeof(T), typeof(TMetadata));
        }
        public static void AddMetadata<TMetadata>() {
            AddMetadata(typeof(TMetadata));
        }
        static void AddMetadata(IEnumerable<Tuple<Type, Type>> newInfoList) {
            CheckMetadata(newInfoList);
            lock (internalMetadataProviders) {
                internalMetadataProviders.AddRange(newInfoList);
            }
            UpdateLocators();
            foreach(var info in newInfoList) {
                IAttributesProvider provider;
                Providers.TryRemove(info.Item1, out provider);
                FilteringProviders.TryRemove(info.Item1, out provider);
            }
        }
        public static void ClearMetadata() {
            lock(internalMetadataProviders) {
                internalMetadataProviders.Clear();
            }
            UpdateLocators();
            Providers.Clear();
            FilteringProviders.Clear();
        }

        internal static void CheckMetadata(IEnumerable<Tuple<Type, Type>> metadataTypes) {
            foreach(var tuple in metadataTypes)
                CheckMetadata(tuple.Item2);
        }
        static void CheckMetadata(Type metadataType) {
            if(metadataType == null || metadataType.IsPublic || metadataType.IsNestedPublic)
                return;
            throw new InvalidOperationException(string.Format(Exception_MetadataShouldBePublic, metadataType.Name));
        }
        const string Exception_MetadataShouldBePublic = "The {0} type should be public";
        #endregion
        #region inner types
        class ExternalMetadataAttributesProvider : IAttributesProvider {
            readonly Type metadataClassType;
            readonly Type componentType;
            public ExternalMetadataAttributesProvider(Type metadataClassType, Type componentType) {
                this.metadataClassType = metadataClassType;
                this.componentType = componentType;
            }
            IEnumerable<Attribute> IAttributesProvider.GetAttributes(string propertyName) {
                if(string.IsNullOrEmpty(propertyName))
                    return Enumerable.Empty<Attribute>();
                MemberInfo metadataProperty = (MemberInfo)metadataClassType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
                    ?? metadataClassType.GetMethod(propertyName, BindingFlags.Instance | BindingFlags.Public);
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
        class AttributeReference {
            public readonly Attribute Attribute;
            public AttributeReference(Attribute attribute) {
                Attribute = attribute;
            }
            public static bool operator ==(AttributeReference a, AttributeReference b) {
                bool aIsNull = ReferenceEquals(a, null);
                bool bIsNull = ReferenceEquals(b, null);
                if(aIsNull && bIsNull) return true;
                if(aIsNull || bIsNull) return false;
                return ReferenceEquals(a.Attribute, b.Attribute);
            }
            public override int GetHashCode() {
                return Attribute.GetHashCode();
            }
            public static bool operator !=(AttributeReference a, AttributeReference b) {
                return !(a == b);
            }
            public override bool Equals(object obj) {
                return this == obj as AttributeReference;
            }
        }
        #endregion

        internal static IEnumerable<T> GetAttributes<T>(MemberInfo member, bool inherit = false) where T : Attribute {
            return GetAllAttributes(member, inherit).OfType<T>();
        }
        internal static T GetAttribute<T>(MemberInfo member, bool inherit = false) where T : Attribute {
            return GetAttributes<T>(member, inherit).FirstOrDefault() as T;
        }
        internal static Attribute[] GetAllAttributes(MemberInfo member, bool inherit = false) {
            var externalAndFluentAPIAttrs = GetExternalAndFluentAPIAttributes(member.ReflectedType, member.Name) ?? new Attribute[0];
            return Attribute.GetCustomAttributes(member, inherit).Concat(externalAndFluentAPIAttrs).ToArray();
        }

        public static IAttributesProvider GetAttributesProvider(Type componentType, IMetadataLocator locator) {
            return CompositeMetadataAttributesProvider.Create(
                GetMetadataTypes(locator, componentType).GetProviders(componentType, false));
        }
        public static IAttributesProvider GetFilteringAttributesProvider(Type componentType, IMetadataLocator locator) {
            return CompositeMetadataAttributesProvider.Create(
                GetMetadataTypes(locator, componentType).GetProviders(componentType, true));
        }
        static IEnumerable<IAttributesProvider> GetProviders(this IEnumerable<Type> metadataTypes, Type type, bool forFiltering) {
            return metadataTypes.SelectMany(x => {
                return forFiltering ? GetAllFilteringMetadataAttributes(x, type) : GetAllMetadataAttributes(x, type);
            });
        }
        static IEnumerable<IAttributesProvider> GetAllMetadataAttributes(Type metadataClassType, Type componentType) {
            if(componentType.IsGenericType && metadataClassType.IsGenericTypeDefinition)
                metadataClassType = metadataClassType.MakeGenericType(componentType.GetGenericArguments());
            yield return GetExternalMetadataAttributes(metadataClassType, componentType);
            yield return GetFluentAPIAttributes(metadataClassType, componentType);
            yield return GetFluentAPIAttributesFromStaticMethod(metadataClassType, componentType);
            yield return GetExternalAndFluentAPIAttributesCore(metadataClassType);
        }
        static IEnumerable<IAttributesProvider> GetAllFilteringMetadataAttributes(Type metadataClassType, Type componentType) {
            if(componentType.IsGenericType && metadataClassType.IsGenericTypeDefinition)
                metadataClassType = metadataClassType.MakeGenericType(componentType.GetGenericArguments());
            yield return GetExternalMetadataAttributes(metadataClassType, componentType);
            yield return GetFluentAPIAttributes(metadataClassType, componentType);
            yield return GetFluentAPIAttributesFromStaticMethod(metadataClassType, componentType);
            yield return GetFluentAPIFilteringAttributes(metadataClassType, componentType);
            yield return GetFluentAPIFilteringAttributesFromStaticMethod(metadataClassType, componentType);
            yield return GetExternalAndFluentAPIFilteringAttributesCore(metadataClassType);
        }
        static readonly ConcurrentDictionary<Type, IAttributesProvider> Providers = new ConcurrentDictionary<Type, IAttributesProvider>();
        static readonly ConcurrentDictionary<Type, IAttributesProvider> FilteringProviders = new ConcurrentDictionary<Type, IAttributesProvider>();
        #region obsolete
        [Obsolete("Use the GetAttributesProvider method instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static IAttributesProvider GetAttrbutesProvider(Type componentType, IMetadataLocator locator) {
            return GetAttributesProvider(componentType, locator);
        }
        [Obsolete("Use the GetFilteringAttributesProvider method instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static IAttributesProvider GetFilteringAttrbutesProvider(Type componentType, IMetadataLocator locator) {
            return GetFilteringAttributesProvider(componentType, locator);
        }
        [Obsolete("Use the GetExternalAndFluentAPIAttributes method instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<Attribute> GetExternalAndFluentAPIAttrbutes(Type componentType, string propertyName) {
            return GetExternalAndFluentAPIAttributes(componentType, propertyName);
        }
        [Obsolete("Use the GetExternalAndFluentAPIFilteringAttributes method instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<Attribute> GetExternalAndFluentAPIFilteringAttrbutes(Type componentType, string propertyName) {
            return GetExternalAndFluentAPIFilteringAttributes(componentType, propertyName);
        }
        #endregion
        public static IEnumerable<Attribute> GetExternalAndFluentAPIAttributes(Type componentType, string propertyName) {
            var attributesProvider = Providers.GetOrAdd(componentType, x => GetExternalAndFluentAPIAttributesCore(x));
            return GetExternalAndFluentAPIAttributes(attributesProvider, propertyName);
        }
        public static IEnumerable<Attribute> GetExternalAndFluentAPIFilteringAttributes(Type componentType, string propertyName) {
            var attributesProvider = Providers.GetOrAdd(componentType, x => GetExternalAndFluentAPIAttributesCore(x));
            var filteringAttributesProvider = FilteringProviders.GetOrAdd(componentType, x => GetExternalAndFluentAPIFilteringAttributesCore(x));
            return GetExternalAndFluentAPIAttributes(CompositeMetadataAttributesProvider.Create(
                new List<IAttributesProvider>() { attributesProvider , filteringAttributesProvider }), propertyName);
        }
        static IEnumerable<Attribute> GetExternalAndFluentAPIAttributes(IAttributesProvider attributesProvider, string propertyName) {
            lock (attributesProvider) {
                var attributes = attributesProvider.GetAttributes(propertyName);
                var groupedAttributes = attributes.SelectMany(attr => GetAttributeTypes(attr).Select(x => new { type = x, value = attr })).GroupBy(x => x.type);
                var attributeCategories = groupedAttributes.GroupBy(g => g.Key.GetCustomAttributes(typeof(AttributeUsageAttribute), true).Cast<AttributeUsageAttribute>().Single().AllowMultiple).ToList();
                var simpleAttributes = attributeCategories.Where(x => x.Key).SelectMany(x => x).SelectMany(x => x).Select(x => x.value);
                var uniqueAttributes = attributeCategories.Where(x => !x.Key).SelectMany(x => x).Select(x => x.Last()).Select(x => x.value);
                return simpleAttributes.Concat(uniqueAttributes).Select(x => new AttributeReference(x)).GroupBy(x => x).Select(x => x.Key.Attribute).ToList();
            }
        }

        static IAttributesProvider GetExternalAndFluentAPIAttributesCore(Type componentType) {
            return GetExternalAndFluentAPIAttributesCore(componentType, false);
        }
        static IAttributesProvider GetExternalAndFluentAPIFilteringAttributesCore(Type componentType) {
            return CompositeMetadataAttributesProvider.Create(new List<IAttributesProvider>() {
                GetExternalAndFluentAPIAttributesCore(componentType, false),
                GetExternalAndFluentAPIAttributesCore(componentType, true)
            });
        }
        static IAttributesProvider GetExternalAndFluentAPIAttributesCore(Type componentType, bool forFiltering) {
            IEnumerable<Type> hierarchy = componentType.Yield().Flatten(x => x.BaseType.YieldIfNotNull()).Reverse();
            IEnumerable<IAttributesProvider> result = new IAttributesProvider[0];
            foreach(var type in hierarchy) {
                IEnumerable<Type> metadataClassType =
                    (forFiltering ? GetFilteringMetadataClassType(type) : GetMetadataClassType(type))
                    .Return(x => new[] { x }, () => Enumerable.Empty<Type>());
                IEnumerable<Type> externalMetadataClassTypes = GetMetadataTypes(MetadataLocator.Default, type);
                result = result.Concat(metadataClassType.Concat(externalMetadataClassTypes).GetProviders(type, forFiltering));
                (forFiltering
                    ? GetFluentAPIFilteringAttributesFromStaticMethod(type, type)
                    : GetFluentAPIAttributesFromStaticMethod(type, type))
                .Do(x => result = result.Concat(new[] { x }));
            }
            return CompositeMetadataAttributesProvider.Create(result);
        }
        public static IEnumerable<Attribute> GetFluentAPIAttributes(Type metadataClassType, Type componentType, string propertyName) {
            return GetFluentAPIAttributes(metadataClassType, componentType).
                With(x => x.GetAttributes(propertyName));
        }
        public static IEnumerable<Attribute> GetFluentAPIFilteringAttributes(Type metadataClassType, Type componentType, string propertyName) {
            var fluentAPI = GetFluentAPIAttributes(metadataClassType, componentType);
            var filteringFluentAPI = GetFluentAPIFilteringAttributes(metadataClassType, componentType);
            var res = CompositeMetadataAttributesProvider.Create(new List<IAttributesProvider> { fluentAPI, filteringFluentAPI });
            return res.GetAttributes(propertyName);
        }
        static IAttributesProvider GetFluentAPIAttributes(Type metadataClassType, Type componentType) {
            return GetFluentAPIAttributesCore(metadataClassType, componentType, false);
        }
        static IAttributesProvider GetFluentAPIFilteringAttributes(Type metadataClassType, Type componentType) {
            return GetFluentAPIAttributesCore(metadataClassType, componentType, true);
        }
        static IAttributesProvider GetFluentAPIAttributesCore(Type metadataClassType, Type componentType, bool forFiltering) {
            MethodInfo buildMetadataMethod =
                GetBuildMetadataMethodsFromMatadataProvider(metadataClassType, componentType,
                    forFiltering ? (Func<Type, Type, bool>)IsFilteringMetadataProviderType : (Func<Type, Type, bool>)IsMetadataProviderType)
                .SingleOrDefault();
            return buildMetadataMethod != null
                ? InvokeBuildMetadataMethodFromMetadataProvider(buildMetadataMethod, metadataClassType, componentType, forFiltering) : null;
        }

        static IAttributesProvider GetFluentAPIAttributesFromStaticMethod(Type metadataClassType, Type componentType) {
            return GetFluentAPIAttributesFromStaticMethodCore(metadataClassType, componentType, false);
        }
        static IAttributesProvider GetFluentAPIFilteringAttributesFromStaticMethod(Type metadataClassType, Type componentType) {
            return GetFluentAPIAttributesFromStaticMethodCore(metadataClassType, componentType, true);
        }
        static IAttributesProvider GetFluentAPIAttributesFromStaticMethodCore(Type metadataClassType, Type componentType, bool forFiltering) {
            MethodInfo buildMetadataMethod =
                GetBuildMetadataStaticMethods(metadataClassType, componentType,
                    forFiltering ? (Func<Type, Type, bool>)IsFilteringMetadataBuilderType : (Func<Type, Type, bool>)IsMetadataBuilderType).
                SingleOrDefault();
            return buildMetadataMethod != null
                ? InvokeBuildMetadataStaticMethod(buildMetadataMethod, componentType, forFiltering) : null;
        }

        static IAttributesProvider GetExternalMetadataAttributes(Type metadataClassType, Type componentType) {
            return metadataClassType.GetMembers(BindingFlags.Public | BindingFlags.Instance).Any()
                ? new ExternalMetadataAttributesProvider(metadataClassType, componentType)
                : null;
        }
        static Type GetMetadataClassType(Type componentType) {
            return GetMetadataClassTypeCore(componentType);
        }
        static Type GetFilteringMetadataClassType(Type componentType) {
            return GetFilteringMetadataClassTypeCore(componentType);
        }
        static Type GetMetadataClassTypeCore(Type componentType) {
            Type metadataTypeAttributeType = componentType.IsEnum ? typeof(EnumMetadataTypeAttribute) :
                typeof(MetadataTypeAttribute);
            object[] metadataTypeAttributes = componentType.GetCustomAttributes(metadataTypeAttributeType, false);
            if(metadataTypeAttributes == null || !metadataTypeAttributes.Any())
                return null;
            return (Type)metadataTypeAttributes[0].GetType().GetProperty("MetadataClassType", BindingFlags.Instance | BindingFlags.Public).GetValue(metadataTypeAttributes[0], null);
        }
        static Type GetFilteringMetadataClassTypeCore(Type componentType) {
            if(componentType.IsEnum) return null;
            var attrs = componentType.GetCustomAttributes(false);
            if(attrs == null || !attrs.Any()) return null;
            object metadataTypeAttribute = attrs.SingleOrDefault(x => {
                Type attrType = x.GetType();
                return attrType.Name == FilterMetadataTypeAttributeHelper.FilteringMetadataTypeName
                && attrType.Namespace == FilterMetadataTypeAttributeHelper.FilteringMetadataTypeNamespace;
            });
            if(metadataTypeAttribute == null) return null;
            return (Type)metadataTypeAttribute.GetType().GetProperty("MetadataClassType", BindingFlags.Instance | BindingFlags.Public).GetValue(metadataTypeAttribute, null);
        }
        const string BuildMetadataMethodName = "BuildMetadata";
        static IEnumerable<MethodInfo> GetBuildMetadataMethodsFromMatadataProvider(Type metadataClassType, Type componentType, Func<Type, Type, bool> isMetadataProviderType) {
            bool isPublic = metadataClassType.IsPublic || metadataClassType.IsNestedPublic;
            if(metadataClassType.IsAbstract || !isPublic || metadataClassType.GetConstructor(new Type[0]) == null)
                return Enumerable.Empty<MethodInfo>();
            return metadataClassType.GetInterfaces().Where(x => {
                return isMetadataProviderType(x, componentType);
            }).Select(x => {
                return x.GetMethod(BuildMetadataMethodName, BindingFlags.Instance | BindingFlags.Public);
            });
        }
        static IEnumerable<MethodInfo> GetBuildMetadataStaticMethods(Type metadataClassType, Type componentType, Func<Type, Type, bool> isMetadataBuilderType) {
            return metadataClassType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(x => {
                    if(x.Name != BuildMetadataMethodName || x.ReturnType != typeof(void))
                        return false;
                    if(x.GetParameters().Length != 1)
                        return false;
                    var parameter = x.GetParameters().Single();
                    return isMetadataBuilderType(parameter.ParameterType, componentType);
                });
        }
        static IAttributesProvider InvokeBuildMetadataMethodFromMetadataProvider(MethodInfo method, Type metadataClassType, Type componentType, bool forFiltering) {
            IAttributesProvider result = forFiltering ? CreateFilteringMetadataBuilder(componentType) : CreateMetadataBuilder(componentType);
            method.Invoke(Activator.CreateInstance(metadataClassType), new[] { result });
            return result;
        }
        static IAttributesProvider InvokeBuildMetadataStaticMethod(MethodInfo method, Type componentType, bool forFiltering) {
            IAttributesProvider result = forFiltering ? CreateFilteringMetadataBuilder(componentType) : CreateMetadataBuilder(componentType);
            method.Invoke(null, new[] { result });
            return result;
        }
        static bool IsMetadataProviderType(Type type, Type componentType) {
            return IsMetadataBuilderTypeCore(type, componentType, typeof(IMetadataProvider<>), typeof(IEnumMetadataProvider<>));
        }
        static bool IsMetadataBuilderType(Type type, Type componentType) {
            return IsMetadataBuilderTypeCore(type, componentType, typeof(MetadataBuilder<>), typeof(EnumMetadataBuilder<>));
        }
        static bool IsMetadataBuilderTypeCore(Type type, Type componentType, Type metadataBuilderOrProviderType, Type enumMetadataBuilderOrProviderType) {
            if(!type.IsGenericType) return false;
            var genericArgument = type.GetGenericArguments().Single();
            if(componentType != null && componentType != genericArgument)
                return false;
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if(genericTypeDefinition == metadataBuilderOrProviderType)
                return componentType == null ? true : !componentType.IsEnum;
            if(genericTypeDefinition == enumMetadataBuilderOrProviderType)
                return componentType == null ? true : componentType.IsEnum;
            return false;
        }
        static bool IsFilteringMetadataProviderType(Type type, Type componentType) {
            return IsFilteringMetadataBuilderTypeCore(type, componentType, typeof(IFilteringMetadataProvider<>));
        }
        static bool IsFilteringMetadataBuilderType(Type type, Type componentType) {
            return IsFilteringMetadataBuilderTypeCore(type, componentType, typeof(FilteringMetadataBuilder<>));
        }
        static bool IsFilteringMetadataBuilderTypeCore(Type type, Type componentType, Type filteringMetadataBuilderOrProviderType) {
            if(!type.IsGenericType) return false;
            var genericArgument = type.GetGenericArguments().Single();
            if(componentType != null && componentType != genericArgument)
                return false;
            if(componentType != null && componentType.IsEnum)
                return false;
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if(genericTypeDefinition == filteringMetadataBuilderOrProviderType)
                return true;
            return false;
        }
        static IAttributesProvider CreateMetadataBuilder(Type componentType) {
            Type metadataBuilderType = componentType.IsEnum ? typeof(EnumMetadataBuilder<>) : typeof(MetadataBuilder<>);
            metadataBuilderType = metadataBuilderType.MakeGenericType(componentType);
            return (IAttributesProvider)Activator.CreateInstance(metadataBuilderType);
        }
        static IAttributesProvider CreateFilteringMetadataBuilder(Type componentType) {
            Type metadataBuilderType = typeof(FilteringMetadataBuilder<>);
            metadataBuilderType = metadataBuilderType.MakeGenericType(componentType);
            return (IAttributesProvider)Activator.CreateInstance(metadataBuilderType);
        }
        static IEnumerable<Type> GetMetadataTypes(IMetadataLocator locator, Type type) {
            return (locator ?? MetadataLocator.Create()).GetMetadataTypes(GetTypeOrGenericTypeDefinition(type))
                .Return(x => x, () => Enumerable.Empty<Type>());
        }
        static Type GetTypeOrGenericTypeDefinition(Type componentType) {
            return componentType.IsGenericType ? componentType.GetGenericTypeDefinition() : componentType;
        }
        static IEnumerable<Type> GetAttributeTypes(Attribute attr) {
            return LinqExtensions.Unfold(attr.GetType(), x => x.BaseType, x => x == typeof(Attribute));
        }

        static class FilterMetadataTypeAttributeHelper {
            public const string FilteringMetadataTypeName = "FilterMetadataTypeAttribute";
            public const string FilteringMetadataTypeNamespace = "DevExpress.Utils.Filtering";
        }
        class FilteringMetadataBuilder<T> { }
        interface IFilteringMetadataProvider<T> { }
    }
}