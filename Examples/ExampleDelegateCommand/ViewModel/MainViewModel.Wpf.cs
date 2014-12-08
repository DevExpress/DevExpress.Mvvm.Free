using DevExpress.Mvvm;
using System.Windows;

namespace Example.ViewModel {
    public class MainViewModel : ViewModelBase {
        #region AutoUpdateCommand
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
        #endregion
        #region ManualUpdateCommand
        public DelegateCommand ManualUpdateCommand { get; private set; }
        public bool IsManualUpdateCommandEnabled {
            get { return GetProperty(() => IsManualUpdateCommandEnabled); }
            set { SetProperty(() => IsManualUpdateCommandEnabled, value, IsManualUpdateCommandEnabledChanged); }
        }
        void IsManualUpdateCommandEnabledChanged() {
            ManualUpdateCommand.RaiseCanExecuteChanged();
        }
        void ManualUpdateCommandExecute() {
            MessageBox.Show("Hello");
        }
        bool ManualUpdateCommandCanExecute() {
            return IsManualUpdateCommandEnabled;
        }
        #endregion
        #region CommandWithParameter
        public DelegateCommand<string> CommandWithParameter { get; private set; }
        void CommandWithParameterExecute(string parameter) {
            MessageBox.Show(parameter);
        }
        bool CommandWithParameterCanExecute(string parameter) {
            return !string.IsNullOrEmpty(parameter);
        }
        #endregion
        public MainViewModel() {
            AutoUpdateCommand = new DelegateCommand(AutoUpdateCommandExecute, AutoUpdateCommandCanExecute);
            ManualUpdateCommand = new DelegateCommand(ManualUpdateCommandExecute, ManualUpdateCommandCanExecute, false);
            CommandWithParameter = new DelegateCommand<string>(CommandWithParameterExecute, CommandWithParameterCanExecute);
        }
    }
}
