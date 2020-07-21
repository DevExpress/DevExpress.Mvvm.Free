using System.ComponentModel;

namespace DevExpress.Mvvm {
    public interface IWindowService {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        void Show(string documentType, object viewModel, object parameter, object parentViewModel);
        string Title { get; set; }
        DXWindowState WindowState { get; set; }
        bool IsWindowAlive { get; }
        void Activate();
        void Restore();
        void Hide();
        void Close();
    }
}