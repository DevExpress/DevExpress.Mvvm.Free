using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace DevExpress.Mvvm {
    public interface IWindowService {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        void Show(string documentType, object viewModel, object parameter, object parentViewModel);
        string Title { get; set; }
#if !SILVERLIGHT
        void SetWindowState(WindowState state);
#endif
        bool IsWindowAlive { get; }
        void Activate();
        void Restore();
        void Hide();
        void Close();
    }
}