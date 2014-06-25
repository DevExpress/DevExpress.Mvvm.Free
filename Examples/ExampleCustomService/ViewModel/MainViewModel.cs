using DevExpress.Mvvm;
using Example.Service;

namespace Example.ViewModel {
    public class MainViewModel : ViewModelBase {
        ICustomMessageBoxService CustomMessageBoxService { get { return GetService<ICustomMessageBoxService>(); } }
        public DelegateCommand ShowMessageCommand { get; private set; }
        void ShowMessage() {
            CustomMessageBoxService.Show();
        }
        public MainViewModel() {
            ShowMessageCommand = new DelegateCommand(ShowMessage);
        }
    }
}
