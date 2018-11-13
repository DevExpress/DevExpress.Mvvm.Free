using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using DevExpress.Mvvm.Native;
using System.Windows;
using System.ComponentModel;

namespace DevExpress.Mvvm.UI.Native {
    public abstract class DependencyObjectExt : Freezable {
        protected override sealed Freezable CreateInstanceCore() { return this; }
        protected override sealed void CloneCore(Freezable sourceFreezable) { base.CloneCore(sourceFreezable); }
        protected override sealed void CloneCurrentValueCore(Freezable sourceFreezable) { base.CloneCurrentValueCore(sourceFreezable); }
        protected override sealed bool FreezeCore(bool isChecking) { return base.FreezeCore(isChecking); }
        protected override sealed void GetAsFrozenCore(Freezable sourceFreezable) { base.GetAsFrozenCore(sourceFreezable); }
        protected override sealed void GetCurrentValueAsFrozenCore(Freezable sourceFreezable) { base.GetCurrentValueAsFrozenCore(sourceFreezable); }
    }
}