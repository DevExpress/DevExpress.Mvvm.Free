using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        public virtual OpenFileDialogViewModel OpenFileDialogViewModel { get; protected set; }
        public virtual SaveFileDialogViewModel SaveFileDialogViewModel { get; protected set; }
#if !SILVERLIGHT
        public virtual FolderBrowserDialogViewModel FolderBrowserDialogViewModel { get; protected set; }
#endif
        public MainViewModel() {
            OpenFileDialogViewModel = ViewModelSource.Create(() => new OpenFileDialogViewModel());
            SaveFileDialogViewModel = ViewModelSource.Create(() => new SaveFileDialogViewModel());
#if !SILVERLIGHT
            FolderBrowserDialogViewModel = ViewModelSource.Create(() => new FolderBrowserDialogViewModel());
#endif
        }
    }
}
