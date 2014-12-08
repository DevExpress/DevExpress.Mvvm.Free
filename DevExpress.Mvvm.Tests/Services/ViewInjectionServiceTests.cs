#if !SILVERLIGHT
using NUnit.Framework;
#else
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Controls.Primitives;
using DevExpress.Mvvm.DataAnnotations;
using System.Reflection;
using DevExpress.Mvvm.UI.ViewInjection;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows.Data;

namespace DevExpress.Mvvm.UI.ViewInjection.Tests {
    public class TestView1 : Grid { }
    public class TestView2 : Grid { }

    [TestFixture]
    public class ViewInjectionManagerTests : BaseWpfFixture {
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
            ViewInjectionManager.Default.Inject("region1", null, () => vm1 = new object(), "TestView1");
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

                ViewInjectionManager.Default.Inject("region2", null, () => vm3 = new object(), typeof(TestView1));
                Assert.AreSame(vm1, target1.Content);
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

            ViewInjectionManager.PersistentManager.Inject("region", null, vmFactory, typeof(TestView1));

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
    }
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
            AssertHelper.AssertThrows<InvalidOperationException>(() => {
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

        #region ContentPresenter and ContentControl
        [Test, Asynchronous]
        public void ContentPresenter_InvalidInitialization() {
            ContentPresenter control = new ContentPresenter() { Content = new object() };
            ContentPresenter_InvalidInitializationTest_Core(control, "It is impossible to use ViewInjectionService for the control that has the Content property set.");
        }
        [Test, Asynchronous]
        public void ContentPresenter_InjectMethods() {
            ViewInjectionService service; ContentPresenterWrapper target;
            InitContentPresenterTest(out service, out target);
            ContentPresenter_InjectMethods_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ContentControl_InjectMethods() {
            ViewInjectionService service; ContentControlWrapper target;
            InitContentControlTest(out service, out target);
            ContentPresenter_InjectMethods_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ContentPresenter_InjectWithContentTemplate() {
            ViewInjectionService service; ContentPresenterWrapper target;
            InitContentPresenterTest(out service, out target);
            ContentPresenter_InjectWithContentTemplate_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ContentControl_InjectWithContentTemplate() {
            ViewInjectionService service; ContentControlWrapper target;
            InitContentControlTest(out service, out target);
            ContentPresenter_InjectWithContentTemplate_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ContentPresenter_SelectedViewModel() {
            ViewInjectionService service; ContentPresenterWrapper target;
            InitContentPresenterTest(out service, out target);
            ContentPresenter_SelectedViewModel_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ContentControl_SelectedViewModel() {
            ViewInjectionService service; ContentControlWrapper target;
            InitContentControlTest(out service, out target);
            ContentPresenter_SelectedViewModel_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ContentPresenter_RemoveViewModel() {
            ViewInjectionService service; ContentPresenterWrapper target;
            InitContentPresenterTest(out service, out target);
            ContentPresenter_RemoveViewModelSimpleTest_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ContentControl_RemoveViewModel() {
            ViewInjectionService service; ContentControlWrapper target;
            InitContentControlTest(out service, out target);
            ContentPresenter_RemoveViewModelSimpleTest_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ContentPresenter_CancelRemoveViewModel() {
            ViewInjectionService service; ContentPresenterWrapper target;
            InitContentPresenterTest(out service, out target);
            ContentPresenter_CancelRemoveViewModelTest_Core(service);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ContentControl_CancelRemoveViewModel() {
            ViewInjectionService service; ContentControlWrapper target;
            InitContentControlTest(out service, out target);
            ContentPresenter_CancelRemoveViewModelTest_Core(service);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }

        void InitContentPresenterTest(out ViewInjectionService service, out ContentPresenterWrapper target) {
            ContentPresenter control = new ContentPresenter();
            service = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(service);
            target = new ContentPresenterWrapper() { Target = control };
        }
        void InitContentControlTest(out ViewInjectionService service, out ContentControlWrapper target) {
            ContentControl control = new ContentControl();
            service = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(service);
            target = new ContentControlWrapper() { Target = control };
        }
        void ContentPresenter_InvalidInitializationTest_Core(FrameworkElement target, string exception) {
            ViewInjectionService service = new ViewInjectionService();
            Interaction.GetBehaviors(target).Add(service);
            EnqueueShowWindow();
            EnqueueCallback(() => {
                AssertHelper.AssertThrows<InvalidOperationException>(() => {
                    Window.Content = target;
                }, x => Assert.AreEqual(exception, x.Message));
            });
            EnqueueTestComplete();
        }
        void ContentPresenter_InjectMethods_Core<T>(ViewInjectionService service, IContentPresenterWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Inject(null, vm1);
                Assert.AreSame(vm1, target.Content);
                iService.Inject(null, vm2, typeof(TestView1));
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm2, target.Content);
                Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<TestView1>().First().DataContext);
            });
            EnqueueTestComplete();
        }
        void ContentPresenter_InjectWithContentTemplate_Core<T>(ViewInjectionService service, IContentPresenterWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            target.ContentTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) };
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Inject(null, vm1);
                Assert.AreSame(vm1, target.Content);
                Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<TextBox>().First().DataContext);
                AssertHelper.AssertThrows<InvalidOperationException>(() => {
                    iService.Inject(null, vm2, typeof(TestView1));
                }, x => Assert.AreEqual("ViewInjectionService cannot create view by name or type, because the target control has the ContentTemplate/ContentTemplateSelector property set.", x.Message));
                iService.Inject(null, vm2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm2, target.Content);
                Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<TextBox>().First().DataContext);
            });
            EnqueueTestComplete();
        }
        void ContentPresenter_SelectedViewModel_Core<T>(ViewInjectionService service, IContentPresenterWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            int selectedViewModelChangedCount = 0;
            service.SelectedViewModelChangedCommand = new DelegateCommand(() => selectedViewModelChangedCount++);
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Inject(null, vm1);
                Assert.AreEqual(1, selectedViewModelChangedCount);
                Assert.AreSame(vm1, target.Content);
                iService.Inject(null, vm2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, selectedViewModelChangedCount);
                Assert.AreSame(vm2, target.Content);
                iService.SelectedViewModel = vm1;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(3, selectedViewModelChangedCount);
                Assert.AreSame(vm1, target.Content);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                AssertHelper.AssertThrows<InvalidOperationException>(() => {
                    iService.SelectedViewModel = new object();
                },
                x => Assert.AreEqual("Cannot set the SelectedViewModel property to a value that does not exist in the ViewModels collection. Inject the view model before selecting it.", x.Message));
            });
            EnqueueTestComplete();
        }
        void ContentPresenter_RemoveViewModelSimpleTest_Core<T>(ViewInjectionService service, IContentPresenterWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            int viewModelClosingCommandCount = 0;
            service.ViewModelClosingCommand = new DelegateCommand(() => viewModelClosingCommandCount++);
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Remove(new object());
                iService.Inject(null, vm1);
                Assert.AreSame(vm1, target.Content);
                iService.Inject(null, vm2);
                iService.Remove(new object());
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm2, target.Content);
                iService.SelectedViewModel = vm1;
                iService.Remove(vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, viewModelClosingCommandCount);
                Assert.IsNull(iService.SelectedViewModel);
                Assert.IsNull(target.Content);
                iService.Inject(null, vm1);
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm1, iService.SelectedViewModel);
                iService.Remove(vm2);
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, viewModelClosingCommandCount);
                Assert.AreSame(vm1, iService.SelectedViewModel);
                iService.Remove(vm1);
                Assert.AreEqual(0, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(3, viewModelClosingCommandCount);
                Assert.IsNull(iService.SelectedViewModel);
            });
            EnqueueTestComplete();
        }
        void ContentPresenter_CancelRemoveViewModelTest_Core(ViewInjectionService service) {
            IViewInjectionService iService = service;
            bool cancel = false;
            service.ViewModelClosingCommand = new DelegateCommand<ViewModelClosingEventArgs>(x => x.Cancel = cancel);
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Inject(null, vm1);
                iService.Inject(null, vm2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                iService.Remove(vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                iService.Inject(null, vm1);
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                cancel = true;
                iService.Remove(vm1);
                iService.Remove(vm2);
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                cancel = false;
                iService.Remove(vm1);
                iService.Remove(vm2);
                Assert.AreEqual(0, iService.ViewModels.Count());
            });
            EnqueueTestComplete();
        }
        void ContentPresenter_TestMemoryReleased() {
            EnqueueLastCallback(() => {
                ViewInjectionService service = Interaction.GetBehaviors((DependencyObject)Window.Content).OfType<ViewInjectionService>().First();
                Interaction.GetBehaviors((DependencyObject)Window.Content).Remove(service);
                WeakReference controlReference = new WeakReference(Window.Content);
                WeakReference serviceReference = new WeakReference(service);
                service = null;
                Window.Content = null;
                MemoryLeaksHelper.CollectOptional(controlReference, serviceReference);
                MemoryLeaksHelper.EnsureCollected(controlReference, serviceReference);
            });
        }
        #endregion

        #region Panel
        [Test, Asynchronous]
        public void Panel_InjectMethods() {
            ViewInjectionService service; PanelWrapper target;
            InitPanelTest(out service, out target);
            Panel_InjectMethods_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void Panel_RemoveViewModel() {
            ViewInjectionService service; PanelWrapper target;
            InitPanelTest(out service, out target);
            Panel_RemoveViewModelSimpleTest_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void Panel_CancelRemoveViewModel() {
            ViewInjectionService service; PanelWrapper target;
            InitPanelTest(out service, out target);
            ContentPresenter_CancelRemoveViewModelTest_Core(service);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }

        void InitPanelTest(out ViewInjectionService service, out PanelWrapper target) {
            Grid control = new Grid();
            service = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(service);
            target = new PanelWrapper() { Target = control };
        }
        void Panel_InjectMethods_Core<T>(ViewInjectionService service, IPanelWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            object vm1 = new object();
            object vm2 = new object();
            UIElementCollection children = (UIElementCollection)target.Children;
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Inject(null, vm1);
                iService.Inject(null, vm2, typeof(TestView1));
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm1, ((ContentPresenter)children[0]).Content);
                Assert.AreSame(vm2, ((ContentPresenter)children[1]).Content);
                Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(children[1]).OfType<TestView1>().First().DataContext);
            });
            EnqueueTestComplete();
        }
        void Panel_RemoveViewModelSimpleTest_Core<T>(ViewInjectionService service, IPanelWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            int viewModelClosingCommandCount = 0;
            service.ViewModelClosingCommand = new DelegateCommand(() => viewModelClosingCommandCount++);
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Remove(new object());
                iService.Inject(null, vm1);
                iService.Inject(null, vm2);
                iService.Remove(new object());
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                iService.SelectedViewModel = vm1;
                iService.Remove(vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, viewModelClosingCommandCount);
                Assert.IsNull(iService.SelectedViewModel);
                iService.Inject(null, vm1);
                iService.SelectedViewModel = vm1;
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm1, iService.SelectedViewModel);
                iService.Remove(vm2);
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, viewModelClosingCommandCount);
                Assert.AreSame(vm1, iService.SelectedViewModel);
                iService.Remove(vm1);
                Assert.AreEqual(0, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(3, viewModelClosingCommandCount);
                Assert.IsNull(iService.SelectedViewModel);
            });
            EnqueueTestComplete();
        }
        #endregion

        #region ItemsControl
        [Test, Asynchronous]
        public void ItemsControl_InvalidInitialization() {
            ItemsControl control = new ItemsControl() { ItemsSource = new List<object>() };
            ContentPresenter_InvalidInitializationTest_Core(control, "It is impossible to use ViewInjectionService for the control that has the ItemsSource property set.");
        }
        [Test, Asynchronous]
        public void ItemsControl_InjectMethods() {
            ViewInjectionService service; ItemsControlWrapper target;
            InitItemsControlTest(out service, out target);
            ItemsControl_InjectMethods_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ItemsControl_InjectWithItemTemplate() {
            ViewInjectionService service; ItemsControlWrapper target;
            InitItemsControlTest(out service, out target);
            ItemsControl_InjectWithItemTemplate_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ItemsControl_SelectedViewModel() {
            ViewInjectionService service; ItemsControlWrapper target;
            InitItemsControlTest(out service, out target);
            ItemsControl_SelectedViewModel_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ItemsControl_RemoveViewModel() {
            ViewInjectionService service; ItemsControlWrapper target;
            InitItemsControlTest(out service, out target);
            ItemsControl_RemoveViewModelSimpleTest_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void ItemsControl_CancelRemoveViewModel() {
            ViewInjectionService service; ItemsControlWrapper target;
            InitItemsControlTest(out service, out target);
            ContentPresenter_CancelRemoveViewModelTest_Core(service);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }

        void InitItemsControlTest(out ViewInjectionService service, out ItemsControlWrapper target) {
            ItemsControl control = new ItemsControl();
            service = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(service);
            target = new ItemsControlWrapper() { Target = control };
        }
        void ItemsControl_InjectMethods_Core<T>(ViewInjectionService service, IItemsControlWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreSame(service.Strategy.ViewModels, target.ItemsSource);
                iService.Inject(null, vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
                iService.Inject(null, vm2, typeof(TestView1));
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<ContentPresenter>().First().DataContext);
                Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<TestView1>().First().DataContext);
            });
            EnqueueTestComplete();
        }
        void ItemsControl_InjectWithItemTemplate_Core<T>(ViewInjectionService service, IItemsControlWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            target.ItemTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) };
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Inject(null, vm1);
                AssertHelper.AssertThrows<InvalidOperationException>(() => {
                    iService.Inject(null, vm2, typeof(TestView1));
                }, x => Assert.AreEqual("ViewInjectionService cannot create view by name or type, because the target control has the ItemTemplate/ItemTemplateSelector property set.", x.Message));
                iService.Inject(null, vm2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
                Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);
            });
            EnqueueTestComplete();
        }
        void ItemsControl_SelectedViewModel_Core<T>(ViewInjectionService service, IItemsControlWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            int selectedViewModelChangedCount = 0;
            service.SelectedViewModelChangedCommand = new DelegateCommand(() => selectedViewModelChangedCount++);
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Inject(null, vm1);
                iService.Inject(null, vm2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(0, selectedViewModelChangedCount);
                Assert.IsNull(iService.SelectedViewModel);
                iService.SelectedViewModel = vm1;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, selectedViewModelChangedCount);
                Assert.AreSame(vm1, iService.SelectedViewModel);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                AssertHelper.AssertThrows<InvalidOperationException>(() => {
                    iService.SelectedViewModel = new object();
                },
                x => Assert.AreEqual("Cannot set the SelectedViewModel property to a value that does not exist in the ViewModels collection. Inject the view model before selecting it.", x.Message));
            });
            EnqueueTestComplete();
        }
        void ItemsControl_RemoveViewModelSimpleTest_Core<T>(ViewInjectionService service, IItemsControlWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            int viewModelClosingCommandCount = 0;
            service.ViewModelClosingCommand = new DelegateCommand(() => viewModelClosingCommandCount++);
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Remove(new object());
                iService.Inject(null, vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
                iService.Inject(null, vm2);
                iService.Remove(new object());
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                iService.SelectedViewModel = vm1;
                iService.Remove(vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, viewModelClosingCommandCount);
                Assert.IsNull(iService.SelectedViewModel);
                iService.Inject(null, vm1);
                Assert.AreEqual(2, iService.ViewModels.Count());
                iService.SelectedViewModel = vm1;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                iService.Remove(vm2);
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, viewModelClosingCommandCount);
                Assert.AreSame(vm1, iService.SelectedViewModel);
                iService.Remove(vm1);
                Assert.AreEqual(0, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(3, viewModelClosingCommandCount);
                Assert.IsNull(iService.SelectedViewModel);
            });
            EnqueueTestComplete();
        }
        #endregion

        #region Selector
        [Test, Asynchronous]
        public void Selector_InvalidInitialization() {
            TabControl control = new TabControl() { ItemsSource = new List<object>() };
            ContentPresenter_InvalidInitializationTest_Core(control, "It is impossible to use ViewInjectionService for the control that has the ItemsSource property set.");
        }
        [Test, Asynchronous]
        public void Selector_SelectedViewModel() {
            ViewInjectionService service; SelectorWrapper target;
            InitSelectorTest(out service, out target);
            Selector_SelectedViewModel_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void Selector_RemoveViewModel() {
            ViewInjectionService service; SelectorWrapper target;
            InitSelectorTest(out service, out target);
            Selector_RemoveViewModelSimpleTest_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void Selector_CancelRemoveViewModel() {
            ViewInjectionService service; SelectorWrapper target;
            InitSelectorTest(out service, out target);
            ContentPresenter_CancelRemoveViewModelTest_Core(service);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }

        void InitSelectorTest(out ViewInjectionService service, out SelectorWrapper target) {
            TabControl control = new TabControl();
            service = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(service);
            target = new SelectorWrapper() { Target = control };
        }
        void Selector_SelectedViewModel_Core<T>(ViewInjectionService service, ISelectorWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            int selectedViewModelChangedCount = 0;
            service.SelectedViewModelChangedCommand = new DelegateCommand(() => selectedViewModelChangedCount++);
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Inject(null, vm1);
                iService.Inject(null, vm2);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, selectedViewModelChangedCount);
                Assert.AreSame(vm1, iService.SelectedViewModel);
                Assert.AreSame(vm1, target.SelectedItem);
                iService.SelectedViewModel = vm2;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, selectedViewModelChangedCount);
                Assert.AreSame(vm2, iService.SelectedViewModel);
                Assert.AreSame(vm2, target.SelectedItem);
                target.SelectedItem = vm1;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(3, selectedViewModelChangedCount);
                Assert.AreSame(vm1, iService.SelectedViewModel);
                Assert.AreSame(vm1, target.SelectedItem);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                AssertHelper.AssertThrows<InvalidOperationException>(() => {
                    iService.SelectedViewModel = new object();
                },
                x => Assert.AreEqual("Cannot set the SelectedViewModel property to a value that does not exist in the ViewModels collection. Inject the view model before selecting it.", x.Message));
            });
            EnqueueTestComplete();
        }
        void Selector_RemoveViewModelSimpleTest_Core<T>(ViewInjectionService service, ISelectorWrapper<T> target) where T : FrameworkElement {
            IViewInjectionService iService = service;
            int viewModelClosingCommandCount = 0;
            service.ViewModelClosingCommand = new DelegateCommand(() => viewModelClosingCommandCount++);
            object vm1 = new object();
            object vm2 = new object();
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.Remove(new object());
                iService.Inject(null, vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
                iService.Inject(null, vm2);
                iService.Remove(new object());
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                iService.SelectedViewModel = vm1;
                iService.Remove(vm1);
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, viewModelClosingCommandCount);
                Assert.AreSame(vm2, iService.SelectedViewModel);
                iService.Inject(null, vm1);
                Assert.AreEqual(2, iService.ViewModels.Count());
                iService.SelectedViewModel = vm1;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                iService.Remove(vm2);
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, viewModelClosingCommandCount);
                Assert.AreSame(vm1, iService.SelectedViewModel);
                iService.Remove(vm1);
                Assert.AreEqual(0, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(3, viewModelClosingCommandCount);
                Assert.IsNull(iService.SelectedViewModel);
            });
            EnqueueTestComplete();
        }
        #endregion

        #region TabControl
        [Test, Asynchronous]
        public void TabControl_InjectMethods() {
            ViewInjectionService service; TabControlWrapper target;
            InitTabControlTest(out service, out target);
            TabControl_InjectMethods_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void TabControl_InjectWithItemTemplate() {
            ViewInjectionService service; TabControlWrapper target;
            InitTabControlTest(out service, out target);
            TabControl_InjectWithItemTemplate_Core(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void TabControl_SelectedViewModel() {
            ViewInjectionService service; TabControlWrapper target;
            InitTabControlTest(out service, out target);
            Selector_SelectedViewModel_Core<Selector>(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void TabControl_RemoveViewModel() {
            ViewInjectionService service; TabControlWrapper target;
            InitTabControlTest(out service, out target);
            Selector_RemoveViewModelSimpleTest_Core<Selector>(service, target);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }
        [Test, Asynchronous]
        public void TabControl_CancelRemoveViewModel() {
            ViewInjectionService service; TabControlWrapper target;
            InitTabControlTest(out service, out target);
            ContentPresenter_CancelRemoveViewModelTest_Core(service);
            service = null; target = null;
            ContentPresenter_TestMemoryReleased();
        }

        void InitTabControlTest(out ViewInjectionService service, out TabControlWrapper target) {
            TabControl control = new TabControl() {
                ItemTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) }
            };
            service = new ViewInjectionService();
            Interaction.GetBehaviors(control).Add(service);
            target = new TabControlWrapper() { Target = control };
        }
        void TabControl_InjectMethods_Core(ViewInjectionService service, TabControlWrapper target) {
            CommonTabControl_InjectMethods_Core(service, target, "PART_SelectedContentHost", "HeaderPanel");
        }
        void CommonTabControl_InjectMethods_Core(ViewInjectionService service, ItemsControlWrapper target, string contentHostElementName, string headerPanelElementName) {
            IViewInjectionService iService = service;
            object vm1 = new object();
            object vm2 = new object();
            FrameworkElement contentHost = null;
            FrameworkElement headerPanel = null;
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreSame(service.Strategy.ViewModels, target.ItemsSource);
                contentHost = LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<FrameworkElement>().First(x => x.Name == contentHostElementName);
                headerPanel = LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<FrameworkElement>().First(x => x.Name == headerPanelElementName);
                iService.Inject(null, vm1);
                iService.SelectedViewModel = vm1;
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
                Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<FrameworkElement>().First(x => x.DataContext == vm1).DataContext);

                iService.Inject(null, vm2, typeof(TestView1));
                iService.SelectedViewModel = vm2;
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);
                Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<TestView1>().First().DataContext);
                iService.SelectedViewModel = vm2;
            });
            EnqueueTestComplete();
        }
        void TabControl_InjectWithItemTemplate_Core(ViewInjectionService service, TabControlWrapper target) {
            CommonTabControl_InjectWithItemTemplate_Core(service, target, "PART_SelectedContentHost", "HeaderPanel");
        }
        void CommonTabControl_InjectWithItemTemplate_Core(ViewInjectionService service, ItemsControlWrapper target, string contentHostElementName, string headerPanelElementName) {
            IViewInjectionService iService = service;
            target.ItemTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) };

            object vm1 = new object();
            object vm2 = new object();
            FrameworkElement contentHost = null;
            FrameworkElement headerPanel = null;
            Window.Content = service.AssociatedObject;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreSame(service.Strategy.ViewModels, target.ItemsSource);
                contentHost = LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<FrameworkElement>().First(x => x.Name == contentHostElementName);
                headerPanel = LayoutTreeHelper.GetVisualChildren(service.AssociatedObject).OfType<FrameworkElement>().First(x => x.Name == headerPanelElementName);
                iService.Inject(null, vm1);
                iService.SelectedViewModel = vm1;
                Assert.AreEqual(1, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
                Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);

                AssertHelper.AssertThrows<InvalidOperationException>(() => {
                    iService.Inject(null, vm2, typeof(TestView1));
                }, x => Assert.AreEqual("ViewInjectionService cannot create view by name or type, because the target control has the ItemTemplate/ItemTemplateSelector property set.", x.Message));
                iService.Inject(null, vm2);
                iService.SelectedViewModel = vm2;
                Assert.AreEqual(2, iService.ViewModels.Count());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);
                Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<TextBox>().First().DataContext);
                iService.SelectedViewModel = vm2;
            });
            EnqueueTestComplete();
        }
        #endregion

    }
    [TestFixture]
    public class ViewInjectionServiceComplexTests : BaseWpfFixture {
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
        class TabView : Grid {
            public TabView() {
                ViewInjectionService service = new ViewInjectionService();
                BindingOperations.SetBinding(service, ViewInjectionService.RegionNameProperty, new Binding("RegionName"));
                Interaction.GetBehaviors(this).Add(service);
            }
        }
        class TabContentViewModel : VMBase { }
        class TabContentView : Grid { }

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