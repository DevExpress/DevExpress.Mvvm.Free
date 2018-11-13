using System;
using System.Collections.Generic;

namespace DevExpress.Mvvm.Internal {
    public class FuzzyKeyValuePair<TKey, TValue> {
        public FuzzyKeyValuePair(TKey key, TValue value, bool useIncludeCondition) {
            Key = key;
            Value = value;
            UseIncludeCondition = useIncludeCondition;
        }
        public TKey Key { get; private set; }
        public TValue Value { get; private set; }
        public bool UseIncludeCondition { get; private set; }
    }
    public class FuzzyDictionary<TKey, TValue> : IEnumerable<FuzzyKeyValuePair<TKey, TValue>> {
        Func<TKey, TKey, bool> includeCondition;
        Dictionary<TKey, TValue> exactlyPairs = new Dictionary<TKey, TValue>();
        Dictionary<TKey, TValue> otherPairs = new Dictionary<TKey, TValue>();

        public FuzzyDictionary(Func<TKey, TKey, bool> includeCondition) {
            this.includeCondition = includeCondition;
        }
        public void Add(TKey key, TValue value, bool useIncludeCondition = false) {
            if(useIncludeCondition && includeCondition != null)
                otherPairs.Add(key, value);
            else
                exactlyPairs.Add(key, value);
        }
        public void Remove(TKey key, bool useIncludeCondition) {
            if(useIncludeCondition && includeCondition != null)
                otherPairs.Remove(key);
            else
                exactlyPairs.Remove(key);
        }
        public bool TryGetValue(TKey key, bool useIncludeCondition, out TValue value) {
            if(useIncludeCondition && includeCondition != null)
                return otherPairs.TryGetValue(key, out value);
            else
                return exactlyPairs.TryGetValue(key, out value);
        }
        public IEnumerable<TValue> GetValues(TKey key) {
            return key == null ? Values : GetValuesCore(key);
        }
        public IEnumerable<TValue> Values {
            get {
                foreach(var pair in exactlyPairs)
                    yield return pair.Value;
                foreach(var pair in otherPairs)
                    yield return pair.Value;
            }
        }
        IEnumerable<TValue> GetValuesCore(TKey key) {
            TValue value;
            if(exactlyPairs.TryGetValue(key, out value)) yield return value;
            foreach(var pair in otherPairs) {
                if(includeCondition(pair.Key, key)) yield return pair.Value;
            }
        }
        IEnumerator<FuzzyKeyValuePair<TKey, TValue>> IEnumerable<FuzzyKeyValuePair<TKey, TValue>>.GetEnumerator() {
            foreach(var pair in exactlyPairs)
                yield return new FuzzyKeyValuePair<TKey, TValue>(pair.Key, pair.Value, false);
            foreach(var pair in otherPairs)
                yield return new FuzzyKeyValuePair<TKey, TValue>(pair.Key, pair.Value, true);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            foreach(var pair in exactlyPairs)
                yield return new FuzzyKeyValuePair<TKey, TValue>(pair.Key, pair.Value, false);
            foreach(var pair in otherPairs)
                yield return new FuzzyKeyValuePair<TKey, TValue>(pair.Key, pair.Value, true);
        }
    }
}