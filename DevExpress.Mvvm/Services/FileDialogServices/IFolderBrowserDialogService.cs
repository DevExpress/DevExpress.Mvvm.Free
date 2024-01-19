
namespace DevExpress.Mvvm {
    public interface IFolderBrowserDialogService {
        string StartPath { get; set; }
        string ResultPath { get; }
        bool ShowDialog();
    }
}