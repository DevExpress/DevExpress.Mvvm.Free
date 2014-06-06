#if !SILVERLIGHT
using NUnit.Framework;
using System.Windows.Forms;
#else
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Linq;


namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class SaveFileDialogServiceTests {
        [Test]
        public void DefaultValues() {
            SaveFileDialogService service = new SaveFileDialogService();
#if !SILVERLIGHT
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
#endif
            Assert.AreEqual(string.Empty, service.DefaultExt);
            Assert.AreEqual(string.Empty, service.DefaultFileName);
            Assert.AreEqual(string.Empty, service.Filter);
            Assert.AreEqual(1, service.FilterIndex);

            ISaveFileDialogService iService = service;
#if !SILVERLIGHT
            Assert.IsNull(iService.File);
#endif
            Assert.AreEqual(string.Empty, iService.SafeFileName);
        }

    }
    [TestFixture]
    public class OpenFileDialogServiceTests {
        [Test]
        public void DefaultValues() {
            OpenFileDialogService service = new OpenFileDialogService();
#if !SILVERLIGHT
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
#endif
            Assert.AreEqual(string.Empty, service.Filter);
            Assert.AreEqual(1, service.FilterIndex);
            Assert.AreEqual(string.Empty, service.InitialDirectory);
            Assert.AreEqual(false, service.Multiselect);

            IOpenFileDialogService iService = service;
            Assert.IsNull(iService.File);
            Assert.AreEqual(0, iService.Files.Count());
        }
    }
}