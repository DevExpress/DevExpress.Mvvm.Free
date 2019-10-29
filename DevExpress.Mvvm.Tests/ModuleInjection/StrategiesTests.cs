using DevExpress.Mvvm.ModuleInjection;
using DevExpress.Mvvm.ModuleInjection.Native;
using DevExpress.Mvvm.UI.Interactivity;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DevExpress.Mvvm.UI.ModuleInjection.Tests {
    [TestFixture]
    public class StrategyManagerTests : BaseWpfFixture {
        [Test]
        public void DefaultStrategyManager() {
            var _default = StrategyManager.Default;
            Assert.IsNotNull(StrategyManager.Default);
            var _custom = new StrategyManager();
            StrategyManager.Default = _custom;
            Assert.AreSame(_custom, StrategyManager.Default);
            StrategyManager.Default = null;
            Assert.AreSame(_default, StrategyManager.Default);
        }
        [Test]
        public void SelectStrategy() {
            Assert.IsTrue(StrategyManager.Default.CreateStrategy(new ContentPresenter()) is ContentPresenterStrategy<ContentPresenter, ContentPresenterWrapper>);
            Assert.IsTrue(StrategyManager.Default.CreateStrategy(new ContentControl()) is ContentPresenterStrategy<ContentControl, ContentControlWrapper>);
            Assert.IsTrue(StrategyManager.Default.CreateStrategy(new HeaderedContentControl()) is ContentPresenterStrategy<ContentControl, ContentControlWrapper>);
            Assert.IsTrue(StrategyManager.Default.CreateStrategy(new StackPanel()) is PanelStrategy<Panel, PanelWrapper>);
            Assert.IsTrue(StrategyManager.Default.CreateStrategy(new TabControl()) is SelectorStrategy<TabControl, TabControlWrapper>);
            AssertHelper.AssertThrows<ModuleInjectionException>(() => {
                StrategyManager.Default.CreateStrategy(new ViewInjectionService());
            }, x => Assert.AreEqual("Cannot find an appropriate strategy for the ViewInjectionService container type.", x.Message));
        }
        [Test]
        public void CustomStrategy() {
            IStrategyManager _defaultSM;
            _defaultSM = StrategyManager.Default;
            StrategyManager.Default = new StrategyManager();
            ((StrategyManager)StrategyManager.Default).RegisterDefaultStrategies();

            StrategyManager.Default.RegisterStrategy<HeaderedContentControl, CustomStrategy1>();
            Assert.IsTrue(StrategyManager.Default.CreateStrategy(new HeaderedContentControl()) is CustomStrategy1);

            StrategyManager.Default.RegisterStrategy<ContentControl, CustomStrategy2>();
            Assert.IsTrue(StrategyManager.Default.CreateStrategy(new ContentControl()) is CustomStrategy2);

            StrategyManager.Default = null;
        }

        class HeaderedContentControlWrapper : ContentControlWrapper, IContentPresenterWrapper<HeaderedContentControl> {
            public new HeaderedContentControl Target {
                get { return (HeaderedContentControl)base.Target; }
                set { base.Target = value; }
            }
        }
        class CustomStrategy1 : ContentPresenterStrategy<HeaderedContentControl, HeaderedContentControlWrapper> { }
        class CustomStrategy2 : ContentPresenterStrategy<ContentControl, ContentControlWrapper> { }
    }

    [TestFixture]
    public class StrategyPanelTests : StrategyBasePanelTests<Panel> {
        protected override void Init(out IViewInjectionService service, out IPanelWrapper<Panel> target) {
            Grid control = new Grid();
            var s = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(s);
            target = new PanelWrapper() { Target = control };
            service = s;
        }
        protected override void Init(string regionName, out IPanelWrapper<Panel> target) {
            Grid control = new Grid();
            UIRegion.SetRegion(control, regionName);
            target = new PanelWrapper() { Target = control };
        }
        [Test]
        public void Test() { }
        [Test]
        public void ClearTest() {
            IPanelWrapper<Panel> target;
            Init("region", out target);
            Manager.Register("region", new Module("A", () => new object(), typeof(View1_BaseTests)));
            Manager.Register("region", new Module("B", () => new object(), typeof(View1_BaseTests)));

            Manager.Inject("region", "A");
            Manager.Inject("region", "B");
            Show(target);
            Assert.AreEqual(2, target.Children.OfType<object>().Count());
            Manager.Clear("region");
            Assert.AreEqual(0, target.Children.OfType<object>().Count());
        }
    }
    [TestFixture]
    public class StrategyContentPresenterTests : StrategyBaseContentPresenterTests<ContentPresenter> {
        protected override void Init(out IViewInjectionService service, out IContentPresenterWrapper<ContentPresenter> target) {
            ContentPresenter control = new ContentPresenter();
            var s = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(s);
            target = new ContentPresenterWrapper() { Target = control };
            service = s;
        }
        protected override void Init(string regionName, out IContentPresenterWrapper<ContentPresenter> target) {
            ContentPresenter control = new ContentPresenter();
            UIRegion.SetRegion(control, regionName);
            target = new ContentPresenterWrapper() { Target = control };
        }
        [Test]
        public void Test() { }
    }
    [TestFixture]
    public class StrategyContentControlTests : StrategyBaseContentPresenterTests<ContentControl> {
        protected override void Init(out IViewInjectionService service, out IContentPresenterWrapper<ContentControl> target) {
            ContentControl control = new ContentControl();
            var s = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(s);
            target = new ContentControlWrapper() { Target = control };
            service = s;
        }
        protected override void Init(string regionName, out IContentPresenterWrapper<ContentControl> target) {
            ContentControl control = new ContentControl();
            UIRegion.SetRegion(control, regionName);
            target = new ContentControlWrapper() { Target = control };
        }
        [Test]
        public void Test() { }
    }
    [TestFixture]
    public class StrategyItemsControlTests : StrategyBaseItemsControlTests<ItemsControl> {
        protected override void Init(out IViewInjectionService service, out IItemsControlWrapper<ItemsControl> target) {
            ItemsControl control = new ItemsControl();
            var s = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(s);
            target = new ItemsControlWrapper() { Target = control };
            service = s;
        }
        protected override void Init(string regionName, out IItemsControlWrapper<ItemsControl> target) {
            ItemsControl control = new ItemsControl();
            UIRegion.SetRegion(control, regionName);
            target = new ItemsControlWrapper() { Target = control };
        }
        protected override IEnumerable<FrameworkElement> GetVisualChildren(IItemsControlWrapper<ItemsControl> target) {
            return LayoutTreeHelper.GetVisualChildren(target.Target).OfType<ContentPresenter>()
                .SelectMany(x => LayoutTreeHelper.GetVisualChildren(x)).OfType<FrameworkElement>();
        }
        [Test]
        public void Test() { }
        [Test]
        public override void ViewInjection_InjectMethods() {
            ViewInjection_InjectMethodsCore(true);
        }
        [Test]
        public override void ModuleInjection_InjectMethods() {
            ModuleInjection_InjectMethodsCore(true);
        }
    }
    [TestFixture]
    public class StrategySelectorTests : StrategyBaseSelectorTests<Selector> {
        protected override void Init(out IViewInjectionService service, out ISelectorWrapper<Selector> target) {
            TabControl control = new TabControl();
            var s = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(s);
            target = new SelectorWrapper() { Target = control };
            service = s;
        }
        protected override void Init(string regionName, out ISelectorWrapper<Selector> target) {
            TabControl control = new TabControl();
            UIRegion.SetRegion(control, regionName);
            target = new SelectorWrapper() { Target = control };
        }
        protected override IEnumerable<FrameworkElement> GetVisualChildren(IItemsControlWrapper<Selector> target) {
            return LayoutTreeHelper.GetVisualChildren(target.Target).OfType<ContentPresenter>()
                .SelectMany(x => LayoutTreeHelper.GetVisualChildren(x)).OfType<FrameworkElement>();
        }
        [Test]
        public void Test() { }
    }
    [TestFixture]
    public class StrategyTabControlTests : StrategyBaseTabControlTests<TabControl> {
        protected override void Init(out IViewInjectionService service, out ISelectorWrapper<TabControl> target) {
            TabControl control = new TabControl();
            var s = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(s);
            target = new TabControlWrapper() { Target = control };
            service = s;
        }
        protected override void Init(string regionName, out ISelectorWrapper<TabControl> target) {
            TabControl control = new TabControl();
            UIRegion.SetRegion(control, regionName);
            target = new TabControlWrapper() { Target = control };
        }
        protected override FrameworkElement GetContentHost(ISelectorWrapper<TabControl> target) {
            return LayoutTreeHelper.GetVisualChildren(target.Target).OfType<FrameworkElement>().First(x => x.Name == "PART_SelectedContentHost");
        }
        protected override FrameworkElement GetHeaderPanel(ISelectorWrapper<TabControl> target) {
            return LayoutTreeHelper.GetVisualChildren(target.Target).OfType<FrameworkElement>().First(x => x.Name == "HeaderPanel");
        }
        protected override void SetHeaderTemplate(ISelectorWrapper<TabControl> target, DataTemplate template) {
            var tabControl = (TabControl)target.Target;
            tabControl.ItemTemplate = template;
        }

        [Test]
        public void Test() { }
    }

    [TestFixture]
    public class StrategyWindowTests : BaseWpfFixture {
        static IWindowStrategy GetStrategy(IUIWindowRegion service, object viewModel) {
            var t = service.GetType();
            var m = t.GetMethod("GetStrategy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return (IWindowStrategy)m.Invoke(service, new object[] { viewModel });
        }

        public IModuleManager Manager { get { return ModuleManager.DefaultManager; } }
        public IModuleWindowManager WindowManager { get { return ModuleManager.DefaultWindowManager; } }
        protected override void SetUpCore() {
            base.SetUpCore();
            InjectionTestHelper.SetUp(typeof(StrategyWindowTests).Assembly);
        }
        protected override void TearDownCore() {
            InjectionTestHelper.TearDown();
            base.TearDownCore();
        }

        [Test]
        public void DefaultValues() {
            UIWindowRegion service = new UIWindowRegion();
            Assert.AreEqual(null, service.RegionName);
            Assert.AreEqual(true, service.SetWindowOwner);
            Assert.AreEqual(WindowShowMode.Default, service.WindowShowMode);
            Assert.AreEqual(WindowStartupLocation.CenterScreen, service.WindowStartupLocation);
            Assert.AreEqual(false, service.IsMainWindow);
        }
        [Test]
        public void InjectWindows() {
            UIWindowRegion service = new UIWindowRegion() { RegionName = "region" };
            ModuleManager.DefaultImplementation.GetRegionImplementation("region").RegisterUIRegion(service);
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);

            object vm1 = null;
            object vm2 = null;
            Manager.Register("region", new Module("1", () => vm1 = new object()));
            Manager.Register("region", new Module("2", () => vm2 = new object()));

            Manager.Inject("region", "1");
            DispatcherHelper.DoEvents();
            var strategy1 = GetStrategy(service, vm1);
            Window w1 = (Window)strategy1.Owner.Target;
            Assert.AreEqual(true, w1 is Window);
            Assert.AreEqual(true, w1.IsActive);

            Manager.Inject("region", "2");
            DispatcherHelper.DoEvents();
            var strategy2 = GetStrategy(service, vm2);
            Window w2 = (Window)strategy2.Owner.Target;
            Assert.AreEqual(false, w1.IsActive);
            Assert.AreEqual(true, w2.IsActive);

            w1.Close();
            DispatcherHelper.DoEvents();
            Assert.AreEqual(null, Manager.GetRegion("region").GetViewModel("1"));
            serviceHelper.AssertViewModelRemoving(1);
            serviceHelper.AssertViewModelRemoved(1);

            Manager.Remove("region", "2");
            DispatcherHelper.DoEvents();
            Assert.AreEqual(false, w2.IsVisible);
            serviceHelper.AssertViewModelRemoving(2);
            serviceHelper.AssertViewModelRemoved(2);
            serviceHelper.Dispose();
        }
        [Test]
        public void ActiveWindow() {
            UIWindowRegion service = new UIWindowRegion() { RegionName = "region" };
            ModuleManager.DefaultImplementation.GetRegionImplementation("region").RegisterUIRegion(service);
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);

            object vm1 = null;
            object vm2 = null;
            Manager.Register("region", new Module("1", () => vm1 = new object()));
            Manager.Register("region", new Module("2", () => vm2 = new object()));

            Manager.Inject("region", "1");
            Manager.Inject("region", "2");
            DispatcherHelper.DoEvents();
            var strategy1 = GetStrategy(service, vm1);
            var strategy2 = GetStrategy(service, vm2);
            Window w1 = (Window)strategy1.Owner.Target;
            Window w2 = (Window)strategy2.Owner.Target;
            Assert.AreEqual(false, w1.IsActive);
            Assert.AreEqual(true, w2.IsActive);
            Assert.AreEqual(vm2, service.SelectedViewModel);
            serviceHelper.AssertSelectionChanged(2);

            w1.Activate();
            DispatcherHelper.DoEvents();
            Assert.AreEqual(true, w1.IsActive);
            Assert.AreEqual(false, w2.IsActive);
            Assert.AreEqual(vm1, service.SelectedViewModel);
            serviceHelper.AssertSelectionChanged(3);

            ((IUIRegion)service).SelectedViewModel = vm2;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(false, w1.IsActive);
            Assert.AreEqual(true, w2.IsActive);
            Assert.AreEqual(vm2, service.SelectedViewModel);
            serviceHelper.AssertSelectionChanged(4);

            Manager.Clear("region");
            DispatcherHelper.DoEvents();
            Assert.AreEqual(false, w1.IsLoaded);
            Assert.AreEqual(false, w2.IsLoaded);
            serviceHelper.Dispose();
        }
#if !DXCORE3
        [Test, Retry(3)]
        public void ClosingWindow() {
            UIWindowRegion service = new UIWindowRegion() { RegionName = "region" };
            ModuleManager.DefaultImplementation.GetRegionImplementation("region").RegisterUIRegion(service);
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);

            object vm1 = null;
            object vm2 = null;
            Manager.Register("region", new Module("1", () => vm1 = new object()));
            Manager.Register("region", new Module("2", () => vm2 = new object()));

            Manager.Inject("region", "1");
            Manager.Inject("region", "2");
            Manager.Navigate("region", "1");
            DispatcherHelper.DoEvents();
            var strategy1 = GetStrategy(service, vm1);
            var strategy2 = GetStrategy(service, vm2);
            Window w1 = (Window)strategy1.Owner.Target;
            Window w2 = (Window)strategy2.Owner.Target;
            Assert.AreEqual(true, w1.IsActive);
            Assert.AreEqual(vm1, service.SelectedViewModel);

            serviceHelper.CancelViewModelRemoving = true;
            w1.Close();
            DispatcherHelper.DoEvents();
            serviceHelper.AssertViewModelRemoving(1);
            serviceHelper.AssertViewModelRemoved(0);
            Assert.AreEqual(true, w1.IsActive);
            Assert.AreEqual(vm1, service.SelectedViewModel);

            Manager.Remove("region", "1");
            DispatcherHelper.DoEvents();
            serviceHelper.AssertViewModelRemoving(2);
            serviceHelper.AssertViewModelRemoved(0);
            Assert.AreEqual(true, w1.IsActive);
            Assert.AreEqual(vm1, service.SelectedViewModel);

            Manager.Remove("region", "1", false);
            DispatcherHelper.DoEvents();
            serviceHelper.AssertViewModelRemoving(2);
            serviceHelper.AssertViewModelRemoved(1);
            Assert.AreEqual(vm2, service.SelectedViewModel);

            Manager.Remove("region", "2", false);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(null, service.SelectedViewModel);

            serviceHelper.Dispose();
            Manager.Clear("region");
        }
#endif
        [Test]
        public void SetResultWindow() {
            UIWindowRegion service = new UIWindowRegion() { RegionName = "region" };
            ModuleManager.DefaultImplementation.GetRegionImplementation("region").RegisterUIRegion(service);
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);

            object vm1 = null;
            object vm2 = null;
            Manager.Register("region", new Module("1", () => vm1 = new object()));
            Manager.Register("region", new Module("2", () => vm2 = new object()));

            var t1 = WindowManager.Show("region", "1");
            var t2 = WindowManager.Show("region", "2");
            DispatcherHelper.DoEvents();

            WindowManager.Close("region", "1", MessageBoxResult.OK);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(MessageBoxResult.OK, t1.Result.Result);
            Assert.AreEqual(vm1, t1.Result.ViewModel);

            WindowManager.Close("region", "2", MessageBoxResult.Cancel);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(MessageBoxResult.Cancel, t2.Result.Result);
            Assert.AreEqual(vm2, t2.Result.ViewModel);

            serviceHelper.Dispose();
            Manager.Clear("region");
        }

    }
}