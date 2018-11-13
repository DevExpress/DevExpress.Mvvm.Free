#if !MVVM
using DevExpress.Xpf.Core.Native;
#endif
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System;
using DevExpress.Mvvm.Native;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#if FREE
using DevExpress.Mvvm.UI.Native;
#endif
#else
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Navigation;
using DevExpress.Mvvm.UI.Native;
#endif

namespace DevExpress.Mvvm.UI {
#if NETFX_CORE
    public abstract class ServiceBaseGeneric<T> : Behavior<T> where T : DependencyObject {
        public string Name { get { return (string)GetValue(FrameworkElement.NameProperty); } set { SetValue(FrameworkElement.NameProperty, value); } }
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
            SetInjectBinding();
        }
        protected override void OnDetaching() {
            ClearValue(ServicesClientInternalProperty);
            base.OnDetaching();
        }
        void SetInjectBinding() {
            //Binding on DataContextProperty doesn't work properly. Only direct change of AssociatedObject.DataContext property triggers binding.
            var fElement = AssociatedObject as FrameworkElement;
            if(fElement != null) {
                SetValue(ServicesClientInternalProperty, fElement.DataContext);
                return;
            }
        }
        protected ISupportServices GetServicesClient() {
            return GetValue(ServicesClientInternalProperty) as ISupportServices;
        }
        protected override void OnDataContextChange(object dataContext) {
            SetValue(ServicesClientInternalProperty, dataContext);
        }
        protected virtual void OnServicesClientChanged(ISupportServices oldServiceClient, ISupportServices newServiceClient) {
            oldServiceClient.Do(x => x.ServiceContainer.UnregisterService(this));
            newServiceClient.Do(x => x.ServiceContainer.RegisterService(Name, this, YieldToParent));
        }
    }
    public abstract class ServiceBase : ServiceBaseGeneric<FrameworkElement> { }
#else
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
#endif
}
#if !FREE && !NETFX_CORE
namespace DevExpress.Mvvm.UI.Native {
    public static class AssignableServiceHelper<TOwner, TService> 
        where TOwner : FrameworkElement 
        where TService : class 
    {
        public static DependencyProperty RegisterServiceProperty(string name) {
            return DependencyProperty.Register(name, typeof(TService), typeof(TOwner), new PropertyMetadata(null, OnServicePropertyChanged));
        }
        static void OnServicePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (e.OldValue as ServiceBase).Do(x => x.Detach());
            (e.NewValue as ServiceBase).Do(x => {
                x.ShouldInject = false;
                x.Attach(d); 
            });
        }
        public static TService GetService(TOwner owner, DependencyProperty property, object templateKey) {
            var service = (TService)owner.GetValue(property);
            if(service != null)
                return service;
            service = LoadServiceFromTemplate(owner, templateKey);
#if NETFX_CORE
            owner.SetValue(property, service);
#else
            owner.SetCurrentValue(property, service);
#endif
            return service;
        }
        static TService LoadServiceFromTemplate(TOwner owner, object templateKey) {
            var template = (DataTemplate)DevExpress.Xpf.Core.ResourceHelper.FindResource(owner, templateKey);
            return TemplateHelper.LoadFromTemplate<TService>(template);
        }
    }
    public static class AssignableServiceHelper2<TOwner, TService>
        where TService : class {
        public static DependencyProperty RegisterServiceTemplateProperty(string name) {
            return RegisterServiceTemplateProperty<DependencyObject>(name, null);
        }
        public static DependencyProperty RegisterServiceTemplateProperty<T>(string name, Action<T> onChanged) {
            var metadata = onChanged == null ? new PropertyMetadata(null) : new PropertyMetadata(null, (d, e) => onChanged((T)(object)d));
            return DependencyProperty.Register(name, typeof(DataTemplate), typeof(TOwner), metadata);
        }
        public static void DoServiceAction(DependencyObject owner, DataTemplate template, Action<TService> action) {
            TService service = TemplateHelper.LoadFromTemplate<TService>(template);
            (service as ServiceBase).Do(x => {
                x.ShouldInject = false;
                x.Attach(owner);
            });
            try {
                action(service);
            } finally {
                (service as ServiceBase).Do(x => x.Detach());
            }
        }
        public static void DoServiceAction(DependencyObject owner, TService service, Action<TService> action) {
            (service as ServiceBase).Do(x => {
                x.ShouldInject = false;
                x.Attach(owner);
            });
            try {
                action(service);
            } finally {
                (service as ServiceBase).Do(x => x.Detach());
            }
        }
    }
}
#endif