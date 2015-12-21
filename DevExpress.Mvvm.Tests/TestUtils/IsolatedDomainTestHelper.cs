using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;

namespace DevExpress.Mvvm.UI.Tests {
    class NewDomainTestHelper<T> : IDisposable {
        AppDomain domain;
        T testObject;
        public T TestObject { get { return testObject; } }
        public NewDomainTestHelper()
            : this(AppDomain.CurrentDomain.SetupInformation) {
        }
        public NewDomainTestHelper(AppDomainSetup setupInformation) {
            domain = AppDomain.CreateDomain("TestDomain", AppDomain.CurrentDomain.Evidence, setupInformation);
            testObject = (T)domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }
        public NewDomainTestHelper(AppDomainSetup setupInformation, PermissionSet permissionSet, params StrongName[] fullTrustAssemblies) {
            domain = AppDomain.CreateDomain("TestDomain", null, setupInformation, permissionSet, fullTrustAssemblies);
            testObject = (T)domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }
        public bool IsAssemblyLoaded(string assemblyName) {
            Assembly[] loadedAssemblies = domain.GetAssemblies();
            foreach(Assembly assembly in loadedAssemblies) {
                if(assembly.FullName.Contains(assemblyName))
                    return true;
            }
            return false;
        }
        public void Dispose() {
            if(domain != null)
                AppDomain.Unload(domain);
        }
    }

    public static class IsolatedDomainTestHelper {
        class IsolatedDomainTester : MarshalByRefObject {
            public void Test(MethodInfo actionInfo, params object[] args) {
                object target = Activator.CreateInstance(actionInfo.DeclaringType);
                MethodInfo setupFixture = target.GetType().GetMethods().FirstOrDefault((mi) => mi.GetCustomAttributes(typeof(TestFixtureSetUpAttribute), true).FirstOrDefault() != null);
                MethodInfo setup = target.GetType().GetMethods().FirstOrDefault((mi) => mi.GetCustomAttributes(typeof(SetUpAttribute), true).FirstOrDefault() != null);
                MethodInfo tearDownFixture = target.GetType().GetMethods().FirstOrDefault((mi) => mi.GetCustomAttributes(typeof(TestFixtureTearDownAttribute), true).FirstOrDefault() != null);
                MethodInfo tearDown = target.GetType().GetMethods().FirstOrDefault((mi) => mi.GetCustomAttributes(typeof(TearDownAttribute), true).FirstOrDefault() != null);

                if(setupFixture != null)
                    setupFixture.Invoke(target, null);
                if(setup != null)
                    setup.Invoke(target, null);
                actionInfo.Invoke(target, args);
                if(tearDown != null)
                    tearDown.Invoke(target, null);
                if(tearDownFixture != null)
                    tearDownFixture.Invoke(target, null);
            }
        }
        static void RunTestCore(MethodInfo actionInfo, params object[] args) {
            using(var helper = new NewDomainTestHelper<IsolatedDomainTester>()) {
                helper.TestObject.Test(actionInfo, args);
            }
        }
        public static void RunTest(Action test) {
            RunTestCore(test.Method, new object[0]);
        }
        public static void RunTest(Action<object> test, object arg) {
            RunTestCore(test.Method, arg);
        }
    }
}