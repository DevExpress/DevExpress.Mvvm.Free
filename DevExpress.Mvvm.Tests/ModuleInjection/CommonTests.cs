#if !FREE
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Tests;
#endif
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.ModuleInjection;
using DevExpress.Mvvm.POCO;
using NUnit.Framework;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevExpress.Mvvm.UI.ModuleInjection.Tests {
    [TestFixture]
    public class ExceptionTests : BaseWpfFixture {
        protected override void SetUpCore() {
            base.SetUpCore();
            InjectionTestHelper.SetUp(GetType().Assembly);
        }
        protected override void TearDownCore() {
            InjectionTestHelper.TearDown();
            base.TearDownCore();
        }
        [Test]
        public void Module() {
            Assert.Throws<ArgumentNullException>(() => new Module(null, (Func<object>)null));
            Assert.Throws<ArgumentNullException>(() => new Module("1", (Func<object>)null));
            Assert.Throws<ArgumentNullException>(() => new Module("1", (Func<object>)null, "View1"));
            Assert.Throws<ArgumentNullException>(() => new Module("1", (Func<object>)null, typeof(ExceptionTests)));
            Assert.Throws<ArgumentNullException>(() => new Module("1", string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new Module("1", string.Empty, "View1"));
        }
        [Test]
        public void ModuleAlreadyExists() {
            Assert.AreEqual(null, ModuleManager.DefaultManager.GetModule("B", "1"));
            ModuleManager.DefaultManager.Register("B", new Module("1", () => new object()));
            Assert.Throws<ModuleInjectionException>(() => ModuleManager.DefaultManager.Register("B", new Module("1", () => new object())),
                "A module with the same key already exists.");
        }
        [Test]
        public void MissingModule() {
            Assert.Throws<ModuleInjectionException>(() => 
                ModuleManager.DefaultManager.Inject("A", "1"),
               "Cannot find a module with the passed key. Register module before working with it.");
            Assert.Throws<ModuleInjectionException>(() => 
               ModuleManager.DefaultManager.InjectOrNavigate("A", "1"),
               "Cannot find a module with the passed key. Register module before working with it.");
            ModuleManager.DefaultManager.Navigate("A", "1");
            Assert.AreEqual(false, ModuleManager.DefaultManager.IsInjected("A", "1"));
            ModuleManager.DefaultManager.Remove("B", "1");
            ModuleManager.DefaultManager.Unregister("B", "1");
            ModuleManager.DefaultManager.Clear("A");
            Assert.AreEqual(null, ModuleManager.DefaultManager.GetRegion("B").GetKey(new object()));
            Assert.AreEqual(null, ModuleManager.DefaultManager.GetRegion("B").SelectedKey);


            Assert.Throws<ModuleInjectionException>(() => 
                ModuleManager.DefaultWindowManager.Show("A", "1"),
               "Cannot find a module with the passed key. Register module before working with it.");
            Assert.Throws<ModuleInjectionException>(() => 
                ModuleManager.DefaultWindowManager.ShowOrActivate("A", "1"),
               "Cannot find a module with the passed key. Register module before working with it.");
            ModuleManager.DefaultWindowManager.Activate("A", "1");
            Assert.AreEqual(false, ModuleManager.DefaultWindowManager.IsShown("A", "1"));
            ModuleManager.DefaultWindowManager.Close("B", "1");
            ModuleManager.DefaultWindowManager.Unregister("B", "1");
            ModuleManager.DefaultWindowManager.Clear("A");
            Assert.AreEqual(null, ModuleManager.DefaultWindowManager.GetRegion("B").GetKey(new object()));
            Assert.AreEqual(null, ModuleManager.DefaultWindowManager.GetRegion("B").SelectedKey);
        }
        [Test]
        public void CannotAttachService() {
            Assert.Throws<ModuleInjectionException>(() =>
                UIRegion.SetRegion(new SolidColorBrush(), "A"),
                "This service can be only attached to a FrameworkElement or FrameworkContentElement");
        }
        [Test]
        public void NullRegionName() {
            var module = new Module("1", () => new object());
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Register(null, module));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Unregister(null, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.RegisterOrInjectOrNavigate(null, module));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.InjectOrNavigate(null, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.RegisterOrShowOrActivate(null, module));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.ShowOrActivate(null, "1"));

            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.GetRegion(string.Empty));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.GetModule(string.Empty, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.GetEvents(string.Empty));

            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.IsInjected(string.Empty, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Inject(string.Empty, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Navigate(string.Empty, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Remove(string.Empty, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Clear(string.Empty));

            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.IsShown(string.Empty, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.Show(string.Empty, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.Activate(string.Empty, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.Close(string.Empty, "1"));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.Clear(string.Empty));
        }
        [Test]
        public void NullModule() {
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Register("A", null));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("A", null));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.RegisterOrShowOrActivate("A", null));
        }
        [Test]
        public void NullKey() {
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.GetModule("A", null));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Unregister("A", null));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.InjectOrNavigate("A", null));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.ShowOrActivate("A", null));

            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.IsInjected("A", null));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Inject("A", null));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultManager.Remove("A", null));
            ModuleManager.DefaultManager.Navigate("A", null);

            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.IsShown("A", null));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.Show("A", null));
            Assert.Throws<ArgumentNullException>(() => ModuleManager.DefaultWindowManager.Close("A", null));
            ModuleManager.DefaultWindowManager.Activate("A", null);
        }
    }

#if !FREE
    [TestFixture]
    public class ModuleInjectionTests : BaseWpfFixture {
        protected override void SetUpCore() {
            base.SetUpCore();
            InjectionTestHelper.SetUp(GetType().Assembly);
        }
        protected override void TearDownCore() {
            InjectionTestHelper.TearDown();
            base.TearDownCore();
        }

        [Test]
        public void TwoControls_Selection() {
            Grid g = new Grid();
            TabControl t1 = new TabControl();
            TabControl t2 = new TabControl();
            g.Children.Add(t1);
            g.Children.Add(t2);
            Window.Content = g;
            UIRegion.SetRegion(t1, "A");
            UIRegion.SetRegion(t2, "A");
            Window.Show();
            object vm1 = null;
            object vm2 = null;
            var service1 = UIRegionBase.GetInheritedService(t1);
            var service2 = UIRegionBase.GetInheritedService(t2);
            var serviceHelper1 = InjectionTestHelper.CreateServiceHelper(service1);
            var serviceHelper2 = InjectionTestHelper.CreateServiceHelper(service2);

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("A", new Module("1", () => vm1 = new object(), typeof(View1_BaseTests)));
            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("A", new Module("2", () => vm2 = new object(), typeof(View2_BaseTests)));
            DispatcherHelper.DoEvents();
            Assert.AreEqual(vm2, t1.SelectedItem);
            Assert.AreEqual(vm2, t2.SelectedItem);
            serviceHelper1.AssertSelectionChanged(2);
            serviceHelper2.AssertSelectionChanged(2);

            ModuleManager.DefaultManager.Navigate("A", "1");
            DispatcherHelper.DoEvents();
            Assert.AreEqual(vm1, t1.SelectedItem);
            Assert.AreEqual(vm1, t2.SelectedItem);
            serviceHelper1.AssertSelectionChanged(3);
            serviceHelper2.AssertSelectionChanged(3);

            ModuleManager.DefaultManager.Navigate("A", null);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(null, t1.SelectedItem);
            Assert.AreEqual(null, t2.SelectedItem);
            serviceHelper1.AssertSelectionChanged(4);
            serviceHelper2.AssertSelectionChanged(4);

            t1.SelectedIndex = 0;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(vm1, t1.SelectedItem);
            Assert.AreEqual(vm1, t2.SelectedItem);
            serviceHelper1.AssertSelectionChanged(5);
            serviceHelper2.AssertSelectionChanged(5);

            t2.SelectedIndex = 1;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(vm2, t1.SelectedItem);
            Assert.AreEqual(vm2, t2.SelectedItem);
            serviceHelper1.AssertSelectionChanged(6);
            serviceHelper2.AssertSelectionChanged(6);
        }
        [Test]
        public void TwoControls_Remove() {
            Grid g = new Grid();
            TabControl t1 = new TabControl();
            DXTabControl t2 = new DXTabControl();
            g.Children.Add(t1);
            g.Children.Add(t2);
            Window.Content = g;
            UIRegion.SetRegion(t1, "A");
            UIRegion.SetRegion(t2, "A");
            Window.Show();
            object vm1 = null;
            object vm2 = null;
            var service1 = UIRegionBase.GetInheritedService(t1);
            var service2 = UIRegionBase.GetInheritedService(t2);
            var serviceHelper1 = InjectionTestHelper.CreateServiceHelper(service1);
            var serviceHelper2 = InjectionTestHelper.CreateServiceHelper(service2);

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("A", new Module("1", () => vm1 = new object(), typeof(View1_BaseTests)));
            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("A", new Module("2", () => vm2 = new object(), typeof(View2_BaseTests)));
            DispatcherHelper.DoEvents();
            Assert.AreEqual(vm2, t1.SelectedItem);
            Assert.AreEqual(vm2, t2.SelectedItem);
            serviceHelper1.AssertSelectionChanged(2);
            serviceHelper2.AssertSelectionChanged(2);

            serviceHelper1.CancelViewModelRemoving = true;
            ModuleManager.DefaultManager.Remove("A", "2");
            serviceHelper1.AssertViewModelRemoving(1);
            serviceHelper2.AssertViewModelRemoving(1);
            serviceHelper1.AssertSelectionChanged(2);
            serviceHelper2.AssertSelectionChanged(2);

            serviceHelper2.CancelViewModelRemoving = true;
            ModuleManager.DefaultManager.Remove("A", "2");
            serviceHelper1.AssertViewModelRemoving(2);
            serviceHelper2.AssertViewModelRemoving(2);
            serviceHelper1.AssertSelectionChanged(2);
            serviceHelper2.AssertSelectionChanged(2);

            serviceHelper1.CancelViewModelRemoving = false;
            ModuleManager.DefaultManager.Remove("A", "2");
            serviceHelper1.AssertViewModelRemoving(3);
            serviceHelper2.AssertViewModelRemoving(3);
            serviceHelper1.AssertSelectionChanged(2);
            serviceHelper2.AssertSelectionChanged(2);

            serviceHelper1.AssertViewModelRemoved(0);
            serviceHelper2.AssertViewModelRemoved(0);
            Assert.AreEqual(2, serviceHelper1.ViewModels.Count());
            Assert.AreEqual(2, serviceHelper2.ViewModels.Count());

            serviceHelper2.CancelViewModelRemoving = false;
            ModuleManager.DefaultManager.Remove("A", "2");
            serviceHelper1.AssertViewModelRemoving(4);
            serviceHelper2.AssertViewModelRemoving(4);
            serviceHelper1.AssertSelectionChanged(3);
            serviceHelper2.AssertSelectionChanged(3);
            serviceHelper1.AssertViewModelRemoved(1);
            serviceHelper2.AssertViewModelRemoved(1);
            Assert.AreEqual(1, serviceHelper1.ViewModels.Count());
            Assert.AreEqual(1, serviceHelper2.ViewModels.Count());

            ModuleManager.DefaultManager.Remove("A", "1");
            serviceHelper1.AssertViewModelRemoving(5);
            serviceHelper2.AssertViewModelRemoving(5);
            serviceHelper1.AssertSelectionChanged(4);
            serviceHelper2.AssertSelectionChanged(4);
            serviceHelper1.AssertViewModelRemoved(2);
            serviceHelper2.AssertViewModelRemoved(2);
            Assert.AreEqual(0, serviceHelper1.ViewModels.Count());
            Assert.AreEqual(0, serviceHelper2.ViewModels.Count());
        }
        [Test]
        public void TwoControls_RemoveFromControl() {
            Grid g = new Grid();
            TabControl t1 = new TabControl();
            DXTabControl t2 = new DXTabControl();
            g.Children.Add(t1);
            g.Children.Add(t2);
            Window.Content = g;
            UIRegion.SetRegion(t1, "A");
            UIRegion.SetRegion(t2, "A");
            Window.Show();
            object vm1 = null;
            object vm2 = null;
            var service1 = UIRegionBase.GetInheritedService(t1);
            var service2 = UIRegionBase.GetInheritedService(t2);
            var serviceHelper1 = InjectionTestHelper.CreateServiceHelper(service1);
            var serviceHelper2 = InjectionTestHelper.CreateServiceHelper(service2);

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("A", new Module("1", () => vm1 = new object(), typeof(View1_BaseTests)));
            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("A", new Module("2", () => vm2 = new object(), typeof(View2_BaseTests)));
            DispatcherHelper.DoEvents();
            Assert.AreEqual(vm2, t1.SelectedItem);
            Assert.AreEqual(vm2, t2.SelectedItem);
            serviceHelper1.AssertSelectionChanged(2);
            serviceHelper2.AssertSelectionChanged(2);

            serviceHelper1.CancelViewModelRemoving = true;
            t2.RemoveTabItem(1);
            serviceHelper1.AssertViewModelRemoving(1);
            serviceHelper2.AssertViewModelRemoving(1);
            serviceHelper1.AssertSelectionChanged(2);
            serviceHelper2.AssertSelectionChanged(2);

            serviceHelper2.CancelViewModelRemoving = true;
            t2.RemoveTabItem(1);
            serviceHelper1.AssertViewModelRemoving(2);
            serviceHelper2.AssertViewModelRemoving(2);
            serviceHelper1.AssertSelectionChanged(2);
            serviceHelper2.AssertSelectionChanged(2);

            serviceHelper1.CancelViewModelRemoving = false;
            t2.RemoveTabItem(1);
            serviceHelper1.AssertViewModelRemoving(3);
            serviceHelper2.AssertViewModelRemoving(3);
            serviceHelper1.AssertSelectionChanged(2);
            serviceHelper2.AssertSelectionChanged(2);

            serviceHelper1.AssertViewModelRemoved(0);
            serviceHelper2.AssertViewModelRemoved(0);
            Assert.AreEqual(2, serviceHelper1.ViewModels.Count());
            Assert.AreEqual(2, serviceHelper2.ViewModels.Count());

            serviceHelper2.CancelViewModelRemoving = false;
            t2.RemoveTabItem(1);
            serviceHelper1.AssertViewModelRemoving(4);
            serviceHelper2.AssertViewModelRemoving(4);
            serviceHelper1.AssertSelectionChanged(4);
            serviceHelper2.AssertSelectionChanged(4);
            serviceHelper1.AssertViewModelRemoved(1);
            serviceHelper2.AssertViewModelRemoved(1);
            Assert.AreEqual(1, serviceHelper1.ViewModels.Count());
            Assert.AreEqual(1, serviceHelper2.ViewModels.Count());

            t2.RemoveTabItem(0);
            serviceHelper1.AssertViewModelRemoving(5);
            serviceHelper2.AssertViewModelRemoving(5);
            serviceHelper1.AssertSelectionChanged(5);
            serviceHelper2.AssertSelectionChanged(5);
            serviceHelper1.AssertViewModelRemoved(2);
            serviceHelper2.AssertViewModelRemoved(2);
            Assert.AreEqual(0, serviceHelper1.ViewModels.Count());
            Assert.AreEqual(0, serviceHelper2.ViewModels.Count());
        }
    }
#endif

    [TestFixture]
    public class ViewModelCreationTests : BaseWpfFixture {
        protected override void SetUpCore() {
            base.SetUpCore();
            InjectionTestHelper.SetUp(GetType().Assembly);
        }
        protected override void TearDownCore() {
            InjectionTestHelper.TearDown();
            base.TearDownCore();
        }
        [Test]
        public void Exception() {
            ContentControl c = new ContentControl();
            UIRegion.SetRegion(c, "R");
            ModuleManager.DefaultManager.Register("R", new Module("1", typeof(VM1).FullName, null));
            Assert.Throws<LocatorException>(() => ModuleManager.DefaultManager.Inject("R", "1"),
                "ViewModelLocator cannot resolve the \"VM1\" type.");

            ModuleManager.DefaultManager.Register("R", new Module("2", typeof(VM2).FullName, null));
            Assert.Throws<ModuleInjectionException>(() => ModuleManager.DefaultManager.Inject("R", "2", "1"),
                "This ViewModel does not support the ISupportParameter interface.");
        }
        [Test]
        public void ViewModelConstructor() {
            ContentControl c = new ContentControl();
            UIRegion.SetRegion(c, "R");

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("R", new Module("2", typeof(VM2).FullName, null));
            Assert.AreEqual(true, ModuleManager.DefaultManager.GetRegion("R").GetViewModel("2") is IPOCOViewModel);

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("R", new Module("3", typeof(VM3).FullName, null));
            Assert.AreEqual(true, ModuleManager.DefaultManager.GetRegion("R").GetViewModel("3") is IPOCOViewModel);
            Assert.AreEqual(true, ModuleManager.DefaultManager.GetRegion("R").GetViewModel("3") is VM2);

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("R", new Module("4", typeof(VM4).FullName, null));
            Assert.AreEqual("DefaultCtor", ((VM4)ModuleManager.DefaultManager.GetRegion("R").GetViewModel("4")).Value);

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("R", new Module("5", typeof(VM5).FullName, null));
            Assert.AreEqual("A", ((VM5)ModuleManager.DefaultManager.GetRegion("R").GetViewModel("5")).Value);

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("R", new Module("6", typeof(VM6).FullName, null));
            Assert.AreEqual("A", ((VM6)ModuleManager.DefaultManager.GetRegion("R").GetViewModel("6")).Value);

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("R", new Module("7", typeof(VM7).FullName, null));
            Assert.AreEqual("A", ((VM7)ModuleManager.DefaultManager.GetRegion("R").GetViewModel("7")).Value);
            Assert.AreEqual(true, ModuleManager.DefaultManager.GetRegion("R").GetViewModel("7") is IPOCOViewModel);

            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("R", new Module("8", typeof(VM8).FullName, null));
            Assert.AreEqual("A", ((VM8)ModuleManager.DefaultManager.GetRegion("R").GetViewModel("8")).Value);
            Assert.AreEqual(true, ModuleManager.DefaultManager.GetRegion("R").GetViewModel("8") is IPOCOViewModel);
        }
        [Test]
        public void PassingParameter() {
            Grid c = new Grid();
            UIRegion.SetRegion(c, "R");

            ModuleManager.DefaultManager.Register("R", new Module("1", () => new VMISupportParameter()));
            ModuleManager.DefaultManager.Register("R", new Module("2", typeof(VMISupportParameter).FullName, null));
            ModuleManager.DefaultManager.RegisterOrInjectOrNavigate("R", new Module("3", () => new VMISupportParameter()), "3");
            ModuleManager.DefaultManager.Inject("R", "1", "1");
            ModuleManager.DefaultManager.InjectOrNavigate("R", "2", "2");
            ModuleManager.DefaultManager.InjectOrNavigate("R", "2", "3");
            Assert.AreEqual("1", ((ISupportParameter)ModuleManager.DefaultManager.GetRegion("R").GetViewModel("1")).Parameter);
            Assert.AreEqual("2", ((ISupportParameter)ModuleManager.DefaultManager.GetRegion("R").GetViewModel("2")).Parameter);
            Assert.AreEqual("3", ((ISupportParameter)ModuleManager.DefaultManager.GetRegion("R").GetViewModel("3")).Parameter);
        }

        public class VM1 {
            public VM1(string value) { }
        }
        [POCOViewModel]
        public class VM2 {
        }
        public class VM3 : VM2 {
        }
        public class VM4 {
            public string Value;
            public VM4(string value = "DefaultCtor") {
                Value = value;
            }
        }
        public class VM5 {
            public string Value;
            public VM5() { Value = "A"; }
            public VM5(string value = null) { Value = value; }
        }
        public class VM6 {
            public string Value;
            public VM6() { Value = "A"; }
            public VM6(string value = null) { Value = value; }
        }
        [POCOViewModel]
        public class VM7 {
            public string Value;
            public VM7(string value = null) { Value = value; }
            public VM7() { Value = "A"; }
        }
        [POCOViewModel]
        public class VM8 {
            public string Value;
            public VM8() { Value = "A"; }
            public VM8(string value = null) { Value = value; }
        }
        public class VMISupportParameter : ISupportParameter {
            public object Parameter { get; set; }
        }
    }
}
