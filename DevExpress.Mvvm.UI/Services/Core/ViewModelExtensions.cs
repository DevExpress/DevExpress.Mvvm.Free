using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm.UI {
    public static class ViewModelExtensions {
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.RegisterAttached("Parameter", typeof(object), typeof(ViewModelExtensions),
            new PropertyMetadata(null, OnParameterChanged));
        public static readonly DependencyProperty ParentViewModelProperty =
            DependencyProperty.RegisterAttached("ParentViewModel", typeof(object), typeof(ViewModelExtensions),
            new PropertyMetadata(null, OnParentViewModelChnaged));

        public static object GetParameter(DependencyObject obj) {
            return (object)obj.GetValue(ParameterProperty);
        }
        public static void SetParameter(DependencyObject obj, object value) {
            obj.SetValue(ParameterProperty, value);
        }
        public static object GetParentViewModel(DependencyObject obj) {
            return (object)obj.GetValue(ParentViewModelProperty);
        }
        public static void SetParentViewModel(DependencyObject obj, object value) {
            obj.SetValue(ParentViewModelProperty, value);
        }

        static void OnParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            SetParameterCore(d, e.NewValue);
            ParameterAndParentViewModelSyncBehavior.AttachTo(d);
        }
        static void OnParentViewModelChnaged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            SetParentViewModelCore(d, e.NewValue);
            ParameterAndParentViewModelSyncBehavior.AttachTo(d);
        }
        static void SetParameterCore(DependencyObject d, object parameter) {
            ViewHelper.GetViewModelFromView(d).With(x => x as ISupportParameter).Do(x => x.Parameter = parameter);
        }
        static void SetParentViewModelCore(DependencyObject d, object parentViewModel) {
            ViewHelper.GetViewModelFromView(d).With(x => x as ISupportParentViewModel).Do(x => x.ParentViewModel = parentViewModel);
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
            protected override void OnAttached() {
                base.OnAttached();
                AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
            }
            protected override void OnDetaching() {
                AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
                base.OnDetaching();
            }
            void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
                SetParameterCore(AssociatedObject, ViewModelExtensions.GetParameter(AssociatedObject));
                SetParentViewModelCore(AssociatedObject, ViewModelExtensions.GetParentViewModel(AssociatedObject));
            }
        }
    }
}