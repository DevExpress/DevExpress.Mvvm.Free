using DevExpress.Mvvm.Native;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;

namespace DevExpress.Mvvm.UI {
    public static class ViewHelper {
        internal const string Error_CreateViewMissArguments = "It is impossible to create a view based on passed parameters. ViewTemplate/ViewTemplateSelector or DocumentType should be set.";
        internal const string HelpLink_CreateViewMissArguments = "https://documentation.devexpress.com/#WPF/CustomDocument17469";
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
            if(documentOwner is DependencyObject)
                documentOwner = DocumentOwnerWrapper.Create(documentOwner);
            if(viewModel != null) {
                ViewModelInitializer.SetViewModelProperties(viewModel, parameter, parentViewModel, documentOwner);
                view.With(x => x as FrameworkElement).Do(x => x.DataContext = viewModel);
                view.With(x => x as FrameworkContentElement).Do(x => x.DataContext = viewModel);
                view.With(x => x as ContentPresenter).Do(x => x.Content = viewModel);
                return;
            }
            if(view is DependencyObject)
                ViewModelInitializer.SetViewModelProperties((DependencyObject)view, parameter, parentViewModel, documentOwner);
        }
        public static object CreateView(IViewLocator viewLocator, string documentType, DataTemplate viewTemplate = null, DataTemplateSelector viewTemplateSelector = null) {
            if(documentType == null && viewTemplate == null && viewTemplateSelector == null) {
                var ex = new InvalidOperationException(string.Format("{0}{1}To learn more, see: {2}", Error_CreateViewMissArguments, System.Environment.NewLine, HelpLink_CreateViewMissArguments));
                ex.HelpLink = HelpLink_CreateViewMissArguments;
                throw ex;
            }
            if(viewTemplate != null || viewTemplateSelector != null) {
                return new ViewPresenter(viewTemplate, viewTemplateSelector);
            }
            IViewLocator actualLocator = viewLocator ?? (ViewLocator.Default ?? ViewLocator.Instance);
            return actualLocator.ResolveView(documentType);
        }
        public static object GetViewModelFromView(object view) {
            return (view as FrameworkElement).Return(x => x.DataContext, null)
                ?? (view as FrameworkContentElement).Return(x => x.DataContext, null);
        }
        public static void SetBindingToViewModel(DependencyObject target, DependencyProperty targetProperty, PropertyPath viewPropertyPath) {
            BindingOperations.SetBinding(target, ViewProperty, new Binding() { Path = viewPropertyPath, Source = target, Mode = BindingMode.OneWay, Converter = new AsFrameworkElementConverter() });
            BindingOperations.SetBinding(target, targetProperty, new Binding() { Path = new PropertyPath("(0).(1)", ViewProperty, FrameworkElement.DataContextProperty), Source = target, Mode = BindingMode.OneWay });
        }
        static readonly DependencyProperty ViewProperty = DependencyProperty.RegisterAttached("View", typeof(FrameworkElement), typeof(ViewHelper), new PropertyMetadata(null));

        class AsFrameworkElementConverter : IValueConverter {
            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                return value as FrameworkElement;
            }
            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                throw new NotSupportedException();
            }
        }

        class ViewPresenter : ContentPresenter {
            public ViewPresenter(DataTemplate viewTemplate, DataTemplateSelector viewTemplateSelector) {
                ContentTemplate = viewTemplate;
                ContentTemplateSelector = viewTemplateSelector;
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
        public sealed class DocumentOwnerWrapper : IDocumentOwner {
            public static DocumentOwnerWrapper Create(IDocumentOwner owner) {
                if(owner == null)
                    return null;
                return owner as DocumentOwnerWrapper ?? new DocumentOwnerWrapper(owner);
            }
            public IDocumentOwner ActualOwner { get { return actualOwner; } }
            readonly IDocumentOwner actualOwner;

            DocumentOwnerWrapper(IDocumentOwner actualOwner) {
                this.actualOwner = actualOwner;
            }

            public void Close(IDocumentContent documentContent, bool force = true) {
                ActualOwner.Close(documentContent, force);
            }
            public override bool Equals(object obj) {
                if(obj is DocumentOwnerWrapper)
                    return ActualOwner.Equals((obj as DocumentOwnerWrapper).ActualOwner);
                if(obj is IDocumentOwner)
                    return ActualOwner.Equals(obj);
                return false;
            }
            public override int GetHashCode() {
                return actualOwner.GetHashCode();
            }
        }
    }
    static class ViewModelInitializer {
        public static void SetViewModelProperties(DependencyObject view, object parameter, object parentViewModel, IDocumentOwner documentOwner) {
            if(view == null) return;
            if(ViewModelExtensions.NotSetParameter == ViewModelExtensions.GetParameter(view) || parameter != null)
                ViewModelExtensions.SetParameter(view, parameter);
            if(ViewModelExtensions.GetParentViewModel(view) == null || parentViewModel != null)
                ViewModelExtensions.SetParentViewModel(view, parentViewModel);
            if(ViewModelExtensions.GetDocumentOwner(view) == null || documentOwner != null)
                ViewModelExtensions.SetDocumentOwner(view, documentOwner);
        }
        public static void SetViewModelProperties(object viewModel, object parameter, object parentViewModel, IDocumentOwner documentOwner) {
            if(viewModel == null) return;
            if(parameter != null) SetViewModelParameter(viewModel, parameter);
            if(parentViewModel != null) SetViewModelParentViewModel(viewModel, parentViewModel);
            if(documentOwner != null) SetViewModelDocumentOwner(viewModel, documentOwner);
        }
        public static void SetViewModelParameter(DependencyObject view, object parameter) {
            SetViewModelParameter(ViewHelper.GetViewModelFromView(view), parameter);
        }
        public static void SetViewModelParentViewModel(DependencyObject view, object parentViewModel) {
            SetViewModelParentViewModel(ViewHelper.GetViewModelFromView(view), parentViewModel);
        }
        public static void SetViewModelDocumentOwner(DependencyObject view, IDocumentOwner documentOwner) {
            SetViewModelDocumentOwner(ViewHelper.GetViewModelFromView(view), documentOwner);
        }

        static void SetViewModelParameter(object viewModel, object parameter) {
            if(viewModel == null) return;
            viewModel.With(x => x as ISupportParameter).Do(x => x.Parameter = parameter);
        }
        static void SetViewModelParentViewModel(object viewModel, object parentViewModel) {
            if(viewModel == null) return;
            if(IsCycleDetected(viewModel, parentViewModel)) return;
            viewModel.With(x => x as ISupportParentViewModel).Do(x => x.ParentViewModel = parentViewModel);
        }
        static void SetViewModelDocumentOwner(object viewModel, IDocumentOwner documentOwner) {
            if(viewModel == null) return;
            viewModel.With(x => x as IDocumentContent).Do(x => x.DocumentOwner = documentOwner);
        }
        static bool IsCycleDetected(object viewModel, object parentViewModel) {
            object p = parentViewModel;
            while(p != null) {
                if(p == viewModel) return true;
                p = (p as ISupportParentViewModel).With(x => x.ParentViewModel);
                if(p == parentViewModel) return false;
            }
            return false;
        }
    }
}