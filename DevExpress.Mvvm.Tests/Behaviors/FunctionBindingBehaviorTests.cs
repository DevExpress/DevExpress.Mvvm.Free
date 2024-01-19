using NUnit.Framework;
using System;
using System.Windows.Controls;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.DataAnnotations;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class FunctionBindingBehaviorTests : BaseWpfFixture {
        #region Props
        MBBControl View { get; set; }
        FunctionBindingBehavior Behavior { get; set; }
        #endregion

        #region Helpers
        void PrepareBehavior(string propName, string methodName, object target = null, object source = null, bool attachToView = true) {
            Behavior = new FunctionBindingBehavior();
            Behavior.Property = propName;
            Behavior.Target = target;
            Behavior.Function = methodName;
            Behavior.Source = source;
            if(attachToView)
                Interaction.GetBehaviors(View).Add(Behavior);
        }
        protected override void SetUpCore() {
            base.SetUpCore();
            View = new MBBControl();
            Window.Content = View;
        }
        protected override void TearDownCore() {
            Behavior = null;
            View = null;
            base.TearDownCore();
        }
        #endregion

        [Test]
        public void ExceptionOnVoidSourceMethod_Test() {
            PrepareBehavior("Data", "");
            Behavior.Arg1 = new { };
            View.DataContext = new MBBObject();
            AssertHelper.AssertThrows<ArgumentException>(() => Behavior.Function = "GetA",
                x => Assert.IsTrue(x.Message.Contains("MBBObject.GetA")));
        }
        [Test]
        public void TraceMessageOnSourceMethodNotFound_Test() {
            MBBTraceListener listener = new MBBTraceListener();
            Trace.Listeners.Add(listener);
            PrepareBehavior("Data", "GetABC", View, new MBBObject());
            Assert.IsTrue(listener.Output.Contains("error: Cannot find function with name GetABC in the MBBObject class."));
            Trace.Listeners.Remove(listener);
            listener.Dispose();
        }
        [Test]
        public void TraceMessageOnTargetPropertyNotFound_Test() {
            MBBTraceListener listener = new MBBTraceListener();
            Trace.Listeners.Add(listener);
            PrepareBehavior("Data_", "GetA", View, new MBBObject());
            Assert.IsTrue(listener.Output.Contains("error: Cannot find property with name Data_ in the MBBControl class."));
            Trace.Listeners.Remove(listener);
            listener.Dispose();
        }
        [Test]
        public void SetDataContextDepPropValue_Test00() {
            View.DataContext = new MBBObject();
            PrepareBehavior("IntData", "GetA");
            Assert.AreEqual(6, View.IntData);
        }
        [Test]
        public void SetDataContextDepPropValue_Test01() {
            View.DataContext = new MBBObject();
            PrepareBehavior("Data", "GetO");
            Behavior.Arg1 = new object { };
            Assert.AreEqual(Behavior.Arg1, View.Data);
        }
        [Test]
        public void SetDataContextDepPropValueWithoutSetter_Test() {
            PrepareBehavior("IntData2", "GetA");
            View.DataContext = new MBBObject();
            Assert.AreEqual(6, View.GetIntData2());
        }
        [Test]
        public void SetDataContextPropValue_Test00() {
            PrepareBehavior("IntData1", "GetA");
            View.DataContext = new MBBObject();
            Assert.AreEqual(6, View.IntData1);
        }
        [Test]
        public void SetDataContextPropValue_Test01() {
            PrepareBehavior("Data1", "GetO");
            View.DataContext = new MBBObject();
            Behavior.Arg1 = new object { };
            Assert.AreEqual(Behavior.Arg1, View.Data1);
        }
        [Test]
        public void UseDataContextIfSourceNotSpecified_Test02() {
            var source = new MBBObject();
            Window.DataContext = source;
            FrameworkContentElement wce = new FrameworkContentElement();
            PrepareBehavior("Name", "GetString", null, null, false);
            Window.Content = wce;
            Behavior.Arg1 = "WCE_Test02";
            Interaction.GetBehaviors(wce).Add(Behavior);
            Assert.AreEqual("WCE_Test02", wce.Name);
        }
        [Test]
        public void UseDataContextIfSourceNotSpecified_Test03() {
            var source = new MBBObject();
            FrameworkContentElement wce = new FrameworkContentElement();
            wce.DataContext = source;
            PrepareBehavior("Name", "GetString", null, null, false);
            Window.Content = wce;
            Behavior.Arg1 = "WCE_Test02";
            Interaction.GetBehaviors(wce).Add(Behavior);
            Assert.AreEqual("WCE_Test02", wce.Name);
        }
        [Test]
        public void TargetPropertyNotChangedUntilBehaviorAttached_Test() {
            var source = new MBBObject();
            PrepareBehavior("IntData", "GetA", View, source, false);
            Assert.AreEqual(-1, View.IntData);
            Behavior.Arg1 = "10";
            Assert.AreEqual(-1, View.IntData);
            Behavior.Arg2 = 10d;
            Assert.AreEqual(-1, View.IntData);
            Interaction.GetBehaviors(View).Add(Behavior);
            Assert.AreEqual(2, View.IntData);
        }
        [Test]
        public void TargetPropertyChangedOnArgsChanged_Test() {
            var source = new MBBObject();
            PrepareBehavior("DoubleData", "GetD", View, source);
            Behavior.Arg1 = 10d;
            Assert.AreEqual(10d, View.DoubleData);
            Behavior.Arg1 = 1d;
            Assert.AreEqual(1d, View.DoubleData);
        }
        [Test]
        public void TargetPropertyChangedOnConverterChanged_Test() {
            var source = new MBBObject();
            PrepareBehavior("Data", "GetD", View, source);
            Behavior.Arg1 = 10d;
            Assert.AreEqual(10d, View.Data);
            Assert.IsTrue(View.Data is double);
            Behavior.Converter = new DoubleToIntConverter();
            Assert.AreEqual(10, View.Data);
            Assert.IsTrue(View.Data is int);
        }
        [Test]
        public void PassParameterToConverter_Test() {
            var source = new MBBObject();
            DoubleToIntConverter converter = new DoubleToIntConverter();
            PrepareBehavior("Data", "GetA", View, source);
            Behavior.Converter = converter;
            Behavior.ConverterParameter = 5;
            Behavior.Arg1 = 10;
            Assert.AreEqual(5, converter.Parameter);
        }
        [Test]
        public void TargetPropertyNotChangedOnUnsetObject_Test() {
            var source = new MBBObject();
            PrepareBehavior("Data", "GetO", View, source);
            object value = new object();
            Behavior.Arg1 = value;
            Assert.AreEqual(value, View.Data);
            Behavior.Function = "GetUnsetValue";
            Assert.AreEqual(value, View.Data);
            IValueConverter converter = new BooleanToObjectConverter() {
                TrueValue = 12d,
                FalseValue = DependencyProperty.UnsetValue
            };
            Behavior.Arg1 = false;
            Assert.AreEqual(value, View.Data);
            Behavior.Function = "GetB";
            Assert.AreEqual(false, View.Data);
            Behavior.Converter = converter;
            Assert.AreEqual(false, View.Data);
            Behavior.Arg1 = true;
            Assert.AreEqual(12d, View.Data);
        }
        [Test]
        public void UseDefaultParamValueIfArgNotSet_Test() {
            var source = new MBBObject();
            DoubleToIntConverter converter = new DoubleToIntConverter();
            PrepareBehavior("IntData", "GetAD", View, source);
            Assert.AreEqual(10, View.IntData);
            Behavior.Arg1 = 12;
            Assert.AreEqual(12, View.IntData);
            Behavior.Arg1 = null;
            Assert.AreEqual(0, View.IntData);
            Binding binding = new Binding() {
                Path = new PropertyPath("Data"),
                Source = View
            };
            Behavior.Arg1 = 12;
            BindingOperations.SetBinding(Behavior, FunctionBindingBehavior.Arg1Property, binding);
            Assert.AreEqual(0, View.IntData);
            View.Data = 15;
            Assert.AreEqual(15, View.IntData);
        }
        [Test]
        public void CallSourceStaticMethod_Test() {
            var source = new MBBObject();
            PrepareBehavior("Data", "GetA", View, source);
            Behavior.Arg1 = 0f;
            Assert.AreEqual(0f, View.Data);
        }
        [Test]
        public void UseStaticClassAsSource_Test() {
            PrepareBehavior("IntData", "GetA", View, typeof(MBBObjectStatic));
            Assert.AreEqual(3, View.IntData);
        }
        [Test]
        public void SetTargetStaticProperty_Test() {
            var source = new MBBObject();
            PrepareBehavior("StringStatic", "GetString", View, source);
            Assert.IsNull(MBBControl.StringStatic);
            Behavior.Arg1 = "some string";
            Assert.AreEqual("some string", MBBControl.StringStatic);
        }
        [Test]
        public void ArgsRanking_Test00() {
            PrepareBehavior("Data", "GetList", View, new MBBObject());
            Behavior.Arg1 = 1;
            Behavior.Arg2 = 2;
            Behavior.Arg3 = 3;
            Behavior.Arg4 = 4;
            Behavior.Arg5 = 5;
            Behavior.Arg6 = 6;
            Behavior.Arg7 = 7;
            Behavior.Arg8 = 8;
            Behavior.Arg9 = 9;
            Behavior.Arg10 = 10;
            Behavior.Arg11 = 11;
            Behavior.Arg12 = 12;
            Behavior.Arg13 = 13;
            Behavior.Arg14 = 14;
            Behavior.Arg15 = 15;

            int count = 1;
            foreach(int i in (List<int>)View.Data) {
                Assert.AreEqual(count++, i);
            }
        }
        [Test]
        public void ArgsRanking_Test01() {
            PrepareBehavior("Data", "GetList", View, new MBBObject());
            Behavior.Arg10 = 10;
            Behavior.Arg11 = 11;
            Behavior.Arg12 = 12;
            Behavior.Arg13 = 13;
            Behavior.Arg14 = 14;
            Behavior.Arg15 = 15;

            int count = 1;
            foreach(int i in (List<int>)View.Data) {
                Assert.AreEqual((count < 10 ? 0 : count), i);
                count++;
            }

            Behavior.Arg1 = 1;
            Behavior.Arg2 = 2;
            Behavior.Arg3 = 3;
            Behavior.Arg4 = 4;
            Behavior.Arg5 = 5;
            count = 1;
            foreach(int i in (List<int>)View.Data) {
                Assert.AreEqual((count < 10 && count > 5 ? 0 : count), i);
                count++;
            }
        }
        [Test]
        public void ArgsConvert_Test() {
            PrepareBehavior("Data", "GetList", View, new MBBObject());
            Behavior.Arg1 = 1f;
            Behavior.Arg2 = 2d;
            Behavior.Arg3 = 3u;
            Behavior.Arg4 = 4L;
            Behavior.Arg5 = "5";
            Behavior.Arg6 = 6uL;
            Behavior.Arg7 = (short)7;
            Behavior.Arg8 = (byte)8;

            int count = 1;
            foreach(int i in (List<int>)View.Data) {
                Assert.AreEqual(count++, i);
                if(count > 8)
                    break;
            }
        }
        [Test]
        public void UpdateMethodBinding_Test() {
            var source = MbbPocoObject.Create();
            PrepareBehavior("IntData", "GetLocalProperty", View, source);
            Assert.AreEqual(0, View.IntData);
            source.LocalProperty = 10;
            Assert.AreEqual(0, View.IntData);
            source.UpdateFunctionBinding(x => x.GetProperty(1));
            Assert.AreEqual(0, View.IntData);
            source.UpdateFunctionBinding(x => x.GetLocalProperty());
            Assert.AreEqual(10, View.IntData);
        }
        [Test]
        public void DataContextShouldNotAffectBehaviorResultIfSourceSet_Test() {
            var source = new MBBObject();
            PrepareBehavior("DoubleData", "GetD", View, source);
            Behavior.Arg1 = 10d;
            Assert.AreEqual(10d, View.DoubleData);
            View.DoubleData = 0;
            View.DataContext = new MBBObject();
            Assert.AreEqual(0d, View.DoubleData);
            Behavior.Source = null;
            Assert.AreEqual(10d, View.DoubleData);
        }
    }

    class DoubleToIntConverter : IValueConverter {
        public object Parameter { get; private set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Parameter = parameter;

            return value == null ? 0 : System.Convert.ToInt32(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
    class MBBControl : Control {
        #region Static
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(MBBControl), new PropertyMetadata(null));
        public static readonly DependencyProperty IntDataProperty =
            DependencyProperty.Register("IntData", typeof(int), typeof(MBBControl), new PropertyMetadata(-1));
        public static readonly DependencyProperty IntData2Property =
            DependencyProperty.Register("IntData2", typeof(int), typeof(MBBControl), new PropertyMetadata(-1));
        #endregion

        #region DependencyProperty
        public object Data {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        public int IntData {
            get { return (int)GetValue(IntDataProperty); }
            set { SetValue(IntDataProperty, value); }
        }
        #endregion

        #region Props
        public object Data1 { get; set; }
        public int IntData1 { get; set; }
        public double DoubleData { get; set; }
        public static string StringStatic { get; set; }
        #endregion

        public int GetIntData2() {
            return (int)GetValue(IntData2Property);
        }
    }
    static class MBBObjectStatic {
        public static int GetA(int i, DateTime obj, double d = 13) {
            return 3;
        }
    }
    class MBBObject {
        public static int GetA(float i) {
            return 0;
        }
        public List<int> GetList(int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8, int i9, int i10, int i11, int i12, int i13, int i14, int i15) {
            return new List<int>() { i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15 };
        }
        public int GetAD(int i = 10) {
            return i;
        }
        public double GetD(double d) {
            return d;
        }
        public int GetA(int i, double d) {
            return 2;
        }
        public int GetA(int i, DateTime obj, double d = 13) {
            return 3;
        }
        public int GetA(int i, object obj) {
            return 4;
        }
        int GetA(string val) {
            return 5;
        }
        protected int GetA() {
            return 6;
        }
        void GetA(object obj) {
        }
        bool GetB(bool value) {
            return value;
        }
        string GetString(string val) {
            return val;
        }
        public object GetO(object obj) {
            return obj;
        }
        public object GetOOOO() {
            return null;
        }
        public int A { get; set; }
        public object GetUnsetValue() {
            return DependencyProperty.UnsetValue;
        }
    }
    public class MbbPocoObject {
        public static MbbPocoObject Create() {
            return ViewModelSource.Create(() => new MbbPocoObject());
        }
        protected MbbPocoObject() { }
        public int LocalProperty { get; set; }
        [Command(false)]
        public int GetLocalProperty() {
            return LocalProperty;
        }
        [Command(false)]
        public int GetProperty(int val) {
            return val;
        }
    }
    class MBBTraceListener : TraceListener {
        StringBuilder output;
        public string Output { get { return output.ToString(); } }
        public MBBTraceListener() {
            Name = "Trace";
            output = new StringBuilder();
        }
        public override void Write(string message) {
            output.Append(message);
        }
        public override void WriteLine(string message) {
            output.AppendLine(message);
        }
    }
}