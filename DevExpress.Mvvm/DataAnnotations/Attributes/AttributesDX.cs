using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.Native {
    public abstract class DXDataTypeAttribute : DXValidationAttribute {
        public PropertyDataType DataType { get; private set; }
        protected DXDataTypeAttribute() { throw new NotSupportedException(); }
        protected DXDataTypeAttribute(Func<string> errorMessageAccessor, Func<string> defaultErrorMessageAccessor, PropertyDataType dataType)
            : base(errorMessageAccessor, defaultErrorMessageAccessor) {
            this.DataType = dataType;
        }
    }

    internal class DXMinLengthAttribute : DXValidationAttribute {
        public DXMinLengthAttribute(int length, Func<object, string> errorMessageAccessor)
            : base(errorMessageAccessor, () => DataAnnotationsResourcesResolver.MinLengthAttribute_ValidationError) {
            this.Length = length;
        }
        protected override string FormatErrorMessage(string error, string name) {
            return string.Format(CultureInfo.CurrentCulture, error, name, this.Length);
        }
        protected override bool IsValid(object value) {
            if(Length < 0)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, DataAnnotationsResourcesResolver.MinLengthAttribute_InvalidMinLength));
            if(value == null)
                return true;
            string stringValue = value as string;
            int valueLength = stringValue != null ? stringValue.Length : ((Array)value).Length;
            return valueLength >= Length;
        }
        public int Length { get; private set; }
    }
    public class DXMaxLengthAttribute : DXValidationAttribute {
        private const int MaxAllowableLength = -1;
        protected DXMaxLengthAttribute() { throw new NotSupportedException(); }
        public DXMaxLengthAttribute(int length, Func<object, string> errorMessageAccessor)
            : base(errorMessageAccessor, () => DataAnnotationsResourcesResolver.MaxLengthAttribute_ValidationError) {
            this.Length = length;
        }

        protected override string FormatErrorMessage(string error, string name) {
            return string.Format(CultureInfo.CurrentCulture, error, name, this.Length);
        }
        protected override bool IsValid(object value) {
            if(Length == 0 || Length < -1)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, DataAnnotationsResourcesResolver.MaxLengthAttribute_InvalidMaxLength));
            if(value != null) {
                string stringValue = value as string;
                int valueLength = stringValue != null ? stringValue.Length : ((Array)value).Length;
                if(Length != -1)
                    return valueLength <= Length;
            }
            return true;
        }
        public int Length { get; private set; }
    }

    #region regex
    internal abstract class RegexAttributeBase : DXDataTypeAttribute {
        protected const RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase;

        readonly Regex regex;

        public RegexAttributeBase(Regex regex, Func<string> errorMessageAccessor, Func<string> defaultErrorMessageAccessor, PropertyDataType dataType)
            : base(errorMessageAccessor, defaultErrorMessageAccessor, dataType) {
            this.regex = regex;
        }
        protected sealed override bool IsValid(object value) {
            if(value == null)
                return true;
            string input = value as string;
            return input != null && regex.Match(input).Length > 0;
        }
    }
    internal sealed class PhoneAttribute : RegexAttributeBase {
        static readonly Regex regex = new Regex(@"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$", DefaultRegexOptions);
        public PhoneAttribute(Func<string> errorMessageAccessor)
            : base(regex, errorMessageAccessor, () => DataAnnotationsResourcesResolver.PhoneAttribute_Invalid, PropertyDataType.PhoneNumber) {
        }
    }
    internal sealed class EmailAddressAttribute : RegexAttributeBase {
        static readonly Regex regex = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$", DefaultRegexOptions);
        public EmailAddressAttribute(Func<string> errorMessageAccessor)
            : base(regex, errorMessageAccessor, () => DataAnnotationsResourcesResolver.EmailAddressAttribute_Invalid, PropertyDataType.Custom) {
        }
    }
    internal sealed class UrlAttribute : RegexAttributeBase {
        private static Regex regex = new Regex(@"^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$", DefaultRegexOptions);
        public UrlAttribute(Func<string> errorMessageAccessor)
            : base(regex, errorMessageAccessor, () => DataAnnotationsResourcesResolver.UrlAttribute_Invalid, PropertyDataType.Url) {
        }
    }
    internal class RegularExpressionAttribute : DXValidationAttribute {
        readonly string pattern;
        Regex regex;
        public RegularExpressionAttribute(string pattern, Func<object, string> errorMessageAccessor)
            : base(errorMessageAccessor, () => DataAnnotationsResourcesResolver.RegexAttribute_ValidationError) {
            if(string.IsNullOrEmpty(pattern))
                throw new ArgumentNullException("pattern");
            this.pattern = pattern;
        }
        protected override string FormatErrorMessage(string error, string name) {
            return string.Format(CultureInfo.CurrentCulture, error, name, pattern);
        }

        protected override bool IsValid(object value) {
            if(regex == null)
                regex = new Regex(pattern);
            string str = Convert.ToString(value, CultureInfo.CurrentCulture);
            if(string.IsNullOrEmpty(str))
                return true;
            Match match = regex.Match(str);
            return match.Success && match.Index == 0 && match.Length == str.Length;
        }
    }
    #endregion

    internal sealed class CreditCardAttribute : DXDataTypeAttribute {
        public CreditCardAttribute(Func<string> errorMessageAccessor)
            : base(errorMessageAccessor, () => DataAnnotationsResourcesResolver.CreditCardAttribute_Invalid, PropertyDataType.Custom) {
        }
        protected override bool IsValid(object value) {
            if(value == null)
                return true;
            string stringValue = value as string;
            if(stringValue == null)
                return false;
            stringValue = stringValue.Replace("-", "").Replace(" ", "");
            int number = 0;
            bool oddEvenFlag = false;
            foreach(char ch in stringValue.Reverse<char>()) {
                if(ch < '0' || ch > '9')
                    return false;
                int digitValue = (ch - '0') * (oddEvenFlag ? 2 : 1);
                oddEvenFlag = !oddEvenFlag;
                while(digitValue > 0) {
                    number += digitValue % 10;
                    digitValue /= 10;
                }
            }
            return (number % 10) == 0;
        }
    }

    internal class RangeAttribute : DXValidationAttribute {
        readonly IComparable maximum;
        readonly IComparable minimum;

        public RangeAttribute(object minimum, object maximum, Func<object, string> errorMessageAccessor)
            : base(errorMessageAccessor, () => DataAnnotationsResourcesResolver.RangeAttribute_ValidationError) {
            if(!(minimum is IComparable))
                throw new ArgumentException("minimum");
            if(!(maximum is IComparable))
                throw new ArgumentException("maximum");
            this.minimum = minimum as IComparable;
            this.maximum = maximum as IComparable;
        }
        protected override string FormatErrorMessage(string error, string name) {
            return string.Format(CultureInfo.CurrentCulture, error, name, minimum, maximum);
        }
        protected override bool IsValid(object value) {
            if(value == null)
                return true;
            string stringValue = value as string;
            if(stringValue == string.Empty)
                return true;

            IComparable val = value as IComparable;
            if(value == null)
                throw new ArgumentException("value");
            return minimum.CompareTo(val) <= 0 && maximum.CompareTo(val) >= 0;
        }
    }
    [AttributeUsageAttribute(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    internal class CustomValidationAttribute : DXValidationAttribute {
        readonly Func<object, bool> isValidFunction;
        readonly Type valueType;
        public CustomValidationAttribute(Type valueType, Func<object, bool> isValidFunction, Func<object, string> errorMessageAccessor)
            : base(errorMessageAccessor, () => DataAnnotationsResourcesResolver.CustomValidationAttribute_ValidationError) {
            this.valueType = valueType;
            this.isValidFunction = isValidFunction;
        }
        protected override bool IsValid(object value) {
            if (IsValueTypeAndNull(valueType, value))
                return true;
            return isValidFunction(value);
        }
        internal static bool IsValueTypeAndNull(Type valueType, object value) {
            bool isValueType = valueType.IsValueType;
            bool isNullable = isValueType
                && valueType.IsGenericType
                && valueType.GetGenericTypeDefinition() == typeof(Nullable<>);
            return isValueType && !isNullable && value == null;
        }
    }
    [AttributeUsageAttribute(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    internal class CustomInstanceValidationAttribute : DXValidationAttribute {
        readonly Func<object, object, bool> isValidFunction;
        readonly Type valueType;
        public CustomInstanceValidationAttribute(Type valueType, Func<object, object, bool> isValidFunction, ErrorMessageAccessorDelegate errorMessageAccessor)
            : base(errorMessageAccessor, () => DataAnnotationsResourcesResolver.CustomValidationAttribute_ValidationError) {
            this.valueType = valueType;
            this.isValidFunction = isValidFunction;
        }
        protected override bool IsValid(object value) { return true; }
        protected override bool IsInstanceValid(object value, object instance) {
            if (CustomValidationAttribute.IsValueTypeAndNull(valueType, value))
                return true;
            return isValidFunction(value, instance);
        }
    }
    public class DXRequiredAttribute : DXValidationAttribute {
        readonly bool allowEmptyStrings;
        protected DXRequiredAttribute() { throw new NotSupportedException(); }
        public DXRequiredAttribute(bool allowEmptyStrings, Func<string> errorMessageAccessor)
            : base(errorMessageAccessor, () => DataAnnotationsResourcesResolver.RequiredAttribute_ValidationError) {
            this.allowEmptyStrings = allowEmptyStrings;
        }
        protected override bool IsValid(object value) {
            if(value == null)
                return false;
            string stringValue = value as string;
            if(stringValue != null && !this.allowEmptyStrings)
                return stringValue.Trim().Length != 0;
            return true;
        }
    }
}