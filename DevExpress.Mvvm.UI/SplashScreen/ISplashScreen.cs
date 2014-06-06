
namespace DevExpress.Mvvm.UI {
    public interface ISplashScreen {
        void Progress(double value);
        void SetProgressState(bool isIndeterminate);
#if !SILVERLIGHT
        void CloseSplashScreen();
#endif
    }
}