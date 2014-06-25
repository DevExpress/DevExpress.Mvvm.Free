using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        protected virtual IMessageBoxService MessageBoxService { get { return null; } }
        public void Save() {
            MessageBoxService.Show("OK!");
        }
    }
}
