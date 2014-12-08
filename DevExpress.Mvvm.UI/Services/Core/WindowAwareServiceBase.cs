using System.Windows;

namespace DevExpress.Mvvm.UI {
    public abstract class WindowAwareServiceBase : ServiceBase {
        public static readonly DependencyProperty WindowProperty =
            DependencyProperty.Register("Window", typeof(Window), typeof(WindowAwareServiceBase), new PropertyMetadata(null,
                (d, e) => ((WindowAwareServiceBase)d).OnWindowChanged(e)));
        static readonly DependencyPropertyKey ActualWindowPropertyKey =
            DependencyProperty.RegisterReadOnly("ActualWindow", typeof(Window), typeof(WindowAwareServiceBase), new PropertyMetadata(null,
                (d, e) => ((WindowAwareServiceBase)d).OnActualWindowChanged((Window)e.OldValue)));
        public static readonly DependencyProperty ActualWindowProperty = ActualWindowPropertyKey.DependencyProperty;

        public Window Window {
            get { return (Window)GetValue(WindowProperty); }
            set { SetValue(WindowProperty, value); }
        }
        public Window ActualWindow {
            get { return (Window)GetValue(ActualWindowProperty); }
            private set { SetValue(ActualWindowPropertyKey, value); }
        }

        protected abstract void OnActualWindowChanged(Window oldWindow);

        void OnWindowChanged(DependencyPropertyChangedEventArgs e) {
            UpdateActualWindow();
        }
        void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e) {
            UpdateActualWindow();
        }
        void OnAssociatedObjectUnloaded(object sender, RoutedEventArgs e) {
            UpdateActualWindow();
        }
        void UpdateActualWindow() {
            ActualWindow = Window ?? (AssociatedObject == null ? null : Window.GetWindow(AssociatedObject));
        }
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.Loaded += OnAssociatedObjectLoaded;
            AssociatedObject.Unloaded += OnAssociatedObjectUnloaded;
            if(AssociatedObject.IsLoaded)
                OnAssociatedObjectLoaded(AssociatedObject, null);
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
            AssociatedObject.Unloaded -= OnAssociatedObjectUnloaded;
            if(AssociatedObject.IsLoaded)
                OnAssociatedObjectUnloaded(AssociatedObject, null);
        }
    }
}