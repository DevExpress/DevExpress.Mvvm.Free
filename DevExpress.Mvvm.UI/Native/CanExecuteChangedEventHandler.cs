using System;
using System.ComponentModel;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI.Native {
    public class CanExecuteChangedEventHandler<TOwner> : WeakEventHandler<TOwner, EventArgs, EventHandler> where TOwner : class {
        static Action<WeakEventHandler<TOwner, EventArgs, EventHandler>, object> action = (h, o) => ((ICommand)o).CanExecuteChanged -= h.Handler;
        static Func<WeakEventHandler<TOwner, EventArgs, EventHandler>, EventHandler> create = h => h.OnEvent;
        public CanExecuteChangedEventHandler(TOwner owner, Action<TOwner, object, EventArgs> onEventAction)
            : base(owner, onEventAction, action, create) {
        }
    }
}