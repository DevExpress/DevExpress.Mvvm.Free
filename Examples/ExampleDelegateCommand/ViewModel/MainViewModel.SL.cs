using DevExpress.Mvvm;
using System.Windows;

namespace Example.ViewModel {
    public class MainViewModel : ViewModelBase {
        #region TestCommand
        public DelegateCommand TestCommand { get; private set; }
        public bool IsTestCommandEnabled {
            get { return GetProperty(() => IsTestCommandEnabled); }
            set { SetProperty(() => IsTestCommandEnabled, value, IsTestCommandEnabledChanged); }
        }
        void IsTestCommandEnabledChanged() {
            TestCommand.RaiseCanExecuteChanged();
        }
        void Test() {
            MessageBox.Show("Hello");
        }
        bool CanTest() {
            return IsTestCommandEnabled;
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
            TestCommand = new DelegateCommand(Test, CanTest);
            CommandWithParameter = new DelegateCommand<string>(CommandWithParameterExecute, CommandWithParameterCanExecute);
        }
    }
}
