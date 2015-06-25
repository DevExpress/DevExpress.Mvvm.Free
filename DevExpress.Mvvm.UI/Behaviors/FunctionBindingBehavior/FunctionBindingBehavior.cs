using System;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm.Native;
using System.Globalization;

namespace DevExpress.Mvvm.UI {
    public class FunctionBindingBehavior : FunctionBindingBehaviorBase {
        #region Dependency Properties
        public static readonly DependencyProperty PropertyProperty =
             DependencyProperty.Register("Property", typeof(string), typeof(FunctionBindingBehavior),
             new PropertyMetadata(null, (d, e) => ((FunctionBindingBehavior)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register("Converter", typeof(IValueConverter), typeof(FunctionBindingBehavior),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehavior)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty ConverterParameterProperty =
            DependencyProperty.Register("ConverterParameter", typeof(object), typeof(FunctionBindingBehavior),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehavior)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty FunctionProperty =
            DependencyProperty.Register("Function", typeof(string), typeof(FunctionBindingBehavior),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehavior)d).OnResultAffectedPropertyChanged()));

        public string Property {
            get { return (string)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public IValueConverter Converter {
            get { return (IValueConverter)GetValue(ConverterProperty); }
            set { SetValue(ConverterProperty, value); }
        }
        public object ConverterParameter {
            get { return GetValue(ConverterParameterProperty); }
            set { SetValue(ConverterParameterProperty, value); }
        }
        public string Function {
            get { return (string)GetValue(FunctionProperty); }
            set { SetValue(FunctionProperty, value); }
        }
        #endregion

        protected override string ActualFunction { get { return Function; } }

        protected object GetSourceMethodValue() {
            object result = InvokeSourceFunction(ActualSource, ActualFunction, GetArgsInfo(this), DefaultMethodInfoChecker);
            return Converter.Return(x => x.Convert(result, null, ConverterParameter, CultureInfo.InvariantCulture), () => result);
        }
        protected override void OnResultAffectedPropertyChanged() {
            if(ActualTarget == null || ActualSource == null || string.IsNullOrEmpty(ActualFunction) || string.IsNullOrEmpty(Property) || !IsAttached)
                return;

            Action<object> propertySetter = GetObjectPropertySetter(ActualTarget, Property, false);
            if(propertySetter == null)
                return;

            object value = GetSourceMethodValue();
            if(value == DependencyProperty.UnsetValue)
                return;

            propertySetter.Invoke(value);
        }
    }
}