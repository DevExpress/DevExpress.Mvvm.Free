using System.IO;

namespace DevExpress.Mvvm {
    public interface IFileInfo {
        string Name { get; }
        string DirectoryName { get; }
        long Length { get; }
        bool Exists { get; }

        StreamWriter AppendText();
        FileInfo CopyTo(string destFileName);
        FileInfo CopyTo(string destFileName, bool overwrite);
        FileStream Create();
        StreamWriter CreateText();
        void Delete();
        void MoveTo(string destFileName);
        FileStream Open(FileMode mode);
        FileStream Open(FileMode mode, FileAccess access);
        FileStream Open(FileMode mode, FileAccess access, FileShare share);
        FileStream OpenRead();
        StreamReader OpenText();
        FileStream OpenWrite();
    }
}