using System.Threading.Tasks;
namespace DevExpress.Mvvm {
    public interface ISplashScreenService {
#if !SILVERLIGHT
        void ShowSplashScreen(string documentType);
#else
        Task ShowSplashScreen(string documentType);
#endif
        void SetSplashScreenProgress(double progress, double maxProgress);
        void SetSplashScreenState(object state);
        void HideSplashScreen();
        bool IsSplashScreenActive { get; }
    }
}