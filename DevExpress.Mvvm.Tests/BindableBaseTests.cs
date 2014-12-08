#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NETFX_CORE
using DevExpress.TestFramework.NUnit;
#else
using NUnit.Framework;
#endif
using System;
using System.Collections.Generic;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class BindableBaseTests {
        [Test]
        public void PropertyChangedCallbackAndPropertyChangedEventOrderTest() {
            BindableBaseTest bb = new BindableBaseTest();
            bool propertyChangedCalled = false;
            bb.PropertyChanged += (s, e) => {
                propertyChangedCalled = true;
                Assert.AreEqual("SomeProperty7", e.PropertyName);
                Assert.AreEqual(0, bb.ChangedCallbackCallCount);
            };
            bb.SomeProperty7 = 777;
            Assert.IsTrue(propertyChangedCalled);
            Assert.AreEqual(1, bb.ChangedCallbackCallCount);
        }
        [Test]
        public void OnPropertiesChangedTest() {
            BindableBaseTest bb = new BindableBaseTest();
            int count = 0;
            List<string> propNames = new List<string>();
            bb.PropertyChanged += (o, e) => { count++; propNames.Add(e.PropertyName); };
            bb.SomeProperty9 = 150;
            Assert.AreEqual("SomeProperty", propNames[0]);
            Assert.AreEqual("SomeProperty9", propNames[1]);
            Assert.AreEqual(2, count);

            propNames.Clear();
            bb.SomeProperty10 = 150;
            Assert.AreEqual("SomeProperty2", propNames[0]);
            Assert.AreEqual("SomeProperty10", propNames[1]);
            Assert.AreEqual(4, count);

            propNames.Clear();
            bb.SomeProperty11 = 150;
            Assert.AreEqual("SomeProperty2", propNames[0]);
            Assert.AreEqual("SomeProperty3", propNames[1]);
            Assert.AreEqual("SomeProperty10", propNames[2]);
            Assert.AreEqual(7, count);

            propNames.Clear();
            bb.SomeProperty12 = 150;
            Assert.AreEqual("SomeProperty2", propNames[0]);
            Assert.AreEqual("SomeProperty3", propNames[1]);
            Assert.AreEqual("SomeProperty10", propNames[2]);
            Assert.AreEqual("SomeProperty11", propNames[3]);
            Assert.AreEqual(11, count);

            propNames.Clear();
            bb.SomeProperty13 = 150;
            Assert.AreEqual("SomeProperty2", propNames[0]);
            Assert.AreEqual("SomeProperty3", propNames[1]);
            Assert.AreEqual("SomeProperty10", propNames[2]);
            Assert.AreEqual("SomeProperty11", propNames[3]);
            Assert.AreEqual("SomeProperty12", propNames[4]);
            Assert.AreEqual(16, count);
        }
        [Test]
        public void OnPropertyChangedTest() {
            BindableBaseTest bb = new BindableBaseTest();
            int count = 0;
            string propName = null;
            bb.SomeProperty = 50;
            bb.PropertyChanged += (o, e) => { count++; propName = e.PropertyName; };
            bb.SomeProperty = 150;
            Assert.AreEqual("SomeProperty", propName);
            Assert.AreEqual(1, count);
            bb.SomeProperty8 = 150;
            Assert.AreEqual("SomeProperty8", propName);
            Assert.AreEqual(2, count);
        }
        [Test]
        public void RaisePropertyChangedWithNoParametersTest_B251476() {
            BindableBaseTest bb = new BindableBaseTest();
            bb.RaisePropertyChanged(null);
            bb.RaisePropertyChanged(string.Empty);
            bb.RaisePropertyChanged();
            bb.RaisePropertiesChanged(null);
            bb.RaisePropertiesChanged(string.Empty);
        }

        [Test]
        public void SetPropertyTest() {
            BindableBaseTest bb = new BindableBaseTest();
            int count = 0;
            string propName = null;
            bb.SomeProperty2 = 50;
            bb.PropertyChanged += (o, e) => { count++; propName = e.PropertyName; };
            bb.SomeProperty2 = 150;
            Assert.AreEqual("SomeProperty2", propName);
            Assert.AreEqual(1, count);
            Assert.AreEqual(150, bb.SomeProperty2);

            bb.SomeProperty2 = 150;
            Assert.AreEqual(1, count);
        }

        [Test]
        public void SetPropertyWithLambdaTest() {
            BindableBaseTest bb = new BindableBaseTest();
            int count = 0;
            string propName = null;
            bb.SomeProperty3 = 50;
            bb.PropertyChanged += (o, e) => { count++; propName = e.PropertyName; };
            bb.SomeProperty3 = 150;
            Assert.AreEqual("SomeProperty3", propName);
            Assert.AreEqual(1, count);
            Assert.AreEqual(150, bb.SomeProperty3);

            bb.SomeProperty3 = 150;
            Assert.AreEqual(1, count);
        }
#if !NETFX_CORE
        [Test, ExpectedException(typeof(ArgumentException))]
        public void SetPropertyInvalidLambdaTest() {
            BindableBaseTest bb = new BindableBaseTest();
            bb.SomeProperty4 = 150;
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void SetPropertyInvalidLambdaTest2() {
            BindableBaseTest bb = new BindableBaseTest();
            bb.SomeProperty5 = 150;
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void SetPropertyInvalidLambdaTest3() {
            BindableBaseTest bb = new BindableBaseTest();
            bb.SomeProperty6 = 150;
        }
#endif
        [Test]
        public void SetPropertyWithCallbackTest() {
            BindableBaseTest bb = new BindableBaseTest();
            int count = 0;
            string propName = null;
            bb.SomeProperty7 = 50;
            Assert.AreEqual(1, bb.ChangedCallbackCallCount);
            bb.PropertyChanged += (o, e) => { count++; propName = e.PropertyName; };
            bb.SomeProperty7 = 150;

            Assert.AreEqual(2, bb.ChangedCallbackCallCount);
            Assert.AreEqual("SomeProperty7", propName);
            Assert.AreEqual(1, count);
            Assert.AreEqual(150, bb.SomeProperty7);

            bb.SomeProperty7 = 150;
            Assert.AreEqual(1, count);
        }
        #region property bag test
        class PropertyBagViewModel : BindableBase {
            public bool? IntPropertySetValueResult;
            public int IntProperty {
                get { return GetProperty(() => IntProperty); }
                set { IntPropertySetValueResult = SetProperty(() => IntProperty, value); }
            }

            public int StringPropertyChangedCount;
            public string StringProperty {
                get { return GetProperty(() => StringProperty); }
                set { SetProperty(() => StringProperty, value, () => StringPropertyChangedCount ++); }
            }
        }
        [Test]
        public void GetSetPropertyTest() {
            var viewModel = new PropertyBagViewModel();

            int propertyChangedCount = 0;
            string propName = null;
            viewModel.PropertyChanged += (o, e) => { propertyChangedCount++; propName = e.PropertyName; };

            Assert.AreEqual(0, viewModel.IntProperty);
            Assert.IsFalse(viewModel.PropertyBag.ContainsKey("IntProperty"));

            viewModel.IntProperty = 0;
            Assert.AreEqual(0, viewModel.IntProperty);
            Assert.IsFalse(viewModel.PropertyBag.ContainsKey("IntProperty"));
            Assert.AreEqual(false, viewModel.IntPropertySetValueResult.Value);
            Assert.AreEqual(0, propertyChangedCount);
            Assert.AreEqual(null, propName);

            viewModel.IntProperty = 9;
            Assert.AreEqual(9, viewModel.IntProperty);
            Assert.IsTrue(viewModel.PropertyBag.ContainsKey("IntProperty"));
            Assert.AreEqual(true, viewModel.IntPropertySetValueResult.Value);
            Assert.AreEqual(1, propertyChangedCount);
            Assert.AreEqual("IntProperty", propName);

            viewModel.IntProperty = 0;
            Assert.AreEqual(0, viewModel.IntProperty);
            Assert.IsTrue(viewModel.PropertyBag.ContainsKey("IntProperty"));
            Assert.AreEqual(true, viewModel.IntPropertySetValueResult.Value);
            Assert.AreEqual(2, propertyChangedCount);
            Assert.AreEqual("IntProperty", propName);

            viewModel.StringProperty = null;
            Assert.AreEqual(null, viewModel.StringProperty);
            Assert.AreEqual(0, viewModel.StringPropertyChangedCount);
            Assert.AreEqual(2, propertyChangedCount);

            viewModel.StringProperty = string.Empty;
            Assert.AreEqual(string.Empty, viewModel.StringProperty);
            Assert.AreEqual(1, viewModel.StringPropertyChangedCount);
            Assert.AreEqual(3, propertyChangedCount);
            Assert.AreEqual("StringProperty", propName);

            viewModel.StringProperty = "x";
            Assert.AreEqual("x", viewModel.StringProperty);
            Assert.AreEqual(2, viewModel.StringPropertyChangedCount);

            viewModel.StringProperty = "x";
            Assert.AreEqual("x", viewModel.StringProperty);
            Assert.AreEqual(2, viewModel.StringPropertyChangedCount);
        }
        #endregion
    }

    class BindableBaseTest : BindableBase {
        public int ChangedCallbackCallCount { get; private set; }
        int someProperty7;
        public int SomeProperty7 {
            get { return someProperty7; }
            set {
                SetProperty(ref someProperty7, value, () => SomeProperty7, () => {
                    ChangedCallbackCallCount++;
                });
            }
        }

        int someProperty6;
        public int SomeProperty6 {
            get { return someProperty6; }
            set { SetProperty(ref someProperty6, value, () => this[0]); }
        }
        int this[int index] { get { return 0; }}

        int someProperty5;
        public int SomeProperty5 {
            get { return someProperty5; }
            set { SetProperty(ref someProperty5, value, () => GetHashCode()); }
        }
        int someProperty4;
        public int SomeProperty4 {
            get { return someProperty4; }
            set { SetProperty(ref someProperty4, value, () => 1); }
        }
        int someProperty3;
        public int SomeProperty3 {
            get { return someProperty3; }
            set { SetProperty(ref someProperty3, value, () => SomeProperty3); }
        }
        int someProperty2;
        public int SomeProperty2 {
            get { return someProperty2; }
            set { SetProperty(ref someProperty2, value, "SomeProperty2"); }
        }
        public int SomeProperty {
            set {
                RaisePropertyChanged("SomeProperty");
            }
        }
        public int SomeProperty8 {
            get { return 0; }
            set {
                RaisePropertyChanged(() => SomeProperty8);
            }
        }
        public int SomeProperty9 {
            set {
                RaisePropertiesChanged("SomeProperty", "SomeProperty9");
            }
        }
        public int SomeProperty10 {
            get { return 0; }
            set {
                RaisePropertiesChanged(() => SomeProperty2, () => SomeProperty10);
            }
        }
        public int SomeProperty11 {
            get { return 0; }
            set {
                RaisePropertiesChanged(() => SomeProperty2, () => SomeProperty3, () => SomeProperty10);
            }
        }
        public int SomeProperty12 {
            get { return 0; }
            set {
                RaisePropertiesChanged(() => SomeProperty2, () => SomeProperty3, () => SomeProperty10, () => SomeProperty11);
            }
        }
        public int SomeProperty13 {
            get { return 0; }
            set {
                RaisePropertiesChanged(() => SomeProperty2, () => SomeProperty3, () => SomeProperty10, () => SomeProperty11, () => SomeProperty12);
            }
        }

        public new void RaisePropertyChanged(string propertyName) {
            base.RaisePropertyChanged(propertyName);
        }
        public new void RaisePropertyChanged() {
            base.RaisePropertyChanged();
        }
        public new void RaisePropertiesChanged(params string[] propertyNames) {
            base.RaisePropertiesChanged(propertyNames);
        }
    }
}