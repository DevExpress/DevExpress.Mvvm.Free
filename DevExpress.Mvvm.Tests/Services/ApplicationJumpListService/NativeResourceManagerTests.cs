using DevExpress.Internal;
using DevExpress.Mvvm.UI.Native;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using DevExpress.Utils;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class NativeResourceManagerTests : BaseWpfFixture {
        protected override void SetUpCore() {
            base.SetUpCore();
            ApplicationJumpListServiceTestsImageSourceHelper.RegisterPackScheme();
            NativeResourceManager.CompanyNameOverride = "DevExpress Tests";
            NativeResourceManager.ProductNameOverride = "DevExpress.Xpf.Core Tests";
            NativeResourceManager.VersionOverride = AssemblyInfo.Version;
        }
        protected override void TearDownCore() {
            string resourcesFolder = NativeResourceManager.ExpandVariables(NativeResourceManager.ResourcesFolder);
            if(Directory.Exists(resourcesFolder))
                Directory.Delete(resourcesFolder, true);
            NativeResourceManager.ProductNameOverride = null;
            NativeResourceManager.CompanyNameOverride = null;
            NativeResourceManager.VersionOverride = null;
            base.TearDownCore();
        }
        [Test]
        public void ExpandVariablesTest() {
            Dictionary<string, Func<string>> variables = new Dictionary<string, Func<string>>();
            variables.Add("%ABC%", () => "_ABC_");
            variables.Add("%DEF%", () => "_DEF_");
            Assert.AreEqual("_ABC__DEF_", NativeResourceManager.ExpandVariablesCore("%ABC%%DEF%", variables));
            Assert.AreEqual("x_ABC_y_DEF_z", NativeResourceManager.ExpandVariablesCore("x%ABC%y%DEF%z", variables));
            Assert.AreEqual("x%%_ABC_y_DEF_", NativeResourceManager.ExpandVariablesCore("x%%%ABC%y%DEF%", variables));
            Assert.AreEqual("x%%_ABC_y%DEF", NativeResourceManager.ExpandVariablesCore("x%%%ABC%y%DEF", variables));
            Assert.AreEqual("%NONE%_ABC_", NativeResourceManager.ExpandVariablesCore("%NONE%%ABC%", variables));
            Assert.AreEqual("_ABC_", NativeResourceManager.ExpandVariablesCore("%Abc%", variables));
            Assert.AreEqual("_DEF_ABC%XYZ%", NativeResourceManager.ExpandVariablesCore("%DEF%ABC%XYZ%", variables));
        }
        [Test]
        public void UseRealEntryAssemblyTest() {
            var savedEntryAssembly = AssemblyHelper.EntryAssembly;
            AssemblyHelper.EntryAssembly = typeof(int).Assembly;
            try {
                Assert.AreNotEqual(NativeResourceManager.EntryAssembly, AssemblyHelper.EntryAssembly);
            } finally {
                AssemblyHelper.EntryAssembly = savedEntryAssembly;
            }
        }
    }
}