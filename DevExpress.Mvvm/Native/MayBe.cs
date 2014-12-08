using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevExpress.Mvvm.Native {
    [DebuggerStepThrough]
    public static class MayBe {
        public static TR With<TI, TR>(this TI input, Func<TI, TR> evaluator)
            where TI : class
            where TR : class {
            if(input == null)
                return null;
            return evaluator(input);
        }
        public static TR WithString<TR>(this string input, Func<string, TR> evaluator)
            where TR : class {
            if(string.IsNullOrEmpty(input))
                return null;
            return evaluator(input);
        }
        public static TR Return<TI, TR>(this TI? input, Func<TI?, TR> evaluator, Func<TR> fallback) where TI : struct {
            if(!input.HasValue)
                return fallback != null ? fallback() : default(TR);
            return evaluator(input.Value);
        }
        public static TR Return<TI, TR>(this TI input, Func<TI, TR> evaluator, Func<TR> fallback) where TI : class {
            if(input == null)
                return fallback != null ? fallback() : default(TR);
            return evaluator(input);
        }
        public static bool ReturnSuccess<TI>(this TI input) where TI : class {
            return input != null;
        }
        public static TI If<TI>(this TI input, Func<TI, bool> evaluator) where TI : class {
            if(input == null)
                return null;
            return evaluator(input) ? input : null;
        }
        public static TI IfNot<TI>(this TI input, Func<TI, bool> evaluator) where TI : class {
            if (input == null)
                return null;
            return evaluator(input) ? null : input;
        }
        public static TI Do<TI>(this TI input, Action<TI> action) where TI : class {
            if(input == null)
                return null;
            action(input);
            return input;
        }
    }
    public static class DictionaryExtensions {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> createValueDelegate) {
            TValue result;
            if(!dictionary.TryGetValue(key, out result)) {
                dictionary[key] = (result = createValueDelegate());
            }
            return result;
        }
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            TValue result;
            dictionary.TryGetValue(key, out result);
            return result;
        }
    }
}