using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevExpress.Mvvm {
    public abstract class ImmutableCollectionCore<T, TCollection> : ReadOnlyCollection<T> where TCollection : ReadOnlyCollection<T> {
        protected ImmutableCollectionCore()
            : base(new List<T>()) { }
        protected ImmutableCollectionCore(IEnumerable<T> values)
            : base(values.ToList()) { }

        public TCollection SetElementAt(int index, T value) {
            var clone = this.ToArray();
            clone[index] = value;
            return Create(clone);
        }
        internal TCollection SelectCore(Func<T, T> selector) {
            return Create(Enumerable.Select(this, selector));
        }
        internal TCollection SelectCore(Func<T, int, T> selector) {
            return Create(Enumerable.Select(this, selector));
        }
        protected abstract TCollection Create(IEnumerable<T> values);
    }
}