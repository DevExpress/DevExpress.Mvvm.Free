#if !SILVERLIGHT
using NUnit.Framework;
#endif
using DevExpress.Mvvm.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class SyncCollectionHelperTests {
        [Test]
        public void AddItems() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>();
            ObservableCollection<string> collection2 = new ObservableCollection<string>();
            SyncCollectionHelper.TwoWayBind(collection2, collection1, a => a.ToString(), s => int.Parse(s));
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
            SyncCollectionHelper.TwoWayBind(collection2, collection1, s => int.Parse(s), a => a.ToString());
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
#if !SILVERLIGHT
        [Test]
        public void MoveItems() {
            ObservableCollection<string> collection1 = new ObservableCollection<string>();
            ObservableCollection<int> collection2 = new ObservableCollection<int>();
            SyncCollectionHelper.TwoWayBind(collection2, collection1, s => int.Parse(s), a => a.ToString());
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
#endif
        [Test]
        public void ReplaceItems() {
            ObservableCollection<int> collection1 = new ObservableCollection<int>();
            ObservableCollection<string> collection2 = new ObservableCollection<string>();
            SyncCollectionHelper.TwoWayBind(collection2, collection1, a => a.ToString(), s => int.Parse(s));
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
            SyncCollectionHelper.TwoWayBind(collection2, collection1, a => a.ToString(), s => int.Parse(s));
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
            IDisposable stopBinding = SyncCollectionHelper.TwoWayBind(collection2, collection1, a => a.ToString(), s => int.Parse(s));
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
            IDisposable stopBinding = SyncCollectionHelper.TwoWayBind(collection1, collection2, s => int.Parse(s), a => a.ToString());
            collection2.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection2[0], 0));
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 434 }, collection1);
            collection1.Add(100);
            stopBinding.Dispose();
            collection2.Remove("100");
            stopBinding = SyncCollectionHelper.TwoWayBind(collection1, collection2, s => int.Parse(s), a => a.ToString());
            collection2.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, "100", 0));
            AssertHelper.AssertEnumerablesAreEqual(new int[] { 434 }, collection1);
        }
        [Test]
        public void SyncWithSimpleList() {
            List<int> list = new List<int>() { 0, 1, 2 };
            ObservableCollection<int> targetCollection = new ObservableCollection<int>();
            SyncCollectionHelper.TwoWayBind(targetCollection, list, i => i, i => i);
            AssertHelper.AssertEnumerablesAreEqual(list, targetCollection);
        }
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
}