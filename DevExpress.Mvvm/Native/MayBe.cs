using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if MVVM
namespace DevExpress.Mvvm.Native {
#else
namespace DevExpress.Internal {
#endif
    [DebuggerStepThrough]
#if MVVM
    public
#endif
    static class MayBe {
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
            if(input == null)
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
#if MVVM
    public
#endif
    static class DictionaryExtensions {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> createValueDelegate) {
            TValue result;
            if(!dictionary.TryGetValue(key, out result)) {
                dictionary[key] = (result = createValueDelegate());
            }
            return result;
        }
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> createValueDelegate) {
            TValue result;
            if(!dictionary.TryGetValue(key, out result)) {
                dictionary[key] = (result = createValueDelegate(key));
            }
            return result;
        }
#if !NETFX_CORE
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            TValue result;
            dictionary.TryGetValue(key, out result);
            return result;
        }
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) {
            TValue result;
            if(dictionary.TryGetValue(key, out result))
                return result;
            return defaultValue;
        }
#endif
    }
#if MVVM
    public
#endif
    static class EmptyArray<TElement> {
        public static readonly TElement[] Instance = new TElement[0];
    }
#if MVVM
    public
#endif
    struct UnitT { }
#if MVVM
    public
#endif
    static class LinqExtensions {
        public static bool IsEmptyOrSingle<T>(this IEnumerable<T> source) {
            return !source.Any() || !source.Skip(1).Any();
        }
        public static bool IsSingle<T>(this IEnumerable<T> source) {
            return source.Any() && !source.Skip(1).Any();
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            if(source == null)
                return;
            foreach(T t in source)
                action(t);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action) {
            if(source == null)
                return;
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
        public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> predicate) {
            int index = 0;
            foreach(var item in source) {
                if(predicate(item))
                    return index;
                index++;
            }
            return -1;
        }
        public static TAccumulate AggregateUntil<T, TAccumulate>(this IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, bool> stop) {
            foreach(var item in source) {
                if(stop(seed)) break;
                seed = func(seed, item);
            }
            return seed;
        }
        public static IEnumerable<T> Unfold<T>(T seed, Func<T, T> next, Func<T, bool> stop) {
            for(var current = seed; !stop(current); current = next(current)) {
                yield return current;
            }
        }
        public static IEnumerable<T> Yield<T>(this T singleElement) {
            yield return singleElement;
        }
        public static IEnumerable<T> YieldIfNotNull<T>(this T singleElement) {
            if(singleElement != null) //-V3111
                yield return singleElement;
        }
        public static IEnumerable<string> YieldIfNotEmpty(this string singleElement) {
            if(!string.IsNullOrEmpty(singleElement))
                yield return singleElement;
        }
        public static T[] YieldToArray<T>(this T singleElement) {
            return new[] { singleElement };
        }
        public static T[] YieldIfNotNullToArray<T>(this T singleElement) {
            return singleElement == null ? EmptyArray<T>.Instance : new[] { singleElement };
        }
        public static string[] YieldIfNotEmptyToArray(this string singleElement) {
            return string.IsNullOrEmpty(singleElement) ? EmptyArray<string>.Instance : new[] { singleElement };
        }
        public static void ForEach<T>(this IEnumerable<T> source, Func<T, int, IEnumerable<T>> getItems, Action<T, int> action) {
            source.ForEachCore(getItems, action, 0);
        }
        static void ForEachCore<T>(this IEnumerable<T> source, Func<T, int, IEnumerable<T>> getItems, Action<T, int> action, int level) {
            source.ForEach(x => action(x, level));
            if(source.Any())
                source.SelectMany(x => getItems(x, level + 1)).ForEachCore(getItems, action, level + 1);
        }
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getItems) {
            return source.SelectMany(item => item.Yield().Concat(getItems(item).Flatten(getItems)));
        }
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, int, IEnumerable<T>> getItems) {
            return source.FlattenCore(getItems, 0);
        }
        static IEnumerable<T> FlattenCore<T>(this IEnumerable<T> source, Func<T, int, IEnumerable<T>> getItems, int level) {
            return source.SelectMany(item => item.Yield().Concat(getItems(item, level).FlattenCore(getItems, level + 1)));
        }
        //public static IEnumerable<T> DepthSearch<T>(this T item, Func<T, IEnumerable<T>> getRelatedItems) {
        //    var set = new HashSet<T>();
        //    return item.Yield().Flatten(x => getRelatedItems(x).Where(y => set.Add(y)));
        //}
        public static T MinByLast<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : IComparable {
            var comparer = Comparer<TKey>.Default;
            return source.Aggregate((x, y) => comparer.Compare(keySelector(x), keySelector(y)) < 0 ? x : y);
        }
        public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : IComparable {
            var comparer = Comparer<TKey>.Default;
            return source.Aggregate((x, y) => comparer.Compare(keySelector(x), keySelector(y)) <= 0 ? x : y);
        }
        public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : IComparable {
            var comparer = Comparer<TKey>.Default;
            return source.Aggregate((x, y) => comparer.Compare(keySelector(x), keySelector(y)) >= 0 ? x : y);
        }
        public static IEnumerable<T> InsertDelimiter<T>(this IEnumerable<T> source, T delimiter) {
            var en = source.GetEnumerator();
            if(en.MoveNext())
                yield return en.Current;
            while(en.MoveNext()) {
                yield return delimiter;
                yield return en.Current;
            }
        }
        public static string ConcatStringsWithDelimiter(this IEnumerable<string> source, string delimiter) {
            var builder = new StringBuilder();
            foreach(var str in source.InsertDelimiter(delimiter)) {
                builder.Append(str);
            }
            return builder.ToString();
        }
#if !NETFX_CORE
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, ListSortDirection sortDirection) {
            return sortDirection == ListSortDirection.Ascending ?
                source.OrderBy(keySelector) :
                source.OrderByDescending(keySelector);
        }
#endif
        public static Func<T> Memoize<T>(this Func<T> getValue) {
            var lazy = new Lazy<T>(getValue);
            return () => lazy.Value;
        }
        public static Func<TIn, TOut> Memoize<TIn, TOut>(this Func<TIn, TOut> getValue) {
            var dict = new Dictionary<TIn, TOut>();
            return x => dict.GetOrAdd(x, getValue);
        }
        public static T WithReturnValue<T>(this Func<Lazy<T>, T> func) {
            var t = default(T);
            var tHasValue = false;
            t = func(new Lazy<T>(() => {
                if(!tHasValue)
                    throw new InvalidOperationException("Fix");
                return t;
            }));
            tHasValue = true;
            return t;
        }
        public static bool AllEqual<T>(this IEnumerable<T> source, Func<T, T, bool> comparer = null) {
            if(!source.Any())
                return true;
            comparer = comparer ?? ((x, y) => Equals(x, y));
            var first = source.First();
            return source.Skip(1).All(x => comparer(x, first));
        }
        public static Action CombineActions(params Action[] actions) {
            return () => actions.ForEach(x => x());
        }
        public static Action<T> CombineActions<T>(params Action<T>[] actions) {
            return p => actions.ForEach(x => x(p));
        }
        public static Action<T1, T2> CombineActions<T1, T2>(params Action<T1, T2>[] actions) {
            return (p1, p2) => actions.ForEach(x => x(p1, p2));
        }
        public static ReadOnlyObservableCollection<T> ToReadOnlyObservableCollection<T>(this IEnumerable<T> source) {
            return new ReadOnlyObservableCollection<T>(source.ToObservableCollection());
        }
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source) {
            return new ObservableCollection<T>(source);
        }
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source) {
            return source.ToList().AsReadOnly();
        }
    }
#if !NETFX_CORE && !FREE
#if MVVM
    public
#endif
    static class DelegateExtensions {
#region ThisOrEmpty
        public static Action ThisOrEmpty(this Action action) {
            return action ?? (() => { });
        }
        public static Action ThisOrDefault(this Action action, Action defaultAction) {
            return action ?? (() => defaultAction());
        }
        public static Action<T> ThisOrDefault<T>(this Action<T> action, Action defaultAction) {
            return action ?? (x => defaultAction());
        }
        public static Action<T1, T2> ThisOrDefault<T1, T2>(this Action<T1, T2> action, Action defaultAction) {
            return action ?? ((x1, x2) => defaultAction());
        }
        public static Action<T1, T2, T3> ThisOrDefault<T1, T2, T3>(this Action<T1, T2, T3> action, Action defaultAction) {
            return action ?? ((x1, x2, x3) => defaultAction());
        }
        internal static Func<TR> ThisOrDefault<TR>(this Func<TR> func, TR defaultValue) {
            return func ?? (() => defaultValue);
        }
        internal static Func<T, TR> ThisOrDefault<T, TR>(this Func<T, TR> func, TR defaultValue) {
            return func ?? (x => defaultValue);
        }
        internal static Func<T1, T2, TR> ThisOrDefault<T1, T2, TR>(this Func<T1, T2, TR> func, TR defaultValue) {
            return func ?? ((x1, x2) => defaultValue);
        }
        internal static Func<T1, T2, T3, TR> ThisOrDefault<T1, T2, T3, TR>(this Func<T1, T2, T3, TR> func, TR defaultValue) {
            return func ?? ((x1, x2, t3) => defaultValue);
        }
#endregion

#region ignored args action
        //internal static Action<TIgnored1, TIgnored2, TIgnored3> AddIgnoredArgs<TIgnored1, TIgnored2, TIgnored3>(this Action action, TIgnored1 typeMarker1, TIgnored2 typeMarker2, TIgnored3 typeMarker3) {
        //    return (ignored1, ignored2, ignored3) => action();
        //}
#endregion

#region ignored args func
        //internal static Func<TIgnored, TR> AddIgnoredArg<TIgnored, TR>(this Func<TR> func, TIgnored typeMarker) {
        //    return ignored => func();
        //}
        //internal static Func<TIgnored1, TIgnored2, TR> AddIgnoredArgs<TIgnored1, TIgnored2, TR>(this Func<TR> func, TIgnored1 typeMarker1, TIgnored2 typeMarker2) {
        //    return (ignored1, ignored2) => func();
        //}
        //internal static Func<TIgnored, T, TR> AddIgnoredArg0<TIgnored, T, TR>(this Func<T, TR> func, TIgnored typeMarker) {
        //    return (ignored, x) => func(x);
        //}
        //internal static Func<T, TIgnored, TR> AddIgnoredArg1<T, TIgnored, TR>(this Func<T, TR> func, TIgnored typeMarker) {
        //    return (x, ignored) => func(x);
        //}
        //internal static Func<TIgnored, T1, T2, TR> AddIgnoredArg0<TIgnored, T1, T2, TR>(this Func<T1, T2, TR> func, TIgnored typeMarker) {
        //    return (ignored, x1, x2) => func(x1, x2);
        //}
        //internal static Func<T1, TIgnored, T2, TR> AddIgnoredArg1<T1, TIgnored, T2, TR>(this Func<T1, T2, TR> func, TIgnored typeMarker) {
        //    return (x1, ignored, x2) => func(x1, x2);
        //}
        //internal static Func<T1, T2, TIgnored, TR> AddIgnoredArg2<T1, T2, TIgnored, TR>(this Func<T1, T2, TR> func, TIgnored typeMarker) {
        //    return (x1, x2, ignored) => func(x1, x2);
        //}
#endregion

#region ChangeType
        public static Action<TNew> ChangeArgType<T, TNew>(this Action<T> action, TNew ignore) where T : TNew {
            if(action == null)
                return null;
            return x => action((T)x);
        }
        public static Action<TNew, T2> ChangeArgType0<T, TNew, T2>(this Action<T, T2> action, TNew ignore) where T : TNew {
            if(action == null)
                return null;
            return (x, x2) => action((T)x, x2);
        }
        public static Action<TNew, T2, T3> ChangeArgType0<T, TNew, T2, T3>(this Action<T, T2, T3> action, TNew ignore) where T : TNew {
            if(action == null)
                return null;
            return (x, x2, x3) => action((T)x, x2, x3);
        }
#endregion

#region ToAction
        public static Action ToAction<TResult>(this Func<TResult> f, Action<TResult> setResult) {
            if(f == null)
                return null;
            return () => setResult(f());
        }
        public static Action<T> ToAction<T, TResult>(this Func<T, TResult> f, Action<TResult> setResult) {
            if(f == null)
                return null;
            return x => setResult(f(x));
        }
        public static Action<T1, T2> ToAction<T1, T2, TResult>(this Func<T1, T2, TResult> f, Action<TResult> setResult) {
            if(f == null)
                return null;
            return (x1, x2) => setResult(f(x1, x2));
        }
        public static Action<T1, T2, T3> ToAction<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> f, Action<TResult> setResult) {
            if(f == null)
                return null;
            return (x1, x2, x3) => setResult(f(x1, x2, x3));
        }
#endregion
    }
#if MVVM
    public
#endif
    struct Stated<TState, T> {
        public Stated(TState state, T value) {
            State = state;
            Value = value;
        }
        public readonly TState State;
        public readonly T Value;
    }
#if MVVM
    public
#endif
    static class Stated {
        public static Stated<TState, T> WithState<T, TState>(this T value, TState state) {
            return new Stated<TState, T>(state, value);
        }
        sealed class Ref<T> {
            public T Value;
            public Ref(T value) { Value = value; }
        }
        static IEnumerable<TOut> SelectManyCore<TIn, TOut, TState>(Ref<TState> state, IEnumerable<TIn> enumerable, Func<TIn, TState, Stated<TState, IEnumerable<TOut>>> selector) {
            foreach(var i in enumerable) {
                var step = selector(i, state.Value);
                state.Value = step.State;
                foreach(var o in step.Value)
                    yield return o;
            }
        }
        public static Stated<TState, TResult> SelectMany<TIn, TOut, TResult, TState>(this Stated<TState, IEnumerable<TIn>> statedEnumerable, Func<TIn, TState, Stated<TState, IEnumerable<TOut>>> selector, Func<IEnumerable<TOut>, TResult> consumer) {
            var state = new Ref<TState>(statedEnumerable.State);
            var result = consumer(SelectManyCore(state, statedEnumerable.Value, selector));
            return result.WithState(state.Value);
        }
        public static IEnumerable<TOut> SelectMany<TIn, TOut, TState>(this Stated<TState, IEnumerable<TIn>> statedEnumerable, Func<TIn, TState, Stated<TState, IEnumerable<TOut>>> selector) {
            var state = new Ref<TState>(statedEnumerable.State);
            return SelectManyCore(state, statedEnumerable.Value, selector);
        }
        static IEnumerable<TOut> SelectCore<TIn, TOut, TState>(Ref<TState> state, IEnumerable<TIn> enumerable, Func<TIn, TState, Stated<TState, TOut>> selector) {
            foreach(var i in enumerable) {
                var step = selector(i, state.Value);
                state.Value = step.State;
                yield return step.Value;
            }
        }
        public static Stated<TState, TResult> Select<TIn, TOut, TResult, TState>(this Stated<TState, IEnumerable<TIn>> statedEnumerable, Func<TIn, TState, Stated<TState, TOut>> selector, Func<IEnumerable<TOut>, TResult> consumer) {
            var state = new Ref<TState>(statedEnumerable.State);
            var result = consumer(SelectCore(state, statedEnumerable.Value, selector));
            return result.WithState(state.Value);
        }
        public static IEnumerable<TOut> Select<TIn, TOut, TState>(this Stated<TState, IEnumerable<TIn>> statedEnumerable, Func<TIn, TState, Stated<TState, TOut>> selector) {
            var state = new Ref<TState>(statedEnumerable.State);
            return SelectCore(state, statedEnumerable.Value, selector);
        }
        static IEnumerable<TOut> SelectUntilCore<TIn, TOut, TState>(Ref<TState> state, IEnumerable<TIn> enumerable, Func<TIn, TState, Stated<TState, TOut>> selector, Func<TState, bool> stop) {
            foreach(var i in enumerable) {
                if(stop(state.Value)) break;
                var step = selector(i, state.Value);
                state.Value = step.State;
                yield return step.Value;
            }
        }
        public static Stated<TState, TResult> SelectUntil<TIn, TOut, TResult, TState>(this Stated<TState, IEnumerable<TIn>> statedEnumerable, Func<TIn, TState, Stated<TState, TOut>> selector, Func<TState, bool> stop, Func<IEnumerable<TOut>, TResult> consumer) {
            var state = new Ref<TState>(statedEnumerable.State);
            var result = consumer(SelectUntilCore(state, statedEnumerable.Value, selector, stop));
            return result.WithState(state.Value);
        }
        public static IEnumerable<TOut> SelectUntil<TIn, TOut, TState>(this Stated<TState, IEnumerable<TIn>> statedEnumerable, Func<TIn, TState, Stated<TState, TOut>> selector, Func<TState, bool> stop) {
            var state = new Ref<TState>(statedEnumerable.State);
            return SelectUntilCore(state, statedEnumerable.Value, selector, stop);
        }
    }
#if MVVM
    public
#endif
    static class TreeExtensions {
        public static TResult FoldTree<T, TResult>(T root, Func<T, IEnumerable<T>> getChildren, Func<T, IEnumerable<TResult>, TResult> combineWithChildren) {
            var children = getChildren(root).Select(x => FoldTree(x, getChildren, combineWithChildren));
            return combineWithChildren(root, children);
        }

        public static IEnumerable<T> FlattenFromWithinForward<T>(this IEnumerable<T> rootPath, Func<T, IList<T>> getChildren, Func<IList<T>, T, int> indedOf = null) where T : class {
            return rootPath.FlattenFromWithin(getChildren, indedOf, (s, gc, io) => GetNextElementInHierarchyCore(s, gc, io, skipChildren: false));
        }
        public static IEnumerable<T> FlattenFromWithinBackward<T>(this IEnumerable<T> rootPath, Func<T, IList<T>> getChildren, Func<IList<T>, T, int> indedOf = null) where T : class {
            return rootPath.FlattenFromWithin(getChildren, indedOf, GetPrevElementInHierarchyCore);
        }
        static IEnumerable<T> FlattenFromWithin<T>(this IEnumerable<T> rootPath, Func<T, IList<T>> getChildren, Func<IList<T>, T, int> indedOf, Func<IImmutableStack<T>, Func<T, IList<T>>, Func<IList<T>, T, int>, IImmutableStack<T>> getNextElement) where T : class {
            indedOf = indedOf ?? ((list, item) => list.IndexOf(item));
            var originalStack = rootPath.ToImmutableStack();
            Func<IImmutableStack<T>, IImmutableStack<T>> next = x => {
                var nextElement = getNextElement(x, getChildren, indedOf);
                return nextElement.Peek() == originalStack.Peek() ? null : nextElement;
            };
            return LinqExtensions.Unfold(originalStack, next, x => x == null).Select(x => x.Peek());
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

            Func<T, IEnumerable<T>> getChildrenPath = element => LinqExtensions.Unfold(element, x => getChildren(x).LastOrDefault(), x => x == null);

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

        public static TreeWrapper<TNew, TValue> TransformTree<T, TNew, TValue, TState>(
            T root,
            TState state,
            Func<T, IEnumerable<T>> getChildren,
            Func<TreeWrapper<TNew, TValue>[], T, TState, TValue> getValue,
            Func<T, TNew> getItem,
            Func<TState, T, TState> getFirstChildState,
            Func<TState, T, TState> advanceChildState) {

            var childrenState = getFirstChildState(state, root);
            var children = getChildren(root)
                .Select(child => {
                    var result = TransformTree(child, childrenState, getChildren, getValue, getItem, getFirstChildState, advanceChildState);
                    childrenState = advanceChildState(childrenState, child);
                    return result;
                })
                .ToArray();
            return new TreeWrapper<TNew, TValue>(getItem(root), getValue(children, root, state), children);
        }
    }
#if MVVM
    public
#endif
    struct TreeWrapper<T, TValue> {
        public readonly T Item;
        public readonly TValue Value;
        public readonly TreeWrapper<T, TValue>[] Children;
        public TreeWrapper(T item, TValue value, TreeWrapper<T, TValue>[] children) {
            Item = item;
            Value = value;
            Children = children;
        }
    }
#endif

#if MVVM
    public
#endif
    sealed class TaskLinq<T> {
        public TaskLinq(Task<T> task, TaskLinq.Chain chain) {
            Task = task;
            Chain = chain;
        }
        internal readonly Task<T> Task;
        internal readonly TaskLinq.Chain Chain;
    }
#if MVVM
    public
#endif
    static class TaskLinq {
        public static TaskLinq<T> Linq<T>(this Task<T> task, TaskScheduler scheduler = null) {
            return task.Linq(new Chain(scheduler));
        }
        public static TaskLinq<T> Linq<T>(this Task<T> task, Chain chain) {
            return new TaskLinq<T>(task, chain);
        }
        public sealed class Chain {
            readonly RunFuture RunFuture = new RunFuture();
            public readonly SchedulerFuture SchedulerFuture;
            public Chain(SchedulerFuture schedulerFuture, Action run = null) {
                SchedulerFuture = schedulerFuture;
                RunFuture.Continue(run);
            }
            public Chain(TaskScheduler scheduler = null, Action run = null) : this(new SchedulerFuture(scheduler), run) { }
            public void Continue(Chain chain) {
                SchedulerFuture.Continue(chain.SchedulerFuture.Run);
                RunFuture.Continue(chain.RunFuture.Run);
            }
            public void Run(TaskScheduler scheduler) {
                if(scheduler == null)
                    throw new ArgumentNullException("scheduler");
                SchedulerFuture.Run(scheduler);
                RunFuture.Run();
            }
        }
        sealed class RunFuture {
            readonly object sync = new object();
            Action @continue;
            bool ran;
            public void Continue(Action action) {
                if(!ran) {
                    lock(sync) {
                        if(!ran) {
                            @continue += action;
                            return;
                        }
                    }
                }
                action?.Invoke();
            }
            public void Run() {
                Action action;
                if(!ran) {
                    lock(sync) {
                        if(!ran) {
                            ran = true;
                            action = @continue;
                            @continue = null;
                        } else {
                            return;
                        }
                    }
                } else {
                    return;
                }
                action?.Invoke();
            }
        }
        public sealed class SchedulerFuture {
            readonly object sync = new object();
#if DEBUG
            static int nextId = 0;
            public readonly int Id = Interlocked.Increment(ref nextId);
#endif
            TaskScheduler scheduler;
            Action<TaskScheduler> @continue;
            public SchedulerFuture(TaskScheduler scheduler = null) {
                this.scheduler = scheduler;
            }
            public void Continue(Action<TaskScheduler> action) {
                if(scheduler == null) {
                    lock(sync) {
                        if(scheduler == null) {
                            @continue += action;
                            return;
                        }
                    }
                }
                action(scheduler);
            }
            public void Run(TaskScheduler scheduler) {
                if(scheduler == null)
                    throw new ArgumentNullException("scheduler");
                Action<TaskScheduler> action;
                if(this.scheduler == null) {
                    lock(sync) {
                        if(this.scheduler == null) {
                            this.scheduler = scheduler;
                            action = @continue;
                            @continue = null;
                        } else {
                            CheckScheduler(scheduler);
                            return;
                        }
                    }
                } else {
                    CheckScheduler(scheduler);
                    return;
                }
                action?.Invoke(this.scheduler);
            }
            void CheckScheduler(TaskScheduler scheduler) {
                if((this.scheduler == TaskScheduler.Default) == (scheduler == TaskScheduler.Default)) return;
                int id;
#if DEBUG
                id = Id;
#else
                id = 0;
#endif
                throw new InvalidOperationException("SchedulerFuture: " + id + " " + this.scheduler.Id + " " + scheduler.Id + " " + TaskScheduler.Default.Id);
            }
        }
        public static SynchronizationContext RethrowAsyncExceptionsContext = null;
        public static TaskLinq<UnitT> LongRunning() { return StartNew(TaskCreationOptions.LongRunning); }
        public static TaskLinq<UnitT> ThreadPool() { return StartNew(TaskCreationOptions.None); }
#if DEBUG
        internal static int StartNewThreadId;
#endif
        static TaskLinq<UnitT> StartNew(TaskCreationOptions creationOptions) {
            var task = new Task<UnitT>(() => {
#if DEBUG
                StartNewThreadId = Thread.CurrentThread.ManagedThreadId;
#endif
                return default(UnitT);
            }, CancellationToken.None, creationOptions);
            var chain = new Chain(TaskScheduler.Default, () => task.Start(TaskScheduler.Default));
            return task.Linq(chain);
        }
        public static TaskLinq<UnitT> Wait(Func<Action, Action> subscribe, Func<bool> ready, TaskScheduler scheduler = null) {
            return Wait(subscribe, ready, new Chain(scheduler));
        }
        public static TaskLinq<UnitT> Wait(Func<Action, Action> subscribe, Func<bool> ready, Chain chain) {
            return ready() ? default(UnitT).Promise() : On(subscribe, chain);
        }
        public static TaskLinq<UnitT> On(Func<Action, Action> subscribe, TaskScheduler scheduler = null) {
            return On(subscribe, new Chain(scheduler));
        }
        public static TaskLinq<UnitT> On(Func<Action, Action> subscribe, Chain chain) {
            return On<UnitT>(x => subscribe(() => x(default(UnitT))), chain);
        }
        public static TaskLinq<T> On<T>(Func<Action<T>, Action> subscribe, TaskScheduler scheduler = null) {
            return On<T>(subscribe, new Chain(scheduler));
        }
        public static TaskLinq<T> On<T>(Func<Action<T>, Action> subscribe, Chain chain) {
            var taskSource = new TaskCompletionSource<T>();
            LinqExtensions.WithReturnValue<Action>(unsubscribe => subscribe(x => {
                unsubscribe.Value();
                taskSource.SetResult(x);
            }));
            return taskSource.Task.Linq(chain);
        }
        static Task<T> Run<T>(this TaskLinq<T> task, Chain chain) {
            chain.Continue(task.Chain);
            return task.Task;
        }
        static TaskScheduler InvalidScheduler() {
            throw new InvalidOperationException("TaskScheduler.Current == TaskScheduler.Default && SynchronizationContext.Current == null");
        }
        public static Task<T> Schedule<T>(this TaskLinq<T> task, TaskScheduler scheduler = null) {
            scheduler = scheduler ?? (TaskScheduler.Current != TaskScheduler.Default ? TaskScheduler.Current : SynchronizationContext.Current == null ? InvalidScheduler() : TaskScheduler.FromCurrentSynchronizationContext());
            return task.Schedule(new SchedulerFuture(scheduler));
        }
        public static Task<T> Schedule<T>(this TaskLinq<T> task, SchedulerFuture schedulerFuture) {
            schedulerFuture.Continue(task.Chain.Run);
            return task.Task;
        }
        public static Task<T> Future<T>(this T value) {
            var taskSource = new TaskCompletionSource<T>();
            taskSource.SetResult(value);
            return taskSource.Task;
        }
        public static Task<T> FutureException<T>(this Exception e) {
            var taskSource = new TaskCompletionSource<T>();
            taskSource.SetException(e);
            return taskSource.Task;
        }
        public static Task<T> FutureCanceled<T>() {
            var taskSource = new TaskCompletionSource<T>();
            taskSource.SetCanceled();
            return taskSource.Task;
        }
        public static TaskLinq<T> Promise<T>(this T value, Chain chain) { return value.Future().Linq(chain); }
        public static TaskLinq<T> PromiseException<T>(this Exception e, Chain chain) { return e.FutureException<T>().Linq(chain); }
        public static TaskLinq<T> PromiseCanceled<T>(Chain chain) { return FutureCanceled<T>().Linq(chain); }
        public static TaskLinq<T> Promise<T>(this T value, TaskScheduler scheduler = null) { return value.Promise(new Chain(scheduler)); }
        public static TaskLinq<T> PromiseException<T>(this Exception e, TaskScheduler scheduler = null) { return e.PromiseException<T>(new Chain(scheduler)); }
        public static TaskLinq<T> PromiseCanceled<T>(TaskScheduler scheduler = null) { return PromiseCanceled<T>(new Chain(scheduler)); }
        public static TaskLinq<T> Where<T>(this TaskLinq<T> task, Func<T, bool> predicate) {
            var chain = task.Chain;
            var taskSource = new TaskCompletionSource<T>();
            taskSource.SetResultFromTask(chain.SchedulerFuture, task.Task, (ts, taskResult) => {
                ts.SetResultSafe(() => predicate(taskResult), (ts_, predicateResult) => {
                    if(predicateResult)
                        ts_.SetResult(taskResult);
                    else
                        ts_.SetCanceled();
                });
            });
            return taskSource.Task.Linq(chain);
        }
        public static TaskLinq<UnitT> Where(this TaskLinq<UnitT> task, Func<bool> predicate) {
            return task.Where(_ => predicate());
        }
        public static TaskLinq<TR> Select<TI, TR>(this TaskLinq<TI> task, Func<TI, TR> selector) {
            var chain = task.Chain;
            var taskSource = new TaskCompletionSource<TR>();
            taskSource.SetResultFromTask(chain.SchedulerFuture, task.Task, (ts, taskResult) => ts.SetResultSafe(() => selector(taskResult)));
            return taskSource.Task.Linq(chain);
        }
        public static TaskLinq<UnitT> Select<TI>(this TaskLinq<TI> task, Action<TI> selector) {
            return task.Select(x => { selector(x); return default(UnitT); });
        }
        public static TaskLinq<TR> Select<TR>(this TaskLinq<UnitT> task, Func<TR> selector) {
            return task.Select(_ => selector());
        }
        public static TaskLinq<T> Select<T>(this TaskLinq<T> task, Action selector) {
            return task.Select(x => { selector(); return x; });
        }
        public static TaskLinq<TR> SelectMany<TI, TR>(this TaskLinq<TI> task, Func<TI, TaskLinq<TR>> selector) {
            return task.SelectMany(selector, (_, x) => x);
        }
        public static TaskLinq<TR> SelectMany<TR>(this TaskLinq<UnitT> task, Func<TaskLinq<TR>> selector) {
            return task.SelectMany(_ => selector());
        }
        public static TaskLinq<T> SelectUnit<T>(this TaskLinq<T> task, Func<TaskLinq<UnitT>> selector) {
            return task.SelectMany(x => selector().Select(() => x));
        }
        public static TaskLinq<TR> SelectMany<TI, TC, TR>(this TaskLinq<TI> task, Func<TI, TaskLinq<TC>> selector, Func<TI, TC, TR> projector) {
            var chain = task.Chain;
            var taskSource = new TaskCompletionSource<TR>();
            taskSource.SetResultFromTask(chain.SchedulerFuture, task.Task, (ts, taskResult) => ts.SetResultFromTaskSafe(chain.SchedulerFuture, () => selector(taskResult).Run(chain), (ts_, selectorResult) => ts_.SetResultSafe(() => projector(taskResult, selectorResult))));
            return taskSource.Task.Linq(chain);
        }
        public static Task Finish<T>(this Task<T> task) {
            return task.ContinueWith(t => {
                if(t.IsFaulted)
                    RethrowAsyncExceptionsContext.Do(x => x.Post(_ => { throw new AggregateException(t.Exception.InnerExceptions); }, default(UnitT)));
                return default(UnitT);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
        public static Task Execute(this TaskLinq<UnitT> task, Action action, TaskScheduler scheduler = null) {
            return task.Execute(_ => action(), scheduler);
        }
        public static Task Execute(this TaskLinq<UnitT> task, Action action, SchedulerFuture schedulerFuture) {
            return task.Execute(_ => action(), schedulerFuture);
        }
        public static Task Execute<T>(this TaskLinq<T> task, Action<T> action, TaskScheduler scheduler = null) {
            return task.Select(r => { action(r); return r; }).Schedule(scheduler).Finish();
        }
        public static Task Execute<T>(this TaskLinq<T> task, Action<T> action, SchedulerFuture schedulerFuture) {
            return task.Select(r => { action(r); return r; }).Schedule(schedulerFuture).Finish();
        }
        static void SetResultFromTask<T>(this TaskCompletionSource<T> taskSource, SchedulerFuture continueWithScheduler, Task<T> task, Action<TaskCompletionSource<T>> setCanceledAction = null, Action<TaskCompletionSource<T>, Exception> setExceptionAction = null) {
            taskSource.SetResultFromTask(continueWithScheduler, task, (ts, taskResult) => ts.SetResult(taskResult), setCanceledAction, setExceptionAction);
        }
        static void SetResultFromTask<TI, TR>(this TaskCompletionSource<TR> taskSource, SchedulerFuture continueWithScheduler, Task<TI> task, Action<TaskCompletionSource<TR>, TI> setResultAction, Action<TaskCompletionSource<TR>> setCanceledAction = null, Action<TaskCompletionSource<TR>, Exception> setExceptionAction = null) {
            setCanceledAction = setCanceledAction ?? (t => t.SetCanceled());
            setExceptionAction = setExceptionAction ?? ((t, e) => t.SetException(e));
            continueWithScheduler.Continue(scheduler => {
                task.ContinueWith(t => {
                    if(t.IsCanceled)
                        setCanceledAction(taskSource);
                    else if(t.IsFaulted)
                        setExceptionAction(taskSource, t.Exception.InnerException);
                    else
                        setResultAction(taskSource, t.Result);
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, scheduler);
            });
        }
        static void SetResultFromTaskSafe<T>(this TaskCompletionSource<T> taskSource, SchedulerFuture continueWithScheduler, Func<Task<T>> getTask, Action<TaskCompletionSource<T>> setCanceledAction = null, Action<TaskCompletionSource<T>, Exception> setExceptionAction = null) {
            taskSource.SetResultFromTaskSafe(continueWithScheduler, getTask, (ts, taskResult) => ts.SetResult(taskResult), setCanceledAction, setExceptionAction);
        }
        static void SetResultFromTaskSafe<TI, TR>(this TaskCompletionSource<TR> taskSource, SchedulerFuture continueWithScheduler, Func<Task<TI>> getTask, Action<TaskCompletionSource<TR>, TI> setResultAction, Action<TaskCompletionSource<TR>> setCanceledAction = null, Action<TaskCompletionSource<TR>, Exception> setExceptionAction = null) {
            Task<TI> task;
            try {
                task = getTask();
            } catch(Exception e) {
                taskSource.SetException(e);
                return;
            }
            taskSource.SetResultFromTask(continueWithScheduler, task, setResultAction, setCanceledAction, setExceptionAction);
        }
        public static TaskLinq<T> IfException<T>(this TaskLinq<T> task, Func<Exception, TaskLinq<T>> handler) {
            var chain = task.Chain;
            var result = new TaskCompletionSource<T>();
            result.SetResultFromTask(chain.SchedulerFuture, task.Task, setExceptionAction: (r, e) => r.SetResultFromTaskSafe(chain.SchedulerFuture, () => handler(e).Run(chain)));
            return result.Task.Linq(chain);
        }
        public static TaskLinq<T> IfException<T>(this TaskLinq<T> task, Func<Exception, T> handler) {
            var chain = task.Chain;
            var result = new TaskCompletionSource<T>();
            result.SetResultFromTask(chain.SchedulerFuture, task.Task, setExceptionAction: (r, e) => r.SetResultSafe(() => handler(e)));
            return result.Task.Linq(chain);
        }
        public static TaskLinq<T> MapException<T>(this TaskLinq<T> task, Func<Exception, Exception> transform) {
            var chain = task.Chain;
            var result = new TaskCompletionSource<T>();
            result.SetResultFromTask(chain.SchedulerFuture, task.Task, setExceptionAction: (r, e) => r.SetExceptionSafe(() => transform(e)));
            return result.Task.Linq(chain);
        }
        public static TaskLinq<T> IfCanceled<T>(this TaskLinq<T> task, Func<Task<T>> handler) {
            var chain = task.Chain;
            var result = new TaskCompletionSource<T>();
            result.SetResultFromTask(chain.SchedulerFuture, task.Task, setCanceledAction: r => r.SetResultFromTaskSafe(chain.SchedulerFuture, handler));
            return result.Task.Linq(chain);
        }
        public static TaskLinq<T> IfCanceled<T>(this TaskLinq<T> task, Func<T> handler) {
            var chain = task.Chain;
            var result = new TaskCompletionSource<T>();
            result.SetResultFromTask(chain.SchedulerFuture, task.Task, setCanceledAction: r => r.SetResultSafe(handler));
            return result.Task.Linq(chain);
        }
        static void SetExceptionSafe<T>(this TaskCompletionSource<T> taskSource, Func<Exception> getException) {
            taskSource.SetResultSafe(() => {
                var e = getException();
                if(e == null)
                    throw new InvalidOperationException("getException() == null");
                return e;
            }, (ts, result) => ts.SetException(result));
        }
        static void SetResultSafe<T>(this TaskCompletionSource<T> taskSource, Func<T> getResult) {
            taskSource.SetResultSafe(getResult, (ts, result) => ts.SetResult(result));
        }
        static void SetResultSafe<TI, TR>(this TaskCompletionSource<TR> taskSource, Func<TI> getResult, Action<TaskCompletionSource<TR>, TI> setResultAction) {
            TI result;
            Exception exception;
            try {
                result = getResult();
                exception = null;
            } catch(Exception e) {
                result = default(TI);
                exception = e;
            }
            if(exception != null)
                taskSource.SetException(exception);
            else
                setResultAction(taskSource, result);
        }
        public static TaskLinq<T> WithDefaultScheduler<T>(Func<TaskLinq<T>> action) {
            if(TaskScheduler.Current == TaskScheduler.Default)
                return action();
            var synchronizationContext = SynchronizationContext.Current;
            if(synchronizationContext == null)
                throw new InvalidOperationException("SynchronizationContext.Current == null");
            var linqScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var continueTask = new TaskCompletionSource<TaskLinq<T>>();
            var startTask = new TaskCompletionSource<UnitT>();
            startTask.SetResult(default(UnitT));
            startTask.Task.ContinueWith(_ => {
                if(Thread.CurrentThread.ManagedThreadId == threadId)
                    SetResultSafe(continueTask, action);
                else
                    synchronizationContext.Post(__ => SetResultSafe(continueTask, action), default(UnitT));
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return continueTask.Task.Linq(linqScheduler).SelectMany(x => x);
        }
    }
}