using NUnit.Framework;
using System.Windows;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class DesignTimeHelperTests {
        public class NestedClass {
        }
        public class TestClass {
            public int ReadonlyProp { get; private set; }
            public int IntProp { get; set; }
            public string StringProp { get; set; }
            public Visibility EnumProp { get; set; }
            public NestedClass NestedProp { get; set; }
            public TestClass RecursiveProp { get; set; }
        }
        public class TestClassWithoutPublicConstructor {
            TestClassWithoutPublicConstructor() {
            }
        }
        public class TestClassWithoutDefaultConstructor {
            public TestClassWithoutDefaultConstructor(int x) {
            }
        }
        [Test]
        public void CreateObject() {
            var obj1 = DesignTimeHelper.CreateDesignTimeObject<TestClass>();
            Assert.IsNotNull(obj1);
            Assert.AreEqual(123, obj1.IntProp);
            Assert.AreEqual("string1", obj1.StringProp);
            Assert.AreEqual(0, obj1.ReadonlyProp);
            Assert.AreEqual(default(Visibility), obj1.EnumProp);
            Assert.AreEqual(null, obj1.NestedProp);
            Assert.AreEqual(null, obj1.RecursiveProp);

            Assert.IsNull(DesignTimeHelper.CreateDesignTimeObject<TestClassWithoutPublicConstructor>());
            Assert.IsNull(DesignTimeHelper.CreateDesignTimeObject<TestClassWithoutDefaultConstructor>());
        }
        [Test]
        public void CreateObjects() {
            TestClass[] objs = DesignTimeHelper.CreateDesignTimeObjects<TestClass>(2);
            Assert.AreEqual(2, objs.Length);
            Assert.IsNotNull(objs[0]);
            Assert.AreEqual(123, objs[0].IntProp);
            Assert.IsNotNull(objs[1]);
            Assert.AreEqual(456, objs[1].IntProp);

            Assert.AreEqual(0, DesignTimeHelper.CreateDesignTimeObjects<TestClassWithoutDefaultConstructor>(2).Length);
        }
    }
}