using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DevExpress.Mvvm.UI.Native {
    public class EnumerableWrap<TItem> : IEnumerable<TItem> {
        readonly Func<object, object> convertMethod;

        public EnumerableWrap(IEnumerable innerCollection, Func<object, object> convertMethod) {
            if(innerCollection == null)
                throw new ArgumentNullException("innerCollection");
            if(convertMethod == null)
                throw new ArgumentNullException("convertMethod");
            this.convertMethod = convertMethod;
            InnerEnumerable = innerCollection;
        }
        public IEnumerator<TItem> GetEnumerator() {
            foreach(var item in InnerEnumerable)
                yield return Convert(item);
        }
        public void CopyTo(Array array, int index) {
            CopyTo((TItem[])array, index);
        }
        public void CopyTo(TItem[] array, int arrayIndex) {
            foreach(var item in this)
                array[arrayIndex++] = item;
        }
        protected IEnumerable InnerEnumerable { get; private set; }
        protected TItem Convert(object innerItem) { return (TItem)convertMethod(innerItem); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}