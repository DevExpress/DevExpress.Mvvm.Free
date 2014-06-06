using System;
using System.Threading.Tasks;

namespace DevExpress.Mvvm {
    public static class SplashScreenServiceExtensions {
#if !SILVERLIGHT
        public static void ShowSplashScreen(this ISplashScreenService service) {
            VerifyService(service);
            service.ShowSplashScreen(null);
        }
#else
        public static Task ShowSplashScreen(this ISplashScreenService service) {
            VerifyService(service);
            return service.ShowSplashScreen(null);
        }
#endif
        static void VerifyService(ISplashScreenService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}