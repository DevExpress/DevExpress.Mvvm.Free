using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Shell;
using DevExpress.Mvvm.UI.Native;
using DevExpress.Utils;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI {
    public class TaskbarThumbButtonInfoCollection : IList<TaskbarThumbButtonInfo>, IList {
        public TaskbarThumbButtonInfoCollection() : this(new ThumbButtonInfoCollection()) { }
        public TaskbarThumbButtonInfoCollection(ThumbButtonInfoCollection collection) {
            GuardHelper.ArgumentNotNull(collection, "collection");
            this.InternalCollection = collection;
        }
        internal ThumbButtonInfoCollection InternalCollection { get; private set; }
        public int IndexOf(TaskbarThumbButtonInfo item) {
            return InternalCollection.IndexOf(TaskbarThumbButtonInfoWrapper.Wrap(item));
        }
        public void Insert(int index, TaskbarThumbButtonInfo item) {
            InternalCollection.Insert(index, TaskbarThumbButtonInfoWrapper.Wrap(item));
        }
        public void RemoveAt(int index) {
            InternalCollection.RemoveAt(index);
        }
        public TaskbarThumbButtonInfo this[int index] {
            get { return TaskbarThumbButtonInfoWrapper.UnWrap(InternalCollection[index]); }
            set { InternalCollection[index] = TaskbarThumbButtonInfoWrapper.Wrap(value); }
        }
        public void Add(TaskbarThumbButtonInfo item) {
            InternalCollection.Add(TaskbarThumbButtonInfoWrapper.Wrap(item));
        }
        public void Clear() {
            InternalCollection.Clear();
        }
        public bool Contains(TaskbarThumbButtonInfo item) {
            return InternalCollection.Contains(TaskbarThumbButtonInfoWrapper.Wrap(item));
        }
        public void CopyTo(TaskbarThumbButtonInfo[] array, int arrayIndex) {
            foreach(TaskbarThumbButtonInfo item in this) {
                array[arrayIndex] = item;
                ++arrayIndex;
            }
        }
        public int Count { get { return InternalCollection.Count; } }
        public bool IsReadOnly { get { return false; } }
        public bool Remove(TaskbarThumbButtonInfo item) {
            return InternalCollection.Remove(TaskbarThumbButtonInfoWrapper.Wrap(item));
        }
        public IEnumerator<TaskbarThumbButtonInfo> GetEnumerator() {
            foreach(ThumbButtonInfo item in InternalCollection) {
                yield return TaskbarThumbButtonInfoWrapper.UnWrap(item);
            }
        }
        #region IList
        object syncRoot = new object();

        int IList.Add(object value) {
            Add((TaskbarThumbButtonInfo)value);
            return Count - 1;
        }
        bool IList.Contains(object value) {
            return Contains((TaskbarThumbButtonInfo)value);
        }
        int IList.IndexOf(object value) {
            return IndexOf((TaskbarThumbButtonInfo)value);
        }
        void IList.Insert(int index, object value) {
            Insert(index, (TaskbarThumbButtonInfo)value);
        }
        bool IList.IsFixedSize {
            get { return false; }
        }
        void IList.Remove(object value) {
            Remove((TaskbarThumbButtonInfo)value);
        }
        object IList.this[int index] {
            get { return this[index];}
            set { this[index] = (TaskbarThumbButtonInfo)value;}
        }
        void ICollection.CopyTo(Array array, int arrayIndex) {
            CopyTo((TaskbarThumbButtonInfo[])array, arrayIndex);
        }
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { return syncRoot; } }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        #endregion
    }
}