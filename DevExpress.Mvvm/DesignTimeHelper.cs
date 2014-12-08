using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DevExpress.Mvvm {
    public static class DesignTimeHelper {
#if DEBUG
        public static int? DesignTimeObjectsCount;
#endif
        public static T[] CreateDesignTimeObjects<T>(int count) where T : class {
#if DEBUG
            count = DesignTimeObjectsCount.GetValueOrDefault(count);
#endif
            return Enumerable.Range(0, count).Select(x => CreateDesignTimeObject<T>(x)).Where(x => x != null).ToArray();
        }
        public static T CreateDesignTimeObject<T>() where T : class {
            return CreateDesignTimeObject<T>(0);
        }
        static T CreateDesignTimeObject<T>(int index) where T : class {
            T obj;
            try {
                obj = Activator.CreateInstance<T>();
                foreach(PropertyInfo property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                    if(property.CanWrite && property.GetSetMethod() != null)
                        property.SetValue(obj, DesignTimeValuesProvider.GetDesignTimeValue(property.PropertyType, index), null);
                }
                return obj;
            } catch {
                return null;
            }
        }

    }
}