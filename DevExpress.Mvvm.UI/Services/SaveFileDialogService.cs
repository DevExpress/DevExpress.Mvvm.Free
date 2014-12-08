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
    public class SaveFileDialogService : FileDialogServiceBase, ISaveFileDialogService {
#if !SILVERLIGHT
        public static readonly DependencyProperty CreatePromptProperty =
            DependencyProperty.Register("CreatePrompt", typeof(bool), typeof(SaveFileDialogService), new PropertyMetadata(false));
        public static readonly DependencyProperty OverwritePromptProperty =
            DependencyProperty.Register("OverwritePrompt", typeof(bool), typeof(SaveFileDialogService), new PropertyMetadata(true));
#endif
        public static readonly DependencyProperty DefaultExtProperty =
            DependencyProperty.Register("DefaultExt", typeof(string), typeof(SaveFileDialogService), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DefaultFileNameProperty =
            DependencyProperty.Register("DefaultFileName", typeof(string), typeof(SaveFileDialogService), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(SaveFileDialogService), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty FilterIndexProperty =
            DependencyProperty.Register("FilterIndex", typeof(int), typeof(SaveFileDialogService), new PropertyMetadata(1));
#if !SILVERLIGHT
        public bool CreatePrompt {
            get { return (bool)GetValue(CreatePromptProperty); }
            set { SetValue(CreatePromptProperty, value); }
        }
        public bool OverwritePrompt {
            get { return (bool)GetValue(OverwritePromptProperty); }
            set { SetValue(OverwritePromptProperty, value); }
        }
#endif
        public string DefaultExt {
            get { return (string)GetValue(DefaultExtProperty); }
            set { SetValue(DefaultExtProperty, value); }
        }
        public string DefaultFileName {
            get { return (string)GetValue(DefaultFileNameProperty); }
            set { SetValue(DefaultFileNameProperty, value); }
        }
        public string Filter {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        public int FilterIndex {
            get { return (int)GetValue(FilterIndexProperty); }
            set { SetValue(FilterIndexProperty, value); }
        }

        SaveFileDialog SaveFileDialog { get { return (SaveFileDialog)GetFileDialog(); } }
        public SaveFileDialogService() {
#if !SILVERLIGHT
            CheckFileExists = false;
#endif
        }
        protected override object CreateFileDialog() {
            return new SaveFileDialog();
        }
        protected override void InitFileDialog() {
#if !SILVERLIGHT
            SaveFileDialog.CreatePrompt = CreatePrompt;
            SaveFileDialog.OverwritePrompt = OverwritePrompt;
            SaveFileDialog.FileName = DefaultFileName;
#else
            SaveFileDialog.DefaultFileName = DefaultFileName;
#endif
            SaveFileDialog.DefaultExt = DefaultExt;
            SaveFileDialog.Filter = Filter;
            SaveFileDialog.FilterIndex = FilterIndex;
        }
        protected override List<FileInfoWrapper> GetFileInfos() {
#if !SILVERLIGHT
            List<FileInfoWrapper> res = new List<FileInfoWrapper>();
            foreach(string fileName in SaveFileDialog.FileNames)
                res.Add(FileInfoWrapper.Create(fileName));
            return res;
#else
            return new List<FileInfoWrapper>();
#endif
        }
#if !SILVERLIGHT
        IFileInfo ISaveFileDialogService.File { get { return GetFiles().FirstOrDefault(); } }
#endif
        bool ISaveFileDialogService.ShowDialog() {
            return Show();
        }
        Stream ISaveFileDialogService.OpenFile() {
#if !SILVERLIGHT
            var file = ((ISaveFileDialogService)this).File;
            if(file == null) return null;
            return file.Open(FileMode.Create, FileAccess.Write);
#else
            return SaveFileDialog.OpenFile();
#endif
        }
        string ISaveFileDialogService.SafeFileName {
            get {
#if !SILVERLIGHT
                var file = ((ISaveFileDialogService)this).File;
                if(file == null) return string.Empty;
                return file.Name;
#else
                return SaveFileDialog.SafeFileName;
#endif
            }
        }
    }
}