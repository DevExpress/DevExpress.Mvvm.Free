using System.Collections.Generic;
using System.IO;

namespace DevExpress.Mvvm {
    public interface IOpenFileDialogService {
        string Filter { get; set; }
        int FilterIndex { get; set; }
        bool ShowDialog();
        IFileInfo File { get; }
        IEnumerable<IFileInfo> Files { get; }
    }
}