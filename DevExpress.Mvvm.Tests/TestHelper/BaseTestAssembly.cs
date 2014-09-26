using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DevExpress {
    class BaseTestAssembly : IAssembly {
        Assembly assembly;
        LazyMethodInfo cleanup;
        LazyMethodInfo init;
        IUnitTestProvider provider;
        UnitTestHarness testHarness;

        public BaseTestAssembly(IUnitTestProvider provider, UnitTestHarness testHarness, Assembly assembly) {
            this.provider = provider;
            this.testHarness = testHarness;
            this.assembly = assembly;
            this.init = new LazyAssemblyMethodInfo(this.assembly, typeof(AssemblyInitializeAttribute));
            this.cleanup = new LazyAssemblyMethodInfo(this.assembly, typeof(AssemblyCleanupAttribute));
        }

        #region IAssembly Members
        public MethodInfo AssemblyCleanupMethod {
            get { return this.cleanup.GetMethodInfo(); }
        }
        public MethodInfo AssemblyInitializeMethod {
            get { return this.init.GetMethodInfo(); }
        }
        public string Name {
            get {
                string n = this.assembly.ToString();
                if(!n.Contains(", ")) {
                    return n;
                }
                return n.Substring(0, n.IndexOf(",", StringComparison.Ordinal));
            }
        }
        public IUnitTestProvider Provider {
            get { return provider; }
        }
        public UnitTestHarness TestHarness {
            get { return testHarness; }
        }

        public ICollection<ITestClass> GetTestClasses() {
            ICollection<Type> classes = ReflectionUtility.GetTypesWithAttribute(this.assembly, typeof(TestFixtureAttribute));
            List<ITestClass> list = new List<ITestClass>();
            foreach(Type type in classes) {
                ITestClass testClass = new BaseTestClass(this, type);
                if(!testClass.Ignore)
                    list.Add(testClass);
            }
            return list;
        }
        #endregion
    }
}