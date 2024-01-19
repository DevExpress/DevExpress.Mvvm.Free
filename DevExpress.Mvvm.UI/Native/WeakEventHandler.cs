using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
namespace DevExpress.Mvvm.UI.Native {
    #region WeakEventHandler
    public interface IWeakEventHandler<THandler> {
        THandler Handler { get; }
    }
    public class WeakEventHandler<TOwner, TEventArgs, THandler> : IWeakEventHandler<THandler> where TOwner : class {
        WeakReference ownerReference;
        Action<WeakEventHandler<TOwner, TEventArgs, THandler>, object> onDetachAction;
        Action<TOwner, object, TEventArgs> onEventAction;
        public THandler Handler { get; private set; }
        public WeakEventHandler(TOwner owner, Action<TOwner, object, TEventArgs> onEventAction, Action<WeakEventHandler<TOwner, TEventArgs, THandler>, object> onDetachAction, Func<WeakEventHandler<TOwner, TEventArgs, THandler>, THandler> createHandlerFunction) {
            this.ownerReference = new WeakReference(owner);
            this.onEventAction = onEventAction;
            this.onDetachAction = onDetachAction;
            this.Handler = createHandlerFunction(this);
        }
        public void OnEvent(object source, TEventArgs eventArgs) {
            TOwner target = ownerReference.Target as TOwner;
            if(target != null) {
                onEventAction(target, source, eventArgs);
            } else {
                onDetachAction(this, source);
            }
        }
    }
    public class PropertyChangedWeakEventHandler<TOwner> : WeakEventHandler<TOwner, PropertyChangedEventArgs, PropertyChangedEventHandler> where TOwner : class {
        static Action<WeakEventHandler<TOwner, PropertyChangedEventArgs, PropertyChangedEventHandler>, object> action = (h, o) => ((INotifyPropertyChanged)o).PropertyChanged -= h.Handler;
        static Func<WeakEventHandler<TOwner, PropertyChangedEventArgs, PropertyChangedEventHandler>, PropertyChangedEventHandler> create = h => new PropertyChangedEventHandler(h.OnEvent);
        public PropertyChangedWeakEventHandler(TOwner owner, Action<TOwner, object, PropertyChangedEventArgs> onEventAction)
            : base(owner, onEventAction, action, create) {
        }
    }
    #endregion
}