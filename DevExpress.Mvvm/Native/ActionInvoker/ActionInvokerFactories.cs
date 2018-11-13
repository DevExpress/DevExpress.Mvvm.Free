using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevExpress.Mvvm.Native;
#if !NETFX_CORE
using System.Windows.Threading;
#endif

namespace DevExpress.Mvvm.Native {
    public class StrongReferenceActionInvokerFactory : IActionInvokerFactory {
        IActionInvoker IActionInvokerFactory.CreateActionInvoker<TMessage>(object recipient, Action<TMessage> action) {
            return new StrongReferenceActionInvoker<TMessage>(recipient, action);
        }
        //IActionInvoker IActionInvokerFactory.CreateActionInvoker(object recipient, Action action) {
        //    return new StrongReferenceActionInvoker(recipient, action);
        //}
    }
    public class WeakReferenceActionInvokerFactory : IActionInvokerFactory {
        IActionInvoker IActionInvokerFactory.CreateActionInvoker<TMessage>(object recipient, Action<TMessage> action) {
#if !NETFX_CORE
            if(action.Method.IsStatic)
#else
            if(action.GetMethodInfo().IsStatic)
#endif
                return new StrongReferenceActionInvoker<TMessage>(recipient, action);
            return new WeakReferenceActionInvoker<TMessage>(recipient, action);
        }
//        IActionInvoker IActionInvokerFactory.CreateActionInvoker(object recipient, Action action) {
//#if !NETFX_CORE
//            if(action.Method.IsStatic)
//#else
//            if(action.GetMethodInfo().IsStatic)
//#endif
//                return new StrongReferenceActionInvoker(recipient, action);
//            return new WeakReferenceActionInvoker(recipient, action);
//        }
    }
}