using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System.IO;

namespace Example.ViewModel {
    [POCOViewModel]
    public class SaveFileDialogViewModel {
        public virtual string DefaultExt { get; set; }
        public virtual string DefaultFileName { get; set; }
        public virtual string Filter { get; set; }
        public virtual int FilterIndex { get; set; }
#if !SILVERLIGHT
        public virtual string Title { get; set; }
        public virtual bool OverwritePrompt { get; set; }
#endif
        public virtual bool DialogResult { get; protected set; }
        public virtual string ResultFileName { get; protected set; }
        public virtual string FileBody { get; set; }
        protected virtual ISaveFileDialogService SaveFileDialogService { get { return null; } }

        public SaveFileDialogViewModel() {
            DefaultExt = "txt";
            DefaultFileName = "Document1";
            Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            FilterIndex = 1;
            FileBody = "Hello";
#if !SILVERLIGHT
            Title = "Test Dialog";
            OverwritePrompt = true;
#endif
        }
        public void Save() {
            SaveFileDialogService.DefaultExt = DefaultExt;
            SaveFileDialogService.DefaultFileName = DefaultFileName;
            SaveFileDialogService.Filter = Filter;
            SaveFileDialogService.FilterIndex = FilterIndex;
            DialogResult = SaveFileDialogService.ShowDialog();
            if(!DialogResult) {
                ResultFileName = string.Empty;
            } else {
                using(var stream = new StreamWriter(SaveFileDialogService.OpenFile())) {
                    stream.Write(FileBody);
                }
            }
        }
    }
}
