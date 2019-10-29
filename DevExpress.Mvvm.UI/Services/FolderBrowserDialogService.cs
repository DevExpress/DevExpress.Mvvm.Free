using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

using DxDialogResult = System.Windows.Forms.DialogResult;

namespace DevExpress.Mvvm.UI.Native {
    public interface IFolderBrowserDialog : ICommonDialog {
        string Description { get; set; }
        Environment.SpecialFolder RootFolder { get; set; }
        bool ShowNewFolderButton { get; set; }
        string SelectedPath { get; set; }
    }
}

namespace DevExpress.Mvvm.UI {
    [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
    [TargetType(typeof(System.Windows.Controls.UserControl)), TargetType(typeof(Window))]
    public class FolderBrowserDialogService : ServiceBase, IFolderBrowserDialogService {
        protected class FolderBrowserDialogAdapter : IFolderBrowserDialog {
            readonly FolderBrowserDialog fileDialog;

            public FolderBrowserDialogAdapter() {
                this.fileDialog = new FolderBrowserDialog();
            }

            Environment.SpecialFolder IFolderBrowserDialog.RootFolder {
                get { return fileDialog.RootFolder; }
                set { fileDialog.RootFolder = value; }
            }
            bool IFolderBrowserDialog.ShowNewFolderButton {
                get { return fileDialog.ShowNewFolderButton; }
                set { fileDialog.ShowNewFolderButton = value; }
            }
            string IFolderBrowserDialog.SelectedPath {
                get { return fileDialog.SelectedPath; }
                set { fileDialog.SelectedPath = value; }
            }
            string IFolderBrowserDialog.Description {
                get { return fileDialog.Description; }
                set { fileDialog.Description = value; }
            }
            void ICommonDialog.Reset() {
                fileDialog.Reset();
            }
            DxDialogResult ICommonDialog.ShowDialog() {
                var dialogResult = fileDialog.ShowDialog();
                return Convert(dialogResult);
            }
            DxDialogResult ICommonDialog.ShowDialog(object ownerWindow) {
                var window = ownerWindow as Window;
                var dialogResult = window == null ? fileDialog.ShowDialog() : fileDialog.ShowDialog(new Win32WindowWrapper(window));
                return Convert(dialogResult);
            }
            event EventHandler ICommonDialog.HelpRequest {
                add { fileDialog.HelpRequest += value; }
                remove { fileDialog.HelpRequest -= value; }
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

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(FolderBrowserDialogService), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty RootFolderProperty =
            DependencyProperty.Register("RootFolder", typeof(Environment.SpecialFolder), typeof(FolderBrowserDialogService), new PropertyMetadata(Environment.SpecialFolder.Desktop));
        public static readonly DependencyProperty ShowNewFolderButtonProperty =
            DependencyProperty.Register("ShowNewFolderButton", typeof(bool), typeof(FolderBrowserDialogService), new PropertyMetadata(true));
        public static readonly DependencyProperty StartPathProperty =
            DependencyProperty.Register("StartPath", typeof(string), typeof(FolderBrowserDialogService), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty RestorePreviouslySelectedDirectoryProperty =
            DependencyProperty.Register("RestorePreviouslySelectedDirectory", typeof(bool), typeof(FolderBrowserDialogService), new PropertyMetadata(true));
        public static readonly DependencyProperty HelpRequestCommandProperty =
            DependencyProperty.Register("HelpRequestCommand", typeof(ICommand), typeof(FolderBrowserDialogService), new PropertyMetadata(null));
        public string Description {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public Environment.SpecialFolder RootFolder {
            get { return (Environment.SpecialFolder)GetValue(RootFolderProperty); }
            set { SetValue(RootFolderProperty, value); }
        }
        public bool ShowNewFolderButton {
            get { return (bool)GetValue(ShowNewFolderButtonProperty); }
            set { SetValue(ShowNewFolderButtonProperty, value); }
        }
        public string StartPath {
            get { return (string)GetValue(StartPathProperty); }
            set { SetValue(StartPathProperty, value); }
        }
        public bool RestorePreviouslySelectedDirectory {
            get { return (bool)GetValue(RestorePreviouslySelectedDirectoryProperty); }
            set { SetValue(RestorePreviouslySelectedDirectoryProperty, value); }
        }
        public ICommand HelpRequestCommand {
            get { return (ICommand)GetValue(HelpRequestCommandProperty); }
            set { SetValue(HelpRequestCommandProperty, value); }
        }
        public event EventHandler HelpRequest {
            add { Dialog.HelpRequest += value; }
            remove { Dialog.HelpRequest -= value; }
        }

        IFolderBrowserDialog Dialog;
        public FolderBrowserDialogService() {
            Dialog = CreateFolderBrowserDialog();
            HelpRequest += (d, e) => {
                if(HelpRequestCommand != null && HelpRequestCommand.CanExecute(e))
                    HelpRequestCommand.Execute(e);
            };
        }
        protected virtual IFolderBrowserDialog CreateFolderBrowserDialog() {
            return new FolderBrowserDialogAdapter();
        }

        protected object GetFileDialog() {
            return Dialog;
        }

        string resultPath = string.Empty;
        string IFolderBrowserDialogService.ResultPath {
            get { return resultPath; }
        }
        bool IFolderBrowserDialogService.ShowDialog() {
            Dialog.Description = Description;
            Dialog.RootFolder = RootFolder;
            Dialog.ShowNewFolderButton = ShowNewFolderButton;
            if(RestorePreviouslySelectedDirectory && !string.IsNullOrEmpty(resultPath))
                Dialog.SelectedPath = resultPath;
            else
                Dialog.SelectedPath = StartPath;
            var res = Dialog.ShowDialog();
            resultPath = Dialog.SelectedPath;
            if(res == DxDialogResult.OK)
                return true;
            if(res == DxDialogResult.Cancel)
                return false;
            throw new InvalidOperationException();
        }
    }
}