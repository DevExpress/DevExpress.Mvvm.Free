using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevExpress.Mvvm.Native;
#if !NETFX_CORE
using System.Windows.Threading;
#endif

namespace DevExpress.Mvvm.Native {
    public abstract class WeakReferenceActionInvokerBase : ActionInvokerBase {
        public WeakReferenceActionInvokerBase(object target, Delegate action)
            : base(target) {
#if !NETFX_CORE
                ActionMethod = action.Method;
#else
                ActionMethod = action.GetMethodInfo();
#endif
                ActionTargetReference = new WeakReference(action.Target);
        }
        protected MethodInfo ActionMethod { get; private set; }
        protected WeakReference ActionTargetReference { get; private set; }
        protected override string MethodName { get { return ActionMethod.Name; } }
        protected override void ClearCore() {
            ActionTargetReference = null;
            ActionMethod = null;
        }
    }

    public class WeakReferenceActionInvoker<T> : WeakReferenceActionInvokerBase {
        public WeakReferenceActionInvoker(object target, Action<T> action)
            : base(target, action) {
        }
        protected override void Execute(object parameter) {
            if(ActionMethod != null && ActionTargetReference.IsAlive) {
                ActionMethod.Invoke(ActionTargetReference.Target, new object[] { (T)parameter });
            }
        }
    }
    public class WeakReferenceActionInvoker : WeakReferenceActionInvokerBase {
        public WeakReferenceActionInvoker(object target, Action action)
            : base(target, action) {
        }
        protected override void Execute(object parameter) {
            if(ActionMethod != null && ActionTargetReference.IsAlive) {
                ActionMethod.Invoke(ActionTargetReference.Target, null);
            }
        }
    }
}
