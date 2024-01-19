using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System;
using DevExpress.Mvvm.Native;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Navigation;
using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI {
    [RuntimeNameProperty("Name")]
    public abstract class ServiceBaseGeneric<T> : Behavior<T> where T : DependencyObject {
        [IgnoreDependencyPropertiesConsistencyCheckerAttribute]
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string),
            typeof(ServiceBaseGeneric<T>), new PropertyMetadata(null));
        public string Name { get { return (string)GetValue(NameProperty); } set { SetValue(NameProperty, value); } }
        [IgnoreDependencyPropertiesConsistencyCheckerAttribute]
        static readonly DependencyProperty ServicesClientInternalProperty = DependencyProperty.Register("ServicesClientInternal", typeof(object), typeof(ServiceBaseGeneric<T>),
            new PropertyMetadata(null, (d, e) => ((ServiceBaseGeneric<T>)d).OnServicesClientChanged(e.OldValue as ISupportServices, e.NewValue as ISupportServices)));
        public bool YieldToParent { get; set; }
        internal bool ShouldInject { get; set; }

        protected ServiceBaseGeneric() {
            ShouldInject = true;
        }
        protected override void OnAttached() {
            base.OnAttached();
            ServiceInjectionHelper.SetInjectBinding(this);
        }
        protected override void OnDetaching() {
            ServiceInjectionHelper.ClearInjectBinding(this);
            base.OnDetaching();
        }
        protected ISupportServices GetServicesClient() {
            return GetValue(ServicesClientInternalProperty) as ISupportServices;
        }
        protected Uri GetBaseUri() {
            Uri baseUri = BaseUriHelper.GetBaseUri(this);
            if(baseUri != null || AssociatedObject == null) return baseUri;
            return BaseUriHelper.GetBaseUri(AssociatedObject);
        }
        protected virtual void OnServicesClientChanged(ISupportServices oldServiceClient, ISupportServices newServiceClient) {
            oldServiceClient.Do(x => x.ServiceContainer.UnregisterService(this));
            newServiceClient.Do(x => x.ServiceContainer.RegisterService(Name, this, YieldToParent));
        }

        internal static class ServiceInjectionHelper {
            public static void SetInjectBinding(ServiceBaseGeneric<T> service) {
                if (service.ShouldInject)
                    BindingOperations.SetBinding(service, ServicesClientInternalProperty, new Binding() { Path = new PropertyPath("DataContext"), Source = service.AssociatedObject });
            }
            public static void ClearInjectBinding(ServiceBaseGeneric<T> service) {
                BindingOperations.ClearBinding(service, ServicesClientInternalProperty);
            }
            public static bool IsInjectBindingSet(ServiceBaseGeneric<T> service) {
                return BindingOperations.IsDataBound(service, ServicesClientInternalProperty);
            }
        }
    }
    public abstract class ServiceBase : ServiceBaseGeneric<FrameworkElement> {
        public static readonly DependencyProperty UnregisterOnUnloadedProperty =
            DependencyProperty.Register("UnregisterOnUnloaded", typeof(bool), typeof(ServiceBase), new PropertyMetadata(false, (d, e) => ((ServiceBase)d).OnUnregisterOnUnloadedChanged()));
        public bool UnregisterOnUnloaded { get { return (bool)GetValue(UnregisterOnUnloadedProperty); } set { SetValue(UnregisterOnUnloadedProperty, value); } }

        protected override void OnAttached() {
            base.OnAttached();
            if (UnregisterOnUnloaded)
                Subscribe();
        }
        protected override void OnDetaching() {
            Unsubscribe();
            base.OnDetaching();
        }
        void OnUnregisterOnUnloadedChanged() {
            if (!IsAttached) return;
            if (UnregisterOnUnloaded)
                Subscribe();
            else Unsubscribe();
        }
        void Subscribe() {
            if (AssociatedObject == null) return;
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Unloaded += OnUnloaded;
        }
        void Unsubscribe() {
            if (AssociatedObject == null) return;
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.Unloaded -= OnUnloaded;
        }

        bool isLoaded = false;
        void OnLoaded(object sender, RoutedEventArgs e) {
            if (isLoaded) return;
            isLoaded = true;
            if(!ServiceInjectionHelper.IsInjectBindingSet(this))
                ServiceInjectionHelper.SetInjectBinding(this);
        }
        void OnUnloaded(object sender, RoutedEventArgs e) {
            if (!isLoaded) return;
            isLoaded = false;
            ServiceInjectionHelper.ClearInjectBinding(this);
        }
    }
}