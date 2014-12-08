using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace DevExpress.Mvvm.Native {
    public static class SyncCollectionHelper {
        class Disposable : IDisposable {
            Action disposeAction;

            public Disposable(Action disposeAction) { this.disposeAction = disposeAction; }
            public void Dispose() { disposeAction(); }
        }
        public static IDisposable TwoWayBind<TSource, TTarget>(IList<TTarget> target, IList<TSource> source, Func<TSource, TTarget> itemConverter, Func<TTarget, TSource> itemBackConverter) {
            if(target == null) throw new ArgumentNullException("target");
            if(source == null) throw new ArgumentNullException("source");
            bool doNotProcessSourceCollectionChanged = false;
            bool doNotProcessTargetCollectionChanged = false;
            NotifyCollectionChangedEventHandler onSourceCollectionChanged = (s, e) => OnCollectionChanged(source, target, e, ref doNotProcessSourceCollectionChanged, ref doNotProcessTargetCollectionChanged, itemConverter);
            NotifyCollectionChangedEventHandler onTargetCollectionChanged = (s, e) => OnCollectionChanged(target, source, e, ref doNotProcessTargetCollectionChanged, ref doNotProcessSourceCollectionChanged, itemBackConverter);
            INotifyCollectionChanged sourceNotify = source as INotifyCollectionChanged;
            if(sourceNotify != null)
                sourceNotify.CollectionChanged += onSourceCollectionChanged;
            INotifyCollectionChanged targetNotify = target as INotifyCollectionChanged;
            if(targetNotify != null)
                targetNotify.CollectionChanged += onTargetCollectionChanged;
            onSourceCollectionChanged(source, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            Action unbindAction = () => {
                if(sourceNotify != null)
                    sourceNotify.CollectionChanged -= onSourceCollectionChanged;
                if(targetNotify != null)
                    targetNotify.CollectionChanged -= onTargetCollectionChanged;
            };
            return new Disposable(unbindAction);
        }
        static void Add<TSource, TTarget>(NotifyCollectionChangedEventArgs e, IList<TTarget> target, Func<TSource, TTarget> itemConverter) {
            int itemIndex = e.NewStartingIndex;
            foreach(TSource item in e.NewItems) {
                target.Insert(itemIndex, itemConverter(item));
                ++itemIndex;
            }
        }
        static void Remove<T>(NotifyCollectionChangedEventArgs e, IList<T> collection) {
            for(int itemIndex = e.OldStartingIndex; itemIndex < e.OldStartingIndex + e.OldItems.Count; ++itemIndex) {
                collection.RemoveAt(itemIndex);
            }
        }
        static void Replace<TSource, TTarget>(NotifyCollectionChangedEventArgs e, IList<TTarget> target, Func<TSource, TTarget> itemConverter) {
            int itemIndex = e.NewStartingIndex;
            foreach(TSource item in e.NewItems) {
                target[itemIndex] = itemConverter(item);
                ++itemIndex;
            }
        }
        static void Reset<TSource, TTarget>(NotifyCollectionChangedEventArgs e, IList<TTarget> target, IList<TSource> source, Func<TSource, TTarget> itemConverter) {
            target.Clear();
            foreach(TSource item in source) {
                target.Add(itemConverter(item));
            }
        }
        static void OnCollectionChanged<TSource, TTarget>(IList<TSource> source, IList<TTarget> target, NotifyCollectionChangedEventArgs e, ref bool doNotProcessSourceCollectionChanged,
                ref bool doNotProcessTargetCollectionChanged, Func<TSource, TTarget> itemConverter) {
            if(doNotProcessSourceCollectionChanged) return;
            doNotProcessTargetCollectionChanged = true;
            try {
                switch(e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        if(source.Count != target.Count)
                            Add(e, target, itemConverter);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if(source.Count != target.Count)
                            Remove(e, target);
                        break;
#if !SILVERLIGHT
                    case NotifyCollectionChangedAction.Move:
                        Remove(e, target);
                        Add(e, target, itemConverter);
                        break;
#endif
                    case NotifyCollectionChangedAction.Replace:
                        Replace(e, target, itemConverter);
                        break;
                    default:
                        Reset(e, target, source, itemConverter);
                        break;
                }
            } finally {
                doNotProcessTargetCollectionChanged = false;
            }
        }

        public static void SyncCollection(
            NotifyCollectionChangedEventArgs e,
            IList target,
            IList source,
            Func<object, object> convertItemAction,
            Action<int, object> insertItemAction = null,
            ISupportInitialize supportInitialize = null,
            Action<object> clearItemAction = null) {

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    DoAction(e.NewItems, (item) => InsertItem(item, target, source, convertItemAction, insertItemAction));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    DoAction(e.OldItems, (item) => { RemoveItem(e.OldStartingIndex, target, clearItemAction); });
                    break;
                case NotifyCollectionChangedAction.Reset:
                    PopulateCore(target, source, convertItemAction, insertItemAction, supportInitialize);
                    break;
#if!SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
                    object insertItem = target[e.OldStartingIndex];
                    object insertItemAt = target[e.NewStartingIndex];

                    int deltaItem = e.NewStartingIndex > e.OldStartingIndex ? 1 : 0;

                    target.Remove(insertItem);
                    target.Insert(target.IndexOf(insertItemAt) + deltaItem, insertItem);
                    break;
#endif
                case NotifyCollectionChangedAction.Replace:
                    RemoveItem(e.NewStartingIndex, target, clearItemAction);
                    InsertItem(e.NewItems[0], target, source, convertItemAction, insertItemAction);
                    break;
            }
        }
        public static void PopulateCore(
            IList target,
            IEnumerable source,
            Func<object, object> convertItemAction,
            ISupportInitialize supportInitialize = null,
            Action<object> clearItemAction = null) {

            if (target == null) return;
            BeginPopulate(target, supportInitialize);
            try {
                var oldItems = target.OfType<object>().ToList();
                target.Clear();
                if (clearItemAction != null)
                    oldItems.ForEach(clearItemAction);
                if (source == null) return;
                DoAction(source, (item) => AddItem(item, target, convertItemAction));
            } finally {
                EndPopulate(target, supportInitialize);
            }
        }
        public static void PopulateCore(
            IList target,
            IList source,
            Func<object, object> convertItemAction,
            Action<int, object> insertItemAction = null,
            ISupportInitialize supportInitialize = null,
            Action<object> clearItemAction = null) {

            if (target == null) return;
            BeginPopulate(target, supportInitialize);
            try {
                var oldItems = target.OfType<object>().ToList();
                target.Clear();
                if (clearItemAction != null)
                    oldItems.ForEach(clearItemAction);
                if (source == null) return;
                if (insertItemAction == null)
                    DoAction(source, (item) => AddItem(item, target, convertItemAction));
                else
                    DoAction(source, (item) => InsertItem(item, target, source, convertItemAction, insertItemAction));
            } finally {
                EndPopulate(target, supportInitialize);
            }
        }
        static void BeginPopulate(IList target, ISupportInitialize supportInitialize) {
            if(supportInitialize != null)
                supportInitialize.BeginInit();
            ILockable lockable = target as ILockable;
            if(lockable != null)
                lockable.BeginUpdate();
        }
        static void EndPopulate(IList target, ISupportInitialize supportInitialize) {
            ILockable lockable = target as ILockable;
            if(lockable != null) lockable.EndUpdate();
            if(supportInitialize != null)
                supportInitialize.EndInit();
        }

        static void AddItem(object item, IList target, Func<object, object> convertItemAction) {
            object loadedItem = convertItemAction(item);
            if(loadedItem == null) return;
            target.Add(loadedItem);
        }
        static void InsertItem(object item, IList target, IList source, Func<object, object> convertItemAction, Action<int, object> insertItemAction) {
            object loadedItem = convertItemAction(item);
            if(loadedItem == null) return;
            int index = source.IndexOf(item);
            if(insertItemAction != null)
                insertItemAction(index, loadedItem);
            else target.Insert(index, loadedItem);
        }
        static void RemoveItem(int index, IList target, Action<object> clearItemAction) {
            if(index < 0 || index >= target.Count) return;
            var item = target[index];
            target.RemoveAt(index);
            if (clearItemAction != null)
                clearItemAction(item);
        }
        static void DoAction(IEnumerable list, Action<object> action) {
            foreach(object item in list)
                action(item);
        }
    }
}