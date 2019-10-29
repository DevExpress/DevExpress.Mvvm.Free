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
    public
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
#if !NETFX_CORE && (!DXCORE3 || MVVM)
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
    public
    static class EmptyArray<TElement> {
        public static readonly TElement[] Instance = new TElement[0];
    }
    public
    struct UnitT { }
    public
    sealed class VoidT {
        VoidT() { }
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
            using(var en1 = first.GetEnumerator())
                using(var en2 = second.GetEnumerator()) {
                    while(en1.MoveNext() && en2.MoveNext()) {
                        action(en1.Current, en2.Current);
                    }
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
            if(singleElement != null)
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
            return source.Flatten((x, _) => getItems(x));
        }
        struct EnumeratorAndLevel<T> {
            public readonly IEnumerator<T> En;
            public readonly int Level;
            public EnumeratorAndLevel(IEnumerator<T> en, int level) {
                En = en;
                Level = level;
            }
        }
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, int, IEnumerable<T>> getItems) {
            var stack = new Stack<EnumeratorAndLevel<T>>();
            var root = source.GetEnumerator();
            if(root.MoveNext())
                stack.Push(new EnumeratorAndLevel<T>(root, 0));
            while(stack.Count != 0) {
                var top = stack.Peek();
                var current = top.En.Current;
                yield return current;
                if(!top.En.MoveNext())
                    stack.Pop();

                var children = getItems(current, top.Level)?.GetEnumerator();
                if(children?.MoveNext() == true) {
                    stack.Push(new EnumeratorAndLevel<T>(children, top.Level + 1));
                }
            }
        }
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
            using(var en = source.GetEnumerator()) {
                if(en.MoveNext())
                    yield return en.Current;
                while(en.MoveNext()) {
                    yield return delimiter;
                    yield return en.Current;
                }
            }
        }
        public static string ConcatStringsWithDelimiter(this IEnumerable<string> source, string delimiter) {
            return string.Join(delimiter, source);
        }
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, ListSortDirection sortDirection) {
            return sortDirection == ListSortDirection.Ascending ?
                source.OrderBy(keySelector) :
                source.OrderByDescending(keySelector);
        }
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
            comparer = comparer ?? ((x, y) => EqualityComparer<T>.Default.Equals(x, y));
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

    public
    sealed class TaskLinq<T> {
        public TaskLinq(Task<T> task, TaskLinq.Chain chain) {
            Task = task;
            Chain = chain;
        }
        internal readonly Task<T> Task;
        internal readonly TaskLinq.Chain Chain;
    }
    public
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