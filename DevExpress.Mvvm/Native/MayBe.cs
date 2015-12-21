using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DevExpress.Mvvm.Native {
    [DebuggerStepThrough]
    public
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
    public
    static class DictionaryExtensions {
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
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) {
            TValue result;
            if(dictionary.TryGetValue(key, out result))
                return result;
            return defaultValue;
        }
    }
    public
    static class EmptyArray<TElement> {
        public static readonly TElement[] Instance = new TElement[0];
    }
    public
    static class LinqExtensions {
        public static bool IsEmptyOrSingle<T>(this IEnumerable<T> source) {
            return !source.Any() || !source.Skip(1).Any();
        }
        public static bool IsSingle<T>(this IEnumerable<T> source) {
            return source.Any() && !source.Skip(1).Any();
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            if (source == null)
                return;
            foreach (T t in source)
                action(t);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action) {
            if (source == null)
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
        public static IEnumerable<T> Unfold<T>(T seed, Func<T, T> next, Func<T, bool> stop) {
            for(var current = seed; !stop(current); current = next(current)) {
                yield return current;
            }
        }
        public static IEnumerable<T> Yield<T>(this T singleElement) {
            yield return singleElement;
        }
        public static IEnumerable<T> YieldIfNotNull<T>(this T singleElement) {
            if(singleElement != null)
                yield return singleElement;
        }
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getItems) {
            return source.SelectMany(item => item.Yield().Concat(getItems(item).Flatten(getItems)));
        }
        public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : IComparable {
            var comparer = Comparer<TKey>.Default;
            return source.Aggregate((x, y) => comparer.Compare(keySelector(x), keySelector(y)) < 0 ? x : y);
        }
        public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : IComparable {
            var comparer = Comparer<TKey>.Default;
            return source.Aggregate((x, y) => comparer.Compare(keySelector(x), keySelector(y)) > 0 ? x : y);
        }
        public static IEnumerable<T> InsertDelimeter<T>(this IEnumerable<T> source, T delimeter) {
            var en = source.GetEnumerator();
            if(en.MoveNext())
                yield return en.Current;
            while(en.MoveNext()) {
                yield return delimeter;
                yield return en.Current;
            }
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
    }
#if !NETFX_CORE && !FREE
    public
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
    public
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

    public
    static class TaskExtensions {
        public static Task<T> Promise<T>(this T value) {
            var taskSource = new TaskCompletionSource<T>();
            taskSource.SetResult(value);
            return taskSource.Task;
        }
        public static Task<T> Where<T>(this Task<T> task, Func<T, bool> predicate) {
            var taskSource = new TaskCompletionSource<T>();
            taskSource.SetResultFromTask(task, (ts, taskResult) => {
                ts.SetResultSafe(() => predicate(taskResult), (ts_, predicateResult) => {
                    if(predicateResult)
                        ts_.SetResult(taskResult);
                    else
                        ts_.SetCanceled();
                });
            });
            return taskSource.Task;
        }
        public static Task<TR> Select<TI, TR>(this Task<TI> task, Func<TI, TR> selector) {
            var taskSource = new TaskCompletionSource<TR>();
            taskSource.SetResultFromTask(task, (ts, taskResult) => ts.SetResultSafe(() => selector(taskResult)));
            return taskSource.Task;
        }
        public static Task<TR> SelectMany<TI, TC, TR>(this Task<TI> task, Func<TI, Task<TC>> selector, Func<TI, TC, TR> projector) {
            var taskSource = new TaskCompletionSource<TR>();
            taskSource.SetResultFromTask(task, (ts, taskResult) => ts.SetResultFromTaskSafe(() => selector(taskResult), (ts_, selectorResult) => ts_.SetResultSafe(() => projector(taskResult, selectorResult))));
            return taskSource.Task;
        }
        public static Task Execute<T>(this Task<T> task, Action<T> action) {
            return task.Select(x => {
                action(x);
                return true;
            }).ContinueWith(_ => { }, TaskContinuationOptions.ExecuteSynchronously);
        }
        public static void SetResultFromTask<T>(this TaskCompletionSource<T> taskSource, Task<T> task) {
            taskSource.SetResultFromTask(task, (ts, taskResult) => ts.SetResult(taskResult));
        }
        public static void SetResultFromTask<TI, TR>(this TaskCompletionSource<TR> taskSource, Task<TI> task, Action<TaskCompletionSource<TR>, TI> setResultAction) {
            task.ContinueWith(t => {
                if(t.IsCanceled)
                    taskSource.SetCanceled();
                else if(t.IsFaulted)
                    taskSource.SetException(t.Exception);
                else
                    setResultAction(taskSource, t.Result);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
        public static void SetResultFromTaskSafe<T>(this TaskCompletionSource<T> taskSource, Func<Task<T>> getTask) {
            taskSource.SetResultFromTaskSafe(getTask, (ts, taskResult) => ts.SetResult(taskResult));
        }
        public static void SetResultFromTaskSafe<TI, TR>(this TaskCompletionSource<TR> taskSource, Func<Task<TI>> getTask, Action<TaskCompletionSource<TR>, TI> setResultAction) {
            Task<TI> task;
            try {
                task = getTask();
            } catch(Exception e) {
                taskSource.SetException(e);
                return;
            }
            taskSource.SetResultFromTask(task, setResultAction);
        }
        public static void SetResultSafe<T>(this TaskCompletionSource<T> taskSource, Func<T> getResult) {
            taskSource.SetResultSafe(getResult, (ts, result) => ts.SetResult(result));
        }
        public static void SetResultSafe<TI, TR>(this TaskCompletionSource<TR> taskSource, Func<TI> getResult, Action<TaskCompletionSource<TR>, TI> setResultAction) {
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
    }
}