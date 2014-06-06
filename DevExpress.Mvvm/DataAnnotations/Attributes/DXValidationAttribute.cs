using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.Native {
    public abstract class DXValidationAttribute : Attribute {
        readonly Func<string> errorMessageAccessor;
        protected DXValidationAttribute() { throw new NotSupportedException(); }
        protected DXValidationAttribute(Func<string> errorMessageAccessor, Func<string> defaultErrorMessageAccessor) {
            this.errorMessageAccessor = errorMessageAccessor ?? defaultErrorMessageAccessor;
        }
        protected virtual string FormatErrorMessage(string name) {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
        public string GetValidationResult(object value, string memberName, object instance = null) {
            if(memberName == null) {
                throw new ArgumentNullException("memberName");
            }
            if(!IsValid(value) || !IsInstanceValid(instance)) {
                return FormatErrorMessage(memberName);
            }
            return null;
        }
        protected abstract bool IsValid(object value);
        protected virtual bool IsInstanceValid(object instance) { return true; }
        protected string ErrorMessageString { get { return errorMessageAccessor(); } }
    }
}