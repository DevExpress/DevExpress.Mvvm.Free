using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm.UI {
    public static class ViewModelExtensions {
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.RegisterAttached("Parameter", typeof(object), typeof(ViewModelExtensions),
            new PropertyMetadata(null, (d,e) => OnParameterChanged(d, e.NewValue)));
        public static readonly DependencyProperty ParentViewModelProperty =
            DependencyProperty.RegisterAttached("ParentViewModel", typeof(object), typeof(ViewModelExtensions),
            new PropertyMetadata(null, (d,e) => OnParentViewModelChanged(d, e.NewValue)));
        public static readonly DependencyProperty DocumentOwnerProperty =
            DependencyProperty.RegisterAttached("DocumentOwner", typeof(IDocumentOwner), typeof(ViewModelExtensions),
            new PropertyMetadata(null, (d, e) => OnDocumentOwnerChanged(d, e.NewValue as IDocumentOwner)));

        public static object GetParameter(DependencyObject obj) {
            return obj.GetValue(ParameterProperty) ?? GetParameterCore(obj);
        }
        public static object GetParentViewModel(DependencyObject obj) {
            return obj.GetValue(ParentViewModelProperty) ?? GetParentViewModelCore(obj);
        }
        public static IDocumentOwner GetDocumentOwner(DependencyObject obj) {
            return (IDocumentOwner)obj.GetValue(DocumentOwnerProperty) ?? GetDocumentOwnerCore(obj);
        }

        public static void SetParameter(DependencyObject obj, object value) {
            OnParameterChanged(obj, value);
            obj.SetValue(ParameterProperty, value);
        }
        public static void SetParentViewModel(DependencyObject obj, object value) {
            OnParentViewModelChanged(obj, value);
            obj.SetValue(ParentViewModelProperty, value);
        }
        public static void SetDocumentOwner(DependencyObject obj, IDocumentOwner value) {
            OnDocumentOwnerChanged(obj, value);
            obj.SetValue(DocumentOwnerProperty, value);
        }

        static void OnParameterChanged(DependencyObject d, object newValue) {
            SetParameterCore(d, newValue);
            ParameterAndParentViewModelSyncBehavior.AttachTo(d);
        }
        static void OnParentViewModelChanged(DependencyObject d, object newValue) {
            SetParentViewModelCore(d, newValue);
            ParameterAndParentViewModelSyncBehavior.AttachTo(d);
        }
        static void OnDocumentOwnerChanged(DependencyObject d, IDocumentOwner newValue) {
            SetDocumentOwnerCore(d, newValue);
            ParameterAndParentViewModelSyncBehavior.AttachTo(d);
        }

        static void SetParameterCore(DependencyObject d, object parameter) {
            ViewHelper.GetViewModelFromView(d).With(x => x as ISupportParameter).Do(x => x.Parameter = parameter);
        }
        static void SetParentViewModelCore(DependencyObject d, object parentViewModel) {
            ViewHelper.GetViewModelFromView(d).With(x => x as ISupportParentViewModel).Do(x => x.ParentViewModel = parentViewModel);
        }
        static void SetDocumentOwnerCore(DependencyObject d, IDocumentOwner documentOwner) {
            ViewHelper.GetViewModelFromView(d).With(x => x as IDocumentContent).Do(x => x.DocumentOwner = documentOwner);
        }

        static object GetParameterCore(DependencyObject d) {
            return ViewHelper.GetViewModelFromView(d).With(x => x as ISupportParameter).Return(x => x.Parameter, null);
        }
        static object GetParentViewModelCore(DependencyObject d) {
            return ViewHelper.GetViewModelFromView(d).With(x => x as ISupportParentViewModel).Return(x => x.ParentViewModel, null);
        }
        static IDocumentOwner GetDocumentOwnerCore(DependencyObject d) {
            return ViewHelper.GetViewModelFromView(d).With(x => x as IDocumentContent).Return(x => x.DocumentOwner, null);
        }

        class ParameterAndParentViewModelSyncBehavior : Behavior<FrameworkElement> {
            public static void AttachTo(DependencyObject obj) {
                FrameworkElement el = obj as FrameworkElement;
                if(el == null) return;
                BehaviorCollection bCol = Interaction.GetBehaviors(el);
                ParameterAndParentViewModelSyncBehavior b = (ParameterAndParentViewModelSyncBehavior)bCol.FirstOrDefault(x => x is ParameterAndParentViewModelSyncBehavior);
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
                AssociatedObject.Unloaded += OnAssociatedObjectUnloaded;
                AssociatedObject.DataContextChanged += OnAssociatedObjectDataContextChanged;
            }
            void Unsubscribe() {
                AssociatedObject.Unloaded -= OnAssociatedObjectUnloaded;
                AssociatedObject.DataContextChanged -= OnAssociatedObjectDataContextChanged;
            }
            void OnAssociatedObjectUnloaded(object sender, RoutedEventArgs e) {
                Unsubscribe();
            }
            void OnAssociatedObjectDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
                SetParameterCore(AssociatedObject, GetParameter(AssociatedObject));
                SetParentViewModelCore(AssociatedObject, GetParentViewModel(AssociatedObject));
                SetDocumentOwnerCore(AssociatedObject, GetDocumentOwner(AssociatedObject));
            }
        }
    }
}