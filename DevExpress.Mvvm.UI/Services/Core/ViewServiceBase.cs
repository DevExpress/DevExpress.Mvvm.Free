using DevExpress.Mvvm.Native;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevExpress.Mvvm.UI {
    public abstract class ViewServiceBase : ServiceBase {
        public IViewLocator ViewLocator {
            get { return (IViewLocator)GetValue(ViewLocatorProperty); }
            set { SetValue(ViewLocatorProperty, value); }
        }
        public static readonly DependencyProperty ViewLocatorProperty =
            DependencyProperty.Register("ViewLocator", typeof(IViewLocator), typeof(ViewServiceBase),
            new PropertyMetadata(null, (d, e) => ((ViewServiceBase)d).OnViewLocatorChanged((IViewLocator)e.OldValue, (IViewLocator)e.NewValue)));
        protected virtual void OnViewLocatorChanged(IViewLocator oldValue, IViewLocator newValue) { }

        public DataTemplate ViewTemplate {
            get { return (DataTemplate)GetValue(ViewTemplateProperty); }
            set { SetValue(ViewTemplateProperty, value); }
        }
        public static readonly DependencyProperty ViewTemplateProperty =
            DependencyProperty.Register("ViewTemplate", typeof(DataTemplate), typeof(ViewServiceBase),
            new PropertyMetadata(null, (d, e) => ((ViewServiceBase)d).OnViewTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue)));
        protected virtual void OnViewTemplateChanged(DataTemplate oldValue, DataTemplate newValue) { }
        public DataTemplateSelector ViewTemplateSelector {
            get { return (DataTemplateSelector)GetValue(ViewTemplateSelectorProperty); }
            set { SetValue(ViewTemplateSelectorProperty, value); }
        }
        public static readonly DependencyProperty ViewTemplateSelectorProperty =
            DependencyProperty.Register("ViewTemplateSelector", typeof(DataTemplateSelector), typeof(ViewServiceBase),
            new PropertyMetadata(null, (d, e) => ((ViewServiceBase)d).OnViewTemplateSelectorChanged((DataTemplateSelector)e.OldValue, (DataTemplateSelector)e.NewValue)));
        protected virtual void OnViewTemplateSelectorChanged(DataTemplateSelector oldValue, DataTemplateSelector newValue) { }
        protected object CreateAndInitializeView(string documentType, object viewModel, object parameter, object parentViewModel, IDocumentOwner documentOwner = null) {
            return ViewHelper.CreateAndInitializeView(ViewLocator, documentType, viewModel, parameter, parentViewModel, documentOwner, ViewTemplate, ViewTemplateSelector);
        }
        protected Style GetDocumentContainerStyle(DependencyObject documentContainer, object view, Style style, StyleSelector styleSelector) {
            return style ?? styleSelector.With(s => s.SelectStyle(ViewHelper.GetViewModelFromView(view), documentContainer));
        }
        public static void UpdateThemeName(FrameworkElement target, FrameworkElement associatedObject) {
        }
        [System.Security.SecuritySafeCritical]
        internal static void UpdateWindowOwner(Window w, DependencyObject ownerObject) {
            if (ownerObject == null)
                return;
            if (!ViewModelBase.IsInDesignMode) {
                w.Owner = LayoutTreeHelper.GetVisualParents(ownerObject).OfType<Window>().FirstOrDefault()
                    ?? Window.GetWindow(ownerObject);
            } else {
                System.Windows.Interop.WindowInteropHelper windowInteropHelper = new System.Windows.Interop.WindowInteropHelper(w);
                windowInteropHelper.Owner = GetActiveWindow();
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        static extern System.IntPtr GetActiveWindow();

        protected void UpdateThemeName(FrameworkElement target) {
        }
        protected void InitializeDocumentContainer(FrameworkElement documentContainer, DependencyProperty documentContainerViewProperty, Style documentContainerStyle) {
            ViewHelper.SetBindingToViewModel(documentContainer, FrameworkElement.DataContextProperty, new PropertyPath(documentContainerViewProperty));
            if(documentContainerStyle != null)
                documentContainer.Style = documentContainerStyle;
        }
    }
}