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
    public abstract class ServiceBase : Behavior<FrameworkElement> {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(ServiceBase), new PropertyMetadata(null));
        public string Name { get { return (string)GetValue(NameProperty); } set { SetValue(NameProperty, value); } }
        [IgnoreDependencyPropertiesConsistencyCheckerAttribute]
        static readonly DependencyProperty ServicesClientInternalProperty = DependencyProperty.Register("ServicesClientInternal", typeof(object), typeof(ServiceBase), new PropertyMetadata(null,
            (d, e) => ((ServiceBase)d).OnServicesClientInternalChanged(e.OldValue as ISupportServices, e.NewValue as ISupportServices)));
        public bool YieldToParent { get; set; }
        internal bool ShouldInject { get; set; }

        protected ServiceBase() {
            ShouldInject = true;
        }
        protected override void OnAttached() {
            base.OnAttached();
            SetInjectBinding();
        }
        protected override void OnDetaching() {
            BindingOperations.ClearBinding(this, ServicesClientInternalProperty);
            base.OnDetaching();
        }
        void SetInjectBinding() {
            if(ShouldInject)
                BindingOperations.SetBinding(this, ServicesClientInternalProperty, new Binding() { Path = new PropertyPath("DataContext"), Source = AssociatedObject });
        }
        protected ISupportServices GetServicesClient() {
            return GetValue(ServicesClientInternalProperty) as ISupportServices;
        }
        protected Uri GetBaseUri() {
            Uri baseUri = BaseUriHelper.GetBaseUri(this);
            if(baseUri != null || AssociatedObject == null) return baseUri;
            return BaseUriHelper.GetBaseUri(AssociatedObject);
        }
        void OnServicesClientInternalChanged(ISupportServices oldServiceClient, ISupportServices newServiceClient) {
            oldServiceClient.Do(x => x.ServiceContainer.UnregisterService(this));
            newServiceClient.Do(x => x.ServiceContainer.RegisterService(Name, this, YieldToParent));
        }
    }
}