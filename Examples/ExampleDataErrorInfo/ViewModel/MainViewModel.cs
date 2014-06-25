using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System.Windows.Media;

namespace Example.ViewModel {
    [POCOViewModel(ImplementIDataErrorInfo = true)]
    public class MainViewModel : ViewModelBase {
        static PropertyMetadataBuilder<MainViewModel, string> AddPasswordCheck(PropertyMetadataBuilder<MainViewModel, string> builder) {
            return builder.MatchesInstanceRule(vm => vm.Password == vm.ConfirmPassword, () => "The passwords don't match.")
                    .MinLength(8, () => "The password must be at least 8 characters long.")
                    .MaxLength(20, () => "The password must not exceed the length of 20.");
        }
        public static void BuildMetadata(MetadataBuilder<MainViewModel> builder) {
            builder.Property(x => x.FirstName)
                .Required(() => "Please enter the first name.");
            builder.Property(x => x.LastName)
                .Required(() => "Please enter the last name.");
            builder.Property(x => x.Email)
                .EmailAddressDataType(() => "Please enter a correct email address.");
            AddPasswordCheck(builder.Property(x => x.Password))
                .Required(() => "Please enter the password.");
            AddPasswordCheck(builder.Property(x => x.ConfirmPassword))
                .Required(() => "Please confirm the password.");
        }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Email { get; set; }
        public virtual string Password { get; set; }
        public virtual string ConfirmPassword { get; set; }
        public void OnPasswordChanged() {
            this.RaisePropertyChanged(() => ConfirmPassword);
        }
        public void OnConfirmPasswordChanged() {
            this.RaisePropertyChanged(() => Password);
        }
    }
}
