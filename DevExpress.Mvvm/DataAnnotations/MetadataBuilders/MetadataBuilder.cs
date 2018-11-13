using DevExpress.Mvvm.Native;
using System;
using System.Linq.Expressions;
using System.Windows.Input;
using System.ComponentModel;

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
#if !FREE
        protected internal FilteringPropertyMetadataBuilder<T, TProperty> FilteringPropertyCore<TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
            return GetBuilder(propertyExpression, x => new FilteringPropertyMetadataBuilder<T, TProperty>(x, this));
        }
#endif
        protected internal CommandMethodMetadataBuilder<T> CommandFromMethodCore(Expression<Action<T>> methodExpression) {
            return CommandFromMethodInternal(methodExpression).AddOrModifyAttribute<CommandAttribute>();
        }
        internal CommandMethodMetadataBuilder<T> CommandFromMethodInternal(Expression<Action<T>> methodExpression) {
            string methodName = GetMethod(methodExpression).Name;
            return GetBuilder(methodName, x => new CommandMethodMetadataBuilder<T>(x, this, methodName));
        }
#if !FREE
        protected internal CommandMetadataBuilder<T> CommandCore(Expression<Func<T, ICommand>> propertyExpression) {
            return GetBuilder(propertyExpression, x => new CommandMetadataBuilder<T>(x, this));
        }

        protected TableGroupContainerLayoutBuilder<T> TableLayoutCore() {
            return new TableGroupContainerLayoutBuilder<T>(this);
        }
        protected ToolBarLayoutBuilder<T> ToolBarLayoutCore() {
            return new ToolBarLayoutBuilder<T>(this);
        }

        internal int CurrentDisplayAttributeOrder { get; set; }
        internal int CurrentDataFormLayoutOrder { get; set; }
        internal int CurrentTableLayoutOrder { get; set; }
        internal int CurrentToolbarLayoutOrder { get; set; }
        internal int CurrentContextMenuLayoutOrder { get; set; }
#endif
        protected static TBuilder DisplayNameCore<TBuilder>(TBuilder builder, string name) where TBuilder : ClassMetadataBuilder<T> {
            builder.AddOrReplaceAttribute(new DisplayNameAttribute(name));
            return builder;
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
#if !FREE
        public CommandMetadataBuilder<T> Command(Expression<Func<T, ICommand>> propertyExpression) {
            return CommandCore(propertyExpression);
        }

        public DataFormLayoutBuilder<T, MetadataBuilder<T>> DataFormLayout() {
            return new DataFormLayoutBuilder<T, MetadataBuilder<T>>(this);
        }
        public TableGroupContainerLayoutBuilder<T> TableLayout() { return TableLayoutCore(); }
        public ToolBarLayoutBuilder<T> ToolBarLayout() { return ToolBarLayoutCore(); }
        public GroupBuilder<T, MetadataBuilder<T>> Group(string groupName) {
            return new GroupBuilder<T, MetadataBuilder<T>>(this, groupName);
        }
#endif
        public MetadataBuilder<T> DisplayName(string name) { return DisplayNameCore(this, name); }
    }
}
