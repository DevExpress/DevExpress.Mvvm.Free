using NUnit.Framework;
using System.Windows.Forms;
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using DevExpress.Mvvm.UI.Native;
using System.Windows;
using DxDialogResult = System.Windows.Forms.DialogResult;
using DxFileDialogCustomPlaces = System.Windows.Forms.FileDialogCustomPlacesCollection;

namespace DevExpress.Mvvm.UI.Tests {
    public abstract class FileDialogServiceBaseTests { }
    [TestFixture]
    public class SaveFileDialogServiceTests : FileDialogServiceBaseTests {
        [Test]
        public void DefaultValues() {
            SaveFileDialogService service = new SaveFileDialogService();
            Assert.AreEqual(true, service.AddExtension);
            Assert.AreEqual(true, service.AutoUpgradeEnabled);
            Assert.AreEqual(false, service.CheckFileExists);
            Assert.AreEqual(true, service.CheckPathExists);
            Assert.AreEqual(false, service.CreatePrompt);
            Assert.AreEqual(true, service.DereferenceLinks);
            Assert.AreEqual(string.Empty, service.InitialDirectory);
            Assert.AreEqual(true, service.OverwritePrompt);
            Assert.AreEqual(false, service.RestoreDirectory);
            Assert.AreEqual(false, service.ShowHelp);
            Assert.AreEqual(false, service.SupportMultiDottedExtensions);
            Assert.AreEqual(string.Empty, service.Title);
            Assert.AreEqual(true, service.ValidateNames);
            Assert.AreEqual(string.Empty, service.DefaultExt);
            Assert.AreEqual(string.Empty, service.DefaultFileName);
            Assert.AreEqual(string.Empty, service.Filter);
            Assert.AreEqual(1, service.FilterIndex);

            ISaveFileDialogService iService = service;
            Assert.IsNull(iService.File);
            Assert.AreEqual(string.Empty, iService.SafeFileName());
        }

    }
    [TestFixture]
    public class OpenFileDialogServiceTests : FileDialogServiceBaseTests {
        [Test]
        public void DefaultValues() {
            OpenFileDialogService service = new OpenFileDialogService();
            Assert.AreEqual(true, service.AddExtension);
            Assert.AreEqual(true, service.AutoUpgradeEnabled);
            Assert.AreEqual(true, service.CheckFileExists);
            Assert.AreEqual(true, service.CheckPathExists);
            Assert.AreEqual(true, service.DereferenceLinks);
            Assert.AreEqual(false, service.RestoreDirectory);
            Assert.AreEqual(false, service.ShowHelp);
            Assert.AreEqual(false, service.ShowReadOnly);
            Assert.AreEqual(false, service.SupportMultiDottedExtensions);
            Assert.AreEqual(string.Empty, service.Title);
            Assert.AreEqual(true, service.ValidateNames);
            Assert.AreEqual(string.Empty, service.Filter);
            Assert.AreEqual(1, service.FilterIndex);
            Assert.AreEqual(string.Empty, service.InitialDirectory);
            Assert.AreEqual(string.Empty, service.DefaultFileName);
            Assert.AreEqual(false, service.Multiselect);

            IOpenFileDialogService iService = service;
            Assert.IsNull(iService.File);
            Assert.AreEqual(0, iService.Files.Count());
        }
        [Test]
        public void UpdateSelectedFilesAtErrorTest() {
            IOpenFileDialogService dialog = new TestFileDialogService();
            var selectedFilesList = new List<string>();

            int retryCount = 2;
            var dialogResult = dialog.ShowDialog(e => {
                selectedFilesList.Add(dialog.File.Name);
                e.Cancel = retryCount-- > 0;
            });
            Assert.AreEqual(3, selectedFilesList.Distinct().Count());
        }
    }
    public class TestFileDialogService : OpenFileDialogService {
        public static TestFileDialog Dialog;
        public class TestFileDialog : IOpenFileDialog {
            public bool AddExtension { get; set; }
            public bool AutoUpgradeEnabled { get; set; }
            public bool CheckFileExists { get; set; }
            public bool CheckPathExists { get; set; }
            public bool DereferenceLinks { get; set; }
            public string InitialDirectory { get; set; }
            public string DefaultFileName { get; set; }
            public bool RestoreDirectory { get; set; }
            public bool ShowHelp { get; set; }
            public bool SupportMultiDottedExtensions { get; set; }
            public string Title { get; set; }
            public bool ValidateNames { get; set; }
            public bool Multiselect { get; set; }
            public bool ReadOnlyChecked { get; set; }
            public bool ShowReadOnly { get; set; }
            public string Filter { get; set; }
            public int FilterIndex { get; set; }
            public string[] FileNames { get; set; }
            public string FileName { get; set; }
            public string DefaultExt { get; set; }

            public string SafeFileName { get; private set; }

            public string[] SafeFileNames { get; private set; }

            public DxFileDialogCustomPlaces CustomPlaces { get; private set; }

            public event CancelEventHandler FileOk;
            public event EventHandler HelpRequest { add { } remove { } }

            public void Reset() { }
            public void Dispose() { }

            public DxDialogResult ShowDialog() {
                FileNames = new[] { "initFileName" };
                var cancelEventArgs = new CancelEventArgs();
                while(true) {
                    FileNames = new[] { string.Format("{0}{1}", FileNames[0], "new" ) };
                    if(FileOk != null)
                        FileOk(this, cancelEventArgs);
                    if(!cancelEventArgs.Cancel)
                        break;
                }
                return DxDialogResult.OK;
            }
            public DxDialogResult ShowDialog(object owner) {
                return ShowDialog();
            }
        }

        protected override IFileDialog CreateFileDialogAdapter() {
            Dialog = new TestFileDialog();
            return Dialog;
        }
    }
}