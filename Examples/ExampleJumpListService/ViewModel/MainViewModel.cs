using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Diagnostics;
using System.Windows.Shell;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        protected virtual IApplicationJumpListService ApplicationJumpListService { get { return null; } }
        public void RunInternetExplorer() {
            Process.Start(@"C:\Program Files\Internet Explorer\iexplore.exe");
        }
    }
}
