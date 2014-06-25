using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        public virtual string SelectedText { get; set; }
    }
}
