using DevExpress.Internal;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DevExpress.Mvvm.Native {
    public static class ViewModelSourceHelper {
        public static Type GetProxyType(Type type) {
            return DevExpress.Mvvm.POCO.ViewModelSource.GetPOCOType(type);
        }
        public static object Create(Type type) {
            return DevExpress.Mvvm.POCO.ViewModelSource.Create(type);
        }
        public static bool IsPOCOViewModelType(Type type) {
            return DevExpress.Mvvm.POCO.ViewModelSource.IsPOCOViewModelType(type);
        }
    }
}
namespace DevExpress.Mvvm.POCO {
    public interface IPOCOViewModel {
        void RaisePropertyChanged(string propertyName);
    }
    public class ViewModelSource {
        #region
        internal const string Error_ObjectDoesntImplementIPOCOViewModel = "Object doesn't implement IPOCOViewModel.";
        internal const string Error_CommandNotFound = "Command not found: {0}.";
        internal const string Error_CommandNotAsync = "Command is not async";
        const string Error_ConstructorNotFound = "Constructor not found.";
        const string Error_TypeHasNoCtors = "Type has no accessible constructors: {0}.";

        const string Error_SealedClass = "Cannot create dynamic class for the sealed class: {0}.";
        const string Error_PrivateClass = "Cannot create dynamic class for the private class: {0}.";
        const string Error_TypeImplementsIPOCOViewModel = "Type cannot implement IPOCOViewModel: {0}.";

        const string Error_RaisePropertyChangedMethodNotFound = "Class already supports INotifyPropertyChanged, but RaisePropertyChanged(string) method not found: {0}.";
        const string Error_PropertyIsNotVirual = "Cannot make non-virtual property bindable: {0}.";
        const string Error_PropertyHasInternalSetter = "Cannot make property with internal setter bindable: {0}.";
        const string Error_PropertyHasNoSetter = "Cannot make property without setter bindable: {0}.";
        const string Error_PropertyHasNoGetter = "Cannot make property without public getter bindable: {0}.";
        const string Error_PropertyIsFinal = "Cannot override final property: {0}.";
        const string Error_MoreThanOnePropertyChangedMethod = "More than one property changed method: {0}.";
        const string Error_PropertyChangedMethodShouldBePublicOrProtected = "Property changed method should be public or protected: {0}.";
        const string Error_PropertyChangedCantHaveMoreThanOneParameter = "Property changed method cannot have more than one parameter: {0}.";
        const string Error_PropertyChangedCantHaveReturnType = "Property changed method cannot have return type: {0}.";
        const string Error_PropertyChangedMethodArgumentTypeShouldMatchPropertyType = "Property changed method argument type should match property type: {0}.";
        const string Error_PropertyChangedMethodNotFound = "Property changed method not found: {0}.";

        const string Error_MemberWithSameCommandNameAlreadyExists = "Member with the same command name already exists: {0}.";

        const string Error_PropertyTypeShouldBeServiceType = "Property type should be service type: {0}.";
        const string Error_CantAccessProperty = "Cannot access property: {0}.";
        const string Error_PropertyIsNotVirtual = "Property is not virtual: {0}.";
        const string Error_PropertyHasSetter = "Property with setter cannot be Service Property: {0}.";

        const string Error_ConstructorExpressionCanReferOnlyToItsArguments = "Constructor expression can refer only to its arguments.";
        const string Error_ConstructorExpressionCanOnlyBeOfNewExpressionType = "Constructor expression can only be of NewExpression type.";
        const string Error_IDataErrorInfoAlreadyImplemented = "The IDataErrorInfo interface is already implemented.";
        #endregion

        static readonly Dictionary<Type, ICustomAttributeBuilderProvider> attributeBuilderProviders = new Dictionary<Type, ICustomAttributeBuilderProvider>();
        static ViewModelSource() {
            RegisterAttributeBuilderProvider(new DisplayAttributeBuilderProvider());
#if !SILVERLIGHT
            RegisterAttributeBuilderProvider(new DisplayNameAttributeBuilderProvider());
            RegisterAttributeBuilderProvider(new ScaffoldColumnAttributeBuilderProvider());
#endif
        }
        static void RegisterAttributeBuilderProvider(ICustomAttributeBuilderProvider provider) {
            attributeBuilderProviders[provider.AttributeType] = provider;
        }

        static readonly Dictionary<Type, Type> types = new Dictionary<Type, Type>();
        static readonly Dictionary<Assembly, ModuleBuilder> builders = new Dictionary<Assembly, ModuleBuilder>();
        static readonly Dictionary<Type, object> Factories = new Dictionary<Type, object>();

        public static T Create<T>() where T : class, new() {
            return Factory(() => new T())();
        }
        public static T Create<T>(Expression<Func<T>> constructorExpression) where T : class {
            ValidateCtorExpression(constructorExpression, false);
            var actualAxpression = GetCtorExpression(constructorExpression, typeof(T), false);
            return Expression.Lambda<Func<T>>(actualAxpression).Compile()();
        }
        #region
        public static Func<TResult> Factory<TResult>(Expression<Func<TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, TResult> Factory<T1, TResult>(Expression<Func<T1, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, T2, TResult> Factory<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, T2, T3, TResult> Factory<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, T2, T3, T4, TResult> Factory<T1, T2, T3, T4, TResult>(Expression<Func<T1, T2, T3, T4, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, T2, T3, T4, T5, TResult> Factory<T1, T2, T3, T4, T5, TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, T2, T3, T4, T5, T6, TResult> Factory<T1, T2, T3, T4, T5, T6, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> Factory<T1, T2, T3, T4, T5, T6, T7, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Factory<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Factory<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Factory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> constructorExpression) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult));
        }
        #endregion

        #region
        static TDelegate GetFactoryCore<TDelegate>(Expression<TDelegate> constructorExpression, Type resultType) {
            ValidateCtorExpression(constructorExpression, true);
            return GetFactoryCore<TDelegate>(() => CreateFactory(constructorExpression, resultType));
        }
        internal static TDelegate GetFactoryCore<TDelegate>(Func<TDelegate> createFactoryDelegate) {
            return (TDelegate)Factories.GetOrAdd(typeof(TDelegate), () => createFactoryDelegate());
        }
        static TDelegate CreateFactory<TDelegate>(Expression<TDelegate> constructorExpression, Type resultType) {
            var actualAxpression = GetCtorExpression(constructorExpression, resultType, true);
            return Expression.Lambda<TDelegate>(actualAxpression, constructorExpression.Parameters).Compile();
        }

        static void ValidateCtorExpression(LambdaExpression constructorExpression, bool useOnlyParameters) {
            NewExpression newExpression = constructorExpression.Body as NewExpression;
            if(newExpression != null) {
                if(useOnlyParameters) {
                    foreach(var item in newExpression.Arguments) {
                        if(!(item is ParameterExpression))
                            throw new ViewModelSourceException(Error_ConstructorExpressionCanReferOnlyToItsArguments);
                    }
                }
                return;
            }
            if(!useOnlyParameters) {
                MemberInitExpression memberInitExpression = constructorExpression.Body as MemberInitExpression;
                if(memberInitExpression != null) {
                    return;
                }
            }
            throw new ViewModelSourceException(Error_ConstructorExpressionCanOnlyBeOfNewExpressionType);
        }
        static Expression GetCtorExpression(LambdaExpression constructorExpression, Type resultType, bool useOnlyParameters) {
            Type type = GetPOCOType(resultType);
            NewExpression newExpression = constructorExpression.Body as NewExpression;
            if(newExpression != null) {
                return GetNewExpression(type, newExpression);
            }
            MemberInitExpression memberInitExpression = constructorExpression.Body as MemberInitExpression;
            if(memberInitExpression != null) {
                return Expression.MemberInit(GetNewExpression(type, memberInitExpression.NewExpression), memberInitExpression.Bindings);
            }
            throw new ArgumentException("constructorExpression");
        }

        static NewExpression GetNewExpression(Type type, NewExpression newExpression) {
            var actualCtor = GetConstructor(type, newExpression.Constructor.GetParameters().Select(x => x.ParameterType).ToArray());
            return Expression.New(actualCtor, newExpression.Arguments);
        }
        internal static object Create(Type type) {
            return Activator.CreateInstance(GetPOCOType(type));
        }
        public static Type GetPOCOType(Type type) {
            return types.GetOrAdd(type, () => CreateType(type));
        }
        internal static ConstructorInfo GetConstructor(Type proxyType, Type[] argsTypes) {
            var ctor = proxyType.GetConstructor(argsTypes ?? Type.EmptyTypes);
            if(ctor == null)
                throw new ViewModelSourceException(Error_ConstructorNotFound);
            return ctor;
        }

        static bool CanAccessFromDescendant(MethodBase method) {
            return method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;
        }

        internal static bool IsPOCOViewModelType(Type type) {
            try {
                if(!CheckType(type, false))
                    return false;
                if(type.GetCustomAttributes(typeof(POCOViewModelAttribute), true).Any())
                    return true;
                if(GetCommandMethods(type).Any() && !type.GetProperties().Where(x => typeof(ICommand).IsAssignableFrom(x.PropertyType)).Any())
                    return true;
                if(GetBindableProperties(type).Any() && !typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                    return true;
                return false;
            } catch {
                return false;
            }
        }
        #endregion
        static Type CreateType(Type type) {
            CheckType(type, true);
            ModuleBuilder moduleBuilder = GetModuleBuilder(type.Assembly);
            TypeBuilder typeBuilder = CreateTypeBuilder(moduleBuilder, type);

            BuildConstructors(type, typeBuilder);
            var raisePropertyChangedMethod = ImplementINotifyPropertyChanged(type, typeBuilder);
            ImplementIPOCOViewModel(typeBuilder, raisePropertyChangedMethod);
            BuildBindableProperties(type, typeBuilder, raisePropertyChangedMethod);
            BuildCommands(type, typeBuilder);
            ImplementISupportServices(type, typeBuilder);
            ImplementISupportParentViewModel(type, typeBuilder);
            BuildServiceProperties(type, typeBuilder);
            ImplementIDataErrorInfo(type, typeBuilder);
            return typeBuilder.CreateType();
        }

        static MethodBuilder BuildExplicitStringGetterOverride(TypeBuilder typeBuilder, string propertyName, MethodInfo methodToCall, Type interfaceType, Type argument = null) {
            MethodAttributes methodAttributes =
                  MethodAttributes.Private
                | MethodAttributes.Virtual
                | MethodAttributes.Final
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot
                | MethodAttributes.SpecialName;
            var method = typeBuilder.DefineMethod(interfaceType.FullName + ".get_" + propertyName,
                methodAttributes,
                typeof(string), argument != null ? new[] { argument } : Type.EmptyTypes);
            ILGenerator gen = method.GetILGenerator();
            if(methodToCall != null) {
                gen.Emit(OpCodes.Ldarg_0);
                if(argument != null)
                    gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Call, methodToCall);
            } else {
                gen.Emit(OpCodes.Ldsfld, typeof(string).GetField("Empty"));
            }
            gen.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(method, interfaceType.GetProperty(propertyName).GetGetMethod());
            return method;
        }

        static bool ShouldImplementIDataErrorInfo(Type type) {
            var pocoViewModelAttr = (POCOViewModelAttribute)type.GetCustomAttributes(typeof(POCOViewModelAttribute), false).FirstOrDefault();
            bool implement = pocoViewModelAttr == null ? false : pocoViewModelAttr.ImplementIDataErrorInfo;
            if(type.GetInterfaces().Contains(typeof(IDataErrorInfo)) && implement)
                throw new ViewModelSourceException(Error_IDataErrorInfoAlreadyImplemented);
            return implement;
        }

        static void ImplementIDataErrorInfo(Type type, TypeBuilder typeBuilder) {
            if(!ShouldImplementIDataErrorInfo(type))
                return;
            var errorGetter = BuildExplicitStringGetterOverride(typeBuilder, "Error", null, typeof(IDataErrorInfo));
            var errorProperty = typeBuilder.DefineProperty("Error", PropertyAttributes.None, typeof(string), new Type[0]);
            errorProperty.SetGetMethod(errorGetter);

            var indexerGetterImpl = typeof(IDataErrorInfoHelper).GetMethod("GetErrorText", BindingFlags.Public | BindingFlags.Static);
            var indexerGetter = BuildExplicitStringGetterOverride(typeBuilder, "Item", indexerGetterImpl, typeof(IDataErrorInfo), typeof(string));
            var indexer = typeBuilder.DefineProperty("Item", PropertyAttributes.None, typeof(string), new[] { typeof(string) });
            indexer.SetGetMethod(indexerGetter);

            ConstructorInfo ciDefaultMemberAttribute = typeof(DefaultMemberAttribute).GetConstructor(new Type[] { typeof(string) });
            CustomAttributeBuilder abDefaultMemberAttribute = new CustomAttributeBuilder(ciDefaultMemberAttribute, new object[] { "Item" });
            typeBuilder.SetCustomAttribute(abDefaultMemberAttribute);
        }

        private static bool CheckType(Type type, bool @throw) {
            if(!type.IsPublic && !type.IsNestedPublic)
                return ReturnFalseOrThrow(@throw, Error_PrivateClass, type);
            if(type.IsSealed)
                return ReturnFalseOrThrow(@throw, Error_SealedClass, type);
            if(typeof(IPOCOViewModel).IsAssignableFrom(type))
                return ReturnFalseOrThrow(@throw, Error_TypeImplementsIPOCOViewModel, type);
            return true;
        }
        static ModuleBuilder GetModuleBuilder(Assembly assembly) {
            return builders.GetOrAdd(assembly, () => CreateBuilder());
        }
        static ModuleBuilder CreateBuilder() {
            var assemblyName = new AssemblyName();
            assemblyName.Name = AssemblyInfo.SRAssemblyXpfMvvm + ".DynamicTypes." + Guid.NewGuid().ToString();
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule(assemblyName.Name, false);

        }

        #region
        static void BuildConstructors(Type type, TypeBuilder typeBuilder) {
            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => CanAccessFromDescendant(x)).ToArray();
            if(!ctors.Any()) {
                throw new ViewModelSourceException(string.Format(Error_TypeHasNoCtors, type.Name));
            }
            foreach(ConstructorInfo constructor in ctors) {
                BuildConstructor(typeBuilder, constructor);
            }
        }
        static ConstructorBuilder BuildConstructor(TypeBuilder type, ConstructorInfo baseConstructor) {
            MethodAttributes methodAttributes =
                  MethodAttributes.Public;
            var parameters = baseConstructor.GetParameters();
            ConstructorBuilder method = type.DefineConstructor(methodAttributes, CallingConventions.Standard, parameters.Select(x => x.ParameterType).ToArray());

            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            for(int i = 0; i < parameters.Length; i++) {
                gen.Emit(OpCodes.Ldarg_S, i + 1);
            }
            gen.Emit(OpCodes.Call, baseConstructor);
            gen.Emit(OpCodes.Ret);

            return method;
        }
        #endregion

        #region
        static void BuildBindableProperties(Type type, TypeBuilder typeBuilder, MethodInfo raisePropertyChangedMethod) {
            foreach(var propertyInfo in GetBindableProperties(type)) {
                var getter = BuildBindablePropertyGetter(typeBuilder, propertyInfo.GetGetMethod());
                typeBuilder.DefineMethodOverride(getter, propertyInfo.GetGetMethod());
                MethodInfo propertyChangedMethod = GetPropertyChangedMethod(type, propertyInfo, "Changed", x => x.OnPropertyChangedMethodName, x => x.OnPropertyChangedMethod);
                MethodInfo propertyChangingMethod = GetPropertyChangedMethod(type, propertyInfo, "Changing", x => x.OnPropertyChangingMethodName, x => x.OnPropertyChangingMethod);

                var setter = BuildBindablePropertySetter(typeBuilder, raisePropertyChangedMethod, propertyInfo, propertyChangedMethod, propertyChangingMethod);
                typeBuilder.DefineMethodOverride(setter, propertyInfo.GetSetMethod(true));

                var newProperty = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, new Type[0]);
                newProperty.SetGetMethod(getter);
                newProperty.SetSetMethod(setter);
            }
        }
        static IEnumerable<PropertyInfo> GetBindableProperties(Type type) {
            return type.GetProperties().Where(x => IsBindableProperty(x));
        }
        static bool IsBindableProperty(PropertyInfo propertyInfo) {
            var bindable = GetBindablePropertyAttribute(propertyInfo);
            if(bindable != null && !bindable.IsBindable)
                return false;

            var getMethod = propertyInfo.GetGetMethod();
            var setMethod = propertyInfo.GetSetMethod(true);
            if(getMethod == null)
                return ReturnFalseOrThrow(bindable, Error_PropertyHasNoGetter, propertyInfo);
            if(!getMethod.IsVirtual)
                return ReturnFalseOrThrow(bindable, Error_PropertyIsNotVirual, propertyInfo);
            if(getMethod.IsFinal)
                return ReturnFalseOrThrow(bindable, Error_PropertyIsFinal, propertyInfo);
            if(setMethod == null)
                return ReturnFalseOrThrow(bindable, Error_PropertyHasNoSetter, propertyInfo);
            if(setMethod.IsAssembly)
                return ReturnFalseOrThrow(bindable, Error_PropertyHasInternalSetter, propertyInfo);
            if(!(IsAutoImplemented(propertyInfo)))
                return bindable != null && bindable.IsBindable;
            return true;
        }
        static BindablePropertyAttribute GetBindablePropertyAttribute(PropertyInfo propertyInfo) {
            return GetAttribute<BindablePropertyAttribute>(propertyInfo);
        }
        static MethodInfo GetPropertyChangedMethod(Type type, PropertyInfo propertyInfo, string methodNameSuffix, Func<BindablePropertyAttribute, string> getMethodName, Func<BindablePropertyAttribute, MethodInfo> getMethod) {
            var bindable = GetBindablePropertyAttribute(propertyInfo);
            if(bindable != null && getMethod(bindable) != null) {
                CheckOnChangedMethod(getMethod(bindable), propertyInfo.PropertyType);
                return getMethod(bindable);
            }
            bool hasCustomPropertyChangedMethodName = bindable != null && !string.IsNullOrEmpty(getMethodName(bindable));
            if(!hasCustomPropertyChangedMethodName && !(IsAutoImplemented(propertyInfo)))
                return null;
            string onChangedMethodName = hasCustomPropertyChangedMethodName ? getMethodName(bindable) : "On" + propertyInfo.Name + methodNameSuffix;
            MethodInfo[] changedMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name == onChangedMethodName).ToArray();
            if(changedMethods.Length > 1)
                throw new ViewModelSourceException(string.Format(Error_MoreThanOnePropertyChangedMethod, propertyInfo.Name));
            if(hasCustomPropertyChangedMethodName && !changedMethods.Any())
                throw new ViewModelSourceException(string.Format(Error_PropertyChangedMethodNotFound, onChangedMethodName));
            changedMethods.FirstOrDefault().Do(x => CheckOnChangedMethod(x, propertyInfo.PropertyType));
            return changedMethods.FirstOrDefault();
        }
        static void CheckOnChangedMethod(MethodInfo method, Type propertyType) {
            if(!CanAccessFromDescendant(method)) {
                throw new ViewModelSourceException(string.Format(Error_PropertyChangedMethodShouldBePublicOrProtected, method.Name));
            }
            if(method.GetParameters().Length >= 2) {
                throw new ViewModelSourceException(string.Format(Error_PropertyChangedCantHaveMoreThanOneParameter, method.Name));
            }
            if(method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType != propertyType) {
                throw new ViewModelSourceException(string.Format(Error_PropertyChangedMethodArgumentTypeShouldMatchPropertyType, method.Name));
            }
            if(method.ReturnType != typeof(void)) {
                throw new ViewModelSourceException(string.Format(Error_PropertyChangedCantHaveReturnType, method.Name));
            }
        }
        static bool IsAutoImplemented(PropertyInfo property) {
            if(property.GetGetMethod().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any())
                return true;
            if(property.GetSetMethod(true).GetParameters().Single().Name != "AutoPropertyValue")
                return false;
            FieldInfo field = property.DeclaringType.GetField("_" + property.Name, BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null && field.FieldType == property.PropertyType && field.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
        }
        static MethodBuilder BuildBindablePropertyGetter(TypeBuilder type, MethodInfo originalGetter) {
            MethodAttributes methodAttributes =
                  MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.HideBySig;
            MethodBuilder method = type.DefineMethod(originalGetter.Name, methodAttributes);
            method.SetReturnType(originalGetter.ReturnType);

            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, originalGetter);
            gen.Emit(OpCodes.Ret);

            return method;
        }
        static MethodBuilder BuildBindablePropertySetter(TypeBuilder type, MethodInfo raisePropertyChangedMethod, PropertyInfo property, MethodInfo propertyChangedMethod, MethodInfo propertyChangingMethod) {
            var setMethod = property.GetSetMethod(true);
            MethodAttributes methodAttributes =
                (setMethod.IsPublic ? MethodAttributes.Public : MethodAttributes.Family)
                | MethodAttributes.Virtual
                | MethodAttributes.HideBySig;
            MethodBuilder method = type.DefineMethod(setMethod.Name, methodAttributes);

            Expression<Action> equalsExpression = () => object.Equals(null, null);
            method.SetReturnType(typeof(void));
            method.SetParameters(property.PropertyType);
            bool shouldBoxValues = property.PropertyType.IsValueType;

            ParameterBuilder value = method.DefineParameter(1, ParameterAttributes.None, "value");
            ILGenerator gen = method.GetILGenerator();

            gen.DeclareLocal(property.PropertyType);
            Label returnLabel = gen.DefineLabel();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, property.GetGetMethod());

            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);

            if(shouldBoxValues)
                gen.Emit(OpCodes.Box, property.PropertyType);
            gen.Emit(OpCodes.Ldarg_1);
            if(shouldBoxValues)
                gen.Emit(OpCodes.Box, property.PropertyType);
            gen.Emit(OpCodes.Call, ExpressionHelper.GetMethod(equalsExpression));
            gen.Emit(OpCodes.Brtrue_S, returnLabel);

            if(propertyChangingMethod != null) {
                gen.Emit(OpCodes.Ldarg_0);
                if(propertyChangingMethod.GetParameters().Length == 1)
                    gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Call, propertyChangingMethod);
            }

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, setMethod);

            if(propertyChangedMethod != null) {
                gen.Emit(OpCodes.Ldarg_0);
                if(propertyChangedMethod.GetParameters().Length == 1)
                    gen.Emit(OpCodes.Ldloc_0);
                gen.Emit(OpCodes.Call, propertyChangedMethod);
            }

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, property.Name);
            gen.Emit(OpCodes.Call, raisePropertyChangedMethod);

            gen.MarkLabel(returnLabel);
            gen.Emit(OpCodes.Ret);

            return method;
        }
        #endregion

        #region
        static MethodBuilder BuildGetPropertyChangedHelperMethod(TypeBuilder type) {
            FieldBuilder propertyChangedHelperField = type.DefineField("propertyChangedHelper", typeof(PropertyChangedHelper), FieldAttributes.Private);

            MethodAttributes methodAttributes =
                  MethodAttributes.Private
                | MethodAttributes.HideBySig;
            MethodBuilder method = type.DefineMethod("GetHelper", methodAttributes);

            Expression<Func<PropertyChangedHelper>> createHelperExpression = () => new PropertyChangedHelper();
            ConstructorInfo helperCtor = ExpressionHelper.GetConstructor(createHelperExpression);

            method.SetReturnType(typeof(PropertyChangedHelper));

            ILGenerator gen = method.GetILGenerator();

            gen.DeclareLocal(typeof(PropertyChangedHelper));

            Label returnLabel = gen.DefineLabel();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, propertyChangedHelperField);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Brtrue_S, returnLabel);
            gen.Emit(OpCodes.Pop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Newobj, helperCtor);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Stfld, propertyChangedHelperField);
            gen.Emit(OpCodes.Ldloc_0);
            gen.MarkLabel(returnLabel);
            gen.Emit(OpCodes.Ret);

            return method;
        }
        static MethodBuilder BuildRaisePropertyChangedMethod(TypeBuilder type, MethodInfo getHelperMethod) {
            MethodAttributes methodAttributes =
                  MethodAttributes.Family
                | MethodAttributes.HideBySig;
            MethodBuilder method = type.DefineMethod("RaisePropertyChanged", methodAttributes);

            Expression<Action> onPropertyChangedExpression = () => new PropertyChangedHelper().OnPropertyChanged(null, null);

            method.SetReturnType(typeof(void));
            method.SetParameters(typeof(String));

            ParameterBuilder propertyName = method.DefineParameter(1, ParameterAttributes.None, "propertyName");
            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, getHelperMethod);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, ExpressionHelper.GetMethod(onPropertyChangedExpression));
            gen.Emit(OpCodes.Ret);

            return method;
        }

        static MethodInfo ImplementINotifyPropertyChanged(Type baseType, TypeBuilder typeBuilder) {
            MethodInfo raisePropertyChangedMethod;
            if(typeof(INotifyPropertyChanged).IsAssignableFrom(baseType)) {
                raisePropertyChangedMethod = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => {
                        var parameters = x.GetParameters();
                        return x.Name == "RaisePropertyChanged" && CanAccessFromDescendant(x) && parameters.Length == 1 && parameters[0].ParameterType == typeof(string) && !parameters[0].IsOut && !parameters[0].ParameterType.IsByRef;
                    });
                if(raisePropertyChangedMethod == null)
                    throw new ViewModelSourceException(string.Format(Error_RaisePropertyChangedMethodNotFound, baseType.Name));
            } else {
                var getHelperMethod = BuildGetPropertyChangedHelperMethod(typeBuilder);
                raisePropertyChangedMethod = BuildRaisePropertyChangedMethod(typeBuilder, getHelperMethod);

                var addHandler = BuildAddRemovePropertyChangedHandler(typeBuilder, getHelperMethod, "add", () => new PropertyChangedHelper().AddHandler(null));
                typeBuilder.DefineMethodOverride(addHandler, typeof(INotifyPropertyChanged).GetMethod("add_PropertyChanged"));
                var removeHandler = BuildAddRemovePropertyChangedHandler(typeBuilder, getHelperMethod, "remove", () => new PropertyChangedHelper().RemoveHandler(null));
                typeBuilder.DefineMethodOverride(removeHandler, typeof(INotifyPropertyChanged).GetMethod("remove_PropertyChanged"));
            }
            return raisePropertyChangedMethod;
        }
        static MethodBuilder BuildAddRemovePropertyChangedHandler(TypeBuilder type, MethodInfo getHelperMethod, string methodName, Expression<Action> addRemoveHandlerExpression) {
            MethodAttributes methodAttributes =
                  MethodAttributes.Private
                | MethodAttributes.Virtual
                | MethodAttributes.Final
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot;
            MethodBuilder method = type.DefineMethod(typeof(INotifyPropertyChanged).FullName + "." + methodName + "_PropertyChanged", methodAttributes);
            method.SetReturnType(typeof(void));
            method.SetParameters(typeof(PropertyChangedEventHandler));

            ParameterBuilder value = method.DefineParameter(1, ParameterAttributes.None, "value");
            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, getHelperMethod);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, ExpressionHelper.GetMethod(addRemoveHandlerExpression));
            gen.Emit(OpCodes.Ret);

            return method;
        }
        #endregion

        #region
        static void ImplementIPOCOViewModel(TypeBuilder typeBuilder, MethodInfo raisePropertyChangedMethod) {
            var raisePropertyChangedMethodImplementation = BuildIPOCOViewModel_RaisePropertyChangedMethod(typeBuilder, raisePropertyChangedMethod);
            IPOCOViewModel pocoViewModel = null;
            Expression<Action> raisePropertyChangedExpression = () => pocoViewModel.RaisePropertyChanged(null);
            typeBuilder.DefineMethodOverride(raisePropertyChangedMethodImplementation, ExpressionHelper.GetMethod(raisePropertyChangedExpression));
        }
        static MethodBuilder BuildIPOCOViewModel_RaisePropertyChangedMethod(TypeBuilder type, MethodInfo raisePropertyChangedMethod) {
            MethodAttributes methodAttributes =
                  MethodAttributes.Private
                | MethodAttributes.Virtual
                | MethodAttributes.Final
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot;
            MethodBuilder method = type.DefineMethod("DevExpress.Mvvm.Native.IPOCOViewModel.RaisePropertyChanged", methodAttributes);

            method.SetReturnType(typeof(void));
            method.SetParameters(typeof(String));

            ParameterBuilder propertyName = method.DefineParameter(1, ParameterAttributes.None, "propertyName");
            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, raisePropertyChangedMethod);
            gen.Emit(OpCodes.Ret);

            return method;
        }

        #endregion
        static TypeBuilder CreateTypeBuilder(ModuleBuilder module, Type baseType) {
            List<Type> interfaces = new List<Type>(new[] {
                typeof(INotifyPropertyChanged),
                typeof(IPOCOViewModel),
                typeof(ISupportServices),
                typeof(ISupportParentViewModel) });
            if(ShouldImplementIDataErrorInfo(baseType)) {
                interfaces.Add(typeof(IDataErrorInfo));
            }
            string typeName = baseType.Name + "_" + Guid.NewGuid().ToString().Replace('-', '_');
            return module.DefineType(typeName, TypeAttributes.Public, baseType, interfaces.ToArray());
        }

        static T GetAttribute<T>(MemberInfo member) where T : Attribute {
            return MetadataHelper.GetAttribute<T>(member);
        }

        static bool ReturnFalseOrThrow(Attribute attribute, string text, PropertyInfo property) {
            if(attribute != null)
                throw new ViewModelSourceException(string.Format(text, property.Name));
            return false;
        }
        static bool ReturnFalseOrThrow(bool @throw, string text, Type type) {
            if(@throw)
                throw new ViewModelSourceException(string.Format(text, type.Name));
            return false;
        }

        #region
        static void BuildCommands(Type type, TypeBuilder typeBuilder) {
            MethodInfo[] methods = GetCommandMethods(type).ToArray();
            foreach(var commandMethod in methods) {
                CommandAttribute attribute = ViewModelBase.GetAttribute<CommandAttribute>(commandMethod);
                bool isAsyncCommand = commandMethod.ReturnType == typeof(Task);
                string commandName = GetCommandName(commandMethod);
                if(type.GetMember(commandName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Any() || methods.Any(x => GetCommandName(x) == commandName && x != commandMethod))
                    throw new ViewModelSourceException(string.Format(Error_MemberWithSameCommandNameAlreadyExists, commandName));

                MethodInfo canExecuteMethod = GetCanExecuteMethod(type, commandMethod);

                bool useCommandManager = attribute != null ? attribute.UseCommandManager : true;
                var getMethod = BuildGetCommandMethod(typeBuilder, commandMethod, canExecuteMethod, commandName, useCommandManager, isAsyncCommand);
                PropertyBuilder commandProperty = typeBuilder.DefineProperty(commandName,
                                                     PropertyAttributes.None,
                                                     getMethod.ReturnType,
                                                     null);
                commandProperty.SetGetMethod(getMethod);
                BuildCommandPropertyAttributes(commandProperty, commandMethod);
            }
        }

        static IEnumerable<MethodInfo> GetCommandMethods(Type type) {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => IsCommandMethod(type, x));
        }

        static bool IsCommandMethod(Type type, MethodInfo method) {
            ParameterInfo[] parameters = method.GetParameters();

            CommandAttribute attribute = ViewModelBase.GetAttribute<CommandAttribute>(method);
            if(attribute != null && !attribute.IsCommand)
                return false;
            if(attribute == null) {
                if(!method.IsPublic)
                    return false;
            } else {
                if(!CanAccessFromDescendant(method))
                    throw new ViewModelSourceException(string.Format(ViewModelBase.Error_MethodShouldBePublic, method.Name));
            }
            string commandName = GetCommandName(method);
            if(method.IsSpecialName || method.DeclaringType == typeof(object) || type.GetProperty(commandName) != null)
                return false;
            if((method.ReturnType != typeof(void) && method.ReturnType != typeof(Task)) && attribute == null)
                return false;
            if(!ViewModelBase.ValidateCommandMethodParameters(method, x => new ViewModelSourceException(x)))
                return false;
            return true;
        }

        internal static string GetCommandName(MethodInfo method) {
            CommandAttribute commandAttribute = ViewModelBase.GetAttribute<CommandAttribute>(method);
            return (commandAttribute != null && !string.IsNullOrEmpty(commandAttribute.Name)) ? commandAttribute.Name : ViewModelBase.GetCommandName(method);
        }
        static MethodInfo GetCanExecuteMethod(Type type, MethodInfo method) {
            return ViewModelBase.GetCanExecuteMethod(type, method, ViewModelBase.GetAttribute<CommandAttribute>(method), x => new ViewModelSourceException(x));
        }
        static MethodBuilder BuildGetCommandMethod(TypeBuilder type, MethodInfo commandMethod, MethodInfo canExecuteMethod, string commandName, bool useCommandManager, bool isAsyncCommand) {
            bool hasParameter = commandMethod.GetParameters().Length == 1;
            bool isCommandMethodReturnTypeVoid = commandMethod.ReturnType == typeof(void);
            Type commandMethodReturnType = commandMethod.ReturnType;
            Type parameterType = hasParameter ? commandMethod.GetParameters()[0].ParameterType : null;
            Type commandPropertyType = isAsyncCommand ?
                hasParameter ? typeof(AsyncCommand<>).MakeGenericType(parameterType) : typeof(AsyncCommand)
                : hasParameter ? typeof(DelegateCommand<>).MakeGenericType(parameterType) : typeof(ICommand);

            var commandField = type.DefineField("_" + commandName, commandPropertyType, FieldAttributes.Private);

            MethodAttributes methodAttributes =
                  MethodAttributes.Public | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig;
            MethodBuilder method = type.DefineMethod("get_" + commandName, methodAttributes);

            ConstructorInfo actionConstructor = (hasParameter ?
                    (isCommandMethodReturnTypeVoid ? typeof(Action<>).MakeGenericType(parameterType) : typeof(Func<,>).MakeGenericType(parameterType, commandMethodReturnType)) :
                    (isCommandMethodReturnTypeVoid ? typeof(Action) : typeof(Func<>).MakeGenericType(commandMethodReturnType)))
                    .GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Object), typeof(IntPtr) }, null);
            ConstructorInfo funcConstructor = (hasParameter ? typeof(Func<,>).MakeGenericType(parameterType, typeof(bool)) : typeof(Func<bool>)).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Object), typeof(IntPtr) }, null);

            MethodInfo createCommandMethod;
            if(isAsyncCommand)
                createCommandMethod = hasParameter ?
                    AsyncCommandFactory.GetGenericMethodWithResult(parameterType, commandMethodReturnType)
                    : AsyncCommandFactory.GetSimpleMethodWithResult(commandMethodReturnType);
            else
                createCommandMethod = hasParameter ?
                (isCommandMethodReturnTypeVoid ?
                    DelegateCommandFactory.GetGenericMethodWithoutResult(parameterType)
                    : DelegateCommandFactory.GetGenericMethodWithResult(parameterType, commandMethodReturnType))
                : (isCommandMethodReturnTypeVoid ?
                    DelegateCommandFactory.GetSimpleMethodWithoutResult()
                    : DelegateCommandFactory.GetSimpleMethodWithResult(commandMethodReturnType));
            method.SetReturnType(commandPropertyType);

            ILGenerator gen = method.GetILGenerator();
            gen.DeclareLocal(commandPropertyType);
            Label returnLabel = gen.DefineLabel();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, commandField);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Brtrue_S, returnLabel);
            gen.Emit(OpCodes.Pop);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldftn, commandMethod);
            gen.Emit(OpCodes.Newobj, actionConstructor);
            if(canExecuteMethod != null) {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldftn, canExecuteMethod);
                gen.Emit(OpCodes.Newobj, funcConstructor);
            } else {
                gen.Emit(OpCodes.Ldnull);
            }
            if(useCommandManager)
                gen.Emit(OpCodes.Ldc_I4_1);
            else
                gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Call, createCommandMethod);

            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Stfld, commandField);
            gen.Emit(OpCodes.Ldloc_0);
            gen.MarkLabel(returnLabel);
            gen.Emit(OpCodes.Ret);

            return method;
        }

        static void BuildCommandPropertyAttributes(PropertyBuilder commandProperty, MethodInfo commandMethod) {
            foreach(Attribute attribute in MetadataHelper.GetAllAttributes(commandMethod)) {
                ICustomAttributeBuilderProvider provider;
                if(attributeBuilderProviders.TryGetValue(attribute.GetType(), out provider))
                    commandProperty.SetCustomAttribute(provider.CreateAttributeBuilder(attribute));
            }
        }
        #endregion

        #region
        static void ImplementISupportServices(Type type, TypeBuilder typeBuilder) {
            if(typeof(ISupportServices).IsAssignableFrom(type))
                return;
            Expression<Func<ISupportServices, IServiceContainer>> getServiceContainerExpression = x => x.ServiceContainer;
            var getter = ExpressionHelper.GetArgumentPropertyStrict(getServiceContainerExpression).GetGetMethod();
            FieldBuilder serviceContainerField = typeBuilder.DefineField("serviceContainer", typeof(IServiceContainer), FieldAttributes.Private);
            var getServiceContainerMethod = BuildGetServiceContainerMethod(typeBuilder, serviceContainerField, getter.Name);
            typeBuilder.DefineMethodOverride(getServiceContainerMethod, getter);
        }
        static MethodBuilder BuildGetServiceContainerMethod(TypeBuilder type, FieldInfo serviceContainerField, string getServiceContainerMethodName) {
            MethodAttributes methodAttributes =
                  MethodAttributes.Private
                | MethodAttributes.Virtual
                | MethodAttributes.Final
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot;
            MethodBuilder method = type.DefineMethod(typeof(ISupportServices).FullName + "." + getServiceContainerMethodName, methodAttributes);

            Expression<Func<ServiceContainer>> serviceContainerCtorExpression = () => new ServiceContainer(null);

            method.SetReturnType(typeof(IServiceContainer));

            ILGenerator gen = method.GetILGenerator();

            gen.DeclareLocal(typeof(IServiceContainer));
            Label returnLabel = gen.DefineLabel();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, serviceContainerField);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Brtrue_S, returnLabel);
            gen.Emit(OpCodes.Pop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Newobj, ExpressionHelper.GetConstructor(serviceContainerCtorExpression));
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Stfld, serviceContainerField);
            gen.Emit(OpCodes.Ldloc_0);
            gen.MarkLabel(returnLabel);
            gen.Emit(OpCodes.Ret);

            return method;
        }


        static void BuildServiceProperties(Type type, TypeBuilder typeBuilder) {
            var serviceProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => IsServiceProperty(x)).ToArray();
            foreach(var propertyInfo in serviceProperties) {
                ServicePropertyAttribute attribute = GetAttribute<ServicePropertyAttribute>(propertyInfo);
                bool required = DataAnnotationsAttributeHelper.HasRequiredAttribute(propertyInfo);
                var getter = BuildGetServicePropertyMethod(typeBuilder, propertyInfo, attribute.With(x => x.Key), attribute.Return(x => x.SearchMode, () => default(ServiceSearchMode)), required);
                typeBuilder.DefineMethodOverride(getter, propertyInfo.GetGetMethod(true));
                var newProperty = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);
                newProperty.SetGetMethod(getter);
            }
        }
        static bool IsServiceProperty(PropertyInfo property) {
            ServicePropertyAttribute attribute = GetAttribute<ServicePropertyAttribute>(property);
            if(attribute != null && !attribute.IsServiceProperty)
                return false;

            if(!property.PropertyType.Name.EndsWith("Service") && attribute == null)
                return false;

            if(!property.PropertyType.IsInterface)
                return ReturnFalseOrThrow(attribute, Error_PropertyTypeShouldBeServiceType, property);
            var getter = property.GetGetMethod(true);
            if(!CanAccessFromDescendant(getter))
                return ReturnFalseOrThrow(attribute, Error_CantAccessProperty, property);
            if(!getter.IsVirtual)
                return ReturnFalseOrThrow(attribute, Error_PropertyIsNotVirtual, property);
            if(getter.IsFinal)
                return ReturnFalseOrThrow(attribute, Error_PropertyIsFinal, property);
            if(property.GetSetMethod(true) != null)
                return ReturnFalseOrThrow(attribute, Error_PropertyHasSetter, property);
            return true;
        }
        static MethodBuilder BuildGetServicePropertyMethod(TypeBuilder type, PropertyInfo property, string serviceName, ServiceSearchMode searchMode, bool required) {
            var getMethod = property.GetGetMethod(true);
            MethodAttributes methodAttributes =
                (getMethod.IsPublic ? MethodAttributes.Public : MethodAttributes.Family)
                | MethodAttributes.Virtual
                | MethodAttributes.HideBySig;
            MethodBuilder method = type.DefineMethod(getMethod.Name, methodAttributes);
            method.SetReturnType(property.PropertyType);
            ILGenerator gen = method.GetILGenerator();
            if(required)
                EmitGetRequiredServicePropertyMethod(property.PropertyType, serviceName, searchMode, gen);
            else
                EmitGetOptionalServicePropertyMethod(property.PropertyType, serviceName, searchMode, gen);
            return method;
        }
        static void EmitGetOptionalServicePropertyMethod(Type serviceType, string serviceName, ServiceSearchMode searchMode, ILGenerator gen) {
            Expression<Func<ISupportServices, IServiceContainer>> serviceContainerPropertyExpression = x => x.ServiceContainer;
            Type[] getServiceMethodParams = string.IsNullOrEmpty(serviceName) ? new Type[] { typeof(ServiceSearchMode) } : new Type[] { typeof(string), typeof(ServiceSearchMode) };
            MethodInfo getServiceMethod = typeof(IServiceContainer).GetMethod("GetService", BindingFlags.Instance | BindingFlags.Public, null, getServiceMethodParams, null).MakeGenericMethod(serviceType);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, ExpressionHelper.GetArgumentPropertyStrict(serviceContainerPropertyExpression).GetGetMethod());
            if(!string.IsNullOrEmpty(serviceName))
                gen.Emit(OpCodes.Ldstr, serviceName);
            gen.Emit(OpCodes.Ldc_I4_S, (int)searchMode);
            gen.Emit(OpCodes.Callvirt, getServiceMethod);
            gen.Emit(OpCodes.Ret);
        }
        static void EmitGetRequiredServicePropertyMethod(Type serviceType, string serviceName, ServiceSearchMode searchMode, ILGenerator gen) {
            Expression<Func<ISupportServices, IServiceContainer>> serviceContainerPropertyExpression = x => x.ServiceContainer;
            Type[] getServiceMethodParams = string.IsNullOrEmpty(serviceName) ? new Type[] { typeof(IServiceContainer), typeof(ServiceSearchMode) } : new Type[] { typeof(IServiceContainer), typeof(string), typeof(ServiceSearchMode) };
            MethodInfo getServiceMethod = typeof(ServiceContainerExtensions).GetMethod("GetRequiredService", BindingFlags.Static | BindingFlags.Public, null, getServiceMethodParams, null).MakeGenericMethod(serviceType);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, ExpressionHelper.GetArgumentPropertyStrict(serviceContainerPropertyExpression).GetGetMethod());
            if(!string.IsNullOrEmpty(serviceName))
                gen.Emit(OpCodes.Ldstr, serviceName);
            gen.Emit(OpCodes.Ldc_I4_S, (int)searchMode);
            gen.Emit(OpCodes.Call, getServiceMethod);
            gen.Emit(OpCodes.Ret);
        }
        #endregion

        #region
        static void ImplementISupportParentViewModel(Type type, TypeBuilder typeBuilder) {
            if(typeof(ISupportParentViewModel).IsAssignableFrom(type))
                return;

            Expression<Func<ISupportParentViewModel, object>> parentViewModelPropertyExpression = x => x.ParentViewModel;
            var getter = ExpressionHelper.GetArgumentPropertyStrict(parentViewModelPropertyExpression).GetGetMethod();
            var setter = ExpressionHelper.GetArgumentPropertyStrict(parentViewModelPropertyExpression).GetSetMethod();
            FieldBuilder parentViewModelField = typeBuilder.DefineField("parentViewModel", typeof(object), FieldAttributes.Private);

            var getMethod = BuildGetParentViewModelMethod(typeBuilder, parentViewModelField, getter.Name);
            var setMethod = BuildSetParentViewModelMethod(typeBuilder, parentViewModelField, setter.Name);
            typeBuilder.DefineMethodOverride(getMethod, getter);
            typeBuilder.DefineMethodOverride(setMethod, setter);
        }
        static MethodBuilder BuildSetParentViewModelMethod(TypeBuilder type, FieldBuilder parentViewModelField, string setterName) {
            System.Reflection.MethodAttributes methodAttributes =
                  MethodAttributes.Private
                | MethodAttributes.Virtual
                | MethodAttributes.Final
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot;
            MethodBuilder method = type.DefineMethod(typeof(ISupportParentViewModel).FullName + "." + setterName, methodAttributes);

            method.SetReturnType(typeof(void));
            method.SetParameters(typeof(object));
            ParameterBuilder value = method.DefineParameter(1, ParameterAttributes.None, "value");
            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, parentViewModelField);
            gen.Emit(OpCodes.Ret);

            return method;
        }
        static MethodBuilder BuildGetParentViewModelMethod(TypeBuilder type, FieldBuilder parentViewModelField, string getterName) {
            System.Reflection.MethodAttributes methodAttributes =
                  MethodAttributes.Private
                | MethodAttributes.Virtual
                | MethodAttributes.Final
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot;
            MethodBuilder method = type.DefineMethod(typeof(ISupportParentViewModel).FullName + "." + getterName, methodAttributes);

            method.SetReturnType(typeof(object));
            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, parentViewModelField);
            gen.Emit(OpCodes.Ret);

            return method;
        }


        #endregion
    }
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ViewModelSourceException : Exception {
        public ViewModelSourceException() { }
        public ViewModelSourceException(string message)
            : base(message) {

        }
    }
    public static class POCOViewModelExtensions {
        public static bool IsInDesignMode(this object viewModel) {
            return ViewModelBase.IsInDesignMode;
        }
        public static void RaisePropertyChanged<T, TProperty>(this T viewModel, Expression<Func<T, TProperty>> propertyExpression) {
            IPOCOViewModel pocoViewModel = GetPOCOViewModel(viewModel);
            pocoViewModel.RaisePropertyChanged(BindableBase.GetPropertyNameFast(propertyExpression));
        }
        public static ICommand GetCommand<T>(this T viewModel, Expression<Action<T>> methodExpression) {
            return GetCommandCore(viewModel, methodExpression);
        }
        public static T SetParentViewModel<T>(this T viewModel, object parentViewModel) {
            ((ISupportParentViewModel)viewModel).ParentViewModel = parentViewModel;
            return viewModel;
        }
        public static IAsyncCommand GetAsyncCommand<T>(this T viewModel, Expression<Func<T, Task>> methodExpression) {
            return GetAsyncCommandCore(viewModel, methodExpression);
        }
        public static void RaiseCanExecuteChanged<T>(this T viewModel, Expression<Action<T>> methodExpression) {
            RaiseCanExecuteChangedCore(viewModel, methodExpression);
        }
        public static bool GetShouldCancel<T>(this T viewModel, Expression<Func<T, Task>> methodExpression) {
            return GetAsyncCommand(viewModel, methodExpression).ShouldCancel;
        }
        public static bool GetIsExecuting<T>(this T viewModel, Expression<Func<T, Task>> methodExpression) {
            return GetAsyncCommand(viewModel, methodExpression).IsExecuting;
        }
        internal static void RaiseCanExecuteChangedCore(object viewModel, LambdaExpression methodExpression) {
            IDelegateCommand command = GetCommandCore(viewModel, methodExpression);
            command.RaiseCanExecuteChanged();
        }
        static IAsyncCommand GetAsyncCommandCore(object viewModel, LambdaExpression methodExpression) {
            GetPOCOViewModel(viewModel);
            ICommand command = GetCommandCore(viewModel, methodExpression);
            if(!(command is IAsyncCommand))
                throw new ViewModelSourceException(ViewModelSource.Error_CommandNotAsync);
            return (IAsyncCommand)command;
        }
        static IDelegateCommand GetCommandCore(object viewModel, LambdaExpression methodExpression) {
            GetPOCOViewModel(viewModel);
            MethodInfo method = ExpressionHelper.GetMethod(methodExpression);
            string commandName = ViewModelSource.GetCommandName(method);
            PropertyInfo property = viewModel.GetType().GetProperty(commandName);
            if(property == null)
                throw new ViewModelSourceException(string.Format(ViewModelSource.Error_CommandNotFound, commandName));
            return property.GetValue(viewModel, null) as IDelegateCommand;
        }
        static IPOCOViewModel GetPOCOViewModel<T>(T viewModel) {
            IPOCOViewModel pocoViewModel = viewModel as IPOCOViewModel;
            if(pocoViewModel == null)
                throw new ViewModelSourceException(ViewModelSource.Error_ObjectDoesntImplementIPOCOViewModel);
            return pocoViewModel;
        }
    }

    #region
    public static class ViewModelSource<T> {
        static TDelegate GetFactoryByTypes<TDelegate>(Func<Type[]> getTypesDelegate) {
            return ViewModelSource.GetFactoryCore<TDelegate>(() => {
                Type[] types = getTypesDelegate();
                var ctor = ViewModelSource.GetConstructor(ViewModelSource.GetPOCOType(typeof(T)), types);
                ParameterExpression[] parameters = types.Select(x => Expression.Parameter(x)).ToArray();
                return Expression.Lambda<TDelegate>(Expression.New(ctor, parameters), parameters).Compile();
            });
        }

        public static T Create() {
            return GetFactoryByTypes<Func<T>>(() => Type.EmptyTypes)();
        }
        public static T Create<T1>(T1 param1) {
            return GetFactoryByTypes<Func<T1, T>>(() => new[] { typeof(T1) })
                (param1);
        }
        public static T Create<T1, T2>(T1 param1, T2 param2) {
            return GetFactoryByTypes<Func<T1, T2, T>>(() => new[] { typeof(T1), typeof(T2) })
                (param1, param2);
        }
        public static T Create<T1, T2, T3>(T1 param1, T2 param2, T3 param3) {
            return GetFactoryByTypes<Func<T1, T2, T3, T>>(() => new[] { typeof(T1), typeof(T2), typeof(T3) })
                (param1, param2, param3);
        }
        public static T Create<T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4) {
            return GetFactoryByTypes<Func<T1, T2, T3, T4, T>>(() => new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) })
                (param1, param2, param3, param4);
        }
        public static T Create<T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) {
            return GetFactoryByTypes<Func<T1, T2, T3, T4, T5, T>>(() => new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) })
                (param1, param2, param3, param4, param5);
        }
        public static T Create<T1, T2, T3, T4, T5, T6>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) {
            return GetFactoryByTypes<Func<T1, T2, T3, T4, T5, T6, T>>(() => new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) })
                (param1, param2, param3, param4, param5, param6);
        }
        public static T Create<T1, T2, T3, T4, T5, T6, T7>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7) {
            return GetFactoryByTypes<Func<T1, T2, T3, T4, T5, T6, T7, T>>(() => new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) })
                (param1, param2, param3, param4, param5, param6, param7);
        }
        public static T Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8) {
            return GetFactoryByTypes<Func<T1, T2, T3, T4, T5, T6, T7, T8, T>>(() => new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) })
                (param1, param2, param3, param4, param5, param6, param7, param8);
        }
        public static T Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9) {
            return GetFactoryByTypes<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T>>(() => new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) })
                (param1, param2, param3, param4, param5, param6, param7, param8, param9);
        }
        public static T Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, T10 param10) {
            return GetFactoryByTypes<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T>>(() => new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10) })
                (param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
        }
    }

    #endregion
}