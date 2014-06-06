using DevExpress.Mvvm.Native;
using System;
using System.Reflection;
using System.Linq;
using DevExpress.Mvvm.POCO;
using System.ComponentModel;

namespace DevExpress.Mvvm {
    public static class IDataErrorInfoHelper {
        public static string GetErrorText(object owner, string propertyName) {
            if(owner == null)
                throw new ArgumentNullException("owner");
            int pathDelimiterIndex = propertyName.IndexOf('.');
            if(pathDelimiterIndex >= 0)
                return GetNestedPropertyErrorText(owner, propertyName, pathDelimiterIndex);
            object propertyValue;
            if(!TryGetPropertyValue(owner, propertyName, out propertyValue))
                return string.Empty;
            return GetErrorText(owner, propertyName, propertyValue) ?? string.Empty;
        }
        static string GetNestedPropertyErrorText(object owner, string path, int pathDelimiterIndex) {
            string propertyName = path.Remove(pathDelimiterIndex);
            object propertyValue;
            if(!TryGetPropertyValue(owner, propertyName, out propertyValue))
                return string.Empty;
            IDataErrorInfo nestedDataErrorInfo = propertyValue as IDataErrorInfo;
            if(nestedDataErrorInfo == null)
                return string.Empty;
            return nestedDataErrorInfo[path.Substring(pathDelimiterIndex + 1, path.Length - pathDelimiterIndex - 1)];
        }
        static string GetErrorText(object obj, string propertyName, object value) {
            Type objType = obj.GetType();
            if(obj is IPOCOViewModel) {
                objType = objType.BaseType;
            }
            PropertyValidator validator = GetPropertyValidator(objType, propertyName);
            if(validator == null)
                return null;
            return validator.GetErrorText(value, obj);
        }
        static bool TryGetPropertyValue(object owner, string propertyName, out object propertyValue) {
            propertyValue = null;
            PropertyInfo pi = owner.GetType().GetProperty(propertyName);
            if(pi == null)
                return false;
            MethodInfo getter = pi.GetGetMethod();
            if(getter == null)
                return false;
            propertyValue = getter.Invoke(owner, null);
            return true;
        }
        static PropertyValidator GetPropertyValidator(Type type, string propertyName) {
            MemberInfo memberInfo = type.GetProperty(propertyName);
            return PropertyValidator.FromAttributes(GetAllAttributes(memberInfo), propertyName);
        }
        static Attribute[] GetAllAttributes(MemberInfo member) {
            return MetadataHelper
                .GetAllAttributes(member)
                .Where(a => a is DXValidationAttribute || a is System.ComponentModel.DataAnnotations.ValidationAttribute)
                .ToArray();
        }
    }
}