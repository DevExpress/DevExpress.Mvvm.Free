using DevExpress.Mvvm;
using System.Windows;

namespace Example.ViewModel {
    public class MainViewModel : ViewModelBase {
        public DelegateCommand AutoUpdateCommand { get; private set; }
        public bool IsAutoUpdateCommandEnabled {
            get { return GetProperty(() => IsAutoUpdateCommandEnabled); }
            set { SetProperty(() => IsAutoUpdateCommandEnabled, value); }
        }
        void AutoUpdateCommandExecute() {
            MessageBox.Show("Hello");
        }
        bool AutoUpdateCommandCanExecute() {
            return IsAutoUpdateCommandEnabled;
        }

        public DelegateCommand ManualUpdateCommand { get; private set; }
        public bool IsManualUpdateCommandEnabled {
            get { return GetProperty(() => IsManualUpdateCommandEnabled); }
            set { SetProperty(() => IsManualUpdateCommandEnabled, value); }
        }
        void ManualUpdateCommandExecute() {
            MessageBox.Show("Hello");
        }
        bool ManualUpdateCommandCanExecute() {
            return IsManualUpdateCommandEnabled;
        }
        public DelegateCommand ForceUpdateManualUpdateCommand { get; private set; }
        void ForceUpdateManualUpdateCommandExecute() {
            ManualUpdateCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand<string> CommandWithParameter { get; private set; }
        void CommandWithParameterExecute(string parameter) {
            MessageBox.Show(parameter);
        }
        bool CommandWithParameterCanExecute(string parameter) {
            return !string.IsNullOrEmpty(parameter);
        }

        public MainViewModel() {
            AutoUpdateCommand = new DelegateCommand(AutoUpdateCommandExecute, AutoUpdateCommandCanExecute);
            ManualUpdateCommand = new DelegateCommand(ManualUpdateCommandExecute, ManualUpdateCommandCanExecute, false);
            ForceUpdateManualUpdateCommand = new DelegateCommand(ForceUpdateManualUpdateCommandExecute);
            CommandWithParameter = new DelegateCommand<string>(CommandWithParameterExecute, CommandWithParameterCanExecute);
        }
    }
}
