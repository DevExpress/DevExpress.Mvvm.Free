using DevExpress.Mvvm;

namespace Example.ViewModel {
    public class MainViewModel : ViewModelBase {
        public string FirstName {
            get { return GetProperty(() => FirstName); }
            set { SetProperty(() => FirstName, value, UpdateFullName); }
        }
        public string LastName {
            get { return GetProperty(() => LastName); }
            set { SetProperty(() => LastName, value, UpdateFullName); }
        }
        public string FullName {
            get { return FirstName + " " + LastName; }
        }
        void UpdateFullName() {
            RaisePropertyChanged(() => FullName);
        }
        protected override void OnInitializeInDesignMode() {
            base.OnInitializeInDesignMode();
            FirstName = "FirstName in DesignMode";
            LastName = "LastName in DesignMode";
        }
        protected override void OnInitializeInRuntime() {
            base.OnInitializeInRuntime();
            FirstName = "FirstName";
            LastName = "LastName";
        }

        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        public DelegateCommand<string> ShowMessageCommand { get; private set; }
        void ShowMessage(string message) {
            MessageBoxService.Show(message);
        }
        bool CanShowMessage(string message) {
            return !string.IsNullOrEmpty(message);
        }
        public MainViewModel() {
            ShowMessageCommand = new DelegateCommand<string>(ShowMessage, CanShowMessage);
        }
    }
}
