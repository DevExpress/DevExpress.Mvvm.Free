using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.Native {
    public class SLWeakReferenceActionInvoker<T> : ActionInvokerBase<T> {
        Action<T> action;

        public SLWeakReferenceActionInvoker(object target, Action<T> action)
            : base(target) {
            this.action = action;
        }
        protected override string MethodName {
            get { return action.Method.Name; }
        }
        protected override void Execute(T parameter) {
            if(action != null) {
                action(parameter);
                return;
            }
        }
        protected override void ClearCore() {
            action = null;
        }
    }
}