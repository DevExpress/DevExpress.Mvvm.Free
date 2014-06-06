using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.Native {
    public class WeakReferenceActionInvoker<T> : ActionInvokerBase<T> {
        MethodInfo actionMethod;
        WeakReference actionTargetReference;

        public WeakReferenceActionInvoker(object target, Action<T> action)
            : base(target) {
            actionMethod = action.Method;
            actionTargetReference = new WeakReference(action.Target);
        }
        object ActionTarget { get { return actionTargetReference.With(x => x.Target); } }
        protected override string MethodName {
            get { return actionMethod.Name; }
        }
        protected override void Execute(T parameter) {
            if(actionMethod != null && actionTargetReference != null && ActionTarget != null) {
                actionMethod.Invoke(ActionTarget, new object[] { parameter });
            }
        }
        protected override void ClearCore() {
            actionTargetReference = null;
            actionMethod = null;
        }
    }
}