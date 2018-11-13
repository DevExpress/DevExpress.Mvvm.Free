using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm.UI {
    public static class ViewModelExtensions {
        public static readonly object NotSetParameter = new object();
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.RegisterAttached("Parameter", typeof(object), typeof(ViewModelExtensions),
            new PropertyMetadata(NotSetParameter, (d, e) => OnParameterChanged(d, e.NewValue)));
        public static readonly DependencyProperty ParentViewModelProperty =
            DependencyProperty.RegisterAttached("ParentViewModel", typeof(object), typeof(ViewModelExtensions),
            new PropertyMetadata(null, (d,e) => OnParentViewModelChanged(d, e.NewValue)));
        public static readonly DependencyProperty DocumentOwnerProperty =
            DependencyProperty.RegisterAttached("DocumentOwner", typeof(IDocumentOwner), typeof(ViewModelExtensions),
            new PropertyMetadata(null, (d, e) => OnDocumentOwnerChanged(d, e.NewValue as IDocumentOwner)));
        public static readonly DependencyProperty DocumentTitleProperty =
            DependencyProperty.RegisterAttached("DocumentTitle", typeof(object), typeof(ViewModelExtensions), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static object GetParameter(DependencyObject obj) {
            return obj.GetValue(ParameterProperty) ??
                ViewHelper.GetViewModelFromView(obj).With(x => x as ISupportParameter).Return(x => x.Parameter, null);
        }
        public static object GetParentViewModel(DependencyObject obj) {
            return obj.GetValue(ParentViewModelProperty) ??
                ViewHelper.GetViewModelFromView(obj).With(x => x as ISupportParentViewModel).Return(x => x.ParentViewModel, null);
        }
        public static IDocumentOwner GetDocumentOwner(DependencyObject obj) {
            return (IDocumentOwner)obj.GetValue(DocumentOwnerProperty) ??
                ViewHelper.GetViewModelFromView(obj).With(x => x as IDocumentContent).Return(x => x.DocumentOwner, null);
        }
        public static void SetParameter(DependencyObject obj, object value) {
            obj.SetValue(ParameterProperty, value);
        }
        public static void SetParentViewModel(DependencyObject obj, object value) {
            obj.SetValue(ParentViewModelProperty, value);
        }
        public static void SetDocumentOwner(DependencyObject obj, IDocumentOwner value) {
            obj.SetValue(DocumentOwnerProperty, value);
        }

        public static object GetDocumentTitle(DependencyObject d) { return d.GetValue(DocumentTitleProperty); }
        public static void SetDocumentTitle(DependencyObject d, object value) { d.SetValue(DocumentTitleProperty, value); }

        static void OnParameterChanged(DependencyObject d, object newValue) {
            if(NotSetParameter == newValue) return;
            ViewModelInitializer.SetViewModelParameter(d, newValue);
            ParameterAndParentViewModelSyncBehavior.AttachTo(d);
        }
        static void OnParentViewModelChanged(DependencyObject d, object newValue) {
            ViewModelInitializer.SetViewModelParentViewModel(d, newValue);
            ParameterAndParentViewModelSyncBehavior.AttachTo(d);
        }
        static void OnDocumentOwnerChanged(DependencyObject d, IDocumentOwner newValue) {
            ViewModelInitializer.SetViewModelDocumentOwner(d, newValue);
            ParameterAndParentViewModelSyncBehavior.AttachTo(d);
        }

        class ParameterAndParentViewModelSyncBehavior : Behavior<DependencyObject> {
            public static void AttachTo(DependencyObject obj) {
                if(!(obj is FrameworkElement || obj is FrameworkContentElement)) return;
                BehaviorCollection bCol = Interaction.GetBehaviors(obj);
                ParameterAndParentViewModelSyncBehavior b =
                    (ParameterAndParentViewModelSyncBehavior)bCol.FirstOrDefault(x => x is ParameterAndParentViewModelSyncBehavior);
                if(b != null) return;
                bCol.Add(new ParameterAndParentViewModelSyncBehavior());
            }
            ParameterAndParentViewModelSyncBehavior() { }
            protected override void OnAttached() {
                base.OnAttached();
                Subscribe();
            }
            protected override void OnDetaching() {
                Unsubscribe();
                base.OnDetaching();
            }
            void Subscribe() {
                Unsubscribe();
                (AssociatedObject as FrameworkElement).Do(x => x.Unloaded += OnAssociatedObjectUnloaded);
                (AssociatedObject as FrameworkElement).Do(x => x.DataContextChanged += OnAssociatedObjectDataContextChanged);
                (AssociatedObject as FrameworkContentElement).Do(x => x.Unloaded += OnAssociatedObjectUnloaded);
                (AssociatedObject as FrameworkContentElement).Do(x => x.DataContextChanged += OnAssociatedObjectDataContextChanged);
            }
            void Unsubscribe() {
                (AssociatedObject as FrameworkElement).Do(x => x.Unloaded -= OnAssociatedObjectUnloaded);
                (AssociatedObject as FrameworkElement).Do(x => x.DataContextChanged -= OnAssociatedObjectDataContextChanged);
                (AssociatedObject as FrameworkContentElement).Do(x => x.Unloaded -= OnAssociatedObjectUnloaded);
                (AssociatedObject as FrameworkContentElement).Do(x => x.DataContextChanged -= OnAssociatedObjectDataContextChanged);
            }
            void OnAssociatedObjectUnloaded(object sender, RoutedEventArgs e) {
                Unsubscribe();
            }
            void OnAssociatedObjectDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
                ViewModelInitializer.SetViewModelParameter(AssociatedObject, GetParameter(AssociatedObject));
                ViewModelInitializer.SetViewModelParentViewModel(AssociatedObject, GetParentViewModel(AssociatedObject));
                ViewModelInitializer.SetViewModelDocumentOwner(AssociatedObject, GetDocumentOwner(AssociatedObject));
            }
        }
    }
}