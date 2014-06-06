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
        public static readonly DependencyProperty FileOkCommandProperty =
            DependencyProperty.Register("FileOkCommand", typeof(ICommand), typeof(FileDialogServiceBase), new PropertyMetadata(null));
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

        public ICommand FileOkCommand {
            get { return (ICommand)GetValue(FileOkCommandProperty); }
            set { SetValue(FileOkCommandProperty, value); }
        }
        public ICommand HelpRequestCommand {
            get { return (ICommand)GetValue(HelpRequestCommandProperty); }
            set { SetValue(HelpRequestCommandProperty, value); }
        }
        public event CancelEventHandler FileOk {
            add {
                FileDialog dialog = (FileDialog)FileDialog;
                dialog.FileOk += value;
            }
            remove {
                FileDialog dialog = (FileDialog)FileDialog;
                dialog.FileOk -= value;
            }
        }
        public event EventHandler HelpRequest {
            add {
                FileDialog dialog = (FileDialog)FileDialog;
                dialog.HelpRequest += value;
            }
            remove {
                FileDialog dialog = (FileDialog)FileDialog;
                dialog.HelpRequest -= value;
            }
        }
#endif

        object FileDialog;
        IEnumerable<FileInfoWrapper> FilesCore;
        public FileDialogServiceBase() {
            FileDialog = CreateFileDialog();
            FilesCore = new List<FileInfoWrapper>();
#if !SILVERLIGHT
            FileOk += (d, e) => {
                if(FileOkCommand != null && FileOkCommand.CanExecute(e))
                    FileOkCommand.Execute(e);
            };
            HelpRequest += (d, e) => {
                if(HelpRequestCommand != null && HelpRequestCommand.CanExecute(e))
                    HelpRequestCommand.Execute(e);
            };
#endif
        }
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
        void UpdateFiles(bool dialogResult) {
            ((IList)FilesCore).Clear();
            if(!dialogResult) return;
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
        protected bool Show() {
            InitFileDialogCore();
            InitFileDialog();
#if !SILVERLIGHT
            DialogResult result = ((FileDialog)FileDialog).ShowDialog();
#else
            bool? result = null;
            if(FileDialog is OpenFileDialog)
                result = ((OpenFileDialog)FileDialog).ShowDialog();
            if(FileDialog is SaveFileDialog)
                result = ((SaveFileDialog)FileDialog).ShowDialog();
#endif
            bool res = ConvertDialogResultToBoolean(result);
            UpdateFiles(res);
            return res;
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
        FileInfo IFileInfo.CopyTo(string destFileName) {
            return FileInfo.CopyTo(destFileName);
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
        FileStream IFileInfo.Open(FileMode mode, FileAccess access) {
            return FileInfo.Open(mode, access);
        }
        FileStream IFileInfo.Open(FileMode mode) {
            return FileInfo.Open(mode);
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
    }
}