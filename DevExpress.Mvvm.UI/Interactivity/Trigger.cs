using System;
using System.Windows;
using DevExpress.Mvvm.UI.Interactivity.Internal;

using System.Windows.Data;
using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI.Interactivity {
    public abstract class TriggerBase : Behavior {
        internal TriggerBase(Type type)
            : base(type) {
        }
    }
    public abstract class TriggerBase<T> : TriggerBase where T : DependencyObject {
        protected TriggerBase()
            : base(typeof(T)) {
        }
        protected new T AssociatedObject {
            get { return (T)base.AssociatedObject; }
        }
    }
    public class EventTriggerBase<T> : TriggerBase<T> where T : DependencyObject {
        #region Static
        [IgnoreDependencyPropertiesConsistencyChecker]
        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(EventTriggerBase<T>),
            new PropertyMetadata("Loaded", (d, e) => ((EventTriggerBase<T>)d).OnEventNameChanged((string)e.OldValue, (string)e.NewValue)));
        [IgnoreDependencyPropertiesConsistencyChecker]
        public static readonly DependencyProperty EventProperty =
            DependencyProperty.Register("Event", typeof(RoutedEvent), typeof(EventTriggerBase<T>),
            new PropertyMetadata(null, (d, e) => ((EventTriggerBase<T>)d).OnEventChanged((RoutedEvent)e.OldValue, (RoutedEvent)e.NewValue)));
        [IgnoreDependencyPropertiesConsistencyChecker]
        public static readonly DependencyProperty SourceNameProperty =
            DependencyProperty.Register("SourceName", typeof(string), typeof(EventTriggerBase<T>),
            new PropertyMetadata(null, (d, e) => ((EventTriggerBase<T>)d).OnSourceNameChanged()));
        [IgnoreDependencyPropertiesConsistencyChecker]
        public static readonly DependencyProperty SourceObjectProperty =
            DependencyProperty.Register("SourceObject", typeof(object), typeof(EventTriggerBase<T>),
            new PropertyMetadata(null, (d, e) => ((EventTriggerBase<T>)d).OnSourceObjectChanged()));

        static BindingExpression GetBindingExp(DependencyObject d, DependencyProperty dp) {
            return BindingOperations.GetBindingExpression(d, dp);
        }
        static string GetObjectName(object obj) {
            FrameworkElement fe = obj as FrameworkElement;
            if(fe != null)
                return fe.Name;
            FrameworkContentElement fce = obj as FrameworkContentElement;
            if(fce != null)
                return fce.Name;
            return null;
        }
        static DependencyObject FindObject(DependencyObject root, string elementName, bool useVisualTree) {
            if(GetObjectName(root) == elementName) return root;
            DependencyObject res = null;
            FrameworkElement fe = root as FrameworkElement;
            FrameworkElement feParent = fe.Parent as FrameworkElement;
            FrameworkElement el = feParent ?? fe;

            try {
                res = LogicalTreeHelper.FindLogicalNode(el, elementName);
            } catch { }
            if(res != null) return res;

            FrameworkContentElement fce = root as FrameworkContentElement;
            res = fce != null ? (DependencyObject)fce.FindName(elementName) : null;
            if(res != null) return res;
            res = el != null ? (DependencyObject)el.FindName(elementName) : null;
            if(res != null) return res;

            if(useVisualTree) {
                res = feParent != null ? LayoutHelper.FindElementByName(feParent, elementName) : null;
                if(res != null) return res;

                res = fe != null ? LayoutHelper.FindElementByName(fe, elementName) : null;
                if(res != null) return res;
            }

            return null;
        }
        #endregion
        internal int RaiseSourceChangedCount = 0;
        public string EventName {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }
        public RoutedEvent Event {
            get { return (RoutedEvent)GetValue(EventProperty); }
            set { SetValue(EventProperty, value); }
        }
        public string SourceName {
            get { return (string)GetValue(SourceNameProperty); }
            set { SetValue(SourceNameProperty, value); }
        }
        public object SourceObject {
            get { return (object)GetValue(SourceObjectProperty); }
            set { SetValue(SourceObjectProperty, value); }
        }
        object source;
        public object Source {
            get {
                VerifyRead();
                return source;
            }
            private set {
                VerifyRead();
                if(source == value) return;
                VerifyWrite();
                object oldValue = source;
                source = value;
                NotifyChanged();
                OnSourceChanged(oldValue, source);
            }
        }

        void ResolveSource(bool forceResolving, bool? useVisualTree = null) {
            if(ViewModelBase.IsInDesignMode
                && !InteractionHelper.GetEnableBehaviorsInDesignTime(AssociatedObject)
            ) return;
            if(!IsAttached) return;
            if(Source != null && !forceResolving)
                return;
            if(SourceObject != null) {
                Source = SourceObject;
                return;
            }
            bool useVisualTreeCore = useVisualTree ?? false;
            var sourceObjectBinding = GetBindingExp(this, SourceObjectProperty);
            if(sourceObjectBinding != null) {
                string elementName = null;
                if(sourceObjectBinding.ParentBinding != null)
                    elementName = sourceObjectBinding.ParentBinding.ElementName;
                if(!string.IsNullOrEmpty(elementName))
                    Source = FindObject(AssociatedObject, elementName, useVisualTreeCore);
                return;
            }
            var sourceNameBinding = GetBindingExp(this, SourceNameProperty);
            if(!string.IsNullOrEmpty(SourceName) || sourceNameBinding != null) {
                Source = FindObject(AssociatedObject, SourceName, useVisualTreeCore);
                return;
            }
            Source = AssociatedObject;
            return;
        }
        void OnSourceNameChanged() {
            ResolveSource(true);
        }
        void OnSourceObjectChanged() {
            ResolveSource(true);
        }

        EventTriggerEventSubscriber EventHelper;
        public EventTriggerBase()
            : base() {
                EventHelper = new EventTriggerEventSubscriber(OnEvent);
        }
        protected virtual void OnEvent(object sender, object eventArgs) { }
        protected virtual void OnSourceChanged(object oldSource, object newSource) {
            RaiseSourceChangedCount++;
            EventHelper.UnsubscribeFromEvent(oldSource, Event);
            EventHelper.UnsubscribeFromEvent(oldSource, EventName);
            EventHelper.SubscribeToEvent(newSource, EventName);
            EventHelper.SubscribeToEvent(newSource, Event);
        }
        protected virtual void OnEventNameChanged(string oldEventName, string newEventName) {
            if(newEventName != null)
                Event = null;
            if(!IsAttached) return;
            EventHelper.UnsubscribeFromEvent(Source, oldEventName);
            EventHelper.SubscribeToEvent(Source, newEventName);
        }
        protected virtual void OnEventChanged(RoutedEvent oldRoutedEvent, RoutedEvent newRoutedEvent) {
            if(newRoutedEvent != null)
                EventName = null;
            if(!IsAttached) return;
            EventHelper.UnsubscribeFromEvent(Source, oldRoutedEvent);
            EventHelper.SubscribeToEvent(Source, newRoutedEvent);
        }
        protected override void OnAttached() {
            base.OnAttached();
            EventHelper.UnsubscribeFromEvent(Source, Event);
            EventHelper.UnsubscribeFromEvent(Source, EventName);
            EventHelper.SubscribeToEvent(Source, EventName);
            EventHelper.SubscribeToEvent(Source, Event);
            ResolveSource(false);
            Dispatcher.BeginInvoke(new Action(() => ResolveSource(false)));
            SubsribeAssociatedObject();
        }
        protected override void OnDetaching() {
            UnsubscribeAssociatedObject();
            EventHelper.UnsubscribeFromEvent(Source, EventName);
            EventHelper.UnsubscribeFromEvent(Source, Event);
            Source = null;
            base.OnDetaching();
        }
        void SubsribeAssociatedObject() {
            UnsubscribeAssociatedObject();
            FrameworkElement fe = AssociatedObject as FrameworkElement;
            if(fe != null) {
                fe.Initialized += OnAssociatedObjectUpdated;
                fe.LayoutUpdated += AssociatedObjectLayoutUpdated;
                fe.SizeChanged += AssociatedObjectSizeChanged;
                fe.Loaded += AssociatedObjectLoaded;
                return;
            }
            FrameworkContentElement fce = AssociatedObject as FrameworkContentElement;
            if(fce != null) {
                fce.Initialized += OnAssociatedObjectUpdated;
                fce.Loaded += OnAssociatedObjectUpdated;
                return;
            }
        }

        void AssociatedObjectLoaded(object sender, RoutedEventArgs e) {
            OnAssociatedObjectUpdated(sender, EventArgs.Empty);
        }

        void AssociatedObjectSizeChanged(object sender, SizeChangedEventArgs e) {
            OnAssociatedObjectUpdated(sender, EventArgs.Empty);
        }

        void AssociatedObjectLayoutUpdated(object sender, object e) {
            OnAssociatedObjectUpdated(sender, EventArgs.Empty);
        }

        void UnsubscribeAssociatedObject() {
            FrameworkElement fe = AssociatedObject as FrameworkElement;
            if(fe != null) {
                fe.Initialized -= OnAssociatedObjectUpdated;
                fe.LayoutUpdated -= AssociatedObjectLayoutUpdated;
                fe.SizeChanged -= AssociatedObjectSizeChanged;
                fe.Loaded -= AssociatedObjectLoaded;
            }
            FrameworkContentElement fce = AssociatedObject as FrameworkContentElement;
            if(fce != null) {
                fce.Initialized -= OnAssociatedObjectUpdated;
                fce.Loaded -= OnAssociatedObjectUpdated;
                return;
            }
        }
        void OnAssociatedObjectUpdated(object sender, EventArgs e) {
            ResolveSource(false);
            FrameworkElement associatedObject = AssociatedObject as FrameworkElement;
            if(associatedObject == null) return;
            if(LayoutHelper.IsElementLoaded(associatedObject) || Source != null) {
                UnsubscribeAssociatedObject();
                if(Source == null) {
                    ResolveSource(false, true);
                }
            }
        }
    }
    public class EventTrigger : EventTriggerBase<DependencyObject> {
        public EventTrigger() { }
        public EventTrigger(string eventName)
            : this() {
            EventName = eventName;
        }
    }
}