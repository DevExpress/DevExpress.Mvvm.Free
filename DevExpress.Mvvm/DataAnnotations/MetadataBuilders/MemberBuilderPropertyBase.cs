using DevExpress.Mvvm.Native;
using System;
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
        protected TBuilder MaxLengthCore(int maxLength, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new DXMaxLengthAttribute(maxLength, errorMessageAccessor));
        }
        protected TBuilder MinLengthCore(int minLength, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new DXMinLengthAttribute(minLength, errorMessageAccessor));
        }
        protected TBuilder MatchesRegularExpressionCore(string pattern, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new RegularExpressionAttribute(pattern, errorMessageAccessor));
        }
        protected TBuilder MatchesRuleCore(Func<TProperty, bool> isValidFunction, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new CustomValidationAttribute(x => isValidFunction((TProperty)x), errorMessageAccessor));
        }
        [Obsolete("Use the MatchesInstanceRule(Func<TProperty, T, bool> isValidFunction, Func<string> errorMessageAccessor = null) method instead.")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected TBuilder MatchesInstanceRuleCore(Func<T, bool> isValidFunction, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new CustomInstanceValidationAttribute((value, instance) => isValidFunction((T)instance), errorMessageAccessor));
        }
        protected TBuilder MatchesInstanceRuleCore(Func<TProperty, T, bool> isValidFunction, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new CustomInstanceValidationAttribute((value, instance) => isValidFunction((TProperty)value, (T)instance), errorMessageAccessor));
        }
    }
}