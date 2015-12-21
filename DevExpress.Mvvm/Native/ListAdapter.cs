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
            List<int> publicIndexes;
            List<int> innerIndexes;

            public CombinedList(IList<T1> list1, IList<T2> list2, Func<T, bool> addItemToFirstList) {
                this.list1 = list1;
                this.list2 = list2;
                this.addItemToFirstList = addItemToFirstList;
                int count = list1.Count + list2.Count;
            }

            void UpdateIndexMap() {
                int count = Count;
                if(publicIndexes == null || publicIndexes.Count != count) {
                    publicIndexes = Enumerable.Range(0, count).ToList();
                    innerIndexes = Enumerable.Range(0, count).ToList();
                }
            }

            int InnerIndex(int publicIndex) {
                return innerIndexes[publicIndex];
            }
            int PublicIndex(int innerIndex) {
                return publicIndexes[innerIndex];
            }
            void SwapItemsVirtual(int publicIndex1, int publicIndex2) {
                int innerIndex1 = innerIndexes[publicIndex1];
                int innerIndex2 = innerIndexes[publicIndex2];
                publicIndexes[innerIndex1] = publicIndex2;
                publicIndexes[innerIndex2] = publicIndex1;
                innerIndexes[publicIndex1] = innerIndex2;
                innerIndexes[publicIndex2] = innerIndex1;
            }
            void MoveItemVirtual(int oldPublicIndex, int newPublicIndex) {
                if(oldPublicIndex < newPublicIndex) {
                    for(int i = oldPublicIndex; i < newPublicIndex; ++i)
                        SwapItemsVirtual(i, i + 1);
                } else {
                    for(int i = oldPublicIndex; i > newPublicIndex; --i)
                        SwapItemsVirtual(i, i - 1);
                }
            }

            public void Insert(int publicIndex, T item) {
                UpdateIndexMap();
                bool toFirstList = addItemToFirstList(item);
                int innerIndex;
                if(toFirstList) {
                    int skip = publicIndexes.Take(list1.Count).Reverse().TakeWhile(p => p >= publicIndex).Count();
                    innerIndex = list1.Count - skip;
                    list1.Insert(innerIndex, (T1)item);
                } else {
                    int skip = publicIndexes.Skip(list1.Count).Reverse().TakeWhile(p => p >= publicIndex).Count();
                    innerIndex = list1.Count + (list2.Count - skip);
                    list2.Insert(innerIndex - list1.Count, (T2)item);
                }
                for(int i = innerIndex; i < publicIndexes.Count; ++i)
                    ++innerIndexes[publicIndexes[i]];
                innerIndexes.Add(innerIndex);
                publicIndexes.Insert(innerIndex, innerIndexes.Count - 1);
                MoveItemVirtual(innerIndexes.Count - 1, publicIndex);
            }
            public bool Remove(T item) {
                int index = IndexOf(item);
                if(index < 0) return false;
                RemoveAt(index);
                return true;
            }
            public void RemoveAt(int publicIndex) {
                UpdateIndexMap();
                int innerIndex = innerIndexes[publicIndex];
                MoveItemVirtual(publicIndex, innerIndexes.Count - 1);
                publicIndexes.RemoveAt(innerIndex);
                innerIndexes.RemoveAt(innerIndexes.Count - 1);
                for(int i = innerIndex; i < publicIndexes.Count; ++i)
                    --innerIndexes[publicIndexes[i]];
                if(innerIndex < list1.Count)
                    list1.RemoveAt(innerIndex);
                else
                    list2.RemoveAt(innerIndex - list1.Count);
            }

            T GetItemCore(int index) {
                int innerIndex = innerIndexes[index];
                if(innerIndex < list1.Count)
                    return list1[innerIndex];
                else
                    return list2[innerIndex - list1.Count];
            }
            public T this[int index] {
                get {
                    UpdateIndexMap();
                    return GetItemCore(index);
                }
                set {
                    UpdateIndexMap();
                    int innerIndex = innerIndexes[index];
                    if(innerIndex < list1.Count)
                        list1[innerIndex] = (T1)value;
                    else
                        list2[innerIndex - list1.Count] = (T2)value;
                }
            }
            public int Count { get { return list1.Count + list2.Count; } }
            bool ICollection<T>.IsReadOnly { get { return list1.IsReadOnly || list2.IsReadOnly; } }
            public void Add(T item) {
                Insert(Count, item);
            }
            public void Clear() {
                list1.Clear();
                list2.Clear();
                publicIndexes.Clear();
                innerIndexes.Clear();
            }
            public bool Contains(T item) {
                if(item == null)
                    return list1.Contains(default(T1)) || list2.Contains(default(T2));
                if(item is T1 && list1.Contains((T1)item)) return true;
                if(item is T2 && list2.Contains((T2)item)) return true;
                return false;
            }
            public int IndexOf(T item) {
                UpdateIndexMap();
                if(item == null) {
                    int index1 = list1.IndexOf(default(T1));
                    if(index1 >= 0) return publicIndexes[index1];
                    int index2 = list2.IndexOf(default(T2));
                    if(index2 >= 0) return publicIndexes[index2 + list1.Count];
                }
                if(item is T1) {
                    int index1 = list1.IndexOf((T1)item);
                    if(index1 >= 0) return publicIndexes[index1];
                }
                if(item is T2) {
                    int index2 = list2.IndexOf((T2)item);
                    if(index2 >= 0) return publicIndexes[index2 + list1.Count];
                }
                return -1;
            }
            void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
                foreach(var item in this)
                    array[arrayIndex++] = item;
            }
            public IEnumerator<T> GetEnumerator() {
                UpdateIndexMap();
                int count = publicIndexes.Count;
                for(int i = 0; i < count; ++i)
                    yield return GetItemCore(i);
            }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
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
        public static IList<T> FromTwoLists<T1, T2>(IList<T1> list1, IList<T2> list2, Func<T, bool> addItemToFirstList = null)
            where T1 : T
            where T2 : T {

            return new CombinedList<T1, T2>(list1, list2, addItemToFirstList ?? (x => x is T1));
        }

        public static IList<T> FromTwoObjectLists<T1, T2>(IList objectList1, IList objectlist2, Func<T, bool> addItemToFirstList = null)
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