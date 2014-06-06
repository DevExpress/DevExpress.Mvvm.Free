using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Native;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System;
using System.Windows.Navigation;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI {
#if !SILVERLIGHT
    [RuntimeNameProperty("Name")]
#endif
    public abstract class ServiceBase : Behavior<FrameworkElement> {
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(ServiceBase), new PropertyMetadata(null));
        [IgnoreDependencyPropertiesConsistencyCheckerAttribute]
        static readonly DependencyProperty ServicesClientInternalProperty =
            DependencyProperty.Register("ServicesClientInternal", typeof(object), typeof(ServiceBase),
            new PropertyMetadata(null, (d, e) => ((ServiceBase)d).RegisterServices()));

        public string Name {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        internal bool ShouldInject { get; set; }
        ISupportServices ServicesClientInternal { get { return GetValue(ServicesClientInternalProperty) as ISupportServices; } }

        protected ServiceBase() {
            ShouldInject = true;
        }
        protected override void OnAttached() {
            base.OnAttached();
            if(ShouldInject)
                BindingOperations.SetBinding(this, ServicesClientInternalProperty, new Binding("DataContext") { Source = AssociatedObject });
        }
        protected override void OnDetaching() {
#if SILVERLIGHT
            ClearValue(ServicesClientInternalProperty);
#else
            BindingOperations.ClearBinding(this, ServicesClientInternalProperty);
#endif
            base.OnDetaching();
        }
        protected ISupportServices GetServicesClient() {
            return ServicesClientInternal;
        }
#if !SILVERLIGHT
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
            servicesClient.ServiceContainer.RegisterService(Name, this);
        }
    }
}