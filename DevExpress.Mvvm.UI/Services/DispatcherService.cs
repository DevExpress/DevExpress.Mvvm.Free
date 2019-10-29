using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DevExpress.Mvvm.UI {
    [TargetType(typeof(UserControl)), TargetType(typeof(Window))]
    public class DispatcherService : ServiceBase, IDispatcherService {
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register("Delay", typeof(TimeSpan), typeof(DispatcherService),
            new PropertyMetadata(TimeSpan.Zero, (d, e) => ((DispatcherService)d).OnDelayChanged()));

        public TimeSpan Delay {
            get { return (TimeSpan)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }
        TimeSpan delay;
        public DispatcherPriority DispatcherPriority { get; set; }

        public DispatcherService() {
            DispatcherPriority = DispatcherPriority.Normal;
        }

        public void Invoke(Action action) {
            InvokeCore(action);
        }
        public Task BeginInvoke(Action action) {
            return InvokeAsyncCore(action);
        }

        void InvokeCore(Action action) {
            Dispatcher.Invoke(action);
        }
        Task InvokeAsyncCore(Action action) {
            if(delay == TimeSpan.Zero)
                return Dispatcher.BeginInvoke(action, DispatcherPriority).Task;

            var source = new TaskCompletionSource<object>();
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority, Dispatcher);
            EventHandler onTimerTick = null;
            onTimerTick = (s, e) => {
                timer.Tick -= onTimerTick;
                timer.Stop();
                action.Invoke();
                source.SetResult(null);
            };

            timer.Tick += onTimerTick;
            timer.Interval = delay;
            timer.Start();
            return source.Task;
        }
        void OnDelayChanged() {
            delay = Delay;
        }
    }
}