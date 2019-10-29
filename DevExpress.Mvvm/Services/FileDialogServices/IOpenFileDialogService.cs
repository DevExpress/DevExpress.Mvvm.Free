using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    public interface IOpenFolderDialogService {
        IFolderInfo Folder { get; }
        IEnumerable<IFolderInfo> Folders { get; }

        string Title { get; set; }

        bool ShowDialog(Action<CancelEventArgs> fileOK, string directoryName);
    }
    public interface IOpenFileDialogService {
        IFileInfo File { get; }
        IEnumerable<IFileInfo> Files { get; }

        string Filter { get; set; }
        int FilterIndex { get; set; }
        string Title { get; set; }

        bool ShowDialog(Action<CancelEventArgs> fileOK, string directoryName);
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
            return service.File.Return(x => x.GetFullName(), () => string.Empty);
        }

        public static bool ShowDialog(this IOpenFolderDialogService service) {
            VerifyService(service);
            return service.ShowDialog(null, null);
        }
        public static bool ShowDialog(this IOpenFolderDialogService service, Action<CancelEventArgs> fileOK) {
            VerifyService(service);
            return service.ShowDialog(fileOK, null);
        }
        public static bool ShowDialog(this IOpenFolderDialogService service, string directoryName) {
            VerifyService(service);
            return service.ShowDialog(null, directoryName);
        }

        static void VerifyService(object service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}