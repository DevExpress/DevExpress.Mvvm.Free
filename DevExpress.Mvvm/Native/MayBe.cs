using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

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
    public static class LinqExtensions {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach(T t in source)
                action(t);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action) {
            int index = 0;
            foreach(T t in source)
                action(t, index++);
        }
        public static void ForEach<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Action<TFirst, TSecond> action) {
            var en1 = first.GetEnumerator();
            var en2 = second.GetEnumerator();
            while(en1.MoveNext() && en2.MoveNext()) {
                action(en1.Current, en2.Current);
            }
        }
        public static IEnumerable<T> Unfold<T>(T seed, Func<T, T> next, Func<T, bool> stop) {
            for(var current = seed; !stop(current); current = next(current)) {
                yield return current;
            }
        }
        public static IEnumerable<T> Yield<T>(this T singleElement) {
            yield return singleElement;
        }
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getItems) {
            return source.SelectMany(item => item.Yield().Concat(getItems(item).Flatten(getItems)));
        }
        public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) {
            var comparer = Comparer<TKey>.Default;
            return source.Aggregate((x, y) => comparer.Compare(keySelector(x), keySelector(y)) < 0 ? x : y);
        }
        public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) {
            var comparer = Comparer<TKey>.Default;
            return source.Aggregate((x, y) => comparer.Compare(keySelector(x), keySelector(y)) > 0 ? x : y);
        }
#if !NETFX_CORE
#if !SILVERLIGHT && !FREE
        public static IEnumerable<T> FlattenFromWithinForward<T>(this IEnumerable<T> rootPath, Func<T, IList<T>> getChildren, Func<IList<T>, T, int> indedOf = null) where T : class {
            return rootPath.FlattenFromWithin(getChildren, indedOf, (s, gc, io) => GetNextElementInHierarchyCore(s, gc, io, skipChildren: false));
        }
        public static IEnumerable<T> FlattenFromWithinBackward<T>(this IEnumerable<T> rootPath, Func<T, IList<T>> getChildren, Func<IList<T>, T, int> indedOf = null)where T : class {
            return rootPath.FlattenFromWithin(getChildren, indedOf, GetPrevElementInHierarchyCore);
        }
        static IEnumerable<T> FlattenFromWithin<T>(this IEnumerable<T> rootPath, Func<T, IList<T>> getChildren, Func<IList<T>, T, int> indedOf, Func<IImmutableStack<T>, Func<T, IList<T>>, Func<IList<T>, T, int>, IImmutableStack<T>> getNextElement) where T : class {
            indedOf = indedOf ?? ((list, item) => list.IndexOf(item));
            var originalStack = rootPath.ToImmutableStack();
            Func<IImmutableStack<T>, IImmutableStack<T>> next = x => {
                var nextElement = getNextElement(x, getChildren, indedOf);
                return nextElement.Peek() == originalStack.Peek() ? null : nextElement;
            };
            return Unfold(originalStack, next, x => x == null).Select(x => x.Peek());
        }

        static IImmutableStack<T> GetNextElementInHierarchyCore<T>(IImmutableStack<T> rootStack, Func<T, IList<T>> getChildren, Func<IList<T>, T, int> indedOf, bool skipChildren) where T : class {
            var currentElement = rootStack.Peek();
            var children = getChildren(currentElement);
            if(!skipChildren && children.Any())
                return rootStack.Push(children.First());

            var parents = rootStack.Pop();
            var parent = parents.FirstOrDefault();
            if(parent == null)
                return rootStack;

            var neighbors = getChildren(parent);
            var index = indedOf(neighbors, currentElement);
            if(index < neighbors.Count - 1)
                return parents.Push(neighbors[index + 1]);

            return GetNextElementInHierarchyCore(parents, getChildren, indedOf, skipChildren: true);
        }

        static IImmutableStack<T> GetPrevElementInHierarchyCore<T>(IImmutableStack<T> rootStack, Func<T, IList<T>> getChildren, Func<IList<T>, T, int> indedOf) where T : class {
            var currentElement = rootStack.Peek();

            Func<T, IEnumerable<T>> getChildrenPath = element => Unfold(element, x => getChildren(x).LastOrDefault(), x => x == null);

            var parents = rootStack.Pop();
            var parent = parents.FirstOrDefault();
            if(parent == null) {
                return ImmutableStack.Empty<T>().PushMultiple(getChildrenPath(currentElement));
            }

            var neighbors = getChildren(parent);
            var index = indedOf(neighbors, currentElement);
            if(index > 0) {
                return parents.PushMultiple(getChildrenPath(neighbors[index - 1]));
            }

            return parents;
        }

#endif
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, ListSortDirection sortDirection) {
            return sortDirection == ListSortDirection.Ascending ?
                source.OrderBy(keySelector) :
                source.OrderByDescending(keySelector);
        }
#endif
    }
}