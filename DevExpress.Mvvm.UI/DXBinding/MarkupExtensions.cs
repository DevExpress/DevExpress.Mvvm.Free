using DevExpress.DXBinding.Native;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace DevExpress.Xpf.DXBinding {
    public abstract class DXMarkupExtensionBase : MarkupExtension {
        internal static bool? IsInDesingModeCore = null;
        protected static bool IsInDesignMode() {
            if(IsInDesingModeCore != null) return IsInDesingModeCore.Value;
            DependencyPropertyDescriptor property =
                DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement));
            return (bool)property.Metadata.DefaultValue;
        }

        protected static string GetTargetPropertyName(object targetProperty) {
            return (targetProperty as DependencyProperty).With(x => x.Name)
                ?? (targetProperty as PropertyInfo).With(x => x.Name)
                ?? (targetProperty as MethodBase).With(x => x.Name)
                ?? (targetProperty as EventInfo).With(x => x.Name);
        }
        protected static Type GetTargetPropertyType(object targetProperty) {
            return (targetProperty as DependencyProperty).With(x => x.PropertyType)
                ?? (targetProperty as PropertyInfo).With(x => x.PropertyType)
                ?? (targetProperty as EventInfo).With(x => x.EventHandlerType);
        }
        protected static bool IsInSetter(IProvideValueTarget targetProvider) {
            if(targetProvider == null) return false;
            return targetProvider.TargetObject is Setter;
        }
        protected static bool IsInTemplate(IProvideValueTarget targetProvider) {
            if(targetProvider == null) return false;
            return targetProvider.TargetObject.GetType().FullName == "System.Windows.SharedDp";
        }
        protected static bool IsInBinding(IProvideValueTarget targetProvider) {
            if(targetProvider == null) return false;
            return targetProvider.TargetObject is BindingBase;
        }

        IServiceProvider serviceProvider;
        IProvideValueTarget targetProvider;
        IXamlTypeResolver xamlTypeResolver;
        protected IServiceProvider ServiceProvider { get { return serviceProvider; } }
        protected IProvideValueTarget TargetProvider {
            get {
                if(targetProvider != null) return targetProvider;
                if(serviceProvider == null) return null;
                return targetProvider = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            }
        }
        protected IXamlTypeResolver XamlTypeResolver {
            get {
                if(xamlTypeResolver != null) return xamlTypeResolver;
                if(serviceProvider == null) return null;
                return xamlTypeResolver = (IXamlTypeResolver)serviceProvider.GetService(typeof(IXamlTypeResolver));
            }
        }

        public sealed override object ProvideValue(IServiceProvider serviceProvider) {
            try {
                this.serviceProvider = serviceProvider;
                return ProvideValueCore();
            } finally {
                this.serviceProvider = null;
                this.targetProvider = null;
                this.xamlTypeResolver = null;
            }
        }
        protected abstract object ProvideValueCore();
    }
    public abstract class DXBindingBase : DXMarkupExtensionBase {
        internal static Binding CreateBinding(IServiceProvider serviceProvider, Operand operand, BindingMode mode, bool isRootBinding) {
            var path = operand != null && !string.IsNullOrEmpty(operand.Path) ? operand.Path : ".";
            Binding res = new Binding(path) { Path = new PropertyPath(path), Mode = mode };
            if(operand == null) return res;
            switch(operand.Source) {
                case Operand.RelativeSource.Context:
                    break;
                case Operand.RelativeSource.Ancestor:
                    res.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) {
                        AncestorType = operand.AncestorType,
                        AncestorLevel = operand.AncestorLevel,
                    };
                    break;
                case Operand.RelativeSource.Self:
                    res.RelativeSource = new RelativeSource(RelativeSourceMode.Self);
                    break;
                case Operand.RelativeSource.Parent:
                    res.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                    break;
                case Operand.RelativeSource.Element:
                    res.ElementName = operand.ElementName;
                    break;
                case Operand.RelativeSource.Resource:
                    res.Source = new StaticResourceExtension(operand.ResourceName).ProvideValue(serviceProvider);
                    break;
                case Operand.RelativeSource.Reference:
                    res.Source = new Reference(operand.ReferenceName).ProvideValue(serviceProvider);
                    break;
                default: throw new InvalidOperationException();
            }
            return res;
        }

        internal readonly IErrorHandler ErrorHandler;
        internal readonly ITypeResolver TypeResolver;
        protected internal string TargetPropertyName { get; private set; }
        protected internal string TargetObjectName { get; private set; }
        protected internal Type TargetPropertyType { get; private set; }
        bool IsInitialized { get; set; }

        public DXBindingBase() {
            ErrorHandler = new IErrorHandlerImpl(this);
            TypeResolver = new ITypeResolverImpl(this);
        }
        protected override object ProvideValueCore() {
            CheckTargetProvider();
            if(XamlTypeResolver != null && !((ITypeResolverImpl)TypeResolver).IsInitialized) {
                ((ITypeResolverImpl)TypeResolver).SetXamlTypeResolver(XamlTypeResolver);
                IsInitialized = true;
                Init();
            }
            if(IsInTemplate(TargetProvider)) return this;
            InitTargetProperties();
            CheckTargetObject();
            if(!IsInitialized) Init();
            var res = GetProvidedValue();
            ((ITypeResolverImpl)TypeResolver).ClearXamlTypeResolver();
            return res;
        }
        protected abstract void Init();
        protected abstract object GetProvidedValue();
        void InitTargetProperties() {
            if(TargetProvider.TargetObject is Setter) {
                Setter setter = (Setter)TargetProvider.TargetObject;
                if(setter.Property != null) {
                    TargetPropertyName = GetTargetPropertyName(setter.Property);
                    TargetPropertyType = GetTargetPropertyType(setter.Property);
                }
            } else {
                TargetObjectName = TargetProvider.TargetObject.GetType().Name;
                TargetPropertyName = GetTargetPropertyName(TargetProvider.TargetProperty);
                TargetPropertyType = GetTargetPropertyType(TargetProvider.TargetProperty);
            }
        }

        protected abstract void Error_Report(string msg);
        protected abstract void Error_Throw(string msg, Exception innerException);
        protected virtual void CheckTargetProvider() {
            if(TargetProvider == null) ErrorHandler.Throw(ErrorHelper.Err001(this), null);
            if(TargetProvider.TargetObject == null || TargetProvider.TargetProperty == null)
                ErrorHandler.Throw(ErrorHelper.Err002(this), null);
        }
        protected virtual void CheckTargetObject() {
            if(IsInSetter(TargetProvider)) return;
            if(TargetPropertyType == typeof(BindingBase)) return;
            if(!(TargetProvider.TargetObject is DependencyObject) || !(TargetProvider.TargetProperty is DependencyProperty))
                ErrorHandler.Throw(ErrorHelper.Err002(this), null);
        }

        class IErrorHandlerImpl : ErrorHandlerBase {
            readonly DXBindingBase owner;
            public IErrorHandlerImpl(DXBindingBase owner) {
                this.owner = owner;
            }
            protected override void ReportCore(string msg) {
                if(msg == null || IsInDesignMode()) return;
                owner.Error_Report(msg);
            }
            protected override void ThrowCore(string msg, Exception innerException) {
                owner.Error_Throw(msg, innerException);
            }
        }
        class ITypeResolverImpl : ITypeResolver {
            readonly DXBindingBase owner;
            public ITypeResolverImpl(DXBindingBase owner) {
                this.owner = owner;
            }
            public bool IsInitialized { get { return this.xamlTypeResolver != null; } }
            public void SetXamlTypeResolver(IXamlTypeResolver xamlTypeResolver) {
                this.xamlTypeResolver = xamlTypeResolver;
            }
            public void ClearXamlTypeResolver() {
                this.xamlTypeResolver = null;
            }

            IXamlTypeResolver xamlTypeResolver;
            Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
            Type ITypeResolver.ResolveType(string type) {
                if(typeCache.ContainsKey(type)) return typeCache[type];
                if(IsInDesignMode()) {
                    owner.ErrorHandler.SetError();
                    return null;
                }
                try {
                    var res = xamlTypeResolver.Resolve(type);
                    typeCache.Add(type, res);
                    return res;
                } catch(Exception e) {
                    owner.ErrorHandler.Throw(ErrorHelper.Err004(type), e);
                    return null;
                }
            }
        }
        internal class DXBindingConverterBase : IMultiValueConverter, IValueConverter {
            protected readonly IErrorHandler errorHandler;
            public DXBindingConverterBase(DXBindingBase owner) {
                this.errorHandler = owner.ErrorHandler;
            }

            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                return ((IMultiValueConverter)this).Convert(new object[] { value }, targetType, parameter, culture);
            }
            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                return ((IMultiValueConverter)this).ConvertBack(value, new Type[] { targetType }, parameter, culture).First();
            }
            object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
                if(!CanConvert(values)) return Binding.DoNothing;
                object res = Convert(values, targetType);
                res = CoerceAfterConvert(res, targetType, parameter, culture);
                return ConvertToTargetType(res, targetType);
            }
            object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
                value = CoerceBeforeConvertBack(value, targetTypes, parameter, culture);
                object[] res = ConvertBack(value, targetTypes);
                for(int i = 0; i < res.Count(); i++)
                    res[i] = ConvertToTargetType(res[i], targetTypes[i]);
                return res.ToArray();
            }
            protected virtual bool CanConvert(object[] values) {
                return !ValuesContainUnsetValue(values);
            }
            protected virtual object Convert(object[] values, Type targetType) {
                return values;
            }
            protected virtual object[] ConvertBack(object value, Type[] targetTypes) {
                throw new NotImplementedException();
            }
            protected virtual object CoerceAfterConvert(object value, Type targetType, object parameter, CultureInfo culture) {
                return value;
            }
            protected virtual object CoerceBeforeConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
                return value;
            }

            public static bool ValuesContainUnsetValue(object[] values) {
                foreach(object v in values) {
                    if(v == DependencyProperty.UnsetValue) return true;
                }
                return false;
            }
            static object ConvertToTargetType(object value, Type targetType) {
                if(value != null && targetType == typeof(string) && !(value is string))
                    value = value.ToString();
                return value;
            }
        }
    }

    public sealed class DXBindingExtension : DXBindingBase {
        internal static UpdateSourceTrigger DefaultUpdateSourceTrigger = UpdateSourceTrigger.Default;
        public string BindingGroupName { get; set; }
        public object TargetNullValue { get; set; }
        public bool NotifyOnSourceUpdated { get; set; }
        public bool NotifyOnTargetUpdated { get; set; }
        public bool NotifyOnValidationError { get; set; }
        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }
        public bool ValidatesOnDataErrors { get; set; }
        public bool ValidatesOnExceptions { get; set; }

        public string Expr { get; set; }
        public string BackExpr { get; set; }
        public BindingMode Mode { get; set; }
        bool isFallbackValueSet = false;
        object fallbackValue = DependencyProperty.UnsetValue;
        public object FallbackValue {
            get { return fallbackValue; }
            set { fallbackValue = value; isFallbackValueSet = true; }
        }
        public IValueConverter Converter { get; set; }
        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
        public CultureInfo ConverterCulture { get; set; }
        public object ConverterParameter { get; set; }

        BindingMode ActualMode { get; set; }
        BindingTreeInfo TreeInfo { get; set; }
        BindingCalculator Calculator { get; set; }

        public DXBindingExtension() : this(string.Empty) { }
        public DXBindingExtension(string expr) {
            Expr = expr;
            UpdateSourceTrigger = DefaultUpdateSourceTrigger;
            Mode = BindingMode.Default;
            BindingGroupName = string.Empty;
        }
        protected override void Error_Report(string msg) {
            DXBindingException.Report(this, msg);
        }
        protected override void Error_Throw(string msg, Exception innerException) {
            DXBindingException.Throw(this, msg, innerException);
        }
        protected override object ProvideValueCore() {
            var targetProperty = TargetProvider.With(x => x.TargetProperty as DependencyProperty);
            var targetObject = TargetProvider.With(x => x.TargetObject as DependencyObject);
            PropertyMetadata metadata = targetProperty != null && targetObject != null ?
                targetProperty.GetMetadata(targetObject) : null;
            if(Mode != BindingMode.Default || metadata == null) {
                ActualMode = Mode;
            } else {
                if(!(metadata is FrameworkPropertyMetadata))
                    ActualMode = BindingMode.OneWay;
                else if(((FrameworkPropertyMetadata)metadata).BindsTwoWayByDefault)
                    ActualMode = BindingMode.TwoWay;
                else ActualMode = BindingMode.OneWay;
            }
            return base.ProvideValueCore();
        }
        protected override void Init() {
            TreeInfo = new BindingTreeInfo(Expr, BackExpr, ErrorHandler);
            Calculator = new BindingCalculator(TreeInfo, FallbackValue);
            Calculator.Init(TypeResolver);
        }
        protected override object GetProvidedValue() {
            if(Mode == BindingMode.Default && TreeInfo.IsEmptyBackExpr()
                && Calculator.Operands.Count() == 0)
                ActualMode = BindingMode.OneWay;
            if((ActualMode == BindingMode.TwoWay || ActualMode == BindingMode.OneWayToSource)
               && TreeInfo.IsEmptyBackExpr()) {
                if(TreeInfo.IsSimpleExpr())
                    Calculator.Operands.FirstOrDefault().Do(x => x.SetMode(true));
                else ErrorHandler.Throw(ErrorHelper.Err101_TwoWay(), null);
            }
            if(IsInSetter(TargetProvider) || TargetPropertyType == typeof(BindingBase))
                return CreateBinding();
            return CreateBinding().ProvideValue(ServiceProvider);
        }
        BindingBase CreateBinding() {
            if(Calculator.Operands.Count() == 0) {
                var binding = CreateBinding(ServiceProvider, null, ActualMode, true);
                SetBindingProperties(binding, true);
                binding.Source = Calculator.Resolve(null);
                binding.Converter = Converter;
                binding.ConverterParameter = ConverterParameter;
                binding.ConverterCulture = ConverterCulture;
                return binding;
            }
            if(Calculator.Operands.Count() == 1) {
                var binding = CreateBinding(ServiceProvider, Calculator.Operands.First(), ActualMode, true);
                SetBindingProperties(binding, true);
                binding.Converter = CreateConverter();
                binding.ConverterParameter = ConverterParameter;
                binding.ConverterCulture = ConverterCulture;
                return binding;
            }
            if(Calculator.Operands.Count() > 1) {
                var binding = new MultiBinding() { Mode = ActualMode };
                SetBindingProperties(binding, true);
                binding.Converter = CreateConverter();
                binding.ConverterParameter = ConverterParameter;
                binding.ConverterCulture = ConverterCulture;
                foreach(var op in Calculator.Operands) {
                    BindingMode mode = ActualMode == BindingMode.OneTime ? BindingMode.OneTime : BindingMode.OneWay;
                    if(op.IsTwoWay) {
                        if(ActualMode == BindingMode.Default) mode = BindingMode.Default;
                        mode = ActualMode == BindingMode.OneWayToSource ? BindingMode.OneWayToSource : BindingMode.TwoWay;
                    }
                    var subBinding = CreateBinding(ServiceProvider, op, mode, false);
                    SetBindingProperties(subBinding, false);
                    binding.Bindings.Add(subBinding);
                }
                return binding;
            }
            throw new NotImplementedException();
        }
        void SetBindingProperties(BindingBase binding, bool isRootBinding) {
            if(isRootBinding) {
                if(isFallbackValueSet) binding.FallbackValue = FallbackValue;
                if(!string.IsNullOrEmpty(BindingGroupName)) binding.BindingGroupName = BindingGroupName;
                if(TargetNullValue != null) binding.TargetNullValue = TargetNullValue;
            }
            if(binding is Binding) {
                var b = (Binding)binding;
                b.NotifyOnSourceUpdated = NotifyOnSourceUpdated;
                b.NotifyOnTargetUpdated = NotifyOnTargetUpdated;
                b.NotifyOnValidationError = NotifyOnValidationError;
                b.UpdateSourceTrigger = UpdateSourceTrigger;
                b.ValidatesOnDataErrors = ValidatesOnDataErrors;
                b.ValidatesOnExceptions = ValidatesOnExceptions;
            } else {
                var b = (MultiBinding)binding;
                b.NotifyOnSourceUpdated = NotifyOnSourceUpdated;
                b.NotifyOnTargetUpdated = NotifyOnTargetUpdated;
                b.NotifyOnValidationError = NotifyOnValidationError;
                b.UpdateSourceTrigger = UpdateSourceTrigger;
                b.ValidatesOnDataErrors = ValidatesOnDataErrors;
                b.ValidatesOnExceptions = ValidatesOnExceptions;
            }
        }
        DXBindingConverter CreateConverter() {
            return new DXBindingConverter(this);
        }

        class DXBindingConverter : DXBindingConverterBase {
            readonly BindingTreeInfo treeInfo;
            readonly BindingCalculator calculator;
            readonly IValueConverter externalConverter;
            Type backConversionType;
            bool isBackConvesionInitialized = false;
            public DXBindingConverter(DXBindingExtension owner) : base(owner) {
                this.treeInfo = owner.TreeInfo;
                this.calculator = owner.Calculator;
                this.backConversionType = owner.TargetPropertyType;
                this.externalConverter = owner.Converter;
            }
            protected override object Convert(object[] values, Type targetType) {
                if(backConversionType == null)
                    backConversionType = targetType;
                errorHandler.ClearError();
                return calculator.Resolve(values);
            }
            protected override object[] ConvertBack(object value, Type[] targetTypes) {
                if(treeInfo.IsEmptyBackExpr() && !treeInfo.IsSimpleExpr())
                    errorHandler.Throw(ErrorHelper.Err101_TwoWay(), null);
                if(!isBackConvesionInitialized) {
                    Type valueType = value.Return(x => x.GetType(), () => null);
                    var backExprType = valueType ?? backConversionType;
                    if(backExprType == null)
                        errorHandler.Throw(ErrorHelper.Err104(), null);
                    calculator.InitBack(valueType ?? backConversionType);
                    isBackConvesionInitialized = true;
                }
                List<object> res = new List<object>();
                foreach(var op in calculator.Operands) {
                    if(!op.IsTwoWay || op.BackConverter == null) res.Add(value);
                    else res.Add(op.BackConverter(new object[] { value }));
                }
                return res.ToArray();
            }
            protected override object CoerceAfterConvert(object value, Type targetType, object parameter, CultureInfo culture) {
                if(externalConverter != null)
                    return externalConverter.Convert(value, targetType, parameter, culture);
                if(value == DependencyProperty.UnsetValue && targetType == typeof(string))
                    value = null;
                else value = ObjectToObjectConverter.Coerce(value, targetType, true);
                return base.CoerceAfterConvert(value, targetType, parameter, culture);
            }
            protected override object CoerceBeforeConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
                if(externalConverter != null) {
                    var t = targetTypes != null && targetTypes.Count() == 1 ? targetTypes[0] : backConversionType;
                    return externalConverter.ConvertBack(value, t, parameter, culture);
                }
                return base.CoerceBeforeConvertBack(value, targetTypes, parameter, culture);
            }
        }
    }
    public sealed class DXCommandExtension : DXBindingBase {
        public string Execute { get; set; }
        public string CanExecute { get; set; }
        public bool FallbackCanExecute { get; set; }
        CommandTreeInfo TreeInfo { get; set; }
        CommandCalculator Calculator { get; set; }

        public DXCommandExtension() : this(string.Empty) { }
        public DXCommandExtension(string execute) {
            Execute = execute;
        }
        protected override void Error_Report(string msg) {
            DXCommandException.Report(this, msg);
        }
        protected override void Error_Throw(string msg, Exception innerException) {
            DXCommandException.Throw(this, msg, innerException);
        }
        protected override void Init() {
            TreeInfo = new CommandTreeInfo(Execute, CanExecute, ErrorHandler);
            Calculator = new CommandCalculator(TreeInfo, FallbackCanExecute);
            Calculator.Init(TypeResolver);
        }
        protected override object GetProvidedValue() {
            if(IsInSetter(TargetProvider)) return CreateBinding();
            return CreateBinding().ProvideValue(ServiceProvider);
        }
        BindingBase CreateBinding() {
            if(Calculator.Operands.Count() == 0) {
                var binding = CreateBinding(ServiceProvider, null, BindingMode.OneWay, true);
                SetBindingProperties(binding, true);
                binding.Source = null;
                binding.Converter = CreateConverter(true);
                return binding;
            }
            if(Calculator.Operands.Count() == 1) {
                var binding = CreateBinding(ServiceProvider, Calculator.Operands.First(), BindingMode.OneWay, true);
                SetBindingProperties(binding, true);
                binding.Converter = CreateConverter(false);
                return binding;
            }
            if(Calculator.Operands.Count() > 1) {
                var binding = new MultiBinding() { Mode = BindingMode.OneWay };
                SetBindingProperties(binding, true);
                binding.Converter = CreateConverter(false);
                foreach(var op in Calculator.Operands) {
                    var subBinding = CreateBinding(ServiceProvider, op, BindingMode.OneWay, false);
                    SetBindingProperties(subBinding, false);
                    binding.Bindings.Add(subBinding);
                }
                return binding;
            }
            throw new NotImplementedException();
        }
        void SetBindingProperties(BindingBase binding, bool isRootBinding) {
            if(binding is Binding) {
                var b = (Binding)binding;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            } else {
                var b = (MultiBinding)binding;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }
        }
        DXCommandConverter CreateConverter(bool isEmpty) {
            return new DXCommandConverter(this, isEmpty);
        }

        class DXCommandConverter : DXBindingConverterBase {
            readonly CommandCalculator calculator;
            readonly bool fallbackCanExecute;
            readonly bool isEmpty;
            public DXCommandConverter(DXCommandExtension owner, bool isEmpty) : base(owner) {
                this.calculator = owner.Calculator;
                this.fallbackCanExecute = owner.FallbackCanExecute;
                this.isEmpty = isEmpty;
            }
            protected override object Convert(object[] values, Type targetType) {
                return new Command(errorHandler, calculator, fallbackCanExecute, isEmpty, values);
            }
            protected override bool CanConvert(object[] values) {
                return true;
            }
        }
        class Command : ICommand {
            readonly WeakReference errorHandler;
            readonly WeakReference calculator;
            readonly WeakReference[] values;
            readonly bool fallbackCanExecute;
            readonly bool isEmpty;
            IErrorHandler ErrorHandler { get { return (IErrorHandler)errorHandler.Target; } }
            CommandCalculator Calculator { get { return (CommandCalculator)calculator.Target; } }
            object[] Values { get { return isEmpty ? null : values.Select(x => x.Target).ToArray(); } }
            bool IsAlive {
                get {
                    bool res = errorHandler.IsAlive && calculator.IsAlive;
                    if(isEmpty) return res;
                    return res && !values.Any(x => !x.IsAlive);
                }
            }
            public Command(IErrorHandler errorHandler, CommandCalculator calculator, bool fallbackCanExecute, bool isEmpty, object[] values) {
                this.errorHandler = new WeakReference(errorHandler);
                this.calculator = new WeakReference(calculator);
                this.fallbackCanExecute = fallbackCanExecute;
                this.isEmpty = isEmpty;
                this.values = values.Select(x => new WeakReference(x)).ToArray();
            }
            void ICommand.Execute(object parameter) {
                if(!IsAlive) return;
                ErrorHandler.ClearError();
                if(!isEmpty && DXBindingConverterBase.ValuesContainUnsetValue(Values)) return;
                Calculator.Execute(Values, parameter);
            }
            bool ICommand.CanExecute(object parameter) {
                if(!IsAlive) return fallbackCanExecute;
                ErrorHandler.ClearError();
                if(!isEmpty && DXBindingConverterBase.ValuesContainUnsetValue(Values)) return fallbackCanExecute;
                return Calculator.CanExecute(Values, parameter);
            }
            event EventHandler ICommand.CanExecuteChanged {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }
    }
    public sealed class DXEventExtension : DXBindingBase {
        public string Handler { get; set; }
        EventTreeInfo TreeInfo { get; set; }
        EventCalculator Calculator { get; set; }

        public DXEventExtension() : this(string.Empty) { }
        public DXEventExtension(string expr) {
            Handler = expr;
        }
        protected override void Error_Report(string msg) {
            DXEventException.Report(this, msg);
        }
        protected override void Error_Throw(string msg, Exception innerException) {
            DXEventException.Throw(this, msg, innerException);
        }
        protected override void CheckTargetProvider() {
            base.CheckTargetProvider();
            if(IsInSetter(TargetProvider))
                ErrorHandler.Throw(ErrorHelper.Err003(this), null);
        }
        protected override void CheckTargetObject() {
            if(!(TargetProvider.TargetObject is DependencyObject))
                ErrorHandler.Throw(ErrorHelper.Err002(this), null);
            if(TargetProvider.TargetProperty is EventInfo) return;
            ErrorHandler.Throw(ErrorHelper.Err002(this), null);
        }
        protected override void Init() {
            TreeInfo = new EventTreeInfo(Handler, ErrorHandler);
            Calculator = new EventCalculator(TreeInfo);
            Calculator.Init(TypeResolver);
        }
        protected override object GetProvidedValue() {
            var eventBinder = new EventBinder(this, GetEventHandlerType(), CreateBinding());
            return eventBinder.GetEventHandler();
        }
        Type GetEventHandlerType() {
            if(TargetProvider.TargetProperty is EventInfo)
                return ((EventInfo)TargetProvider.TargetProperty).EventHandlerType;
            MethodInfo m = (MethodInfo)TargetProvider.TargetProperty;
            return m.GetParameters().ElementAt(1).ParameterType;
        }
        BindingBase CreateBinding() {
            if(Calculator.Operands.Count() == 0) {
                var binding = CreateBinding(ServiceProvider, null, BindingMode.OneWay, true);
                SetBindingProperties(binding, true);
                binding.Source = null;
                binding.Converter = new EventConverter(this, true);
                return binding;
            }
            if(Calculator.Operands.Count() == 1) {
                var binding = CreateBinding(ServiceProvider, Calculator.Operands.First(), BindingMode.OneWay, true);
                SetBindingProperties(binding, true);
                binding.Converter = new EventConverter(this, false);
                return binding;
            }
            if(Calculator.Operands.Count() > 1) {
                var binding = new MultiBinding() { Mode = BindingMode.OneWay };
                foreach(var op in Calculator.Operands) {
                    var subBinding = CreateBinding(ServiceProvider, op, BindingMode.OneWay, false);
                    SetBindingProperties(subBinding, false);
                    binding.Bindings.Add(subBinding);
                }
                binding.Converter = new EventConverter(this, false);
                return binding;
            }
            throw new NotImplementedException();
        }
        void SetBindingProperties(BindingBase binding, bool isRootBinding) {
            if(binding is Binding) {
                var b = (Binding)binding;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            } else {
                var b = (MultiBinding)binding;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }
        }

        class EventBinder {
            readonly IErrorHandler errorHandler;
            readonly EventCalculator calculator;
            readonly WeakReference targetObject;
            readonly Type eventHandlerType;
            DependencyProperty dataProperty;
            DependencyObject TargetObject { get { return (DependencyObject)targetObject.Target; } }
            bool IsAlive { get { return targetObject.IsAlive; } }
            public EventBinder(DXEventExtension owner, Type eventHandlerType, BindingBase binding) {
                this.errorHandler = owner.ErrorHandler;
                this.calculator = owner.Calculator;
                var target = (DependencyObject)owner.TargetProvider.TargetObject;
                this.targetObject = new WeakReference(target);
                this.eventHandlerType = eventHandlerType;
                dataProperty = DependencyProperty.RegisterAttached(
                    owner.TargetPropertyName + Guid.NewGuid().ToString(), typeof(object), typeof(EventBinder), null);
                BindingOperations.SetBinding(target, dataProperty, binding);
            }
            public Delegate GetEventHandler() {
                var eventSubscriber = new DevExpress.Mvvm.UI.Interactivity.Internal.EventTriggerEventSubscriber(OnEvent);
                return eventSubscriber.CreateEventHandler(eventHandlerType);
            }
            object[] GetBoundEventData() {
                if(!IsAlive) return null;
                var res = (IEnumerable<object>)TargetObject.GetValue(dataProperty);
                return res == null ? null : res.ToArray();
            }
            void OnEvent(object sender, object eventArgs) {
                var data = GetBoundEventData();
                errorHandler.ClearError();
                calculator.Event(data, sender, eventArgs);
            }
        }
        class EventConverter : DXBindingConverterBase {
            readonly bool isEmpty;
            public EventConverter(DXEventExtension owner, bool isEmpty) : base(owner) {
                this.isEmpty = isEmpty;
            }
            protected override object Convert(object[] values, Type targetType) {
                return isEmpty ? null : new List<object>(values ?? new object[] { });
            }
        }
    }

    public abstract class DXBindingExceptionBase<TSelf, TOwner> : Exception
        where TSelf : DXBindingExceptionBase<TSelf, TOwner>
        where TOwner : DXBindingBase {
        protected readonly TOwner owner;
        public string TargetPropertyName { get { return owner.TargetPropertyName; } }
        public string TargetObjectType { get { return owner.TargetObjectName; } }
        protected DXBindingExceptionBase(TOwner owner, string message, Exception innerException)
            : base(message, innerException) {
            this.owner = owner;
        }
        protected abstract string Report(string message);
        public static void Throw(TOwner owner, string message, Exception innerException) {
            throw (TSelf)Activator.CreateInstance(typeof(TSelf), owner, message, innerException);
        }
        public static void Report(TOwner owner, string message) {
            var ex = (TSelf)Activator.CreateInstance(typeof(TSelf), owner, message, null);
            PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 40, ex.Report(message));
        }
    }
    public sealed class DXBindingException : DXBindingExceptionBase<DXBindingException, DXBindingExtension> {
        public string Expr { get { return owner.Expr; } }
        public string BackExpr { get { return owner.BackExpr; } }
        public DXBindingException(DXBindingExtension owner, string message, Exception innerException)
            : base(owner, message, innerException) { }
        protected override string Report(string message) {
            return ErrorHelper.ReportBindingError(message, Expr, BackExpr);
        }
    }
    public sealed class DXCommandException : DXBindingExceptionBase<DXCommandException, DXCommandExtension> {
        public string Execute { get { return owner.Execute; } }
        public string CanExecute { get { return owner.CanExecute; } }
        public DXCommandException(DXCommandExtension owner, string message, Exception innerException)
            : base(owner, message, innerException) { }
        protected override string Report(string message) {
            return ErrorHelper.ReportCommandError(message, Execute, CanExecute);
        }
    }
    public sealed class DXEventException : DXBindingExceptionBase<DXEventException, DXEventExtension> {
        public string Handler { get { return owner.Handler; } }
        public DXEventException(DXEventExtension owner, string message, Exception innerException)
            : base(owner, message, innerException) { }
        protected override string Report(string message) {
            return ErrorHelper.ReportEventError(message, Handler);
        }
    }
}