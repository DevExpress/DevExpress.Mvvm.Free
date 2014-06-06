using DevExpress.Mvvm.UI.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevExpress.Mvvm.UI {
    [TargetTypeAttribute(typeof(UserControl))]
    [TargetTypeAttribute(typeof(Window))]
    public class CurrentWindowService : ServiceBase, ICurrentWindowService {
        public static readonly DependencyProperty WindowProperty =
            DependencyProperty.Register("Window", typeof(Window), typeof(CurrentWindowService), new PropertyMetadata(null));
        public Window Window {
            get { return (Window)GetValue(WindowProperty); }
            set { SetValue(WindowProperty, value); }
        }
        Window actualWindow = null;
        Window ActualWindow {
            get {
                if(Window != null)
                    return Window;
                if(AssociatedObject is Window)
                    return (Window)AssociatedObject;
                bool isWindowPropertyBound = GetBindingExp(this, WindowProperty) != null;
                if(!isWindowPropertyBound) {
                    if(actualWindow == null && AssociatedObject != null) {
                        actualWindow = LayoutHelper.FindParentObject<Window>(AssociatedObject);
                    }
                    return actualWindow;
                }
                return null;
            }
        }
        static BindingExpression GetBindingExp(DependencyObject d, DependencyProperty dp) {
#if !SILVERLIGHT
            return BindingOperations.GetBindingExpression(d, dp);
#else
            if(d is FrameworkElement)
                return ((FrameworkElement)d).GetBindingExpression(dp);
            return d.ReadLocalValue(dp) as BindingExpression;
#endif
        }

        void ICurrentWindowService.Close() {
            if(ActualWindow != null)
                ActualWindow.Close();
        }
        void ICurrentWindowService.Activate() {
            if(ActualWindow != null)
                ActualWindow.Activate();
        }
        void ICurrentWindowService.Hide() {
            if(ActualWindow != null)
                ActualWindow.Hide();
        }
        void ICurrentWindowService.SetWindowState(WindowState state) {
            if(ActualWindow != null)
                ActualWindow.WindowState = state;
        }
        void ICurrentWindowService.Show() {
            if(ActualWindow != null)
                ActualWindow.Show();
        }
    }
}