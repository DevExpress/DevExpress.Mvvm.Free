using DevExpress.Mvvm.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace DevExpress.Mvvm.DataAnnotations {
    public abstract class PropertyMetadataBuilderBase<T, TProperty, TBuilder> :
        MemberMetadataBuilderBase<T, TBuilder, ClassMetadataBuilder<T>>
        where TBuilder : PropertyMetadataBuilderBase<T, TProperty, TBuilder> {
        internal PropertyMetadataBuilderBase(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent)
            : base(storage, parent) {
        }

        protected TBuilder RequiredCore(bool allowEmptyStrings = false, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new DXRequiredAttribute(allowEmptyStrings, errorMessageAccessor));
        }
        protected TBuilder RequiredCore(Func<string> errorMessageAccessor) {
            return RequiredCore(false, errorMessageAccessor);
        }
        protected TBuilder MaxLengthCore(int maxLength, Func<TProperty, string> errorMessageAccessor) {
            var _errorMessageAccessor = DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor);
            return AddOrReplaceAttribute(new DXMaxLengthAttribute(maxLength, _errorMessageAccessor));
        }
        protected TBuilder MinLengthCore(int minLength, Func<TProperty, string> errorMessageAccessor) {
            var _errorMessageAccessor = DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor);
            return AddOrReplaceAttribute(new DXMinLengthAttribute(minLength, _errorMessageAccessor));
        }
        protected TBuilder MatchesRegularExpressionCore(string pattern, Func<TProperty, string> errorMessageAccessor) {
            var _errorMessageAccessor = DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor);
            return AddOrReplaceAttribute(new RegularExpressionAttribute(pattern, _errorMessageAccessor));
        }
        protected TBuilder MatchesRuleCore(Func<TProperty, bool> isValidFunction, Func<TProperty, string> errorMessageAccessor) {
            var _errorMessageAccessor = DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor);
            return AddAttribute(new CustomValidationAttribute(typeof(TProperty), x => isValidFunction((TProperty)x), _errorMessageAccessor));
        }
        protected TBuilder MatchesInstanceRuleCore(Func<TProperty, T, bool> isValidFunction, Func<TProperty, T, string> errorMessageAccessor) {
            var _errorMessageAccessor = DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor);
            return AddAttribute(new CustomInstanceValidationAttribute(typeof(TProperty), (value, instance) => isValidFunction((TProperty)value, (T)instance), _errorMessageAccessor));
        }
        [Obsolete("Use the MatchesInstanceRule(Func<TProperty, T, bool> isValidFunction, Func<string> errorMessageAccessor = null) method instead.")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected TBuilder MatchesInstanceRuleCore(Func<T, bool> isValidFunction, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new CustomInstanceValidationAttribute(typeof(T), (value, instance) => isValidFunction((T)instance),
                errorMessageAccessor == null ? null : new DXValidationAttribute.ErrorMessageAccessorDelegate((x, y) => errorMessageAccessor()) ));
        }
        protected static Func<TProperty, string> GetErrorMessageAccessor(Func<string> errorMessageAccessor) {
            if(errorMessageAccessor == null) return null;
            return x => errorMessageAccessor();
        }
    }
}