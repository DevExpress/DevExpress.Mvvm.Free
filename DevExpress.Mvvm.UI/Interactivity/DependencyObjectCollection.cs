using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace DevExpress.Mvvm.UI.Interactivity {
    public class DependencyObjectCollection<T> : FrameworkElement, INotifyCollectionChanged, IEnumerable<T>, IList<T>, ICollection<T> where T : DependencyObject {
        protected ObservableCollection<T> CollectionCore { get; set; }
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public DependencyObjectCollection() {
            CollectionCore = new ObservableCollection<T>();
            CollectionCore.CollectionChanged += ObservableCollectionChanged;
        }

        void ObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaiseCollectionChanged(e);
        }

        protected virtual void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if(CollectionChanged != null) CollectionChanged(this, e);
        }

        public IEnumerator<T> GetEnumerator() {
            return CollectionCore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return CollectionCore.GetEnumerator();
        }


        public int IndexOf(T item) {
            return CollectionCore.IndexOf(item);
        }

        public void Insert(int index, T item) {
            CollectionCore.Insert(index, item);
        }

        public void RemoveAt(int index) {
            CollectionCore.RemoveAt(index);
        }

        public T this[int index] {
            get { return CollectionCore[index]; }
            set { CollectionCore[index] = value; }
        }

        public void Add(T item) {
            CollectionCore.Add(item);
        }

        public void Clear() {
            CollectionCore.Clear();
        }

        public bool Contains(T item) {
            return CollectionCore.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            CollectionCore.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return CollectionCore.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(T item) {
            return CollectionCore.Remove(item);
        }
    }
}