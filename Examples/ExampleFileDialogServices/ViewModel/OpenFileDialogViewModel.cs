using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System.Linq;

namespace Example.ViewModel {
    [POCOViewModel]
    public class OpenFileDialogViewModel {
        public virtual string Filter { get; set; }
        public virtual int FilterIndex { get; set; }
        public virtual string Title { get; set; }
        public virtual bool DialogResult { get; protected set; }
        public virtual string ResultFileName { get; protected set; }
        public virtual string FileBody { get; set; }
        protected virtual IOpenFileDialogService OpenFileDialogService { get { return null; } }

        public OpenFileDialogViewModel() {
            Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            FilterIndex = 1;
            Title = "Test Dialog";
        }
        public void Open() {
            OpenFileDialogService.Filter = Filter;
            OpenFileDialogService.FilterIndex = FilterIndex;
            DialogResult = OpenFileDialogService.ShowDialog();
            if(!DialogResult) {
                ResultFileName = string.Empty;
            } else {
                IFileInfo file = OpenFileDialogService.Files.First();
                ResultFileName = file.Name;
                using(var stream = file.OpenText()) {
                    FileBody = stream.ReadToEnd();
                }
            }
        }
    }
}
