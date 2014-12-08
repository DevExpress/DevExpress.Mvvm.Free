using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Interactivity.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
#if NETFX_CORE
using Windows.UI.Xaml;
using DevExpress.Mvvm.Native;
#endif

namespace DevExpress.Mvvm.UI {
    public class DependencyPropertyBehavior : Behavior<DependencyObject> {
#if !NETFX_CORE
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
#endif
        public static readonly DependencyProperty BindingProperty = DependencyProperty.RegisterAttached("Binding", typeof(object), typeof(DependencyPropertyBehavior),
#if !SILVERLIGHT && !NETFX_CORE
            new FrameworkPropertyMetadata(null, (d, e) => ((DependencyPropertyBehavior)d).OnBindingPropertyChanged(e))  { BindsTwoWayByDefault = true } );
#else
            new PropertyMetadata(null, (d, e) => ((DependencyPropertyBehavior)d).OnBindingPropertyChanged(e)));
#endif
        EventTriggerEventSubscriber EventHelper;
        public DependencyPropertyBehavior() {
            EventHelper = new EventTriggerEventSubscriber(OnEvent);
        }
        public object Binding {
            get { return GetValue(BindingProperty); }
            set { SetValue(BindingProperty, value); }
        }
        public string PropertyName { get; set; }
        public string EventName { get; set; }
        protected PropertyInfo PropertyInfo {
#if !NETFX_CORE
            get { return PropertyAssociatedObject.GetType().GetProperty(ShortPropertyName, BindingFlags.Instance | BindingFlags.Public); }
#else
            get { return PropertyAssociatedObject.GetType().GetProperty(ShortPropertyName); }
#endif

        }
        protected object PropertyAssociatedObject {
            get { return GetAssociatedObjectForName(PropertyName); }
        }
        protected object EventAssociatedObject {
            get { return GetAssociatedObjectForName(EventName); }
        }
        protected string ShortEventName {
            get { return GetShortName(EventName); }
        }
        protected string ShortPropertyName {
            get { return GetShortName(PropertyName); }
        }
        protected override void OnAttached() {
            EventHelper.UnsubscribeFromEvent(EventAssociatedObject, ShortEventName);
            EventHelper.SubscribeToEvent(EventAssociatedObject, ShortEventName);
        }
        protected override void OnDetaching() {
            EventHelper.UnsubscribeFromEvent(EventAssociatedObject, ShortEventName);
        }
        void OnEvent(object sender, object eventArgs) {
            Binding = PropertyInfo.GetValue(PropertyAssociatedObject, null);
        }
        void OnBindingPropertyChanged(DependencyPropertyChangedEventArgs e) {
            if(PropertyAssociatedObject == null)
                return;
            object oldValue = PropertyInfo.GetValue(PropertyAssociatedObject, null);
            if(oldValue.Equals(e.NewValue)) return;
            if(PropertyInfo.CanWrite)
                PropertyInfo.SetValue(PropertyAssociatedObject, Convert.ChangeType(e.NewValue, PropertyInfo.PropertyType, null), null);
        }
        protected virtual object GetAssociatedObjectForName(string name) {
            if(AssociatedObject == null)
                return null;
            var namePaths = name.Split('.');
            object currentObject = AssociatedObject;
            foreach(var propertyPath in namePaths.Take(namePaths.Length - 1)) {
#if !NETFX_CORE
                currentObject = currentObject.GetType().GetProperty(propertyPath, bindingFlags).GetValue(currentObject, null);
#else
                currentObject = currentObject.GetType().GetProperty(propertyPath).GetValue(currentObject, null);
#endif
            }
            return currentObject;
        }
        protected virtual string GetShortName(string name) {
            return name.Split('.').Last();
        }
    }
}