using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System;
using DevExpress.Mvvm.Native;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using DevExpress.Mvvm.UI.Native;
#else
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Navigation;
using DevExpress.Mvvm.UI.Native;
#endif

namespace DevExpress.Mvvm.UI {
#if !SILVERLIGHT && !NETFX_CORE
    [RuntimeNameProperty("Name")]
#endif
    public abstract class ServiceBase : Behavior<FrameworkElement> {
#if !NETFX_CORE
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(ServiceBase), new PropertyMetadata(null));
#endif
        [IgnoreDependencyPropertiesConsistencyCheckerAttribute]
        static readonly DependencyProperty ServicesClientInternalProperty =
            DependencyProperty.Register("ServicesClientInternal", typeof(object), typeof(ServiceBase),
            new PropertyMetadata(null, (d, e) => ((ServiceBase)d).RegisterServices()));
#if !NETFX_CORE
        public string Name {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
#endif
        public bool YieldToParent { get; set; }
        internal bool ShouldInject { get; set; }
        ISupportServices ServicesClientInternal { get { return GetValue(ServicesClientInternalProperty) as ISupportServices; } }

        protected ServiceBase() {
            ShouldInject = true;
        }
        protected override void OnAttached() {
            base.OnAttached();
            SetInjectBinding();
        }
        void SetInjectBinding() {
            if(ShouldInject)
                BindingOperations.SetBinding(this, ServicesClientInternalProperty, new Binding() { Path = new PropertyPath("DataContext"), Source = AssociatedObject });
        }
#if NETFX_CORE
        protected override void OnDataContextChange(object dataContext) {
            base.OnDataContextChange(dataContext);
            SetInjectBinding();
        }
#endif
        protected override void OnDetaching() {
#if SILVERLIGHT || NETFX_CORE
            ClearValue(ServicesClientInternalProperty);
#else
            BindingOperations.ClearBinding(this, ServicesClientInternalProperty);
#endif
            base.OnDetaching();
        }
        protected ISupportServices GetServicesClient() {
            return ServicesClientInternal;
        }
#if !SILVERLIGHT && !NETFX_CORE
        protected Uri GetBaseUri() {
            Uri baseUri = BaseUriHelper.GetBaseUri(this);
            if(baseUri != null || AssociatedObject == null) return baseUri;
            return BaseUriHelper.GetBaseUri(AssociatedObject);
        }
#endif
        void RegisterServices() {
            var servicesClient = GetServicesClient();
            if(servicesClient == null)
                return;
            servicesClient.ServiceContainer.RegisterService(Name, this, YieldToParent);
        }
    }
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
#if SILVERLIGHT  || NETFX_CORE
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
        where TOwner : FrameworkElement
        where TService : class {
        public static DependencyProperty RegisterServiceTemplateProperty(string name) {
            return DependencyProperty.Register(name, typeof(DataTemplate), typeof(TOwner), new PropertyMetadata(null));
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