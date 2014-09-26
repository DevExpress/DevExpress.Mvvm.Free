using System;
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
#if !SILVERLIGHT
    public static class OpenFileDialogServiceExtensions {
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
#endif
}