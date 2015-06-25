using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    public static class CollectionBindingHelper {
        #region inner classses
        sealed class CollectionLocker {
            bool locked;

            public void DoIfNotLocked(Action action) {
                if(!locked)
                    action();
            }
            public void DoLockedAction(Action action) {
                locked = true;
                try {
                    action();
                } finally {
                    locked = false;
                }
            }
            public void DoLockedActionIfNotLocked(CollectionLocker possibleLocked, Action action) {
                possibleLocked.DoIfNotLocked(() => DoLockedAction(action));
            }
        }

        sealed class CollectionOneWayBinding<TTarget, TSource> : IDisposable {
            CollectionLocker doNotProcessSourceCollectionChanged;
            CollectionLocker doNotProcessTargetCollectionChanged;
            Func<TSource, TTarget> itemConverter;
            Func<TTarget, TSource> itemBackConverter;
            bool reverse;

            public CollectionOneWayBinding(object target, Func<TSource, TTarget> itemConverter, object source, Func<TTarget, TSource> itemBackConverter,
                    CollectionLocker doNotProcessSourceCollectionChanged,
                    CollectionLocker doNotProcessTargetCollectionChanged, bool reverse) {
                this.reverse = reverse;
                this.doNotProcessSourceCollectionChanged = doNotProcessSourceCollectionChanged;
                this.doNotProcessTargetCollectionChanged = doNotProcessTargetCollectionChanged;
                sourceRef = new WeakReference(source);
                targetRef = new WeakReference(target);
                this.itemConverter = itemConverter;
                this.itemBackConverter = itemBackConverter;
                var sourceNotify = source as INotifyCollectionChanged;
                if(sourceNotify != null)
                    sourceNotify.CollectionChanged += OnSourceCollectionChanged;
            }
            public void Dispose() {
                var source = GetSource() as INotifyCollectionChanged;
                if(source != null)
                    source.CollectionChanged -= OnSourceCollectionChanged;
            }
            IEnumerable<T> ReverseIfNeeded<T>(IEnumerable<T> items) {
                return reverse ? items.Reverse() : items;
            }
            public void Reset() {
                doNotProcessTargetCollectionChanged.DoLockedActionIfNotLocked(doNotProcessSourceCollectionChanged, () => {
                    var source = GetSource();
                    var target = GetTarget();
                    if(source == null || target == null) return;
                    target.Clear();
                    foreach(var item in ReverseIfNeeded(source))
                        target.Add(itemConverter(item));
                });
            }
            void AddCore(NotifyCollectionChangedEventArgs e, IList<TTarget> target) {
                int itemIndex = reverse ? target.Count + 1 - e.NewStartingIndex - e.NewItems.Count : e.NewStartingIndex;
                foreach(var item in ReverseIfNeeded(e.NewItems.Cast<TSource>())) {
                    target.Insert(itemIndex, itemConverter(item));
                    ++itemIndex;
                }
            }
            void RemoveCore(NotifyCollectionChangedEventArgs e, IList<TTarget> target) {
                int itemIndex = reverse ? target.Count - e.OldStartingIndex - 1 : e.OldStartingIndex + e.OldItems.Count - 1;
                for(int i = e.OldItems.Count; --i >= 0;) {
                    target.RemoveAt(itemIndex);
                    --itemIndex;
                }
            }
            void Add(NotifyCollectionChangedEventArgs e) {
                doNotProcessTargetCollectionChanged.DoLockedActionIfNotLocked(doNotProcessSourceCollectionChanged, () => {
                    var source = GetSource();
                    var target = GetTarget();
                    if(source == null || target == null) return;
                    if(source.Count == target.Count) return;
                    AddCore(e, target);
                });
            }
            void Remove(NotifyCollectionChangedEventArgs e) {
                doNotProcessTargetCollectionChanged.DoLockedActionIfNotLocked(doNotProcessSourceCollectionChanged, () => {
                    var source = GetSource();
                    var target = GetTarget();
                    if(source == null || target == null) return;
                    if(source.Count == target.Count) return;
                    RemoveCore(e, target);
                });
            }
            void Move(NotifyCollectionChangedEventArgs e) {
                doNotProcessTargetCollectionChanged.DoLockedActionIfNotLocked(doNotProcessSourceCollectionChanged, () => {
                    var target = GetTarget();
                    if(target == null) return;
                    RemoveCore(e, target);
                    AddCore(e, target);
                });
            }
            void Replace(NotifyCollectionChangedEventArgs e) {
                doNotProcessTargetCollectionChanged.DoLockedActionIfNotLocked(doNotProcessSourceCollectionChanged, () => {
                    var source = GetSource();
                    var target = GetTarget();
                    if(source == null || target == null) return;
                    int itemIndex = reverse ? target.Count - e.NewStartingIndex - e.NewItems.Count : e.NewStartingIndex;
                    foreach(var item in ReverseIfNeeded(e.NewItems.Cast<TSource>())) {
                        target[itemIndex] = itemConverter(item);
                        ++itemIndex;
                    }
                });
            }
            void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
                var source = GetSource();
                var target = GetTarget();
                if(target == null) return;
                switch(e.Action) {
                    case NotifyCollectionChangedAction.Add: Add(e); break;
                    case NotifyCollectionChangedAction.Remove: Remove(e); break;
                    case NotifyCollectionChangedAction.Move: Move(e); break;
                    case NotifyCollectionChangedAction.Replace: Replace(e); break;
                    default: Reset(); break;
                }
            }
            WeakReference sourceRef;
            IList<TSource> GetSource() { return GetList<TSource>(sourceRef); }
            WeakReference targetRef;
            IList<TTarget> GetTarget() { return GetList<TTarget>(targetRef); }
            IList<T> GetList<T>(WeakReference listRef) {
                object listObject = listRef.Target;
                if(listObject == null) return null;
                var list = listObject as IList<T>;
                return list != null ? list : ListAdapter<T>.FromObjectList((IList)listObject);
            }
        }

        sealed class CollectionTwoWayBinding<TTarget, TSource> : IDisposable {
            CollectionOneWayBinding<TTarget, TSource> sourceToTarget;
            CollectionOneWayBinding<TSource, TTarget> targetToSource;

            public CollectionTwoWayBinding(object target, Func<TSource, TTarget> itemConverter, object source, Func<TTarget, TSource> itemBackConverter, bool reverse) {
                CollectionLocker sourceLocker = new CollectionLocker();
                CollectionLocker targetLocker = new CollectionLocker();
                targetToSource = new CollectionOneWayBinding<TSource, TTarget>(source, itemBackConverter, target, itemConverter, targetLocker, sourceLocker, reverse);
                sourceToTarget = new CollectionOneWayBinding<TTarget, TSource>(target, itemConverter, source, itemBackConverter, sourceLocker, targetLocker, reverse);
            }
            public void Reset() {
                sourceToTarget.Reset();
            }
            public void Dispose() {
                sourceToTarget.Dispose();
                sourceToTarget = null;
                targetToSource.Dispose();
                targetToSource = null;
            }
        }
        #endregion

        public static IDisposable Bind<TTarget, TSource>(Func<TSource, TTarget> itemConverter, IList target, IList<TSource> source, Func<TTarget, TSource> itemBackConverter, bool reverse = false) {
            AssertCollections(target, source);
            return BindCore<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse);
        }
        public static IDisposable Bind<TTarget, TSource>(IList<TTarget> target, Func<TSource, TTarget> itemConverter, Func<TTarget, TSource> itemBackConverter, IList source, bool reverse = false) {
            AssertCollections(target, source);
            return BindCore<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse);
        }
        public static IDisposable Bind<TTarget, TSource>(Func<TSource, TTarget> itemConverter, IList target, Func<TTarget, TSource> itemBackConverter, IList source, bool reverse = false) {
            AssertCollections(target, source);
            return BindCore<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse);
        }
        public static IDisposable Bind<TTarget, TSource>(IList<TTarget> target, Func<TSource, TTarget> itemConverter, IList<TSource> source, Func<TTarget, TSource> itemBackConverter, bool reverse = false) {
            AssertCollections(target, source);
            return BindCore<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse);
        }
        static IDisposable BindCore<TTarget, TSource>(object target, Func<TSource, TTarget> itemConverter, object source, Func<TTarget, TSource> itemBackConverter, bool reverse) {
            var binding = new CollectionTwoWayBinding<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse);
            binding.Reset();
            return binding;
        }
        static void AssertCollections<TTarget, TSource>(TTarget target, TSource source) where TTarget : class where TSource : class {
            if(target == null)
                throw new ArgumentNullException("target");
            if(source == null)
                throw new ArgumentNullException("source");
        }
    }
}