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
using System.Runtime.InteropServices;

namespace DevExpress.Mvvm.Native {
    public static class ViewModelSourceHelper {
        public static Type GetProxyType(Type type) {
            return DevExpress.Mvvm.POCO.ViewModelSource.GetPOCOType(type);
        }
        public static object Create(Type type, Action<object, object, object, string> customSetterAction = null) {
            return DevExpress.Mvvm.POCO.ViewModelSource.Create(type, customSetterAction);
        }
        public static bool IsPOCOViewModelType(Type type) {
            return DevExpress.Mvvm.POCO.ViewModelSource.IsPOCOViewModelType(type);
        }
        public static ConstructorInfo FindConstructorWithAllOptionalParameters(Type type) {
            return DevExpress.Mvvm.POCO.ViewModelSource.FindConstructorWithAllOptionalParameters(type);
        }

    }
}
namespace DevExpress.Mvvm.POCO {
    public interface IPOCOViewModel {
        void RaisePropertyChanged(string propertyName);
        void RaisePropertyChanging(string propertyName);
    }

    public class ViewModelSource {
#if DEBUG
        static Action<Type> checkTypeInTests;
        public static void SetCheckTypeInTestsDelegate(Action<Type> check) {
            if (checkTypeInTests != null)
                throw new InvalidOperationException();
            checkTypeInTests = check;
        }
#endif

        [ThreadStatic]
        static Dictionary<Type, ICustomAttributeBuilderProvider> attributeBuilderProviders;
        static Dictionary<Type, ICustomAttributeBuilderProvider> AttributeBuilderProviders {
            get { return attributeBuilderProviders ?? (attributeBuilderProviders = new Dictionary<Type, ICustomAttributeBuilderProvider>()); }
        }

        static ViewModelSource() {
            RegisterAttributeBuilderProvider(new DisplayAttributeBuilderProvider());
            RegisterAttributeBuilderProvider(new DisplayNameAttributeBuilderProvider());
            RegisterAttributeBuilderProvider(new ScaffoldColumnAttributeBuilderProvider());
            RegisterAttributeBuilderProvider(new BrowsableAttributeBuilderProvider());
        }
        static void RegisterAttributeBuilderProvider(ICustomAttributeBuilderProvider provider) {
            AttributeBuilderProviders[provider.AttributeType] = provider;
        }
        [ThreadStatic]
        static Dictionary<Type, Type> types;
        static Dictionary<Type, Type> Types {
            get { return types ?? (types = new Dictionary<Type, Type>()); }
        }
        [ThreadStatic]
        static Dictionary<Type, object> factories;
        static Dictionary<Type, object> Factories {
            get { return factories ?? (factories = new Dictionary<Type, object>()); }
        }

        public static T Create<T>(Action<object, object, object, string> customSetterAction = null) where T : class, new() {
            return Factory(() => new T(), customSetterAction)();
        }
        public static T Create<T>(Expression<Func<T>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where T : class {
            ValidateCtorExpression(constructorExpression, false);
            var actualAxpression = GetCtorExpression(constructorExpression, typeof(T), false, customSetterAction);
            return Expression.Lambda<Func<T>>(actualAxpression).Compile()();
        }
        #region GetFactory
        public static Func<TResult> Factory<TResult>(Expression<Func<TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, TResult> Factory<T1, TResult>(Expression<Func<T1, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, T2, TResult> Factory<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, T2, T3, TResult> Factory<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, T2, T3, T4, TResult> Factory<T1, T2, T3, T4, TResult>(Expression<Func<T1, T2, T3, T4, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, T2, T3, T4, T5, TResult> Factory<T1, T2, T3, T4, T5, TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, T2, T3, T4, T5, T6, TResult> Factory<T1, T2, T3, T4, T5, T6, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> Factory<T1, T2, T3, T4, T5, T6, T7, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Factory<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Factory<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Factory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> constructorExpression, Action<object, object, object, string> customSetterAction = null) where TResult : class {
            return GetFactoryCore(constructorExpression, typeof(TResult), customSetterAction);
        }
        #endregion

        #region helpers
        static TDelegate GetFactoryCore<TDelegate>(Expression<TDelegate> constructorExpression, Type resultType, Action<object, object, object, string> customSetterAction) {
            ValidateCtorExpression(constructorExpression, true);
            return GetFactoryCore<TDelegate>(() => CreateFactory(constructorExpression, resultType, customSetterAction));
        }
        internal static TDelegate GetFactoryCore<TDelegate>(Func<TDelegate> createFactoryDelegate) {
            return (TDelegate)Factories.GetOrAdd(typeof(TDelegate), () => createFactoryDelegate());
        }
        static TDelegate CreateFactory<TDelegate>(Expression<TDelegate> constructorExpression, Type resultType, Action<object, object, object, string> customSetterAction) {
            var actualAxpression = GetCtorExpression(constructorExpression, resultType, true, customSetterAction);
            return Expression.Lambda<TDelegate>(actualAxpression, constructorExpression.Parameters).Compile();
        }

        static void ValidateCtorExpression(LambdaExpression constructorExpression, bool useOnlyParameters) {
            NewExpression newExpression = constructorExpression.Body as NewExpression;
            if(newExpression != null) {
                if(useOnlyParameters) {
                    foreach(var item in newExpression.Arguments) {
                        if(!(item is ParameterExpression))
                            throw new ViewModelSourceException(ViewModelSourceException.Error_ConstructorExpressionCanReferOnlyToItsArguments);
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
            throw new ViewModelSourceException(ViewModelSourceException.Error_ConstructorExpressionCanOnlyBeOfNewExpressionType);
        }
        static Expression GetCtorExpression(LambdaExpression constructorExpression, Type resultType, bool useOnlyParameters, Action<object, object, object, string> customSetterAction) {
            Type type = GetPOCOType(resultType, customSetterAction);
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
        internal static object Create(Type type, Action<object, object, object, string> customSetterAction = null) {
            Type pocoType = GetPOCOType(type, customSetterAction);
            var defaultCtor = pocoType.GetConstructor(new Type[0]);
            if(defaultCtor != null)
                return defaultCtor.Invoke(null);
            defaultCtor = FindConstructorWithAllOptionalParameters(type);
            if(defaultCtor != null)
                return pocoType.GetConstructor(defaultCtor.GetParameters().Select(x => x.ParameterType).ToArray()).Invoke(defaultCtor.GetParameters().Select(x => x.DefaultValue).ToArray());
            return Activator.CreateInstance(GetPOCOType(type));
        }

        internal static ConstructorInfo FindConstructorWithAllOptionalParameters(Type type) {
            return type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .FirstOrDefault(x => (x.Attributes.HasFlag(MethodAttributes.Public) || x.Attributes.HasFlag(MethodAttributes.Family)) && x.GetParameters().All(y => y.IsOptional));
        }

        public static Type GetPOCOType(Type type, ViewModelSourceBuilderBase builder = null) {
            Func<Type> createType = () => CreateType(type, builder);
            if(builder == null) builder = ViewModelSourceBuilderBase.Default;
            return builder == ViewModelSourceBuilderBase.Default ? Types.GetOrAdd(type, createType) : createType();
        }

        private static Type GetPOCOType(Type type, Action<object, object, object, string> customSetterAction, ViewModelSourceBuilderBase builder = null)
        {
            Func<Type> createType = () => CreateType(type, builder, customSetterAction);
            if (builder == null) builder = ViewModelSourceBuilderBase.Default;
            return builder == ViewModelSourceBuilderBase.Default ? Types.GetOrAdd(type, createType) : createType();
        }
        internal static ConstructorInfo GetConstructor(Type proxyType, Type[] argsTypes) {
            var ctor = proxyType.GetConstructor(argsTypes ?? Type.EmptyTypes);
            if(ctor == null)
                throw new ViewModelSourceException(ViewModelSourceException.Error_ConstructorNotFound);
            return ctor;
        }

        internal static bool IsPOCOViewModelType(Type type) {
            try {
                if(!CheckType(type, false))
                    return false;
                if(type.GetCustomAttributes(typeof(POCOViewModelAttribute), true).Any())
                    return true;
                if(typeof(Component).IsAssignableFrom(type))
                    return false;
                if(GetCommandMethods(type).Any() && !type.GetProperties().Where(x => typeof(ICommand).IsAssignableFrom(x.PropertyType)).Any())
                    return true;
                if(ViewModelSourceBuilderBase.Default.GetBindableProperties(type).Any() && !typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                    return true;
                return false;
            } catch {
                return false;
            }
        }
        #endregion
        static Type CreateType(Type type, ViewModelSourceBuilderBase builder, Action<object, object, object, string> customSetterAction = null) {
            CheckType(type, true);
            TypeBuilder typeBuilder = BuilderType.CreateTypeBuilder(type);
            BuilderType.BuildConstructors(type, typeBuilder);
            MethodInfo raisePropertyChanged, raisePropertyChanging;
            BuilderINPC.ImplementINPC(type, typeBuilder, out raisePropertyChanged, out raisePropertyChanging);
            BuilderIPOCOViewModel.ImplementIPOCOViewModel(type, typeBuilder, raisePropertyChanged, raisePropertyChanging);
            builder.BuildBindableProperties(type, typeBuilder, raisePropertyChanged, raisePropertyChanging, customSetterAction);
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

        static void ImplementIDataErrorInfo(Type type, TypeBuilder typeBuilder) {
            if(!BuilderCommon.ShouldImplementIDataErrorInfo(type))
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
#if DEBUG
            if (checkTypeInTests != null)
                checkTypeInTests(type);
#endif
            if (!type.IsPublic && !type.IsNestedPublic)
                return ViewModelSourceException.ReturnFalseOrThrow(@throw, ViewModelSourceException.Error_InternalClass, type);
            if(type.IsSealed)
                return ViewModelSourceException.ReturnFalseOrThrow(@throw, ViewModelSourceException.Error_SealedClass, type);
            if(typeof(IPOCOViewModel).IsAssignableFrom(type))
                return ViewModelSourceException.ReturnFalseOrThrow(@throw, ViewModelSourceException.Error_TypeImplementsIPOCOViewModel, type);
            return true;
        }

        #region commands
        static void BuildCommands(Type type, TypeBuilder typeBuilder) {
            MethodInfo[] methods = GetCommandMethods(type).ToArray();
            foreach(var commandMethod in methods) {
                CommandAttribute attribute = ViewModelBase.GetAttribute<CommandAttribute>(commandMethod);
                bool isAsyncCommand = commandMethod.ReturnType == typeof(Task);
                string commandName = GetCommandName(commandMethod);
                if(type.GetMember(commandName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Any() || methods.Any(x => GetCommandName(x) == commandName && x != commandMethod))
                    throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_MemberWithSameCommandNameAlreadyExists, commandName));

                MethodInfo canExecuteMethod = GetCanExecuteMethod(type, commandMethod);

                bool? useCommandManager = attribute != null ? attribute.GetUseCommandManager() : null;
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
                if(!BuilderCommon.CanAccessFromDescendant(method))
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
            return ViewModelBase.GetCanExecuteMethod(type, method, ViewModelBase.GetAttribute<CommandAttribute>(method), x => new ViewModelSourceException(x), BuilderCommon.CanAccessFromDescendant);
        }
        static MethodBuilder BuildGetCommandMethod(TypeBuilder type, MethodInfo commandMethod, MethodInfo canExecuteMethod, string commandName, bool? useCommandManager, bool isAsyncCommand) {
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
                    AsyncCommandFactory.GetGenericMethodWithResult(parameterType, commandMethodReturnType, useCommandManager != null)
                    : AsyncCommandFactory.GetSimpleMethodWithResult(commandMethodReturnType, useCommandManager != null);
            else
                createCommandMethod = hasParameter ?
                (isCommandMethodReturnTypeVoid ?
                    DelegateCommandFactory.GetGenericMethodWithoutResult(parameterType, useCommandManager != null)
                    : DelegateCommandFactory.GetGenericMethodWithResult(parameterType, commandMethodReturnType, useCommandManager != null))
                : (isCommandMethodReturnTypeVoid ?
                    DelegateCommandFactory.GetSimpleMethodWithoutResult(useCommandManager != null)
                    : DelegateCommandFactory.GetSimpleMethodWithResult(commandMethodReturnType, useCommandManager != null));
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
            if(isAsyncCommand) {
                AsyncCommandAttribute attribute = ViewModelBase.GetAttribute<AsyncCommandAttribute>(commandMethod);
                bool allowMultipleExecution = attribute != null ? attribute.AllowMultipleExecution : false;
                gen.Emit(allowMultipleExecution ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            }
            if(useCommandManager != null)
                gen.Emit(useCommandManager.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
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
                if(AttributeBuilderProviders.TryGetValue(attribute.GetType(), out provider))
                    commandProperty.SetCustomAttribute(provider.CreateAttributeBuilder(attribute));
            }
        }
        #endregion

        #region services
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
                ServicePropertyAttribute attribute = BuilderCommon.GetAttribute<ServicePropertyAttribute>(propertyInfo);
                bool required = DataAnnotationsAttributeHelper.HasRequiredAttribute(propertyInfo);
                var getter = BuildGetServicePropertyMethod(typeBuilder, propertyInfo, attribute.With(x => x.Key), attribute.Return(x => x.SearchMode, () => default(ServiceSearchMode)), required);
                typeBuilder.DefineMethodOverride(getter, propertyInfo.GetGetMethod(true));
                var newProperty = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);
                newProperty.SetGetMethod(getter);
            }
        }
        static bool IsServiceProperty(PropertyInfo property) {
            ServicePropertyAttribute attribute = BuilderCommon.GetAttribute<ServicePropertyAttribute>(property);
            if(attribute != null && !attribute.IsServiceProperty)
                return false;

            string propertyTypeName = property.PropertyType.Name;
            if (propertyTypeName.Contains('`'))
                propertyTypeName = propertyTypeName.Substring(0, propertyTypeName.IndexOf('`'));
            if (!propertyTypeName.EndsWith("Service") && attribute == null)
                return false;

            if(!property.PropertyType.IsInterface)
                return ViewModelSourceException.ReturnFalseOrThrow(attribute, ViewModelSourceException.Error_PropertyTypeShouldBeServiceType, property);
            var getter = property.GetGetMethod(true);
            if(!BuilderCommon.CanAccessFromDescendant(getter))
                return ViewModelSourceException.ReturnFalseOrThrow(attribute, ViewModelSourceException.Error_CantAccessProperty, property);
            if(!getter.IsVirtual)
                return ViewModelSourceException.ReturnFalseOrThrow(attribute, ViewModelSourceException.Error_PropertyIsNotVirtual, property);
            if(getter.IsFinal)
                return ViewModelSourceException.ReturnFalseOrThrow(attribute, ViewModelSourceException.Error_PropertyIsFinal, property);
            if(property.GetSetMethod(true) != null)
                return ViewModelSourceException.ReturnFalseOrThrow(attribute, ViewModelSourceException.Error_PropertyHasSetter, property);
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
            Expression<Func<ISupportServices, IServiceContainer>> serviceContainerPropertyExpression = x => x.ServiceContainer;
            Type[] getServiceMethodParams = new Type[] { typeof(string), typeof(ServiceSearchMode) };
            MethodInfo getServiceMethod =
                required
                ? typeof(ServiceContainerExtensions).GetMethod("GetRequiredService", BindingFlags.Static | BindingFlags.Public, null,
                    new Type[] { typeof(IServiceContainer), typeof(string), typeof(ServiceSearchMode) }, null)
                : typeof(IServiceContainer).GetMethod("GetService", BindingFlags.Instance | BindingFlags.Public, null,
                    new Type[] { typeof(string), typeof(ServiceSearchMode) }, null);
            getServiceMethod = getServiceMethod.MakeGenericMethod(property.PropertyType);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, ExpressionHelper.GetArgumentPropertyStrict(serviceContainerPropertyExpression).GetGetMethod());
            if(string.IsNullOrEmpty(serviceName))
                gen.Emit(OpCodes.Ldnull);
            else gen.Emit(OpCodes.Ldstr, serviceName);
            gen.Emit(OpCodes.Ldc_I4_S, (int)searchMode);
            gen.Emit(required ? OpCodes.Call : OpCodes.Callvirt, getServiceMethod);
            gen.Emit(OpCodes.Ret);
            return method;
        }
        #endregion

        #region ISupportParentViewModel
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
            Label returnLabel = gen.DefineLabel();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse_S, returnLabel);

            Expression<Func<InvalidOperationException>> exceptionExpression = () => new InvalidOperationException((string)null);
            ConstructorInfo exceptionCtor = ExpressionHelper.GetConstructor(exceptionExpression);
            gen.Emit(OpCodes.Ldstr, ViewModelBase.Error_ParentViewModel);
            gen.Emit(OpCodes.Newobj, exceptionCtor);
            gen.Emit(OpCodes.Throw);

            gen.MarkLabel(returnLabel);
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
    public class ViewModelSourceBuilderBase {
        static ViewModelSourceBuilderBase _default;
        internal static ViewModelSourceBuilderBase Default { get { return _default ?? (_default = new ViewModelSourceBuilderBase()); } }

        public void BuildBindableProperties(Type type, TypeBuilder typeBuilder, MethodInfo raisePropertyChangedMethod, MethodInfo raisePropertyChangingMethod, Action<object, object, object, string> customSetterAction) {
            var bindableProps = GetBindableProperties(type);
            var propertyRelations = GetPropertyRelations(type, bindableProps);
            foreach(var propertyInfo in bindableProps) {
                var newProperty = BuilderBindableProperty.BuildBindableProperty(type, typeBuilder,
                    propertyInfo, raisePropertyChangedMethod, raisePropertyChangingMethod, customSetterAction,
                    DictionaryExtensions.GetValueOrDefault(propertyRelations, propertyInfo.Name, null));
                BuildBindablePropertyAttributes(propertyInfo, newProperty);
            }
        }
        public IEnumerable<PropertyInfo> GetBindableProperties(Type type) {
            return type.GetProperties().Where(x => BuilderCommon.IsBindableProperty(x) || ForceOverrideProperty(x));
        }
        protected virtual bool ForceOverrideProperty(PropertyInfo property) {
            return false;
        }
        protected virtual void BuildBindablePropertyAttributes(PropertyInfo property, PropertyBuilder builder) { }

        static Dictionary<string, IEnumerable<string>> GetPropertyRelations(Type type, IEnumerable<PropertyInfo> bindableProperties) {
            Dictionary<string, IEnumerable<string>> res = new Dictionary<string, IEnumerable<string>>();
            var allProps = type.GetProperties();
            var allPropNames = allProps.Select(x => x.Name);
            var bindablePropertyNames = bindableProperties.Select(x => x.Name);
            foreach(var prop in allProps) {
                var attrs = BuilderCommon.GetAttributes<DependsOnPropertiesAttribute>(prop, true);
                var dependsOn = attrs.Any() ?
                    attrs.Select(x => x.Properties).Aggregate((x, y) => x.Concat(y).ToArray()).Distinct() : null;
                if(dependsOn == null) continue;
                foreach(var dependedProp in dependsOn) {
                    if(!allPropNames.Contains(dependedProp))
                        throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_DependsOnNotExist, prop.Name, dependedProp));
                    if(!bindablePropertyNames.Contains(dependedProp))
                        throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_DependsOnNotBindable, prop.Name, dependedProp));
                    if(!res.ContainsKey(dependedProp)) res.Add(dependedProp, new string[] { });
                    res[dependedProp] = res[dependedProp].Concat(prop.Name.Yield());
                }
            }
            return res;
        }
    }
    static class BuilderCommon {
        public static POCOViewModelAttribute GetPOCOViewModelAttribute(Type type) {
            return (POCOViewModelAttribute)type.GetCustomAttributes(typeof(POCOViewModelAttribute), false).FirstOrDefault();
        }
        public static bool CanAccessFromDescendant(MethodBase method) {
            return method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;
        }
        public static T GetAttribute<T>(MemberInfo member) where T : Attribute {
            return MetadataHelper.GetAttribute<T>(member);
        }
        public static IEnumerable<T> GetAttributes<T>(MemberInfo member, bool inherit) where T : Attribute {
            return MetadataHelper.GetAttributes<T>(member, inherit);
        }
        public static bool ShouldImplementIDataErrorInfo(Type type) {
            var pocoViewModelAttr = GetPOCOViewModelAttribute(type);
            bool implement = pocoViewModelAttr == null ? false : pocoViewModelAttr.ImplementIDataErrorInfo;
            if(type.GetInterfaces().Contains(typeof(IDataErrorInfo)) && implement)
                throw new ViewModelSourceException(ViewModelSourceException.Error_IDataErrorInfoAlreadyImplemented);
            return implement;
        }

        public static bool IsBindableProperty(PropertyInfo propertyInfo) {
            var bindable = GetBindablePropertyAttribute(propertyInfo);
            if(bindable != null && !bindable.IsBindable)
                return false;

            var getMethod = propertyInfo.GetGetMethod();
            var setMethod = propertyInfo.GetSetMethod(true);
            if(getMethod == null)
                return ViewModelSourceException.ReturnFalseOrThrow(bindable, ViewModelSourceException.Error_PropertyHasNoGetter, propertyInfo);
            if(!getMethod.IsVirtual)
                return ViewModelSourceException.ReturnFalseOrThrow(bindable, ViewModelSourceException.Error_PropertyIsNotVirual, propertyInfo);
            if(getMethod.IsFinal)
                return ViewModelSourceException.ReturnFalseOrThrow(bindable, ViewModelSourceException.Error_PropertyIsFinal, propertyInfo);
            if(setMethod == null)
                return ViewModelSourceException.ReturnFalseOrThrow(bindable, ViewModelSourceException.Error_PropertyHasNoSetter, propertyInfo);
            if(setMethod.IsAssembly)
                return ViewModelSourceException.ReturnFalseOrThrow(bindable, ViewModelSourceException.Error_PropertyHasInternalSetter, propertyInfo);
            if(!(IsAutoImplemented(propertyInfo)))
                return bindable != null && bindable.IsBindable;
            return true;
        }
        public static BindablePropertyAttribute GetBindablePropertyAttribute(PropertyInfo propertyInfo) {
            return GetAttribute<BindablePropertyAttribute>(propertyInfo);
        }
        public static bool IsAutoImplemented(PropertyInfo property) {
            if(property.GetGetMethod().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any())
                return true;
            if(property.GetSetMethod(true).GetParameters().Single().Name != "AutoPropertyValue")
                return false;
            FieldInfo field = property.DeclaringType.GetField("_" + property.Name, BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null && field.FieldType == property.PropertyType && field.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
        }
    }
    static class BuilderType {
        public static TypeBuilder CreateTypeBuilder(Type baseType) {
            ModuleBuilder module = GetModuleBuilder(baseType.Assembly);
            List<Type> interfaces = new List<Type>(new[] {
                typeof(INotifyPropertyChanged),
                typeof(IPOCOViewModel),
                typeof(ISupportServices),
                typeof(ISupportParentViewModel) });
            if(BuilderCommon.ShouldImplementIDataErrorInfo(baseType))
                interfaces.Add(typeof(IDataErrorInfo));
            if(ShouldImplementINotifyPropertyChanging(baseType))
                interfaces.Add(typeof(INotifyPropertyChanging));
            string typeName = baseType.Name + "_" + Guid.NewGuid().ToString().Replace('-', '_');
            return module.DefineType(typeName, TypeAttributes.Public, baseType, interfaces.ToArray());
        }

        public static void BuildConstructors(Type type, TypeBuilder typeBuilder) {
            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => BuilderCommon.CanAccessFromDescendant(x)).ToArray();
            if(!ctors.Any()) {
                throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_TypeHasNoCtors, type.Name));
            }
            foreach(ConstructorInfo constructor in ctors) {
                BuildConstructor(typeBuilder, constructor);
            }
        }
        static ConstructorBuilder BuildConstructor(TypeBuilder type, ConstructorInfo baseConstructor) {
            var parameters = baseConstructor.GetParameters();
            ConstructorBuilder method = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameters.Select(x => x.ParameterType).ToArray());
            for(int i = 0; i < parameters.Length; i++)
                method.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);
            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            for(int i = 0; i < parameters.Length; i++)
                gen.Emit(OpCodes.Ldarg_S, i + 1);
            gen.Emit(OpCodes.Call, baseConstructor);
            gen.Emit(OpCodes.Ret);
            return method;
        }
        [ThreadStatic]
        static Dictionary<Assembly, ModuleBuilder> builders;
        static Dictionary<Assembly, ModuleBuilder> Builders {
            get { return builders ?? (builders = new Dictionary<Assembly, ModuleBuilder>()); }
        }
        static ModuleBuilder GetModuleBuilder(Assembly assembly) {
            return Builders.GetOrAdd(assembly, () => CreateBuilder());
        }
        static ModuleBuilder CreateBuilder() {
            var assemblyName = new AssemblyName();
            assemblyName.Name = AssemblyInfo.SRAssemblyXpfMvvm + ".DynamicTypes." + Guid.NewGuid().ToString();
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }
        static bool ShouldImplementINotifyPropertyChanging(Type type) {
            if(type.GetInterfaces().Contains(typeof(INotifyPropertyChanging)))
                return false;
            var pocoViewModelAttr = BuilderCommon.GetPOCOViewModelAttribute(type);
            return pocoViewModelAttr == null ? false : pocoViewModelAttr.ImplementINotifyPropertyChanging;
        }
    }
    static class BuilderINPC {
        public static void ImplementINPC(Type baseType, TypeBuilder typeBuilder, out MethodInfo raisePropertyChanged, out MethodInfo raisePropertyChanging) {
            raisePropertyChanged = null;
            raisePropertyChanging = null;
            MethodBuilder _getHelper = null;
            Func<MethodBuilder> getHelper = () => _getHelper ?? (_getHelper = BuildMethod_GetEventHelper(typeBuilder));
            Func<INPCInfo, MethodInfo> buildINPC = (inpcInfo) => {
                MethodInfo raiseEventMethod = CheckExistingMethod_RaiseEvent(inpcInfo, baseType);
                if(raiseEventMethod == null) {
                    raiseEventMethod = BuildRaiseEventHandlerMethod(typeBuilder, getHelper(), inpcInfo);
                    BuildAddRemoveEventHandlers(typeBuilder, getHelper(), inpcInfo);
                }
                return raiseEventMethod;
            };
            raisePropertyChanged = buildINPC(INPCInfo.INPChangedInfo);
            if(ImplementINPChanging(baseType))
                raisePropertyChanging = buildINPC(INPCInfo.INPChangingInfo);
        }
        static MethodInfo CheckExistingMethod_RaiseEvent(INPCInfo inpcInfo, Type baseType) {
            if(!inpcInfo.InterfaceType.IsAssignableFrom(baseType)) return null;
            var res = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x => {
                    var parameters = x.GetParameters();
                    return x.Name == inpcInfo.RaiseEventMethodName
                        && BuilderCommon.CanAccessFromDescendant(x)
                        && parameters.Length == 1 && parameters[0].ParameterType == typeof(string)
                        && !parameters[0].IsOut && !parameters[0].ParameterType.IsByRef;
                });
            if(res == null)
                throw new ViewModelSourceException(string.Format(inpcInfo.RaiseEventMethodNotFoundException, baseType.Name));
            return res;
        }

        static MethodBuilder BuildMethod_GetEventHelper(TypeBuilder type) {
            FieldBuilder helperField = type.DefineField("_inpcEventHelper_", typeof(INPCEventHelper), FieldAttributes.Private);
            MethodBuilder method = type.DefineMethod("_GetINPCEventHelper_", MethodAttributes.Private | MethodAttributes.HideBySig);
            Expression<Func<INPCEventHelper>> createHelperExpression = () => new INPCEventHelper();
            ConstructorInfo helperCtor = ExpressionHelper.GetConstructor(createHelperExpression);

            method.SetReturnType(typeof(INPCEventHelper));
            ILGenerator gen = method.GetILGenerator();
            gen.DeclareLocal(typeof(INPCEventHelper));
            Label returnLabel = gen.DefineLabel();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, helperField);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Brtrue_S, returnLabel);
            gen.Emit(OpCodes.Pop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Newobj, helperCtor);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Stfld, helperField);
            gen.Emit(OpCodes.Ldloc_0);
            gen.MarkLabel(returnLabel);
            gen.Emit(OpCodes.Ret);

            return method;
        }
        static void BuildAddRemoveEventHandlers(TypeBuilder type, MethodInfo getHelperMethod, INPCInfo inpcInfo) {
            Func<string, Expression<Action>, MethodBuilder> emitAddRemoveEventHandler = (methodName, handlerExpression) => {
                MethodBuilder method = type.DefineMethod(inpcInfo.InterfaceType.FullName + "." + methodName + "_" + inpcInfo.EventName,
                    MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot);
                method.SetReturnType(typeof(void));
                method.SetParameters(inpcInfo.EventHandlerType);
                ParameterBuilder value = method.DefineParameter(1, ParameterAttributes.None, "value");
                ILGenerator gen = method.GetILGenerator();

                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, getHelperMethod);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, ExpressionHelper.GetMethod(handlerExpression));
                gen.Emit(OpCodes.Ret);
                return method;
            };
            var addHandler = emitAddRemoveEventHandler("add", inpcInfo.AddEventHandlerHelperMethod);
            type.DefineMethodOverride(addHandler, inpcInfo.GetAddEventMethod());
            var removeHandler = emitAddRemoveEventHandler("remove", inpcInfo.RemoveEventHandlerHelperMethod);
            type.DefineMethodOverride(removeHandler, inpcInfo.GetRemoveEventMethod());
        }
        static MethodBuilder BuildRaiseEventHandlerMethod(TypeBuilder type, MethodInfo getHelperMethod, INPCInfo inpcInfo) {
            MethodBuilder method = type.DefineMethod(inpcInfo.RaiseEventMethodName, MethodAttributes.Family | MethodAttributes.HideBySig);
            method.SetReturnType(typeof(void));
            method.SetParameters(typeof(String));
            ParameterBuilder propertyName = method.DefineParameter(1, ParameterAttributes.None, "propertyName");
            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, getHelperMethod);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, ExpressionHelper.GetMethod(inpcInfo.RaiseEventHandlerHelperMethod));
            gen.Emit(OpCodes.Ret);
            return method;
        }
        static bool ImplementINPChanging(Type type) {
            var attr = BuilderCommon.GetPOCOViewModelAttribute(type);
            return attr == null ? false : attr.ImplementINotifyPropertyChanging;
        }

        class INPCInfo {
            public readonly static INPCInfo INPChangedInfo =
                new INPCInfo() {
                    InterfaceType = typeof(INotifyPropertyChanged),
                    EventHandlerType = typeof(PropertyChangedEventHandler),
                    EventName = "PropertyChanged",
                    RaiseEventMethodName = "RaisePropertyChanged",
                    RaiseEventMethodNotFoundException = ViewModelSourceException.Error_RaisePropertyChangedMethodNotFound,
                    AddEventHandlerHelperMethod = () => new INPCEventHelper().AddPropertyChangedHandler(null),
                    RemoveEventHandlerHelperMethod = () => new INPCEventHelper().RemovePropertyChangedHandler(null),
                    RaiseEventHandlerHelperMethod = () => new INPCEventHelper().OnPropertyChanged(null, null),
                };
            public readonly static INPCInfo INPChangingInfo =
                new INPCInfo() {
                    InterfaceType = typeof(INotifyPropertyChanging),
                    EventHandlerType = typeof(PropertyChangingEventHandler),
                    EventName = "PropertyChanging",
                    RaiseEventMethodName = "RaisePropertyChanging",
                    RaiseEventMethodNotFoundException = ViewModelSourceException.Error_RaisePropertyChangingMethodNotFound,
                    AddEventHandlerHelperMethod = () => new INPCEventHelper().AddPropertyChangingHandler(null),
                    RemoveEventHandlerHelperMethod = () => new INPCEventHelper().RemovePropertyChangingHandler(null),
                    RaiseEventHandlerHelperMethod = () => new INPCEventHelper().OnPropertyChanging(null, null),
                };

            public Type InterfaceType { get; private set; }
            public Type EventHandlerType { get; private set; }
            public string EventName { get; private set; }
            public string RaiseEventMethodName { get; private set; }
            public string RaiseEventMethodNotFoundException { get; private set; }
            public Expression<Action> AddEventHandlerHelperMethod { get; private set; }
            public Expression<Action> RemoveEventHandlerHelperMethod { get; private set; }
            public Expression<Action> RaiseEventHandlerHelperMethod { get; private set; }
            public MethodInfo GetAddEventMethod() { return InterfaceType.GetMethod("add_" + EventName); }
            public MethodInfo GetRemoveEventMethod() { return InterfaceType.GetMethod("remove_" + EventName); }
            INPCInfo() { }
        }
    }
    static class BuilderIPOCOViewModel {
        public static void ImplementIPOCOViewModel(Type baseType, TypeBuilder typeBuilder, MethodInfo raisePropertyChangedMethod, MethodInfo raisePropertyChangingMethod) {
            Expression<Action> raisePropertyChangedExpression =
                () => ((IPOCOViewModel)null).RaisePropertyChanged(null);
            typeBuilder.DefineMethodOverride(
                RaiseEventMethod(baseType, typeBuilder, raisePropertyChangedMethod, "RaisePropertyChanged"),
                ExpressionHelper.GetMethod(raisePropertyChangedExpression));

            Expression<Action> raisePropertyChangingExpression =
                () => ((IPOCOViewModel)null).RaisePropertyChanging(null);
            typeBuilder.DefineMethodOverride(
                RaiseEventMethod(baseType, typeBuilder, raisePropertyChangingMethod, "RaisePropertyChanging"),
                ExpressionHelper.GetMethod(raisePropertyChangingExpression));
        }
        static MethodBuilder RaiseEventMethod(Type baseType, TypeBuilder type, MethodInfo raiseMethod, string methodName) {
            MethodBuilder method = type.DefineMethod("DevExpress.Mvvm.Native.IPOCOViewModel" + methodName,
                MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot);
            method.SetReturnType(typeof(void));
            method.SetParameters(typeof(String));
            ParameterBuilder propertyName = method.DefineParameter(1, ParameterAttributes.None, "propertyName");
            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            if(raiseMethod != null)
                gen.Emit(OpCodes.Call, raiseMethod);
            else {
                Expression<Func<ViewModelSourceException>> exceptionExpression = () => new ViewModelSourceException((string)null);
                ConstructorInfo exceptionCtor = ExpressionHelper.GetConstructor(exceptionExpression);
                gen.Emit(OpCodes.Ldstr, string.Format(ViewModelSourceException.Error_INotifyPropertyChangingIsNotImplemented, baseType.Name));
                gen.Emit(OpCodes.Newobj, exceptionCtor);
                gen.Emit(OpCodes.Throw);
            }
            gen.Emit(OpCodes.Ret);
            return method;
        }
    }
    static class BuilderBindableProperty {
        public static PropertyBuilder BuildBindableProperty(Type type, TypeBuilder typeBuilder, PropertyInfo propertyInfo,
            MethodInfo raisePropertyChangedMethod, MethodInfo raisePropertyChangingMethod, Action<object, object, object, string> customSetterAction, IEnumerable<string> relatedProperties) {

            var getter = BuildBindablePropertyGetter(typeBuilder, propertyInfo.GetGetMethod());
            typeBuilder.DefineMethodOverride(getter, propertyInfo.GetGetMethod());
            MethodInfo propertyChangedMethod = GetPropertyChangedMethod(type, propertyInfo, "Changed", x => x.OnPropertyChangedMethodName, x => x.OnPropertyChangedMethod);
            MethodInfo propertyChangingMethod = GetPropertyChangedMethod(type, propertyInfo, "Changing", x => x.OnPropertyChangingMethodName, x => x.OnPropertyChangingMethod);
            var onChangedFirst = ShouldInvokeOnPropertyChangedMethodsFirst(type);
            var setter = BuildBindablePropertySetter(typeBuilder, propertyInfo,
                raisePropertyChangedMethod,
                raisePropertyChangingMethod,
                propertyChangedMethod,
                propertyChangingMethod,
                customSetterAction,
                relatedProperties,
                onChangedFirst);
            typeBuilder.DefineMethodOverride(setter, propertyInfo.GetSetMethod(true));
            var newProperty = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, new Type[0]);
            newProperty.SetGetMethod(getter);
            newProperty.SetSetMethod(setter);
            return newProperty;
        }

        static MethodBuilder BuildBindablePropertyGetter(TypeBuilder type, MethodInfo originalGetter) {
            MethodBuilder method = type.DefineMethod(originalGetter.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig);
            method.SetReturnType(originalGetter.ReturnType);

            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, originalGetter);
            gen.Emit(OpCodes.Ret);
            return method;
        }
        static MethodBuilder BuildBindablePropertySetter(TypeBuilder type, PropertyInfo property, MethodInfo raisePropertyChangedMethod, MethodInfo raisePropertyChangingMethod, MethodInfo propertyChangedMethod, MethodInfo propertyChangingMethod, Action<object, object, object, string> customSetterAction, IEnumerable<string> relatedProperties, bool onChangedFirst) {
            var setMethod = property.GetSetMethod(true);
            MethodAttributes methodAttributes = (setMethod.IsPublic ? MethodAttributes.Public : MethodAttributes.Family) | MethodAttributes.Virtual | MethodAttributes.HideBySig;
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

            if(onChangedFirst) {
                EmitPropertyChanging(gen, propertyChangingMethod);
                EmitRaisePropertyChangedOrChanging(gen, raisePropertyChangingMethod, property.Name);
            } else {
                EmitRaisePropertyChangedOrChanging(gen, raisePropertyChangingMethod, property.Name);
                EmitPropertyChanging(gen, propertyChangingMethod);
            }
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, setMethod);
            if(onChangedFirst) {
                EmitPropertyChanged(gen, propertyChangedMethod);
                EmitRaisePropertyChangedOrChanging(gen, raisePropertyChangedMethod, property.Name);
            } else {
                EmitRaisePropertyChangedOrChanging(gen, raisePropertyChangedMethod, property.Name);
                EmitPropertyChanged(gen, propertyChangedMethod);
            }

            if (customSetterAction != null)
            {
                EmitCustomChangedMethod(gen, customSetterAction, property.Name, shouldBoxValues, property);
            }

            EmitRaiseRelatedPropertyChanged(gen, raisePropertyChangedMethod, relatedProperties);
            gen.MarkLabel(returnLabel);
            gen.Emit(OpCodes.Ret);
            return method;
        }

        static void EmitCustomChangedMethod(ILGenerator gen, Action<object, object, object, string> customChangedMethod, string pName, bool shouldBoxValues, PropertyInfo property)
        {

            if (customChangedMethod == null) return;
            var methodInfo = customChangedMethod.Method;

            if (methodInfo == null) return;
            gen.Emit(OpCodes.Ldloc_0);
            if (shouldBoxValues)
            {
                gen.Emit(OpCodes.Box, property.PropertyType);
            }

            gen.Emit(OpCodes.Ldarg_1);
            if (shouldBoxValues)
            {
                gen.Emit(OpCodes.Box, property.PropertyType);
            }

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, pName);
            gen.Emit(OpCodes.Call, methodInfo);

        }

        static void EmitRaiseRelatedPropertyChanged(ILGenerator gen, MethodInfo m, IEnumerable<string> relatedProperties) {
            if(relatedProperties == null) return;
            foreach(string prop in relatedProperties)
                EmitRaisePropertyChangedOrChanging(gen, m, prop);
        }
        static void EmitRaisePropertyChangedOrChanging(ILGenerator gen, MethodInfo m, string pName) {
            if(m == null) return;
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, pName);
            gen.Emit(OpCodes.Call, m);
        }
        static void EmitPropertyChanging(ILGenerator gen, MethodInfo m) {
            if(m == null) return;
            gen.Emit(OpCodes.Ldarg_0);
            if(m.GetParameters().Length == 1) gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, m);
        }
        static void EmitPropertyChanged(ILGenerator gen, MethodInfo m) {
            if(m == null) return;
            gen.Emit(OpCodes.Ldarg_0);
            if(m.GetParameters().Length == 1) gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, m);
        }

        static MethodInfo GetPropertyChangedMethod(Type type, PropertyInfo propertyInfo, string methodNameSuffix, Func<BindablePropertyAttribute, string> getMethodName, Func<BindablePropertyAttribute, MethodInfo> getMethod) {
            var bindable = BuilderCommon.GetBindablePropertyAttribute(propertyInfo);
            if(bindable != null && getMethod(bindable) != null) {
                CheckOnChangedMethod(getMethod(bindable), propertyInfo.PropertyType);
                return getMethod(bindable);
            }
            bool hasCustomPropertyChangedMethodName = bindable != null && !string.IsNullOrEmpty(getMethodName(bindable));
            if(!hasCustomPropertyChangedMethodName && !(BuilderCommon.IsAutoImplemented(propertyInfo)))
                return null;
            string onChangedMethodName = hasCustomPropertyChangedMethodName ? getMethodName(bindable) : "On" + propertyInfo.Name + methodNameSuffix;
            MethodInfo[] changedMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name == onChangedMethodName).ToArray();
            if(changedMethods.Length > 1)
                throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_MoreThanOnePropertyChangedMethod, propertyInfo.Name));
            if(hasCustomPropertyChangedMethodName && !changedMethods.Any())
                throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_PropertyChangedMethodNotFound, onChangedMethodName));
            changedMethods.FirstOrDefault().Do(x => CheckOnChangedMethod(x, propertyInfo.PropertyType));
            return changedMethods.FirstOrDefault();
        }
        static void CheckOnChangedMethod(MethodInfo method, Type propertyType) {
            if(!BuilderCommon.CanAccessFromDescendant(method)) {
                throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_PropertyChangedMethodShouldBePublicOrProtected, method.Name));
            }
            if(method.GetParameters().Length >= 2) {
                throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_PropertyChangedCantHaveMoreThanOneParameter, method.Name));
            }
            if(method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType != propertyType) {
                throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_PropertyChangedMethodArgumentTypeShouldMatchPropertyType, method.Name));
            }
            if(method.ReturnType != typeof(void)) {
                throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_PropertyChangedCantHaveReturnType, method.Name));
            }
        }
        static bool ShouldInvokeOnPropertyChangedMethodsFirst(Type type) {
            var pocoViewModelAttr = BuilderCommon.GetPOCOViewModelAttribute(type);
            return pocoViewModelAttr == null ? false : pocoViewModelAttr.InvokeOnPropertyChangedMethodBeforeRaisingINPC;
        }
    }


    [Serializable]
    public class ViewModelSourceException : Exception {
        #region error messages
        internal const string Error_ObjectDoesntImplementIPOCOViewModel = "Object doesn't implement IPOCOViewModel.";
        internal const string Error_ObjectDoesntImplementISupportServices = "Object doesn't implement ISupportServices.";
        internal const string Error_CommandNotFound = "Command not found: {0}.";
        internal const string Error_CommandNotAsync = "Command is not async";
        internal const string Error_ConstructorNotFound = "Constructor not found.";
        internal const string Error_TypeHasNoCtors = "Type has no accessible constructors: {0}.";

        internal const string Error_SealedClass = "Cannot create dynamic class for the sealed class: {0}.";
        internal const string Error_InternalClass = "Cannot create a dynamic class for the non-public class: {0}.";
        internal const string Error_TypeImplementsIPOCOViewModel = "Type cannot implement IPOCOViewModel: {0}.";

        internal const string Error_RaisePropertyChangedMethodNotFound = "Class already supports INotifyPropertyChanged, but RaisePropertyChanged(string) method not found: {0}.";
        internal const string Error_RaisePropertyChangingMethodNotFound = "Class already supports INotifyPropertyChanging, but RaisePropertyChanging(string) method not found: {0}.";
        internal const string Error_PropertyIsNotVirual = "Cannot make non-virtual property bindable: {0}.";
        internal const string Error_PropertyHasInternalSetter = "Cannot make property with internal setter bindable: {0}.";
        internal const string Error_PropertyHasNoSetter = "Cannot make property without setter bindable: {0}.";
        internal const string Error_PropertyHasNoGetter = "Cannot make property without public getter bindable: {0}.";
        internal const string Error_PropertyIsFinal = "Cannot override final property: {0}.";
        internal const string Error_MoreThanOnePropertyChangedMethod = "More than one property changed method: {0}.";
        internal const string Error_PropertyChangedMethodShouldBePublicOrProtected = "Property changed method should be public or protected: {0}.";
        internal const string Error_PropertyChangedCantHaveMoreThanOneParameter = "Property changed method cannot have more than one parameter: {0}.";
        internal const string Error_PropertyChangedCantHaveReturnType = "Property changed method cannot have return type: {0}.";
        internal const string Error_PropertyChangedMethodArgumentTypeShouldMatchPropertyType = "Property changed method argument type should match property type: {0}.";
        internal const string Error_PropertyChangedMethodNotFound = "Property changed method not found: {0}.";

        internal const string Error_MemberWithSameCommandNameAlreadyExists = "Member with the same command name already exists: {0}.";

        internal const string Error_PropertyTypeShouldBeServiceType = "Service properties should have an interface type: {0}.";
        internal const string Error_CantAccessProperty = "Cannot access property: {0}.";
        internal const string Error_PropertyIsNotVirtual = "Property is not virtual: {0}.";
        internal const string Error_PropertyHasSetter = "Property with setter cannot be Service Property: {0}.";

        internal const string Error_ConstructorExpressionCanReferOnlyToItsArguments = "Constructor expression can refer only to its arguments.";
        internal const string Error_ConstructorExpressionCanOnlyBeOfNewExpressionType = "Constructor expression can only be of NewExpression type.";
        internal const string Error_IDataErrorInfoAlreadyImplemented = "The IDataErrorInfo interface is already implemented.";
        internal const string Error_INotifyPropertyChangingIsNotImplemented = "The INotifyPropertyChanging interface is not implemented or implemented explicitly: {0}";

        internal const string Error_DependsOnNotBindable = "The {0} property cannot depend on the {1} property, because the latter is not bindable.";
        internal const string Error_DependsOnNotExist = "The {0} property cannot depend on the {1} property, because the latter does not exist.";
        #endregion

        public ViewModelSourceException() { }
        public ViewModelSourceException(string message)
            : base(message) {

        }

        internal static bool ReturnFalseOrThrow(Attribute attribute, string text, PropertyInfo property) {
            if(attribute != null)
                throw new ViewModelSourceException(string.Format(text, property.Name));
            return false;
        }
        internal static bool ReturnFalseOrThrow(bool @throw, string text, Type type) {
            if(@throw)
                throw new ViewModelSourceException(string.Format(text, type.Name));
            return false;
        }
    }
#pragma warning disable 612,618
    public static class POCOViewModelExtensions {
        public static bool IsInDesignMode(this object viewModel) {
            return ViewModelBase.IsInDesignMode;
        }
        public static void RaisePropertiesChanged(this object viewModel) {
            IPOCOViewModel pocoViewModel = GetPOCOViewModel(viewModel);
            pocoViewModel.RaisePropertyChanged(string.Empty);
        }
        public static void RaisePropertyChanged<T, TProperty>(this T viewModel, Expression<Func<T, TProperty>> propertyExpression) {
            IPOCOViewModel pocoViewModel = GetPOCOViewModel(viewModel);
            pocoViewModel.RaisePropertyChanged(BindableBase.GetPropertyNameFast(propertyExpression));
        }
        public static IDelegateCommand GetCommand<T>(this T viewModel, Expression<Action<T>> methodExpression) {
            return GetCommandCore(viewModel, methodExpression);
        }
        public static T SetParentViewModel<T>(this T viewModel, object parentViewModel) {
            ((ISupportParentViewModel)viewModel).ParentViewModel = parentViewModel;
            return viewModel;
        }
        public static T GetParentViewModel<T>(this object viewModel) where T : class {
            return (T)((ISupportParentViewModel)viewModel).ParentViewModel;
        }
        public static IAsyncCommand GetAsyncCommand<T>(this T viewModel, Expression<Func<T, Task>> methodExpression) {
            return GetAsyncCommandCore(viewModel, methodExpression);
        }
        public static void RaiseCanExecuteChanged<T>(this T viewModel, Expression<Action<T>> methodExpression) {
            RaiseCanExecuteChangedCore(viewModel, methodExpression);
        }
        public static void RaiseCanExecuteChanged<T>(this T viewModel, Expression<Func<T, Task>> methodExpression) {
            RaiseCanExecuteChangedCore(viewModel, methodExpression);
        }

        public static bool HasError<T, TProperty>(this T viewModel, Expression<Func<T, TProperty>> propertyExpression) {
            IDataErrorInfo dataErrorInfoViewModel = viewModel as IDataErrorInfo;
            return (dataErrorInfoViewModel != null) && !string.IsNullOrEmpty(dataErrorInfoViewModel[BindableBase.GetPropertyNameFast(propertyExpression)]);
        }
        public static void UpdateFunctionBinding<T>(this T viewModel, Expression<Action<T>> methodExpression) {
            UpdateFunctionBehaviorCore(viewModel, methodExpression);
        }
        public static void UpdateMethodToCommandCanExecute<T>(this T viewModel, Expression<Action<T>> methodExpression) {
            UpdateFunctionBehaviorCore(viewModel, methodExpression);
        }
        static void UpdateFunctionBehaviorCore<T>(this T viewModel, Expression<Action<T>> methodExpression) {
            string methodName = ExpressionHelper.GetMethod(methodExpression).Name;
            GetPOCOViewModel(viewModel).RaisePropertyChanged(methodName);
        }

        #region GetService methods
        public static TService GetService<TService>(this object viewModel) where TService : class {
            return GetServiceContainer(viewModel).GetService<TService>();
        }
        public static TService GetService<TService>(this object viewModel, string key) where TService : class {
            return GetServiceContainer(viewModel).GetService<TService>(key);
        }
        public static TService GetRequiredService<TService>(this object viewModel) where TService : class {
            return ServiceContainerExtensions.GetRequiredService<TService>(GetServiceContainer(viewModel));
        }
        public static TService GetRequiredService<TService>(this object viewModel, string key) where TService : class {
            return ServiceContainerExtensions.GetRequiredService<TService>(GetServiceContainer(viewModel), key);
        }
        static IServiceContainer GetServiceContainer(object viewModel) {
            if(!(viewModel is ISupportServices))
                throw new ViewModelSourceException(ViewModelSourceException.Error_ObjectDoesntImplementISupportServices);
            return ((ISupportServices)viewModel).ServiceContainer;
        }
        #endregion

        [Obsolete("Use the GetAsyncCommand method instead.")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool GetShouldCancel<T>(this T viewModel, Expression<Func<T, Task>> methodExpression) {
            return GetAsyncCommand(viewModel, methodExpression).ShouldCancel;
        }
        [Obsolete("Use the GetAsyncCommand method instead.")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
                throw new ViewModelSourceException(ViewModelSourceException.Error_CommandNotAsync);
            return (IAsyncCommand)command;
        }
        static IDelegateCommand GetCommandCore(object viewModel, LambdaExpression methodExpression) {
            GetPOCOViewModel(viewModel);
            MethodInfo method = ExpressionHelper.GetMethod(methodExpression);
            string commandName = ViewModelSource.GetCommandName(method);
            PropertyInfo property = viewModel.GetType().GetProperty(commandName);
            if(property == null)
                throw new ViewModelSourceException(string.Format(ViewModelSourceException.Error_CommandNotFound, commandName));
            return property.GetValue(viewModel, null) as IDelegateCommand;
        }
        static IPOCOViewModel GetPOCOViewModel<T>(T viewModel) {
            IPOCOViewModel pocoViewModel = viewModel as IPOCOViewModel;
            if(pocoViewModel == null)
                throw new ViewModelSourceException(ViewModelSourceException.Error_ObjectDoesntImplementIPOCOViewModel);
            return pocoViewModel;
        }
    }
#pragma warning restore 612,618
    #region ViewModelSource<T>
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

namespace DevExpress.Mvvm.Native {
    using DevExpress.Mvvm.POCO;
    public class INPCEventHelper {
        event PropertyChangedEventHandler PropertyChanged;
        event PropertyChangingEventHandler PropertyChanging;
        public void AddPropertyChangedHandler(PropertyChangedEventHandler handler) {
            PropertyChanged += handler;
        }
        public void AddPropertyChangingHandler(PropertyChangingEventHandler handler) {
            PropertyChanging += handler;
        }
        public void RemovePropertyChangedHandler(PropertyChangedEventHandler handler) {
            PropertyChanged -= handler;
        }
        public void RemovePropertyChangingHandler(PropertyChangingEventHandler handler) {
            PropertyChanging -= handler;
        }
        public void OnPropertyChanged(INotifyPropertyChanged obj, string propertyName) {
            var handler = PropertyChanged;
            handler.Do(x => x(obj, new PropertyChangedEventArgs(propertyName)));
        }
        public void OnPropertyChanging(INotifyPropertyChanging obj, string propertyName) {
            var handler = PropertyChanging;
            handler.Do(x => x(obj, new PropertyChangingEventArgs(propertyName)));
        }
    }
}