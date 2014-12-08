#if !SILVERLIGHT
using System.Windows.Forms;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using DevExpress.Mvvm.UI.Interactivity;

namespace DevExpress.Mvvm.UI {
    [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
    [TargetType(typeof(System.Windows.Controls.UserControl)), TargetType(typeof(Window))]
    public class OpenFileDialogService : FileDialogServiceBase, IOpenFileDialogService {
        public static readonly DependencyProperty MultiselectProperty =
            DependencyProperty.Register("Multiselect", typeof(bool), typeof(OpenFileDialogService), new PropertyMetadata(false));
#if !SILVERLIGHT
        public static readonly DependencyProperty ReadOnlyCheckedProperty =
            DependencyProperty.Register("ReadOnlyChecked", typeof(bool), typeof(OpenFileDialogService), new PropertyMetadata(false));
        public static readonly DependencyProperty ShowReadOnlyProperty =
            DependencyProperty.Register("ShowReadOnly", typeof(bool), typeof(OpenFileDialogService), new PropertyMetadata(false));
#else
        public static readonly DependencyProperty InitialDirectoryProperty =
            DependencyProperty.Register("InitialDirectory", typeof(string), typeof(OpenFileDialogService), new PropertyMetadata(string.Empty));
#endif
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(OpenFileDialogService), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty FilterIndexProperty =
            DependencyProperty.Register("FilterIndex", typeof(int), typeof(OpenFileDialogService), new PropertyMetadata(1));
        public bool Multiselect {
            get { return (bool)GetValue(MultiselectProperty); }
            set { SetValue(MultiselectProperty, value); }
        }
#if !SILVERLIGHT
        public bool ReadOnlyChecked {
            get { return (bool)GetValue(ReadOnlyCheckedProperty); }
            set { SetValue(ReadOnlyCheckedProperty, value); }
        }
        public bool ShowReadOnly {
            get { return (bool)GetValue(ShowReadOnlyProperty); }
            set { SetValue(ShowReadOnlyProperty, value); }
        }
#else
        public string InitialDirectory {
            get { return (string)GetValue(InitialDirectoryProperty); }
            set { SetValue(InitialDirectoryProperty, value); }
        }
#endif
        public string Filter {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        public int FilterIndex {
            get { return (int)GetValue(FilterIndexProperty); }
            set { SetValue(FilterIndexProperty, value); }
        }

        OpenFileDialog OpenFileDialog { get { return (OpenFileDialog)GetFileDialog(); } }
        public OpenFileDialogService() {
#if !SILVERLIGHT
            CheckFileExists = true;
#endif
        }
        protected override object CreateFileDialog() {
            return new OpenFileDialog();
        }
        protected override void InitFileDialog() {
            OpenFileDialog.Multiselect = Multiselect;
#if !SILVERLIGHT
            OpenFileDialog.ReadOnlyChecked = ReadOnlyChecked;
            OpenFileDialog.ShowReadOnly = ShowReadOnly;
#else
            OpenFileDialog.InitialDirectory = InitialDirectory;
#endif
            OpenFileDialog.Filter = Filter;
            OpenFileDialog.FilterIndex = FilterIndex;
        }
        protected override List<FileInfoWrapper> GetFileInfos() {
#if !SILVERLIGHT
            List<FileInfoWrapper> res = new List<FileInfoWrapper>();
            foreach(string fileName in OpenFileDialog.FileNames)
                res.Add(FileInfoWrapper.Create(fileName));
            return res;
#else
            List<FileInfoWrapper> res = new List<FileInfoWrapper>();
            foreach(FileInfo fileInfo in OpenFileDialog.Files)
                res.Add(new FileInfoWrapper(fileInfo));
            return res;
#endif
        }
        IFileInfo IOpenFileDialogService.File { get { return GetFiles().FirstOrDefault(); } }
        IEnumerable<IFileInfo> IOpenFileDialogService.Files { get { return GetFiles(); } }
        bool IOpenFileDialogService.ShowDialog() {
            return Show();
        }
    }
}