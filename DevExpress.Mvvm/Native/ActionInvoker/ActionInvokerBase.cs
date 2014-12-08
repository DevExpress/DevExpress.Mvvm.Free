using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevExpress.Mvvm.Native;
#if !NETFX_CORE
using System.Windows.Threading;
#endif

namespace DevExpress.Mvvm.Native {
    public abstract class ActionInvokerBase : IActionInvoker {
        WeakReference targetReference;

        public ActionInvokerBase(object target) {
            targetReference = target == null ? null : new WeakReference(target);
        }
        public object Target { get { return targetReference.With(x => x.Target); } }
        void IActionInvoker.ExecuteIfMatched(Type messageTargetType, object parameter) {
            object target = Target;
            if(target == null) return;
            if(messageTargetType == null || messageTargetType.IsAssignableFrom(target.GetType())) {
                Execute(parameter);
            }
        }
        void IActionInvoker.ClearIfMatched(Delegate action, object recipient) {
            object target = Target;
            if(recipient != target)
                return;
#if !NETFX_CORE
            if(action != null && action.Method.Name != MethodName)
#else
            if(action != null && action.GetMethodInfo().Name != MethodName)
#endif
                return;
            targetReference = null;
            ClearCore();
        }

        protected abstract string MethodName { get; }
        protected abstract void Execute(object parameter);
        protected abstract void ClearCore();
    }
}