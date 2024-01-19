using DevExpress.Mvvm.Native;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DevExpress.Mvvm.DataAnnotations {
    public abstract class PropertyMetadataBuilderGeneric<T, TProperty, TBuilder> :
        PropertyMetadataBuilderBase<T, TProperty, TBuilder>
        where TBuilder : PropertyMetadataBuilderGeneric<T, TProperty, TBuilder> {
        internal PropertyMetadataBuilderGeneric(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent)
            : base(storage, parent) {
        }
        public MetadataBuilder<T> EndProperty() { return (MetadataBuilder<T>)parent; }

        public TBuilder Required(bool allowEmptyStrings = false, Func<string> errorMessageAccessor = null) { return RequiredCore(allowEmptyStrings, errorMessageAccessor); }
        public TBuilder Required(Func<string> errorMessageAccessor) { return RequiredCore(errorMessageAccessor); }
        public TBuilder MaxLength(int maxLength, Func<string> errorMessageAccessor = null) { return MaxLengthCore(maxLength, GetErrorMessageAccessor(errorMessageAccessor)); }
        public TBuilder MaxLength(int maxLength, Func<TProperty, string> errorMessageAccessor) { return MaxLengthCore(maxLength, errorMessageAccessor); }
        public TBuilder MinLength(int minLength, Func<string> errorMessageAccessor = null) { return MinLengthCore(minLength, GetErrorMessageAccessor(errorMessageAccessor)); }
        public TBuilder MinLength(int minLength, Func<TProperty, string> errorMessageAccessor) { return MinLengthCore(minLength, errorMessageAccessor); }
        public TBuilder MatchesRegularExpression(string pattern, Func<string> errorMessageAccessor = null) { return MatchesRegularExpressionCore(pattern, GetErrorMessageAccessor(errorMessageAccessor)); }
        public TBuilder MatchesRegularExpression(string pattern, Func<TProperty, string> errorMessageAccessor) { return MatchesRegularExpressionCore(pattern, errorMessageAccessor); }
        public TBuilder MatchesRule(Func<TProperty, bool> isValidFunction, Func<string> errorMessageAccessor = null) { return MatchesRuleCore(isValidFunction, GetErrorMessageAccessor(errorMessageAccessor)); }
        public TBuilder MatchesRule(Func<TProperty, bool> isValidFunction, Func<TProperty, string> errorMessageAccessor) { return MatchesRuleCore(isValidFunction, errorMessageAccessor); }
        public TBuilder MatchesInstanceRule(Func<TProperty, T, bool> isValidFunction, Func<string> errorMessageAccessor = null) { return MatchesInstanceRuleCore(isValidFunction, errorMessageAccessor == null ? null : new Func<TProperty, T, string>((x,y) => errorMessageAccessor())); }
        public TBuilder MatchesInstanceRule(Func<TProperty, T, bool> isValidFunction, Func<TProperty, T, string> errorMessageAccessor) { return MatchesInstanceRuleCore(isValidFunction, errorMessageAccessor); }
        [Obsolete("Use the MatchesInstanceRule(Func<TProperty, T, bool> isValidFunction, Func<string> errorMessageAccessor = null) method instead.")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public TBuilder MatchesInstanceRule(Func<T, bool> isValidFunction, Func<string> errorMessageAccessor = null) { return MatchesInstanceRuleCore(isValidFunction, errorMessageAccessor); }


        #region POCO
        public TBuilder DoNotMakeBindable() {
            return AddOrReplaceAttribute(new BindablePropertyAttribute(false));
        }
        public TBuilder MakeBindable() {
            return AddOrReplaceAttribute(new BindablePropertyAttribute());
        }
        public TBuilder OnPropertyChangedCall(Expression<Action<T>> onPropertyChangedExpression) {
            return AddOrModifyAttribute<BindablePropertyAttribute>(x => x.OnPropertyChangedMethod = ExpressionHelper.GetArgumentMethodStrict(onPropertyChangedExpression));
        }
        public TBuilder OnPropertyChangingCall(Expression<Action<T>> onPropertyChangingExpression) {
            return AddOrModifyAttribute<BindablePropertyAttribute>(x => x.OnPropertyChangingMethod = ExpressionHelper.GetArgumentMethodStrict(onPropertyChangingExpression));
        }
        public TBuilder ReturnsService(ServiceSearchMode searchMode = default(ServiceSearchMode)) {
            return ReturnsService(null, searchMode);
        }
        public TBuilder ReturnsService(string key, ServiceSearchMode searchMode = default(ServiceSearchMode)) {
            return AddOrReplaceAttribute(new ServicePropertyAttribute() { SearchMode = searchMode, Key = key });
        }
        public TBuilder DoesNotReturnService() {
            return AddOrReplaceAttribute(new ServicePropertyAttribute(false));
        }
        public TBuilder DependsOn(params Expression<Func<T, object>>[] propertyExpression) {
            foreach(var exp in propertyExpression) {
                var propName = ExpressionHelper.GetArgumentPropertyStrict(exp).Name;
                AddAttribute(new DependsOnPropertiesAttribute(propName));
            }
            return (TBuilder)this;
        }
        #endregion
    }
    public class PropertyMetadataBuilder<T, TProperty> :
        PropertyMetadataBuilderGeneric<T, TProperty, PropertyMetadataBuilder<T, TProperty>> {
        internal PropertyMetadataBuilder(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent)
            : base(storage, parent) { }
    }
}