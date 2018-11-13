
#if !FREE
namespace DevExpress.Xpf.Core {
#else
namespace DevExpress.Mvvm.UI {
#endif
    public interface ISplashScreen {
        void Progress(double value);
        void SetProgressState(bool isIndeterminate);
        void CloseSplashScreen();
    }
}
