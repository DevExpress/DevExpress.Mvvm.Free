using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DevExpress.Mvvm {
    public interface IDelegateCommand : ICommand {
        void RaiseCanExecuteChanged();
    }
    public interface IAsyncCommand : IDelegateCommand {
        bool IsExecuting { get; }
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use the IsCancellationRequested property instead.")]
        bool ShouldCancel { get; }
        CancellationTokenSource CancellationTokenSource { get; }
        bool IsCancellationRequested { get; }
        ICommand CancelCommand { get; }
#if DEBUG
        [Obsolete("Use 'await ExecuteAsync' instead.")]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        void Wait(TimeSpan timeout);
        Task ExecuteAsync(object parameter);
    }
}
namespace DevExpress.Mvvm {
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IAsyncCommandExtensions {
#if DEBUG
        [Obsolete("Use 'await ExecuteAsync' instead.")]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Wait(this IAsyncCommand service) {
            VerifyService(service);
            service.Wait(TimeSpan.FromMilliseconds(-1));
        }
        static void VerifyService(IAsyncCommand service) {
            if(service == null) throw new ArgumentNullException("service");
        }
    }
}