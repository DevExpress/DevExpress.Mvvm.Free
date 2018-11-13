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
            bool reverse;
            readonly bool useStrongReferences;

            public CollectionOneWayBinding(IEnumerable target, Func<TSource, TTarget> itemConverter, IEnumerable source,
                    CollectionLocker doNotProcessSourceCollectionChanged,
                    CollectionLocker doNotProcessTargetCollectionChanged, bool reverse, bool useStrongReferences) {
                source = GetListObject(source);
                this.reverse = reverse;
                this.useStrongReferences = useStrongReferences;
                this.doNotProcessSourceCollectionChanged = doNotProcessSourceCollectionChanged;
                this.doNotProcessTargetCollectionChanged = doNotProcessTargetCollectionChanged;
                sourceRef = useStrongReferences ? (object)source : new WeakReference(source);
                targetRef = useStrongReferences ? (object)target : new WeakReference(target);
                this.itemConverter = itemConverter;
                var sourceNotify = source as INotifyCollectionChanged;
                if(sourceNotify != null)
                    sourceNotify.CollectionChanged += OnSourceCollectionChanged;
            }
            public void Dispose() {
                var source = (useStrongReferences ? sourceRef : ((WeakReference)sourceRef).Target) as INotifyCollectionChanged;
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

            object sourceRef;
            object targetRef;

            IList<TSource> GetSource() { return GetList<TSource>(useStrongReferences ? sourceRef : ((WeakReference)sourceRef).Target); }
            IList<TTarget> GetTarget() { return GetList<TTarget>(useStrongReferences ? targetRef : ((WeakReference)targetRef).Target); }
            IList<T> GetList<T>(object listRef) {
                if(listRef == null) return null;
                var list = listRef as IList<T>;
                return list != null ? list : ListAdapter<T>.FromObjectList((IList)listRef);
            }
            IEnumerable GetListObject(IEnumerable source) {
                var list = source as IList<TSource>;
                if(list != null) return list;
                var objectList = source as IList;
                if(objectList != null) return objectList;
                return source.Cast<TSource>().ToList();
            }
        }

        sealed class CollectionTwoWayBinding<TTarget, TSource> : IDisposable {
            CollectionOneWayBinding<TTarget, TSource> sourceToTarget;
            CollectionOneWayBinding<TSource, TTarget> targetToSource;

            public CollectionTwoWayBinding(IEnumerable target, Func<TSource, TTarget> itemConverter, IEnumerable source, Func<TTarget, TSource> itemBackConverter, bool reverse, bool useStrongReferences) {
                CollectionLocker sourceLocker = new CollectionLocker();
                CollectionLocker targetLocker = new CollectionLocker();
                targetToSource = new CollectionOneWayBinding<TSource, TTarget>(source, itemBackConverter, target, targetLocker, sourceLocker, reverse, useStrongReferences);
                sourceToTarget = new CollectionOneWayBinding<TTarget, TSource>(target, itemConverter, source, sourceLocker, targetLocker, reverse, useStrongReferences);
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

        public static IDisposable BindOneWay<TTarget, TSource>(Func<TSource, TTarget> itemConverter, IList target, IList<TSource> source, bool reverse = false, bool useStrongReferences = false) {
            AssertCollections(target, source);
            return BindOneWayCore<TTarget, TSource>(target, itemConverter, source, reverse, useStrongReferences);
        }
        public static IDisposable BindOneWay<TTarget, TSource>(IList<TTarget> target, Func<TSource, TTarget> itemConverter, IEnumerable source, bool reverse = false, bool useStrongReferences = false) {
            AssertCollections(target, source);
            return BindOneWayCore<TTarget, TSource>(target, itemConverter, source, reverse, useStrongReferences);
        }
        public static IDisposable Bind<TTarget, TSource>(Func<TSource, TTarget> itemConverter, IList target, IList<TSource> source, Func<TTarget, TSource> itemBackConverter, bool reverse = false, bool useStrongReferences = false) {
            AssertCollections(target, source);
            return BindCore<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse, useStrongReferences);
        }
        public static IDisposable Bind<TTarget, TSource>(IList<TTarget> target, Func<TSource, TTarget> itemConverter, Func<TTarget, TSource> itemBackConverter, IList source, bool reverse = false, bool useStrongReferences = false) {
            AssertCollections(target, source);
            return BindCore<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse, useStrongReferences);
        }
        public static IDisposable Bind<TTarget, TSource>(Func<TSource, TTarget> itemConverter, IList target, Func<TTarget, TSource> itemBackConverter, IList source, bool reverse = false, bool useStrongReferences = false) {
            AssertCollections(target, source);
            return BindCore<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse, useStrongReferences);
        }
        public static IDisposable Bind<TTarget, TSource>(IList<TTarget> target, Func<TSource, TTarget> itemConverter, IList<TSource> source, Func<TTarget, TSource> itemBackConverter, bool reverse = false, bool useStrongReferences = false) {
            AssertCollections(target, source);
            return BindCore<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse, useStrongReferences);
        }
        static IDisposable BindCore<TTarget, TSource>(IEnumerable target, Func<TSource, TTarget> itemConverter, IEnumerable source, Func<TTarget, TSource> itemBackConverter, bool reverse, bool useStrongReferences) {
            var binding = new CollectionTwoWayBinding<TTarget, TSource>(target, itemConverter, source, itemBackConverter, reverse, useStrongReferences);
            binding.Reset();
            return binding;
        }
        static IDisposable BindOneWayCore<TTarget, TSource>(IEnumerable target, Func<TSource, TTarget> itemConverter, IEnumerable source, bool reverse, bool useStrongReferences) {
            var binding = new CollectionOneWayBinding<TTarget, TSource>(target, itemConverter, source, new CollectionLocker(), new CollectionLocker(), reverse, useStrongReferences);
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