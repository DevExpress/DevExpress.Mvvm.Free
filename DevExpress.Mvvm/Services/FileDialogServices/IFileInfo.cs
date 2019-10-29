using System;
using System.IO;

namespace DevExpress.Mvvm {
    public interface IFolderInfo {
        string Path { get; }

        string DirectoryName { get; }
        string Name { get; }
        bool Exists { get; }
        FileAttributes Attributes { get; set; }

        void MoveTo(string destinationDirectoryName);
        void Delete();
    }
    public interface IFileInfo {
        long Length { get; }

        string DirectoryName { get; }
        string Name { get; }
        bool Exists { get; }
        FileAttributes Attributes { get; set; }

        void MoveTo(string destinationFileName);
        void Delete();

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
        public static FileInfo CopyTo(this IFileInfo fileInfo, string destFileName) {
            Verify(fileInfo);
            return fileInfo.CopyTo(destFileName, false);
        }
        public static string GetFullName(this IFileInfo fileInfo) {
            Verify(fileInfo);
            return Path.Combine(fileInfo.DirectoryName, fileInfo.Name);
        }

        internal static void Verify(IFileInfo fileInfo) {
            if(fileInfo == null)
                throw new ArgumentNullException("fileInfo");
        }
    }
}