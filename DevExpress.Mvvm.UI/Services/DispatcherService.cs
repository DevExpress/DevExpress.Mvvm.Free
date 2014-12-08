using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DevExpress.Mvvm.UI {
    [TargetType(typeof(UserControl)), TargetType(typeof(Window))]
    public class DispatcherService : ServiceBase, IDispatcherService {
        #region Dependency Properties
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register("Delay", typeof(TimeSpan), typeof(DispatcherService),
            new PropertyMetadata(TimeSpan.Zero, (d,e) => ((DispatcherService)d).OnDelayChanged()));
        #endregion

#if !SILVERLIGHT
        DispatcherPriority dispatcherPriority = DispatcherPriority.Normal;
        public DispatcherPriority DispatcherPriority {
            get { return dispatcherPriority; }
            set { dispatcherPriority = value; }
        }
#endif
        public TimeSpan Delay {
            get { return (TimeSpan)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }
        void OnDelayChanged() {
            delay = Delay;
        }
        TimeSpan delay;

        public void BeginInvoke(Action action) {
            if(delay == TimeSpan.Zero) {
                BeginInvokeCore(action);
                return;
            }
#if SILVERLIGHT
            DispatcherTimer timer = new DispatcherTimer();
#else
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority, Dispatcher);
#endif
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
#if !SILVERLIGHT
            Dispatcher.BeginInvoke(action, DispatcherPriority, null);
#else
            Dispatcher.BeginInvoke(action);
#endif
        }
    }
}