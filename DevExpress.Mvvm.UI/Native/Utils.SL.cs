using System.Windows;

namespace System.Windows.Controls {
    public class DataTemplateSelector {
        public virtual DataTemplate SelectTemplate(object item, DependencyObject container) {
            return null;
        }
    }
}

namespace DevExpress.Xpf.Core {
    public static class AppHelper {
        public static double HostHeight {
            get { return Application.Current.Host.Content.ActualHeight / Application.Current.Host.Content.ZoomFactor; }
        }
        public static double HostWidth {
            get { return Application.Current.Host.Content.ActualWidth / Application.Current.Host.Content.ZoomFactor; }
        }

        public static FrameworkElement RootVisual { get { return (FrameworkElement)Application.Current.RootVisual; } }
    }
}