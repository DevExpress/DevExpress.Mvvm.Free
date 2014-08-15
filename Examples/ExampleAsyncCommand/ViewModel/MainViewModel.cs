using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        public virtual int Progress { get; set; }
        protected virtual IMessageBoxService MessageBoxService { get { return null; } }
        protected virtual IDispatcherService DispatcherService { get { return null; } }
        public Task Calculate() {
            var command = this.GetAsyncCommand(x => x.Calculate());
            return Task.Factory.StartNew(CalculateCore, command.CancellationTokenSource.Token).
                ContinueWith(x => {
                    if(x.IsCanceled)
                        DispatcherService.BeginInvoke(() => MessageBoxService.Show("The calculation was canceled"));
                    else
                        DispatcherService.BeginInvoke(() => MessageBoxService.Show("The calculation succeeded"));
                });
        }
        void CalculateCore() {
            var command = this.GetAsyncCommand(x => x.Calculate());
            for(int i = 0; i <= 100; i++) {
                command.CancellationTokenSource.Token.ThrowIfCancellationRequested();
#if !SILVERLIGHT
                Progress = i;
#else
                DispatcherService.BeginInvoke(() => Progress = i);
#endif
                Thread.Sleep(TimeSpan.FromSeconds(0.02));
            }
        }
    }
}
