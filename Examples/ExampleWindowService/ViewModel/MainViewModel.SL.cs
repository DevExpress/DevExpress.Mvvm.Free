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
    }
}
