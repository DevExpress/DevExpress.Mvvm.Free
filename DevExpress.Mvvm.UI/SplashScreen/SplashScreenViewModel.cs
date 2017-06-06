using DevExpress.Mvvm;

namespace DevExpress.Mvvm.UI {
    public class SplashScreenViewModel : ViewModelBase, ISupportSplashScreen {
        public const double ProgressDefaultValue = 0d;
        public const double MaxProgressDefaultValue = 100d;
        public const string StateDefaultValue = "Loading...";

        static SplashScreenViewModel designTimeData = null;
        public static SplashScreenViewModel DesignTimeData {
            get { return designTimeData ?? (designTimeData = new SplashScreenViewModel()); }
        }

        public bool IsIndeterminate {
            get { return GetProperty(() => IsIndeterminate); }
            set { SetProperty(() => IsIndeterminate, value); }
        }
        public double MaxProgress {
            get { return GetProperty(() => MaxProgress); }
            set { SetProperty(() => MaxProgress, value, DisableMarquee); }
        }
        public double Progress {
            get { return GetProperty(() => Progress); }
            set { SetProperty(() => Progress, value, DisableMarquee); }
        }
        public object State {
            get { return GetProperty(() => State); }
            set { SetProperty(() => State, value); }
        }
        public SplashScreenViewModel() {
            allowDisableMarquee = false;
            IsIndeterminate = true;
            MaxProgress = MaxProgressDefaultValue;
            Progress = ProgressDefaultValue;
            State = StateDefaultValue;
            allowDisableMarquee = true;
        }
        void DisableMarquee() {
            if(!allowDisableMarquee)
                return;
            IsIndeterminate = false;
        }
        bool allowDisableMarquee = false;

        public SplashScreenViewModel Clone() {
            var result = new SplashScreenViewModel();
            result.allowDisableMarquee = false;
            result.MaxProgress = MaxProgress;
            result.Progress = Progress;
            result.State = State;
            result.IsIndeterminate = IsIndeterminate;
            result.allowDisableMarquee = true;
            return result;
        }
    }
}