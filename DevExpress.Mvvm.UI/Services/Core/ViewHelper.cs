using DevExpress.Mvvm.Native;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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
            object actualViewModel = viewModel ?? GetViewModelFromView(view);
            actualViewModel.With(x => x as ISupportParameter).Do(x => x.Parameter = parameter);
            actualViewModel.With(x => x as ISupportParentViewModel).Do(x => x.ParentViewModel = parentViewModel);
            actualViewModel.With(x => x as IDocumentContent).Do(x => x.DocumentOwner = documentOwner);
            if(viewModel != null) {
                view.With(x => x as FrameworkElement).Do(x => x.DataContext = viewModel);
                view.With(x => x as ContentPresenter).Do(x => x.Content = viewModel);
            }
        }
        public static object CreateView(IViewLocator viewLocator, string documentType, DataTemplate viewTemplate = null, DataTemplateSelector viewTemplateSelector = null) {
            if(viewTemplate != null || viewTemplateSelector != null) {
                var presenter = new ContentPresenter() {
                    ContentTemplate = viewTemplate,
#if !SILVERLIGHT
                    ContentTemplateSelector = viewTemplateSelector
#endif
                };
                return presenter;
            }
            IViewLocator actualLocator = viewLocator ?? (ViewLocator.Default ?? ViewLocator.Instance);
            return actualLocator.ResolveView(documentType);
        }
        public static object GetViewModelFromView(object view) {
            return view.With(x => x as FrameworkElement).With(x => x.DataContext);
        }
    }
}