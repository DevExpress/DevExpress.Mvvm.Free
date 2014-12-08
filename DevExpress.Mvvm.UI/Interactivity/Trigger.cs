using System;
using System.Windows;
using DevExpress.Mvvm.UI.Interactivity.Internal;

#if !NETFX_CORE
using System.Windows.Data;
using DevExpress.Mvvm.UI.Native;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.ApplicationModel;
using DevExpress.Mvvm.UI.Native;
#endif

namespace DevExpress.Mvvm.UI.Interactivity {
    public abstract class TriggerBase : Behavior {
        internal TriggerBase(Type type)
            : base(type) {
        }
    }
#if !NETFX_CORE
    public abstract class TriggerBase<T> : TriggerBase where T : DependencyObject {
#else
    public abstract class TriggerBase<T> : TriggerBase where T : FrameworkElement {
#endif
        protected TriggerBase()
            : base(typeof(T)) {
        }
        protected new T AssociatedObject {
            get { return (T)base.AssociatedObject; }
        }
    }
#if !NETFX_CORE
    public class EventTriggerBase<T> : TriggerBase<T> where T : DependencyObject {
#else
    public class EventTriggerBase<T> : TriggerBase<T> where T : FrameworkElement {
#endif
        #region Static
        [IgnoreDependencyPropertiesConsistencyChecker]
        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(EventTriggerBase<T>),
            new PropertyMetadata("Loaded", (d, e) => ((EventTriggerBase<T>)d).OnEventNameChanged((string)e.OldValue, (string)e.NewValue)));
#if !SILVERLIGHT && !NETFX_CORE
        [IgnoreDependencyPropertiesConsistencyChecker]
        public static readonly DependencyProperty EventProperty =
            DependencyProperty.Register("Event", typeof(RoutedEvent), typeof(EventTriggerBase<T>),
            new PropertyMetadata(null, (d, e) => ((EventTriggerBase<T>)d).OnEventChanged((RoutedEvent)e.OldValue, (RoutedEvent)e.NewValue)));
#endif
        [IgnoreDependencyPropertiesConsistencyChecker]
        public static readonly DependencyProperty SourceNameProperty =
            DependencyProperty.Register("SourceName", typeof(string), typeof(EventTriggerBase<T>),
            new PropertyMetadata(null, (d, e) => ((EventTriggerBase<T>)d).OnSourceNameChanged()));
        [IgnoreDependencyPropertiesConsistencyChecker]
        public static readonly DependencyProperty SourceObjectProperty =
            DependencyProperty.Register("SourceObject", typeof(object), typeof(EventTriggerBase<T>),
            new PropertyMetadata(null, (d, e) => ((EventTriggerBase<T>)d).OnSourceObjectChanged()));

        static BindingExpression GetBindingExp(DependencyObject d, DependencyProperty dp) {
#if !SILVERLIGHT && !NETFX_CORE
            return BindingOperations.GetBindingExpression(d, dp);
#else
            if(d is FrameworkElement)
                return ((FrameworkElement)d).GetBindingExpression(dp);
            return d.ReadLocalValue(dp) as BindingExpression;
#endif
        }
        static string GetObjectName(object obj) {
            FrameworkElement fe = obj as FrameworkElement;
            if(fe != null)
                return fe.Name;
#if !SILVERLIGHT && !NETFX_CORE
            FrameworkContentElement fce = obj as FrameworkContentElement;
            if(fce != null)
                return fce.Name;
#endif
            return null;
        }
        static DependencyObject FindObject(DependencyObject root, string elementName, bool useVisualTree) {
            if(GetObjectName(root) == elementName) return root;
            DependencyObject res = null;
            FrameworkElement fe = root as FrameworkElement;
            FrameworkElement feParent = fe.Parent as FrameworkElement;
            FrameworkElement el = feParent ?? fe;

#if !SILVERLIGHT && !NETFX_CORE
            try {
                res = LogicalTreeHelper.FindLogicalNode(el, elementName);
            } catch { }
            if(res != null) return res;

            FrameworkContentElement fce = root as FrameworkContentElement;
            res = fce != null ? (DependencyObject)fce.FindName(elementName) : null;
            if(res != null) return res;
#endif
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
#if !SILVERLIGHT && !NETFX_CORE
        public RoutedEvent Event {
            get { return (RoutedEvent)GetValue(EventProperty); }
            set { SetValue(EventProperty, value); }
        }
#endif
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
            if(ViewModelBase.IsInDesignMode) return;
            if(!IsAttached) return;
            if(Source != null && !forceResolving)
                return;
            if(SourceObject != null) {
                Source = SourceObject;
                return;
            }
#if !SILVERLIGHT && !NETFX_CORE
            bool useVisualTreeCore = useVisualTree ?? false;
#else
            bool useVisualTreeCore = useVisualTree ?? true;
#endif
            var sourceObjectBinding = GetBindingExp(this, SourceObjectProperty);
            if(sourceObjectBinding != null) {
                string elementName = null;
                if(sourceObjectBinding.ParentBinding != null)
                    elementName = sourceObjectBinding.ParentBinding.ElementName;
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
#if !SILVERLIGHT && !NETFX_CORE
            EventHelper.UnsubscribeFromEvent(oldSource, Event);
#endif
            EventHelper.UnsubscribeFromEvent(oldSource, EventName);
            EventHelper.SubscribeToEvent(newSource, EventName);
#if !SILVERLIGHT && !NETFX_CORE
            EventHelper.SubscribeToEvent(newSource, Event);
#endif
        }
        protected virtual void OnEventNameChanged(string oldEventName, string newEventName) {
#if !SILVERLIGHT && !NETFX_CORE
            if(newEventName != null)
                Event = null;
#endif
            if(!IsAttached) return;
            EventHelper.UnsubscribeFromEvent(Source, oldEventName);
            EventHelper.SubscribeToEvent(Source, newEventName);
        }
#if !SILVERLIGHT && !NETFX_CORE
        protected virtual void OnEventChanged(RoutedEvent oldRoutedEvent, RoutedEvent newRoutedEvent) {
            if(newRoutedEvent != null)
                EventName = null;
            if(!IsAttached) return;
            EventHelper.UnsubscribeFromEvent(Source, oldRoutedEvent);
            EventHelper.SubscribeToEvent(Source, newRoutedEvent);
        }
#endif
        protected override void OnAttached() {
            base.OnAttached();
#if !SILVERLIGHT && !NETFX_CORE
            EventHelper.UnsubscribeFromEvent(Source, Event);
#endif
            EventHelper.UnsubscribeFromEvent(Source, EventName);
            EventHelper.SubscribeToEvent(Source, EventName);
#if !SILVERLIGHT && !NETFX_CORE
            EventHelper.SubscribeToEvent(Source, Event);
#endif
            ResolveSource(false);
#if !NETFX_CORE
            Dispatcher.BeginInvoke(new Action(() => ResolveSource(false)));
#else
#pragma warning disable 4014
            if (!DesignMode.DesignModeEnabled)
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, new Windows.UI.Core.DispatchedHandler(() => ResolveSource(false)));
#pragma warning restore 4014

#endif
            SubsribeAssociatedObject();
        }
        protected override void OnDetaching() {
            UnsubscribeAssociatedObject();
            EventHelper.UnsubscribeFromEvent(Source, EventName);
#if !SILVERLIGHT && !NETFX_CORE
            EventHelper.UnsubscribeFromEvent(Source, Event);
#endif
            Source = null;
            base.OnDetaching();
        }
        void SubsribeAssociatedObject() {
            UnsubscribeAssociatedObject();
            FrameworkElement fe = AssociatedObject as FrameworkElement;
            if(fe != null) {
#if !SILVERLIGHT && !NETFX_CORE
                fe.Initialized += OnAssociatedObjectUpdated;
#endif
                fe.LayoutUpdated += AssociatedObjectLayoutUpdated;
                fe.SizeChanged += AssociatedObjectSizeChanged;
                fe.Loaded += AssociatedObjectLoaded;
                return;
            }
#if !SILVERLIGHT && !NETFX_CORE
            FrameworkContentElement fce = AssociatedObject as FrameworkContentElement;
            if(fce != null) {
                fce.Initialized += OnAssociatedObjectUpdated;
                fce.Loaded += OnAssociatedObjectUpdated;
                return;
            }
#endif
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
#if !SILVERLIGHT && !NETFX_CORE
                fe.Initialized -= OnAssociatedObjectUpdated;
#endif
                fe.LayoutUpdated -= AssociatedObjectLayoutUpdated;
                fe.SizeChanged -= AssociatedObjectSizeChanged;
                fe.Loaded -= AssociatedObjectLoaded;
            }
#if !SILVERLIGHT && !NETFX_CORE
            FrameworkContentElement fce = AssociatedObject as FrameworkContentElement;
            if(fce != null) {
                fce.Initialized -= OnAssociatedObjectUpdated;
                fce.Loaded -= OnAssociatedObjectUpdated;
                return;
            }
#endif
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
#if !NETFX_CORE
    public class EventTrigger : EventTriggerBase<DependencyObject> {
#else
    public class EventTrigger : EventTriggerBase<FrameworkElement> {
#endif
        public EventTrigger() { }
        public EventTrigger(string eventName)
            : this() {
            EventName = eventName;
        }
    }
}