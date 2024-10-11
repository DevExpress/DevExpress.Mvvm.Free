using System;
using System.IO;

namespace DevExpress.Mvvm {
    public interface IFileSystemInfo {
        string DirectoryName { get; }
        string Name { get; }
        bool Exists { get; }
        FileAttributes Attributes { get; set; }

        void MoveTo(string destinationFileName);
        void Delete();
    }
    public interface IFolderInfo : IFileSystemInfo {
        string Path { get; }
    }
    public interface IFileInfo : IFileSystemInfo {
        long Length { get; }

        StreamWriter AppendText();
        FileInfo CopyTo(string destinationFileName, bool overwrite);
        FileStream Create();
        StreamWriter CreateText();
        FileStream Open(FileMode mode, FileAccess access, FileShare share);
        FileStream OpenRead();
        StreamReader OpenText();
        FileStream OpenWrite();
    }

    public static class FileInfoExtensions {
        public static FileStream Open(this IFileInfo fileInfo, FileMode mode) {
            Verify(fileInfo);
            return fileInfo.Open(mode, FileAccess.ReadWrite, FileShare.None);
        }
        public static FileStream Open(this IFileInfo fileInfo, FileMode mode, FileAccess access) {
            Verify(fileInfo);
            return fileInfo.Open(mode, access, FileShare.None);
        }
        public static FileInfo CopyTo(this IFileInfo fileInfo, string destinationFileName) {
            Verify(fileInfo);
            return fileInfo.CopyTo(destinationFileName, false);
        }
        public static string GetFullName(this IFileInfo fileInfo) {
            Verify(fileInfo);
            return Path.Combine(fileInfo.DirectoryName, fileInfo.Name);
        }

        internal static void Verify(IFileInfo fileInfo) {
            if(fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));
        }
    }
}