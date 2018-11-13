using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DevExpress.Mvvm.DataAnnotations {
    public abstract class MetadataBuilderBase<T, TMetadataBuilder> : 
        IAttributesProvider, 
        IAttributeBuilderInternal<TMetadataBuilder>
        where TMetadataBuilder : MetadataBuilderBase<T, TMetadataBuilder> {

        Dictionary<string, MemberMetadataStorage> storages = new Dictionary<string, MemberMetadataStorage>();
        IEnumerable<Attribute> IAttributesProvider.GetAttributes(string propertyName) {
            MemberMetadataStorage storage;
            storages.TryGetValue(propertyName ?? string.Empty, out storage);
            return storage != null ? storage.GetAttributes() : null;
        }
        internal TBuilder GetBuilder<TBuilder>(string memberName, Func<MemberMetadataStorage, TBuilder> createBuilderCallBack) where TBuilder : IPropertyMetadataBuilder {
            MemberMetadataStorage storage = storages.GetOrAdd(memberName ?? string.Empty, () => new MemberMetadataStorage());
            return (TBuilder)createBuilderCallBack(storage);
        }

        protected TMetadataBuilder AddOrReplaceAttribute<TAttribute>(TAttribute attribute) where TAttribute : Attribute {
            GetBuilder<IPropertyMetadataBuilder>(null, x => {
                x.AddOrReplaceAttribute(attribute);
                return null;
            });
            return (TMetadataBuilder)this;
        }
        protected TMetadataBuilder AddOrModifyAttribute<TAttribute>(Action<TAttribute> setAttributeValue) where TAttribute : Attribute, new() {
            GetBuilder<IPropertyMetadataBuilder>(null, x => {
                x.AddOrModifyAttribute(setAttributeValue);
                return null;
            });
            return (TMetadataBuilder)this;
        }
        TMetadataBuilder IAttributeBuilderInternal<TMetadataBuilder>.AddOrReplaceAttribute<TAttribute>(TAttribute attribute) {
            return AddOrReplaceAttribute(attribute);
        }
        TMetadataBuilder IAttributeBuilderInternal<TMetadataBuilder>.AddOrModifyAttribute<TAttribute>(Action<TAttribute> setAttributeValue) {
            return AddOrModifyAttribute(setAttributeValue);
        }

        internal static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression) {
            return ExpressionHelper.GetArgumentPropertyStrict(expression).Name;
        }
        internal static MethodInfo GetMethod(Expression<Action<T>> expression) {
            return ExpressionHelper.GetArgumentMethodStrict(expression);
        }

#if !FREE
        public TMetadataBuilder DefaultEditor(object templateKey) {
            IAttributeBuilderInternal<TMetadataBuilder> builder = this;
            return builder.AddOrModifyAttribute((DefaultEditorAttribute a) => a.TemplateKey = templateKey);
        }
        public TMetadataBuilder GridEditor(object templateKey) {
            IAttributeBuilderInternal<TMetadataBuilder> builder = this;
            return builder.AddOrModifyAttribute((GridEditorAttribute a) => a.TemplateKey = templateKey);
        }
        public TMetadataBuilder LayoutControlEditor(object templateKey) {
            IAttributeBuilderInternal<TMetadataBuilder> builder = this;
            return builder.AddOrModifyAttribute((LayoutControlEditorAttribute a) => a.TemplateKey = templateKey);
        }
        public TMetadataBuilder PropertyGridEditor(object templateKey) {
            IAttributeBuilderInternal<TMetadataBuilder> builder = this;
            return builder.AddOrModifyAttribute((PropertyGridEditorAttribute a) => a.TemplateKey = templateKey);
        }
#endif
    }
}
