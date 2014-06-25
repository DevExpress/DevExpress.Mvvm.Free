using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Example.ViewModel {
    [POCOViewModel]
    public class LoginViewModel {
        protected LoginViewModel() { }
        public static LoginViewModel Create() {
            return ViewModelSource.Create(() => new LoginViewModel());
        }

        public virtual string UserName { get; set; }
        public virtual string Error { get; set; }
        protected virtual IMessageBoxService MessageBoxService { get { return null; } }

        protected void OnUserNameChanged() {
            this.RaiseCanExecuteChanged(x => x.Login());
        }
        public void Login() {
            allowValidate = true;
            Validate();
            if(!CanLogin()) {
                ShowErrorMessage();
                this.RaiseCanExecuteChanged(x => x.Login());
            } else ShowOK();
        }
        public bool CanLogin() {
            if(!allowValidate) return true;
            Validate();
            return string.IsNullOrEmpty(Error);
        }
        bool allowValidate = false;
        void Validate() {
            if(string.IsNullOrEmpty(UserName))
                Error = "UserName cannot be empty";
            else
                Error = null;
        }
        void ShowErrorMessage() {
            if(!string.IsNullOrEmpty(Error))
                MessageBoxService.Show(Error);
        }
        void ShowOK() {
            MessageBoxService.Show("OK");
        }
    }
}
