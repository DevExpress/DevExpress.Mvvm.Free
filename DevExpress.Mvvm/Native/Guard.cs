using System;
using System.Collections.Generic;

namespace DevExpress.Mvvm.Native {
    public static class GuardHelper {
        public static void ArgumentNotNull(object value, string name) {
            if (Object.ReferenceEquals(value, null))
                ThrowArgumentNullException(name);
        }
        public static void ArgumentNonNegative(int value, string name) {
            if (value < 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentPositive(int value, string name) {
            if (value <= 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNonNegative(float value, string name) {
            if (value < 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentPositive(float value, string name) {
            if (value <= 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNonNegative(double value, string name) {
            if (value < 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentPositive(double value, string name) {
            if (value <= 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentIsNotNullOrEmpty(string value, string name) {
            if (string.IsNullOrEmpty(value))
                ThrowArgumentException(name, value);
        }
        public static void ArgumentIsInRange<T>(IList<T> list, int index, string name) {
            ArgumentIsInRange(0, list.Count - 1, index, name);
        }
        public static void ArgumentIsInRange(int minValue, int maxValue, int value, string name) {
            if(value < minValue || value > maxValue)
                ThrowArgumentException(name, value);
        }

        public static TValue ArgumentMatchType<TValue>(object value, string name) {
            try {
                return (TValue)value;
            } catch(InvalidCastException e) {
                ThrowArgumentException(name, value, e);
                throw new InvalidOperationException();
            }
        }
        public static void ArgumentMatch<TValue>(TValue value, string name, Func<TValue, bool> predicate) {
            if(!predicate(value))
                ThrowArgumentException(name, value);
        }
        static void ThrowArgumentException(string propName, object val, Exception innerException = null) {
            string valueStr =
                Object.ReferenceEquals(val, string.Empty) ? "String.Empty" :
                Object.ReferenceEquals(val, null) ? "null" :
                val.ToString();
            string s = String.Format("'{0}' is not a valid value for '{1}'", valueStr, propName);
            throw new ArgumentException(s, innerException);
        }
        static void ThrowArgumentNullException(string propName) {
            throw new ArgumentNullException(propName);
        }
    }
}