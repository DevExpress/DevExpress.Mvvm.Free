using DevExpress.Mvvm.UI.Interactivity;
using System;
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

        public void BeginInvoke(Action action) {
            if(delay == TimeSpan.Zero) {
                BeginInvokeCore(action);
                return;
            }
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority, Dispatcher);
            EventHandler onTimerTick = null;
            onTimerTick = (s, e) => {
                timer.Tick -= onTimerTick;
                timer.Stop();
                BeginInvokeCore(action);
            };
            timer.Tick += onTimerTick;
            timer.Interval = delay;
            timer.Start();
        }

        void BeginInvokeCore(Action action) {
            Dispatcher.BeginInvoke(action, DispatcherPriority, null);
        }
        void OnDelayChanged() {
            delay = Delay;
        }
    }
}