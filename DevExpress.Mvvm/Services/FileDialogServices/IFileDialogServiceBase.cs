using System.ComponentModel;

namespace DevExpress.Mvvm {
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public interface IFileDialogServiceBase {
        bool CheckFileExists { get; set; }
        bool AddExtension { get; set; }
        bool AutoUpgradeEnabled { get; set; }
        bool CheckPathExists { get; set; }
        bool DereferenceLinks { get; set; }
        string InitialDirectory { get; set; }
        bool RestoreDirectory { get; set; }
        bool ShowHelp { get; set; }
        bool SupportMultiDottedExtensions { get; set; }
        string Title { get; set; }
        bool ValidateNames { get; set; }

        void Reset();
    }
}