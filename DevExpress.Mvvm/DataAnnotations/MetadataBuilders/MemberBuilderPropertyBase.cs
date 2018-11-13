using DevExpress.Mvvm.Native;
using System;
using System.ComponentModel;

namespace DevExpress.Mvvm.DataAnnotations {
#if !FREE
    public enum PropertyLocation {
        BeforePropertiesWithoutSpecifiedLocation,
        AfterPropertiesWithoutSpecifiedLocation
    }
#endif
    public abstract class PropertyMetadataBuilderBase<T, TProperty, TBuilder> :
        MemberMetadataBuilderBase<T, TBuilder, ClassMetadataBuilder<T>>
        where TBuilder : PropertyMetadataBuilderBase<T, TProperty, TBuilder> {
        internal PropertyMetadataBuilderBase(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent)
            : base(storage, parent) {
        }
#if !FREE
        internal TBuilder GroupName(string groupName, LayoutType layoutType) {
            if(layoutType == LayoutType.DataForm)
                return AddOrReplaceAttribute(new DataFormGroupAttribute(groupName, parent.CurrentDataFormLayoutOrder++));
            if(layoutType == LayoutType.Table)
                return AddOrReplaceAttribute(new TableGroupAttribute(groupName, parent.CurrentTableLayoutOrder++));
            throw new NotSupportedException();
        }

        protected TBuilder DefaultEditorCore(object templateKey) {
            return AddOrModifyAttribute<DefaultEditorAttribute>(x => x.TemplateKey = templateKey);
        }
        protected TBuilder GridEditorCore(object templateKey) {
            return AddOrModifyAttribute<GridEditorAttribute>(x => x.TemplateKey = templateKey);
        }
        protected TBuilder LayoutControlEditorCore(object templateKey) {
            return AddOrModifyAttribute<LayoutControlEditorAttribute>(x => x.TemplateKey = templateKey);
        }
        protected TBuilder PropertyGridEditorCore(object templateKey) {
            return AddOrModifyAttribute<PropertyGridEditorAttribute>(x => x.TemplateKey = templateKey);
        }

        protected TBuilder HiddenCore(bool hidden = true) {
            return AddOrModifyAttribute<HiddenAttribute>(x => x.Hidden = hidden);
        }
        protected TBuilder ReadOnlyCore() {
            return DataAnnotationsAttributeHelper.SetReadonly((TBuilder)this);
        }
        protected TBuilder NotEditableCore() {
            return DataAnnotationsAttributeHelper.SetNotEditable((TBuilder)this);
        }
        protected TBuilder InitializerCore<TValue>(Func<TValue> createDelegate, string name = null) {
            return InitializerCore(createDelegate, name, (t, n, c) => new InstanceInitializerAttribute(t, n, c));
        }
        internal TBuilder InitializerCore<TValue, TInstanceInitializerAttribute>(Func<TValue> createDelegate, string name, Func<Type, string, Func<object>, TInstanceInitializerAttribute> attributeFactory)
            where TInstanceInitializerAttribute : InstanceInitializerAttributeBase {
            return AddAttribute(attributeFactory(typeof(TValue), name ?? typeof(TValue).Name, () => createDelegate()));
        }
        protected TypeConverterBuilder<T, TProperty, TBuilder> TypeConverterCore() {
            return new TypeConverterBuilder<T, TProperty, TBuilder>((TBuilder)this);
        }
        protected TBuilder TypeConverterCore<TConverter>() where TConverter : TypeConverter, new() {
            return AddOrModifyAttribute<TypeConverterWrapperAttribute>(x => x.BaseConverterType = typeof(TConverter));
        }

        protected TBuilder DoNotConvertEmptyStringToNullCore() {
            return DataAnnotationsAttributeHelper.SetConvertEmptyStringToNull((TBuilder)this, false);
        }
        protected TBuilder NullDisplayTextCore(string nullDisplayText) {
            return DataAnnotationsAttributeHelper.SetNullDisplayText((TBuilder)this, nullDisplayText);
        }
        protected TBuilder DisplayFormatStringCore(string dataFormatString, bool applyDisplayFormatInEditMode = false) {
            return DataAnnotationsAttributeHelper.SetDataFormatString((TBuilder)this, dataFormatString, applyDisplayFormatInEditMode);
        }
#endif
        
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
