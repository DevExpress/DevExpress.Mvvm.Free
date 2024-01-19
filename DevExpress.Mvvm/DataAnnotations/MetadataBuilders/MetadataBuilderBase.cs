using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace DevExpress.Mvvm.DataAnnotations {
    public abstract class MetadataBuilderBase<T, TMetadataBuilder> :
        IAttributesProvider,
        IAttributeBuilderInternal,
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

        protected internal TMetadataBuilder AddOrReplaceAttribute<TAttribute>(TAttribute attribute) where TAttribute : Attribute {
            GetBuilder<IPropertyMetadataBuilder>(null, x => {
                x.AddOrReplaceAttribute(attribute);
                return null;
            });
            return (TMetadataBuilder)this;
        }
        protected internal TMetadataBuilder AddOrModifyAttribute<TAttribute>(Action<TAttribute> setAttributeValue) where TAttribute : Attribute, new() {
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
        void IAttributeBuilderInternal.AddOrReplaceAttribute<TAttribute>(TAttribute attribute) {
            AddOrReplaceAttribute(attribute);
        }
        void IAttributeBuilderInternal.AddOrModifyAttribute<TAttribute>(Action<TAttribute> setAttributeValue) {
            AddOrModifyAttribute(setAttributeValue);
        }

        internal static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression) {
            return ExpressionHelper.GetArgumentPropertyStrict(expression).Name;
        }

    }
}