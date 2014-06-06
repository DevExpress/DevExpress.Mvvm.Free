using DevExpress.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace DevExpress.Mvvm.UI {
    public class NumericToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null)
                return false;
            try {
                var d = (double)System.Convert.ChangeType(value, typeof(double), null);
                return d != 0d;
            } catch(Exception) { }
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
    public class StringToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(!(value is string))
                return false;
            return !String.IsNullOrEmpty((string)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
    public class ObjectToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value != null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
    public class MapItem {
        public MapItem() { }
        public MapItem(object source, object target) {
            Source = source;
            Target = target;
        }
        public object Source { get; set; }
        public object Target { get; set; }
    }
    [ContentProperty("Map")]
    public class ObjectToObjectConverter : IValueConverter {
        public object DefaultSource { get; set; }
        public object DefaultTarget { get; set; }
        public ObservableCollection<MapItem> Map { get; set; }
        public ObjectToObjectConverter() {
            Map = new ObservableCollection<MapItem>();
        }
        object Coerce(object value, Type targetType) {
            if(value == null || targetType == value.GetType()) {
                return value;
            }
            if(targetType == typeof(string)) {
                return value.ToString();
            }
            if(targetType.IsEnum && value is string) {
                return Enum.Parse(targetType, (string)value, false);
            }
            try {
                return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            } catch {
                return value;
            }
        }
        public static bool SafeCompare(object left, object right) {
            if(left == null) {
                if(right == null)
                    return true;
                return right.Equals(left);
            }
            return left.Equals(right);
        }
        Func<MapItem, bool> MakeMapPredicate(Func<MapItem, object> selector, object value) {
            return mapItem => {
                object source = Coerce(selector(mapItem), (value ?? string.Empty).GetType());
                return SafeCompare(source, value);
            };
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            MapItem entry = Map.FirstOrDefault(MakeMapPredicate(item => item.Source, value));
            return entry == null ? DefaultTarget : entry.Target;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            MapItem entry = Map.FirstOrDefault(MakeMapPredicate(item => item.Target, value));
            return entry == null ? DefaultSource : entry.Source;
        }
    }
    public class BooleanToVisibilityConverter : IValueConverter {
        bool hiddenInsteadOfCollapsed;
        public bool Inverse { get; set; }
        [Obsolete, Browsable(false)]
        public bool HiddenInsteadCollapsed { get { return hiddenInsteadOfCollapsed; } set { hiddenInsteadOfCollapsed = value; } }
        public bool HiddenInsteadOfCollapsed { get { return hiddenInsteadOfCollapsed; } set { hiddenInsteadOfCollapsed = value; } }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool booleanValue = ConverterHelper.GetBooleanValue(value);
            return (booleanValue ^ Inverse) ?
                Visibility.Visible :
#if !SILVERLIGHT
                (HiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed);
#else
                Visibility.Collapsed;
#endif
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            bool booleanValue = (value is Visibility && (Visibility)value == Visibility.Visible) ^ Inverse;
            if(targetType == typeof(DefaultBoolean))
                return ConverterHelper.ToDefaultBoolean(booleanValue);
            return booleanValue;
        }
    }
    public class DefaultBooleanToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool? booleanValue = ConverterHelper.GetNullableBooleanValue(value);
            if(targetType == typeof(bool)) return booleanValue ?? false;
            else if(targetType == typeof(DefaultBoolean)) return ConverterHelper.ToDefaultBoolean(booleanValue);
            else return booleanValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            bool? booleanValue = ConverterHelper.GetNullableBooleanValue(value);
            if(targetType == typeof(bool)) return booleanValue ?? false;
            else if(targetType == typeof(bool?)) return booleanValue;
            else return ConverterHelper.ToDefaultBoolean(booleanValue);
        }
    }
    public class BooleanNegationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool? booleanValue = ConverterHelper.GetNullableBooleanValue(value);
            if(booleanValue != null)
                booleanValue = !booleanValue.Value;
            if(targetType == typeof(bool)) return booleanValue ?? true;
            else if(targetType == typeof(DefaultBoolean)) return ConverterHelper.ToDefaultBoolean(booleanValue);
            else return booleanValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value, targetType, parameter, culture);
        }
    }
    public class FormatStringConverter : IValueConverter {
        public string FormatString { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return GetFormattedValue(FormatString, value, CultureInfo.CurrentUICulture);
        }
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        public static object GetFormattedValue(string formatString, object value, CultureInfo culture) {
            string displayFormat = GetDisplayFormat(formatString);
            return string.IsNullOrEmpty(displayFormat) ? value : string.Format(culture, displayFormat, value);
        }
        static string GetDisplayFormat(string displayFormat) {
            if(string.IsNullOrEmpty(displayFormat))
                return string.Empty;
            string res = displayFormat;
            if(res.Contains("{"))
                return res;
            return string.Format("{{0:{0}}}", res);
        }
    }
    public class BooleanToObjectConverter : IValueConverter {
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }
        public object NullValue { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is DefaultBoolean) {
                var asDefaultBoolean = (DefaultBoolean)value;
                if(asDefaultBoolean == DefaultBoolean.True) {
                    value = true;
                } else if(asDefaultBoolean == DefaultBoolean.False) {
                    value = false;
                } else {
                    value = null;
                }
            }
            if(value is bool?) {
                value = (bool?)value == true;
            }
            if(value == null) {
                return NullValue;
            }
            if(!(value is bool))
                return null;
            bool asBool = (bool)value;
            return asBool ? TrueValue : FalseValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    static class ConverterHelper {
        public static string[] GetParameters(object parameter) {
            string param = parameter as string;
            if(string.IsNullOrEmpty(param))
                return new string[0];
            return param.Split(';');
        }
        public static bool GetBooleanParameter(string[] parameters, string name) {
            foreach(string parameter in parameters) {
                if(string.Equals(parameter, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        public static bool GetBooleanValue(object value) {
            bool booleanValue = false;
            if(value is bool) {
                booleanValue = (bool)value;
            } else if(value is bool?) {
                bool? nullable = (bool?)value;
                booleanValue = nullable.HasValue ? nullable.Value : false;
            } else if(value is DefaultBoolean) {
                booleanValue = (DefaultBoolean)value == DefaultBoolean.True;
            }
            return booleanValue;
        }
        public static bool? GetNullableBooleanValue(object value) {
            if(value is bool) return (bool)value;
            if(value is bool?) return (bool?)value;
            if(value is DefaultBoolean) {
                DefaultBoolean defaultBoolean = (DefaultBoolean)value;
                return defaultBoolean == DefaultBoolean.Default ? (bool?)null : defaultBoolean == DefaultBoolean.True;
            }
            return null;
        }
        public static DefaultBoolean ToDefaultBoolean(bool? booleanValue) {
            return booleanValue == null ? DefaultBoolean.Default : booleanValue.Value ? DefaultBoolean.True : DefaultBoolean.False;
        }
    }
}