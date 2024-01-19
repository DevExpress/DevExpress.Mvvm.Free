using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Interactivity.Internal;
using System;
using System.Linq;
using System.Reflection;
using DevExpress.Mvvm.Native;
using System.Windows;

namespace DevExpress.Mvvm.UI {
    public class DependencyPropertyBehavior : Behavior<DependencyObject> {
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

        public static readonly DependencyProperty BindingProperty = DependencyProperty.RegisterAttached("Binding", typeof(object), typeof(DependencyPropertyBehavior),
            new FrameworkPropertyMetadata(null, (d, e) => ((DependencyPropertyBehavior)d).OnBindingPropertyChanged())  { BindsTwoWayByDefault = true } );
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
            get { return PropertyAssociatedObject.GetType().GetProperty(ShortPropertyName, bindingFlags); }
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
            if(Binding != null) OnBindingPropertyChanged();
            if(string.IsNullOrEmpty(EventName)) return;
            EventHelper.UnsubscribeFromEvent(EventAssociatedObject, ShortEventName);
            EventHelper.SubscribeToEvent(EventAssociatedObject, ShortEventName);
        }
        protected override void OnDetaching() {
            if(string.IsNullOrEmpty(EventName)) return;
            EventHelper.UnsubscribeFromEvent(EventAssociatedObject, ShortEventName);
        }
        void OnEvent(object sender, object eventArgs) {
            Binding = PropertyAssociatedObject != null ? PropertyInfo.GetValue(PropertyAssociatedObject, null) : null;
        }
        void OnBindingPropertyChanged() {
            if(PropertyAssociatedObject == null)
                return;
            object oldValue = PropertyInfo.GetValue(PropertyAssociatedObject, null);
            if(object.Equals(oldValue, Binding)) return;
            if(PropertyInfo.CanWrite)
                PropertyInfo.SetValue(PropertyAssociatedObject, Convert.ChangeType(Binding, PropertyInfo.PropertyType, null), null);
        }
        protected virtual object GetAssociatedObjectForName(string name) {
            if(AssociatedObject == null)
                return null;
            var namePaths = name.Split('.');
            object currentObject = AssociatedObject;
            foreach(var propertyPath in namePaths.Take(namePaths.Length - 1)) {
                if (currentObject == null) return null;
                currentObject = currentObject.GetType().GetProperty(propertyPath, bindingFlags).GetValue(currentObject, null);
            }
            return currentObject;
        }
        protected virtual string GetShortName(string name) {
            return name.Split('.').Last();
        }
    }
}