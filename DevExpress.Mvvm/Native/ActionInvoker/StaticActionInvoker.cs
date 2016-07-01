using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevExpress.Mvvm.Native;
using System.Windows.Threading;

namespace DevExpress.Mvvm.Native {
    public abstract class StrongReferenceActionInvokerBase : ActionInvokerBase {
        public StrongReferenceActionInvokerBase(object target, Delegate action)
            : base(target) {
            ActionToInvoke = action;
        }
        protected Delegate ActionToInvoke { get; private set; }
        protected override string MethodName { get { return ActionToInvoke.Method.Name; } }
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