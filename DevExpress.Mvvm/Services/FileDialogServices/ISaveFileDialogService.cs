using System.Collections.Generic;
using System.IO;

namespace DevExpress.Mvvm {
    public interface ISaveFileDialogService {
        string Filter { get; set; }
        int FilterIndex { get; set; }
        string DefaultExt { get; set; }
        string DefaultFileName { get; set; }
        bool ShowDialog();
        string SafeFileName { get; }
        Stream OpenFile();
#if !SILVERLIGHT
        IFileInfo File { get; }
#endif
    }
}