#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
#endif
using System;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class ServiceContainerTests {
        public interface IService1 { }
        public interface IService2 { }
        public class TestService { }
        public class TestService1 : IService1 { }
        public class TestService2 : IService2 { }
        public class TestService1_2 : IService1, IService2 { }
        [Test]
        public void AddServicesWithoutKey() {
            ServiceContainer container = new ServiceContainer(null);
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
        [Test, ExpectedException(typeof(ArgumentException))]
        public void GetServiceNotByInterfaceType() {
            ServiceContainer container = new ServiceContainer(null);
            TestService1 svc1 = new TestService1();
            container.RegisterService(svc1);
            container.GetService<TestService1>();
        }
        [Test, ExpectedException(typeof(ArgumentException))]
        public void GetServiceNotByInterfaceTypeWinKeyName() {
            ServiceContainer container = new ServiceContainer(null);
            container.GetService<TestService1>("svc1_");
        }
        [Test]
        public void AddServicesWithKey() {
            ServiceContainer container = new ServiceContainer(null);
            TestService1 svc1 = new TestService1();
            TestService1 svc1_ = new TestService1();
            container.RegisterService("svc1", svc1);
            container.RegisterService("svc1_", svc1_);
            Assert.AreEqual(svc1, container.GetService<IService1>());
            Assert.AreEqual(svc1, container.GetService<IService1>("svc1"));
            Assert.AreEqual(svc1_, container.GetService<IService1>("svc1_"));
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
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void RegisterNull() {
            var child = new TestSupportServices();
            child.ServiceContainer.RegisterService(null);
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void RegisterNullByKey() {
            var child = new TestSupportServices();
            child.ServiceContainer.RegisterService("key", null);
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void GetServiecByNullKey() {
            var child = new TestSupportServices();
            child.ServiceContainer.GetService<IService1>(null);
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
}