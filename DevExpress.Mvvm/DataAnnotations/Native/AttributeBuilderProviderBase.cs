using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace DevExpress.Mvvm.Native {
    public interface IAttributeBuilderInternal {
        void AddOrReplaceAttribute<TAttribute>(TAttribute attribute) where TAttribute : Attribute;
        void AddOrModifyAttribute<TAttribute>(Action<TAttribute> setAttributeValue = null) where TAttribute : Attribute, new();
    }
    public interface IAttributeBuilderInternal<TBuilder> {
        TBuilder AddOrReplaceAttribute<TAttribute>(TAttribute attribute) where TAttribute : Attribute;
        TBuilder AddOrModifyAttribute<TAttribute>(Action<TAttribute> setAttributeValue = null) where TAttribute : Attribute, new();
    }

    public interface ICustomAttributeBuilderProvider {
        Type AttributeType { get; }
        CustomAttributeBuilder CreateAttributeBuilder(Attribute attribute);
    }
    public abstract class CustomAttributeBuilderProviderBase : ICustomAttributeBuilderProvider {
        Type ICustomAttributeBuilderProvider.AttributeType { get { return AttributeType; } }
        protected abstract Type AttributeType { get; }
        CustomAttributeBuilder ICustomAttributeBuilderProvider.CreateAttributeBuilder(Attribute attribute) {
            var pairs = GetPropertyValuePairsCore(attribute);
            return new CustomAttributeBuilder(ExpressionHelper.GetConstructorCore(GetConstructorExpressionCore()), GetConstructorParametersCore(attribute).ToArray(), pairs.Select(x => x.Item1).ToArray(), pairs.Select(x => x.Item2).ToArray());
        }
        internal virtual IEnumerable<object> GetConstructorParametersCore(Attribute attribute) {
            yield break;
        }
        internal virtual IEnumerable<Tuple<PropertyInfo, object>> GetPropertyValuePairsCore(Attribute attribute) {
            yield break;
        }
        internal abstract LambdaExpression GetConstructorExpressionCore();
    }
    public abstract class CustomAttributeBuilderProviderBase<T> : CustomAttributeBuilderProviderBase where T : Attribute {
        protected sealed override Type AttributeType { get { return typeof(T); } }
        internal sealed override IEnumerable<Tuple<PropertyInfo, object>> GetPropertyValuePairsCore(Attribute attribute) {
            return GetPropertyValuePairs((T)attribute);
        }
        internal sealed override LambdaExpression GetConstructorExpressionCore() {
            return GetConstructorExpression();
        }
        internal sealed override IEnumerable<object> GetConstructorParametersCore(Attribute attribute) {
            return GetConstructorParameters((T)attribute);
        }

        protected Tuple<PropertyInfo, object> GetPropertyValuePair<TAttribute, TProperty>(TAttribute attribute, Expression<Func<TAttribute, TProperty>> propertyExpression) {
            return DataAnnotationsAttributeHelper.GetPropertyValuePair(attribute, propertyExpression);
        }
        internal virtual IEnumerable<object> GetConstructorParameters(T attribute) {
            yield break;
        }
        internal virtual IEnumerable<Tuple<PropertyInfo, object>> GetPropertyValuePairs(T attribute) {
            yield break;
        }
        internal abstract Expression<Func<T>> GetConstructorExpression();
    }

    class BrowsableAttributeBuilderProvider : CustomAttributeBuilderProviderBase<BrowsableAttribute> {
        internal override Expression<Func<BrowsableAttribute>> GetConstructorExpression() {
            Expression<Func<BrowsableAttribute>> expression = () => new BrowsableAttribute(true);
            return expression;
        }
        internal override IEnumerable<object> GetConstructorParameters(BrowsableAttribute attribute) {
            return new List<object>() { attribute.Browsable };
        }
    }
    class DisplayAttributeBuilderProvider : CustomAttributeBuilderProviderBase {
        protected override Type AttributeType {
            get { return DataAnnotationsAttributeHelper.GetDisplayAttributeType(); }
        }
        internal override LambdaExpression GetConstructorExpressionCore() {
            return DataAnnotationsAttributeHelper.GetDisplayAttributeCreateExpression();
        }
        internal override IEnumerable<Tuple<PropertyInfo, object>> GetPropertyValuePairsCore(Attribute attribute) {
            return DataAnnotationsAttributeHelper.GetDisplayAttributePropertyValuePairs(attribute);
        }
    }
    public class DisplayNameAttributeBuilderProvider : CustomAttributeBuilderProviderBase<DisplayNameAttribute> {
        internal override Expression<Func<DisplayNameAttribute>> GetConstructorExpression() {
            return () => new DisplayNameAttribute(default(string));
        }
        internal override IEnumerable<object> GetConstructorParameters(DisplayNameAttribute attribute) {
            yield return attribute.DisplayName;
        }
    }
    public class ScaffoldColumnAttributeBuilderProvider : CustomAttributeBuilderProviderBase {
        protected override Type AttributeType { get { return DataAnnotationsAttributeHelper.GetScaffoldColumnAttributeType(); } }
        internal override LambdaExpression GetConstructorExpressionCore() {
            return DataAnnotationsAttributeHelper.GetScaffoldColumnAttributeCreateExpression();
        }
        internal override IEnumerable<object> GetConstructorParametersCore(Attribute attribute) {
            return DataAnnotationsAttributeHelper.GetScaffoldColumnAttributeConstructorParameters(attribute);
        }
    }
}