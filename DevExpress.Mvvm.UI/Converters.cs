using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.Native;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;
using System.Collections.Generic;
using DevExpress.Mvvm.UI.Native;
using System.Collections;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.UI {
    public class ReflectionConverter : IValueConverter {
        class TypeUnsetValue { }
        Type convertBackMethodOwner = typeof(TypeUnsetValue);

        class ConvertMethodSignature {
            int valueIndex;
            int targetTypeIndex;
            int parameterIndex;
            int cultureIndex;

            public ConvertMethodSignature(Type[] parameterTypes, int valueIndex, int targetTypeIndex, int parameterIndex, int cultureIndex) {
                ParameterTypes = parameterTypes;
                this.valueIndex = valueIndex;
                this.targetTypeIndex = targetTypeIndex;
                this.parameterIndex = parameterIndex;
                this.cultureIndex = cultureIndex;
            }
            public Type[] ParameterTypes { get; private set; }
            public void AssignArgs(object[] args, object value, Type targetType, object parameter, CultureInfo culture) {
                args[valueIndex] = value;
                if(targetTypeIndex >= 0)
                    args[targetTypeIndex] = targetType;
                if(parameterIndex >= 0)
                    args[parameterIndex] = parameter;
                if(cultureIndex >= 0)
                    args[cultureIndex] = culture;
            }
        }

        static readonly ConvertMethodSignature[] ConvertMethodSignatures = new ConvertMethodSignature[] {
            new ConvertMethodSignature(new Type[] { null }, 0, -1, -1, -1),
            new ConvertMethodSignature(new Type[] { null, typeof(CultureInfo) }, 0, -1, -1, 1),
            new ConvertMethodSignature(new Type[] { null, typeof(Type) }, 0, 1, -1, -1),
            new ConvertMethodSignature(new Type[] { null, null }, 0, -1, 1, -1),
            new ConvertMethodSignature(new Type[] { null, typeof(Type), null }, 0, 1, 2, -1),
            new ConvertMethodSignature(new Type[] { null, typeof(Type), typeof(CultureInfo) }, 0, 1, -1, 2),
            new ConvertMethodSignature(new Type[] { null, null, typeof(CultureInfo) }, 0, -1, 1, 2),
            new ConvertMethodSignature(new Type[] { null, typeof(Type), null, typeof(CultureInfo) }, 0, 1, 2, 3),
        };

        public Type ConvertMethodOwner { get; set; }
        public string ConvertMethod { get; set; }
        public Type ConvertBackMethodOwner {
            get { return convertBackMethodOwner == typeof(TypeUnsetValue) ? ConvertMethodOwner : convertBackMethodOwner; }
            set { convertBackMethodOwner = value; }
        }
        public string ConvertBackMethod { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ConvertCore(value, targetType, parameter, culture, ConvertMethodOwner, ConvertMethod);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ConvertCore(value, targetType, parameter, culture, ConvertBackMethodOwner, ConvertBackMethod);
        }
        static object ConvertCore(object value, Type targetType, object parameter, CultureInfo culture, Type convertMethodOwner, string convertMethod) {
            if(convertMethodOwner == null) {
                if(convertMethod == null)
                    return targetType == null ? value : ConvertByTargetTypeConstructor(value, targetType, parameter, culture);
                else
                    return value == null ? null : ConvertBySourceValueMethod(value, targetType, parameter, culture, convertMethod);
            } else {
                if(convertMethod == null)
                    return ConvertByConstructor(value, targetType, parameter, culture, convertMethodOwner);
                else
                    return ConvertByStaticMethod(value, targetType, parameter, culture, convertMethodOwner, convertMethod);
            }
        }
        static object ConvertByTargetTypeConstructor(object value, Type targetType, object parameter, CultureInfo culture) {
            return ConvertByConstructor(value, targetType, parameter, culture, targetType.GetConstructors());
        }
        static object ConvertByConstructor(object value, Type targetType, object parameter, CultureInfo culture, Type convertMethodOwner) {
            return ConvertByConstructor(value, targetType, parameter, culture, convertMethodOwner.GetConstructors());
        }
        static object ConvertByConstructor(object value, Type targetType, object parameter, CultureInfo culture, IEnumerable<ConstructorInfo> methods) {
            if(value == null && (targetType == null || !targetType.IsValueType || targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))) return null;
            ConstructorInfo convertMethod = methods.Where(c => c.GetParameters().Length == 1).FirstOrDefault();
            if(convertMethod == null)
                convertMethod = methods.Where(c => c.GetParameters().Length > 0 && !c.GetParameters().Skip(1).Any(p => !p.IsOptional)).FirstOrDefault();
            if(convertMethod == null)
                throw new InvalidOperationException();
            ParameterInfo[] parameters = convertMethod.GetParameters();
            object[] args = new object[parameters.Length];
            args[0] = value;
            parameters.Skip(1).Select(p => p.DefaultValue).ToArray().CopyTo(args, 1);
            return convertMethod.Invoke(args);
        }
        static object ConvertBySourceValueMethod(object value, Type targetType, object parameter, CultureInfo culture, string convertMethodName) {
            MethodInfo convertMethod = value.GetType().GetMethod(convertMethodName, new Type[] { });
            if(convertMethod == null)
                convertMethod = value.GetType().GetMethods().Where(c => c.Name == convertMethodName && c.GetParameters().Length > 0 && !c.GetParameters().Any(p => !p.IsOptional)).FirstOrDefault();
            if(convertMethod == null)
                throw new InvalidOperationException();
            ParameterInfo[] parameters = convertMethod.GetParameters();
            object[] args = parameters.Select(p => p.DefaultValue).ToArray();
            return convertMethod.Invoke(value, args);
        }
        static object ConvertByStaticMethod(object value, Type targetType, object parameter, CultureInfo culture, Type convertMethodOwner, string convertMethodName) {
            Tuple<MethodInfo, ConvertMethodSignature> convertMethod = GetMethod(convertMethodOwner.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name == convertMethodName));
            if(convertMethod == null)
                throw new InvalidOperationException();
            ParameterInfo[] parameters = convertMethod.Item1.GetParameters();
            object[] args = new object[parameters.Length];
            convertMethod.Item2.AssignArgs(args, value, targetType, parameter, culture);
            for(int i = convertMethod.Item2.ParameterTypes.Length; i < args.Length; ++i)
                args[i] = parameters[i].DefaultValue;
            return convertMethod.Item1.Invoke(null, args);
        }
        static Tuple<MethodInfo, ConvertMethodSignature> GetMethod(IEnumerable<MethodInfo> methods) {
            foreach(var method in methods) {
                ParameterInfo[] parameters = method.GetParameters();
                var variantMatch = ConvertMethodSignatures.Where(v => Match(parameters, v.ParameterTypes)).FirstOrDefault();
                if(variantMatch != null) return new Tuple<MethodInfo, ConvertMethodSignature>(method, variantMatch);
            }
            return null;
        }
        static bool Match(ParameterInfo[] parameterInfo, Type[] parameterTypes) {
            if(parameterTypes.Length > parameterInfo.Length) return false;
            for(int i = parameterTypes.Length; i < parameterInfo.Length; ++i) {
                if(!parameterInfo[i].IsOptional) return false;
            }
            for(int i = 0; i < parameterTypes.Length; ++i) {
                if(!Match(parameterInfo[i].ParameterType, parameterTypes[i])) return false;
            }
            return true;
        }
        static bool Match(Type actual, Type expected) {
            return expected == null || actual == expected;
        }
    }
    public class EnumerableConverter : IValueConverter {
        public Type TargetItemType { get; set; }
        public IValueConverter ItemConverter { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            IEnumerable enumerable = value as IEnumerable;
            if(enumerable == null) return null;
            Type targetItemType = GetTargetItemType(targetType);
            Func<object, object> convertItem = x => ItemConverter == null ? x : ItemConverter.Convert(x, targetItemType, parameter, culture);
            IEnumerable convertedEnumerable = (IEnumerable)Activator.CreateInstance(typeof(EnumerableWrap<>).MakeGenericType(targetItemType), enumerable, convertItem);
            if(targetType == null || targetType.IsAssignableFrom(convertedEnumerable.GetType()))
                return convertedEnumerable;
            else if(targetType.IsInterface)
                return CreateList(targetType, targetItemType, convertedEnumerable);
            else if(targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>))
                return CreateReadOnlyCollection(targetType, targetItemType, convertedEnumerable);
            else
                return CreateCollection(targetType, targetItemType, convertedEnumerable);
        }
        Type GetTargetItemType(Type targetType) {
            if(TargetItemType != null)
                return TargetItemType;
            if(targetType == null)
                throw new InvalidOperationException();
            var interfaces = new Type[] { targetType }.Where(t => t.IsInterface).Concat(targetType.GetInterfaces());
            Type targetItemType = interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).Select(i => i.GetGenericArguments()[0]).FirstOrDefault();
            if(targetItemType == null)
                throw new InvalidOperationException();
            return targetItemType;
        }
        object CreateList(Type targetType, Type itemType, IEnumerable enumerable) {
            if(targetType != null && (targetType == typeof(IEnumerable) || targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                return enumerable;
            Type collectionType = typeof(List<>).MakeGenericType(itemType);
            if(targetType != null && !targetType.IsAssignableFrom(collectionType))
                throw new NotSupportedCollectionException(targetType);
            return Activator.CreateInstance(collectionType, enumerable);
        }
        object CreateReadOnlyCollection(Type targetType, Type itemType, IEnumerable enumerable) {
            object list = CreateList(null, itemType, enumerable);
            return list.GetType().GetMethod("AsReadOnly").Invoke(list, new object[] { });
        }
        object CreateCollection(Type targetType, Type itemType, IEnumerable enumerable) {
            ConstructorInfo constructor1 = targetType.GetConstructor(new Type[] { typeof(IEnumerable) });
            if(constructor1 != null)
                return constructor1.Invoke(new object[] { enumerable });
            ConstructorInfo constructor2 = targetType.GetConstructor(new Type[] { typeof(IEnumerable<>).MakeGenericType(itemType) });
            if(constructor2 != null)
                return constructor2.Invoke(new object[] { enumerable });
            return CreateCollectionWithDefaultConstructor(targetType, itemType, enumerable);
        }
        object CreateCollectionWithDefaultConstructor(Type targetType, Type itemType, IEnumerable enumerable) {
            object collection;
            try {
                collection = Activator.CreateInstance(targetType);
            } catch(MissingMethodException e) {
                throw new NotSupportedCollectionException(targetType, null, e);
            }
            IList list = collection as IList;
            if(list != null) {
                foreach(var item in enumerable)
                    list.Add(item);
                return list;
            }
            MethodInfo addMethod;
            Type genericListType = typeof(IList<>).MakeGenericType(itemType);
            if(targetType.GetInterfaces().Any(t => t == genericListType)) {
                addMethod = genericListType.GetMethod("Add", new Type[] { itemType });
            } else {
                addMethod = targetType.GetMethod("Add", new Type[] { itemType });
                if(addMethod == null)
                    addMethod = targetType.GetMethods().Where(m => m.GetParameters().Length == 1).Where(m => m.GetParameters()[0].ParameterType.IsAssignableFrom(itemType)).FirstOrDefault();
            }
            if(addMethod == null)
                throw new NotSupportedCollectionException(targetType);
            foreach(var item in enumerable)
                addMethod.Invoke(collection, new object[] { item });
            return collection;
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
    public class NotSupportedCollectionException : Exception {
        public NotSupportedCollectionException(Type collectionType, string message = null, Exception innerException = null)
            : base(message, innerException) {
            CollectionType = collectionType;
        }
        public Type CollectionType { get; private set; }
    }
    public class TypeCastConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return TypeCastHelper.TryCast(value, targetType);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return TypeCastHelper.TryCast(value, targetType);
        }
    }
    public class NumericToBooleanConverter : IValueConverter {
        public bool Inverse { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ConverterHelper.NumericToBoolean(value, Inverse);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
    public class StringToBooleanConverter : IValueConverter {
        public bool Inverse { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ConverterHelper.StringToBoolean(value, Inverse);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
    public class ObjectToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value != null ^ Inverse;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
        public bool Inverse { get; set; }
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
        internal static object Coerce(object value, Type targetType, bool ignoreImplicitXamlConversions = false) {
            if(value == null || targetType == typeof(object) || value.GetType() == targetType) return value;
            if(targetType.IsAssignableFrom(value.GetType())) return value;
            var nullableType = Nullable.GetUnderlyingType(targetType);
            var coerced = CoerceNonNullable(value, nullableType ?? targetType, ignoreImplicitXamlConversions);
            if(nullableType != null) {
                return Activator.CreateInstance(targetType, coerced);
            }
            return coerced;
        }
        internal static bool IsImplicitXamlConvertion(Type valueType, Type targetType) {
            if(targetType == typeof(Thickness))
                return true;
            if (targetType == typeof(GridLength))
                return true;
            if (targetType == typeof(ImageSource) && (valueType == typeof(string) || valueType == typeof(Uri)))
                return true;
            return false;
        }
        internal static object CoerceNonNullable(object value, Type targetType, bool ignoreImplicitXamlConversions) {
            if (!ignoreImplicitXamlConversions && IsImplicitXamlConvertion(value.GetType(), targetType))
                return value;
            if (targetType == typeof(string)) {
                return value.ToString();
            }
            if (targetType.IsEnum && value is string) {
                return Enum.Parse(targetType, (string)value, false);
            }
            if (targetType == typeof(Color)) {
                var c = new ColorConverter();
                if (c.IsValid(value))
                    return c.ConvertFrom(value);
                return value;
            }
            if (targetType == typeof(Brush) || targetType == typeof(SolidColorBrush)) {
                var c = new BrushConverter();
                if (c.IsValid(value))
                    return c.ConvertFrom(value);
                if(value is Color)
                    return BrushesCache.GetBrush((Color)value);
                return value;
            }
            var cc = TypeDescriptor.GetConverter(targetType);
            try {
                if (cc != null && cc.IsValid(value))
                    return cc.ConvertFrom(null, System.Globalization.CultureInfo.InvariantCulture, value);
                return System.Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            MapItem entry = Map.FirstOrDefault(MakeMapPredicate(item => item.Source, value));
            return Coerce(entry == null ? DefaultTarget : entry.Target, targetType);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            MapItem entry = Map.FirstOrDefault(MakeMapPredicate(item => item.Target, value));
            return Coerce(entry == null ? DefaultSource : entry.Source, targetType);
        }
    }
    public class BooleanToVisibilityConverter : IValueConverter {
        bool hiddenInsteadOfCollapsed;
        public bool Inverse { get; set; }
        [Obsolete("Use the HiddenInsteadOfCollapsed property instead."), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool HiddenInsteadCollapsed { get { return hiddenInsteadOfCollapsed; } set { hiddenInsteadOfCollapsed = value; } }
        public bool HiddenInsteadOfCollapsed { get { return hiddenInsteadOfCollapsed; } set { hiddenInsteadOfCollapsed = value; } }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool booleanValue = ConverterHelper.GetBooleanValue(value);
            return ConverterHelper.BooleanToVisibility(booleanValue ^ Inverse, HiddenInsteadOfCollapsed);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            bool booleanValue = (value is Visibility && (Visibility)value == Visibility.Visible) ^ Inverse;
            return booleanValue;
        }
    }
    public class NumericToVisibilityConverter : IValueConverter {
        bool hiddenInsteadOfCollapsed;
        public bool Inverse { get; set; }
        public bool HiddenInsteadOfCollapsed { get { return hiddenInsteadOfCollapsed; } set { hiddenInsteadOfCollapsed = value; } }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool boolean = ConverterHelper.NumericToBoolean(value, Inverse);
            return ConverterHelper.BooleanToVisibility(boolean, HiddenInsteadOfCollapsed);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
    public class StringToVisibilityConverter : IValueConverter {
        bool hiddenInsteadOfCollapsed;
        public bool Inverse { get; set; }
        public bool HiddenInsteadOfCollapsed { get { return hiddenInsteadOfCollapsed; } set { hiddenInsteadOfCollapsed = value; } }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool boolean = ConverterHelper.StringToBoolean(value, Inverse);
            return ConverterHelper.BooleanToVisibility(boolean, HiddenInsteadOfCollapsed);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }

    public class DefaultBooleanToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool? booleanValue = ConverterHelper.GetNullableBooleanValue(value);
            if(targetType == typeof(bool)) return booleanValue ?? false;
            return booleanValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            bool? booleanValue = ConverterHelper.GetNullableBooleanValue(value);
            if(targetType == typeof(bool)) return booleanValue ?? false;
            return booleanValue;
        }
    }
    public class BooleanNegationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool? booleanValue = ConverterHelper.GetNullableBooleanValue(value);
            if(booleanValue != null)
                booleanValue = !booleanValue.Value;
            if(targetType == typeof(bool)) return booleanValue ?? true;
            return booleanValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value, targetType, parameter, culture);
        }
    }
    public class FormatStringConverter : IValueConverter {
        public string FormatString { get; set; }
        bool allowSimpleFormatString = true;
        public bool AllowSimpleFormatString { get { return allowSimpleFormatString; } set { allowSimpleFormatString = value; } }
        public TextCaseFormat OutStringCaseFormat { get; set; }
        public bool SplitPascalCase { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var res = GetFormattedValue(FormatString, value, System.Globalization.CultureInfo.CurrentUICulture, OutStringCaseFormat, AllowSimpleFormatString);
            if(res is string && SplitPascalCase)
                return SplitStringHelper.SplitPascalCaseString((string)res);
            return res;
        }
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        public static object GetFormattedValue(string formatString, object value, System.Globalization.CultureInfo culture, bool allowSimpleFormatString = true) {
            string displayFormat = GetDisplayFormat(formatString, allowSimpleFormatString);
            return string.IsNullOrEmpty(displayFormat) ? value : string.Format(culture, displayFormat, value);
        }
        public static object GetFormattedValue(string formatString, object value, System.Globalization.CultureInfo culture, TextCaseFormat outStringCaseFormat, bool allowSimpleFormatString = true) {
            object o = GetFormattedValue(formatString, value, culture, allowSimpleFormatString);
            if(o == null)
                return null;

            switch(outStringCaseFormat) {
                case TextCaseFormat.Lower:
                    return o.ToString().ToLower();
                case TextCaseFormat.Upper:
                    return o.ToString().ToUpper();
                default:
                    return o.ToString();
            }
        }
        public static string GetDisplayFormat(string displayFormat, bool allowSimpleFormatString = true) {
            if(string.IsNullOrEmpty(displayFormat))
                return string.Empty;
            if(!allowSimpleFormatString) return displayFormat;
            string res = displayFormat;
            if(res.Contains("{"))
                return res;
            return string.Format("{{0:{0}}}", res);
        }

        public enum TextCaseFormat { Default, Lower, Upper }
    }
    public class BooleanToObjectConverter : IValueConverter {
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }
        public object NullValue { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
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

    public class EnumToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null)
                return null;
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || !targetType.IsEnum)
                return value;
            return Enum.Parse(targetType, value.ToString());
        }
    }

    public class ColorToBrushConverter : IValueConverter {
        public byte? CustomA { get; set; }
        public static SolidColorBrush Convert(object value, byte? customA = null) {
            if(value == null) return null;
            Color color;
            if(value is System.Drawing.Color) {
                var mColor = (System.Drawing.Color)value;
                color = Color.FromArgb(mColor.A, mColor.R, mColor.G, mColor.B);
            } else
                color = (Color)value;
            if(customA != null)
                color.A = customA.Value;
            return BrushesCache.GetBrush(color);
        }
        public static Color ConvertBack(object value) {
            return value != null ? ((SolidColorBrush)value).Color : default(Color);
        }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value, CustomA);
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ConvertBack(value);
        }
    }
    public class BrushToColorConverter : IValueConverter {
        public static Color Convert(object value) {
            return ColorToBrushConverter.ConvertBack(value);
        }
        public static SolidColorBrush ConvertBack(object value) {
            return ColorToBrushConverter.Convert(value);
        }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is SolidColorBrush))
                return Colors.Black;
            return Convert(value);
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ConvertBack(value);
        }
    }

    public static class DelegateConverterFactory {
        public static IValueConverter CreateValueConverter(Func<object, object> convert, Func<object, object> convertBack = null) {
            return new DelegateValueConverter<object, object>(convert, convertBack, true);
        }
        public static IValueConverter CreateValueConverter(Func<object, Type, object, CultureInfo, object> convert, Func<object, Type, object, CultureInfo, object> convertBack = null) {
            return new DelegateValueConverter<object, object>(convert, convertBack, true);
        }
        public static IValueConverter CreateValueConverter<TIn, TOut>(Func<TIn, TOut> convert, Func<TOut, TIn> convertBack = null) {
            return new DelegateValueConverter<TIn, TOut>(convert, convertBack, false);
        }
        public static IValueConverter CreateValueConverter<TIn, TOut>(Func<TIn, object, CultureInfo, TOut> convert, Func<TOut, object, CultureInfo, TIn> convertBack = null) {
            return new DelegateValueConverter<TIn, TOut>(convert, convertBack, false);
        }

        public static IMultiValueConverter CreateMultiValueConverter(Func<object[], object> convert, Func<object, object[]> convertBack = null) {
            return new DelegateMultiValueConverter(convert, convertBack, -1, true);
        }
        public static IMultiValueConverter CreateMultiValueConverter(Func<object[], Type, object, CultureInfo, object> convert, Func<object, Type[], object, CultureInfo, object[]> convertBack = null) {
            return new DelegateMultiValueConverter(convert, convertBack, -1, true);
        }
        public static IMultiValueConverter CreateMultiValueConverter<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> convert, Func<TOut, Tuple<TIn1, TIn2>> convertBack = null) {
            return new DelegateMultiValueConverter(x => convert((TIn1)x[0], (TIn2)x[1]), x => ToArray(convertBack((TOut)x)), 2, false);
        }
        public static IMultiValueConverter CreateMultiValueConverter<TIn1, TIn2, TIn3, TOut>(Func<TIn1, TIn2, TIn3, TOut> convert, Func<TOut, Tuple<TIn1, TIn2, TIn3>> convertBack = null) {
            return new DelegateMultiValueConverter(x => convert((TIn1)x[0], (TIn2)x[1], (TIn3)x[2]), x => ToArray(convertBack((TOut)x)), 3, false);
        }
        public static IMultiValueConverter CreateMultiValueConverter<TIn1, TIn2, TIn3, TIn4, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TOut> convert, Func<TOut, Tuple<TIn1, TIn2, TIn3, TIn4>> convertBack = null) {
            return new DelegateMultiValueConverter(x => convert((TIn1)x[0], (TIn2)x[1], (TIn3)x[2], (TIn4)x[3]), x => ToArray(convertBack((TOut)x)), 4, false);
        }
        public static IMultiValueConverter CreateMultiValueConverter<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> convert, Func<TOut, Tuple<TIn1, TIn2, TIn3, TIn4, TIn5>> convertBack = null) {
            return new DelegateMultiValueConverter(x => convert((TIn1)x[0], (TIn2)x[1], (TIn3)x[2], (TIn4)x[3], (TIn5)x[4]), x => ToArray(convertBack((TOut)x)), 5, false);
        }
        public static IMultiValueConverter CreateMultiValueConverter<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> convert, Func<TOut, Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>> convertBack = null) {
            return new DelegateMultiValueConverter(x => convert((TIn1)x[0], (TIn2)x[1], (TIn3)x[2], (TIn4)x[3], (TIn5)x[4], (TIn6)x[5]), x => ToArray(convertBack((TOut)x)), 6, false);
        }

        static object[] ToArray<T1, T2>(Tuple<T1, T2> tuple) {
            return new object[] { tuple.Item1, tuple.Item2 };
        }
        static object[] ToArray<T1, T2, T3>(Tuple<T1, T2, T3> tuple) {
            return new object[] { tuple.Item1, tuple.Item2, tuple.Item3 };
        }
        static object[] ToArray<T1, T2, T3, T4>(Tuple<T1, T2, T3, T4> tuple) {
            return new object[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4 };
        }
        static object[] ToArray<T1, T2, T3, T4, T5>(Tuple<T1, T2, T3, T4, T5> tuple) {
            return new object[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5 };
        }
        static object[] ToArray<T1, T2, T3, T4, T5, T6>(Tuple<T1, T2, T3, T4, T5, T6> tuple) {
            return new object[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6 };
        }

        class DelegateValueConverter<TIn, TOut> : IValueConverter {
            readonly Func<TIn, TOut> convert1;
            readonly Func<TOut, TIn> convertBack1;
            readonly Func<TIn, object, CultureInfo, TOut> convert2;
            readonly Func<TOut, object, CultureInfo, TIn> convertBack2;
            readonly Func<TIn, Type, object, CultureInfo, TOut> convert3;
            readonly Func<TOut, Type, object, CultureInfo, TIn> convertBack3;
            readonly bool allowUnsetValue;
            public DelegateValueConverter(Func<TIn, TOut> convert, Func<TOut, TIn> convertBack, bool allowUnsetValue) {
                this.convert1 = convert;
                this.convertBack1 = convertBack;
                this.allowUnsetValue = allowUnsetValue;
            }
            public DelegateValueConverter(Func<TIn, object, CultureInfo, TOut> convert, Func<TOut, object, CultureInfo, TIn> convertBack, bool allowUnsetValue) {
                this.convert2 = convert;
                this.convertBack2 = convertBack;
                this.allowUnsetValue = allowUnsetValue;
            }
            public DelegateValueConverter(Func<TIn, Type, object, CultureInfo, TOut> convert, Func<TOut, Type, object, CultureInfo, TIn> convertBack, bool allowUnsetValue) {
                this.convert3 = convert;
                this.convertBack3 = convertBack;
                this.allowUnsetValue = allowUnsetValue;
            }
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                if (!allowUnsetValue && value == DependencyProperty.UnsetValue)
                    return Binding.DoNothing;
                if (convert1 != null)
                    return convert1((TIn)value);
                if (convert2 != null)
                    return convert2((TIn)value, parameter, culture);
                if (convert3 != null)
                    return convert3((TIn)value, targetType, parameter, culture);
                throw new InvalidOperationException();
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                if (convertBack1 != null)
                    return convertBack1((TOut)value);
                if (convertBack2 != null)
                    return convertBack2((TOut)value, parameter, culture);
                if (convertBack3 != null)
                    return convertBack3((TOut)value, targetType, parameter, culture);
                throw new InvalidOperationException();
            }
        }
        class DelegateMultiValueConverter : IMultiValueConverter {
            readonly Func<object[], object> convert1;
            readonly Func<object, object[]> convertBack1;
            readonly Func<object[], Type, object, CultureInfo, object> convert2;
            readonly Func<object, Type[], object, CultureInfo, object[]> convertBack2;
            readonly int valuesCount;
            readonly bool allowUnsetValue;
            public DelegateMultiValueConverter(Func<object[], object> convert, Func<object, object[]> convertBack, int valuesCount, bool allowUnsetValue) {
                this.convert1 = convert;
                this.convertBack1 = convertBack;
                this.valuesCount = valuesCount;
                this.allowUnsetValue = allowUnsetValue;
            }
            public DelegateMultiValueConverter(Func<object[], Type, object, CultureInfo, object> convert, Func<object, Type[], object, CultureInfo, object[]> convertBack, int valuesCount, bool allowUnsetValue) {
                this.convert2 = convert;
                this.convertBack2 = convertBack;
                this.valuesCount = valuesCount;
                this.allowUnsetValue = allowUnsetValue;
            }
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
                if (this.valuesCount > 0 && values.Count() != this.valuesCount)
                    throw new TargetParameterCountException();
                if(!allowUnsetValue && values.Any(x => x == DependencyProperty.UnsetValue))
                    return Binding.DoNothing;
                if (convert1 != null)
                    return convert1(values);
                if (convert2 != null)
                    return convert2(values, targetType, parameter, culture);
                throw new InvalidOperationException();
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
                if (convertBack1 != null)
                    return convertBack1(value);
                if (convertBack2 != null)
                    return convertBack2(value, targetTypes, parameter, culture);
                throw new InvalidOperationException();
            }
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
            if(value is bool)
                return (bool)value;
            if(value is bool?) {
                bool? nullable = (bool?)value;
                return nullable.HasValue ? nullable.Value : false;
            }
            return false;
        }
        public static bool? GetNullableBooleanValue(object value) {
            if(value is bool) return (bool)value;
            if(value is bool?) return (bool?)value;
            return null;
        }
        public static bool NumericToBoolean(object value, bool inverse) {
            if (value == null)
                return CorrectBoolean(false, inverse);
            try {
                var d = (double)System.Convert.ChangeType(value, typeof(double), null);
                return CorrectBoolean(d != 0d, inverse);
            } catch (Exception) { }
            return CorrectBoolean(false, inverse);
        }
        public static bool StringToBoolean(object value, bool inverse) {
            if (!(value is string))
                return CorrectBoolean(false, inverse);
            return CorrectBoolean(!String.IsNullOrEmpty((string)value), inverse);
        }
        public static Visibility BooleanToVisibility(bool booleanValue, bool hiddenInsteadOfCollapsed) {
            return booleanValue ?
                Visibility.Visible :
 (hiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed);
        }
        static bool CorrectBoolean(bool value, bool inverse) {
            return value ^ inverse;
        }
    }
}
namespace DevExpress.Mvvm.UI.Native {
    public static class BrushesCache {
        public static SolidColorBrush GetBrush(Color color) {
            WeakReference r;
            cache.TryGetValue(color, out r);
            if (r != null && r.IsAlive)
                return (SolidColorBrush)r.Target;
            SolidColorBrush res = new SolidColorBrush(color);
            res.Freeze();
            lock (cache) cache[color] = new WeakReference(res);
            return res;
        }

        readonly static Dictionary<Color, WeakReference> cache;
        static BrushesCache() {
            cache = new Dictionary<Color, WeakReference>();
            AddDefaultBrushes();
        }
        static void AddDefaultBrushes() {
            var props =
                typeof(Brushes)
                .GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .AsEnumerable();
            props.Where(x => x.PropertyType == typeof(SolidColorBrush));
            foreach(var prop in props) {
                var brush = (SolidColorBrush)prop.GetValue(null, null);
                var color = brush.Color;
                if(cache.ContainsKey(color)) continue;
                cache.Add(color, new WeakReference(brush));
            }
        }
    }
}