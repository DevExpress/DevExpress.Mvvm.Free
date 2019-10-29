using DevExpress.DXBinding.Native;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core.Native;
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
using System.Xaml;

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
        IXamlSchemaContextProvider xamlSchemaContextProvider;
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
        protected IXamlSchemaContextProvider XamlSchemaContextProvider {
            get {
                if (xamlSchemaContextProvider != null) return xamlSchemaContextProvider;
                if (serviceProvider == null) return null;
                return xamlSchemaContextProvider = (IXamlSchemaContextProvider)serviceProvider.GetService(typeof(IXamlSchemaContextProvider));
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

    public enum DXBindingResolvingMode {
        LegacyStaticTyping,
        DynamicTyping
    }
    public abstract class DXBindingBase : DXMarkupExtensionBase {
        protected Binding CreateBinding(Operand operand, BindingMode mode) {

            var path = operand != null && !string.IsNullOrEmpty(operand.Path) ? operand.Path : ".";
            Binding res = new Binding(path) { Path = new PropertyPath(path), Mode = mode };
            if (operand == null) return res;
            switch (operand.Source) {
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
                    res.Source = GetStaticResource(operand.ResourceName);
                    break;
                case Operand.RelativeSource.Reference:
                    res.Source = new Reference(operand.ReferenceName).ProvideValue(ServiceProvider);
                    break;
                default: throw new InvalidOperationException();
            }
            return res;
        }
        object GetStaticResource(string resourceName) {
            object res;
            if (staticResources != null)
                staticResources.TryGetValue(resourceName, out res);
            else res = new StaticResourceExtension(resourceName).ProvideValue(ServiceProvider);
            return res;
        }

        Dictionary<string, object> staticResources;
        internal readonly IErrorHandler ErrorHandler;
        internal readonly ITypeResolver TypeResolver;
        protected internal string TargetPropertyName { get; private set; }
        protected internal string TargetObjectName { get; private set; }
        protected internal Type TargetPropertyType { get; private set; }
        bool IsInitialized { get; set; }

        public static DXBindingResolvingMode DefaultResolvingMode { get; set; }
        public DXBindingResolvingMode? ResolvingMode { get; set; }
        protected DXBindingResolvingMode ActualResolvingMode {
            get { return ResolvingMode ?? DefaultResolvingMode; }
        }

        static DXBindingBase() {
            DefaultResolvingMode = DXBindingResolvingMode.DynamicTyping;
        }
        public bool CatchExceptions {
            get { return ErrorHandler.CatchAllExceptions; }
            set { ErrorHandler.CatchAllExceptions = value; }
        }
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
            if (IsInTemplate(TargetProvider)) {
                CollectStaticResources();
                return this;
            }
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
        void CollectStaticResources() {
            if (XamlSchemaContextProvider == null) return;
            var operands = GetOperands();
            if (operands == null) return;
            staticResources = operands
                .Where(x => x.Source == Operand.RelativeSource.Resource)
                .Select(x => x.ResourceName)
                .Distinct()
                .ToDictionary(x => x, x => new StaticResourceExtension(x).ProvideValue(ServiceProvider));
        }

        protected abstract IEnumerable<Operand> GetOperands();
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
            if(TargetProvider.TargetObject is Style && TargetProvider.TargetObject is ITypedStyle) return;
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
                if (value == Binding.DoNothing)
                    return value;
                if(value != null && targetType == typeof(string) && !(value is string))
                    value = value.ToString();
                return value;
            }
        }
    }

    public sealed class DXBindingExtension : DXBindingBase {
        public string BindingGroupName { get; set; }
        public object TargetNullValue { get; set; }
        public bool NotifyOnSourceUpdated { get; set; }
        public bool NotifyOnTargetUpdated { get; set; }
        public bool NotifyOnValidationError { get; set; }
        internal static UpdateSourceTrigger? DefaultUpdateSourceTrigger = null;
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
        public bool AllowUnsetValue { get; set; }

        BindingMode ActualMode { get; set; }
        BindingTreeInfo TreeInfo { get; set; }
        IBindingCalculator Calculator { get; set; }

        public DXBindingExtension() : this(string.Empty) { }
        public DXBindingExtension(string expr) {
            Expr = expr;
            UpdateSourceTrigger = DefaultUpdateSourceTrigger ?? UpdateSourceTrigger.Default;
            Mode = BindingMode.Default;
            BindingGroupName = string.Empty;
            AllowUnsetValue = false;
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
            if(ActualResolvingMode == DXBindingResolvingMode.LegacyStaticTyping)
                Calculator = new BindingCalculator(TreeInfo, FallbackValue);
            else Calculator = new BindingCalculatorDynamic(TreeInfo, FallbackValue);
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
        protected override IEnumerable<Operand> GetOperands() {
            return Calculator != null ? Calculator.Operands : null;
        }
        BindingBase CreateBinding() {
            if(Calculator.Operands.Count() == 0) {
                var binding = CreateBinding(null, ActualMode);
                SetBindingProperties(binding, true);
                binding.Source = Calculator.Resolve(null);
                binding.Converter = Converter;
                binding.ConverterParameter = ConverterParameter;
                binding.ConverterCulture = ConverterCulture;
                return binding;
            }
            if(Calculator.Operands.Count() == 1) {
                var binding = CreateBinding(Calculator.Operands.First(), ActualMode);
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
                    var subBinding = CreateBinding(op, mode);
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
        DXBindingConverterBase CreateConverter() {
            if(ActualResolvingMode == DXBindingResolvingMode.LegacyStaticTyping)
                return new DXBindingConverter(this, (BindingCalculator)Calculator);
            return new DXBindingConverterDynamic(this, (BindingCalculatorDynamic)Calculator, AllowUnsetValue);
        }

        class DXBindingConverter : DXBindingConverterBase {
            readonly BindingTreeInfo treeInfo;
            readonly BindingCalculator calculator;
            readonly IValueConverter externalConverter;
            Type backConversionType;
            bool isBackConversionInitialized = false;
            public DXBindingConverter(DXBindingExtension owner, BindingCalculator calculator) : base(owner) {
                this.treeInfo = owner.TreeInfo;
                this.calculator = calculator;
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
                if(!isBackConversionInitialized) {
                    Type valueType = value.Return(x => x.GetType(), () => null);
                    var backExprType = valueType ?? backConversionType;
                    if(backExprType == null)
                        errorHandler.Throw(ErrorHelper.Err104(), null);
                    calculator.InitBack(valueType ?? backConversionType);
                    isBackConversionInitialized = true;
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
        class DXBindingConverterDynamic : DXBindingConverterBase {
            readonly BindingTreeInfo treeInfo;
            readonly BindingCalculatorDynamic calculator;
            readonly IValueConverter externalConverter;
            readonly bool allowUnsetValue;
            public DXBindingConverterDynamic(DXBindingExtension owner, BindingCalculatorDynamic calculator, bool allowUnsetValue) : base(owner) {
                this.treeInfo = owner.TreeInfo;
                this.calculator = calculator;
                this.externalConverter = owner.Converter;
                this.allowUnsetValue = allowUnsetValue;
            }
            List<WeakReference> valueRefs;
            protected override object Convert(object[] values, Type targetType) {
                errorHandler.ClearError();
                this.valueRefs = values.Select(x => new WeakReference(x)).ToList();
                return calculator.Resolve(values);
            }
            protected override object[] ConvertBack(object value, Type[] targetTypes) {
                if (treeInfo.IsEmptyBackExpr() && !treeInfo.IsSimpleExpr())
                    errorHandler.Throw(ErrorHelper.Err101_TwoWay(), null);
                if (treeInfo.IsEmptyBackExpr())
                    return Enumerable.Range(0, calculator.Operands.Count()).Select(x => value).ToArray();
                var values = valueRefs.Select(x => x.Target).ToArray();
                var res = calculator.ResolveBack(values, value).ToArray();
                return res;
            }
            protected override object CoerceAfterConvert(object value, Type targetType, object parameter, CultureInfo culture) {
                if (externalConverter != null)
                    return externalConverter.Convert(value, targetType, parameter, culture);
                if (value == Binding.DoNothing)
                    return value;
                if (value == DependencyProperty.UnsetValue && targetType == typeof(string))
                    value = null;
                else value = ObjectToObjectConverter.Coerce(value, targetType, true);
                return base.CoerceAfterConvert(value, targetType, parameter, culture);
            }
            protected override object CoerceBeforeConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
                if (externalConverter != null) {
                    var t = targetTypes != null && targetTypes.Count() == 1 ? targetTypes[0] : null;
                    return externalConverter.ConvertBack(value, t, parameter, culture);
                }
                return base.CoerceBeforeConvertBack(value, targetTypes, parameter, culture);
            }
            protected override bool CanConvert(object[] values) {
                return allowUnsetValue || !ValuesContainUnsetValue(values);
            }
        }
    }
    public sealed class DXCommandExtension : DXBindingBase {
        public string Execute { get; set; }
        public string CanExecute { get; set; }
        public bool FallbackCanExecute { get; set; }
        CommandTreeInfo TreeInfo { get; set; }
        ICommandCalculator Calculator { get; set; }

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
            if (ActualResolvingMode == DXBindingResolvingMode.LegacyStaticTyping)
                Calculator = new CommandCalculator(TreeInfo, FallbackCanExecute);
            else Calculator = new CommandCalculatorDynamic(TreeInfo, FallbackCanExecute);
            Calculator.Init(TypeResolver);
        }
        protected override object GetProvidedValue() {
            if(IsInSetter(TargetProvider)) return CreateBinding();
            return CreateBinding().ProvideValue(ServiceProvider);
        }
        protected override IEnumerable<Operand> GetOperands() {
            return Calculator != null ? Calculator.Operands : null;
        }
        BindingBase CreateBinding() {
            if(Calculator.Operands.Count() == 0) {
                var binding = CreateBinding(null, BindingMode.OneWay);
                SetBindingProperties(binding, true);
                binding.Source = null;
                binding.Converter = CreateConverter(true);
                return binding;
            }
            if(Calculator.Operands.Count() == 1) {
                var binding = CreateBinding(Calculator.Operands.First(), BindingMode.OneWay);
                SetBindingProperties(binding, true);
                binding.Converter = CreateConverter(false);
                return binding;
            }
            if(Calculator.Operands.Count() > 1) {
                var binding = new MultiBinding() { Mode = BindingMode.OneWay };
                SetBindingProperties(binding, true);
                binding.Converter = CreateConverter(false);
                foreach(var op in Calculator.Operands) {
                    var subBinding = CreateBinding(op, BindingMode.OneWay);
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
            readonly ICommandCalculator calculator;
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
            ICommandCalculator Calculator { get { return (ICommandCalculator)calculator.Target; } }
            object[] Values { get { return isEmpty ? null : values.Select(x => x.Target).ToArray(); } }
            bool IsAlive {
                get {
                    bool res = errorHandler.IsAlive && calculator.IsAlive;
                    if(isEmpty) return res;
                    return res && !values.Any(x => !x.IsAlive);
                }
            }
            public Command(IErrorHandler errorHandler, ICommandCalculator calculator, bool fallbackCanExecute, bool isEmpty, object[] values) {
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
        IEventCalculator Calculator { get; set; }

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
            if(TargetProvider.TargetProperty is MethodInfo) {
                MethodInfo m = (MethodInfo)TargetProvider.TargetProperty;
                if(m.Name.StartsWith("Add") && m.Name.EndsWith("Handler"))
                    return;
            }
            ErrorHandler.Throw(ErrorHelper.Err002(this), null);
        }
        protected override void Init() {
            TreeInfo = new EventTreeInfo(Handler, ErrorHandler);
            if(ActualResolvingMode == DXBindingResolvingMode.LegacyStaticTyping)
                Calculator = new EventCalculator(TreeInfo);
            else Calculator = new EventCalculatorDynamic(TreeInfo);
            Calculator.Init(TypeResolver);
        }
        protected override object GetProvidedValue() {
            var eventBinder = new EventBinder(this, GetEventHandlerType(), CreateBinding());
            return eventBinder.GetEventHandler();
        }
        protected override IEnumerable<Operand> GetOperands() {
            return Calculator != null ? Calculator.Operands : null;
        }
        Type GetEventHandlerType() {
            if(TargetProvider.TargetProperty is EventInfo)
                return ((EventInfo)TargetProvider.TargetProperty).EventHandlerType;
            MethodInfo m = (MethodInfo)TargetProvider.TargetProperty;
            return m.GetParameters().ElementAt(1).ParameterType;
        }
        BindingBase CreateBinding() {
            if(Calculator.Operands.Count() == 0) {
                var binding = CreateBinding(null, BindingMode.OneTime);
                binding.Source = null;
                binding.Converter = new EventConverter(this, true);
                return binding;
            }
            if(Calculator.Operands.Count() == 1) {
                var binding = CreateBinding(Calculator.Operands.First(), BindingMode.OneTime);
                binding.Converter = new EventConverter(this, false);
                return binding;
            }
            if(Calculator.Operands.Count() > 1) {
                var binding = new MultiBinding() { Mode = BindingMode.OneTime };
                foreach(var op in Calculator.Operands) {
                    var subBinding = CreateBinding(op, BindingMode.OneTime);
                    binding.Bindings.Add(subBinding);
                }
                binding.Converter = new EventConverter(this, false);
                return binding;
            }
            throw new NotImplementedException();
        }

        class EventBinder {
            readonly IErrorHandler errorHandler;
            readonly IEventCalculator calculator;
            readonly WeakReference targetObject;
            readonly string handler;
            readonly Type targetType;
            readonly string targetPropertyName;
            readonly Type targetPropertyType;
            readonly Type eventHandlerType;
            DependencyProperty dataProperty;
            static object locker = new object();
            static long dataPropertyIndex = 0;
            static Dictionary<Tuple<Type, string>, DependencyProperty> propertiesCache = new Dictionary<Tuple<Type, string>, DependencyProperty>();

            DependencyObject TargetObject { get { return (DependencyObject)targetObject.Target; } }
            bool IsAlive { get { return targetObject.IsAlive; } }
            public EventBinder(DXEventExtension owner, Type eventHandlerType, BindingBase binding) {
                lock (locker) {
                    this.errorHandler = owner.ErrorHandler;
                    this.calculator = owner.Calculator;
                    var target = (DependencyObject)owner.TargetProvider.TargetObject;
                    this.targetObject = new WeakReference(target);
                    this.handler = owner.Handler;
                    this.targetType = target.GetType();
                    this.targetPropertyName = owner.TargetPropertyName;
                    this.targetPropertyType = owner.TargetPropertyType;
                    this.eventHandlerType = eventHandlerType;
                    var dataPropertyInfo = Tuple.Create(target.GetType(), owner.Handler);
                    if (!propertiesCache.TryGetValue(dataPropertyInfo, out dataProperty)) {
                        dataProperty = DependencyProperty.Register(
                            "Tag" + dataPropertyIndex++.ToString(), typeof(object), dataPropertyInfo.Item1);
                        propertiesCache[dataPropertyInfo] = dataProperty;
                    }
                    BindCore(binding);
                }
            }
            public Delegate GetEventHandler() {
                var eventSubscriber = new DevExpress.Mvvm.UI.Interactivity.Internal.EventTriggerEventSubscriber(OnEvent);
                return eventSubscriber.CreateEventHandler(eventHandlerType);
            }
            object[] GetBoundEventData() {
                if(!IsAlive) return null;
                var res = (IEnumerable<object>)TargetObject.GetValue(dataProperty);
                if (res == null) {
                    var expr = BindingOperations.GetBindingExpression(TargetObject, dataProperty);
                    if(expr != null && expr.Status == BindingStatus.Unattached) {
                        var b = BindingOperations.GetBinding(TargetObject, dataProperty);
                        BindCore(b);
                        res = (IEnumerable<object>)TargetObject.GetValue(dataProperty);
                    }
                }
                return res == null ? null : res.ToArray();
            }
            void OnEvent(object sender, object eventArgs) {
                var data = GetBoundEventData();
                errorHandler.ClearError();
                calculator.Event(data, sender, eventArgs);
            }
            void BindCore(BindingBase binding) {
                if (TargetObject == null) return;
                try {
                    BindingOperations.SetBinding(TargetObject, dataProperty, binding);
                } catch (Exception e) {
                    string message = "DXEvent cannot set binding on data property. " + Environment.NewLine
                        + "Expr: " + handler + Environment.NewLine
                        + "TargetProperty: " + targetPropertyName + Environment.NewLine
                        + "TargetPropertyType: " + targetPropertyType.ToString() + Environment.NewLine
                        + "TargetObjectType: " + targetType + Environment.NewLine
                        + "DataProperty: " + dataProperty.Name;
                    throw new DXEventException(targetPropertyName, targetType.ToString(), handler, message, e);
                }
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
        public string TargetPropertyName { get; private set; }
        public string TargetObjectType { get; private set; }
        protected DXBindingExceptionBase(TOwner owner, string message, Exception innerException)
            : this(owner.TargetPropertyName, owner.TargetObjectName, message, innerException) {
        }
        protected DXBindingExceptionBase(string targetPropertyName, string targetObjectType, string message, Exception innerException)
            : base(message, innerException) {
            TargetPropertyName = targetPropertyName;
            TargetObjectType = targetObjectType;
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
        public string Expr { get; private set; }
        public string BackExpr { get; private set; }
        public DXBindingException(DXBindingExtension owner, string message, Exception innerException)
            : base(owner, message, innerException) {
            Expr = owner.Expr;
            BackExpr = owner.BackExpr;
        }
        protected override string Report(string message) {
            return ErrorHelper.ReportBindingError(message, Expr, BackExpr);
        }
    }
    public sealed class DXCommandException : DXBindingExceptionBase<DXCommandException, DXCommandExtension> {
        public string Execute { get; private set; }
        public string CanExecute { get; private set; }
        public DXCommandException(DXCommandExtension owner, string message, Exception innerException)
            : base(owner, message, innerException) {
            Execute = owner.Execute;
            CanExecute = owner.CanExecute;
        }
        protected override string Report(string message) {
            return ErrorHelper.ReportCommandError(message, Execute, CanExecute);
        }
    }
    public sealed class DXEventException : DXBindingExceptionBase<DXEventException, DXEventExtension> {
        public string Handler { get; private set; }
        public DXEventException(DXEventExtension owner, string message, Exception innerException)
            : this(owner.TargetPropertyName, owner.TargetObjectName, owner.Handler, message, innerException) { }
        public DXEventException(string targetPropertyName, string targetObjectType, string handler, string message, Exception innerException)
            : base(targetPropertyName, targetObjectType, message, innerException) {
            Handler = handler;
        }
        protected override string Report(string message) {
            return ErrorHelper.ReportEventError(message, Handler);
        }
    }
}
namespace DevExpress.Xpf.Core.Native {
    public interface ITypedStyle { }
}