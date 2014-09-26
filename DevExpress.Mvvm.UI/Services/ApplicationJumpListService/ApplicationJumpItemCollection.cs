using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Interactivity.Internal;
using DevExpress.Mvvm.UI.Native;
using DevExpress.Utils;
using DevExpress.Internal;
using System.Windows.Markup;

namespace DevExpress.Mvvm.UI {
    public abstract class ApplicationJumpItemCollection : IList<ApplicationJumpItem>, IList {
        IApplicationJumpListImplementation implementation;
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public FreezableCollection<ApplicationJumpItem> SourceItems { get; private set; }
        protected ApplicationJumpItemCollection(IApplicationJumpListImplementation implementation) {
            if(implementation == null) throw new ArgumentNullException("implementation");
            this.implementation = implementation;
            SourceItems = new FreezableCollection<ApplicationJumpItem>();
        }
        public void Add(ApplicationJumpItem item) {
            implementation.AddItem(ApplicationJumpItem.GetItemInfo(item));
            SourceItems.Add(item);
        }
        public int IndexOf(ApplicationJumpItem item) {
            return implementation.IndexOfItem(ApplicationJumpItem.GetItemInfo(item));
        }
        public void Insert(int index, ApplicationJumpItem item) {
            implementation.InsertItem(index, ApplicationJumpItem.GetItemInfo(item));
            SourceItems.Insert(index, item);
        }
        public void RemoveAt(int index) {
            implementation.RemoveItemAt(index);
            SourceItems.RemoveAt(index);
        }
        public ApplicationJumpItem this[int index] {
            get { return ApplicationJumpItem.GetItem(implementation.GetItem(index)); }
            set { implementation.SetItem(index, ApplicationJumpItem.GetItemInfo(value)); }
        }
        public void Clear() {
            implementation.ClearItems();
            SourceItems.Clear();
        }
        public bool Contains(ApplicationJumpItem item) {
            return implementation.ContainsItem(ApplicationJumpItem.GetItemInfo(item));
        }
        public void CopyTo(ApplicationJumpItem[] array, int arrayIndex) {
            foreach(ApplicationJumpItemInfo item in implementation.GetItems()) {
                array[arrayIndex] = ApplicationJumpItem.GetItem(item);
                ++arrayIndex;
            }
        }
        public int Count {
            get { return implementation.ItemsCount(); }
        }
        public bool IsReadOnly { get { return false; } }
        public bool Remove(ApplicationJumpItem item) {
            SourceItems.Remove(item);
            return implementation.RemoveItem(ApplicationJumpItem.GetItemInfo(item));
        }
        public IEnumerator<ApplicationJumpItem> GetEnumerator() {
            return implementation.GetItems().Select(ApplicationJumpItem.GetItem).GetEnumerator();
        }
        #region IList
        object syncRoot = new object();

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        int IList.Add(object value) {
            Add((ApplicationJumpItem)value);
            return Count - 1;
        }
        void IList.Insert(int index, object value) { Insert(index, (ApplicationJumpItem)value); }
        object IList.this[int index] {
            get { return this[index]; }
            set { this[index] = (ApplicationJumpItem)value; }
        }
        bool IList.Contains(object value) { return Contains((ApplicationJumpItem)value); }
        int IList.IndexOf(object value) { return IndexOf((ApplicationJumpItem)value); }
        bool IList.IsFixedSize { get { return false; } }
        bool IList.IsReadOnly { get { return IsReadOnly; } }
        void IList.Remove(object value) { Remove((ApplicationJumpItem)value); }
        void ICollection.CopyTo(Array array, int index) { CopyTo((ApplicationJumpItem[])array, index); }
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { return syncRoot; } }
        #endregion
    }
}