using DevExpress.Mvvm.UI.Interactivity;
using NUnit.Framework;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevExpress.Mvvm.UI.ModuleInjection.Tests {
    [TestFixture]
    public class ViewInjectionManagerTests : BaseWpfFixture {
        public class TestInjectionView1 : Grid { }
        public class TestInjectionView2 : Grid { }

        class VM {
            public int NavigatedCount { get; private set; }
            public int NavigatedAwayCount { get; private set; }
            public int ViewModelClosingCount { get; private set; }
            public bool CancelViewModelClosing { get; set; }
            public VM() {
                NavigatedCount = 0;
                NavigatedAwayCount = 0;
                ViewModelClosingCount = 0;
                CancelViewModelClosing = false;
                ViewInjectionManager.Default.RegisterNavigatedEventHandler(this, () => NavigatedCount++);
                ViewInjectionManager.Default.RegisterNavigatedAwayEventHandler(this, () => NavigatedAwayCount++);
                ViewInjectionManager.Default.RegisterViewModelClosingEventHandler(this, x => {
                    ViewModelClosingCount++;
                    x.Cancel = CancelViewModelClosing;
                });
            }
        }

        [Test]
        public void DefaultViewInjectionManager() {
            var _default = ViewInjectionManager.Default;
            Assert.IsNotNull(ViewInjectionManager.Default);
            var _custom = new ViewInjectionManager(ViewInjectionMode.Default);
            ViewInjectionManager.Default = _custom;
            Assert.AreSame(_custom, ViewInjectionManager.Default);
            ViewInjectionManager.Default = null;
            Assert.AreSame(_default, ViewInjectionManager.Default);
        }
        [Test, Asynchronous]
        public void GetService() {
            Grid container = new Grid();
            Window.Content = container;

            ViewInjectionService service1 = new ViewInjectionService();
            ContentPresenter target1 = new ContentPresenter();
            Interactivity.Interaction.GetBehaviors(target1).Add(service1);
            Assert.IsNull(ViewInjectionManager.Default.GetService(service1.RegionName));

            ViewInjectionService service2 = new ViewInjectionService() { RegionName = "region2" };
            ContentPresenter target2 = new ContentPresenter();
            Interactivity.Interaction.GetBehaviors(target2).Add(service2);
            Assert.IsNull(ViewInjectionManager.Default.GetService(service2.RegionName));

            ViewInjectionService service3 = new ViewInjectionService() { RegionName = "region3" };
            ContentPresenter target3 = new ContentPresenter();
            Interactivity.Interaction.GetBehaviors(target3).Add(service3);

            EnqueueShowWindow();
            EnqueueCallback(() => {
                container.Children.Add(target1);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.IsNull(ViewInjectionManager.Default.GetService(service1.RegionName));
                container.Children.Add(target2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(service2, ViewInjectionManager.Default.GetService("region2"));
                container.Children.Add(target3);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(service2, ViewInjectionManager.Default.GetService("region2"));
                Assert.AreSame(service3, ViewInjectionManager.Default.GetService("region3"));
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void RegisterUnregister() {
            Grid container = new Grid();
            Window.Content = container;

            ViewInjectionService service1 = new ViewInjectionService() { RegionName = "region1" };
            ContentPresenter target1 = new ContentPresenter();
            container.Children.Add(target1);
            Interactivity.Interaction.GetBehaviors(target1).Add(service1);

            ViewInjectionService service2 = new ViewInjectionService() { RegionName = "region2" };
            ContentPresenter target2 = new ContentPresenter();
            container.Children.Add(target2);
            Interactivity.Interaction.GetBehaviors(target2).Add(service2);

            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreSame(service1, ViewInjectionManager.Default.GetService("region1"));
                Assert.AreSame(service2, ViewInjectionManager.Default.GetService("region2"));
                Interactivity.Interaction.GetBehaviors(target1).Remove(service1);
                container.Children.Remove(target2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.IsNull(ViewInjectionManager.Default.GetService("region1"));
                Assert.IsNull(ViewInjectionManager.Default.GetService("region2"));
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void CannotRegisterTwoRegions() {
            Grid container = new Grid();
            Window.Content = container;
            ViewInjectionService service1 = new ViewInjectionService() { RegionName = "region1" };
            ContentPresenter target1 = new ContentPresenter();
            container.Children.Add(target1);
            Interactivity.Interaction.GetBehaviors(target1).Add(service1);

            ViewInjectionService service2 = new ViewInjectionService() { RegionName = "region1" };
            ContentPresenter target2 = new ContentPresenter();
            container.Children.Add(target2);

            EnqueueShowWindow();
            EnqueueCallback(() => {
                AssertHelper.AssertThrows<InvalidOperationException>(() => {
                    Interactivity.Interaction.GetBehaviors(target2).Add(service2);
                }, x => Assert.AreEqual("Cannot register services with the same RegionName", x.Message));
            });
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
        public void InjectRemove() {
            var refs = InjectRemove_Core();
            EnqueueLastCallback(() => {
                MemoryLeaksHelper.CollectOptional(refs);
                MemoryLeaksHelper.EnsureCollected(refs);
            });
        }
        WeakReference[] InjectRemove_Core() {
            Grid container = new Grid();
            Window.Content = container;

            object vm1 = null;
            object vm2 = null;
            object vm3 = null;
            ViewInjectionManager.Default.Inject("region1", null, () => vm1 = new object(), "TestInjectionView1");
            ViewInjectionManager.Default.Inject("region2", null, () => vm2 = new object());

            ViewInjectionService service1 = new ViewInjectionService() { RegionName = "region1" };
            ContentPresenter target1 = new ContentPresenter();
            container.Children.Add(target1);
            Interactivity.Interaction.GetBehaviors(target1).Add(service1);

            ViewInjectionService service2 = new ViewInjectionService() { RegionName = "region2" };
            ContentPresenter target2 = new ContentPresenter();
            container.Children.Add(target2);
            Interactivity.Interaction.GetBehaviors(target2).Add(service2);

            Assert.IsNull(vm1);
            Assert.IsNull(vm2);
            Assert.IsNull(vm3);

            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreSame(vm1, target1.Content);
                Assert.AreSame(vm2, target2.Content);

                ViewInjectionManager.Default.Inject("region2", null, () => vm3 = new object(), typeof(TestInjectionView1));
                Assert.AreSame(vm1, target1.Content);
                Assert.AreSame(vm2, target2.Content);
                ViewInjectionManager.Default.Navigate("region2", vm3);
                Assert.AreSame(vm3, target2.Content);
                Assert.AreEqual(2, ((IViewInjectionService)service2).ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                ViewInjectionManager.Default.Remove("region1", vm1);
                ViewInjectionManager.Default.Remove("region2", vm2);
                ViewInjectionManager.Default.Remove("region2", vm3);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(0, ViewInjectionManager.Default.GetService("region1").ViewModels.Count());
                Assert.AreEqual(0, ViewInjectionManager.Default.GetService("region2").ViewModels.Count());
            });
            EnqueueTestComplete();


            WeakReference target1Reference = new WeakReference(target1);
            WeakReference target2Reference = new WeakReference(target2);
            WeakReference service1Reference = new WeakReference(service1);
            WeakReference service2Reference = new WeakReference(service2);
            WeakReference vm1Reference = new WeakReference(vm1);
            WeakReference vm2Reference = new WeakReference(vm2);
            WeakReference vm3Reference = new WeakReference(vm3);
            Interaction.GetBehaviors(target1).Remove(service1);
            Interaction.GetBehaviors(target2).Remove(service2);
            Window.Content = null;
            return new WeakReference[] { target1Reference, target2Reference, service1Reference, service2Reference, vm1Reference, vm2Reference, vm3Reference };
        }
        [Test, Asynchronous]
        public void Navigate_SimpleTest() {
            var refs = Navigate_SimpleTest_Core();
            EnqueueLastCallback(() => {
                MemoryLeaksHelper.CollectOptional(refs);
                MemoryLeaksHelper.EnsureCollected(refs);
            });
        }
        WeakReference[] Navigate_SimpleTest_Core() {
            TabControl target = new TabControl();
            object vm1 = new object();
            object vm2 = new object();
            object vm3 = new object();
            ViewInjectionManager.Default.Inject("region1", null, () => vm1);
            ViewInjectionManager.Default.Inject("region1", null, () => vm2);
            ViewInjectionManager.Default.Navigate("region1", vm2);
            ViewInjectionService service = new ViewInjectionService() { RegionName = "region1" };
            Interaction.GetBehaviors(target).Add(service);
            Window.Content = target;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(2, target.Items.Count);
                Assert.AreSame(vm2, target.SelectedItem);
                Assert.AreEqual(1, target.SelectedIndex);

                ViewInjectionManager.Default.Navigate("region1", vm3);
                ViewInjectionManager.Default.Inject("region1", null, () => vm3);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(3, target.Items.Count);
                Assert.AreSame(vm3, target.SelectedItem);
                Assert.AreEqual(2, target.SelectedIndex);
            });
            EnqueueTestComplete();

            WeakReference targetReference = new WeakReference(target);
            WeakReference serviceReference = new WeakReference(service);
            WeakReference vm1Reference = new WeakReference(vm1);
            WeakReference vm2Reference = new WeakReference(vm2);
            WeakReference vm3Reference = new WeakReference(vm3);
            Interaction.GetBehaviors(target).Remove(service);
            Window.Content = null;
            return new WeakReference[] { targetReference, serviceReference, vm1Reference, vm2Reference, vm3Reference };
        }
        [Test, Asynchronous]
        public void NavigatedEvent_NavigatedAwayEvent() {
            var refs = NavigatedEvent_NavigatedAwayEvent_Core();
            EnqueueLastCallback(() => {
                MemoryLeaksHelper.CollectOptional(refs);
                MemoryLeaksHelper.EnsureCollected(refs);
            });
        }
        WeakReference[] NavigatedEvent_NavigatedAwayEvent_Core() {
            TabControl target = new TabControl();
            VM vm1 = new VM();
            VM vm2 = new VM();
            VM vm3 = new VM();
            ViewInjectionManager.Default.Inject("region1", null, () => vm1);
            ViewInjectionManager.Default.Inject("region1", null, () => vm2);
            ViewInjectionManager.Default.Navigate("region1", vm2);
            ViewInjectionService service = new ViewInjectionService() { RegionName = "region1" };
            Interaction.GetBehaviors(target).Add(service);
            Window.Content = target;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(1, vm1.NavigatedCount);
                Assert.AreEqual(1, vm1.NavigatedAwayCount);
                Assert.AreEqual(1, vm2.NavigatedCount);
                Assert.AreEqual(0, vm2.NavigatedAwayCount);
                Assert.AreEqual(0, vm3.NavigatedCount);
                Assert.AreEqual(0, vm3.NavigatedAwayCount);

                ViewInjectionManager.Default.Navigate("region1", vm3);
                ViewInjectionManager.Default.Inject("region1", null, () => vm3);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, vm1.NavigatedCount);
                Assert.AreEqual(1, vm1.NavigatedAwayCount);
                Assert.AreEqual(1, vm2.NavigatedCount);
                Assert.AreEqual(1, vm2.NavigatedAwayCount);
                Assert.AreEqual(1, vm3.NavigatedCount);
                Assert.AreEqual(0, vm3.NavigatedAwayCount);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                target.SelectedItem = vm1;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, vm1.NavigatedCount);
                Assert.AreEqual(1, vm1.NavigatedAwayCount);
                Assert.AreEqual(1, vm2.NavigatedCount);
                Assert.AreEqual(1, vm2.NavigatedAwayCount);
                Assert.AreEqual(1, vm3.NavigatedCount);
                Assert.AreEqual(1, vm3.NavigatedAwayCount);
            });
            EnqueueTestComplete();

            WeakReference targetReference = new WeakReference(target);
            WeakReference serviceReference = new WeakReference(service);
            WeakReference vm1Reference = new WeakReference(vm1);
            WeakReference vm2Reference = new WeakReference(vm2);
            WeakReference vm3Reference = new WeakReference(vm3);
            Interaction.GetBehaviors(target).Remove(service);
            Window.Content = null;
            return new WeakReference[] { targetReference, serviceReference, vm1Reference, vm2Reference, vm3Reference };
        }
        [Test, Asynchronous]
        public void ViewModelClosingEvent() {
            var refs = ViewModelClosingEvent_Core();
            EnqueueLastCallback(() => {
                MemoryLeaksHelper.CollectOptional(refs);
                MemoryLeaksHelper.EnsureCollected(refs);
            });
        }
        WeakReference[] ViewModelClosingEvent_Core() {
            TabControl target = new TabControl();
            VM vm1 = new VM();
            VM vm2 = new VM();
            VM vm3 = new VM();
            ViewInjectionManager.Default.Inject("region1", null, () => vm1);
            ViewInjectionManager.Default.Inject("region1", null, () => vm2);
            ViewInjectionManager.Default.Inject("region1", null, () => vm3);
            ViewInjectionService service = new ViewInjectionService() { RegionName = "region1" };
            Interaction.GetBehaviors(target).Add(service);
            Window.Content = target;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreSame(vm1, target.SelectedItem);
                ViewInjectionManager.Default.Remove("region1", vm1);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm2, target.SelectedItem);
                Assert.AreEqual(1, vm1.ViewModelClosingCount);
                vm2.CancelViewModelClosing = true;
                ViewInjectionManager.Default.Remove("region1", vm2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm2, target.SelectedItem);
                Assert.AreEqual(1, vm2.ViewModelClosingCount);
            });
            EnqueueTestComplete();

            WeakReference targetReference = new WeakReference(target);
            WeakReference serviceReference = new WeakReference(service);
            WeakReference vm1Reference = new WeakReference(vm1);
            WeakReference vm2Reference = new WeakReference(vm2);
            WeakReference vm3Reference = new WeakReference(vm3);
            Interaction.GetBehaviors(target).Remove(service);
            Window.Content = null;
            return new WeakReference[] { targetReference, serviceReference, vm1Reference, vm2Reference, vm3Reference };
        }

#if !DXCORE3
        [Test, Asynchronous]
        public void PersistentMode_InjectRemove() {
            object vm = new object();
            Func<object> vmFactory = () => vm;
            WeakReference refVM = new WeakReference(vm);
            var refs = PersistentMode_InjectRemove_Core(vmFactory, vm);
            vm = null;
            vmFactory = null;
            EnqueueLastCallback(() => {
                Window.Content = null;
                MemoryLeaksHelper.CollectOptional(refs);
                MemoryLeaksHelper.EnsureCollected(refs);

                MemoryLeaksHelper.CollectOptional(refVM);
                MemoryLeaksHelper.EnsureCollected(refVM);
            });
        }
        WeakReference[] PersistentMode_InjectRemove_Core(Func<object> vmFactory, object vm) {
            Grid container = new Grid();
            Window.Content = container;

            ViewInjectionManager.PersistentManager.Inject("region", null, vmFactory, typeof(TestInjectionView1));

            ViewInjectionService service1 = new ViewInjectionService() { RegionName = "region", ViewInjectionManager = ViewInjectionManager.PersistentManager };
            ContentPresenter target1 = new ContentPresenter();
            container.Children.Add(target1);
            Interactivity.Interaction.GetBehaviors(target1).Add(service1);

            ViewInjectionService service2 = new ViewInjectionService() { RegionName = "region", ViewInjectionManager = ViewInjectionManager.PersistentManager };
            ItemsControl target2 = new ItemsControl();

            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreSame(vm, target1.Content);
                Assert.AreEqual(1, ((IViewInjectionService)service1).ViewModels.Count());
                container.Children.Remove(target1);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(0, ((IViewInjectionService)service1).ViewModels.Count());
                container.Children.Add(target1);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm, target1.Content);
                Assert.AreEqual(1, ((IViewInjectionService)service1).ViewModels.Count());
                container.Children.Remove(target1);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Interactivity.Interaction.GetBehaviors(target2).Add(service2);
                container.Children.Add(target2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm, target2.Items[0]);
                Assert.AreEqual(0, ((IViewInjectionService)service1).ViewModels.Count());
                Assert.AreEqual(1, ((IViewInjectionService)service2).ViewModels.Count());
                Interaction.GetBehaviors(target1).Remove(service1);
                Interaction.GetBehaviors(target2).Remove(service2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                service1.ViewInjectionManager = null;
                service2.ViewInjectionManager = null;
                Assert.AreEqual(0, ((IViewInjectionService)service1).ViewModels.Count());
                Assert.AreEqual(0, ((IViewInjectionService)service2).ViewModels.Count());
            });
            EnqueueTestComplete();
            WeakReference target1Reference = new WeakReference(target1);
            WeakReference target2Reference = new WeakReference(target2);
            WeakReference service1Reference = new WeakReference(service1);
            WeakReference service2Reference = new WeakReference(service2);
            return new WeakReference[] { target1Reference, target2Reference, service1Reference, service2Reference };
        }
#endif
    }

    public class TabView : Grid {
        public TabView() {
            ViewInjectionService service = new ViewInjectionService();
            BindingOperations.SetBinding(service, ViewInjectionService.RegionNameProperty, new Binding("RegionName"));
            Interaction.GetBehaviors(this).Add(service);
        }
    }
    public class TabContentView : Grid { }
    [TestFixture]
    public class ViewInjectionServiceTests : BaseWpfFixture {
        [Test]
        public void NullServiceTest() {
            IViewInjectionService iService = null;
            AssertHelper.AssertThrows<ArgumentNullException>(() => { iService.Inject(null, null); });
            AssertHelper.AssertThrows<ArgumentNullException>(() => { iService.Inject(null, null, string.Empty); });
            AssertHelper.AssertThrows<ArgumentNullException>(() => { iService.Inject(null, null, typeof(string)); });
            AssertHelper.AssertThrows<ArgumentNullException>(() => { iService.GetViewModel(null); });
        }
        [Test, Asynchronous]
        public void ViewModelKeyTest() {
            ViewInjectionService service = new ViewInjectionService();
            IViewInjectionService iService = service;
            ContentControl target = new ContentControl();
            Interaction.GetBehaviors(target).Add(service);
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = target;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Inject(null, null);
                iService.Inject(null, vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
                Assert.AreSame(vm1, iService.ViewModels.ElementAt(0));
                Assert.AreEqual(vm1, iService.GetKey(vm1));
                Assert.AreSame(vm1, iService.GetViewModel(vm1));

                AssertHelper.AssertThrows<InvalidOperationException>(() => iService.Inject(null, vm1),
                    x => Assert.AreEqual("A view model with the same key already exists in the ViewInjectionService region.", x.Message));
                service.RegionName = "Test";
                AssertHelper.AssertThrows<InvalidOperationException>(() => iService.Inject(null, vm1),
                    x => Assert.AreEqual("A view model with the same key already exists in the Test region.", x.Message));

                iService.Inject("New", vm2);
                Assert.AreEqual(2, iService.ViewModels.Count());
                Assert.AreSame(vm2, iService.ViewModels.ElementAt(1));
                Assert.AreEqual("New", iService.GetKey(vm2));
                Assert.AreSame(vm2, iService.GetViewModel("New"));

                iService.Remove(vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
                iService.Remove(vm2);
                Assert.AreEqual(0, iService.ViewModels.Count());
            });
            EnqueueTestComplete();
        }

        class VMBase : ViewModelBase {
            public int NavigatedCount { get; private set; }
            public int NavigatedAwayCount { get; private set; }
            public int ViewModelClosingCount { get; private set; }
            public bool CancelViewModelClosing { get; set; }
            public VMBase() {
                NavigatedCount = 0;
                NavigatedAwayCount = 0;
                ViewModelClosingCount = 0;
                CancelViewModelClosing = false;
                ViewInjectionManager.Default.RegisterNavigatedEventHandler(this, () => NavigatedCount++);
                ViewInjectionManager.Default.RegisterNavigatedAwayEventHandler(this, () => NavigatedAwayCount++);
                ViewInjectionManager.Default.RegisterViewModelClosingEventHandler(this, x => {
                    ViewModelClosingCount++;
                    x.Cancel = CancelViewModelClosing;
                });
            }
        }
        class TabViewModel : VMBase {
            public string RegionName {
                get { return GetProperty(() => RegionName); }
                set { SetProperty(() => RegionName, value); }
            }
        }
        class TabContentViewModel : VMBase { }

        [Test, Asynchronous]
        public void ComplexTest1() {
            TabViewModel tab1VM = null;
            TabViewModel tab2VM = null;
            TabContentViewModel tab1ContentVM = null;
            TabContentViewModel tab2ContentVM = null;
            ViewInjectionManager.Default.Inject("mainRegion", "tab1", () => tab1VM = new TabViewModel() { RegionName = "tab1ContentRegion" }, typeof(TabView));
            ViewInjectionManager.Default.Inject("mainRegion", "tab2", () => tab2VM = new TabViewModel() { RegionName = "tab2ContentRegion" }, typeof(TabView));
            ViewInjectionManager.Default.Inject("tab1ContentRegion", "tab1Content", () => tab1ContentVM = new TabContentViewModel(), typeof(TabContentView));
            ViewInjectionManager.Default.Inject("tab2ContentRegion", "tab2Content", () => tab2ContentVM = new TabContentViewModel(), typeof(TabContentView));

            TabControl tabControl = new TabControl();
            ViewInjectionService mainService = new ViewInjectionService() { RegionName = "mainRegion" };
            Interaction.GetBehaviors(tabControl).Add(mainService);
            Window.Content = tabControl;

            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(1, tab1VM.NavigatedCount);
                Assert.AreEqual(0, tab1ContentVM.NavigatedCount);
                Assert.IsNull(tab2ContentVM);
                LayoutTreeHelper.GetVisualChildren(tabControl).OfType<TabContentView>().First(x => x.DataContext == tab1ContentVM);

                ViewInjectionManager.Default.Navigate("tab2ContentRegion", "tab2Content");
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, tab1VM.NavigatedCount);
                Assert.AreEqual(0, tab1ContentVM.NavigatedCount);
                Assert.IsNull(tab2ContentVM);

                ViewInjectionManager.Default.Navigate("mainRegion", "tab2");
                ViewInjectionManager.Default.Navigate("tab2ContentRegion", "tab2Content");
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, tab1VM.NavigatedCount);
                Assert.AreEqual(1, tab1VM.NavigatedAwayCount);
                Assert.AreEqual(1, tab2VM.NavigatedCount);
                Assert.AreEqual(0, tab1ContentVM.NavigatedCount);
                Assert.AreEqual(0, tab1ContentVM.NavigatedAwayCount);
                Assert.AreEqual(1, tab2ContentVM.NavigatedCount);
                LayoutTreeHelper.GetVisualChildren(tabControl).OfType<TabContentView>().First(x => x.DataContext == tab2ContentVM);
            });
            EnqueueTestComplete();
        }
    }
}