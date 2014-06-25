using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        protected virtual IWindowService WindowService { get { return null; } }

        public void ShowWindow() {
            WindowService.Show("TestView", ViewModelSource.Create(() => new TestViewModel()));
        }
        public bool CanShowWindow() {
            return !WindowService.IsWindowAlive;
        }
        [Command(CanExecuteMethodName = "CanClose")]
        public void HideWindow() {
            WindowService.Hide();
        }
        [Command(CanExecuteMethodName = "CanClose")]
        public void RestoreWindow() {
            WindowService.Restore();
        }
        [Command(CanExecuteMethodName = "CanClose")]
        public void CloseWindow() {
            WindowService.Close();
        }
        public bool CanClose() {
            return !CanShowWindow();
        }
    }
}
