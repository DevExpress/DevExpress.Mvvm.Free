using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.IO;

namespace DevExpress.Mvvm.UI.Tests {
    public class NewDomainTestHelper<T> : IDisposable {
        class LoadedAssembliesHelper : MarshalByRefObject {
            public bool IsAssemblyLoaded(string assemblyName) {
                Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach(Assembly assembly in loadedAssemblies) {
                    if(assembly.FullName.Contains(assemblyName))
                        return true;
                }
                return false;
            }
        }
        public AppDomain AppDomain { get { return domain; } }
        AppDomain domain;
        T testObject;
        public T TestObject { get { return testObject; } }
        public NewDomainTestHelper()
            : this(AppDomain.CurrentDomain.SetupInformation) {
        }
        public NewDomainTestHelper(AppDomainSetup setupInformation) {
#if DXCORE3
            domain = AppDomain.CreateDomain("TestDomain");
#else
            domain = AppDomain.CreateDomain("TestDomain", AppDomain.CurrentDomain.Evidence, setupInformation);
#endif
            testObject = (T)domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }
        public NewDomainTestHelper(AppDomainSetup setupInformation, PermissionSet permissionSet, params StrongName[] fullTrustAssemblies) {
#if DXCORE3
            domain = AppDomain.CreateDomain("TestDomain");
#else
            domain = AppDomain.CreateDomain("TestDomain", null, setupInformation, permissionSet, fullTrustAssemblies);
#endif
            testObject = (T)domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }
        public bool IsAssemblyLoaded(string assemblyName) {
#if DXCORE3
            LoadedAssembliesHelper helper = (LoadedAssembliesHelper)domain.CreateInstanceAndUnwrap(this.GetType().Assembly.FullName, typeof(LoadedAssembliesHelper).FullName);
            return helper.IsAssemblyLoaded(assemblyName);
#else
            Assembly[] loadedAssemblies = domain.GetAssemblies();
            foreach(Assembly assembly in loadedAssemblies) {
                if(assembly.FullName.Contains(assemblyName))
                    return true;
            }
            return false;
#endif
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
                MethodInfo setupFixture = target.GetType().GetMethods().FirstOrDefault((mi) => mi.GetCustomAttributes(typeof(OneTimeSetUpAttribute), true).FirstOrDefault() != null);
                MethodInfo setup = target.GetType().GetMethods().FirstOrDefault((mi) => mi.GetCustomAttributes(typeof(SetUpAttribute), true).FirstOrDefault() != null);
                MethodInfo tearDownFixture = target.GetType().GetMethods().FirstOrDefault((mi) => mi.GetCustomAttributes(typeof(OneTimeTearDownAttribute), true).FirstOrDefault() != null);
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
                helper.AppDomain.AssemblyResolve += AppDomain_AssemblyResolve;
                helper.TestObject.Test(actionInfo, args);
                helper.AppDomain.AssemblyResolve -= AppDomain_AssemblyResolve;
            }
        }

        static Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            if(args.Name.ToLower().Contains("nunit.framework"))
                return Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, "nunit.framework.dll"));
            return null;
        }
        public static void RunTest(Action test) {
            RunTestCore(test.Method, new object[0]);
        }
        public static void RunTest(Action<object> test, object arg) {
            RunTestCore(test.Method, arg);
        }
    }
}