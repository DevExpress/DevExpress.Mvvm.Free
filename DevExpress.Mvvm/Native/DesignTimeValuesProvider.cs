using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.Native {
    public static class DesignTimeValuesProvider {
        public static readonly DateTime Today = DateTime.Today;
        public static object[] Types = new object[] { typeof(string), typeof(DateTime), typeof(int), typeof(Decimal), typeof(byte),
             typeof(Int64), typeof(double), typeof(bool) };
        public static object[] CreateValues() {
            return new object[] { "string", Today, 123, 123, 123, 123, 123, null };
        }
        public static object[][] CreateDistinctValues() {
            return new object[][] {
                    new object[] { "string1", Today, 123, 123, (byte)123, 123, 123, null },
                    new object[] { "string2", Today.AddDays(1), 456, 456, (byte)124, 456, 456, true },
                    new object[] { "string3", Today.AddDays(2), 789, 789, (byte)125, 789, 789, false },
                };
        }
        static readonly object[][] distinctValues = CreateDistinctValues();
        public static object GetDesignTimeValue(Type propertyType, int index) {
            return GetDesignTimeValue(propertyType, index, null, distinctValues, true);
        }
        public static object GetDesignTimeValue(Type propertyType, object component, object[] values, object[][] distinctValues, bool useDistinctValues) {
            Type type = Nullable.GetUnderlyingType(propertyType);
            if(type == null)
                type = propertyType;
            int index = Array.IndexOf(DesignTimeValuesProvider.Types, type);
            if(index == -1) {
                if(type.IsValueType)
                    return Activator.CreateInstance(type);
                else
                    return null;
            }
            object value = GetValues((int)component, values, distinctValues, useDistinctValues)[index];
            return value != null ? Convert.ChangeType(value, type, null) : null;
        }
        static object[] GetValues(int index, object[] values, object[][] distinctValues, bool useDistinctValues) {
            return useDistinctValues ? distinctValues[index % distinctValues.Length] : values;
        }
    }
}