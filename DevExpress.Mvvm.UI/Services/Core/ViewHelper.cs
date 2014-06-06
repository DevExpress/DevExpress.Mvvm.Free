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
            object view = CreateView(viewLocator, documentType, viewTemplate, viewTemplateSelector);
            InitializeView(view, viewModel, parameter, parentViewModel);
            return view;
        }
        public static void InitializeView(object view, object viewModel, object parameter, object parentViewModel) {
            if(viewModel != null) {
                viewModel.With(x => x as ISupportParameter).Do(x => x.Parameter = parameter);
                viewModel.With(x => x as ISupportParentViewModel).Do(x => x.ParentViewModel = parentViewModel);
                view.With(x => x as FrameworkElement).Do(x => x.DataContext = viewModel);
                view.With(x => x as ContentPresenter).Do(x => x.Content = viewModel);
                return;
            }
            if(viewModel == null) {
                GetViewModelFromView(view).With(x => x as ISupportParameter).Do(x => x.Parameter = parameter);
                GetViewModelFromView(view).With(x => x as ISupportParentViewModel).Do(x => x.ParentViewModel = parentViewModel);
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