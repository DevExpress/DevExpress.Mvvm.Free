using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DevExpress.Mvvm.DataAnnotations {

    public abstract class MemberMetadataBuilderBase<T, TBuilder, TParent> : IPropertyMetadataBuilder, IAttributeBuilderInternal<TBuilder>
        where TBuilder : MemberMetadataBuilderBase<T, TBuilder, TParent>
        where TParent : MetadataBuilderBase<T> {

        readonly MemberMetadataStorage storage;
        protected internal readonly TParent parent;

        internal MemberMetadataBuilderBase(MemberMetadataStorage storage, TParent parent) {
            this.storage = storage;
            this.parent = parent;
        }
        internal TBuilder AddOrModifyAttribute<TAttribute>(Action<TAttribute> setAttributeValue = null) where TAttribute : Attribute, new() {
            storage.AddOrModifyAttribute(setAttributeValue);
            return (TBuilder)this;
        }
        internal TBuilder AddOrReplaceAttribute<TAttribute>(TAttribute attribute) where TAttribute : Attribute {
            storage.AddOrReplaceAttribute(attribute);
            return (TBuilder)this;
        }
        TBuilder IAttributeBuilderInternal<TBuilder>.AddOrReplaceAttribute<TAttribute>(TAttribute attribute) {
            return AddOrReplaceAttribute(attribute);
        }
        TBuilder IAttributeBuilderInternal<TBuilder>.AddOrModifyAttribute<TAttribute>(Action<TAttribute> setAttributeValue) {
            return AddOrModifyAttribute(setAttributeValue);
        }
        internal TBuilder AddAttribute(Attribute attribute) {
            storage.AddAttribute(attribute);
            return (TBuilder)this;
        }
        IEnumerable<Attribute> IPropertyMetadataBuilder.Attributes {
            get { return storage.GetAttributes(); }
        }
        protected TBuilder ImageUriCore(string imageUri) {
            return AddOrModifyAttribute<ImageAttribute>(x => x.ImageUri = imageUri);
        }
    }

    public abstract class PropertyMetadataBuilderBase<T, TProperty, TBuilder> : MemberMetadataBuilderBase<T, TBuilder, MetadataBuilder<T>> where TBuilder : PropertyMetadataBuilderBase<T, TProperty, TBuilder> {
        internal PropertyMetadataBuilderBase(MemberMetadataStorage storage, MetadataBuilder<T> parent)
            : base(storage, parent) {
        }
    }
    public class PropertyMetadataBuilder<T, TProperty> : PropertyMetadataBuilderBase<T, TProperty, PropertyMetadataBuilder<T, TProperty>> {
        internal PropertyMetadataBuilder(MemberMetadataStorage storage, MetadataBuilder<T> parent)
            : base(storage, parent) {
        }
        public PropertyMetadataBuilder<T, TProperty> Required(bool allowEmptyStrings = false, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new DXRequiredAttribute(allowEmptyStrings, errorMessageAccessor));
        }
        public PropertyMetadataBuilder<T, TProperty> Required(Func<string> errorMessageAccessor) {
            return Required(false, errorMessageAccessor);
        }
        public PropertyMetadataBuilder<T, TProperty> MaxLength(int maxLength, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new DXMaxLengthAttribute(maxLength, errorMessageAccessor));
        }
        public PropertyMetadataBuilder<T, TProperty> MinLength(int minLength, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new DXMinLengthAttribute(minLength, errorMessageAccessor));
        }
        public PropertyMetadataBuilder<T, TProperty> MatchesRegularExpression(string pattern, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new RegularExpressionAttribute(pattern, errorMessageAccessor));
        }
        public PropertyMetadataBuilder<T, TProperty> MatchesRule(Func<TProperty, bool> isValidFunction, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new CustomValidationAttribute(x => isValidFunction((TProperty)x), errorMessageAccessor));
        }
        public PropertyMetadataBuilder<T, TProperty> MatchesInstanceRule(Func<T, bool> isValidFunction, Func<string> errorMessageAccessor = null) {
            return AddOrReplaceAttribute(new CustomInstanceValidationAttribute(x => isValidFunction((T)x), errorMessageAccessor));
        }

        public MetadataBuilder<T> EndProperty() {
            return parent;
        }

#region POCO
        public PropertyMetadataBuilder<T, TProperty> DoNotMakeBindable() {
            return AddOrReplaceAttribute(new BindablePropertyAttribute(false));
        }
        public PropertyMetadataBuilder<T, TProperty> MakeBindable() {
            return AddOrReplaceAttribute(new BindablePropertyAttribute());
        }
        public PropertyMetadataBuilder<T, TProperty> OnPropertyChangedCall(Expression<Action<T>> onPropertyChangedExpression) {
            return AddOrModifyAttribute<BindablePropertyAttribute>(x => x.OnPropertyChangedMethod = MetadataBuilder<T>.GetMethod(onPropertyChangedExpression));
        }
        public PropertyMetadataBuilder<T, TProperty> OnPropertyChangingCall(Expression<Action<T>> onPropertyChangingExpression) {
            return AddOrModifyAttribute<BindablePropertyAttribute>(x => x.OnPropertyChangingMethod = MetadataBuilder<T>.GetMethod(onPropertyChangingExpression));
        }
        public PropertyMetadataBuilder<T, TProperty> ReturnsService(ServiceSearchMode searchMode = default(ServiceSearchMode)) {
            return ReturnsService(null, searchMode);
        }
        public PropertyMetadataBuilder<T, TProperty> ReturnsService(string key, ServiceSearchMode searchMode = default(ServiceSearchMode)) {
            return AddOrReplaceAttribute(new ServicePropertyAttribute() { SearchMode = searchMode, Key = key });
        }
        public PropertyMetadataBuilder<T, TProperty> DoesNotReturnService() {
            return AddOrReplaceAttribute(new ServicePropertyAttribute(false));
        }
#endregion
    }
}