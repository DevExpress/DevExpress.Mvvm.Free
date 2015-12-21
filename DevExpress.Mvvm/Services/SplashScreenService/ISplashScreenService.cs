using System.Threading.Tasks;
namespace DevExpress.Mvvm {
    public interface ISplashScreenService {
        void ShowSplashScreen(string documentType);
        void SetSplashScreenProgress(double progress, double maxProgress);
        void SetSplashScreenState(object state);
        void HideSplashScreen();
        bool IsSplashScreenActive { get; }
    }
}