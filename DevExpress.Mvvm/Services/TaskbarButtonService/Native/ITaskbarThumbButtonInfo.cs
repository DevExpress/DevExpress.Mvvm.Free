using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.Native {
    public interface ITaskbarThumbButtonInfo {
        EventHandler Click { get; set; }
    }
}