using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;

namespace DevExpress.Mvvm.DataAnnotations {
    public interface IMetadataProvider<T> {
        void BuildMetadata(MetadataBuilder<T> builder);
    }
    public abstract class MetadataBuilderBase<T> : IAttributesProvider {
        Dictionary<string, MemberMetadataStorage> storages = new Dictionary<string, MemberMetadataStorage>();
        IEnumerable<Attribute> IAttributesProvider.GetAttributes(string propertyName) {
            MemberMetadataStorage storage;
            storages.TryGetValue(propertyName, out storage);
            return storage != null ? storage.GetAttributes() : null;
        }
        internal TBuilder GetBuilder<TBuilder>(string memberName, Func<MemberMetadataStorage, TBuilder> createBuilderCallBack) where TBuilder : IPropertyMetadataBuilder {
            MemberMetadataStorage storage = storages.GetOrAdd(memberName, () => new MemberMetadataStorage());
            return (TBuilder)createBuilderCallBack(storage);
        }
    }
    public class MetadataBuilder<T> : MetadataBuilderBase<T> {
        internal static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression) {
            return ExpressionHelper.GetArgumentPropertyStrict(expression).Name;
        }
        internal static MethodInfo GetMethod(Expression<Action<T>> expression) {
            return ExpressionHelper.GetArgumentMethodStrict(expression);
        }

        public MetadataBuilder() { }
        public PropertyMetadataBuilder<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
            return GetBuilder(propertyExpression, x => new PropertyMetadataBuilder<T, TProperty>(x, this));
        }
        public CommandMethodMetadataBuilder<T> CommandFromMethod(Expression<Action<T>> methodExpression) {
            return CommandFromMethodCore(methodExpression).AddOrModifyAttribute<CommandAttribute>();
        }
        internal CommandMethodMetadataBuilder<T> CommandFromMethodCore(Expression<Action<T>> methodExpression) {
            string methodName = GetMethod(methodExpression).Name;
            return GetBuilder(methodName, x => new CommandMethodMetadataBuilder<T>(x, this, methodName));
        }
        TBuilder GetBuilder<TProperty, TBuilder>(Expression<Func<T, TProperty>> propertyExpression, Func<MemberMetadataStorage, TBuilder> createBuilderCallBack) where TBuilder : IPropertyMetadataBuilder {
            return GetBuilder(GetPropertyName(propertyExpression), createBuilderCallBack);
        }
    }
}