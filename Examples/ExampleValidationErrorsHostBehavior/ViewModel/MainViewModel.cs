using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Example.ViewModel {
    [POCOViewModel(ImplementIDataErrorInfo=true)]
    public class MainViewModel {
        [Required]
        public virtual string FirstName { get; set; }
        [Required]
        public virtual string LastName { get; set; }
    }
}
