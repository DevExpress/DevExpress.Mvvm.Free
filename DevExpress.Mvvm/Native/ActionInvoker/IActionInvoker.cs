using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.Native {
    public interface IActionInvoker {
        object Target { get; }
        void ExecuteIfMatched(Type messageTargetType, object parameter);
        void ClearIfMatched(Delegate action, object recipient);
    }
    public interface IActionInvokerFactory {
        IActionInvoker CreateActionInvoker<TMessage>(object recipient, Action<TMessage> action);
    }
}