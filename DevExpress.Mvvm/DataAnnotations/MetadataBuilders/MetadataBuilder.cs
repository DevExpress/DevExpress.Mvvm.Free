using DevExpress.Mvvm.Native;
using System;
using System.Linq.Expressions;
using System.Windows.Input;

namespace DevExpress.Mvvm.DataAnnotations {
    public interface IMetadataProvider<T> {
        void BuildMetadata(MetadataBuilder<T> builder);
    }

    public abstract class ClassMetadataBuilder<T> : MetadataBuilderBase<T, ClassMetadataBuilder<T>> {
        TBuilder GetBuilder<TProperty, TBuilder>(Expression<Func<T, TProperty>> propertyExpression, Func<MemberMetadataStorage, TBuilder> createBuilderCallBack) where TBuilder : IPropertyMetadataBuilder {
            return GetBuilder(GetPropertyName(propertyExpression), createBuilderCallBack);
        }

        protected internal PropertyMetadataBuilder<T, TProperty> PropertyCore<TProperty>(string memberName) {
            return GetBuilder(memberName, x => new PropertyMetadataBuilder<T, TProperty>(x, this));
        }
        protected internal PropertyMetadataBuilder<T, TProperty> PropertyCore<TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
            return GetBuilder(propertyExpression, x => new PropertyMetadataBuilder<T, TProperty>(x, this));
        }
        protected internal CommandMethodMetadataBuilder<T> CommandFromMethodCore(Expression<Action<T>> methodExpression) {
            return CommandFromMethodInternal(methodExpression).AddOrModifyAttribute<CommandAttribute>();
        }
        internal CommandMethodMetadataBuilder<T> CommandFromMethodInternal(Expression<Action<T>> methodExpression) {
            string methodName = GetMethod(methodExpression).Name;
            return GetBuilder(methodName, x => new CommandMethodMetadataBuilder<T>(x, this, methodName));
        }
    }
    public class MetadataBuilder<T> : ClassMetadataBuilder<T> {
        public PropertyMetadataBuilder<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
            return PropertyCore(propertyExpression);
        }
        public PropertyMetadataBuilder<T, TProperty> Property<TProperty>(string propertyName) {
            return PropertyCore<TProperty>(propertyName);
        }
        public CommandMethodMetadataBuilder<T> CommandFromMethod(Expression<Action<T>> methodExpression) {
            return CommandFromMethodCore(methodExpression);
        }
    }
}