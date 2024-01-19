using DevExpress.Mvvm.ModuleInjection;
using DevExpress.Mvvm.ModuleInjection.Native;
using DevExpress.Mvvm.UI.Interactivity;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.POCO;

namespace DevExpress.Mvvm.UI.ModuleInjection.Tests {
    internal class TestEmptyViewModel {
        public string name;
    }
    public class View1_BaseTests : Grid { }
    public class View2_BaseTests : Grid { }

    public class InjectionTestHelper {
        #region ServiceHelper
        public interface IServiceTestHelper {
            IEnumerable<object> ViewModels { get; }

            int SelectionChangedCount { get; }
            int NavigationCount { get; }

            bool CancelViewModelRemoving { get; set; }
            int ViewModelRemovingCount { get; }
            int ViewModelRemovedCount { get; }

            void Dispose();
            void AssertSelectionChanged(int expectedSelectionChangedCount);
            void AssertViewModelRemoving(int expectedViewModelRemovingCount);
            void AssertViewModelRemoved(int expectedViewModelRemovedCount);
        }
        public class ServiceTestHelper_ModuleInjectionService : IServiceTestHelper {
            public IEnumerable<object> ViewModels { get { return ((IUIRegion)service).ViewModels; } }
            public int SelectionChangedCount { get; private set; }
            public int NavigationCount { get; private set; }
            public bool CancelViewModelRemoving { get; set; }
            public int ViewModelRemovingCount { get; private set; }
            public int ViewModelRemovedCount { get; private set; }
            readonly UIRegionBase service;
            public ServiceTestHelper_ModuleInjectionService(UIRegionBase service) {
                this.service = service;
                ModuleManager.DefaultManager.GetEvents(service.RegionName).Navigation += OnNavigation;
                ModuleManager.DefaultManager.GetEvents(service.RegionName).ViewModelRemoving += OnViewModelRemoving;
                ModuleManager.DefaultManager.GetEvents(service.RegionName).ViewModelRemoved += OnViewModelRemoved;
                DependencyPropertyDescriptor.FromProperty(UIRegionBase.SelectedViewModelProperty, typeof(UIRegionBase))
                    .AddValueChanged(service, OnSelectedViewModelPropertyChanged);
                SelectionChangedCount = 0;
                NavigationCount = 0;
                CancelViewModelRemoving = false;
                ViewModelRemovingCount = 0;
                ViewModelRemovedCount = 0;
            }
            public void Dispose() {
                DependencyPropertyDescriptor.FromProperty(UIRegionBase.SelectedViewModelProperty, typeof(UIRegionBase))
                    .RemoveValueChanged(service, OnSelectedViewModelPropertyChanged);
            }
            public void AssertSelectionChanged(int expectedSelectionChangedCount) {
                string msg = "SelectionCount = " + SelectionChangedCount.ToString();
                msg += Environment.NewLine + "NavigationCount = " + NavigationCount.ToString();
                Assert.AreEqual(expectedSelectionChangedCount, SelectionChangedCount, msg);
                Assert.AreEqual(expectedSelectionChangedCount, NavigationCount, msg);
            }
            public void AssertViewModelRemoving(int expectedViewModelRemovingCount) {
                Assert.AreEqual(expectedViewModelRemovingCount, ViewModelRemovingCount);
            }
            public void AssertViewModelRemoved(int expectedViewModelRemovedCount) {
                Assert.AreEqual(expectedViewModelRemovedCount, ViewModelRemovedCount);
            }

            void OnNavigation(object sender, NavigationEventArgs e) {
                NavigationCount++;
            }
            void OnSelectedViewModelPropertyChanged(object sender, EventArgs e) {
                SelectionChangedCount++;
            }
            void OnViewModelRemoving(object sender, ViewModelRemovingEventArgs e) {
                ViewModelRemovingCount++;
                if(CancelViewModelRemoving)
                    e.Cancel = true;
            }
            void OnViewModelRemoved(object sender, ViewModelRemovedEventArgs e) {
                ViewModelRemovedCount++;
            }
        }
        public class ServiceTestHelper_ViewInjectionService : IServiceTestHelper {
            public IEnumerable<object> ViewModels { get { return service.ViewModels; } }
            public int SelectionChangedCount { get; private set; }
            public int NavigationCount { get; private set; }
            public bool CancelViewModelRemoving { get; set; }
            public int ViewModelRemovingCount { get; private set; }
            public int ViewModelRemovedCount { get; private set; }
            readonly ViewInjectionService service;
            public ServiceTestHelper_ViewInjectionService(ViewInjectionService service) {
                this.service = service;
                service.ViewModelClosingCommand = new DelegateCommand<ViewModelClosingEventArgs>(OnViewModelRemoving);
                DependencyPropertyDescriptor.FromProperty(ViewInjectionService.SelectedViewModelProperty, typeof(ViewInjectionService))
                    .AddValueChanged(service, OnSelectionChanged);
                SelectionChangedCount = 0;
                NavigationCount = 0;
                CancelViewModelRemoving = false;
                ViewModelRemovingCount = 0;
                ViewModelRemovedCount = 0;
            }
            public void Dispose() {
                service.ViewModelClosingCommand = null;
                DependencyPropertyDescriptor.FromProperty(ViewInjectionService.SelectedViewModelProperty, typeof(ViewInjectionService))
                    .RemoveValueChanged(service, OnSelectionChanged);
            }
            public void AssertSelectionChanged(int expectedSelectionChangedCount) {
                Assert.AreEqual(expectedSelectionChangedCount, SelectionChangedCount);
                Assert.AreEqual(expectedSelectionChangedCount, NavigationCount);
            }
            public void AssertViewModelRemoving(int expectedViewModelRemovingCount) {
                Assert.AreEqual(expectedViewModelRemovingCount, ViewModelRemovingCount);
            }
            public void AssertViewModelRemoved(int expectedViewModelRemovedCount) {
                Assert.AreEqual(expectedViewModelRemovedCount, ViewModelRemovedCount);
            }

            void OnSelectionChanged(object sender, EventArgs e) {
                NavigationCount++;
                SelectionChangedCount++;
            }
            void OnViewModelRemoving(ViewModelClosingEventArgs e) {
                ViewModelRemovingCount++;
                e.Cancel = CancelViewModelRemoving;
                if(!CancelViewModelRemoving)
                    ViewModelRemovedCount++;
            }
        }
        #endregion

        public static IServiceTestHelper CreateServiceHelper(DependencyObject target, string regionName) {
            var serv = Interaction.GetBehaviors(target).OfType<UIRegionBase>().FirstOrDefault();
            Assert.AreEqual(regionName, serv.RegionName);
            return CreateServiceHelper(serv);
        }
        public static IServiceTestHelper CreateServiceHelper(IUIRegion service) {
            return new ServiceTestHelper_ModuleInjectionService((UIRegionBase)service);
        }
        public static IServiceTestHelper CreateServiceHelper(IViewInjectionService service) {
            return new ServiceTestHelper_ViewInjectionService((ViewInjectionService)service);
        }

        public static void SetUp(System.Reflection.Assembly viewModelAndViewAssembly) {
            ViewModelLocator.Default = new ViewModelLocator(typeof(InjectionTestHelper).Assembly, viewModelAndViewAssembly);
            ViewLocator.Default = new ViewLocator(typeof(InjectionTestHelper).Assembly, viewModelAndViewAssembly);
            ModuleManager.DefaultManager = new ModuleManager();
        }
        public static void TearDown() {
            ModuleManager.DefaultManager = null;
            ViewModelLocator.Default = null;
            ViewLocator.Default = null;
        }
        public static WeakReference[] CollectReferences(object[] refs) {
            return refs.Select(x => new WeakReference(x)).ToArray();
        }
        public static void EnsureCollected(WeakReference[] refs) {
            DispatcherHelper.DoEvents();
            MemoryLeaksHelper.CollectOptional(refs);
            MemoryLeaksHelper.EnsureCollected(refs);
        }
    }

    internal static class ModuleInjectionExtensions {
        public static void Register(this IModuleManagerBase manager, string regionName, string key, Func<object> vmFactory) {
            manager.Register(regionName, new Module(key, vmFactory));
        }
        public static void Register(this IModuleManagerBase manager, string regionName, string key, Func<object> vmFactory, Type vType) {
            manager.Register(regionName, new Module(key, vmFactory, vType));
        }
    }

    public abstract class StrategyBaseTests<T> : BaseWpfFixture where T : DependencyObject {
        public IModuleManager Manager { get { return ModuleManager.DefaultManager; } }
        public IViewInjectionManager OldManager { get { return ViewInjectionManager.Default; } }
        protected override void SetUpCore() {
            base.SetUpCore();
            memoryTest = true;
            InjectionTestHelper.SetUp(this.GetType().Assembly);
        }
        protected override void TearDownCore() {
            InjectionTestHelper.TearDown();
            base.TearDownCore();
        }
        protected virtual void Show(ITargetWrapper<T> target) {
            Window.Content = target.Target;
            Window.Show();
        }
        protected virtual void Close(ITargetWrapper<T> target) {
            Window.Content = null;
            DispatcherHelper.UpdateLayoutAndDoEvents(Window);
        }
        protected void DoNotTestMemory() {
            memoryTest = false;
        }
        internal WeakReference[] CollectReferencesAndCloseWindow(ITargetWrapper<T> target, object[] refs) {
            var res = memoryTest
                ? refs.Select(x => new WeakReference(x))
                : new WeakReference[] { };
            Close(target);
            return res.ToArray();
        }

        protected void ViewInjection_CancelRemoveViewModelSimpleTestCore(IViewInjectionService service) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };

            service.Inject(null, vm1);
            service.Inject(null, vm2);
            DispatcherHelper.DoEvents();

            service.Remove(vm1);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, service.ViewModels.Count());
            service.Inject(null, vm1);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(2, service.ViewModels.Count());

            serviceHelper.AssertViewModelRemoving(1);
            serviceHelper.AssertViewModelRemoved(1);
            serviceHelper.CancelViewModelRemoving = true;
            service.Remove(vm1);
            service.Remove(vm2);
            DispatcherHelper.DoEvents();
            serviceHelper.AssertViewModelRemoving(3);
            serviceHelper.AssertViewModelRemoved(1);
            Assert.AreEqual(2, service.ViewModels.Count());

            serviceHelper.CancelViewModelRemoving = false;
            service.Remove(vm1);
            service.Remove(vm2);
            DispatcherHelper.DoEvents();
            serviceHelper.AssertViewModelRemoving(5);
            serviceHelper.AssertViewModelRemoved(3);
            Assert.AreEqual(0, service.ViewModels.Count());

            serviceHelper.Dispose();
        }
        protected void ModuleInjection_CancelRemoveViewModelSimpleTestCore(string regionName, DependencyObject target) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target, regionName);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };

            Manager.Register(regionName, "1", () => vm1);
            Manager.Register(regionName, "2", () => vm2);
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm2));

            Manager.Inject(regionName, "1");
            Manager.Inject(regionName, "2");
            DispatcherHelper.DoEvents();
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));

            Manager.Remove(regionName, "1");
            DispatcherHelper.DoEvents();
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));

            Manager.Inject(regionName, "1");
            DispatcherHelper.DoEvents();
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));

            serviceHelper.AssertViewModelRemoving(1);
            serviceHelper.AssertViewModelRemoved(1);
            serviceHelper.CancelViewModelRemoving = true;
            Manager.Remove(regionName, "1");
            Manager.Remove(regionName, "2");
            DispatcherHelper.DoEvents();
            serviceHelper.AssertViewModelRemoving(3);
            serviceHelper.AssertViewModelRemoved(1);
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));

            serviceHelper.CancelViewModelRemoving = false;
            Manager.Remove(regionName, "1");
            Manager.Remove(regionName, "2");
            DispatcherHelper.DoEvents();
            serviceHelper.AssertViewModelRemoving(5);
            serviceHelper.AssertViewModelRemoved(3);
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm2));

            serviceHelper.Dispose();
        }
        bool memoryTest;

        protected WeakReference[] ModuleInjection__ClearCore(string regionName, ITargetWrapper<T> target, bool nullSelectionAtStartup) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target.Target, regionName);
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, new Module("1", () => vm1 = new object()));
            Manager.Register(regionName, new Module("2", () => vm2 = new object()));
            Manager.Inject(regionName, "1");
            Manager.Inject(regionName, "2");

            if(nullSelectionAtStartup) {
                serviceHelper.AssertSelectionChanged(0);
                Assert.AreEqual(null, Manager.GetRegion(regionName).SelectedKey);
            }
            Manager.Navigate(regionName, "1");
            Assert.AreEqual("1", Manager.GetRegion(regionName).SelectedKey);
            serviceHelper.AssertSelectionChanged(1);
            Manager.Clear(regionName);
            Assert.AreEqual(null, Manager.GetRegion(regionName).SelectedKey);
            serviceHelper.AssertSelectionChanged(2);
            serviceHelper.AssertViewModelRemoving(0);
            serviceHelper.AssertViewModelRemoved(2);

            serviceHelper.Dispose();
            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }
    }
    public abstract class StrategyBasePanelTests<T> : StrategyBaseTests<T> where T : DependencyObject {
        protected abstract void Init(out IViewInjectionService service, out IPanelWrapper<T> target);
        protected abstract void Init(string regionName, out IPanelWrapper<T> target);

        [Test]
        public virtual void ViewInjection_InjectMethodsTest() {
            IViewInjectionService service; IPanelWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_InjectMethodsTestCore(service, target);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        [Test]
        public virtual void ModuleInjection_InjectMethodsTest() {
            IPanelWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_InjectMethodsTestCore("region", target);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_InjectMethodsTestCore(IViewInjectionService service, IPanelWrapper<T> target) {
            Show(target);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Func<int, FrameworkElement> getChild = x => target.Children.OfType<FrameworkElement>().ElementAt(x);

            service.Inject(null, vm1);
            service.Inject(null, vm2, typeof(View1_BaseTests));
            Assert.AreEqual(2, service.ViewModels.Count());

            DispatcherHelper.DoEvents();

            Assert.AreSame(vm1, ((ContentPresenter)getChild(0)).Content);
            Assert.AreSame(vm2, ((ContentPresenter)getChild(1)).Content);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(getChild(1)).OfType<View1_BaseTests>().First().DataContext);

            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_InjectMethodsTestCore(string regionName, IPanelWrapper<T> target) {
            Show(target);
            object vm1 = null;
            object vm2 = null;
            Func<IPanelWrapper<T> ,int, ContentPresenter> getChild = (t, i) => (ContentPresenter)t.Children.OfType<object>().ElementAt(i);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" }, typeof(View1_BaseTests));

            Manager.Inject(regionName, "1");
            Manager.Inject(regionName, "2");
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));

            DispatcherHelper.DoEvents();
            Assert.AreSame(vm1, getChild(target, 0).Content);
            Assert.AreSame(vm2, getChild(target, 1).Content);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(getChild(target, 1)).OfType<View1_BaseTests>().First().DataContext);

            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_RemoveViewModel() {
            IViewInjectionService service; IPanelWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection__RemoveViewModelSimpleTestCore(service, target);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        [Test]
        public virtual void ModuleInjection_RemoveViewModel() {
            IPanelWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection__RemoveViewModelSimpleTestCore("region", target);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection__RemoveViewModelSimpleTestCore(IViewInjectionService service, IPanelWrapper<T> target) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Remove(new object());
            service.Inject(null, vm1);
            service.Inject(null, vm2);
            service.Remove(new object());
            Assert.AreEqual(2, service.ViewModels.Count());

            service.SelectedViewModel = vm1;
            service.Remove(vm1);
            Assert.AreEqual(1, service.ViewModels.Count());

            serviceHelper.AssertViewModelRemoving(1);
            Assert.IsNull(service.SelectedViewModel);
            service.Inject(null, vm1);
            service.SelectedViewModel = vm1;
            Assert.AreEqual(2, service.ViewModels.Count());

            Assert.AreSame(vm1, service.SelectedViewModel);
            service.Remove(vm2);
            Assert.AreEqual(1, service.ViewModels.Count());

            serviceHelper.AssertViewModelRemoving(2);
            Assert.AreSame(vm1, service.SelectedViewModel);
            service.Remove(vm1);
            Assert.AreEqual(0, service.ViewModels.Count());

            serviceHelper.AssertViewModelRemoving(3);
            serviceHelper.AssertViewModelRemoved(3);
            Assert.IsNull(service.SelectedViewModel);

            serviceHelper.Dispose();
            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection__RemoveViewModelSimpleTestCore(string regionName, IPanelWrapper<T> target) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target.Target, regionName);
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" });

            Manager.Remove(regionName, "3");
            Manager.Inject(regionName, "1");
            Manager.Inject(regionName, "2");

            Manager.Navigate(regionName, "1");
            Manager.Remove(regionName, "1");
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetViewModel("1"));
            Assert.AreEqual(vm2, Manager.GetRegion(regionName).GetViewModel("2"));

            serviceHelper.AssertViewModelRemoving(1);
            serviceHelper.AssertViewModelRemoved(1);
            Assert.AreEqual(null, Manager.GetRegion(regionName).SelectedViewModel);
            Assert.AreEqual(null, Manager.GetRegion(regionName).SelectedKey);

            Manager.Inject(regionName, "1");
            Manager.Navigate(regionName, "1");
            Assert.AreEqual(vm1, Manager.GetRegion(regionName).GetViewModel("1"));
            Assert.AreEqual(vm2, Manager.GetRegion(regionName).GetViewModel("2"));
            Assert.AreEqual(vm1, Manager.GetRegion(regionName).SelectedViewModel);
            Assert.AreEqual("1", Manager.GetRegion(regionName).SelectedKey);

            Manager.Remove(regionName, "2");
            serviceHelper.AssertViewModelRemoving(2);
            Manager.Remove(regionName, "1");

            serviceHelper.AssertViewModelRemoving(3);
            serviceHelper.AssertViewModelRemoved(3);

            serviceHelper.Dispose();
            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_CancelRemoveViewModel() {
            IViewInjectionService service; IPanelWrapper<T> target;
            Init(out service, out target);
            Show(target);
            ViewInjection_CancelRemoveViewModelSimpleTestCore(service);
            service = null; target = null;
        }
        [Test]
        public virtual void ModuleInjection_CancelRemoveViewModel() {
            IPanelWrapper<T> target;
            Init("region", out target);
            Show(target);
            ModuleInjection_CancelRemoveViewModelSimpleTestCore("region", target.Target);
            target = null;
        }

        [Test]
        public virtual void ModuleInjection_Clear() {
            IPanelWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection__ClearCore("region", target, true);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
    }
    public abstract class StrategyBaseContentPresenterTests<T> : StrategyBaseTests<T> where T : DependencyObject {
        protected abstract void Init(out IViewInjectionService service, out IContentPresenterWrapper<T> target);
        protected abstract void Init(string regionName, out IContentPresenterWrapper<T> target);

        [Test]
        public virtual void ViewInjection_InjectMethods() {
            IViewInjectionService service; IContentPresenterWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_InjectMethodsCore(service, target);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        [Test]
        public virtual void ModuleInjection_InjectMethods() {
            IContentPresenterWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_InjectMethodsCore("region", target);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_InjectMethodsCore(IViewInjectionService service, IContentPresenterWrapper<T> target) {
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Inject(null, vm1);
            Assert.AreSame(vm1, target.Content);
            service.Inject(null, vm2, typeof(View1_BaseTests));
            Assert.AreEqual(2, service.ViewModels.Count());
            service.SelectedViewModel = vm2;

            DispatcherHelper.DoEvents();
            Assert.AreSame(vm2, target.Content);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(target.Target).OfType<View1_BaseTests>().First().DataContext);

            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_InjectMethodsCore(string regionName, IContentPresenterWrapper<T> target) {
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" }, typeof(View1_BaseTests));

            Manager.Inject(regionName, "1");
            Assert.AreSame(vm1, target.Content);
            Manager.Inject(regionName, "2");
            Manager.Navigate(regionName, "2");

            DispatcherHelper.DoEvents();
            Assert.AreSame(vm2, target.Content);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(target.Target).OfType<View1_BaseTests>().First().DataContext);

            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_InjectWithContentTemplate() {
            IViewInjectionService service; IContentPresenterWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_InjectWithContentTemplateCore(service, target);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        [Test]
        public virtual void ModuleInjection_InjectWithContentTemplate() {
            IContentPresenterWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_InjectWithContentTemplateCore("region", target);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_InjectWithContentTemplateCore(IViewInjectionService service, IContentPresenterWrapper<T> target) {
            target.ContentTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) };
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Inject(null, vm1);
            DispatcherHelper.DoEvents();
            Assert.AreSame(vm1, target.Content);
            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(target.Target).OfType<TextBox>().First().DataContext);
            service.Inject(null, vm2, typeof(View1_BaseTests));
            DispatcherHelper.DoEvents();
            Assert.AreEqual(vm1, service.SelectedViewModel);
            service.SelectedViewModel = vm2;
            DispatcherHelper.DoEvents();

            Assert.AreSame(vm2, target.Content);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(target.Target).OfType<TextBox>().First().DataContext);

            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_InjectWithContentTemplateCore(string regionName, IContentPresenterWrapper<T> target) {
            target.ContentTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) };
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" }, typeof(View1_BaseTests));

            Manager.Inject(regionName, "1");
            DispatcherHelper.DoEvents();
            Assert.AreSame(vm1, target.Content);
            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(target.Target).OfType<TextBox>().First().DataContext);
            Manager.Inject(regionName, "2");
            DispatcherHelper.DoEvents();
            Assert.AreEqual(vm1, target.Content);
            Manager.Navigate(regionName, "2");
            DispatcherHelper.DoEvents();

            Assert.AreSame(vm2, target.Content);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(target.Target).OfType<TextBox>().First().DataContext);

            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_SelectedViewModel() {
            ViewInjection_SelectedViewModelCore(true);
        }
        [Test]
        public virtual void ModuleInjection_SelectedViewModel() {
            ModuleInjection_SelectedViewModelCore(true);
        }
        protected void ViewInjection_SelectedViewModelCore(bool selectedItemBeNotNullOnFirstInject) {
            IViewInjectionService service; IContentPresenterWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_SelectedViewModelCore(service, target, selectedItemBeNotNullOnFirstInject);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        protected void ModuleInjection_SelectedViewModelCore(bool selectedItemBeNotNullOnFirstInject) {
            IContentPresenterWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_SelectedViewModelCore("region", target, selectedItemBeNotNullOnFirstInject);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_SelectedViewModelCore(IViewInjectionService service, IContentPresenterWrapper<T> target, bool selectedItemBeNotNullOnFirstInject) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Inject(null, vm1);
            DispatcherHelper.DoEvents();
            if(!selectedItemBeNotNullOnFirstInject) {
                service.SelectedViewModel = vm1;
                DispatcherHelper.DoEvents();
            }
            serviceHelper.AssertSelectionChanged(1);
            Assert.AreSame(vm1, target.Content);
            service.Inject(null, vm2);
            serviceHelper.AssertSelectionChanged(1);
            service.SelectedViewModel = vm2;

            DispatcherHelper.DoEvents();
            serviceHelper.AssertSelectionChanged(2);
            Assert.AreSame(vm2, target.Content);
            service.SelectedViewModel = vm1;

            DispatcherHelper.DoEvents();
            serviceHelper.AssertSelectionChanged(3);
            Assert.AreSame(vm1, target.Content);

            service.SelectedViewModel = new object();

            serviceHelper.Dispose();
            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_SelectedViewModelCore(string regionName, IContentPresenterWrapper<T> target, bool selectedItemBeNotNullOnFirstInject) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target.Target, regionName);
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" });
            Manager.Inject(regionName, "1");
            DispatcherHelper.DoEvents();

            if(!selectedItemBeNotNullOnFirstInject) {
                Manager.Navigate(regionName, "1");
                DispatcherHelper.DoEvents();
            }

            serviceHelper.AssertSelectionChanged(1);
            Assert.AreSame(vm1, target.Content);
            Manager.Inject(regionName, "2");
            DispatcherHelper.DoEvents();
            serviceHelper.AssertSelectionChanged(1);

            Manager.Navigate(regionName, "2");
            DispatcherHelper.DoEvents();
            serviceHelper.AssertSelectionChanged(2);
            Assert.AreSame(vm2, target.Content);

            Manager.Navigate(regionName, "1");
            DispatcherHelper.DoEvents();
            serviceHelper.AssertSelectionChanged(3);
            Assert.AreSame(vm1, target.Content);

            Manager.Navigate(regionName, "3");
            serviceHelper.Dispose();

            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_RemoveViewModel() {
            IViewInjectionService service; IContentPresenterWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_RemoveViewModelSimpleTestCore(service, target);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        [Test]
        public virtual void ModuleInjection_RemoveViewModel() {
            IContentPresenterWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_RemoveViewModelSimpleTestCore("region", target);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_RemoveViewModelSimpleTestCore(IViewInjectionService service, IContentPresenterWrapper<T> target) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Remove(new object());
            service.Inject(null, vm1);
            DispatcherHelper.DoEvents();
            Assert.AreSame(vm1, target.Content);
            service.Inject(null, vm2);
            service.SelectedViewModel = vm2;
            service.Remove(new object());
            DispatcherHelper.DoEvents();
            Assert.AreEqual(2, service.ViewModels.Count());

            Assert.AreSame(vm2, target.Content);
            service.SelectedViewModel = vm1;
            service.Remove(vm1);
            Assert.AreEqual(1, service.ViewModels.Count());

            serviceHelper.AssertViewModelRemoving(1);
            Assert.IsNull(service.SelectedViewModel);
            Assert.IsNull(target.Content);
            service.Inject(null, vm1);
            service.SelectedViewModel = vm1;
            Assert.AreEqual(2, service.ViewModels.Count());

            Assert.AreSame(vm1, service.SelectedViewModel);
            service.Remove(vm2);
            Assert.AreEqual(1, service.ViewModels.Count());

            serviceHelper.AssertViewModelRemoving(2);
            Assert.AreSame(vm1, service.SelectedViewModel);
            service.Remove(vm1);
            Assert.AreEqual(0, service.ViewModels.Count());

            serviceHelper.AssertViewModelRemoving(3);
            serviceHelper.AssertViewModelRemoved(3);
            Assert.IsNull(service.SelectedViewModel);

            serviceHelper.Dispose();
            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_RemoveViewModelSimpleTestCore(string regionName, IContentPresenterWrapper<T> target) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target.Target, regionName);
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" });

            Manager.Remove(regionName, "100");
            Manager.Remove(regionName, "1");
            Manager.Inject(regionName, "1");
            DispatcherHelper.DoEvents();
            Assert.AreSame(vm1, target.Content);
            Manager.Inject(regionName, "2");
            Manager.Navigate(regionName, "2");
            DispatcherHelper.DoEvents();
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));

            Assert.AreSame(vm2, target.Content);
            Manager.Navigate(regionName, "1");
            Manager.Remove(regionName, "1");
            DispatcherHelper.DoEvents();
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));

            serviceHelper.AssertViewModelRemoving(1);
            Assert.IsNull(Manager.GetRegion(regionName).SelectedViewModel);
            Assert.IsNull(target.Content);
            Manager.Inject(regionName, "1");
            Manager.Navigate(regionName, "1");
            DispatcherHelper.DoEvents();
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));

            Assert.AreSame(vm1, Manager.GetRegion(regionName).SelectedViewModel);
            Manager.Remove(regionName, "2");

            serviceHelper.AssertViewModelRemoving(2);
            Assert.AreSame(vm1, Manager.GetRegion(regionName).SelectedViewModel);
            Manager.Remove(regionName, "1");
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm2));

            serviceHelper.AssertViewModelRemoving(3);
            serviceHelper.AssertViewModelRemoved(3);
            Assert.IsNull(target.Content);

            serviceHelper.Dispose();
            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_CancelRemoveViewModel() {
            IViewInjectionService service; IContentPresenterWrapper<T> target;
            Init(out service, out target);
            Show(target);
            ViewInjection_CancelRemoveViewModelSimpleTestCore(service);
            service = null; target = null;
        }
        [Test]
        public virtual void ModuleInjection_CancelRemoveViewModel() {
            IContentPresenterWrapper<T> target;
            Init("region", out target);
            Show(target);
            ModuleInjection_CancelRemoveViewModelSimpleTestCore("region", target.Target);
            target = null;
        }

        [Test]
        public virtual void ModuleInjection_Clear() {
            IContentPresenterWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection__ClearCore("region", target, false);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
    }
    public abstract class StrategyBaseItemsControlTests<T> : StrategyBaseTests<T> where T : DependencyObject {
        protected abstract void Init(out IViewInjectionService service, out IItemsControlWrapper<T> target);
        protected abstract void Init(string regionName, out IItemsControlWrapper<T> target);
        protected virtual IEnumerable<FrameworkElement> GetVisualChildren(IItemsControlWrapper<T> target) {
            return LayoutTreeHelper.GetVisualChildren(target.Target).OfType<FrameworkElement>();
        }

        [Test]
        public virtual void ViewInjection_InjectMethods() {
            ViewInjection_InjectMethodsCore(false);
        }
        [Test]
        public virtual void ModuleInjection_InjectMethods() {
            ViewInjection_InjectMethodsCore(false);
        }
        protected void ViewInjection_InjectMethodsCore(bool navigateBeforeCheck, bool navigateBeforeFirstCheck = false) {
            IViewInjectionService service; IItemsControlWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_InjectMethodsCore(service, target, navigateBeforeCheck, navigateBeforeFirstCheck);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        protected void ModuleInjection_InjectMethodsCore(bool navigateBeforeCheck, bool navigateBeforeFirstCheck = false) {
            IItemsControlWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_InjectMethodsCore("region", target, navigateBeforeCheck, navigateBeforeFirstCheck);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_InjectMethodsCore(IViewInjectionService service, IItemsControlWrapper<T> target, bool navigateBeforeCheck, bool navigateBeforeFirstCheck) {
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Inject(null, vm1);
            Assert.AreEqual(1, service.ViewModels.Count());
            service.Inject(null, vm2, typeof(View1_BaseTests));
            Assert.AreEqual(2, service.ViewModels.Count());

            DispatcherHelper.DoEvents();
            if(navigateBeforeFirstCheck) {
                service.SelectedViewModel = vm1;
                DispatcherHelper.DoEvents();
            }
            Assert.AreSame(vm1, GetVisualChildren(target).First().DataContext);
            if(navigateBeforeCheck) {
                service.SelectedViewModel = vm2;
                DispatcherHelper.DoEvents();
            }
            Assert.AreSame(vm2, GetVisualChildren(target).OfType<View1_BaseTests>().First().DataContext);

            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_InjectMethodsCore(string regionName, IItemsControlWrapper<T> target, bool navigateBeforeCheck, bool navigateBeforeFirstCheck) {
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" }, typeof(View1_BaseTests));

            Manager.Inject(regionName, "1");
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm2));
            Manager.Inject(regionName, "2");
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));

            DispatcherHelper.DoEvents();
            if(navigateBeforeFirstCheck) {
                Manager.Navigate(regionName, "1");
                DispatcherHelper.DoEvents();
            }
            Assert.AreSame(vm1, GetVisualChildren(target).First().DataContext);
            if(navigateBeforeCheck) {
                Manager.Navigate(regionName, "2");
                DispatcherHelper.DoEvents();
            }
            Assert.AreSame(vm2, GetVisualChildren(target).OfType<View1_BaseTests>().First().DataContext);

            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_InjectWithItemTemplate() {
            ViewInjection_InjectWithItemTemplateCore(false);
        }
        [Test]
        public virtual void ModuleInjection_InjectWithItemTemplate() {
            ModuleInjection_InjectWithItemTemplateCore(false);
        }
        protected void ViewInjection_InjectWithItemTemplateCore(bool navigateBeforeCheck, bool navigateBeforeFirstCheck = false) {
            IViewInjectionService service; IItemsControlWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_InjectWithItemTemplateCore(service, target, navigateBeforeCheck, navigateBeforeFirstCheck);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        protected void ModuleInjection_InjectWithItemTemplateCore(bool navigateBeforeCheck, bool navigateBeforeFirstCheck = false) {
            IItemsControlWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_InjectWithItemTemplateCore("region", target, navigateBeforeCheck, navigateBeforeFirstCheck);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_InjectWithItemTemplateCore(IViewInjectionService service, IItemsControlWrapper<T> target, bool navigateBeforeCheck, bool navigateBeforeFirstCheck) {
            target.ItemTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) };
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Inject(null, vm1);
            service.Inject(null, vm2, typeof(View1_BaseTests));
            DispatcherHelper.DoEvents();
            if(navigateBeforeFirstCheck) {
                service.SelectedViewModel = vm1;
                DispatcherHelper.DoEvents();
            }
            Assert.AreSame(vm1, GetVisualChildren(target).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
            if(navigateBeforeCheck) {
                service.SelectedViewModel = vm2;
                DispatcherHelper.DoEvents();
            }
            Assert.AreSame(vm2, GetVisualChildren(target).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);

            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_InjectWithItemTemplateCore(string regionName, IItemsControlWrapper<T> target, bool navigateBeforeCheck, bool navigateBeforeFirstCheck) {
            target.ItemTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) };
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" }, typeof(View1_BaseTests));

            Manager.Inject(regionName, "1");
            Manager.Inject(regionName, "2");
            DispatcherHelper.DoEvents();
            if(navigateBeforeFirstCheck) {
                Manager.Navigate(regionName, "1");
                DispatcherHelper.DoEvents();
            }
            Assert.AreSame(vm1, GetVisualChildren(target).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
            if(navigateBeforeCheck) {
                Manager.Navigate(regionName, "2");
                DispatcherHelper.DoEvents();
            }
            Assert.AreSame(vm2, GetVisualChildren(target).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);

            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_SelectedViewModel() {
            ViewInjection_SelectedViewModelCore(false);
        }
        [Test]
        public virtual void ModuleInjection_SelectedViewModel() {
            ModuleInjection_SelectedViewModelCore(false);
        }
        protected void ViewInjection_SelectedViewModelCore(bool selectedItemShouldBeNotNullOnFirstInject) {
            IViewInjectionService service; IItemsControlWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_SelectedViewModelCore(service, target, selectedItemShouldBeNotNullOnFirstInject);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        protected void ModuleInjection_SelectedViewModelCore(bool selectedItemShouldBeNotNullOnFirstInject) {
            IItemsControlWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_SelectedViewModelCore("region", target, selectedItemShouldBeNotNullOnFirstInject);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_SelectedViewModelCore(IViewInjectionService service, IItemsControlWrapper<T> target, bool selectedItemShouldBeNotNullOnFirstInject) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Inject(null, vm1);
            service.Inject(null, vm2);
            DispatcherHelper.DoEvents();
            if(!selectedItemShouldBeNotNullOnFirstInject) {
                serviceHelper.AssertSelectionChanged(0);
                Assert.IsNull(service.SelectedViewModel);
            }

            service.SelectedViewModel = vm1;
            DispatcherHelper.DoEvents();
            serviceHelper.AssertSelectionChanged(1);
            Assert.AreSame(vm1, service.SelectedViewModel);

            service.SelectedViewModel = new object();
            serviceHelper.Dispose();
            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_SelectedViewModelCore(string regionName, IItemsControlWrapper<T> target, bool selectedItemShouldBeNotNullOnFirstInject) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target.Target, regionName);
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" });

            Manager.Inject(regionName, "1");
            Manager.Inject(regionName, "2");
            DispatcherHelper.DoEvents();
            if(!selectedItemShouldBeNotNullOnFirstInject) {
                Assert.AreEqual(null, Manager.GetRegion(regionName).SelectedViewModel);
                DispatcherHelper.DoEvents();
                serviceHelper.AssertSelectionChanged(0);
                Assert.AreEqual(null, Manager.GetRegion(regionName).SelectedViewModel);
            }

            Manager.Navigate(regionName, "1");
            DispatcherHelper.DoEvents();
            serviceHelper.AssertSelectionChanged(1);
            Assert.AreEqual(vm1, Manager.GetRegion(regionName).SelectedViewModel);

            serviceHelper.Dispose();
            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_RemoveViewModelSimpleTest() {
            IViewInjectionService service; IItemsControlWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_RemoveViewModelSimpleTestCore(service, target);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        [Test]
        public virtual void ModuleInjection_RemoveViewModelSimpleTest() {
            IItemsControlWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_RemoveViewModelSimpleTestCore("region", target);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_RemoveViewModelSimpleTestCore(IViewInjectionService service, IItemsControlWrapper<T> target) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Remove(new object());
            service.Inject(null, vm1);
            Assert.AreEqual(1, service.ViewModels.Count());
            service.Inject(null, vm2);
            service.Remove(new object());
            Assert.AreEqual(2, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            service.SelectedViewModel = vm1;
            service.Remove(vm1);
            Assert.AreEqual(1, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(1);
            Assert.IsNull(service.SelectedViewModel);
            service.Inject(null, vm1);
            Assert.AreEqual(2, service.ViewModels.Count());
            service.SelectedViewModel = vm1;
            DispatcherHelper.DoEvents();

            service.Remove(vm2);
            Assert.AreEqual(1, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(2);
            Assert.AreSame(vm1, service.SelectedViewModel);
            service.Remove(vm1);
            Assert.AreEqual(0, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(3);
            Assert.IsNull(service.SelectedViewModel);

            serviceHelper.Dispose();
            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target, service });
        }
        WeakReference[] ModuleInjection_RemoveViewModelSimpleTestCore(string regionName, IItemsControlWrapper<T> target) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target.Target, regionName);
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" });
            Manager.Inject(regionName, "1");
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm2));
            Manager.Inject(regionName, "2");
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));
            DispatcherHelper.DoEvents();

            Manager.Navigate(regionName, "1");
            Manager.Remove(regionName, "1");
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(1);
            Assert.IsNull(Manager.GetRegion(regionName).SelectedViewModel);
            Manager.Inject(regionName, "1");
            Manager.Navigate(regionName, "1");
            DispatcherHelper.DoEvents();

            Manager.Remove(regionName, "2");
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm2));
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(2);
            Assert.AreSame(vm1, Manager.GetRegion(regionName).SelectedViewModel);
            Manager.Remove(regionName, "1");
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(3);
            Assert.IsNull(Manager.GetRegion(regionName).SelectedViewModel);

            serviceHelper.Dispose();
            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_CancelRemoveViewModel() {
            IViewInjectionService service; IItemsControlWrapper<T> target;
            Init(out service, out target);
            Show(target);
            ViewInjection_CancelRemoveViewModelSimpleTestCore(service);
            service = null; target = null;
        }
        [Test]
        public virtual void ModuleInjection_CancelRemoveViewModel() {
            IItemsControlWrapper<T> target;
            Init("region", out target);
            Show(target);
            ModuleInjection_CancelRemoveViewModelSimpleTestCore("region", target.Target);
            target = null;
        }

        [Test]
        public virtual void ModuleInjection_Clear() {
            IItemsControlWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection__ClearCore("region", target, true);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
    }
    public abstract class StrategyBaseSelectorTests<T> : StrategyBaseItemsControlTests<T> where T : DependencyObject {
        protected abstract void Init(out IViewInjectionService service, out ISelectorWrapper<T> target);
        protected abstract void Init(string regionName, out ISelectorWrapper<T> target);
        protected sealed override void Init(out IViewInjectionService service, out IItemsControlWrapper<T> target) {
            ISelectorWrapper<T> t;
            Init(out service, out t);
            target = t;
        }
        protected sealed override void Init(string regionName, out IItemsControlWrapper<T> target) {
            ISelectorWrapper<T> t;
            Init(regionName, out t);
            target = t;
        }

        [Test]
        public override void ViewInjection_InjectMethods() {
            ViewInjection_InjectMethodsCore(true);
        }
        [Test]
        public override void ModuleInjection_InjectMethods() {
            ViewInjection_InjectMethodsCore(true);
        }

        [Test]
        public override void ViewInjection_SelectedViewModel() {
            ViewInjection_SelectedViewModelCore(true);
        }
        [Test]
        public override void ModuleInjection_SelectedViewModel() {
            ModuleInjection_SelectedViewModelCore(true);
        }

        [Test]
        public override void ViewInjection_RemoveViewModelSimpleTest() {
            ViewInjection_RemoveViewModelSimpleTest(true);
        }
        [Test]
        public override void ModuleInjection_RemoveViewModelSimpleTest() {
            ModuleInjection_RemoveViewModelSimpleTest(true);
        }
        protected void ViewInjection_RemoveViewModelSimpleTest(bool checkSelection) {
            IViewInjectionService service; ISelectorWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_RemoveViewModelSimpleTestCore(service, target, checkSelection);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        protected void ModuleInjection_RemoveViewModelSimpleTest(bool checkSelection) {
            ISelectorWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_RemoveViewModelSimpleTestCore("region", target, checkSelection);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_RemoveViewModelSimpleTestCore(IViewInjectionService service, ISelectorWrapper<T> target, bool checkSelection) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Remove(new object());
            service.Inject(null, vm1);
            Assert.AreEqual(1, service.ViewModels.Count());
            service.Inject(null, vm2);
            service.Remove(new object());
            Assert.AreEqual(2, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            service.SelectedViewModel = vm1;
            service.Remove(vm1);
            Assert.AreEqual(1, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(1);
            if(checkSelection) Assert.AreEqual(vm2, service.SelectedViewModel);
            service.Inject(null, vm1);
            DispatcherHelper.DoEvents();

            Assert.AreEqual(2, service.ViewModels.Count());
            service.SelectedViewModel = vm1;
            DispatcherHelper.DoEvents();

            service.Remove(vm2);
            Assert.AreEqual(1, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(2);
            if(checkSelection) Assert.AreSame(vm1, service.SelectedViewModel);
            service.Remove(vm1);
            Assert.AreEqual(0, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(3);
            Assert.IsNull(service.SelectedViewModel);

            serviceHelper.Dispose();
            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target, service });
        }
        WeakReference[] ModuleInjection_RemoveViewModelSimpleTestCore(string regionName, ISelectorWrapper<T> target, bool checkSelection) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target.Target, regionName);
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" });
            Manager.Inject(regionName, "1");
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm2));
            Manager.Inject(regionName, "2");
            Manager.Remove(regionName, "3");
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));
            DispatcherHelper.DoEvents();

            Manager.Navigate(regionName, "1");
            Manager.Remove(regionName, "1");
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual("2", Manager.GetRegion(regionName).GetKey(vm2));
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(1);
            if(checkSelection) Assert.AreEqual(vm2, Manager.GetRegion(regionName).SelectedViewModel);
            Manager.Inject(regionName, "1");
            Manager.Navigate(regionName, "1");
            DispatcherHelper.DoEvents();

            Manager.Remove(regionName, "2");
            Assert.AreEqual("1", Manager.GetRegion(regionName).GetKey(vm1));
            Assert.AreEqual(null, Manager.GetRegion(regionName).GetKey(vm2));
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(2);
            if(checkSelection) Assert.AreSame(vm1, Manager.GetRegion(regionName).SelectedViewModel);
            Manager.Remove(regionName, "1");
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(3);
            if(checkSelection) Assert.IsNull(Manager.GetRegion(regionName).SelectedViewModel);

            serviceHelper.Dispose();
            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_SelectedViewModel2() {
            ViewInjection_SelectedViewModel2Core(true);
        }
        [Test]
        public virtual void ModuleInjection_SelectedViewModel2() {
            ModuleInjection_SelectedViewModel2Core(true);
        }
        protected void ViewInjection_SelectedViewModel2Core(bool selectedItemShouldBeNotNullOnFirstInject) {
            IViewInjectionService service; ISelectorWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_SelectedViewModelCore2(service, target, selectedItemShouldBeNotNullOnFirstInject);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        protected void ModuleInjection_SelectedViewModel2Core(bool selectedItemShouldBeNotNullOnFirstInject) {
            ISelectorWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_SelectedViewModelCore2("region", target, selectedItemShouldBeNotNullOnFirstInject);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_SelectedViewModelCore2(IViewInjectionService service, ISelectorWrapper<T> target, bool selectedItemShouldBeNotNullOnFirstInject) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);

            service.Inject(null, vm1);
            service.Inject(null, vm2);
            DispatcherHelper.DoEvents();
            if(!selectedItemShouldBeNotNullOnFirstInject) {
                service.SelectedViewModel = vm1;
                DispatcherHelper.DoEvents();
            }

            serviceHelper.AssertSelectionChanged(1);
            Assert.AreSame(vm1, service.SelectedViewModel);
            Assert.AreSame(vm1, target.SelectedItem);
            service.SelectedViewModel = vm2;
            DispatcherHelper.DoEvents();

            serviceHelper.AssertSelectionChanged(2);
            Assert.AreSame(vm2, service.SelectedViewModel);
            Assert.AreSame(vm2, target.SelectedItem);
            target.SelectedItem = vm1;
            DispatcherHelper.DoEvents();

            serviceHelper.AssertSelectionChanged(3);
            Assert.AreSame(vm1, service.SelectedViewModel);
            Assert.AreSame(vm1, target.SelectedItem);
            DispatcherHelper.DoEvents();

            service.SelectedViewModel = new object();
            serviceHelper.Dispose();
            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_SelectedViewModelCore2(string regionName, ISelectorWrapper<T> target, bool selectedItemShouldBeNotNullOnFirstInject) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target.Target, regionName);
            object vm1 = null;
            object vm2 = null;
            Show(target);
            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" });
            Manager.Inject(regionName, "1");
            Manager.Inject(regionName, "2");
            DispatcherHelper.DoEvents();

            if(!selectedItemShouldBeNotNullOnFirstInject) {
                Manager.Navigate(regionName, "1");
                DispatcherHelper.DoEvents();
            }

            serviceHelper.AssertSelectionChanged(1);
            Assert.AreSame(vm1, Manager.GetRegion(regionName).SelectedViewModel);
            Assert.AreSame(vm1, target.SelectedItem);
            Manager.Navigate(regionName, "2");
            DispatcherHelper.DoEvents();

            serviceHelper.AssertSelectionChanged(2);
            Assert.AreSame(vm2, Manager.GetRegion(regionName).SelectedViewModel);
            Assert.AreSame(vm2, target.SelectedItem);
            target.SelectedItem = vm1;
            DispatcherHelper.DoEvents();

            serviceHelper.AssertSelectionChanged(3);
            Assert.AreSame(vm1, Manager.GetRegion(regionName).SelectedViewModel);
            Assert.AreSame(vm1, target.SelectedItem);
            DispatcherHelper.DoEvents();

            Manager.Navigate(regionName, "3");
            serviceHelper.Dispose();
            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_RemoveViewModelSimpleTest2() {
            ViewInjection_RemoveViewModelSimpleTest2(true);
        }
        [Test]
        public virtual void ModuleInjection_RemoveViewModelSimpleTest2() {
            ModuleInjection_RemoveViewModelSimpleTest2(true);
        }
        protected void ViewInjection_RemoveViewModelSimpleTest2(bool checkSelection) {
            IViewInjectionService service; ISelectorWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_RemoveViewModelSimpleTestCore2(service, target, checkSelection);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        protected void ModuleInjection_RemoveViewModelSimpleTest2(bool checkSelection) {
            ISelectorWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_RemoveViewModelSimpleTestCore2("region", target, checkSelection);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_RemoveViewModelSimpleTestCore2(IViewInjectionService service, ISelectorWrapper<T> target, bool checkSelection) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(service);
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);
            service.Remove(new object());
            service.Inject(null, vm1);
            Assert.AreEqual(1, service.ViewModels.Count());
            service.Inject(null, vm2);
            service.Remove(new object());
            Assert.AreEqual(2, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            service.SelectedViewModel = vm1;
            service.Remove(vm1);
            Assert.AreEqual(1, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(1);
            if(checkSelection) Assert.AreSame(vm2, service.SelectedViewModel);
            service.Inject(null, vm1);
            Assert.AreEqual(2, service.ViewModels.Count());
            service.SelectedViewModel = vm1;
            DispatcherHelper.DoEvents();

            service.Remove(vm2);
            Assert.AreEqual(1, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(2);
            if(checkSelection) Assert.AreSame(vm1, service.SelectedViewModel);
            service.Remove(vm1);
            Assert.AreEqual(0, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(3);
            Assert.IsNull(service.SelectedViewModel);

            serviceHelper.Dispose();
            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_RemoveViewModelSimpleTestCore2(string regionName, ISelectorWrapper<T> target, bool checkSelection) {
            var serviceHelper = InjectionTestHelper.CreateServiceHelper(target.Target, regionName);
            object vm1 = null;
            object vm2 = null;
            Show(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" });
            Manager.Inject(regionName, "1");

            Assert.AreEqual(1, ((IEnumerable)target.ItemsSource).OfType<object>().Count());
            Manager.Inject(regionName, "2");
            Assert.AreEqual(2, ((IEnumerable)target.ItemsSource).OfType<object>().Count());
            DispatcherHelper.DoEvents();

            Manager.Navigate(regionName, "1");
            Manager.Remove(regionName, "1");
            Assert.AreEqual(1, ((IEnumerable)target.ItemsSource).OfType<object>().Count());
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(1);
            if(checkSelection) Assert.AreSame(vm2, target.SelectedItem);
            Manager.Inject(regionName, "1");
            Assert.AreEqual(2, ((IEnumerable)target.ItemsSource).OfType<object>().Count());
            Manager.Navigate(regionName, "1");
            DispatcherHelper.DoEvents();

            Manager.Remove(regionName, "2");
            DispatcherHelper.DoEvents();

            serviceHelper.AssertViewModelRemoving(2);
            if(checkSelection) Assert.AreSame(vm1, target.SelectedItem);
            Manager.Remove(regionName, "1");
            Assert.AreSame(null, target.SelectedItem);
            serviceHelper.AssertViewModelRemoving(3);

            serviceHelper.Dispose();
            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public override void ModuleInjection_Clear() {
            IItemsControlWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection__ClearCore("region", target, false);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
    }
    public abstract class StrategyBaseTabControlTests<T> : StrategyBaseSelectorTests<T> where T : DependencyObject {
        protected abstract FrameworkElement GetContentHost(ISelectorWrapper<T> target);
        protected abstract FrameworkElement GetHeaderPanel(ISelectorWrapper<T> target);
        protected abstract void SetHeaderTemplate(ISelectorWrapper<T> target, DataTemplate template);
        protected override IEnumerable<FrameworkElement> GetVisualChildren(IItemsControlWrapper<T> target) {
            return LayoutTreeHelper.GetVisualChildren(GetContentHost((ISelectorWrapper<T>)target)).OfType<FrameworkElement>();
        }

        [Test]
        public override void ViewInjection_InjectWithItemTemplate() {
            ViewInjection_InjectWithItemTemplateCore(true);
        }
        [Test]
        public override void ModuleInjection_InjectWithItemTemplate() {
            ModuleInjection_InjectWithItemTemplateCore(true);
        }

        [Test]
        public virtual void ViewInjection_InjectMethods2() {
            IViewInjectionService service; ISelectorWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_InjectMethodsCore2(service, target);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        [Test]
        public virtual void ModuleInjection_InjectMethods2() {
            ModuleInjection_InjectMethodsCore2(true);
        }
        protected void ModuleInjection_InjectMethodsCore2(bool checkSelectedItemOnLoaded) {
            ISelectorWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_InjectMethodsCore2("region", target, checkSelectedItemOnLoaded);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_InjectMethodsCore2(IViewInjectionService service, ISelectorWrapper<T> target) {
            SetHeaderTemplate(target, new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) });
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);
            DispatcherHelper.DoEvents();
            FrameworkElement contentHost = GetContentHost(target);
            FrameworkElement headerPanel = GetHeaderPanel(target);

            service.Inject(null, vm1);
            service.SelectedViewModel = vm1;
            Assert.AreEqual(1, service.ViewModels.Count());
            DispatcherHelper.UpdateLayoutAndDoEvents(Window);
            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<FrameworkElement>().First(x => x.DataContext == vm1).DataContext);

            service.Inject(null, vm2, typeof(View1_BaseTests));
            service.SelectedViewModel = vm2;
            Assert.AreEqual(2, service.ViewModels.Count());
            DispatcherHelper.DoEvents();

            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<View1_BaseTests>().First().DataContext);
            service.SelectedViewModel = vm2;

            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target, service });
        }
        WeakReference[] ModuleInjection_InjectMethodsCore2(string regionName, ISelectorWrapper<T> target, bool checkSelectedItemOnLoaded) {
            SetHeaderTemplate(target, new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) });
            object vm1 = null;
            object vm2 = null;
            Show(target);
            DispatcherHelper.DoEvents();
            FrameworkElement contentHost = GetContentHost(target);
            FrameworkElement headerPanel = GetHeaderPanel(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" }, typeof(View1_BaseTests));
            Manager.Inject(regionName, "1");
            DispatcherHelper.DoEvents();
            if(checkSelectedItemOnLoaded)
                Assert.AreEqual(null, target.SelectedItem);

            Manager.Navigate(regionName, "1");
            DispatcherHelper.DoEvents();
            Assert.AreEqual(vm1, target.SelectedItem);
            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<FrameworkElement>().First(x => x.DataContext == vm1).DataContext);

            Manager.Inject(regionName, "2");
            Manager.Navigate(regionName, "2");
            DispatcherHelper.DoEvents();

            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<View1_BaseTests>().First().DataContext);

            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }

        [Test]
        public virtual void ViewInjection_InjectWithItemTemplate2() {
            IViewInjectionService service; ISelectorWrapper<T> target;
            Init(out service, out target);
            var refs = ViewInjection_InjectWithItemTemplateCore2(service, target);
            service = null; target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        [Test]
        public virtual void ModuleInjection_InjectWithItemTemplate2() {
            ISelectorWrapper<T> target;
            Init("region", out target);
            var refs = ModuleInjection_InjectWithItemTemplateCore2("region", target);
            target = null;
            InjectionTestHelper.EnsureCollected(refs);
        }
        WeakReference[] ViewInjection_InjectWithItemTemplateCore2(IViewInjectionService service, ISelectorWrapper<T> target) {
            SetHeaderTemplate(target, new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) });
            target.ItemTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) };
            object vm1 = new TestEmptyViewModel { name = "vm1" };
            object vm2 = new TestEmptyViewModel { name = "vm2" };
            Show(target);
            DispatcherHelper.DoEvents();
            FrameworkElement contentHost = GetContentHost(target);
            FrameworkElement headerPanel = GetHeaderPanel(target);

            service.Inject(null, vm1);
            service.SelectedViewModel = vm1;
            Assert.AreEqual(1, ((IEnumerable)target.ItemsSource).OfType<object>().Count());
            DispatcherHelper.DoEvents();

            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
            service.Inject(null, vm2, typeof(View1_BaseTests));
            service.SelectedViewModel = vm2;
            Assert.AreEqual(2, ((IEnumerable)target.ItemsSource).OfType<object>().Count());
            DispatcherHelper.DoEvents();

            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);
            service.SelectedViewModel = vm2;

            return CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, service, target.Target });
        }
        WeakReference[] ModuleInjection_InjectWithItemTemplateCore2(string regionName, ISelectorWrapper<T> target) {
            SetHeaderTemplate(target, new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) });
            target.ItemTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(TextBox)) };
            object vm1 = null;
            object vm2 = null;
            Show(target);
            DispatcherHelper.DoEvents();
            FrameworkElement contentHost = GetContentHost(target);
            FrameworkElement headerPanel = GetHeaderPanel(target);

            Manager.Register(regionName, "1", () => vm1 = new TestEmptyViewModel { name = "vm1" });
            Manager.Register(regionName, "2", () => vm2 = new TestEmptyViewModel { name = "vm2" }, typeof(View1_BaseTests));
            Manager.Inject(regionName, "1");
            Manager.Navigate(regionName, "1");
            DispatcherHelper.DoEvents();

            Assert.AreEqual(1, ((IEnumerable)target.ItemsSource).OfType<object>().Count());
            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);
            Assert.AreSame(vm1, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<TextBox>().First(x => x.DataContext == vm1).DataContext);

            Manager.Inject(regionName, "2");
            Manager.Navigate(regionName, "2");
            DispatcherHelper.DoEvents();
            Assert.AreEqual(2, ((IEnumerable)target.ItemsSource).OfType<object>().Count());
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(headerPanel).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);
            Assert.AreSame(vm2, LayoutTreeHelper.GetVisualChildren(contentHost).OfType<TextBox>().First(x => x.DataContext == vm2).DataContext);

            var res = CollectReferencesAndCloseWindow(target, new object[] { vm1, vm2, target.Target });
            vm1 = null;
            vm2 = null;
            return res;
        }
    }
}