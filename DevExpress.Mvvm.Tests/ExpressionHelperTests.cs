#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
#endif
using DevExpress.Mvvm.Native;
using System;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class ExpressionHelperTests {
        public class TestViewModel : BindableBase {
            string stringProperty;
            public string StringProperty {
                get { return stringProperty; }
                set { SetProperty(ref stringProperty, value, () => StringProperty); }
            }

            string stringProperty2;
            public string StringProperty2 {
                get { return stringProperty2; }
                set { SetProperty(ref stringProperty2, value, () => StringProperty2); }
            }

            NestedTestViewModel nestedViewModel;
            public NestedTestViewModel NestedViewModel {
                get { return nestedViewModel; }
                set { SetProperty(ref nestedViewModel, value, () => NestedViewModel); }
            }

            public int TestCommandExecuteCount { get; private set; }
            public object ExecuteParameter { get; private set; }
            public object CanExecuteParameter { get; private set; }
            public bool CanExecuteTestCommand { get; set; }
            DelegateCommand<object> testCommand;
            public DelegateCommand<object> TestCommand {
                get {
                    if(testCommand == null) {
                        testCommand = new DelegateCommand<object>(o => {
                            ExecuteParameter = o;
                            TestCommandExecuteCount++;
                        }, o => {
                            CanExecuteParameter = o;
                            return CanExecuteTestCommand;
                        });
                    }
                    return testCommand;
                }
            }


            int intProperty;
            public int IntProperty {
                get { return intProperty; }
                set { SetProperty(ref intProperty, value, () => IntProperty); }
            }

            double doubleProperty;
            public double DoubleProperty {
                get { return doubleProperty; }
                set { SetProperty(ref doubleProperty, value, () => DoubleProperty); }
            }

            public TestViewModel() {
                CanExecuteTestCommand = true;
            }
            public void SomeMethod() { }
            public void SomeMethod(int x) { }
            public string SomeMethod2() {
                return null;
            }
            public int someField;
        }
        public class NestedTestViewModel : BindableBase {

            string nestedStringProperty;

            public string NestedStringProperty {
                get { return nestedStringProperty; }
                set { SetProperty(ref nestedStringProperty, value, () => NestedStringProperty); }
            }

        }

        public int MyProperty { get; set; }
        public int GetInt() { return 0; }
        [Test]
        public void GetPropertyName() {
            Assert.AreEqual("MyProperty", ExpressionHelper.GetPropertyName(() => MyProperty));
            TestHelper.AssertThrows<ArgumentException>(() => {
                ExpressionHelper.GetPropertyName(() => GetInt());
            });
            TestHelper.AssertThrows<ArgumentNullException>(() => {
                ExpressionHelper.GetPropertyName<int>(null);
            });

            TestViewModel viewModel = null;
            Assert.AreEqual("StringProperty", ExpressionHelper.GetPropertyName(() => viewModel.StringProperty));

            TestHelper.AssertThrows<ArgumentException>(() => {
                ExpressionHelper.GetPropertyName(() => viewModel.NestedViewModel.NestedStringProperty);
            });

            Assert.AreEqual("StringProperty", ExpressionHelper.GetPropertyName<object>(() => viewModel.StringProperty));
            Assert.AreEqual("IntProperty", ExpressionHelper.GetPropertyName<object>(() => viewModel.IntProperty));
        }

        [Test]
        public void GetMemberInfo() {
            Assert.AreEqual("SomeMethod", ExpressionHelper.GetMethodName(() => SomeMethod()));
        }
        void SomeMethod() {
        }
        string SomeMethod2() {
            return null;
        }

        [Test]
        public void GetArgumentMethodStrict() {
            Assert.AreEqual("SomeMethod", ExpressionHelper.GetArgumentMethodStrict<TestViewModel>(x => x.SomeMethod()).Name);
            Assert.AreEqual("SomeMethod", ExpressionHelper.GetArgumentMethodStrict<TestViewModel>(x => x.SomeMethod(default(int))).Name);
            TestHelper.AssertThrows<ArgumentException>(() => ExpressionHelper.GetArgumentMethodStrict<ExpressionHelperTests>(x => SomeMethod()));
            TestHelper.AssertThrows<ArgumentException>(() => ExpressionHelper.GetArgumentMethodStrict<TestViewModel>(x => x.StringProperty2.ToString()));
            string s = null;
            TestHelper.AssertThrows<ArgumentException>(() => ExpressionHelper.GetArgumentMethodStrict<TestViewModel>(x => s.ToString()));
        }
        [Test]
        public void GetFunctionMethodStrict() {
            Assert.AreEqual("SomeMethod2", ExpressionHelper.GetArgumentFunctionStrict<TestViewModel, string>(x => x.SomeMethod2()).Name);
            TestHelper.AssertThrows<ArgumentException>(() => ExpressionHelper.GetArgumentFunctionStrict<ExpressionHelperTests, string>(x => SomeMethod2()));
        }
        [Test]
        public void GetArgumentPropertyStrict() {
            Assert.AreEqual("IntProperty", ExpressionHelper.GetArgumentPropertyStrict<TestViewModel, int>(x => x.IntProperty).Name);
            TestHelper.AssertThrows<InvalidCastException>(() => ExpressionHelper.GetArgumentPropertyStrict<TestViewModel, int>(x => x.someField));
            TestHelper.AssertThrows<ArgumentException>(() => ExpressionHelper.GetArgumentPropertyStrict<ExpressionHelperTests, int>(x => MyProperty));
            TestHelper.AssertThrows<ArgumentException>(() => ExpressionHelper.GetArgumentPropertyStrict<TestViewModel, int>(x => x.SomeMethod2().Length));
            string s = null;
            TestHelper.AssertThrows<ArgumentException>(() => ExpressionHelper.GetArgumentPropertyStrict<TestViewModel, int>(x => s.Length));
        }

        public interface ISomeInterface {
            string Title { get; set; }
        }
        public class PublicClass : ISomeInterface {
            public string Title { get { return null; } set { } }
        }
        public class PublicClassWithExplicitImplementation : ISomeInterface {
            public string Title { get; set; }
            string ISomeInterface.Title { get { return null; } set { } }
        }
        class PrivateClass : ISomeInterface {
            public string Title { get { return null; } set { } }
        }

        [Test]
        public void PropertyHasImplicitImplementationTest() {
            Assert.IsTrue(ExpressionHelper.PropertyHasImplicitImplementation((ISomeInterface)new PublicClass(), i => i.Title));
            Assert.IsFalse(ExpressionHelper.PropertyHasImplicitImplementation((ISomeInterface)new PublicClassWithExplicitImplementation(), i => i.Title));
#if SILVERLIGHT
            Assert.IsFalse(ExpressionHelper.PropertyHasImplicitImplementation((ISomeInterface)new PrivateClass(), i => i.Title));
#else
            Assert.IsTrue(ExpressionHelper.PropertyHasImplicitImplementation((ISomeInterface)new PrivateClass(), i => i.Title));
#endif
        }
    }
    public static class TestHelper {
        public static void AssertThrows<TException>(Action action) where TException : Exception {
            try {
                action();
            } catch(TException) {
                return;
            }
            Assert.Fail();
        }
    }
}