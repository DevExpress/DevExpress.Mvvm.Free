using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevExpress.Mvvm.Native;
#if !NETFX_CORE
using System.Windows.Threading;
#endif

namespace DevExpress.Mvvm.Native {
    public abstract class StrongReferenceActionInvokerBase : ActionInvokerBase {
        public StrongReferenceActionInvokerBase(object target, Delegate action)
            : base(target) {
            ActionToInvoke = action;
        }
        protected Delegate ActionToInvoke { get; private set; }
#if !NETFX_CORE
        protected override string MethodName { get { return ActionToInvoke.Method.Name; } }
#else
        protected override string MethodName { get { return ActionToInvoke.GetMethodInfo().Name; } }
#endif
        protected override void ClearCore() {
            ActionToInvoke = null;
        }
    }
    public class StrongReferenceActionInvoker<T> : StrongReferenceActionInvokerBase {
        public StrongReferenceActionInvoker(object target, Action<T> action)
            : base(target, action) {
        }
        protected override void Execute(object parameter) {
            ((Action<T>)ActionToInvoke)((T)parameter);
        }
    }
    public class StrongReferenceActionInvoker : StrongReferenceActionInvokerBase {
        public StrongReferenceActionInvoker(object target, Action action)
            : base(target, action) {
        }
        protected override void Execute(object parameter) {
            ((Action)ActionToInvoke)();
        }
    }
}