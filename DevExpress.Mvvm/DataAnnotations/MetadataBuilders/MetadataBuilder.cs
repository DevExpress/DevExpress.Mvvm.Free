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
    public class MetadataBuilder<T> : IAttributesProvider {
        internal static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression) {
            return ExpressionHelper.GetArgumentPropertyStrict(expression).Name;
        }
        internal static MethodInfo GetMethod(Expression<Action<T>> expression) {
            return ExpressionHelper.GetArgumentMethodStrict(expression);
        }
        Dictionary<string, PropertyMetadataStorage> storages = new Dictionary<string, PropertyMetadataStorage>();

        public MetadataBuilder() { }
        IEnumerable<Attribute> IAttributesProvider.GetAttributes(string propertyName) {
            PropertyMetadataStorage storage;
            storages.TryGetValue(propertyName, out storage);
            return storage != null ? storage.GetAttributes() : null;
        }
        public PropertyMetadataBuilder<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
            return GetBuilder(propertyExpression, x => new PropertyMetadataBuilder<T, TProperty>(x, this));
        }
        public CommandMethodMetadataBuilder<T> CommandFromMethod(Expression<Action<T>> methodExpression) {
            return CommandFromMethodCore(methodExpression).AddOrModifyAttribute<CommandAttribute>();
        }
        internal CommandMethodMetadataBuilder<T> CommandFromMethodCore(Expression<Action<T>> methodExpression) {
            string methodName = GetMethod(methodExpression).Name;
            return GetBuilder<ICommand, CommandMethodMetadataBuilder<T>>(methodName, x => new CommandMethodMetadataBuilder<T>(x, this, methodName));
        }
        TBuilder GetBuilder<TProperty, TBuilder>(Expression<Func<T, TProperty>> propertyExpression, Func<PropertyMetadataStorage, TBuilder> createBuilderCallBack) where TBuilder : IPropertyMetadataBuilder {
            return GetBuilder<TProperty, TBuilder>(GetPropertyName(propertyExpression), createBuilderCallBack);
        }
        TBuilder GetBuilder<TProperty, TBuilder>(string memberName, Func<PropertyMetadataStorage, TBuilder> createBuilderCallBack) where TBuilder : IPropertyMetadataBuilder {
            PropertyMetadataStorage storage = storages.GetOrAdd(memberName, () => new PropertyMetadataStorage());
            return (TBuilder)createBuilderCallBack(storage);
        }
    }
}