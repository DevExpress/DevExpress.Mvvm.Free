using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    public interface ISaveFileDialogService {
        string Filter { get; set; }
        int FilterIndex { get; set; }
        string DefaultExt { get; set; }
        string DefaultFileName { get; set; }
        bool ShowDialog(Action<CancelEventArgs> fileOK, string directoryName, string fileName);
        IFileInfo File { get; }
        string Title { get; set; }
    }
    public static class SaveFileDialogServiceExtensions {
        public static bool ShowDialog(this ISaveFileDialogService service, Action<CancelEventArgs> fileOK = null, IFileInfo fileInfo = null) {
            VerifyService(service);
            return service.ShowDialog(fileOK, fileInfo.With(x => x.DirectoryName), fileInfo.With(x => x.Name));
        }
        public static string GetFullFileName(this ISaveFileDialogService service) {
            VerifyService(service);
            if(service.File == null) return string.Empty;
            string directory = service.File.DirectoryName;
            if(!directory.EndsWith(@"\"))
                directory += @"\";
            return directory + service.File.Name;
        }
        public static string SafeFileName(this ISaveFileDialogService service) {
            VerifyService(service);
            return service.File.Return(x => x.Name, () => string.Empty);
        }
        public static Stream OpenFile(this ISaveFileDialogService service) {
            VerifyService(service);
            return service.File.Return(x => x.Open(FileMode.Create, FileAccess.Write), () => null);
        }
        static void VerifyService(ISaveFileDialogService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}