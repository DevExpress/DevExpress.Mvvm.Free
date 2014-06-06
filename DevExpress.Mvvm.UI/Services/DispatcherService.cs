using System;
using System.Windows;
using System.Windows.Threading;

namespace DevExpress.Mvvm.UI {
    public class DispatcherService : ServiceBase, IDispatcherService {
        #region
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register("Delay", typeof(TimeSpan), typeof(DispatcherService), new PropertyMetadata(new TimeSpan()));
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

        public void BeginInvoke(Action action) {
            if(Delay == TimeSpan.Zero) {
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
            timer.Interval = Delay;
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