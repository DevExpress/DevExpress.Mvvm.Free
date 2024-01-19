using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    public interface IOpenDialogServiceBase : IFileDialogServiceBase {
        bool Multiselect { get; set; }

        bool ShowDialog(Action<CancelEventArgs> fileOK, string directoryName);
    }

    public interface IOpenFolderDialogService : IOpenDialogServiceBase {
        IFolderInfo Folder { get; }
        IEnumerable<IFolderInfo> Folders { get; }
    }
    public interface IOpenFileDialogService : IOpenDialogServiceBase {
        IFileInfo File { get; }
        IEnumerable<IFileInfo> Files { get; }
    }
    public static class OpenFileDialogServiceExtensions {
        public static bool ShowDialog(this IOpenDialogServiceBase service) {
            VerifyService(service);
            return service.ShowDialog(null, null);
        }
        public static bool ShowDialog(this IOpenDialogServiceBase service, Action<CancelEventArgs> fileOK) {
            VerifyService(service);
            return service.ShowDialog(fileOK, null);
        }
        public static bool ShowDialog(this IOpenDialogServiceBase service, string directoryName) {
            VerifyService(service);
            return service.ShowDialog(null, directoryName);
        }
        public static string GetFullFileName(this IOpenFileDialogService service) {
            VerifyService(service);
            return service.File.Return(x => x.GetFullName(), () => string.Empty);
        }
        static void VerifyService(IOpenDialogServiceBase service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}