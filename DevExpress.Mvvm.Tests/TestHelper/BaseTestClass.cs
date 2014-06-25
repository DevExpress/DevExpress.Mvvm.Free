using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DevExpress {
    class BaseTestClass : ITestClass {
        IDictionary<Methods, LazyMethodInfo> _m;
        ICollection<ITestMethod> _tests;
        bool _testsLoaded;
        Type _type;


        public BaseTestClass(IAssembly assembly, Type testClassType)
            : this(assembly) {
            this._type = testClassType;
            if(this._type == null) {
                throw new ArgumentNullException("testClassType");
            }
            this._m[Methods.ClassCleanup] = new LazyMethodInfo(this._type, typeof(TestFixtureTearDownAttribute));
            this._m[Methods.ClassInitialize] = new LazyMethodInfo(this._type, typeof(TestFixtureSetUpAttribute));
            this._m[Methods.TestCleanup] = new LazyMethodInfo(this._type, typeof(TearDownAttribute));
            this._m[Methods.TestInitialize] = new LazyMethodInfo(this._type, typeof(SetUpAttribute));
        }
        private BaseTestClass(IAssembly assembly) {
            this._tests = new List<ITestMethod>();
            this._m = new Dictionary<Methods, LazyMethodInfo>(4);
            this._m[Methods.ClassCleanup] = null;
            this._m[Methods.ClassInitialize] = null;
            this._m[Methods.TestCleanup] = null;
            this._m[Methods.TestInitialize] = null;
            this.Assembly = assembly;
        }

        public ICollection<ITestMethod> GetTestMethods() {
            if(!this._testsLoaded) {
                ICollection<MethodInfo> methods = ReflectionUtility.GetMethodsWithAttribute(this._type, typeof(TestAttribute));
                this._tests = new List<ITestMethod>(methods.Count);
                foreach(MethodInfo method in methods) {
                    if(ReflectionUtility.HasAttribute(method, typeof(IgnoreAttribute))) continue;
                    CategoryAttribute category = (CategoryAttribute)ReflectionUtility.GetAttribute(method, typeof(CategoryAttribute));
                    if(category != null && TestHelper.IgnoredCategories.Contains(category.Category)) continue;
                    this._tests.Add(new TestMethod(method));
                }
                this._testsLoaded = true;
            }
            return this._tests;
        }

        public IAssembly Assembly { get; private set; }
        public MethodInfo ClassCleanupMethod {
            get {
                if(this._m[Methods.ClassCleanup] != null) {
                    return this._m[Methods.ClassCleanup].GetMethodInfo();
                }
                return null;
            }
        }
        public MethodInfo ClassInitializeMethod {
            get {
                if(this._m[Methods.ClassInitialize] != null) {
                    return this._m[Methods.ClassInitialize].GetMethodInfo();
                }
                return null;
            }
        }
        public bool Ignore {
            get { return ReflectionUtility.HasAttribute(this._type, typeof(IgnoreAttribute)); }
        }
        public string Name {
            get { return this._type.Name; }
        }
        public MethodInfo TestCleanupMethod {
            get {
                if(this._m[Methods.TestCleanup] != null) {
                    return this._m[Methods.TestCleanup].GetMethodInfo();
                }
                return null;
            }
        }
        public MethodInfo TestInitializeMethod {
            get {
                if(this._m[Methods.TestInitialize] != null) {
                    return this._m[Methods.TestInitialize].GetMethodInfo();
                }
                return null;
            }
        }
        public Type Type {
            get { return this._type; }
        }

        internal enum Methods {
            ClassInitialize,
            ClassCleanup,
            TestInitialize,
            TestCleanup
        }
        public string Namespace {
            get { return this._type.Namespace; }
        }
    }
}