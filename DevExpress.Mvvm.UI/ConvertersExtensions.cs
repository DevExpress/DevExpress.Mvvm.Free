using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace DevExpress.Mvvm.UI {
    public class TypeCastConverterExtension : MarkupExtension {
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new TypeCastConverter();
        }
    }
    public class NumericToBooleanConverterExtension : MarkupExtension {
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new NumericToBooleanConverter();
        }
    }
    public class StringToBooleanConverterExtension : MarkupExtension {
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new StringToBooleanConverter();
        }
    }
    public class ObjectToBooleanConverterExtension : MarkupExtension {
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new ObjectToBooleanConverter() { Inverse = this.Inverse };
        }
        public bool Inverse { get; set; }
    }
    public class BooleanToVisibilityConverterExtension : MarkupExtension {
        public bool Inverse { get; set; }
        public bool HiddenInsteadOfCollapsed { get; set; }
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new BooleanToVisibilityConverter() { Inverse = this.Inverse, HiddenInsteadOfCollapsed = this.HiddenInsteadOfCollapsed };
        }
    }
    public class NumericToVisibilityConverterExtension : MarkupExtension {
        public bool Inverse { get; set; }
        public bool HiddenInsteadOfCollapsed { get; set; }
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new NumericToVisibilityConverter() { Inverse = this.Inverse, HiddenInsteadOfCollapsed = this.HiddenInsteadOfCollapsed };
        }
    }
    public class DefaultBooleanToBooleanConverterExtension : MarkupExtension {
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new DefaultBooleanToBooleanConverter();
        }
    }
    public class BooleanNegationConverterExtension : MarkupExtension {
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new BooleanNegationConverter();
        }
    }
    public class FormatStringConverterExtension : MarkupExtension {
        public string FormatString { get; set; }
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new FormatStringConverter() { FormatString = this.FormatString };
        }
    }
    public class BooleanToObjectConverterExtension : MarkupExtension {
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }
        public object NullValue { get; set; }
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new BooleanToObjectConverter() { TrueValue = this.TrueValue, FalseValue = this.FalseValue, NullValue = this.NullValue };
        }
    }
}