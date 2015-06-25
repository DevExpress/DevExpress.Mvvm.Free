#if !SILVERLIGHT
using System.Windows.Forms;
#endif
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

namespace DevExpress.Mvvm.UI {
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class FileDialogServiceBase : ServiceBase, IFileDialogServiceBase {
#if !SILVERLIGHT
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
#endif
        public static readonly DependencyProperty FileOkCommandProperty =
            DependencyProperty.Register("FileOkCommand", typeof(ICommand), typeof(FileDialogServiceBase), new PropertyMetadata(null));
        public ICommand FileOkCommand {
            get { return (ICommand)GetValue(FileOkCommandProperty); }
            set { SetValue(FileOkCommandProperty, value); }
        }
        public event CancelEventHandler FileOk;

        object FileDialog;
        IEnumerable<FileInfoWrapper> FilesCore;
        Action<CancelEventArgs> fileOK;
        public FileDialogServiceBase() {
            FileDialog = CreateFileDialog();
            FilesCore = new List<FileInfoWrapper>();
#if !SILVERLIGHT
            FileDialog dialog = (FileDialog)FileDialog;
            dialog.FileOk += OnDialogFileOk;
            dialog.HelpRequest += OnDialogHelpRequest;
#endif
        }
        void OnDialogFileOk(object sender, CancelEventArgs e) {
            UpdateFiles();
            FileOk.Do(x => x(sender, e));
            FileOkCommand.If(x => x.CanExecute(e)).Do(x => x.Execute(e));
            fileOK.Do(x => x(e));
        }
#if !SILVERLIGHT
        void OnDialogHelpRequest(object sender, EventArgs e) {
            HelpRequest.Do(x => x(sender, e));
            HelpRequestCommand.If(x => x.CanExecute(e)).Do(x => x.Execute(e));
        }
#endif


        protected abstract object CreateFileDialog();
        protected abstract void InitFileDialog();
        protected abstract List<FileInfoWrapper> GetFileInfos();
        void InitFileDialogCore() {
#if !SILVERLIGHT
            FileDialog dialog = (FileDialog)FileDialog;
            dialog.CheckFileExists = CheckFileExists;
            dialog.AddExtension = AddExtension;
            dialog.AutoUpgradeEnabled = AutoUpgradeEnabled;
            dialog.CheckPathExists = CheckPathExists;
            dialog.DereferenceLinks = DereferenceLinks;
            dialog.InitialDirectory = InitialDirectory;
            dialog.RestoreDirectory = RestoreDirectory;
            dialog.ShowHelp = ShowHelp;
            dialog.SupportMultiDottedExtensions = SupportMultiDottedExtensions;
            dialog.Title = Title;
            dialog.ValidateNames = ValidateNames;

            if(RestorePreviouslySelectedDirectory && FilesCore.Count() > 0)
                dialog.InitialDirectory = FilesCore.First().FileInfo.DirectoryName;
            else
                dialog.InitialDirectory = InitialDirectory;

#endif
        }
        void UpdateFiles() {
            var fileInfos = GetFileInfos();
            foreach(FileInfoWrapper fileInfo in fileInfos)
                ((IList)FilesCore).Add(fileInfo);
        }
#if SILVERLIGHT
        bool ConvertDialogResultToBoolean(bool? result) {
            return result.Value;
        }
#else
        bool ConvertDialogResultToBoolean(DialogResult result) {
            if(result == DialogResult.OK)
                return true;
            if(result == DialogResult.Cancel)
                return false;
            throw new InvalidOperationException("The Dialog has returned a not supported value");
        }
#endif
        protected object GetFileDialog() {
            return FileDialog;
        }
        protected IEnumerable<FileInfoWrapper> GetFiles() {
            return FilesCore;
        }
        protected bool Show(Action<CancelEventArgs> fileOK) {
            this.fileOK = fileOK;
            InitFileDialogCore();
            InitFileDialog();
            ((IList)FilesCore).Clear();
            bool res = ShowCore();
#if SILVERLIGHT
            if(res) {
                CancelEventArgs fileOKArgs = new CancelEventArgs();
                OnDialogFileOk(this, fileOKArgs);
                if(fileOKArgs.Cancel)
                    res = false;
            }
#endif
            return res;
        }
        bool ShowCore() {
#if !SILVERLIGHT
            DialogResult result = ((FileDialog)FileDialog).ShowDialog();
             if(result == DialogResult.OK)
                return true;
            if(result == DialogResult.Cancel)
                return false;
            throw new InvalidOperationException("The Dialog has returned a not supported value");
#else
            bool? result = null;
            if(FileDialog is OpenFileDialog)
                result = ((OpenFileDialog)FileDialog).ShowDialog();
            if(FileDialog is SaveFileDialog)
                result = ((SaveFileDialog)FileDialog).ShowDialog();
            return result.Value;
#endif
        }

#if !SILVERLIGHT
        void IFileDialogServiceBase.Reset() {
            ((FileDialog)FileDialog).Reset();
        }
#endif
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class FileInfoWrapper : IFileInfo {
        public static FileInfoWrapper Create(string fileName) {
            return new FileInfoWrapper(new FileInfo(fileName));
        }
        public FileInfo FileInfo { get; private set; }
        public FileInfoWrapper(FileInfo fileInfo) {
            FileInfo = fileInfo;
        }
        StreamWriter IFileInfo.AppendText() {
            return FileInfo.AppendText();
        }
        FileInfo IFileInfo.CopyTo(string destFileName, bool overwrite) {
            return FileInfo.CopyTo(destFileName, overwrite);
        }
        FileStream IFileInfo.Create() {
            return FileInfo.Create();
        }
        StreamWriter IFileInfo.CreateText() {
            return FileInfo.CreateText();
        }
        void IFileInfo.Delete() {
            FileInfo.Delete();
        }
        string IFileInfo.DirectoryName {
            get { return FileInfo.DirectoryName; }
        }
        bool IFileInfo.Exists {
            get { return FileInfo.Exists; }
        }
        long IFileInfo.Length {
            get { return FileInfo.Length; }
        }
        void IFileInfo.MoveTo(string destFileName) {
            FileInfo.MoveTo(destFileName);
        }
        string IFileInfo.Name {
            get { return FileInfo.Name; }
        }
        FileStream IFileInfo.Open(FileMode mode, FileAccess access, FileShare share) {
            return FileInfo.Open(mode, access, share);
        }
        FileStream IFileInfo.OpenRead() {
            return FileInfo.OpenRead();
        }
        StreamReader IFileInfo.OpenText() {
            return FileInfo.OpenText();
        }
        FileStream IFileInfo.OpenWrite() {
            return FileInfo.OpenWrite();
        }
        FileAttributes IFileInfo.Attributes {
            get { return FileInfo.Attributes; }
            set { FileInfo.Attributes = value; }
        }
    }
}