using DevExpress.Mvvm.Native;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevExpress.Mvvm.UI {
    public static class ViewHelper {
        [Obsolete("This method is obsolete. Use other method overloads.")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static object CreateAndInitializeView(IViewLocator viewLocator, string documentType, object parameter, object parentViewModel = null, bool useParameterAsViewModel = false, DataTemplate viewTemplate = null, DataTemplateSelector viewTemplateSelector = null) {
            if(useParameterAsViewModel)
                return CreateAndInitializeView(viewLocator, documentType, parameter, parameter, parentViewModel, viewTemplate, viewTemplateSelector);
            else
                return CreateAndInitializeView(viewLocator, documentType, null, parameter, parentViewModel, viewTemplate, viewTemplateSelector);
        }
        public static object CreateAndInitializeView(IViewLocator viewLocator, string documentType, object viewModel, object parameter, object parentViewModel, DataTemplate viewTemplate = null, DataTemplateSelector viewTemplateSelector = null) {
            return CreateAndInitializeView(viewLocator, documentType, viewModel, parameter, parentViewModel, null, viewTemplate, viewTemplateSelector);
        }
        public static object CreateAndInitializeView(IViewLocator viewLocator, string documentType, object viewModel, object parameter, object parentViewModel, IDocumentOwner documentOwner, DataTemplate viewTemplate = null, DataTemplateSelector viewTemplateSelector = null) {
            object view = CreateView(viewLocator, documentType, viewTemplate, viewTemplateSelector);
            InitializeView(view, viewModel, parameter, parentViewModel, documentOwner);
            return view;
        }
        public static void InitializeView(object view, object viewModel, object parameter, object parentViewModel, IDocumentOwner documentOwner = null) {
            if(viewModel != null) {
                if(parameter != null) viewModel.With(x => x as ISupportParameter).Do(x => x.Parameter = parameter);
                if(parentViewModel != null) viewModel.With(x => x as ISupportParentViewModel).Do(x => x.ParentViewModel = parentViewModel);
                if(documentOwner != null) viewModel.With(x => x as IDocumentContent).Do(x => x.DocumentOwner = documentOwner);

                view.With(x => x as FrameworkElement).Do(x => x.DataContext = viewModel);
                view.With(x => x as ContentPresenter).Do(x => x.Content = viewModel);
                return;
            }
            if(!(view is DependencyObject)) return;
            if(ViewModelExtensions.GetParameter((DependencyObject)view) == null)
                ViewModelExtensions.SetParameter((DependencyObject)view, parameter);
            if(ViewModelExtensions.GetParentViewModel((DependencyObject)view) == null)
                ViewModelExtensions.SetParentViewModel((DependencyObject)view, parentViewModel);
            if(ViewModelExtensions.GetDocumentOwner((DependencyObject)view) == null)
                ViewModelExtensions.SetDocumentOwner((DependencyObject)view, documentOwner);
        }
        public static object CreateView(IViewLocator viewLocator, string documentType, DataTemplate viewTemplate = null, DataTemplateSelector viewTemplateSelector = null) {
            if(viewTemplate != null || viewTemplateSelector != null) {
                return new ViewPresenter(viewTemplate, viewTemplateSelector);
            }
            IViewLocator actualLocator = viewLocator ?? (ViewLocator.Default ?? ViewLocator.Instance);
            return actualLocator.ResolveView(documentType);
        }
        public static object GetViewModelFromView(object view) {
            return view.With(x => x as FrameworkElement).With(x => x.DataContext);
        }


        class ViewPresenter : ContentPresenter {
            public ViewPresenter(DataTemplate viewTemplate, DataTemplateSelector viewTemplateSelector) {
                ContentTemplate = viewTemplate;
#if !SILVERLIGHT
                ContentTemplateSelector = viewTemplateSelector;
#endif
                Loaded += ViewPresenter_Loaded;
            }
            void ViewPresenter_Loaded(object sender, RoutedEventArgs e) {
                if(VisualTreeHelper.GetChildrenCount(this) == 0) return;
                var child = VisualTreeHelper.GetChild(this, 0);
                var parameter = ViewModelExtensions.GetParameter(this);
                var parentViewModel = ViewModelExtensions.GetParentViewModel(this);
                var documentOwner = ViewModelExtensions.GetDocumentOwner(this);
                if(parameter != null) ViewModelExtensions.SetParameter((DependencyObject)child, parameter);
                if(parentViewModel != null) ViewModelExtensions.SetParentViewModel((DependencyObject)child, parentViewModel);
                if(documentOwner != null) ViewModelExtensions.SetDocumentOwner((DependencyObject)child, documentOwner);
            }
        }
    }
}