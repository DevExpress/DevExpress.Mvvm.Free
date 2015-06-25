using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace DevExpress.Mvvm {
    public interface IOpenFileDialogService {
        string Filter { get; set; }
        int FilterIndex { get; set; }
        bool ShowDialog(Action<CancelEventArgs> fileOK, string directoryName);
        IFileInfo File { get; }
        IEnumerable<IFileInfo> Files { get; }
#if !SILVERLIGHT
        string Title { get; set; }
#endif
    }
    public static class OpenFileDialogServiceExtensions {
        public static bool ShowDialog(this IOpenFileDialogService service) {
            VerifyService(service);
            return service.ShowDialog(null, null);
        }
        public static bool ShowDialog(this IOpenFileDialogService service, Action<CancelEventArgs> fileOK) {
            VerifyService(service);
            return service.ShowDialog(fileOK, null);
        }
        public static bool ShowDialog(this IOpenFileDialogService service, string directoryName) {
            VerifyService(service);
            return service.ShowDialog(null, directoryName);
        }

        public static string GetFullFileName(this IOpenFileDialogService service) {
            VerifyService(service);
            if(service.File == null) return string.Empty;
            string directory = service.File.DirectoryName;
            if(!directory.EndsWith(@"\"))
                directory += @"\";
            return directory + service.File.Name;
        }
        static void VerifyService(IOpenFileDialogService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}