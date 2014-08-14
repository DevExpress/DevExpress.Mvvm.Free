using System;
using System.Threading;
using System.Windows.Input;

namespace DevExpress.Mvvm.Native {
    public interface IDelegateCommand : ICommand {
        void RaiseCanExecuteChanged();
    }
    public interface IAsyncCommand : IDelegateCommand {
        bool IsExecuting { get; }
        [Obsolete("This property is obsolete. Use the IsCancellationRequested property instead.")]
        bool ShouldCancel { get; }
        CancellationTokenSource CancellationTokenSource { get; }
        bool IsCancellationRequested { get; }
        ICommand CancelCommand { get; }
    }
}