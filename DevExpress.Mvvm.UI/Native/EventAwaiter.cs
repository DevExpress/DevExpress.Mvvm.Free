using System;
using System.Threading.Tasks;

namespace DevExpress.Core {
    public static class EventAwaiter {
        public static Task<object> WaitEventAsync<T>(Action<EventHandler<T>> addEventHandler, Action<EventHandler<T>> removeEventHandler, Action beginAction = null) {
            return new EventTaskSource<T>(addEventHandler, removeEventHandler, beginAction).Task;
        }
        sealed class EventTaskSource<TEventArgs> {
            readonly TaskCompletionSource<object> taskSource;
            readonly Action<EventHandler<TEventArgs>> removeEventHandler;
            public Task<object> Task { get { return taskSource.Task; } }

            public EventTaskSource(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler, Action startAction = null) {
                this.taskSource = new TaskCompletionSource<object>();
                this.removeEventHandler = removeHandler;
                addHandler(OnEvent);
                if(startAction != null) {
                    startAction();
                }
            }
            void OnEvent(object sender, TEventArgs args) {
                removeEventHandler(OnEvent);
                taskSource.SetResult(args);
            }
        }
    }
}