using System;
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
#if !SILVERLIGHT
    public static class SaveFileDialogServiceExtensions {
        public static string GetFullFileName(this ISaveFileDialogService service) {
            VerifyService(service);
            if(service.File == null) return string.Empty;
            string directory = service.File.DirectoryName;
            if(!directory.EndsWith(@"\"))
                directory += @"\";
            return directory + service.File.Name;
        }
        static void VerifyService(ISaveFileDialogService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
#endif
}