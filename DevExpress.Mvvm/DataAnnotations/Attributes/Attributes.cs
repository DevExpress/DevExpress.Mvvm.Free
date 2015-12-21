using System;

namespace DevExpress.Mvvm.DataAnnotations {
    public abstract class OrderAttribute : Attribute {
        int? order;
        public int Order {
            get {
                if(order == null)
                    throw new InvalidOperationException();
                return order.Value;
            }
            set {
                order = value;
            }
        }
        public int? GetOrder() {
            return order;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class ToolBarItemAttribute : OrderAttribute {
        public string Page { get; set; }
        public string PageGroup { get; set; }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class ContextMenuItemAttribute : OrderAttribute {
        public string Group { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class CommandParameterAttribute : Attribute {
        public string CommandParameter { get; private set; }
        public CommandParameterAttribute(string commandParameter) {
            this.CommandParameter = commandParameter;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HiddenAttribute : Attribute {
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class InstanceInitializerAttributeBase : Attribute {
        readonly Func<object> createInstanceCallback;

        protected InstanceInitializerAttributeBase(Type type)
            : this(type, type.Name, null) {

        }
        protected InstanceInitializerAttributeBase(Type type, string name, Func<object> createInstanceCallback) {
            if(Object.ReferenceEquals(type, null))
                throw new ArgumentNullException("type");
            if(string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            this.Type = type;
            this.Name = name;
            this.createInstanceCallback = createInstanceCallback;
        }
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public virtual object CreateInstance() {
            return createInstanceCallback != null ? createInstanceCallback() : Activator.CreateInstance(Type);
        }
#if !NETFX_CORE
        public override object TypeId {
            get {
                return this;
            }
        }
#endif
    }
    public class InstanceInitializerAttribute : InstanceInitializerAttributeBase {
        public InstanceInitializerAttribute(Type type)
            : base(type) {
        }
        public InstanceInitializerAttribute(Type type, string name)
            : base(type, name, null) {
        }
        internal InstanceInitializerAttribute(Type type, string name, Func<object> createInstanceCallback)
            : base(type, name, createInstanceCallback) {
        }
    }
    public class NewItemInstanceInitializerAttribute : InstanceInitializerAttributeBase {
        public NewItemInstanceInitializerAttribute(Type type)
            : base(type) {
        }
        public NewItemInstanceInitializerAttribute(Type type, string name)
            : base(type, name, null) {
        }
        internal NewItemInstanceInitializerAttribute(Type type, string name, Func<object> createInstanceCallback)
            : base(type, name, createInstanceCallback) {
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ScaffoldDetailCollectionAttribute : Attribute {
        public const bool DefaultScaffold = true;
        public ScaffoldDetailCollectionAttribute()
            : this(DefaultScaffold) {
        }
        public ScaffoldDetailCollectionAttribute(bool scaffold) {
            this.Scaffold = scaffold;
        }
        public bool Scaffold { get; private set; }
    }
}