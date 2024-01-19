using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.UI.Native {
    public class ApplicationJumpItemCollectionInternal : ApplicationJumpItemCollection {
        public ApplicationJumpItemCollectionInternal(IApplicationJumpListImplementation service) : base(service) { }
    }
}