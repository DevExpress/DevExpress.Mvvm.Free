using System.Windows.Input;

namespace DevExpress.Mvvm.Native {
    public interface IDelegateCommand : ICommand {
        void RaiseCanExecuteChanged();
    }
    public interface IAsyncCommand : IDelegateCommand {
        bool IsExecuting { get; }
        bool ShouldCancel { get; }
        ICommand CancelCommand { get; }
    }
}