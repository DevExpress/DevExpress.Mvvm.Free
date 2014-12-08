#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Media;
#else
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif
using System;
using System.Windows;
using System.ComponentModel;

namespace DevExpress.Mvvm.UI {
    public interface IViewLocator {
        object ResolveView(string name);
        Type ResolveViewType(string name);
    }

    public static class ViewLocatorExtensions {
#if !NETFX_CORE && !SILVERLIGHT
        public static DataTemplate CreateViewTemplate(this IViewLocator viewLocator, Type viewType) {
            Verify(viewLocator);
            if(viewType == null) throw new ArgumentNullException("viewType");
            DataTemplate res = null;
            try {
                res = new DataTemplate() { VisualTree = new FrameworkElementFactory(viewType) };
                res.Seal();
            } catch {
                res = CreateFallbackViewTemplate(GetErrorMessage_CannotCreateDataTemplateFromViewType(viewType.Name));
            }
            return res;
        }
        public static DataTemplate CreateViewTemplate(this IViewLocator viewLocator, string viewName) {
            Verify(viewLocator);
            Type viewType = viewLocator.ResolveViewType(viewName);
            if(viewType == null) return CreateFallbackViewTemplate(GetErrorMessage_CannotResolveViewType(viewName));
            return CreateViewTemplate(viewLocator, viewType);
        }
        internal static DataTemplate CreateFallbackViewTemplate(string errorText) {
            var factory = new FrameworkElementFactory(typeof(FallbackView));
            factory.SetValue(FallbackView.TextProperty, errorText);
            var res = new DataTemplate() { VisualTree = factory };
            res.Seal();
            return res;
        }
#endif
        internal static object CreateFallbackView(string errorText) {
            return new FallbackView() { Text = errorText };
        }
        internal static string GetErrorMessage_CannotResolveViewType(string name) {
            if(string.IsNullOrEmpty(name))
                return "ViewType is not specified.";
            if(ViewModelBase.IsInDesignMode)
                return string.Format("[{0}]", name);
            return string.Format("\"{0}\" type not found.", name);
        }
        internal static string GetErrorMessage_CannotCreateDataTemplateFromViewType(string name) {
            if(ViewModelBase.IsInDesignMode)
                return string.Format("[{0}]", name);
            return string.Format("Cannot create DataTemplate from the \"{0}\" view type.", name);
        }

        static void Verify(IViewLocator viewLocator) {
            if(viewLocator == null)
                throw new ArgumentNullException("viewLocator");
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class FallbackView : Panel {
            public static readonly DependencyProperty TextProperty =
                DependencyProperty.Register("Text", typeof(string), typeof(FallbackView),
                new PropertyMetadata(null, (d,e) => ((FallbackView)d).OnTextChanged()));
            public string Text {
                get { return (string)GetValue(TextProperty); }
                set { SetValue(TextProperty, value); }
            }
            TextBlock tb;
            public FallbackView() {
                tb = new TextBlock();
                if(ViewModelBase.IsInDesignMode) {
                    tb.FontSize = 25;
                    tb.Foreground = new SolidColorBrush(Colors.Red);
                } else {
                    tb.FontSize = 18;
                    tb.Foreground = new SolidColorBrush(Colors.Gray);
                }
                HorizontalAlignment = HorizontalAlignment.Center;
                VerticalAlignment = VerticalAlignment.Center;
                Children.Add(tb);
            }
            void OnTextChanged() {
                tb.Text = Text;
            }
            protected override Size MeasureOverride(Size availableSize) {
                tb.Measure(availableSize);
                return tb.DesiredSize;
            }
            protected override Size ArrangeOverride(Size finalSize) {
                tb.Arrange(new Rect(new Point(), finalSize));
                return finalSize;
            }
        }
    }
}