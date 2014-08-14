using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        string SavedText;
        public virtual string Text { get; set; }
        public bool IsSaved {
            get {
                return Text == SavedText;
            }
        }
        public void Save() {
            SavedText = Text;
            this.RaisePropertyChanged(x => x.IsSaved);
        }
        public void Close() {
            SavedText = "";
            Text = "";
        }
        protected void OnTextChanged() {
            this.RaisePropertyChanged(x => x.IsSaved);
        }
    }
}
