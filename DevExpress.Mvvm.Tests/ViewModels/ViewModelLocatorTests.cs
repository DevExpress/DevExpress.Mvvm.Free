using DevExpress.Mvvm.POCO;
using NUnit.Framework;
using System;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class ViewModelLocatorTests {
        [SetUp]
        public void SetUp() {
            ViewModelLocator.Default = new ViewModelLocator(typeof(ViewModelLocatorTests).Assembly);
        }
        [TearDown]
        public void TearDown() {
            ViewModelLocator.Default = null;
        }
        [Test]
        public void RegularVM() {
            RegularVM(typeof(ViewModelLocatorTests_a));
            RegularVM(typeof(SubClass));
        }
        [Test]
        public void POCOVM() {
            POCOVMCore(typeof(ViewModelLocatorTests_a));
            POCOVMCore(typeof(SubClass));
        }
        void RegularVM(Type vmType) {
            var vm = ViewModelLocator.Default.ResolveViewModel(vmType.Name);
            var vmName = ViewModelLocator.Default.GetViewModelTypeName(vm.GetType());
            Assert.AreEqual(vmType.FullName, vmName);
            Assert.IsFalse(vm is IPOCOViewModel);
        }
        void POCOVMCore(Type vmType) {
            object vm = ViewModelSource.Create(vmType);
            var vmName = ViewModelLocator.Default.GetViewModelTypeName(vm.GetType());
            Assert.AreEqual("IsPOCOViewModel=True;" + vmType.FullName, vmName);
            vm = ViewModelLocator.Default.ResolveViewModel(vmName);
            Assert.IsTrue(vm is IPOCOViewModel);
        }

        public class SubClass : ViewModelLocatorTests_a { }
    }

    public class ViewModelLocatorTests_a {
        public virtual string Value { get; set; }
    }
}