using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.Native {
    public class StrongReferenceActionInvoker<T> : ActionInvokerBase<T> {
        Action<T> staticAction;

        public StrongReferenceActionInvoker(object target, Action<T> action)
            : base(target) {
            staticAction = action;
        }
        protected override string MethodName { get { return staticAction.Method.Name; } }
        protected override void Execute(T parameter) {
            staticAction(parameter);
        }
        protected override void ClearCore() {
            staticAction = null;
        }
    }
}