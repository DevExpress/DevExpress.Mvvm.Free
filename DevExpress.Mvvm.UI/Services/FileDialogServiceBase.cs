using System.Windows.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Native;
using DevExpress.Utils;
using DevExpress.Internal;
using DevExpress.Xpf.Core.Native;
using System.Windows.Interop;
using DxFileCustomPlaces = System.Windows.Forms.FileDialogCustomPlacesCollection;
using DxDialogResult = System.Windows.Forms.DialogResult;

namespace DevExpress.Mvvm.UI.Native {
    public class Win32WindowWrapper : System.Windows.Forms.IWin32Window {
        public IntPtr Handle { get; private set; }

        public Win32WindowWrapper(IntPtr handle) {
            Handle = handle;
        }
        public Win32WindowWrapper(Window window) {
            if(window.IsLoaded)
                Handle = new WindowInteropHelper(window).Handle;
        }
    }
    public interface ICommonDialog : IDisposable {
        event EventHandler HelpRequest;
        DialogResult ShowDialog();
        DialogResult ShowDialog(object owner);

        void Reset();
    }
    public interface IFileDialog : ICommonDialog {
        bool AddExtension { get; set; }
        bool CheckFileExists { get; set; }
        bool CheckPathExists { get; set; }
        FileDialogCustomPlacesCollection CustomPlaces { get; }
        string DefaultExt { get; set; }
        bool DereferenceLinks { get; set; }
        string FileName { get; set; }
        string[] FileNames { get; }
        string Filter { get; set; }
        int FilterIndex { get; set; }
        string InitialDirectory { get; set; }
        bool RestoreDirectory { get; set; }
        bool ShowHelp { get; set; }
        bool SupportMultiDottedExtensions { get; set; }
        string Title { get; set; }
        bool ValidateNames { get; set; }
        event CancelEventHandler FileOk;
    }
}

namespace DevExpress.Mvvm.UI {
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class FileDialogServiceBase : WindowAwareServiceBase, IFileDialogServiceBase {
        protected abstract class FileDialogAdapter<TFileDialog> : IFileDialog where TFileDialog : FileDialog {
            protected readonly TFileDialog fileDialog;
            public event CancelEventHandler FileOk;
            public event EventHandler HelpRequest;

            public FileDialogAdapter(TFileDialog fileDialog) {
                this.fileDialog = fileDialog;
                this.fileDialog.FileOk += OnDialogFileOk;
                this.fileDialog.HelpRequest += OnDialogHelpRequest;
            }

            bool IFileDialog.CheckPathExists {
                get { return fileDialog.CheckPathExists; }
                set { fileDialog.CheckPathExists = value; }
            }
            bool IFileDialog.CheckFileExists {
                get { return fileDialog.CheckFileExists; }
                set { fileDialog.CheckFileExists = value; }
            }
            bool IFileDialog.AddExtension {
                get { return fileDialog.AddExtension; }
                set { fileDialog.AddExtension = value; }
            }
            bool IFileDialog.DereferenceLinks {
                get { return fileDialog.DereferenceLinks; }
                set { fileDialog.DereferenceLinks = value; }
            }
            bool IFileDialog.RestoreDirectory {
                get { return fileDialog.RestoreDirectory; }
                set { fileDialog.RestoreDirectory = value; }
            }
            bool IFileDialog.ShowHelp {
                get { return fileDialog.ShowHelp; }
                set { fileDialog.ShowHelp = value; }
            }
            bool IFileDialog.SupportMultiDottedExtensions {
                get { return fileDialog.SupportMultiDottedExtensions; }
                set { fileDialog.SupportMultiDottedExtensions = value; }
            }
            bool IFileDialog.ValidateNames {
                get { return fileDialog.ValidateNames; }
                set { fileDialog.ValidateNames = value; }
            }
            string IFileDialog.InitialDirectory {
                get { return fileDialog.InitialDirectory; }
                set { fileDialog.InitialDirectory = value; }
            }
            string IFileDialog.Title {
                get { return fileDialog.Title; }
                set { fileDialog.Title = value; }
            }
            string[] IFileDialog.FileNames { get { return fileDialog.FileNames; } }

            string IFileDialog.Filter {
                get { return fileDialog.Filter; }
                set { fileDialog.Filter = value; }
            }
            int IFileDialog.FilterIndex {
                get { return fileDialog.FilterIndex; }
                set { fileDialog.FilterIndex = value; }
            }
            string IFileDialog.FileName {
                get { return fileDialog.FileName; }
                set { fileDialog.FileName = value; }
            }
            string IFileDialog.DefaultExt {
                get { return fileDialog.DefaultExt; }
                set { fileDialog.DefaultExt = value; }
            }
            DxFileCustomPlaces IFileDialog.CustomPlaces {
                get {
                    return fileDialog.CustomPlaces;
                }
            }

            void ICommonDialog.Reset() {
                fileDialog.Reset();
            }

            public DialogResult ShowDialog() {
                return fileDialog.ShowDialog();
            }
            public DialogResult ShowDialog(Window ownerWindow) {
                return fileDialog.ShowDialog(new Win32WindowWrapper(ownerWindow));
            }
            DxDialogResult ICommonDialog.ShowDialog(object ownerWindow) {
                var window = ownerWindow as Window;
                var dialogResult = window == null ? ShowDialog() : ShowDialog(window);
                return Convert(dialogResult);
            }
            DxDialogResult ICommonDialog.ShowDialog() {
                var dialogResult = ShowDialog();
                return Convert(dialogResult);
            }

            void OnDialogFileOk(object sender, CancelEventArgs e) {
                if(FileOk != null)
                    FileOk(sender, e);
            }
            void OnDialogHelpRequest(object sender, EventArgs e) {
                if(HelpRequest != null)
                    HelpRequest(sender, e);
            }

            void IDisposable.Dispose() {
                fileDialog.Dispose();
            }
            static DxDialogResult Convert(DialogResult result) {
                switch (result) {
                    case DialogResult.OK:
                        return DxDialogResult.OK;
                    case DialogResult.Cancel:
                        return DxDialogResult.Cancel;
                    case DialogResult.Abort:
                        return DxDialogResult.Abort;
                    case DialogResult.Retry:
                        return DxDialogResult.Retry;
                    case DialogResult.Ignore:
                        return DxDialogResult.Ignore;
                    case DialogResult.Yes:
                        return DxDialogResult.Yes;
                    case DialogResult.No:
                        return DxDialogResult.No;
                    default:
                        return DxDialogResult.None;
                }
            }
        }

        public static readonly DependencyProperty CheckFileExistsProperty =
            DependencyProperty.Register("CheckFileExists", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(false));
        public static readonly DependencyProperty AddExtensionProperty =
           DependencyProperty.Register("AddExtension", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(true));
        public static readonly DependencyProperty AutoUpgradeEnabledProperty =
            DependencyProperty.Register("AutoUpgradeEnabled", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(true));
        public static readonly DependencyProperty CheckPathExistsProperty =
            DependencyProperty.Register("CheckPathExists", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(true));
        public static readonly DependencyProperty DereferenceLinksProperty =
           DependencyProperty.Register("DereferenceLinks", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(true));
        public static readonly DependencyProperty InitialDirectoryProperty =
            DependencyProperty.Register("InitialDirectory", typeof(string), typeof(FileDialogServiceBase), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DefaultFileNameProperty =
            DependencyProperty.Register("DefaultFileName", typeof(string), typeof(FileDialogServiceBase), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty RestoreDirectoryProperty =
            DependencyProperty.Register("RestoreDirectory", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(false));
        public static readonly DependencyProperty ShowHelpProperty =
            DependencyProperty.Register("ShowHelp", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(false));
        public static readonly DependencyProperty SupportMultiDottedExtensionsProperty =
            DependencyProperty.Register("SupportMultiDottedExtensions", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(false));
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(FileDialogServiceBase), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ValidateNamesProperty =
            DependencyProperty.Register("ValidateNames", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(true));
        public static readonly DependencyProperty RestorePreviouslySelectedDirectoryProperty =
            DependencyProperty.Register("RestorePreviouslySelectedDirectory", typeof(bool), typeof(FileDialogServiceBase), new PropertyMetadata(true));
        public static readonly DependencyProperty HelpRequestCommandProperty =
            DependencyProperty.Register("HelpRequestCommand", typeof(ICommand), typeof(FileDialogServiceBase), new PropertyMetadata(null));

        public bool CheckFileExists {
            get { return (bool)GetValue(CheckFileExistsProperty); }
            set { SetValue(CheckFileExistsProperty, value); }
        }
        public bool AddExtension {
            get { return (bool)GetValue(AddExtensionProperty); }
            set { SetValue(AddExtensionProperty, value); }
        }
        public bool AutoUpgradeEnabled {
            get { return (bool)GetValue(AutoUpgradeEnabledProperty); }
            set { SetValue(AutoUpgradeEnabledProperty, value); }
        }
        public bool CheckPathExists {
            get { return (bool)GetValue(CheckPathExistsProperty); }
            set { SetValue(CheckPathExistsProperty, value); }
        }
        public bool DereferenceLinks {
            get { return (bool)GetValue(DereferenceLinksProperty); }
            set { SetValue(DereferenceLinksProperty, value); }
        }
        public string InitialDirectory {
            get { return (string)GetValue(InitialDirectoryProperty); }
            set { SetValue(InitialDirectoryProperty, value); }
        }
        public string DefaultFileName {
            get { return (string)GetValue(DefaultFileNameProperty); }
            set { SetValue(DefaultFileNameProperty, value); }
        }
        public bool RestoreDirectory {
            get { return (bool)GetValue(RestoreDirectoryProperty); }
            set { SetValue(RestoreDirectoryProperty, value); }
        }
        public bool ShowHelp {
            get { return (bool)GetValue(ShowHelpProperty); }
            set { SetValue(ShowHelpProperty, value); }
        }
        public bool SupportMultiDottedExtensions {
            get { return (bool)GetValue(SupportMultiDottedExtensionsProperty); }
            set { SetValue(SupportMultiDottedExtensionsProperty, value); }
        }
        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public bool ValidateNames {
            get { return (bool)GetValue(ValidateNamesProperty); }
            set { SetValue(ValidateNamesProperty, value); }
        }
        public bool RestorePreviouslySelectedDirectory {
            get { return (bool)GetValue(RestorePreviouslySelectedDirectoryProperty); }
            set { SetValue(RestorePreviouslySelectedDirectoryProperty, value); }
        }

        public ICommand HelpRequestCommand {
            get { return (ICommand)GetValue(HelpRequestCommandProperty); }
            set { SetValue(HelpRequestCommandProperty, value); }
        }
        public event EventHandler HelpRequest;

        public static readonly DependencyProperty FileOkCommandProperty =
            DependencyProperty.Register("FileOkCommand", typeof(ICommand), typeof(FileDialogServiceBase), new PropertyMetadata(null));
        public ICommand FileOkCommand {
            get { return (ICommand)GetValue(FileOkCommandProperty); }
            set { SetValue(FileOkCommandProperty, value); }
        }

        public event CancelEventHandler FileOk;
        IFileDialog FileDialog;
        IEnumerable<object> FilesCore;
        Action<CancelEventArgs> fileOK;

        public FileDialogServiceBase() {
            FileDialog = CreateFileDialogAdapter();
            FilesCore = new List<object>();
            FileDialog.FileOk += OnDialogFileOk;
            FileDialog.HelpRequest += OnDialogHelpRequest;
        }
        void OnDialogFileOk(object sender, CancelEventArgs e) {
            UpdateFiles();
            FileOk.Do(x => x(sender, e));
            FileOkCommand.If(x => x.CanExecute(e)).Do(x => x.Execute(e));
            fileOK.Do(x => x(e));
        }
        void OnDialogHelpRequest(object sender, EventArgs e) {
            HelpRequest.Do(x => x(sender, e));
            HelpRequestCommand.If(x => x.CanExecute(e)).Do(x => x.Execute(e));
        }

        protected abstract IFileDialog CreateFileDialogAdapter();

        protected abstract void InitFileDialog();
        protected abstract List<object> GetFileInfos();
        void InitFileDialogCore() {
            FileDialog.CheckFileExists = CheckFileExists;
            FileDialog.AddExtension = AddExtension;
            FileDialog.CheckPathExists = CheckPathExists;
            FileDialog.DereferenceLinks = DereferenceLinks;
            FileDialog.InitialDirectory = InitialDirectory;
            FileDialog.FileName = DefaultFileName;
            FileDialog.RestoreDirectory = RestoreDirectory;
            FileDialog.ShowHelp = ShowHelp;
            FileDialog.SupportMultiDottedExtensions = SupportMultiDottedExtensions;
            FileDialog.Title = Title;
            FileDialog.ValidateNames = ValidateNames;

            if(RestorePreviouslySelectedDirectory && FilesCore.Count() > 0)
                FileDialog.InitialDirectory = GetPreviouslySelectedDirectory();
            else
                FileDialog.InitialDirectory = InitialDirectory;
        }
        string GetPreviouslySelectedDirectory() {
            if(FilesCore.First() is IFileInfo)
                return ((IFileInfo)FilesCore.First()).DirectoryName;
            if(FilesCore.First() is IFolderInfo)
                return ((IFolderInfo)FilesCore.First()).DirectoryName;
            return null;
        }

        void UpdateFiles() {
            var fileInfos = GetFileInfos();
            IList filesCore = (IList)FilesCore;
            filesCore.Clear();
            foreach(var fileInfo in fileInfos)
                filesCore.Add(fileInfo);
        }
        bool ConvertDialogResultToBoolean(DialogResult result) {
            if(result == DialogResult.OK)
                return true;
            if(result == DialogResult.Cancel)
                return false;
            throw new InvalidOperationException("The Dialog has returned a not supported value");
        }

        protected object GetFileDialog() {
            return FileDialog;
        }
        protected IEnumerable<object> GetFiles() {
            return FilesCore;
        }
        protected bool Show(Action<CancelEventArgs> fileOK) {
            this.fileOK = fileOK;
            InitFileDialogCore();
            InitFileDialog();
            ((IList)FilesCore).Clear();
            bool res = ShowCore();
            return res;
        }
        bool ShowCore() {
            DxDialogResult result = ActualWindow == null ? FileDialog.ShowDialog() : FileDialog.ShowDialog(new Win32WindowWrapper(ActualWindow));
            if(result == DxDialogResult.OK)
                return true;
            if(result == DxDialogResult.Cancel)
                return false;
            throw new InvalidOperationException("The Dialog has returned a not supported value");
        }
        void IFileDialogServiceBase.Reset() {
            FileDialog.Reset();
        }

        protected override void OnActualWindowChanged(Window oldWindow) { }
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class FileInfoWrapper : IFileInfo {
        public static FileInfoWrapper CreateDirectoryWrapper(string directoryPath) {
            return new FileInfoWrapper(new DirectoryInfo(directoryPath));
        }
        public static FileInfoWrapper Create(string fileName) {
            return new FileInfoWrapper(new FileInfo(fileName));
        }

        public FileSystemInfo FileSystemInfo { get; }
        public FileInfo FileInfo { get { return FileSystemInfo as FileInfo; } }
        DirectoryInfo DirectoryInfo { get { return FileSystemInfo as DirectoryInfo; } }

        public FileInfoWrapper(FileInfo fileInfo) {
            FileSystemInfo = fileInfo;
        }
        FileInfoWrapper(DirectoryInfo directoryInfo) {
            FileSystemInfo = directoryInfo;
        }

        StreamWriter IFileInfo.AppendText() {
            return Match(() => FileInfo.AppendText());
        }
        FileInfo IFileInfo.CopyTo(string destFileName, bool overwrite) {
            return Match(() => FileInfo.CopyTo(destFileName, overwrite));
        }
        FileStream IFileInfo.Create() {
            return Match(() => FileInfo.Create());
        }
        StreamWriter IFileInfo.CreateText() {
            return Match(() => FileInfo.CreateText());
        }
        void IFileInfo.Delete() {
            FileSystemInfo.Delete();
        }
        string IFileInfo.DirectoryName {
            get { return Match(() => FileInfo.DirectoryName, () => DirectoryInfo.Parent.FullName); }
        }
        bool IFileInfo.Exists {
            get { return FileSystemInfo.Exists; }
        }
        long IFileInfo.Length {
            get { return FileInfo.Return(x => x.Length, () => 0); }
        }
        void IFileInfo.MoveTo(string destFileName) {
            FileInfo.Do(x => x.MoveTo(destFileName));
        }
        string IFileInfo.Name {
            get { return FileSystemInfo.Name; }
        }
        FileStream IFileInfo.Open(FileMode mode, FileAccess access, FileShare share) {
            return Match(() => FileInfo.Open(mode, access, share));
        }
        FileStream IFileInfo.OpenRead() {
            return Match(() => FileInfo.OpenRead());
        }
        StreamReader IFileInfo.OpenText() {
            return Match(() => FileInfo.OpenText());
        }
        FileStream IFileInfo.OpenWrite() {
            return Match(() => FileInfo.OpenWrite());
        }
        FileAttributes IFileInfo.Attributes {
            get { return FileSystemInfo.Attributes; }
            set { FileSystemInfo.Attributes = value; }
        }

        T Match<T>(Func<T> file, Func<T> directory = null) where T: class {
            return FileInfo.Return(_ => file(), () => directory?.Invoke());
        }
    }
}