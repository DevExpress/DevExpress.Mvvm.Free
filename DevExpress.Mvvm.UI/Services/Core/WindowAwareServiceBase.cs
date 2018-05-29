using System;
using System.Windows;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI {
    public abstract class WindowAwareServiceBase : ServiceBase {
        public static readonly DependencyProperty WindowSourceProperty =
            DependencyProperty.Register("WindowSource", typeof(FrameworkElement), typeof(WindowAwareServiceBase), new PropertyMetadata(null,
                (d, e) => ((WindowAwareServiceBase)d).OnWindowSourceChanged(e)));
        public static readonly DependencyProperty WindowProperty =
            DependencyProperty.Register("Window", typeof(Window), typeof(WindowAwareServiceBase), new PropertyMetadata(null,
                (d, e) => ((WindowAwareServiceBase)d).OnWindowChanged(e)));
        static readonly DependencyPropertyKey ActualWindowPropertyKey =
            DependencyProperty.RegisterReadOnly("ActualWindow", typeof(Window), typeof(WindowAwareServiceBase), new PropertyMetadata(null,
                (d, e) => ((WindowAwareServiceBase)d).OnActualWindowChanged((Window)e.OldValue)));
        public static readonly DependencyProperty ActualWindowProperty = ActualWindowPropertyKey.DependencyProperty;

        public FrameworkElement WindowSource {
            get { return (FrameworkElement)GetValue(WindowSourceProperty); }
            set { SetValue(WindowSourceProperty, value); }
        }
        public Window Window {
            get { return (Window)GetValue(WindowProperty); }
            set { SetValue(WindowProperty, value); }
        }
        public Window ActualWindow {
            get { return (Window)GetValue(ActualWindowProperty); }
            private set { SetValue(ActualWindowPropertyKey, value); }
        }

        protected abstract void OnActualWindowChanged(Window oldWindow);
        protected void UpdateActualWindow() {
            ActualWindow = Window ?? WindowSource.With(GetWindowEx) ?? AssociatedObject.With(GetWindowEx);
        }

        void OnWindowChanged(DependencyPropertyChangedEventArgs e) {
            UpdateActualWindow();
        }
        void OnWindowSourceIsLoadedChanged(object sender, RoutedEventArgs e) {
            UpdateActualWindow();
        }
        static Window GetWindowEx(DependencyObject d) {
            return d as Window ?? Window.GetWindow(d);
        }
        void OnWindowSourceChanged(DependencyPropertyChangedEventArgs e) {
            var oldValue = (FrameworkElement)e.OldValue;
            var newValue = (FrameworkElement)e.NewValue;
            if(oldValue != null)
                Detach(oldValue);
            if(newValue != null)
                Attach(newValue);
            UpdateActualWindow();
        }
        void Attach(FrameworkElement windowSource) {
            windowSource.Loaded += OnWindowSourceIsLoadedChanged;
            windowSource.Unloaded += OnWindowSourceIsLoadedChanged;
        }
        void Detach(FrameworkElement windowSource) {
            windowSource.Loaded -= OnWindowSourceIsLoadedChanged;
            windowSource.Unloaded -= OnWindowSourceIsLoadedChanged;
        }

        protected override void OnAttached() {
            base.OnAttached();
            Attach(AssociatedObject);
            UpdateActualWindow();
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            Detach(AssociatedObject);
            UpdateActualWindow();
        }
    }
}