using DevExpress.Mvvm;

namespace DevExpress.Mvvm.UI {
    public class SplashScreenViewModel : ViewModelBase, ISupportSplashScreen {
        static SplashScreenViewModel designTimeData = null;
        public static SplashScreenViewModel DesignTimeData {
            get {
                if(designTimeData == null) {
                    designTimeData = new SplashScreenViewModel();
                    designTimeData.InitializeInDesignModeCore();
                }
                return designTimeData;
            }
        }

        public bool IsIndeterminate {
            get { return isIndeterminate; }
            set { SetProperty(ref isIndeterminate, value, () => IsIndeterminate); }
        }
        public double MaxProgress {
            get { return maxProgress; }
            set { SetProperty(ref maxProgress, value, () => MaxProgress); }
        }
        public double Progress {
            get { return progress; }
            set { SetProperty(ref progress, value, () => Progress); }
        }
        public object State {
            get { return state; }
            set { SetProperty(ref state, value, () => State); }
        }
        protected override void OnInitializeInDesignMode() {
            base.OnInitializeInDesignMode();
            InitializeInDesignModeCore();
        }
        void InitializeInDesignModeCore() {
            IsIndeterminate = false;
            MaxProgress = 100;
            Progress = 50;
            State = "State";
        }

        bool isIndeterminate = true;
        object state = null;
        double progress = 0;
        double maxProgress = 100;
    }
}