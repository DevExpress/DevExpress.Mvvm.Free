using System;
using System.Reflection;
using System.Windows;

namespace DevExpress.Mvvm.UI.Native {
    public static class ObjectPropertyHelper {
        public static PropertyInfo GetPropertyInfoSetter(object obj, string propName) {
            return GetPropertyInfo(obj, propName, BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
        }
        public static PropertyInfo GetPropertyInfoGetter(object obj, string propName) {
            return GetPropertyInfo(obj, propName, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
        }
        public static PropertyInfo GetPropertyInfo(object obj, string propName, BindingFlags flags) {
            if(string.IsNullOrWhiteSpace(propName) || obj == null)
                return null;
            return obj.GetType().GetProperty(propName, flags);
        }
        public static DependencyProperty GetDependencyProperty(object obj, string propName) {
            if(string.IsNullOrWhiteSpace(propName) || obj == null)
                return null;

            Type objType = obj.GetType();
            FieldInfo field = objType.GetField(propName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            field = field ?? objType.GetField(propName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return field == null ? null : (DependencyProperty)field.GetValue(obj);
        }
    }
}