#if !FREE
using DevExpress.Xpf.Core.Tests;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DevExpress.Mvvm.Native;
using NUnit.Framework;

namespace DevExpress.Mvvm.Tests {
#if !FREE
    [TestFixture]
    public class ListAdapterTests {
        [Test]
        public void AddRemoveItems() {
            List<int> list1 = new List<int>();
            List<string> list2 = new List<string>();
            var combinedList = ListAdapter<object>.FromTwoLists(list1, list2);
            Assert.AreEqual(0, combinedList.Count);
            combinedList.Add(13);
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 13 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new string[] {  }, list2);
            combinedList.Add("qwerty");
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 13 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "qwerty" }, list2);
            combinedList.Add(14);
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 13, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "qwerty" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 13, "qwerty", 14 }, combinedList);
            combinedList.Remove("qwerty");
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 13, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new string[] { }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 13, 14 }, combinedList);
            combinedList.Insert(1, 18);
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 13, 18, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new string[] { }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 13, 18, 14 }, combinedList);
            combinedList.Insert(1, "abc");
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 13, 18, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "abc" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 13, "abc", 18, 14 }, combinedList);
            combinedList.Add("def");
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 13, 18, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "abc", "def" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 13, "abc", 18, 14, "def" }, combinedList);
            combinedList.Insert(1, "012");
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 13, 18, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "012", "abc", "def" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 13, "012", "abc", 18, 14, "def" }, combinedList);
        }
        [Test]
        public void RemoveFirstElementIfFirstListIsEmpty() {
            List<int> list1 = new List<int>();
            List<string> list2 = new List<string>() { "a", "b", "c" };
            var combinedList = ListAdapter<object>.FromTwoLists(list1, list2);
            combinedList.RemoveAt(0);
            AssertHelper.AssertEnumerablesAreEqual(new int[] {  }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "b", "c" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { "b", "c" }, combinedList);
        }
        [Test]
        public void RestoreIndexMapTest() {
            var list1 = new List<string> { "str1" };
            var list2 = new List<int> { 13 };
            var listAdapter = ListAdapter<object>.FromTwoLists(list1, list2);
            Assert.AreEqual(2, listAdapter.Count);
            listAdapter.Add("str2");
            Assert.AreEqual(3, listAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { "str1", "str2" }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13 }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { "str1", 13, "str2" }, listAdapter);
            var newListAdapter = ListAdapter<object>.FromTwoLists(list1, list2);
            Assert.AreEqual(3, newListAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { "str1", "str2", 13 }, newListAdapter);
            var oldMappedList = (IMappedList)listAdapter;
            var newMappedList = (IMappedList)newListAdapter;
            newMappedList.Map = oldMappedList.Map;
            AssertHelper.AssertEnumerablesAreEqual(new object[] { "str1", 13, "str2" }, listAdapter);
        }
        [Test]
        public void RestoreIndexMapInResortedListTest() {
            var list = new List<int> { 13 };
            var listAdapter = ListAdapter<int>.FromUnsortedList(list, x => x > 10 ? 0 : list.Count - 1);
            Assert.AreEqual(1, listAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13 }, listAdapter);
            listAdapter.Insert(0, 14);
            Assert.AreEqual(2, list.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13, 14 }, list);
            Assert.AreEqual(2, listAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 14, 13 }, listAdapter);
            var newListAdapter = ListAdapter<int>.FromUnsortedList(list, _ => list.Count - 1);
            Assert.AreEqual(2, newListAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13, 14 }, newListAdapter);
            var oldMappedList = (IMappedList)listAdapter;
            var newMappedList = (IMappedList)newListAdapter;
            newMappedList.Map = oldMappedList.Map;
            Assert.AreEqual(2, newListAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 14, 13 }, newListAdapter);
        }
        [Test]
        public void RestoreSeveralIndexMapsInCombinedListTest() {
            var list1 = new List<int> { 13 };
            var list2 = new List<string> { "str1" };
            var list3 = new List<char> { 'a' };
            var list4 = new List<long> { 128L };
            var listAdapter = ListAdapter<object>.FromTwoLists(list1, list2);
            listAdapter = ListAdapter<object>.FromTwoLists(list3, listAdapter);
            listAdapter = ListAdapter<object>.FromTwoLists(list4, listAdapter);
            Assert.AreEqual(4, listAdapter.Count);
            listAdapter.Add(14);
            Assert.AreEqual(5, listAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new[] { "str1" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 'a' }, list3);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 128L }, list4);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 128L, 'a', 13, "str1", 14 }, listAdapter);
            listAdapter.Add("str2");
            Assert.AreEqual(6, listAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new[] { "str1", "str2" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 'a' }, list3);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 128L }, list4);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 128L, 'a', 13, "str1", 14, "str2" }, listAdapter);
            listAdapter.Add('b');
            Assert.AreEqual(7, listAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new[] { "str1", "str2" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 'a', 'b' }, list3);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 128L }, list4);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 128L, 'a', 13, "str1", 14, "str2", 'b' }, listAdapter);
            listAdapter.Add(256L);
            Assert.AreEqual(8, listAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new[] { "str1", "str2" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 'a', 'b' }, list3);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 128L, 256L }, list4);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 128L, 'a', 13, "str1", 14, "str2", 'b', 256L }, listAdapter);
            var map = ((IMappedList)listAdapter).Map;
            listAdapter = ListAdapter<object>.FromTwoLists(list1, list2);
            listAdapter = ListAdapter<object>.FromTwoLists(list3, listAdapter);
            listAdapter = ListAdapter<object>.FromTwoLists(list4, listAdapter);
            Assert.AreEqual(8, listAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new[] { "str1", "str2" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 'a', 'b' }, list3);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 128L, 256L }, list4);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 128L, 256L, 'a', 'b', 13, 14, "str1", "str2" }, listAdapter);
            ((IMappedList)listAdapter).Map = map;
            Assert.AreEqual(8, listAdapter.Count);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 13, 14 }, list1);
            AssertHelper.AssertEnumerablesAreEqual(new[] { "str1", "str2" }, list2);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 'a', 'b' }, list3);
            AssertHelper.AssertEnumerablesAreEqual(new[] { 128L, 256L }, list4);
            AssertHelper.AssertEnumerablesAreEqual(new object[] { 128L, 'a', 13, "str1", 14, "str2", 'b', 256L }, listAdapter);
        }
    }
#endif
    [TestFixture]
    public class CollectionBindingHelperTests {
        static void CreateAndCheckBinding(Func<IDisposable> binding) {
            WeakReference bindingRef = new WeakReference(binding());
#if !FREE
            MemoryLeaksTestHelper.AssertLeaks(new WeakReference[] { bindingRef });
#endif
        }
        [Test]
        public void AddItemsReverse() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>();
            ObservableCollection<string> collection2 = new ObservableCollection<string>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, a => a.ToString(), collection1, s => int.Parse(s), true));
            collection1.Add(8);
            Assert.AreEqual(collection1[0].ToString(), collection2[0]);
            collection2.Add("555");
            collection2.Add("56");
            Assert.AreEqual(collection1[0], int.Parse(collection2[2]));
            Assert.AreEqual(collection1[1], int.Parse(collection2[1]));
            Assert.AreEqual(collection1[2], int.Parse(collection2[0]));
            Assert.AreEqual(collection1.Count, 3);
            Assert.AreEqual(collection2.Count, 3);
        }
        [Test]
        public void RemoveItemsReverse() {
            ObservableCollection<string> collection1 = new ObservableCollection<string>();
            ObservableCollection<int> collection2 = new ObservableCollection<int>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, s => int.Parse(s), collection1, a => a.ToString(), true));
            collection1.Add("100");
            collection1.Add("101");
            collection1.Add("102");
            collection1.Add("103");
            collection1.RemoveAt(1);
            Assert.AreEqual(int.Parse(collection1[0]), collection2[2]);
            Assert.AreEqual(int.Parse(collection1[1]), collection2[1]);
            Assert.AreEqual(int.Parse(collection1[2]), collection2[0]);
            Assert.AreEqual(collection1.Count, 3);
            Assert.AreEqual(collection2.Count, 3);
            collection2.RemoveAt(2);
            Assert.AreEqual(int.Parse(collection1[0]), collection2[1]);
            Assert.AreEqual(int.Parse(collection1[1]), collection2[0]);
            Assert.AreEqual(collection1.Count, 2);
            Assert.AreEqual(collection2.Count, 2);
        }
        [Test]
        public void MoveItemsReverse() {
            ObservableCollection<string> collection1 = new ObservableCollection<string>();
            ObservableCollection<int> collection2 = new ObservableCollection<int>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, s => int.Parse(s), collection1, a => a.ToString(), true));
            collection1.Add("560");
            collection1.Add("200");
            collection1.Add("-300");
            collection1.Add("-1");
            collection1.Move(0, 2);
            for(int i = 0; i < collection1.Count; i++) {
                Assert.AreEqual(int.Parse(collection1[i]), collection2[collection2.Count - 1 - i]);
            }
            collection2.Move(3, 0);
            for(int i = 0; i < collection2.Count; i++) {
                Assert.AreEqual(int.Parse(collection1[i]), collection2[collection2.Count - 1 - i]);
            }
            Assert.AreEqual(collection1.Count, 4);
            Assert.AreEqual(collection2.Count, 4);
        }
        [Test]
        public void ReplaceItemsReverse() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>();
            ObservableCollection<string> collection2 = new ObservableCollection<string>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, a => a.ToString(), collection1, s => int.Parse(s), true));
            collection1.Add(-100);
            collection1.Add(254);
            collection1.Add(1);
            collection1.Add(1000);
            collection1[1] = collection1[2];
            for(int i = 0; i < collection1.Count; i++) {
                Assert.AreEqual(collection1[i], int.Parse(collection2[collection2.Count - 1 - i]));
            }
            collection2[3] = collection2[2];
            for(int i = 0; i < collection2.Count; i++) {
                Assert.AreEqual(collection1[i], int.Parse(collection2[collection2.Count - 1 - i]));
            }
            Assert.AreEqual(collection1.Count, 4);
            Assert.AreEqual(collection2.Count, 4);
        }
        [Test]
        public void MoveItemForward_CompareCollections() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>() { 0, 1, 2, 3, 4, 5 };
            ObservableCollection<int> collection2 = new ObservableCollection<int>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, x => x, collection1, x => x));
            AssertHelper.AssertEnumerablesAreEqual(collection1, collection2);
            collection1.Move(1, 3);
            AssertHelper.AssertEnumerablesAreEqual(collection1, collection2);
        }
        [Test]
        public void AddItems() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>();
            ObservableCollection<string> collection2 = new ObservableCollection<string>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, a => a.ToString(), collection1, s => int.Parse(s)));
            collection1.Add(8);
            Assert.AreEqual(collection1[0].ToString(), collection2[0]);
            collection2.Add("555");
            collection2.Add("56");
            Assert.AreEqual(collection1[0], int.Parse(collection2[0]));
            Assert.AreEqual(collection1[1], int.Parse(collection2[1]));
            Assert.AreEqual(collection1[2], int.Parse(collection2[2]));
            Assert.AreEqual(collection1.Count, 3);
            Assert.AreEqual(collection2.Count, 3);
        }
        [Test]
        public void RemoveItems() {
            ObservableCollection<string> collection1 = new ObservableCollection<string>();
            ObservableCollection<int> collection2 = new ObservableCollection<int>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, s => int.Parse(s), collection1, a => a.ToString()));
            collection1.Add("100");
            collection1.Add("101");
            collection1.Add("102");
            collection1.Add("103");
            collection1.RemoveAt(1);
            Assert.AreEqual(int.Parse(collection1[0]), collection2[0]);
            Assert.AreEqual(int.Parse(collection1[1]), collection2[1]);
            Assert.AreEqual(int.Parse(collection1[2]), collection2[2]);
            Assert.AreEqual(collection1.Count, 3);
            Assert.AreEqual(collection2.Count, 3);
            collection2.RemoveAt(2);
            Assert.AreEqual(int.Parse(collection1[0]), collection2[0]);
            Assert.AreEqual(int.Parse(collection1[1]), collection2[1]);
            Assert.AreEqual(collection1.Count, 2);
            Assert.AreEqual(collection2.Count, 2);
        }
        [Test]
        public void MoveItems() {
            ObservableCollection<string> collection1 = new ObservableCollection<string>();
            ObservableCollection<int> collection2 = new ObservableCollection<int>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, s => int.Parse(s), collection1, a => a.ToString()));
            collection1.Add("560");
            collection1.Add("200");
            collection1.Add("-300");
            collection1.Add("-1");
            collection1.Move(0, 2);
            for(int i = 0; i < collection1.Count; i++) {
                Assert.AreEqual(int.Parse(collection1[i]), collection2[i]);
            }
            collection2.Move(3, 0);
            for(int i = 0; i < collection2.Count; i++) {
                Assert.AreEqual(int.Parse(collection1[i]), collection2[i]);
            }
            Assert.AreEqual(collection1.Count, 4);
            Assert.AreEqual(collection2.Count, 4);
        }
        [Test]
        public void ReplaceItems() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>();
            ObservableCollection<string> collection2 = new ObservableCollection<string>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, a => a.ToString(), collection1, s => int.Parse(s)));
            collection1.Add(-100);
            collection1.Add(254);
            collection1.Add(1);
            collection1.Add(1000);
            collection1[1] = collection1[2];
            for(int i = 0; i < collection1.Count; i++) {
                Assert.AreEqual(collection1[i], int.Parse(collection2[i]));
            }
            collection2[3] = collection2[2];
            for(int i = 0; i < collection2.Count; i++) {
                Assert.AreEqual(collection1[i], int.Parse(collection2[i]));
            }
            Assert.AreEqual(collection1.Count, 4);
            Assert.AreEqual(collection2.Count, 4);
        }
        [Test]
        public void ClearItems() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>();
            ObservableCollection<string> collection2 = new ObservableCollection<string>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection2, a => a.ToString(), collection1, s => int.Parse(s)));
            collection1.Add(8);
            collection1.Add(60);
            collection1.Clear();
            Assert.AreEqual(collection1.Count, 0);
            Assert.AreEqual(collection2.Count, 0);
            collection2.Add("50");
            collection2.Add("56");
            collection2.Clear();
            Assert.AreEqual(collection1.Count, 0);
            Assert.AreEqual(collection2.Count, 0);
        }
        [Test]
        public void UnBinding() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>();
            ObservableCollection<string> collection2 = new ObservableCollection<string>();
            IDisposable stopBinding = CollectionBindingHelper.Bind(collection2, a => a.ToString(), collection1, s => int.Parse(s));
            collection1.Add(-100);
            collection1.Add(254);
            collection1.Add(1);
            Assert.AreEqual(collection1[0], int.Parse(collection2[0]));
            Assert.AreEqual(collection1[1], int.Parse(collection2[1]));
            Assert.AreEqual(collection1[2], int.Parse(collection2[2]));
            Assert.AreEqual(collection1.Count, 3);
            Assert.AreEqual(collection2.Count, 3);
            stopBinding.Dispose();
            collection1[1] = 10;
            collection2.RemoveAt(2);
            Assert.AreEqual(collection1[0], int.Parse(collection2[0]));
            Assert.AreEqual(collection1[1], 10);
            Assert.AreEqual(collection2[1], "254");
            Assert.AreEqual(collection1[2], 1);
            Assert.AreEqual(collection1.Count, 3);
            Assert.AreEqual(collection2.Count, 2);
        }
        [Test]
        public void TaskbarButtonInfoCollectionBehavior() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>();
            MyTestCollection<string> collection2 = new MyTestCollection<string>();
            collection2.Add("434");
            IDisposable stopBinding = CollectionBindingHelper.Bind(collection1, s => int.Parse(s), collection2, a => a.ToString());
            collection2.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection2[0], 0));
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 434 }, collection1);
            collection1.Add(100);
            stopBinding.Dispose();
            collection2.Remove("100");
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(collection1, s => int.Parse(s), collection2, a => a.ToString()));
            collection2.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, "100", 0));
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 434 }, collection1);
        }
        [Test]
        public void SyncWithSimpleList() {
            List<int> list = new List<int>() { 0, 1, 2 };
            ObservableCollection<int> targetCollection = new ObservableCollection<int>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(targetCollection, i => i, list, i => i));
            AssertHelper.AssertEnumerablesAreEqual(list, targetCollection);
        }
        [Test]
        public void OldListToNewList() {
            var list = new ArrayList() { 0, 1, 2 };
            ObservableCollection<int> targetCollection = new ObservableCollection<int>();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(targetCollection, i => i, i => i, list));
            Assert.AreEqual(1, targetCollection[1]);
        }
        [Test]
        public void NewListToOldList() {
            var list = new ObservableCollection<int>() { 0, 1, 2 };
            var target = new ArrayList();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind(i => i, target, list, i => i));
            Assert.AreEqual(1, target[1]);
        }
        [Test]
        public void OldListToOldList() {
            var list = new ArrayList() { 0, 1, 2 };
            var target = new ArrayList();
            CreateAndCheckBinding(() => CollectionBindingHelper.Bind((int i) => i, target, i => i, list));
            Assert.AreEqual(1, target[1]);
        }
        [Test]
        public void OldListToOldList_WithGC() {
            var list = new MyTestCollection();
            list.Add(0);
            list.Add(1);
            var target = new ArrayList();
            var binding = CollectionBindingHelper.Bind((int i) => i, target, i => i, list);
            Assert.AreEqual(1, target[1]);
#if !FREE
            MemoryLeaksTestHelper.GarbageCollect();
            list.Add(2);
            list.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list[2], 2));
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(2, target[2]);
#endif
            GC.KeepAlive(binding);
            GC.KeepAlive(list);
            GC.KeepAlive(target);
        }
#if !FREE
        [Test]
        public void MemoryLeaksTest() {
            var list = new ArrayList();
            list.Add(0);
            list.Add(1);
            var target = new ArrayList();
            var binding = CollectionBindingHelper.Bind((int i) => i, target, i => i, list);
            var listRef = new WeakReference(list);
            var targetRef = new WeakReference(target);
            list = null;
            target = null;
            MemoryLeaksTestHelper.AssertLeaks(new WeakReference[] { listRef, targetRef });
            GC.KeepAlive(binding);
        }

        [Test]
        public void CheckReferencesCollectedTest() {
            CheckReferencesCollected(false);
            CheckReferencesCollected(true); 
        }
        void CheckReferencesCollected(bool useStrongReferences) {
            var source = new ObservableCollection<int>();
            var target = new ObservableCollection<int>();
            var binding = CollectionBindingHelper.Bind(target, (int i) => i, source, (int i) => i, false, useStrongReferences);
            var sourceRef = new WeakReference(source);
            source = null;
            binding = null;

            MemoryLeaksTestHelper.GarbageCollect();
            Assert.AreEqual(useStrongReferences, sourceRef.IsAlive);
            GC.KeepAlive(target);
        }

#endif
    }
    public class MyTestCollection<T> : IList<T>, INotifyCollectionChanged {
        List<T> internalCollection = new List<T>();

        public void Add(T item) { internalCollection.Add(item); }
        public int IndexOf(T item) { return internalCollection.IndexOf(item); }
        public void Insert(int index, T item) { internalCollection.Insert(index, item); }
        public void RemoveAt(int index) { internalCollection.RemoveAt(index); }
        public T this[int index] {
            get { return internalCollection[index]; }
            set { internalCollection[index] = value; }
        }
        public void Clear() { internalCollection.Clear(); }
        public bool Contains(T item) { return internalCollection.Contains(item); }
        public void CopyTo(T[] array, int arrayIndex) { internalCollection.CopyTo(array, arrayIndex); }
        public int Count { get { return internalCollection.Count; } }
        public bool IsReadOnly { get { return false; } }
        public bool Remove(T item) { return internalCollection.Remove(item); }
        public IEnumerator<T> GetEnumerator() { return internalCollection.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if(CollectionChanged != null)
                CollectionChanged(this, e);
        }
    }
    public class MyTestCollection : IList, INotifyCollectionChanged {
        ArrayList internalCollection = new ArrayList();

        public int Add(object item) { return internalCollection.Add(item); }
        public int IndexOf(object item) { return internalCollection.IndexOf(item); }
        public void Insert(int index, object item) { internalCollection.Insert(index, item); }
        public void RemoveAt(int index) { internalCollection.RemoveAt(index); }
        public object this[int index] {
            get { return internalCollection[index]; }
            set { internalCollection[index] = value; }
        }
        public void Clear() { internalCollection.Clear(); }
        public bool Contains(object item) { return internalCollection.Contains(item); }
        public void CopyTo(Array array, int arrayIndex) { internalCollection.CopyTo(array, arrayIndex); }
        public int Count { get { return internalCollection.Count; } }
        public bool IsReadOnly { get { return false; } }
        public bool IsFixedSize { get { return internalCollection.IsFixedSize; } }
        public object SyncRoot { get { return internalCollection.SyncRoot; } }
        public bool IsSynchronized { get { return internalCollection.IsSynchronized; } }
        public void Remove(object item) { internalCollection.Remove(item); }
        public IEnumerator GetEnumerator() { return internalCollection.GetEnumerator(); }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if(CollectionChanged != null)
                CollectionChanged(this, e);
        }
    }
}