using DevExpress.Mvvm;
using System.Windows;

namespace Example.ViewModel {
    public class MainViewModel : ViewModelBase {
        public DelegateCommand TestCommand { get; private set; }
        public bool IsTestCommandEnabled {
            get { return GetProperty(() => IsTestCommandEnabled); }
            set { SetProperty(() => IsTestCommandEnabled, value); }
        }
        void Test() {
            MessageBox.Show("Hello");
        }
        bool CanTest() {
            return IsTestCommandEnabled;
        }

        public DelegateCommand UpdateTestCommand { get; private set; }
        void UpdateTest() {
            TestCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand<string> CommandWithParameter { get; private set; }
        void CommandWithParameterExecute(string parameter) {
            MessageBox.Show(parameter);
        }
        bool CommandWithParameterCanExecute(string parameter) {
            return !string.IsNullOrEmpty(parameter);
        }

        public MainViewModel() {
            TestCommand = new DelegateCommand(Test, CanTest);
            UpdateTestCommand = new DelegateCommand(UpdateTest);
            CommandWithParameter = new DelegateCommand<string>(CommandWithParameterExecute, CommandWithParameterCanExecute);
        }
    }
}