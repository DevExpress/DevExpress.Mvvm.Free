using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        public virtual int Progress { get; protected set; }
        public virtual Brush Color { get; protected set; }
        public virtual bool IsProgress { get; protected set; }
        protected virtual IDispatcherService DispatcherService { get { return null; } }
        public void Calculate() {
            IsProgress = true;
            Task.Factory.StartNew(CalcCore).ContinueWith(x => {
                DispatcherService.BeginInvoke(new Action(() => IsProgress = false));
            });
            this.RaiseCanExecuteChanged(x => x.Calculate());
        }
        public bool CanCalculate() {
            return !IsProgress;
        }
        void CalcCore() {
            for(int i = 1; i <= 100; i++) {
                DispatcherService.BeginInvoke(() => {
                    Progress = i;
                    Color = new SolidColorBrush(GetColor(i));
                });
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
        }
        Color GetColor(int coef) {
            return new Color() {
                R = (byte)(coef * 15),
                G = (byte)(coef * 7),
                B = (byte)(coef * 8),
                A = 255,
            };
        }
    }
}
