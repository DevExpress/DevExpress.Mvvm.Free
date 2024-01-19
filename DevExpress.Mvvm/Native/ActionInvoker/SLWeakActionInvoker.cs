using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.Native {
    public class SLWeakReferenceActionInvoker<T> : StrongReferenceActionInvoker<T> {
        public SLWeakReferenceActionInvoker(object target, Action<T> action)
            : base(target, action) {
        }
        protected override void Execute(object parameter) {
            if(ActionToInvoke != null)
                base.Execute(parameter);
        }
    }
    public class SLWeakReferenceActionInvoker : StrongReferenceActionInvoker {
        public SLWeakReferenceActionInvoker(object target, Action action)
            : base(target, action) {
        }
        protected override void Execute(object parameter) {
            if(ActionToInvoke != null)
                base.Execute(parameter);
        }
    }
}