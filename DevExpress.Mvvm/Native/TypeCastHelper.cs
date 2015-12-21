using DevExpress.Mvvm.Native;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Reflection;
#if !NETFX_CORE
using System.Windows.Threading;
#else
using Windows.UI.Xaml;
using Windows.UI.Core;
#endif

namespace DevExpress.Mvvm.Native {
    public static class TypeCastHelper {
        public static object TryCast(object value, Type targetType) {
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
#if !NETFX_CORE
            if(underlyingType.IsEnum && value is string) {
#else
            if(underlyingType.IsEnum() && value is string) {
#endif
                value = Enum.Parse(underlyingType, (string)value, false);
            } else if(
#if !NETFX_CORE
                value is IConvertible &&
#else
                IsConvertableType(value) &&
#endif
                !targetType.IsAssignableFrom(value.GetType())) {
                value = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
            }
#if !NETFX_CORE
            if(value == null && targetType.IsValueType)
#else
            if(value == null && targetType.IsValueType())
#endif
                value = Activator.CreateInstance(targetType);
            return value;
        }
#if NETFX_CORE
        static readonly Type[] convertableTypes = new Type[] {
                    typeof(System.Decimal), typeof(System.Decimal?),
                    typeof(System.Single), typeof(System.Single?),
                    typeof(System.Double), typeof(System.Double?),
                    typeof(System.Int16), typeof(System.Int16?),
                    typeof(System.Int32), typeof(System.Int32?),
                    typeof(System.Int64), typeof(System.Int64?),
                    typeof(System.UInt16), typeof(System.UInt16?),
                    typeof(System.UInt32), typeof(System.UInt32?),
                    typeof(System.UInt64), typeof(System.UInt64?),
                    typeof(System.Byte), typeof(System.Byte?),
                    typeof(System.SByte), typeof(System.SByte?),
                    typeof(System.String), typeof(DateTime)
        };
        static bool IsConvertableType(object parameter) {
            return parameter != null && Array.IndexOf<Type>(convertableTypes, parameter.GetType()) >= 0;
        }
#endif
    }
}