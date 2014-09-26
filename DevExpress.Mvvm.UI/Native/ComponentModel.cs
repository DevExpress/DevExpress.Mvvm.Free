using System.Linq;
using System.Collections.Generic;

namespace System.ComponentModel {

    public enum DesignerSerializationVisibility {
        Hidden,
        Visible,
        Content
    }

    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class DesignerSerializationVisibilityAttribute : Attribute {
        public static readonly DesignerSerializationVisibilityAttribute Content = new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content);
        public static readonly DesignerSerializationVisibilityAttribute Default = Visible;
        public static readonly DesignerSerializationVisibilityAttribute Hidden = new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden);
        public static readonly DesignerSerializationVisibilityAttribute Visible = new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible);

        private DesignerSerializationVisibility visibility;

        public DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility visibility) {
            this.visibility = visibility;
        }

        public DesignerSerializationVisibility Visibility { get { return this.visibility; } }

        public override bool Equals(object obj) {
            if(obj == this) {
                return true;
            }
            DesignerSerializationVisibilityAttribute attribute = obj as DesignerSerializationVisibilityAttribute;
            return ((attribute != null) && (attribute.Visibility == this.visibility));
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    public delegate void CancelEventHandler(object sender, CancelEventArgs e);

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotifyParentPropertyAttribute : Attribute {
        public static readonly NotifyParentPropertyAttribute Default = No;
        public static readonly NotifyParentPropertyAttribute No = new NotifyParentPropertyAttribute(false);
        public static readonly NotifyParentPropertyAttribute Yes = new NotifyParentPropertyAttribute(true);
        bool notifyParent;

        public NotifyParentPropertyAttribute(bool notifyParent) {
            this.notifyParent = notifyParent;
        }

        public bool NotifyParent { get { return this.notifyParent; } }

        public override bool Equals(object obj) {
            return ((obj == this) || (((obj != null) && (obj is NotifyParentPropertyAttribute)) && (((NotifyParentPropertyAttribute)obj).NotifyParent == this.notifyParent)));
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
    public enum RefreshProperties {
        None,
        All,
        Repaint
    }
    [AttributeUsage(AttributeTargets.All)]
    public sealed class RefreshPropertiesAttribute : Attribute {
        public static readonly RefreshPropertiesAttribute All = new RefreshPropertiesAttribute(RefreshProperties.All);
        public static readonly RefreshPropertiesAttribute Default = new RefreshPropertiesAttribute(RefreshProperties.None);
        public static readonly RefreshPropertiesAttribute Repaint = new RefreshPropertiesAttribute(RefreshProperties.Repaint);
        RefreshProperties refresh;

        public RefreshPropertiesAttribute(RefreshProperties refresh) {
            this.refresh = refresh;
        }

        public RefreshProperties RefreshProperties { get { return this.refresh; } }

        public override bool Equals(object value) {
            return ((value is RefreshPropertiesAttribute) && (((RefreshPropertiesAttribute)value).RefreshProperties == this.refresh));
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public sealed class LocalizableAttribute : Attribute {
        public static readonly LocalizableAttribute Default = No;
        public static readonly LocalizableAttribute No = new LocalizableAttribute(false);
        public static readonly LocalizableAttribute Yes = new LocalizableAttribute(true);

        bool isLocalizable;

        public LocalizableAttribute(bool isLocalizable) {
            this.isLocalizable = isLocalizable;
        }

        public bool IsLocalizable { get { return this.isLocalizable; } }

        public override bool Equals(object obj) {
            LocalizableAttribute attribute = obj as LocalizableAttribute;
            return ((attribute != null) && (attribute.IsLocalizable == this.isLocalizable));
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ListBindableAttribute : Attribute {
        public static readonly ListBindableAttribute Default = Yes;
        public static readonly ListBindableAttribute No = new ListBindableAttribute(false);
        public static readonly ListBindableAttribute Yes = new ListBindableAttribute(true);
        bool isDefault;
        bool listBindable;

        public bool ListBindable { get { return this.listBindable; } }

        public ListBindableAttribute(bool listBindable) {
            this.listBindable = listBindable;
        }

        public ListBindableAttribute(BindableSupport flags) {
            this.listBindable = flags != BindableSupport.No;
            this.isDefault = flags == BindableSupport.Default;
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            ListBindableAttribute attribute = obj as ListBindableAttribute;
            return ((attribute != null) && (attribute.ListBindable == this.listBindable));
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    public class ToolboxItemAttribute : Attribute {
        public ToolboxItemAttribute(bool defaultType) { }
        public ToolboxItemAttribute(string name) { }
    }

    public class AttributeCollection : IEnumerable<Attribute> {
        public static readonly AttributeCollection Empty;

        List<Attribute> attributes;

        static AttributeCollection() {
            Empty = new AttributeCollection(null);
        }

        public AttributeCollection() { }
        public AttributeCollection(System.Collections.IEnumerable attributes) {
            if(attributes != null)
                this.attributes = new List<Attribute>(attributes.Cast<Attribute>());
            else
                this.attributes = new List<Attribute>();
        }

        public virtual Attribute this[Type type] { get { return Find(type); } }
        public int Count { get { return List.Count; } }

        protected virtual List<Attribute> List { get { return attributes; } }

        Attribute Find(Type type) {
            foreach(Attribute attribute in attributes) {
                if(type.IsAssignableFrom(attribute.GetType()))
                    return attribute;
            }
            return null;
        }

        public void CopyTo(Attribute[] array, int index) {
            List.CopyTo(array, index);
        }

        public static AttributeCollection FromExisting(AttributeCollection existing, params Attribute[] newAttributes) {
            if(existing == null) {
                throw new ArgumentNullException("existing");
            }
            if(newAttributes == null) {
                newAttributes = new Attribute[0];
            }
            Attribute[] array = new Attribute[existing.Count + newAttributes.Length];
            int count = existing.Count;
            existing.CopyTo(array, 0);
            for(int i = 0; i < newAttributes.Length; i++) {
                if(newAttributes[i] == null) {
                    throw new ArgumentNullException("newAttributes");
                }
                bool flag = false;
                for(int j = 0; j < existing.Count; j++) {
                    if(array[j].GetType().Equals(newAttributes[i].GetType())) {
                        flag = true;
                        array[j] = newAttributes[i];
                        break;
                    }
                }
                if(!flag) {
                    array[count++] = newAttributes[i];
                }
            }
            Attribute[] destinationArray = null;
            if(count < array.Length) {
                destinationArray = new Attribute[count];
                Array.Copy(array, 0, destinationArray, 0, count);
            } else {
                destinationArray = array;
            }
            return new AttributeCollection(destinationArray);
        }
        public IEnumerator<Attribute> GetEnumerator() {
            return List.GetEnumerator();
        }

        protected Attribute GetDefaultAttribute(Type attributeType) {
            return null;
        }

        #region IEnumerable<Attribute> Members
        IEnumerator<Attribute> IEnumerable<Attribute>.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion
        #region IEnumerable Members
        Collections.IEnumerator Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion
    }
    [AttributeUsage(AttributeTargets.All)]
    public sealed class MergablePropertyAttribute : Attribute {
        bool allowMerge;
        public static readonly MergablePropertyAttribute Default;
        public static readonly MergablePropertyAttribute No;
        public static readonly MergablePropertyAttribute Yes;

        static MergablePropertyAttribute() {
            Yes = new MergablePropertyAttribute(true);
            No = new MergablePropertyAttribute(false);
            Default = Yes;

        }
        public MergablePropertyAttribute(bool allowMerge) {
            this.allowMerge = allowMerge;
        }

        public bool AllowMerge { get { return this.allowMerge; } }

        public override bool Equals(object obj) {
            if(obj == this) {
                return true;
            }
            MergablePropertyAttribute attribute = obj as MergablePropertyAttribute;
            return ((attribute != null) && (attribute.AllowMerge == this.allowMerge));

        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }


}

namespace System.ComponentModel.Design {
    public interface IServiceContainer : IServiceProvider {
        void AddService(Type serviceType, object serviceInstance);
        void AddService(Type serviceType, ServiceCreatorCallback callback);
        void AddService(Type serviceType, object serviceInstance, bool promote);
        void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote);
        void RemoveService(Type serviceType);
        void RemoveService(Type serviceType, bool promote);
    }
    public delegate object ServiceCreatorCallback(IServiceContainer container, Type serviceType);
}