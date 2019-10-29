using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.POCO;
using System.Windows.Controls;
using System.Threading;
using System;
using System.Windows;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class ServiceContainerTests : BaseWpfFixture {
        protected override void TearDownCore() {
            base.TearDownCore();
            ServiceContainer.Default = null;
        }
        public interface IService1 { }
        public interface IService2 { }
        public class TestService1 : IService1 { }
        public class TestService2 : IService2 { }
        public class TestService1_2 : IService1, IService2 { }
        [Test]
        public void ContainersLoopTest() {
            var child = new TestSupportServices();
            var parent = new TestSupportServices();
            child.ParentViewModel = parent;
            parent.ParentViewModel = child;
            var service1 = new TestService1();
            child.ServiceContainer.RegisterService(service1, true);
            AssertHelper.AssertThrows<Exception>(() => {
                child.ServiceContainer.GetService<IService1>();
            }, e => {
                Assert.AreEqual("A ServiceContainer should not be a direct or indirect parent for itself.", e.Message);
            });
        }
        [Test]
        public void AddServicesWithoutKey() {
            IServiceContainer container = new ServiceContainer(null);
            TestService1 svc1 = new TestService1();
            TestService1 svc1_ = new TestService1();
            TestService2 svc2 = new TestService2();
            container.RegisterService(svc1);
            Assert.AreEqual(svc1, container.GetService<IService1>());
            container.RegisterService(svc2);
            Assert.AreEqual(svc1, container.GetService<IService1>());
            Assert.AreEqual(svc2, container.GetService<IService2>());
            container.RegisterService("svc1_", svc1_);
            Assert.AreEqual(svc1, container.GetService<IService1>());
            Assert.AreEqual(svc1_, container.GetService<IService1>("svc1_"));
        }
        [Test]
        public void AddServicesWithoutKey_2() {
            IServiceContainer container = new ServiceContainer(null);
            TestService1 svc1 = new TestService1();
            TestService1 svc1_ = new TestService1();
            TestService2 svc2 = new TestService2();
            container.RegisterService("svc1_", svc1_);
            container.RegisterService(svc1);
            container.RegisterService(svc2);
            Assert.AreEqual(svc1, container.GetService<IService1>());
            Assert.AreEqual(svc2, container.GetService<IService2>());
            Assert.AreEqual(svc1_, container.GetService<IService1>("svc1_"));
        }
        [Test]
        public void RemoveServices() {
            IServiceContainer container = new ServiceContainer(null);
            TestService1 svc1 = new TestService1();
            TestService1 svc1_ = new TestService1();
            TestService2 svc2 = new TestService2();
            container.RegisterService(svc1);
            container.RegisterService(svc2);
            container.RegisterService("svc1_", svc1_);
            Assert.AreEqual(svc1, container.GetService<IService1>());
            Assert.AreEqual(svc1_, container.GetService<IService1>("svc1_"));
            Assert.AreEqual(svc2, container.GetService<IService2>());

            container.UnregisterService(svc1);
            Assert.AreEqual(svc1_, container.GetService<IService1>());
            Assert.AreEqual(svc1_, container.GetService<IService1>("svc1_"));
            Assert.AreEqual(svc2, container.GetService<IService2>());
            container.UnregisterService(svc1_);
            Assert.AreEqual(null, container.GetService<IService1>());
            Assert.AreEqual(null, container.GetService<IService1>("svc1_"));
            Assert.AreEqual(svc2, container.GetService<IService2>());
            container.UnregisterService(svc2);
            Assert.AreEqual(null, container.GetService<IService2>());
        }
        [Test]
        public void GetServiceNotByInterfaceType() {
            IServiceContainer container = new ServiceContainer(null);
            TestService1 svc1 = new TestService1();
            container.RegisterService(svc1);
            Assert.Throws<ArgumentException>(() => { container.GetService<TestService1>(); });
        }
        [Test]
        public void GetServiceNotByInterfaceTypeWinKeyName() {
            ServiceContainer container = new ServiceContainer(null);
            Assert.Throws<ArgumentException>(() => { container.GetService<TestService1>("svc1_"); });
        }
        [Test]
        public void AddServicesWithKey() {
            IServiceContainer container = new ServiceContainer(null);
            TestService1 svc1 = new TestService1();
            TestService1 svc1_ = new TestService1();
            container.RegisterService("svc1", svc1);
            container.RegisterService("svc1_", svc1_);
            Assert.AreEqual(svc1_, container.GetService<IService1>());
            Assert.AreEqual(svc1, container.GetService<IService1>("svc1"));
            Assert.AreEqual(svc1_, container.GetService<IService1>("svc1_"));
            container.RegisterService(svc1_);
            Assert.AreEqual(svc1_, container.GetService<IService1>());
            container.RegisterService(svc1);
            Assert.AreEqual(svc1, container.GetService<IService1>());
        }
        [Test]
        public void GetServiceFromParent() {
            var parent = new TestSupportServices();
            var child = new TestSupportServices();
            var service1 = new TestService1();
            child.ServiceContainer.RegisterService(service1);
            Assert.AreEqual(service1, child.ServiceContainer.GetService<IService1>());

            var service2 = new TestService2();
            parent.ServiceContainer.RegisterService(service2);
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService2>());

            child.ParentViewModel = parent;
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>());

            Assert.AreEqual(null, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.LocalOnly));
            var service2Local = new TestService2();
            child.ServiceContainer.RegisterService(service2Local);
            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.LocalOnly));
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.PreferParents));
        }
        [Test]
        public void B235125_GetServiceFromSecondParent() {
            var parent1 = new TestSupportServices();
            var parent2 = new TestSupportServices();
            var child = new TestSupportServices();
            var service1 = new TestService1();
            var service2 = new TestService1();
            parent1.ServiceContainer.RegisterService(service1);
            parent2.ServiceContainer.RegisterService(service2);

            child.ParentViewModel = parent1;
            parent1.ParentViewModel = parent2;
            Assert.AreEqual(service1, child.ServiceContainer.GetService<IService1>());
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService1>(ServiceSearchMode.PreferParents));
        }
        [Test]
        public void B235125_GetServiceFromSecondParentByKey() {
            var parent1 = new TestSupportServices();
            var parent2 = new TestSupportServices();
            var child = new TestSupportServices();
            var service1 = new TestService1();
            var service2 = new TestService1();
            parent1.ServiceContainer.RegisterService("key", service1);
            parent2.ServiceContainer.RegisterService("key", service2);

            child.ParentViewModel = parent1;
            parent1.ParentViewModel = parent2;
            Assert.AreEqual(service1, child.ServiceContainer.GetService<IService1>("key"));
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService1>("key", ServiceSearchMode.PreferParents));
        }
        [Test]
        public void GetServiceFromParentByKey() {
            var parent = new TestSupportServices();
            var child = new TestSupportServices();

            var service2 = new TestService2();
            parent.ServiceContainer.RegisterService("key", service2);
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService2>("key"));

            child.ParentViewModel = parent;
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>("key"));

            Assert.AreEqual(null, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.LocalOnly));
            var service2Local = new TestService2();
            child.ServiceContainer.RegisterService("key", service2Local);
            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.LocalOnly));
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.PreferParents));
            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.PreferLocal));
        }
        [Test]
        public void YieldToParentTest1() {
            var parent = new TestSupportServices();
            var child = new TestSupportServices();
            var service2 = new TestService2();
            parent.ServiceContainer.RegisterService("key", service2);
            child.ParentViewModel = parent;
            var service2Local = new TestService2();
            child.ServiceContainer.RegisterService("key", service2Local, true);

            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.PreferParents));
            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.LocalOnly));
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.PreferLocal));
        }
        [Test]
        public void YieldToParentTest2() {
            var parent = new TestSupportServices();
            var child = new TestSupportServices();
            var service2 = new TestService2();
            parent.ServiceContainer.RegisterService(service2);
            child.ParentViewModel = parent;
            var service2Local = new TestService2();
            child.ServiceContainer.RegisterService(service2Local, true);

            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.PreferParents));
            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.LocalOnly));
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.PreferLocal));
        }
        [Test]
        public void YieldToParentTest3() {
            var child = new TestSupportServices();
            var service2Local = new TestService2();
            child.ServiceContainer.RegisterService(service2Local, true);

            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.LocalOnly));
            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.PreferParents));
            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.PreferLocal));
        }
        [Test]
        public void YieldToParentTest4() {
            var child = new TestSupportServices();
            var service2Local = new TestService2();
            child.ServiceContainer.RegisterService("key", service2Local, true);

            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.LocalOnly));
            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.PreferParents));
            Assert.AreEqual(service2Local, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.PreferLocal));
        }
        [Test]
        public void GetService_PreferParent() {
            var parent = new TestSupportServices();
            var child = new TestSupportServices();
            var service2 = new TestService2();
            child.ServiceContainer.RegisterService(service2);
            child.ParentViewModel = parent;
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.PreferParents));
        }
        [Test]
        public void GetServiceByKey_PreferParent() {
            var parent = new TestSupportServices();
            var child = new TestSupportServices();
            var service2 = new TestService2();
            child.ServiceContainer.RegisterService("key", service2);
            child.ParentViewModel = parent;
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>("key", ServiceSearchMode.PreferParents));
        }
        [Test]
        public void GetServiceFromParentByKey_InvalidParent() {
            var child = new TestSupportServices();

            var service2 = new TestService2();
            child.ServiceContainer.RegisterService("key", service2);
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>("key"));
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService1>("key2"));

            child.ParentViewModel = child;
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>("key"));
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService1>("key2"));
        }
        [Test]
        public void GetServiceFromParent_InvalidParent() {
            var child = new TestSupportServices();

            var service2 = new TestService2();
            child.ServiceContainer.RegisterService(service2);
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>());
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService1>());

            child.ParentViewModel = child;
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>());
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService1>());

            var service1 = new TestService1();
            ServiceContainer.Default.RegisterService(service1);
            try {
                Assert.AreEqual(service1, child.ServiceContainer.GetService<IService1>());
            } finally {
                ServiceContainer.Default.Clear();
            }
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService1>());
        }
        [Test]
        public void RegisterNull() {
            var child = new TestSupportServices();
            Assert.Throws<ArgumentNullException>(() => { child.ServiceContainer.RegisterService(null); });
            Assert.Throws<ArgumentNullException>(() => { child.ServiceContainer.RegisterService("key", null); });
        }
        [Test]
        public void RegisterByNullKey() {
            var child = new TestSupportServices();
            var service = new TestService1();
            child.ServiceContainer.RegisterService(null, service);
            Assert.AreEqual(service, child.ServiceContainer.GetService<IService1>());
        }
        [Test]
        public void Clear() {
            var parent = new TestSupportServices();
            var child = new TestSupportServices();

            var service2 = new TestService2();
            child.ServiceContainer.RegisterService(service2);
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>());

            child.ServiceContainer.Clear();
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService2>());

            child.ServiceContainer.RegisterService("key", service2);
            child.ServiceContainer.Clear();
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService2>());
            Assert.AreEqual(null, child.ServiceContainer.GetService<IService2>("key"));
        }
        [Test]
        public void RegisterServiceWithoutNameTwice() {
            var viewModel = new TestSupportServices();
            viewModel.ServiceContainer.RegisterService(new TestService1());

            var service1_2 = new TestService1_2();
            viewModel.ServiceContainer.RegisterService(service1_2);
            Assert.AreEqual(service1_2, viewModel.ServiceContainer.GetService<IService1>());
            Assert.AreEqual(service1_2, viewModel.ServiceContainer.GetService<IService2>());

            var service2 = new TestService2();
            viewModel.ServiceContainer.RegisterService(service2);
            Assert.AreEqual(service1_2, viewModel.ServiceContainer.GetService<IService1>());
            Assert.AreEqual(service2, viewModel.ServiceContainer.GetService<IService2>());
        }
        [Test]
        public void FindServiceWithoutKeyTest() {
            foreach(var useEmptyStringAsParentServiceKey in new bool?[] { null, false, true })
                foreach(var useEmptyStringAsLocalServiceKey in new bool?[] { null, false, true }) {
                    KeylessServiceSearchTestHelper tester = new KeylessServiceSearchTestHelper() { UseEmptyStringAsParentServiceKey = useEmptyStringAsParentServiceKey, UseEmptyStringAsLocalServiceKey = useEmptyStringAsLocalServiceKey };
                    tester.TestKeylessServiceSearch(29, parentHasKey: false, nestedHasKey: false, yieldToParent: false, preferLocal: false, assertActualIsNested: false);
                    tester.TestKeylessServiceSearch(30, parentHasKey: false, nestedHasKey: false, yieldToParent: false, preferLocal: true, assertActualIsNested: true);
                    tester.TestKeylessServiceSearch(31, parentHasKey: false, nestedHasKey: false, yieldToParent: true, preferLocal: false, assertActualIsNested: false);
                    tester.TestKeylessServiceSearch(32, parentHasKey: false, nestedHasKey: false, yieldToParent: true, preferLocal: true, assertActualIsNested: false);
                    tester.TestKeylessServiceSearch(33, parentHasKey: false, nestedHasKey: true, yieldToParent: false, preferLocal: false, assertActualIsNested: false);
                    tester.TestKeylessServiceSearch(34, parentHasKey: false, nestedHasKey: true, yieldToParent: false, preferLocal: true, assertActualIsNested: false);
                    tester.TestKeylessServiceSearch(35, parentHasKey: false, nestedHasKey: true, yieldToParent: true, preferLocal: false, assertActualIsNested: false);
                    tester.TestKeylessServiceSearch(36, parentHasKey: false, nestedHasKey: true, yieldToParent: true, preferLocal: true, assertActualIsNested: false);
                    tester.TestKeylessServiceSearch(37, parentHasKey: true, nestedHasKey: false, yieldToParent: false, preferLocal: false, assertActualIsNested: true);
                    tester.TestKeylessServiceSearch(38, parentHasKey: true, nestedHasKey: false, yieldToParent: false, preferLocal: true, assertActualIsNested: true);
                    tester.TestKeylessServiceSearch(39, parentHasKey: true, nestedHasKey: false, yieldToParent: true, preferLocal: false, assertActualIsNested: true);
                    tester.TestKeylessServiceSearch(40, parentHasKey: true, nestedHasKey: false, yieldToParent: true, preferLocal: true, assertActualIsNested: true);
                    tester.TestKeylessServiceSearch(41, parentHasKey: true, nestedHasKey: true, yieldToParent: false, preferLocal: false, assertActualIsNested: false);
                    tester.TestKeylessServiceSearch(42, parentHasKey: true, nestedHasKey: true, yieldToParent: false, preferLocal: true, assertActualIsNested: true);
                    tester.TestKeylessServiceSearch(43, parentHasKey: true, nestedHasKey: true, yieldToParent: true, preferLocal: false, assertActualIsNested: false);
                    tester.TestKeylessServiceSearch(44, parentHasKey: true, nestedHasKey: true, yieldToParent: true, preferLocal: true, assertActualIsNested: false);
                }
        }

        [Test]
        public void GetApplicationService() {
            var defaultServiceContainer = new DefaultServiceContainer2();
            var service2InApp = new TestService2();
            defaultServiceContainer.Resources.Add("testService2", service2InApp);
            var service11 = new TestService1();
            var service12 = new TestService1();
            defaultServiceContainer.Resources.Add("testService11", service11);
            defaultServiceContainer.Resources.Add("testService12", service12);
            ServiceContainer.Default = defaultServiceContainer;
            var parent = new TestSupportServices();
            var child = new TestSupportServices();
            var service2 = new TestService2();
            child.ServiceContainer.RegisterService(service2);
            child.ParentViewModel = parent;
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>(ServiceSearchMode.PreferParents));
            Assert.AreEqual(service2, child.ServiceContainer.GetService<IService2>());
            Assert.IsNotNull(child.ServiceContainer.GetService<IService1>());
            Assert.AreEqual(service11, child.ServiceContainer.GetService<IService1>("testService11"));
        }
        [Test]
        public void T711283() {
            var defaultServiceContainer = new DefaultServiceContainer2();
            defaultServiceContainer.Resources.Add("testService", null);
            ServiceContainer.Default = defaultServiceContainer;
            var vm = new TestSupportServices();
            vm.GetService<IMessageBoxService>();
        }
        [Test]
        public void GetLastParent() {
            var root = new TestSupportServices();
            var parent = new TestSupportServices();
            var child = new TestSupportServices();
            parent.ParentViewModel = root;
            child.ParentViewModel = parent;

            var rootSrv = new TestService1();
            var parentSrv = new TestService1();
            var childSrv = new TestService1();

            root.ServiceContainer.RegisterService(rootSrv);
            parent.ServiceContainer.RegisterService(parentSrv);
            child.ServiceContainer.RegisterService(childSrv);

            Assert.AreEqual(rootSrv, child.ServiceContainer.GetService<IService1>(ServiceSearchMode.PreferParents));
        }
        [Test]
        public void GetApplicationService_DoNotUseServicesInMergedDictionaries() {
            var defaultServiceContainer = new DefaultServiceContainer2();
            var service1 = new TestService1();
            var mergedDictionary = new ResourceDictionary();
            mergedDictionary.Add("testService2", service1);
            defaultServiceContainer.Resources.MergedDictionaries.Add(mergedDictionary);
            Assert.IsNull(defaultServiceContainer.GetService<IService1>());
        }

        class KeylessServiceSearchTestHelper {
            public bool? UseEmptyStringAsParentServiceKey;
            public bool? UseEmptyStringAsLocalServiceKey;
            public void TestKeylessServiceSearch(int message, bool parentHasKey, bool nestedHasKey, bool yieldToParent, bool preferLocal, bool assertActualIsNested) {
                IServiceContainer parentContainer = new ServiceContainer(null);
                IServiceContainer nestedContainer = new ServiceContainer(new ParentServiceContainerWrapper(parentContainer));
                TestService1 parentService = new TestService1();
                RegisterService(parentHasKey, "p", parentService, false, parentContainer, UseEmptyStringAsParentServiceKey);
                TestService1 nestedService = new TestService1();
                RegisterService(nestedHasKey, "n", nestedService, yieldToParent, nestedContainer, UseEmptyStringAsLocalServiceKey);
                string messageString = message.ToString() + " " + ToString(UseEmptyStringAsParentServiceKey) + " " + ToString(UseEmptyStringAsLocalServiceKey);
                Assert.AreEqual(assertActualIsNested ? nestedService : parentService, nestedContainer.GetService<IService1>(preferLocal ? ServiceSearchMode.PreferLocal : ServiceSearchMode.PreferParents), messageString);
            }
            void RegisterService<T>(bool registerWithKey, string key, T service, bool yieldToParent, IServiceContainer serviceContainer, bool? useEmptyStringAsServiceKey) {
                if(registerWithKey)
                    serviceContainer.RegisterService(key, service, yieldToParent);
                else if(!useEmptyStringAsServiceKey.HasValue)
                    serviceContainer.RegisterService(service, yieldToParent);
                else if(useEmptyStringAsServiceKey.Value)
                    serviceContainer.RegisterService(string.Empty, service, yieldToParent);
                else
                    serviceContainer.RegisterService(null, service, yieldToParent);
            }
            string ToString(bool? v) {
                return v == null ? "none" : v.Value ? "empty" : "null";
            }
        }
    }
    public class TestSupportServices : ISupportServices, ISupportParentViewModel, ISupportParameter {
        readonly ServiceContainer serviceContainer;
        public TestSupportServices() {
            serviceContainer = new ServiceContainer(this);
        }
        public IServiceContainer ServiceContainer { get { return serviceContainer; } }
        public object ParentViewModel { get; set; }
        public object Parameter { get; set; }

        public void OnNavigatedTo(object parameter) {
            Parameter = parameter;
        }
    }
    public class ParentServiceContainerWrapper : ISupportParentViewModel {
        class ParentServiceContainerOwner : ISupportServices {
            IServiceContainer parentServiceContainer;

            public ParentServiceContainerOwner(IServiceContainer parentServiceContainer) {
                this.parentServiceContainer = parentServiceContainer;
            }
            IServiceContainer ISupportServices.ServiceContainer { get { return parentServiceContainer; } }
        }

        ParentServiceContainerOwner parentServiceContainerOwner;

        public ParentServiceContainerWrapper(IServiceContainer parentServiceContainer) {
            this.parentServiceContainerOwner = new ParentServiceContainerOwner(parentServiceContainer);
        }
        object ISupportParentViewModel.ParentViewModel {
            get { return parentServiceContainerOwner; }
            set { throw new NotSupportedException(); }
        }
    }
    class DefaultServiceContainer2 : DefaultServiceContainer {
        public DefaultServiceContainer2() : base() {
            Resources = new ResourceDictionary();
        }
        public ResourceDictionary Resources { get; private set; }
        protected override ResourceDictionary GetApplicationResources() {
            return Resources;
        }
    }

    [TestFixture]
    public class ServiceBaseTests : BaseWpfFixture {
        [Test]
        public void UnregisterServiceOnDataContextChanged() {
            Button control = new Button();
            TestVM vm1 = TestVM.Create();
            TestVM vm2 = TestVM.Create();
            TestServiceBase service = new TestServiceBase();
            Interaction.GetBehaviors(control).Add(service);
            control.DataContext = vm1;
            Assert.AreEqual(service, vm1.GetService<ITestService>());
            control.DataContext = vm2;
            Assert.AreEqual(null, vm1.GetService<ITestService>());
            Assert.AreEqual(service, vm2.GetService<ITestService>());
        }
        [Test]
        public void UnregisterServiceOnDetaching() {
            Button control = new Button();
            TestVM vm1 = TestVM.Create();
            TestServiceBase service = new TestServiceBase();
            Interaction.GetBehaviors(control).Add(service);
            control.DataContext = vm1;
            Assert.AreEqual(service, vm1.GetService<ITestService>());
            Interaction.GetBehaviors(control).Remove(service);
            Assert.AreEqual(null, vm1.GetService<ITestService>());
        }
        [Test]
        public void UnregisterServiceOnUnloaded() {
            Button control = new Button();
            TestVM vm1 = TestVM.Create();
            TestServiceBase service = new TestServiceBase() { UnregisterOnUnloaded = true };
            Interaction.GetBehaviors(control).Add(service);
            control.DataContext = vm1;
            Assert.AreEqual(service, vm1.GetService<ITestService>());
            Window.Content = control;
            Window.Show();
            Assert.AreEqual(service, vm1.GetService<ITestService>());
            Window.Content = null;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(null, vm1.GetService<ITestService>());
        }
        [Test]
        public void T250427() {
            Grid mainV = new Grid();
            TestVM mainVM = TestVM.Create();
            TestServiceBase mainService = new TestServiceBase();
            Interaction.GetBehaviors(mainV).Add(mainService);
            mainV.DataContext = mainVM;

            Grid childV = new Grid();
            TestVM childVM = TestVM.Create();
            TestServiceBase childService = new TestServiceBase();
            Interaction.GetBehaviors(childV).Add(childService);
            mainV.Children.Add(childV);

            Assert.AreEqual(childService, mainVM.GetService<ITestService>());
            childV.DataContext = childVM;
            Assert.AreEqual(mainService, mainVM.GetService<ITestService>());
            Assert.AreEqual(childService, childVM.GetService<ITestService>());
        }

        public class TestVM {
            public static TestVM Create() { return ViewModelSource.Create(() => new TestVM()); }
            protected TestVM() { }
        }
        public interface ITestService { }
        public class TestServiceBase : ServiceBase, ITestService { }
    }

#if !DXCORE3
    [TestFixture]
    public class ServiceContainerThreadTest :BaseWpfFixture {
        const int iterationCount = 1000;
        const int threadCount = 100;
        void RunThreads(Action test) {
            List<Thread> threads = new List<Thread>();
            for(int i = 0; i < threadCount; i++)
                threads.Add(new Thread(new ThreadStart(test)));
            foreach(var t in threads) t.Start();
            foreach(var t in threads) t.Join();
            foreach(var t in threads) t.Abort();
        }

        protected override void SetUpCore() {
            base.SetUpCore();
            ServiceContainer.Default = null;
        }
        protected override void TearDownCore() {
            ServiceContainer.Default = null;
            base.TearDownCore();
        }
        [Test]
        public void DefaultServiceContainerIsThreadSafe() {
            Exception ex = null;
            Action test = () => {
                try {
                    var key = Guid.NewGuid().ToString();
                    ServiceContainer.Default.RegisterService(key, new TestServiceBase());
                    for(int i = 0; i < iterationCount; i++)
                        ServiceContainer.Default.GetService<ITestService>(key).AddItem(i.ToString());
                } catch(Exception e) { ex = e; }
            };
            RunThreads(test);
            Assert.AreEqual(null, ex);
            Assert.AreEqual(threadCount, ServiceContainer.Default.GetServices<ITestService>().Count());
            foreach(var s in ServiceContainer.Default.GetServices<ITestService>())
                Assert.AreEqual(iterationCount, s.Items.Count());
        }

        public class TestVM : ISupportServices {
            public IServiceContainer ServiceContainer { get; set; }
            public TestVM(bool isThreadSafe) {
                ServiceContainer = new ServiceContainer(isThreadSafe);
            }
        }
        public interface ITestService {
            IEnumerable<string> Items { get; }
            void AddItem(string value);
        }
        public class TestServiceBase : ServiceBase, ITestService {
            public IEnumerable<string> Items { get { return items; } }
            List<string> items = new List<string>();
            public void AddItem(string value) {
                lock(items) {
                    items.Add(value);
                }
            }
        }
    }
#endif
}