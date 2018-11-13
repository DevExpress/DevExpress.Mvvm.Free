using System.ComponentModel;
#if !NETFX_CORE
using DevExpress.Mvvm.UI.Interactivity.Internal;
using System;
using System.Windows;
using System.Windows.Media.Animation;
#else
using System;
using DevExpress.Mvvm.Native;
using DevExpress.Core.Native;
using Windows.UI.Xaml;
using DevExpress.Mvvm.UI.Interactivity.Internal;
#endif

namespace DevExpress.Mvvm.UI.Interactivity {
    public interface IAttachableObject {
        DependencyObject AssociatedObject { get; }
        void Attach(DependencyObject dependencyObject);
        void Detach();
    }

#if NETFX_CORE
    public abstract class AttachableObjectBase : DependencyObject, IAttachableObject, INotifyPropertyChanged {
#else
    public abstract class AttachableObjectBase : Animatable, IAttachableObject, INotifyPropertyChanged {
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
                RaisePropertyChanged("AssociatedObject");
            }
        }
        internal event EventHandler AssociatedObjectChanged;

        internal AttachableObjectBase(Type type) {
            associatedType = type;
        }
        PropertyChangedEventHandler propertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }
        protected void RaisePropertyChanged(string name) {
            if(propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(name));
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
#if NETFX_CORE
            DataContextRegistrator.Register(AssociatedObject, this);
            var fElement = AssociatedObject as FrameworkElement;
            if(fElement != null)
                fElement.DataContextChanged += OnDataContextChanged;
#endif
            OnAttached();
        }
        public void Detach() {
            OnDetaching();
#if NETFX_CORE
            DataContextRegistrator.Unregister(AssociatedObject, this);
            var fElement = AssociatedObject as FrameworkElement;
            if(fElement != null)
                fElement.DataContextChanged -= OnDataContextChanged;
#endif
            AssociatedObject = null;
            IsAttached = false;
        }
#if !NETFX_CORE
        protected override bool FreezeCore(bool isChecking) {
            return false;
        }
#else
        void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
            OnDataContextChange(args.NewValue);
        }
        protected virtual void OnDataContextChange(object dataContext) { }
#endif
        protected virtual void OnAttached() { }
        protected virtual void OnDetaching() { }

        protected void VerifyRead() {
#if !NETFX_CORE
            ReadPreamble();
#endif
        }
        protected void VerifyWrite() {
#if !NETFX_CORE
            WritePreamble();
#endif
        }
        protected void NotifyChanged() {
#if !NETFX_CORE
            WritePostscript();
#endif
        }

#if !NETFX_CORE
        protected override Freezable CreateInstanceCore() {
            return (Freezable)Activator.CreateInstance(GetType());
        }
#endif
    }
}
