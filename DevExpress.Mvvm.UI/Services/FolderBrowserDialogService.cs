using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI {
    [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
    [TargetType(typeof(UserControl)), TargetType(typeof(Window))]
    public class FolderBrowserDialogService : ServiceBase, IFolderBrowserDialogService {
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

        FolderBrowserDialog Dialog;
        public FolderBrowserDialogService() {
            Dialog = new FolderBrowserDialog();
            HelpRequest += (d, e) => {
                if(HelpRequestCommand != null && HelpRequestCommand.CanExecute(e))
                    HelpRequestCommand.Execute(e);
            };
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
            if(res == DialogResult.OK)
                return true;
            if(res == DialogResult.Cancel)
                return false;
            throw new InvalidOperationException();
        }
    }
}