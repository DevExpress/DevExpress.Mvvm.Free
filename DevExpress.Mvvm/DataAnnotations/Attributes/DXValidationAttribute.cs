using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.Native {
    public abstract class DXValidationAttribute : Attribute {
        public delegate string ErrorMessageAccessorDelegate(object value, object instance);
        public static Func<object, string> ErrorMessageAccessor(Func<string> errorMessageAccessor) {
            if(errorMessageAccessor == null) return null;
            return x => errorMessageAccessor();
        }
        public static Func<object, string> ErrorMessageAccessor<TProperty>(Func<TProperty, string> errorMessageAccessor) {
            if(errorMessageAccessor == null) return null;
            return x => errorMessageAccessor((TProperty)x);
        }
        public static ErrorMessageAccessorDelegate ErrorMessageAccessor<TProperty, TInstance>(Func<TProperty, TInstance, string> errorMessageAccessor) {
            if(errorMessageAccessor == null) return null;
            return (x,y) => errorMessageAccessor((TProperty)x, (TInstance)y);
        }

        readonly ErrorMessageAccessorDelegate errorMessageAccessor;
        protected DXValidationAttribute() { throw new NotSupportedException(); }
        protected DXValidationAttribute(Func<string> errorMessageAccessor, Func<string> defaultErrorMessageAccessor)
            : this(errorMessageAccessor == null ? null : new ErrorMessageAccessorDelegate((x,y) => errorMessageAccessor())
                  , defaultErrorMessageAccessor) { }
        protected DXValidationAttribute(Func<object, string> errorMessageAccessor, Func<string> defaultErrorMessageAccessor)
            : this(errorMessageAccessor == null ? null : new ErrorMessageAccessorDelegate((x,y) => errorMessageAccessor(x))
                  , defaultErrorMessageAccessor) { }
        protected DXValidationAttribute(ErrorMessageAccessorDelegate errorMessageAccessor, Func<string> defaultErrorMessageAccessor) {
            ErrorMessageAccessorDelegate _defaultErrorMessageAccessor = defaultErrorMessageAccessor == null
                ? null : new ErrorMessageAccessorDelegate((x, y) => defaultErrorMessageAccessor());
            this.errorMessageAccessor = errorMessageAccessor ?? _defaultErrorMessageAccessor;
        }
        protected virtual string FormatErrorMessage(string error, string name) {
            return string.Format(CultureInfo.CurrentCulture, error, name);
        }
        public string GetValidationResult(object value, string memberName, object instance = null) {
            if(memberName == null) {
                throw new ArgumentNullException("memberName");
            }
            if(!IsValid(value) || !IsInstanceValid(value, instance)) {
                return FormatErrorMessage(GetErrorMessageString(value, instance), memberName);
            }
            return null;
        }
        protected abstract bool IsValid(object value);
        protected virtual bool IsInstanceValid(object value, object instance) { return true; }
        protected string GetErrorMessageString(object value, object instance) { return errorMessageAccessor(value, instance); }
    }
}