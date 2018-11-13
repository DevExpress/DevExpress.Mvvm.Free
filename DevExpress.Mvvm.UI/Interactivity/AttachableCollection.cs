using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Linq;

#if NETFX_CORE
using Windows.UI.Xaml;
using System.Reflection;
using Windows.Foundation.Collections;
#endif

namespace DevExpress.Mvvm.UI.Interactivity {
#if !NETFX_CORE
    public abstract class AttachableCollection<T> : FreezableCollection<T>, IAttachableObject where T : DependencyObject, IAttachableObject {
#else
    public abstract class AttachableCollection<T> : DependencyObjectCollection<T>, IAttachableObject where T : DependencyObject, IAttachableObject {
#endif

        internal AttachableCollection() {
            ((INotifyCollectionChanged)this).CollectionChanged += this.OnCollectionChanged;
        }


        public void Attach(DependencyObject obj) {
            if(obj == this.AssociatedObject) return;
            if(AssociatedObject != null)
                throw new InvalidOperationException("This AttachableCollection is already attached");
            AssociatedObject = obj;
            OnAttached();
        }
        public void Detach() {
            OnDetaching();
            AssociatedObject = null;
        }

        DependencyObject _associatedObject;
        DependencyObject IAttachableObject.AssociatedObject { get { return AssociatedObject; } }
        protected internal DependencyObject AssociatedObject {
            get {
                VerifyRead();
                return _associatedObject;
            }
            private set {
                VerifyWrite();
                _associatedObject = value;
                NotifyChanged();
            }
        }
        protected virtual void OnAttached() {
            foreach(T item in this) {
                if(ShouldAttachItem(item))
                    item.Attach(AssociatedObject);
            }
        }
        protected virtual void OnDetaching() {
            foreach(T item in this)
                item.Detach();
        }

        bool ShouldAttachItem(T item) {
#if !NETFX_CORE
            if(InteractionHelper.GetEnableBehaviorsInDesignTime(AssociatedObject))
                return true;
#endif
            if(!InteractionHelper.IsInDesignMode(AssociatedObject))
                return true;
            return !InteractionHelper.IsInDesignMode(item);
        }
        void VerifyRead() {
#if !NETFX_CORE
            ReadPreamble();
#endif
        }
        void VerifyWrite() {
#if !NETFX_CORE
            WritePreamble();
#endif
        }
        void NotifyChanged() {
#if !NETFX_CORE
            WritePostscript();
#endif
        }

        internal virtual void ItemAdded(T item) {
            if(ShouldAttachItem(item))
                AssociatedObject.Do(x => item.Attach(x));
        }
        internal virtual void ItemRemoved(T item) {
            if(item.AssociatedObject == null) return;
            item.Detach();
        }
        List<T> snapshot = new List<T>();
        void AddItem(T item) {
            if(snapshot.Contains(item))
                return;
            ItemAdded(item);
            snapshot.Insert(this.IndexOf(item), item);
        }
        void RemoveItem(T item) {
            ItemRemoved(item);
            snapshot.Remove(item);
        }
        void ClearItems() {
            foreach(T item in snapshot)
                ItemRemoved(item);
            snapshot.Clear();
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(e.Action == NotifyCollectionChangedAction.Move)
                return;

            if(e.Action == NotifyCollectionChangedAction.Reset) {
                ClearItems();
                foreach(T item in this)
                    AddItem(item);
                return;
            }
            if(e.OldItems != null) {
                foreach(T item in e.OldItems)
                    RemoveItem(item);
            }
            if(e.NewItems != null) {
                foreach(T item in e.NewItems)
                    AddItem(item);
            }
        }

#if !NETFX_CORE
        protected override sealed Freezable CreateInstanceCore() {
            return (Freezable)Activator.CreateInstance(GetType());
        }
#endif
    }

    public sealed class BehaviorCollection : AttachableCollection<Behavior> {
        internal override void ItemAdded(Behavior item) {
            CheckBehavior(item);
            base.ItemAdded(item);
        }
        void CheckBehavior(Behavior item) {
            var itemType = UniqueBehaviorTypeAttribute.GetDeclaredType(item.GetType());
            if(itemType != null) {
                var existBehaviorName =
#if NETFX_CORE
                this.FirstOrDefault(x => x != item && itemType.GetTypeInfo().IsAssignableFrom(x.GetType().GetTypeInfo())).With(x => x.GetType().Name);
#else
                this.FirstOrDefault(x => x != item && itemType.IsAssignableFrom(x.GetType())).With(x => x.GetType().Name);
#endif
                if(!string.IsNullOrEmpty(existBehaviorName)) {
                    throw new InvalidOperationException(string.Format("A behavior of the {0} base type already exists.", existBehaviorName));
                }
            }
        }
    }
    public sealed class TriggerCollection : AttachableCollection<TriggerBase> { }
}