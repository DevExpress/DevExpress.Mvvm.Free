using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.POCO;
using System.Windows.Threading;

namespace DevExpress.Mvvm {
    public abstract class ViewModelBase : BindableBase, ISupportParentViewModel, ISupportServices, ISupportParameter, ICustomTypeDescriptor {
        internal const string Error_ParentViewModel = "ViewModel cannot be parent of itself.";
        static readonly object NotSetParameter = new object();
        private object parameter = NotSetParameter;
        static bool? isInDesignMode;

        public static bool IsInDesignMode {
            get {
                if(ViewModelDesignHelper.IsInDesignModeOverride.HasValue)
                    return ViewModelDesignHelper.IsInDesignModeOverride.Value;
                if(!isInDesignMode.HasValue) {
                    DependencyPropertyDescriptor property = DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement));
                    isInDesignMode = (bool)property.Metadata.DefaultValue;
                }
                return isInDesignMode.Value;
            }
        }


        object parentViewModel;
        object ISupportParentViewModel.ParentViewModel {
            get { return parentViewModel; }
            set {
                if(parentViewModel == value)
                    return;
                if(value == this)
                    throw new InvalidOperationException(Error_ParentViewModel);
                parentViewModel = value;
                OnParentViewModelChanged(parentViewModel);
            }
        }
        IServiceContainer serviceContainer;
        IServiceContainer ISupportServices.ServiceContainer { get { return ServiceContainer; } }
        protected IServiceContainer ServiceContainer { get { return serviceContainer ?? (serviceContainer = CreateServiceContainer()); } }
        bool IsPOCOViewModel { get { return this is IPOCOViewModel; } }

        public ViewModelBase() {
            BuildCommandProperties();
            if(IsInDesignMode) {
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(OnInitializeInDesignMode));
            } else {
                OnInitializeInRuntime();
            }
        }
        protected object Parameter {
            get { return object.Equals(parameter, NotSetParameter) ? null : parameter; }
            set {
                if(parameter == value)
                    return;
                parameter = value;
                OnParameterChanged(value);
            }
        }
        object ISupportParameter.Parameter { get { return Parameter; } set { Parameter = value; } }

        protected virtual void OnParameterChanged(object parameter) {
        }
        protected virtual IServiceContainer CreateServiceContainer() {
            return new ServiceContainer(this);
        }
        protected virtual void OnParentViewModelChanged(object parentViewModel) {
        }
        protected virtual void OnInitializeInDesignMode() {
            OnParameterChanged(null);
        }
        protected virtual void OnInitializeInRuntime() {
        }
        protected virtual T GetService<T>() where T : class {
            return GetService<T>(ServiceSearchMode.PreferLocal);
        }
        protected virtual T GetService<T>(string key) where T : class {
            return GetService<T>(key, ServiceSearchMode.PreferLocal);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual T GetService<T>(ServiceSearchMode searchMode) where T : class {
            return ServiceContainer.GetService<T>(searchMode);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual T GetService<T>(string key, ServiceSearchMode searchMode) where T : class {
            return ServiceContainer.GetService<T>(key, searchMode);
        }
#region CommandAttributeSupport
        protected internal void RaiseCanExecuteChanged(Expression<Action> commandMethodExpression) {
            RaiseCanExecuteChangedCore(commandMethodExpression);
        }
        protected internal void RaiseCanExecuteChanged(Expression<Func<System.Threading.Tasks.Task>> commandMethodExpression) {
            RaiseCanExecuteChangedCore(commandMethodExpression);
        }
        void RaiseCanExecuteChangedCore(LambdaExpression commandMethodExpression) {
            if(IsPOCOViewModel) {
                POCOViewModelExtensions.RaiseCanExecuteChangedCore(this, commandMethodExpression);
            } else {
                ((IDelegateCommand)commandProperties[ExpressionHelper.GetMethod(commandMethodExpression)]
                .GetValue(this)
                ).RaiseCanExecuteChanged();
            }
        }
        internal const string CommandNameSuffix = "Command";
        const string CanExecuteSuffix = "Can";
        const string Error_PropertyWithSameNameAlreadyExists = "Property with the same name already exists: {0}.";
        internal const string Error_MethodShouldBePublic = "Method should be public: {0}.";
        const string Error_MethodCannotHaveMoreThanOneParameter = "Method cannot have more than one parameter: {0}.";
        const string Error_MethodCannotHaveOutORRefParameters = "Method cannot have out or reference parameter: {0}.";
        const string Error_MethodCannotShouldNotBeGeneric = "Method should not be generic: {0}.";
        const string Error_CanExecuteMethodHasIncorrectParameters = "Can execute method has incorrect parameters: {0}.";
        const string Error_MethodNotFound = "Method not found: {0}.";
        Dictionary<MethodInfo, CommandProperty> commandProperties;
        internal static string GetCommandName(MethodInfo commandMethod) {
            return commandMethod.Name + CommandNameSuffix;
        }
        internal static string GetCanExecuteMethodName(MethodInfo commandMethod) {
            return CanExecuteSuffix + commandMethod.Name;
        }
        internal static T GetAttribute<T>(MethodInfo method) {
            return MetadataHelper.GetAllAttributes(method).OfType<T>().FirstOrDefault();
        }

        [ThreadStatic]
        static Dictionary<Type, Dictionary<MethodInfo, CommandProperty>> propertiesCache;
        static Dictionary<Type, Dictionary<MethodInfo, CommandProperty>> PropertiesCache { get { return propertiesCache ?? (propertiesCache = new Dictionary<Type, Dictionary<MethodInfo, CommandProperty>>()); } }
        void BuildCommandProperties() {
            commandProperties = IsPOCOViewModel ? new Dictionary<MethodInfo, CommandProperty>() : GetCommandProperties(GetType());
        }
        static Dictionary<MethodInfo, CommandProperty> GetCommandProperties(Type type) {
            Dictionary<MethodInfo, CommandProperty> result = PropertiesCache.GetOrAdd(type, () => CreateCommandProperties(type));
            return result;
        }
        static Dictionary<MethodInfo, CommandProperty> CreateCommandProperties(Type type) {
            Dictionary<MethodInfo, CommandProperty> commandProperties = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => GetAttribute<CommandAttribute>(x) != null).ToArray()
                .Select(x => {
                    CommandAttribute attribute = GetAttribute<CommandAttribute>(x);
                    string name = attribute.Name ?? (x.Name.EndsWith(CommandNameSuffix) ? x.Name : GetCommandName(x));

                    MethodInfo canExecuteMethod = GetCanExecuteMethod(type, x, attribute, s => new CommandAttributeException(s), m => m.IsPublic);
                    var attributes = MetadataHelper.GetAllAttributes(x);
                    return new CommandProperty(x, canExecuteMethod, name, attribute.GetUseCommandManager(), attributes, type, attribute.AllowMultipleExecutionCore);
                })
                .ToDictionary(x => x.Method);
            foreach(var property in commandProperties.Values) {
                if(type.GetProperty(property.Name) != null || commandProperties.Values.Any(x => x.Name == property.Name && x != property))
                    throw new CommandAttributeException(string.Format(Error_PropertyWithSameNameAlreadyExists, property.Name));
                if(!property.Method.IsPublic)
                    throw new CommandAttributeException(string.Format(Error_MethodShouldBePublic, property.Method.Name));
                ValidateCommandMethodParameters(property.Method, x => new CommandAttributeException(x));
            }
            return commandProperties;
        }
        internal static bool ValidateCommandMethodParameters(MethodInfo method, Func<string, Exception> createException) {
            ParameterInfo[] parameters = method.GetParameters();
            if(CheckCommandMethodConditionValue(parameters.Length <= 1, method, Error_MethodCannotHaveMoreThanOneParameter, createException))
                return false;
            bool isValidSingleParameter = (parameters.Length == 1) && (parameters[0].IsOut || parameters[0].ParameterType.IsByRef);
            if(CheckCommandMethodConditionValue(!isValidSingleParameter, method, Error_MethodCannotHaveOutORRefParameters, createException))
                return false;
            if(CheckCommandMethodConditionValue(!method.IsGenericMethodDefinition, method, Error_MethodCannotShouldNotBeGeneric, createException))
                return false;
            return true;
        }
        static bool CheckCommandMethodConditionValue(bool value, MethodInfo method, string errorString, Func<string, Exception> createException) {
            CommandAttribute attribute = GetAttribute<CommandAttribute>(method);
            if(!value && attribute != null && attribute.IsCommand)
                throw createException(string.Format(errorString, method.Name));
            return !value;
        }
        internal static MethodInfo GetCanExecuteMethod(Type type, MethodInfo methodInfo, CommandAttribute commandAttribute, Func<string, Exception> createException, Func<MethodInfo, bool> canAccessMethod) {
            if(commandAttribute != null && commandAttribute.CanExecuteMethod != null) {
                CheckCanExecuteMethod(methodInfo, createException, commandAttribute.CanExecuteMethod, canAccessMethod);
                return commandAttribute.CanExecuteMethod;
            }
            bool hasCustomCanExecuteMethod = commandAttribute != null && !string.IsNullOrEmpty(commandAttribute.CanExecuteMethodName);
            string canExecuteMethodName = hasCustomCanExecuteMethod ? commandAttribute.CanExecuteMethodName : GetCanExecuteMethodName(methodInfo);
            MethodInfo canExecuteMethod = type.GetMethod(canExecuteMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if(hasCustomCanExecuteMethod && canExecuteMethod == null)
                throw createException(string.Format(Error_MethodNotFound, commandAttribute.CanExecuteMethodName));
            if(canExecuteMethod != null) {
                CheckCanExecuteMethod(methodInfo, createException, canExecuteMethod, canAccessMethod);
            }
            return canExecuteMethod;
        }

        static void CheckCanExecuteMethod(MethodInfo methodInfo, Func<string, Exception> createException, MethodInfo canExecuteMethod, Func<MethodInfo, bool> canAccessMethod) {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            ParameterInfo[] canExecuteParameters = canExecuteMethod.GetParameters();
            if(parameters.Length != canExecuteParameters.Length)
                throw createException(string.Format(Error_CanExecuteMethodHasIncorrectParameters, canExecuteMethod.Name));
            if(parameters.Length == 1 && (parameters[0].ParameterType != canExecuteParameters[0].ParameterType || parameters[0].IsOut != canExecuteParameters[0].IsOut))
                throw createException(string.Format(Error_CanExecuteMethodHasIncorrectParameters, canExecuteMethod.Name));
            if(!canAccessMethod(canExecuteMethod))
                throw createException(string.Format(Error_MethodShouldBePublic, canExecuteMethod.Name));
        }
        public static class CreateCommandHelper<T> {
            public static IDelegateCommand CreateCommand(object owner, MethodInfo method, MethodInfo canExecuteMethod, bool? useCommandManager, bool hasParameter) {
                return new DelegateCommand<T>(
                    x => method.Invoke(owner, GetInvokeParameters(x, hasParameter)),
                    GetCanExecute(owner, canExecuteMethod, hasParameter),
                    useCommandManager
                );
            }
            public static IDelegateCommand CreateAsyncCommand(object owner, MethodInfo method, MethodInfo canExecuteMethod, bool? useCommandManager, bool hasParameter, bool allowMultipleExecution) {
                return new AsyncCommand<T>(
                    x => (System.Threading.Tasks.Task)method.Invoke(owner, GetInvokeParameters(x, hasParameter)),
                    GetCanExecute(owner, canExecuteMethod, hasParameter),
                    allowMultipleExecution: allowMultipleExecution,
                    useCommandManager: useCommandManager
                );
            }
            static Func<T, bool> GetCanExecute(object owner, MethodInfo canExecuteMethod, bool hasParameter) {
                return x => canExecuteMethod != null ? (bool)canExecuteMethod.Invoke(owner, GetInvokeParameters(x, hasParameter)) : true;
            }
            static object[] GetInvokeParameters(object parameter, bool hasParameter) {
                return hasParameter ? new[] { parameter } : new object[0];
            }
        }
        readonly Dictionary<MethodInfo, IDelegateCommand> commands = new Dictionary<MethodInfo, IDelegateCommand>();
        IDelegateCommand GetCommand(MethodInfo method, MethodInfo canExecuteMethod, bool? useCommandManager, bool hasParameter, bool allowMultipleExecution) {
            return commands.GetOrAdd(method, () => CreateCommand(method, canExecuteMethod, useCommandManager, hasParameter, allowMultipleExecution));
        }
        IDelegateCommand CreateCommand(MethodInfo method, MethodInfo canExecuteMethod, bool? useCommandManager, bool hasParameter, bool allowMultipleExecution) {
            bool isAsync = method.ReturnType == typeof(System.Threading.Tasks.Task);
            Type commandType = hasParameter ? method.GetParameters()[0].ParameterType : typeof(object);
            var args = new object[] { this, method, canExecuteMethod, useCommandManager, hasParameter };
            return (IDelegateCommand)typeof(CreateCommandHelper<>).MakeGenericType(commandType)
                .GetMethod(isAsync ? nameof(CreateCommandHelper<object>.CreateAsyncCommand) : nameof(CreateCommandHelper<object>.CreateCommand), BindingFlags.Static | BindingFlags.Public)
                .Invoke(null, isAsync ? args.Concat(allowMultipleExecution.Yield<object>()).ToArray() : args);
        }
        #region CommandProperty
        class CommandProperty :
            PropertyDescriptor
        {
            readonly MethodInfo method;
            readonly MethodInfo canExecuteMethod;
            readonly string name;
            readonly bool? useCommandManager;
            readonly bool allowMultipleExecution;
            readonly bool hasParameter;
            readonly Attribute[] attributes;
            readonly Type reflectedType;
            public MethodInfo Method { get { return method; } }
            public MethodInfo CanExecuteMethod { get { return canExecuteMethod; } }
            public CommandProperty(MethodInfo method, MethodInfo canExecuteMethod, string name, bool? useCommandManager, Attribute[] attributes, Type reflectedType, bool allowMultipleExecution)
                : base(name, attributes) {
                this.method = method;
                this.hasParameter = method.GetParameters().Length == 1;
                this.canExecuteMethod = canExecuteMethod;
                this.name = name;
                this.useCommandManager = useCommandManager;
                this.allowMultipleExecution = allowMultipleExecution;
                this.attributes = attributes;
                this.reflectedType = reflectedType;
            }
            IDelegateCommand GetCommand(object component) {
                return ((ViewModelBase)component).GetCommand(method, canExecuteMethod, useCommandManager, hasParameter, allowMultipleExecution);
            }
            public override bool CanResetValue(object component) { return false; }
            public override Type ComponentType { get { return method.DeclaringType; } }
            public override object GetValue(object component) { return GetCommand(component); }
            public override bool IsReadOnly { get { return true; } }
            public override Type PropertyType { get { return typeof(ICommand); } }
            public override void ResetValue(object component) { throw new NotSupportedException(); }
            public override void SetValue(object component, object value) { throw new NotSupportedException(); }
            public override bool ShouldSerializeValue(object component) { return false; }
        }
        #endregion

        #region ICustomTypeDescriptor
        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return TypeDescriptor.GetAttributes(this, true);
        }
        string ICustomTypeDescriptor.GetClassName() {
            return TypeDescriptor.GetClassName(this, true);
        }
        string ICustomTypeDescriptor.GetComponentName() {
            return TypeDescriptor.GetComponentName(this, true);
        }
        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return TypeDescriptor.GetConverter(this, true);
        }
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return TypeDescriptor.GetEvents(this, true);
        }
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            return TypeDescriptor.GetProperties(this, attributes, true);
        }
        PropertyDescriptorCollection properties;
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return properties ??
                (properties = new PropertyDescriptorCollection(TypeDescriptor.GetProperties(this, true).Cast<PropertyDescriptor>().Concat(commandProperties.Values).ToArray()));
        }
        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return this;
        }
        #endregion
#endregion CommandAttributeSupport
    }
    [Serializable]
    public class CommandAttributeException : Exception {
        public CommandAttributeException() { }
        public CommandAttributeException(string message)
            : base(message) {

        }
    }
}