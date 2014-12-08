#if !NETFX_CORE
using DevExpress.Mvvm.UI.Interactivity.Internal;
using System;
using System.Windows;
using System.Windows.Media.Animation;
#else
using System;
using Windows.UI.Xaml;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity.Internal;
#endif

namespace DevExpress.Mvvm.UI.Interactivity {
    public interface IAttachableObject {
        DependencyObject AssociatedObject { get; }
        void Attach(DependencyObject dependencyObject);
        void Detach();
    }

#if SILVERLIGHT
    public abstract class AttachableObjectBase : DependencyObject, IAttachableObject {
#elif NETFX_CORE
    public abstract class AttachableObjectBase : FrameworkElement, IAttachableObject {
#else
    public abstract class AttachableObjectBase : Animatable, IAttachableObject {
#endif
        public bool IsAttached { get; private set; }
        internal bool _AllowAttachInDesignMode { get { return AllowAttachInDesignMode; } }
        protected virtual bool AllowAttachInDesignMode {
            get {
                if(InteractionHelper.GetBehaviorInDesignMode(this) == InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode)
                    return true;
                return false;
            }
        }
        Type associatedType;
        protected virtual Type AssociatedType {
            get {
                VerifyRead();
                return associatedType;
            }
        }
        DependencyObject associatedObject;
        public DependencyObject AssociatedObject {
            get {
                VerifyRead();
                return associatedObject;
            }
            private set {
                VerifyRead();
                if(associatedObject == value) return;
                VerifyWrite();
                associatedObject = value;
                NotifyChanged();

                EventHandler handler = AssociatedObjectChanged;
                if(handler != null)
                    handler(this, EventArgs.Empty);
            }
        }
        internal event EventHandler AssociatedObjectChanged;

        internal AttachableObjectBase(Type type) {
            associatedType = type;
        }
        public void Attach(DependencyObject obj) {
            if(AssociatedObject == obj)
                return;
            if(AssociatedObject != null)
                throw new InvalidOperationException("Cannot attach this object twice");
            Type type = obj.GetType();
            if(!this.AssociatedType.IsAssignableFrom(type))
                throw new InvalidOperationException(string.Format("This object cannot be attached to a {0} object", type.ToString()));
            AssociatedObject = obj;
            IsAttached = true;
            OnAttached();
        }
        public void Detach() {
            OnDetaching();
            AssociatedObject = null;
            IsAttached = false;
        }
        protected virtual void OnAttached() {
#if NETFX_CORE
            if(AssociatedObject is FrameworkElement) {
                var frameworkElement = AssociatedObject as FrameworkElement;
                DataContext = frameworkElement.DataContext;
                frameworkElement.DataContextChanged += frameworkElement_DataContextChanged;
            }
#endif
        }
#if NETFX_CORE
        void frameworkElement_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
            OnDataContextChange(args.NewValue);
        }
        protected virtual void OnDataContextChange(object dataContext) {
            DataContext = dataContext;
        }
#endif
        protected virtual void OnDetaching() {
#if NETFX_CORE
            if(AssociatedObject is FrameworkElement) {
                var frameworkElement = AssociatedObject as FrameworkElement;
                DataContext = null;
                frameworkElement.DataContextChanged -= frameworkElement_DataContextChanged;
            }
#endif
        }

        protected void VerifyRead() {
#if !SILVERLIGHT && !NETFX_CORE
            ReadPreamble();
#endif
        }
        protected void VerifyWrite() {
#if !SILVERLIGHT && !NETFX_CORE
            WritePreamble();
#endif
        }
        protected void NotifyChanged() {
#if !SILVERLIGHT && !NETFX_CORE
            WritePostscript();
#endif
        }

#if !SILVERLIGHT && !NETFX_CORE
        protected override Freezable CreateInstanceCore() {
            return (Freezable)Activator.CreateInstance(GetType());
        }
#endif
    }
}