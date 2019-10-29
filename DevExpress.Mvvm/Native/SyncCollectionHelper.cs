using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace DevExpress.Mvvm.Native {
    public static class SyncCollectionHelper {
        public static IDisposable TwoWayBind<TSource, TTarget>(IList<TTarget> target, IList<TSource> source, Func<TSource, TTarget> itemConverter, Func<TTarget, TSource> itemBackConverter) {
            return CollectionBindingHelper.Bind(target, itemConverter, source, itemBackConverter);
        }
        public static void SyncCollection(
            NotifyCollectionChangedEventArgs e,
            IList target,
            IList source,
            Func<object, object> convertItemAction,
            Action<int, object> insertItemAction = null,
            ISupportInitialize supportInitialize = null,
            Action<object> clearItemAction = null) {

            GuardHelper.ArgumentNotNull(target, nameof(target));
            GuardHelper.ArgumentNotNull(source, nameof(source));

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
                case NotifyCollectionChangedAction.Move:
                    object insertItem = target[e.OldStartingIndex];

                    target.RemoveAt(e.OldStartingIndex);
                    target.Insert(e.NewStartingIndex, insertItem);
                    break;
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