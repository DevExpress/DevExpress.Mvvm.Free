using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.Native {
    public static class ListAdapter<T> {
        sealed class ObjectList : IList<T> {
            readonly IList innerList;

            public ObjectList(IList innerList) {
                this.innerList = innerList;
            }
            T IList<T>.this[int index] {
                get { return (T)innerList[index]; }
                set { innerList[index] = value; }
            }
            int ICollection<T>.Count { get { return innerList.Count; } }
            bool ICollection<T>.IsReadOnly { get { return innerList.IsReadOnly; } }
            void ICollection<T>.Add(T item) { innerList.Add(item); }
            void ICollection<T>.Clear() { innerList.Clear(); }
            bool ICollection<T>.Contains(T item) { return innerList.Contains(item); }
            void ICollection<T>.CopyTo(T[] array, int arrayIndex) { innerList.CopyTo(array, arrayIndex); }
            public IEnumerator<T> GetEnumerator() {
                foreach(T item in innerList)
                    yield return item;
            }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            int IList<T>.IndexOf(T item) { return innerList.IndexOf(item); }
            void IList<T>.Insert(int index, T item) { innerList.Insert(index, item); }
            bool ICollection<T>.Remove(T item) {
                int index = innerList.IndexOf(item);
                if(index < 0) return false;
                innerList.RemoveAt(index);
                return true;
            }
            void IList<T>.RemoveAt(int index) { innerList.RemoveAt(index); }
        }

        sealed class CombinedList<T1, T2> : IList<T>
            where T1 : T
            where T2 : T {

            readonly IList<T1> list1;
            readonly IList<T2> list2;
            readonly Func<T, bool> addItemToFirstList;

            public CombinedList(IList<T1> list1, IList<T2> list2, Func<T, bool> addItemToFirstList) {
                this.list1 = list1;
                this.list2 = list2;
                this.addItemToFirstList = addItemToFirstList;
            }

            T IList<T>.this[int index] {
                get {
                    if(index < list1.Count)
                        return list1[index];
                    else
                        return list2[index - list1.Count];
                }
                set {
                    if(index < list1.Count)
                        list1[index] = (T1)value;
                    else
                        list2[index - list1.Count] = (T2)value;
                }
            }
            int ICollection<T>.Count { get { return list1.Count + list2.Count; } }
            bool ICollection<T>.IsReadOnly { get { return list1.IsReadOnly || list2.IsReadOnly; } }
            void ICollection<T>.Add(T item) {
                list2.Add((T2)item);
            }
            void ICollection<T>.Clear() {
                list1.Clear();
                list2.Clear();
            }
            bool ICollection<T>.Contains(T item) {
                if(item == null)
                    return list1.Contains(default(T1)) || list2.Contains(default(T2));
                if(item is T1 && list1.Contains((T1)item)) return true;
                if(item is T2 && list2.Contains((T2)item)) return true;
                return false;
            }
            bool ICollection<T>.Remove(T item) {
                if(item == null)
                    return list1.Remove(default(T1)) || list2.Remove(default(T2));
                if(item is T1 && list1.Remove((T1)item)) return true;
                if(item is T2 && list2.Remove((T2)item)) return true;
                return false;
            }
            int IList<T>.IndexOf(T item) {
                if(item == null) {
                    int index1 = list1.IndexOf(default(T1));
                    if(index1 >= 0) return index1;
                    int index2 = list2.IndexOf(default(T2));
                    if(index2 >= 0) return index2 + list1.Count;
                }
                if(item is T1) {
                    int index1 = list1.IndexOf(default(T1));
                    if(index1 >= 0) return index1;
                }
                if(item is T2) {
                    int index2 = list2.IndexOf(default(T2));
                    if(index2 >= 0) return index2 + list1.Count;
                }
                return -1;
            }
            void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
                foreach(var item in this)
                    array[arrayIndex++] = item;
            }
            public IEnumerator<T> GetEnumerator() {
                foreach(var item1 in list1)
                    yield return item1;
                foreach(var item2 in list2)
                    yield return item2;
            }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            void IList<T>.Insert(int index, T item) {
                bool toFirstList;
                if(index == list1.Count)
                    toFirstList = addItemToFirstList(item);
                else
                    toFirstList = index < list1.Count;
                if(toFirstList)
                    list1.Insert(index, (T1)item);
                else
                    list2.Insert(index - list1.Count, (T2)item);
            }
            void IList<T>.RemoveAt(int index) {
                if(index < list1.Count)
                    list1.RemoveAt(index);
                else
                    list2.RemoveAt(index - list1.Count);
            }
        }

        sealed class SingletonList : IList<T> {
            readonly bool hasEmptyValue;
            readonly T emptyValue;
            readonly Func<T> getValue;
            readonly Action<T> setValue;

            public SingletonList(Func<T> getValue, Action<T> setValue, bool hasEmptyValue, T emptyValue) {
                this.getValue = getValue;
                this.setValue = setValue;
                this.hasEmptyValue = hasEmptyValue;
                this.emptyValue = emptyValue;
            }

            T IList<T>.this[int index] {
                get { return getValue(); }
                set { setValue(value); }
            }
            bool IsEmpty { get { return hasEmptyValue && Equals(getValue(), emptyValue); } }
            bool Contains(T item) { return !IsEmpty && Equals(item, getValue()); }
            int ICollection<T>.Count { get { return IsEmpty ? 0 : 1; } }
            bool ICollection<T>.IsReadOnly { get { return false; } }
            void ICollection<T>.Add(T item) {
                if(!IsEmpty)
                    throw new NotSupportedException();
                setValue(item);
            }
            void ICollection<T>.Clear() {
                if(!hasEmptyValue)
                    throw new NotSupportedException();
                setValue(emptyValue);
            }
            bool ICollection<T>.Contains(T item) {
                return Contains(item);
            }
            void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
                if(!IsEmpty)
                    array[arrayIndex] = getValue();
            }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            public IEnumerator<T> GetEnumerator() {
                if(!IsEmpty)
                    yield return getValue();
            }
            int IList<T>.IndexOf(T item) {
                return Contains(item) ? 0 : -1;
            }
            void IList<T>.Insert(int index, T item) {
                if(!IsEmpty)
                    throw new NotSupportedException();
                if(index != 0)
                    throw new InvalidOperationException();
                setValue(item);
            }
            bool ICollection<T>.Remove(T item) {
                if(!Contains(item)) return false;
                if(!hasEmptyValue)
                    throw new NotSupportedException();
                setValue(emptyValue);
                return true;
            }
            void IList<T>.RemoveAt(int index) {
                if(IsEmpty || index != 0)
                    throw new InvalidOperationException();
                if(!hasEmptyValue)
                    throw new NotSupportedException();
                setValue(emptyValue);
            }
        }

        public static IList<T> FromObjectList(IList objectList) {
            return new ObjectList(objectList);
        }
        public static IList<T> FromTwoLists<T1, T2>(IList<T1> list1, IList<T2> list2, Func<T, bool> addItemToFirstList)
            where T1 : T
            where T2 : T {

            return new CombinedList<T1, T2>(list1, list2, addItemToFirstList);
        }
        public static IList<T> FromTwoObjectLists<T1, T2>(IList objectList1, IList objectlist2, Func<T, bool> addItemToFirstList)
            where T1 : T
            where T2 : T {

            return FromTwoLists(ListAdapter<T1>.FromObjectList(objectList1), ListAdapter<T2>.FromObjectList(objectlist2), addItemToFirstList);
        }
        public static IList<T> FromObject(Func<T> getValue, Action<T> setValue) {
            return new SingletonList(getValue, setValue, false, default(T));
        }
        public static IList<T> FromObject(Func<T> getValue, Action<T> setValue, T emptyValue) {
            return new SingletonList(getValue, setValue, true, emptyValue);
        }
    }
}