using DevExpress.Mvvm.UI.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace DevExpress.Mvvm.UI {
    [TargetType(typeof(Control))]
    public class FocusBehavior : EventTriggerBase<Control> {
#if !SILVERLIGHT
        public readonly static TimeSpan DefaultFocusDelay = TimeSpan.FromMilliseconds(0);
#else
        public readonly static TimeSpan DefaultFocusDelay = TimeSpan.FromMilliseconds(500);
#endif
        public static readonly DependencyProperty FocusDelayProperty =
            DependencyProperty.Register("FocusDelay", typeof(TimeSpan?), typeof(FocusBehavior),
            new PropertyMetadata(null));
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(FocusBehavior),
            new PropertyMetadata(string.Empty));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty PropertyValueProperty =
            DependencyProperty.Register("PropertyValue", typeof(object), typeof(FocusBehavior),
            new PropertyMetadata(null, (d, e) => ((FocusBehavior)d).OnPropertyValueChanged()));
        public TimeSpan? FocusDelay {
            get { return (TimeSpan?)GetValue(FocusDelayProperty); }
            set { SetValue(FocusDelayProperty, value); }
        }
        public string PropertyName {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        bool lockPropertyValueChanged;
        protected override void OnEvent(object sender, object eventArgs) {
            base.OnEvent(sender, eventArgs);
            if(!string.IsNullOrEmpty(PropertyName)) return;
            DoFocus();
        }
        protected override void OnSourceChanged(object oldSource, object newSource) {
            base.OnSourceChanged(oldSource, newSource);
            lockPropertyValueChanged = true;
            ClearValue(PropertyValueProperty);
            lockPropertyValueChanged = false;
            if(!string.IsNullOrEmpty(PropertyName) && newSource != null) {
                lockPropertyValueChanged = true;
                BindingOperations.SetBinding(this, PropertyValueProperty,
                    new Binding() { Path = new PropertyPath(PropertyName), Source = newSource, Mode = BindingMode.OneWay });
                lockPropertyValueChanged = false;
            }
        }
        void OnPropertyValueChanged() {
            if(lockPropertyValueChanged) return;
            DoFocus();
        }
        internal TimeSpan GetFocusDelay() {
            if(EventName == "Loaded")
                return FocusDelay ?? DefaultFocusDelay;
            else
                return FocusDelay ?? TimeSpan.FromMilliseconds(0);
        }
        void AssociatedObjectFocus() {
#if !NETFX_CORE
            AssociatedObject.Focus();
#else
            AssociatedObject.Focus(FocusState.Programmatic);
#endif
        }
        void DoFocus() {
            if(!IsAttached) return;
            var focusDelay = GetFocusDelay();
            if(focusDelay == TimeSpan.FromMilliseconds(0)) {
                AssociatedObjectFocus();
                return;
            }
            DispatcherTimer timer = new DispatcherTimer() {
                Interval = focusDelay,
            };
            timer.Tick += OnTimerTick;
            timer.Start();
        }
 #if NETFX_CORE
        void OnTimerTick(object sender, object e) {
#else
        void OnTimerTick(object sender, EventArgs e) {
#endif
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Tick -= OnTimerTick;
            timer.Stop();
            AssociatedObjectFocus();
        }
    }
}