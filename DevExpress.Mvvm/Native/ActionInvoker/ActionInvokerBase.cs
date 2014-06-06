using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.Native {
    public abstract class ActionInvokerBase<T> : IActionInvoker {
        WeakReference targetReference;

        public ActionInvokerBase(object target) {
            targetReference = target == null ? null : new WeakReference(target);
        }
        public object Target { get { return targetReference.With(x => x.Target); } }
        void IActionInvoker.ExecuteIfMatched(Type messageTargetType, object parameter) {
            object target = Target;
            if(target == null) return;
            if(messageTargetType == null || messageTargetType.IsAssignableFrom(target.GetType())) {
                Execute((T)parameter);
            }
        }
        void IActionInvoker.ClearIfMatched(Delegate action, object recipient) {
            object target = Target;
            if(recipient != target)
                return;
            if(action != null && action.Method.Name != MethodName)
                return;
            targetReference = null;
            ClearCore();
        }

        protected abstract string MethodName { get; }
        protected abstract void Execute(T parameter);
        protected abstract void ClearCore();
    }
}