using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Threading;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        public void Calculate() {
#if SILVERLIGHT
            SplashScreenService.ShowSplashScreen().ContinueWith(x => {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                SplashScreenService.HideSplashScreen();
            });
#else
            SplashScreenService.ShowSplashScreen();
            Thread.Sleep(TimeSpan.FromSeconds(5));
            SplashScreenService.HideSplashScreen();
#endif
        }
        protected virtual ISplashScreenService SplashScreenService { get { return null; } }
    }
}
