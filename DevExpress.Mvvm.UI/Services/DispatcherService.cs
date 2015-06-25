using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Windows;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Threading;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
#endif

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
#if NETFX_CORE
        public CoreDispatcherPriority DispatcherPriority { get; set; }
#elif !SILVERLIGHT
        public DispatcherPriority DispatcherPriority { get; set; }

        public DispatcherService() {
            DispatcherPriority = DispatcherPriority.Normal;
        }
#endif

        public void BeginInvoke(Action action) {
            if(delay == TimeSpan.Zero) {
                BeginInvokeCore(action);
                return;
            }
#if SILVERLIGHT || NETFX_CORE
            DispatcherTimer timer = new DispatcherTimer();
#else
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority, Dispatcher);
#endif
#if NETFX_CORE
            EventHandler<object> onTimerTick = null;
#else
            EventHandler onTimerTick = null;
#endif
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
#if SILVERLIGHT
            Dispatcher.BeginInvoke(action);
#elif !NETFX_CORE
            Dispatcher.BeginInvoke(action, DispatcherPriority, null);
#else
            Dispatcher.RunAsync(DispatcherPriority, new DispatchedHandler(action)).AsTask();
#endif
        }
        void OnDelayChanged() {
            delay = Delay;
        }
    }
}