using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace DevExpress.Mvvm.UI {
    public class ReflectionConverterExtension : MarkupExtension {
        class TypeUnsetValue { }
        Type convertBackMethodOwner = typeof(TypeUnsetValue);

        public Type ConvertMethodOwner { get; set; }
        public string ConvertMethod { get; set; }
        public Type ConvertBackMethodOwner {
            get { return convertBackMethodOwner == typeof(TypeUnsetValue) ? ConvertMethodOwner : convertBackMethodOwner; }
            set { convertBackMethodOwner = value; }
        }
        public string ConvertBackMethod { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return new ReflectionConverter() { ConvertMethodOwner = ConvertMethodOwner, ConvertMethod = ConvertMethod, ConvertBackMethodOwner = ConvertBackMethodOwner, ConvertBackMethod = ConvertBackMethod };
        }
    }
    public class EnumerableConverterExtension : MarkupExtension {
        public EnumerableConverterExtension() { }
        public EnumerableConverterExtension(IValueConverter itemConverter) {
            ItemConverter = itemConverter;
        }
        public IValueConverter ItemConverter { get; set; }
        public Type TargetItemType { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return new EnumerableConverter() { ItemConverter = ItemConverter, TargetItemType = TargetItemType };
        }
    }
    public class TypeCastConverterExtension : MarkupExtension {
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new TypeCastConverter();
        }
    }
    public class NumericToBooleanConverterExtension : MarkupExtension {
        public bool Inverse { get; set; }
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new NumericToBooleanConverter() { Inverse = this.Inverse };
        }
    }
    public class StringToBooleanConverterExtension : MarkupExtension {
        public bool Inverse { get; set; }
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new StringToBooleanConverter() { Inverse = this.Inverse };
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
    public class StringToVisibilityConverterExtension : MarkupExtension {
        public bool Inverse { get; set; }
        public bool HiddenInsteadOfCollapsed { get; set; }
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new StringToVisibilityConverter() { Inverse = this.Inverse, HiddenInsteadOfCollapsed = this.HiddenInsteadOfCollapsed };
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
        bool allowSimpleFormatString = true;
        public bool AllowSimpleFormatString { get { return allowSimpleFormatString; } set { allowSimpleFormatString = value; } }
        public bool SplitPascalCase { get; set; }
        public FormatStringConverter.TextCaseFormat OutStringCaseFormat { get; set; }
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new FormatStringConverter() {
                FormatString = this.FormatString,
                AllowSimpleFormatString = this.AllowSimpleFormatString,
                SplitPascalCase = this.SplitPascalCase,
                OutStringCaseFormat = this.OutStringCaseFormat,
            };
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
    public class EnumToStringConverterExtension : MarkupExtension {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return new EnumToStringConverter();
        }
    }
    public class ColorToBrushConverterExtension : MarkupExtension {
        public byte? CustomA { get; set; }
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new ColorToBrushConverter() { CustomA = CustomA };
        }
    }
    public class BrushToColorConverterExtension : MarkupExtension {
        public override object ProvideValue(System.IServiceProvider serviceProvider) {
            return new BrushToColorConverter();
        }
    }
}