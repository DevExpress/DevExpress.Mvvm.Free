using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace DevExpress.Mvvm.UI.Interactivity {
    public abstract class AttachableCollection<T> : FreezableCollection<T>, IAttachableObject where T : DependencyObject, IAttachableObject {

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
            if(!InteractionHelper.IsInDesignMode(AssociatedObject))
                return true;
            return !InteractionHelper.IsInDesignMode(item);
        }
        void VerifyRead() {
            ReadPreamble();
        }
        void VerifyWrite() {
            WritePreamble();
        }
        void NotifyChanged() {
            WritePostscript();
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
            snapshot.Insert(IndexOf(item), item);
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

        protected override sealed Freezable CreateInstanceCore() {
            return (Freezable)Activator.CreateInstance(GetType());
        }
    }

    public sealed class BehaviorCollection : AttachableCollection<Behavior> { }
    public sealed class TriggerCollection : AttachableCollection<TriggerBase> { }
}