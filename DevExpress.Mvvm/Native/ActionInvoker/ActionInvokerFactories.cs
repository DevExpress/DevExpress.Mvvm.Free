using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.Native {
    public class StrongReferenceActionInvokerFactory : IActionInvokerFactory {
        IActionInvoker IActionInvokerFactory.CreateActionInvoker<TMessage>(object recipient, Action<TMessage> action) {
            return new StrongReferenceActionInvoker<TMessage>(recipient, action);
        }
    }
    public class WeakReferenceActionInvokerFactory : IActionInvokerFactory {
        IActionInvoker IActionInvokerFactory.CreateActionInvoker<TMessage>(object recipient, Action<TMessage> action) {
            if(action.Method.IsStatic)
                return new StrongReferenceActionInvoker<TMessage>(recipient, action);
#if SILVERLIGHT
            if(ShouldStoreActionItself(action))
                return new SLWeakReferenceActionInvoker<TMessage>(recipient, action);
#endif
            return new WeakReferenceActionInvoker<TMessage>(recipient, action);
        }
#if SILVERLIGHT
        static bool ShouldStoreActionItself<TMessage>(Action<TMessage> action) {
            if(!action.Method.IsPublic)
                return true;
            if(action.Target != null && !action.Target.GetType().IsPublic && !action.Target.GetType().IsNestedPublic)
                return true;
            var name = action.Method.Name;
            if(name.Contains("<") && name.Contains(">"))
                return true;
            return false;
        }
#endif
    }
}