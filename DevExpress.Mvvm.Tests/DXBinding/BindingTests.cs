using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace DevExpress.Xpf.DXBinding.Tests {
    static class BindingTestHelper {
        public static TextBlock BindAssert(string property, string binding, object expected = null, object dataContext = null) {
            return BindAssert<TextBlock>("TextBlock", property, binding, expected, dataContext);
        }
        public static void DoEvents(FrameworkElement obj) {
            if(obj == null) return;
            obj.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
        public static void DoEvents(FrameworkContentElement obj) {
            if(obj == null) return;
            obj.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
        public static T BindAssert<T>(string objToBind, string property, string binding, object expected = null, object dataContext = null, bool assert = true) {
            string xaml = string.Format("<{0} {1}=\"{2}\" />", objToBind, property, binding);
            T res = LoadXaml<T>(xaml);
            if(dataContext != null && res is FrameworkElement)
                (res as FrameworkElement).DataContext = dataContext;
            if(dataContext != null && res is FrameworkContentElement)
                (res as FrameworkContentElement).DataContext = dataContext;
            if(res is FrameworkElement)
                DoEvents(res as FrameworkElement);
            if(res is FrameworkContentElement)
                DoEvents(res as FrameworkContentElement);
            if(assert) {
                expected.Do(x => PropertyAssert(res, property, x));
            }
            return res;
        }

        public static T LoadXaml<T>(string xaml) {
            ParserContext xamlContext = new ParserContext();
            xamlContext.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            xamlContext.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            xamlContext.XmlnsDictionary.Add("sys", "clr-namespace:System;assembly=mscorlib");
            xamlContext.XmlnsDictionary.Add("b", "clr-namespace:DevExpress.Xpf.DXBinding;assembly=" + "DevExpress.Mvvm.UI");
            xamlContext.XmlnsDictionary.Add("test", "clr-namespace:DevExpress.Xpf.DXBinding.Tests;assembly=" + "DevExpress.Mvvm.Tests.Free");
            return (T)XamlReader.Parse(xaml, xamlContext);
        }

        public static void SetDataContextAssert(FrameworkElement tb, string property, object dataContext, object expected) {
            tb.DataContext = dataContext;
            DoEvents(tb);
            PropertyAssert(tb, property, expected);
        }
        public static void PropertyAssert(object obj, string property, object expected) {
            DoEvents(obj as FrameworkElement);
            object value = null;
            var fieldInfo = obj.GetType().GetField(property + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if(fieldInfo != null) {
                DependencyProperty dp = (DependencyProperty)fieldInfo.GetValue(obj);
                value = ((DependencyObject)obj).GetValue(dp);
            } else {
                var propInfo = obj.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                value = propInfo.GetValue(obj, null);
            }
            Assert.AreEqual(expected, value);
            if(expected != null) Assert.AreEqual(expected.GetType(), value.GetType());
        }

        public static void AssertException(Exception e, string message) {
            DXBindingException ee = (e as DXBindingException) ?? (e.InnerException as DXBindingException);
            Assert.AreEqual(message, ee.Message);
        }
        public static void AssertException(Exception e, string expr, string backExpr, string message) {
            DXBindingException ee = (e as DXBindingException) ?? (e.InnerException as DXBindingException);
            Assert.AreEqual(expr, ee.Expr);
            Assert.AreEqual(backExpr, ee.BackExpr);
            Assert.AreEqual(message, ee.Message);
        }

        public static void VisualTest(object content, Action test = null) {
            Window w = new Window() {
                WindowStyle = WindowStyle.None,
                ShowActivated = true,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Topmost = true,
            };
            try {
                w.Content = content;
                w.Show();
                DoEvents(w);
                test.Do(x => x());
            } finally {
                w.Close();
                w.Content = null;
            }
        }

        public static void TestsSetUp() {
            DXBindingExtension.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        }
        public static void TestsTearDown() {
            DXBindingExtension.DefaultUpdateSourceTrigger = null;
            DXBindingExtension.IsInDesingModeCore = null;
        }
        public static void SetResolvingMode(DXBindingResolvingMode mode) {
            DXBindingBase.DefaultResolvingMode = mode;
        }
        public static void ClearResolvingMode() {
            DXBindingBase.DefaultResolvingMode = DXBindingResolvingMode.DynamicTyping;
        }

        public static void DoCommand(Button bt) {
            bt.Command.Execute(bt.CommandParameter);
        }
        public static bool CanDoCommand(Button bt) {
            return bt.Command.CanExecute(bt.CommandParameter);
        }
    }
    class BindingListener : TraceListener {
        StringBuilder errors = new StringBuilder();
        public string Error { get { return errors.ToString(); } }
        public override void Write(string message) {
            errors.AppendLine(message);
        }
        public override void WriteLine(string message) {
            Write(message);
        }
        public void ResetError() {
            errors.Clear();
        }
        BindingListener() { }

        readonly static BindingListener listener = new BindingListener();
        static SourceLevels oldLevel;
        public static void Enable() {
            oldLevel = PresentationTraceSources.DataBindingSource.Switch.Level;
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(listener);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
        }
        public static void Disable() {
            Reset();
            PresentationTraceSources.DataBindingSource.Listeners.Remove(listener);
            PresentationTraceSources.DataBindingSource.Switch.Level = oldLevel;
        }
        public static string GetError() {
            listener.Flush();
            return listener.Error;
        }
        public static void Reset() {
            listener.ResetError();
        }
    }

    [Platform("NET")]
    [TestFixture]
    public class BindingDefaultMode {
         [Test]
         public void BindingDefaultModeTest() {
            Assert.That(new DXBindingExtension().ResolvingMode == null);
        }
    }

    [Platform("NET")]
    [TestFixture]
    public class BindingTests {
        [SetUp]
        public virtual void Init() {
            BindingListener.Enable();
            BindingTestHelper.TestsSetUp();
            BindingTestHelper.SetResolvingMode(DXBindingResolvingMode.LegacyStaticTyping);
        }
        [TearDown]
        public virtual void TearDown() {
            BindingTestHelper.TestsTearDown();
            BindingListener.Disable();
            BindingTestHelper.ClearResolvingMode();
        }
        [Test]
        public virtual void ErrorBasic() {
            AssertHelper.AssertThrows<XamlParseException>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "SelectedText", "{b:DXBinding}");
            }, x => BindingTestHelper.AssertException(x, "", null,
            "The DXBinding can only be set on a DependencyProperty of a DependencyObject."));

            AssertHelper.AssertThrows<XamlParseException>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Loaded", "{b:DXBinding}");
            }, x => BindingTestHelper.AssertException(x, "", null,
            "The DXBinding can only be set on a DependencyProperty of a DependencyObject."));

            AssertHelper.AssertThrows<XamlParseException>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Width", "{b:DXBinding x::InvalidType.Prop}");
            });
        }
        [Test]
        public virtual void NoOperandsSimple() {
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1}");
            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1, Mode=TwoWay}");
            }, x => BindingTestHelper.AssertException(x, "1", null,
            "The TwoWay or OneWayToSource binding mode requires the DXBinding.BackExpr property to be set in complex DXBindings."));
            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1, Mode=OneWayToSource}");
            }, x => BindingTestHelper.AssertException(x, "1", null,
            "The TwoWay or OneWayToSource binding mode requires the DXBinding.BackExpr property to be set in complex DXBindings."));

            AssertHelper.AssertThrows<Exception>(() => {
                var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntProp+1}", "2", BindingTests_a.Create(intV: 1));
                tb.Text = "1"; BindingTestHelper.DoEvents(tb);
            }, x => BindingTestHelper.AssertException(x, "IntProp+1", null,
            "The TwoWay or OneWayToSource binding mode requires the DXBinding.BackExpr property to be set in complex DXBindings."));
            AssertHelper.AssertThrows<Exception>(() => {
                var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntProp+1, Mode=TwoWay}", "2", BindingTests_a.Create(intV: 1));
                tb.Text = "1"; BindingTestHelper.DoEvents(tb);
            }, x => BindingTestHelper.AssertException(x, "IntProp+1", null,
            "The TwoWay or OneWayToSource binding mode requires the DXBinding.BackExpr property to be set in complex DXBindings."));
            AssertHelper.AssertThrows<Exception>(() => {
                var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntProp+1, Mode=OneWayToSource}");
                tb.Text = "1"; BindingTestHelper.DoEvents(tb);
            }, x => BindingTestHelper.AssertException(x, "IntProp+1", null,
            "The TwoWay or OneWayToSource binding mode requires the DXBinding.BackExpr property to be set in complex DXBindings."));

            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1, BackExpr=1}");
            }, x => BindingTestHelper.AssertException(x, "1", "1",
            "The DXBinding.BackExpr property is specified in the short form, but the DXBinding.Expr expression contains no binding operands."));
            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1, BackExpr=@v, Mode=TwoWay}");
            }, x => BindingTestHelper.AssertException(x, "1", "@v",
            "The DXBinding.BackExpr property is specified in the short form, but the DXBinding.Expr expression contains no binding operands."));
            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1, BackExpr=@value, Mode=OneWayToSource}");
            }, x => BindingTestHelper.AssertException(x, "1", "@value",
            "The DXBinding.BackExpr property is specified in the short form, but the DXBinding.Expr expression contains no binding operands."));
        }
        [Test]
        public virtual void NoOperandsComplex() {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1+2}");
            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1+2, Mode=TwoWay}");
            }, x => BindingTestHelper.AssertException(x, "1+2", null,
            "The TwoWay or OneWayToSource binding mode requires the DXBinding.BackExpr property to be set in complex DXBindings."));
            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1+2, Mode=OneWayToSource}");
            }, x => BindingTestHelper.AssertException(x, "1+2", null,
            "The TwoWay or OneWayToSource binding mode requires the DXBinding.BackExpr property to be set in complex DXBindings."));
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1+2, Mode=OneWay}", "3");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1+2, Mode=OneTime}", "3");

            BindingTestHelper.BindAssert<TextBox>("TextBox", "Width", "{b:DXBinding 1+2}", 3d);
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Width", "{b:DXBinding 1+2, Mode=OneWay}", 3d);
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Width", "{b:DXBinding 1+2, Mode=OneTime}", 3d);

            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Width", "{b:DXBinding 1+2, Mode=TwoWay}");
            }, x => BindingTestHelper.AssertException(x, "1+2", null,
            "The TwoWay or OneWayToSource binding mode requires the DXBinding.BackExpr property to be set in complex DXBindings."));
            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Width", "{b:DXBinding 1+2, Mode=OneWayToSource}");
            }, x => BindingTestHelper.AssertException(x, "1+2", null,
            "The TwoWay or OneWayToSource binding mode requires the DXBinding.BackExpr property to be set in complex DXBindings."));
        }
        [Test]
        public virtual void NoOperandWithBackConversion() {
            var vm = BindingTests_a.Create(stringV: "1");
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 2, BackExpr='StringProp=@value'}", "2", vm);
            Assert.AreEqual("1", vm.StringProp);
            tb.Text = "3"; BindingTestHelper.DoEvents(tb);
            Assert.AreEqual("3", vm.StringProp);
            Assert.AreEqual("2", tb.Text);
            vm.StringProp = "1"; BindingTestHelper.DoEvents(tb);
            Assert.AreEqual("2", tb.Text);
            Assert.AreEqual("1", vm.StringProp);
        }
        [Test]
        public virtual void NoOperandIncorrectExpr() {
            AssertHelper.AssertThrows<XamlParseException>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding Expr='1+'}");
            }, x => BindingTestHelper.AssertException(x,
            "DXBinding error (position 3): invalid identifier expression."));

            AssertHelper.AssertThrows<XamlParseException>(() => {
                BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding Expr='(1+2'}");
            }, x => BindingTestHelper.AssertException(x,
            "DXBinding error (position 5): \")\" expected."));
        }
        [Test]
        public virtual void Operators() {
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding true &amp;&amp; true}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding true || true}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding true and true}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding true or true}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 > 2}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 &lt; 2}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding '1 >= 2'}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding '1 &lt;= 2'}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 gt 2}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 lt 2}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 ge 2}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 le 2}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 shl 2}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 shr 2}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 eq 2}");
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding 1 ne 2}");
        }

        [Test]
        public virtual void OneOperand() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding}", "1", 1);
            tb.DataContext = 2;
            Assert.AreEqual("2", tb.Text);
            tb.Text = "1";
            Assert.AreEqual(2, tb.DataContext);

            var vm = BindingTests_a.Create(intV: 1);
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Visibility",
                "{b:DXBinding Expr='IntProp==1 ? $Visibility.Visible : $Visibility.Collapsed'}",
                Visibility.Visible, vm);
            vm.IntProp = 2;
            Assert.AreEqual(Visibility.Collapsed, tb.Visibility);
        }
        [Test]
        public virtual void OneOperandSimple() {
            var tb = BindingTestHelper.BindAssert<TextBlock>("TextBlock", "Width", "{b:DXBinding 1+IntProp}", double.NaN);
            var vm = BindingTests_a.Create(intV: 2);

            BindingTestHelper.SetDataContextAssert(tb, "Width", vm, 3d);
            vm.IntProp = 3;
            BindingTestHelper.PropertyAssert(tb, "Width", 4d);
            BindingTestHelper.BindAssert<TextBlock>("TextBlock", "Width", "{b:DXBinding 2+IntProp}", 5d, vm);

            tb = BindingTestHelper.BindAssert<TextBlock>("TextBlock", "Text", "{b:DXBinding 1+IntProp}", string.Empty);
            vm = BindingTests_a.Create(intV: 2);
            BindingTestHelper.SetDataContextAssert(tb, "Text", vm, "3");
            vm.IntProp = 3;
            BindingTestHelper.PropertyAssert(tb, "Text", "4");
            BindingTestHelper.BindAssert<TextBlock>("TextBlock", "Text", "{b:DXBinding 2+IntProp}", "5", vm);

            BindingTestHelper.BindAssert<TextBlock>("TextBlock", "Text", "{b:DXBinding IntProp+IntProp}", "6", vm);
        }
        [Test]
        public virtual void OneOperandTwoWay() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntProp}", "2", vm);
            tb.Text = "3";
            Assert.AreEqual(3, vm.IntProp);
            vm.IntProp = 4;
            Assert.AreEqual("4", tb.Text);

            vm = BindingTests_a.Create(intV: 2);
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntProp-2, BackExpr='IntProp=int.Parse(@v)+2'}", "0", vm);
            tb.Text = "1";
            Assert.AreEqual(3, vm.IntProp);
            vm.IntProp = 4;
            Assert.AreEqual("2", tb.Text);
        }
        [Test]
        public virtual void OneOperandTwoWay2() {
            var vm = BindingTests_a.Create();
            vm.IntProp = 1;
            var tb = BindingTestHelper.BindAssert("Width", "{b:DXBinding 1+IntProp, BackExpr=@v-1, Mode=TwoWay}", 2d, vm);
            vm.IntProp = 2;
            BindingTestHelper.PropertyAssert(tb, "Width", 3d);
            tb.Width = 2; BindingTestHelper.DoEvents(tb);
            Assert.AreEqual(1, vm.IntProp);

            vm = BindingTests_a.Create();
            vm.IntProp = 1;
            tb = BindingTestHelper.BindAssert("Width", "{b:DXBinding 1+IntProp, BackExpr='IntProp=@v-1', Mode=TwoWay}", 2d, vm);
            vm.IntProp = 2;
            BindingTestHelper.PropertyAssert(tb, "Width", 3d);
            tb.Width = 2; BindingTestHelper.DoEvents(tb);
            Assert.AreEqual(1, vm.IntProp);
        }
        [Test]
        public virtual void OneOperandOneWayToSource() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntProp, Mode=OneWayToSource}", string.Empty, vm);
            vm.IntProp = 3;
            Assert.AreEqual(string.Empty, tb.Text);
            tb.Text = "4";
            Assert.AreEqual(4, vm.IntProp);
        }

        [Test]
        public virtual void TwoOperands() {
            var tb = BindingTestHelper.BindAssert<TextBlock>("TextBlock", "Width", "{b:DXBinding DoubleProp+IntProp}", double.NaN);
            var vm = BindingTests_a.Create(intV: 2, doubleV: 2.1);
            BindingTestHelper.SetDataContextAssert(tb, "Width", vm, 4.1d);
            vm.IntProp = 3;
            BindingTestHelper.PropertyAssert(tb, "Width", 5.1d);
            vm.DoubleProp = 3.1;
            BindingTestHelper.PropertyAssert(tb, "Width", 6.1d);
            BindingTestHelper.BindAssert("Width", "{b:DXBinding DoubleProp+IntProp+1}", 7.1d, vm);

            tb = BindingTestHelper.BindAssert<TextBlock>("TextBlock", "Text", "{b:DXBinding DoubleProp+IntProp}", string.Empty);
            vm = BindingTests_a.Create(intV: 2, doubleV: 2.1);
            BindingTestHelper.SetDataContextAssert(tb, "Text", vm, "4.1");
            vm.IntProp = 3;
            BindingTestHelper.PropertyAssert(tb, "Text", "5.1");
            vm.DoubleProp = 3.1;
            BindingTestHelper.PropertyAssert(tb, "Text", "6.1");
            BindingTestHelper.BindAssert<TextBlock>("TextBlock", "Text", "{b:DXBinding DoubleProp+IntProp+1}", "7.1", vm);
        }
        [Test]
        public virtual void TwoOperandsTwoWay() {
            var vm = BindingTests_a.Create(intV: 1, doubleV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding DoubleProp+IntProp, BackExpr='DoubleProp=double.Parse(@value);'}", "3", vm);
            vm.DoubleProp = 3;
            Assert.AreEqual(1, vm.IntProp);
            Assert.AreEqual(3, vm.DoubleProp);
            Assert.AreEqual("4", tb.Text);
            tb.Text = "5";
            Assert.AreEqual(1, vm.IntProp);
            Assert.AreEqual(5, vm.DoubleProp);
            Assert.AreEqual("6", tb.Text);

            vm = BindingTests_a.Create(intV: 1, doubleV: 2);
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding DoubleProp+IntProp, BackExpr='DoubleProp=double.Parse(@value);IntProp=int.Parse(@v);'}", "3", vm);
            vm.DoubleProp = 3;
            Assert.AreEqual(1, vm.IntProp);
            Assert.AreEqual(3, vm.DoubleProp);
            Assert.AreEqual("4", tb.Text);
            vm.IntProp = 4;
            Assert.AreEqual(4, vm.IntProp);
            Assert.AreEqual(3, vm.DoubleProp);
            Assert.AreEqual("7", tb.Text);

            tb.Text = "8";
            Assert.AreEqual(8, vm.IntProp);
            Assert.AreEqual(8, vm.DoubleProp);
            Assert.AreEqual("16", tb.Text);
        }

        [Test]
        public virtual void PropertyAndMethod() {
            BindingTestHelper.BindAssert("Text", "{b:DXBinding GetSelf(GetSelf()).IntProp}", "1", new BindingTests_a(intV: 1));
            BindingTestHelper.BindAssert("Text", "{b:DXBinding GetSelf().IntProp}", "1", new BindingTests_a(intV: 1));
            BindingTestHelper.BindAssert("Text", "{b:DXBinding GetSelf().IntField}", "1", new BindingTests_a(intV: 1));
            BindingTestHelper.BindAssert("Text", "{b:DXBinding GetSelf().IntProp + GetSelf().IntField}", "2", new BindingTests_a(intV: 1));
            BindingTestHelper.BindAssert("Text", "{b:DXBinding GetSelf().IntProp + GetSelf().DoubleField}", "2.1", new BindingTests_a(intV: 1, doubleV: 1.1));
            BindingTestHelper.BindAssert("Text", "{b:DXBinding GetSelf().StringProp + GetSelf().StringField}", "aa", new BindingTests_a(stringV: "a"));
            BindingTestHelper.BindAssert("Text", "{b:DXBinding GetSelf().Self.GetSelf().IntProp}", "1", new BindingTests_a(intV: 1));

            BindingTestHelper.BindAssert("Text", "{b:DXBinding GetInt(1)}", "1", new BindingTests_a());
            BindingTestHelper.BindAssert("Text", "{b:DXBinding GetInt(1.1)}", "1", new BindingTests_a());
            BindingTestHelper.BindAssert("Text", "{b:DXBinding Expr='GetObject(1)'}", "1", new BindingTests_a());
            BindingTestHelper.BindAssert("Text", "{b:DXBinding Expr=GetObject(1d)}", "1", new BindingTests_a());
            BindingTestHelper.BindAssert("Text", @"{b:DXBinding Expr='GetObject(`a`)'}", "a", new BindingTests_a());
        }
        [Test]
        public virtual void Type() {
            BindingTests_a.Static(1);
            BindingTestHelper.BindAssert("Text", "{b:DXBinding $test:BindingTests_a.StaticIntProp}", "1");
            BindingTestHelper.BindAssert("Text", "{b:DXBinding $test:BindingTests_a.StaticIntProp + $test:BindingTests_a.StaticIntField}", "2");
            BindingTestHelper.BindAssert("Text", "{b:DXBinding $test:BindingTests_a.StaticSelf.IntProp}", "1");
            BindingTestHelper.BindAssert("Text", "{b:DXBinding $test:BindingTests_a.StaticSelf.IntProp + $test:BindingTests_a.StaticGetInt()}", "2");
        }
        [Test]
        public virtual void Attached() {
            BindingTestHelper.BindAssert("Text", "{b:DXBinding ($test:BindingTests_a.AttachedProperty)}", "1", new BindingTests_visual(1));
        }

        [Test]
        public virtual void RelativeSourceSelf() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding @s.Tag}", null, null);
            tb.Tag = 1;
            Assert.AreEqual("1", tb.Text);
            tb.Text = "2";
            Assert.AreEqual("2", tb.Tag);
        }
        [Test]
        public virtual void RelativeSourceElementName() {
            string xaml = @"
<Grid x:Name=""panel"" Tag=""1"">
    <TextBox Text=""{b:DXBinding @e(panel).Tag}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            var tb = (TextBox)panel.Children[0];
            Assert.AreEqual("1", tb.Text);
            tb.Text = "2";
            Assert.AreEqual("2", panel.Tag);
        }
        [Test]
        public virtual void RelativeSourceElementName2_T491236() {
            string xaml = @"
<Grid x:Name=""panel"" Tag=""1"">
    <CheckBox x:Name=""cb"" IsChecked=""True""/>
    <TextBox x:Name=""tb1"" Text=""text1""/>
    <TextBox x:Name=""tb2"" Text=""text2""/>
    <TextBox Text=""{b:DXBinding '@e(cb).IsChecked ? @e(tb1).Text : @e(tb2).Text', Mode=OneWay}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            var cb = (CheckBox)panel.Children[0];
            var tb1 = (TextBox)panel.Children[1];
            var tb2 = (TextBox)panel.Children[2];
            var tb = (TextBox)panel.Children[3];
            Assert.AreEqual("text1", tb.Text);
            cb.IsChecked = false;
            Assert.AreEqual("text2", tb.Text);
        }
        [Test]
        public virtual void RelativeSourceStaticResource() {
            string xaml = @"
<Grid x:Name=""panel"" Tag=""1"">
    <Grid.Resources>
        <sys:Int32 x:Key=""resource"">5</sys:Int32>
    </Grid.Resources>
    <TextBox Text=""{b:DXBinding @r(resource)}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            var tb = (TextBox)panel.Children[0];
            Assert.AreEqual("5", tb.Text);
            tb.Text = "2";
        }

        [Test]
        public virtual void NullOperand() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntPropB}",
                null, null);
            Assert.IsEmpty(BindingListener.GetError());
            Assert.AreEqual(string.Empty, tb.Text);
        }
        [Test]
        public virtual void FallbackValue() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntPropB, FallbackValue=fail}",
                null, null);
            Assert.IsEmpty(BindingListener.GetError());
            Assert.AreEqual("fail", tb.Text);

            BindingListener.Reset();
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntPropB, FallbackValue=fail}",
                null, FallbackValueTests_a.Create());
            Assert.IsNotEmpty(BindingListener.GetError());
            Assert.AreEqual("fail", tb.Text);

            BindingListener.Reset();
            tb.DataContext = FallbackValueTests_b.Create(); BindingTestHelper.DoEvents(tb);
            Assert.IsEmpty(BindingListener.GetError());
            Assert.AreEqual("0", tb.Text);
        }
        [Test]
        public virtual void FallbackValue2() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntPropB}", null, null);
            Assert.IsEmpty(BindingListener.GetError());
            Assert.AreEqual(string.Empty, tb.Text);
            tb.DataContext = 1;
            BindingTestHelper.DoEvents(tb);
            Assert.IsNotEmpty(BindingListener.GetError());
            BindingListener.Reset();
            Assert.AreEqual(string.Empty, tb.Text);


            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{Binding IntPropB}", null, null);
            Assert.IsEmpty(BindingListener.GetError());
            Assert.AreEqual(string.Empty, tb.Text);
            tb.DataContext = 1;
            BindingTestHelper.DoEvents(tb);
            Assert.IsNotEmpty(BindingListener.GetError());
            BindingListener.Reset();
            Assert.AreEqual(string.Empty, tb.Text);
        }
        [Test]
        public virtual void FallbackValue3() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Width", "{b:DXBinding IntPropB}", null, null);
            Assert.IsEmpty(BindingListener.GetError());
            Assert.AreEqual(double.NaN, tb.Width);
            tb.DataContext = 1;
            BindingTestHelper.DoEvents(tb);
            Assert.IsNotEmpty(BindingListener.GetError());
            BindingListener.Reset();
            Assert.AreEqual(double.NaN, tb.Width);

            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Width", "{Binding IntPropB}", null, null);
            Assert.IsEmpty(BindingListener.GetError());
            Assert.AreEqual(double.NaN, tb.Width);
            tb.DataContext = 1;
            BindingTestHelper.DoEvents(tb);
            Assert.IsNotEmpty(BindingListener.GetError());
            BindingListener.Reset();
            Assert.AreEqual(double.NaN, tb.Width);
        }
        [Test]
        public virtual void TargetNullValue() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding StringProp, TargetNullValue='test'}",
                null, BindingTests_a.Create(stringV: null));
            Assert.AreEqual("test", tb.Text);
        }

        [Test]
        public virtual void BindingInDataTemplate() {
            string xaml = @"
<Grid>
    <Grid.Resources>
        <DataTemplate x:Key=""temp"">
            <TextBox Text=""{b:DXBinding 'IntProp+$test:BindingTests_a.StaticIntProp',
                                        BackExpr='IntProp=int.Parse(@v)-$test:BindingTests_a.StaticIntProp'}""/>
        </DataTemplate>
    </Grid.Resources>
    <ContentControl Content=""{b:DXBinding}"" ContentTemplate=""{StaticResource temp}""/>
    <ContentControl Content=""{b:DXBinding}"" ContentTemplate=""{StaticResource temp}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            BindingTestHelper.VisualTest(panel, () => {
                BindingTests_a.Static(2);
                var vm = BindingTests_a.Create(intV: 1);
                var tb1 = LayoutTreeHelper.GetVisualChildren(panel.Children[0]).OfType<TextBox>().First();
                var tb2 = LayoutTreeHelper.GetVisualChildren(panel.Children[1]).OfType<TextBox>().First();
                panel.DataContext = vm;
                BindingTestHelper.DoEvents(panel);
                Assert.AreEqual("3", tb1.Text);
                Assert.AreEqual("3", tb2.Text);
                tb1.Text = "4";
                BindingTestHelper.DoEvents(panel);
                Assert.AreEqual(2, vm.IntProp);
                Assert.AreEqual("4", tb2.Text);
            });
        }
        [Test]
        public virtual void BindingInStyleSetter() {
            string xaml1 = @"
<Grid>
    <Grid.Resources>
        <Style x:Key=""st"" TargetType=""TextBox"">
            <Setter Property=""Text"" Value=""{b:DXBinding IntProp+$test:BindingTests_a.StaticIntProp,
                                        BackExpr='IntProp=int.Parse(@v)-$test:BindingTests_a.StaticIntProp'}""/>
        </Style>
    </Grid.Resources>
    <TextBox Style=""{StaticResource st}""/>
    <TextBox Style=""{StaticResource st}""/>
</Grid>
";
            string xaml2 = @"
<Grid>
    <Grid.Resources>
        <Style TargetType=""TextBox"">
            <Setter Property=""Text"" Value=""{b:DXBinding IntProp+$test:BindingTests_a.StaticIntProp,
                                        BackExpr='IntProp=int.Parse(@v)-$test:BindingTests_a.StaticIntProp'}""/>
        </Style>
    </Grid.Resources>
    <TextBox/>
    <TextBox/>
</Grid>
";

            Action<string> test = xamlStr => {
                BindingTests_a.Static(2);
                var panel = BindingTestHelper.LoadXaml<Grid>(xamlStr);
                var tb1 = (TextBox)panel.Children[0];
                var tb2 = (TextBox)panel.Children[1];
                var vm1 = BindingTests_a.Create(intV: 1);
                var vm2 = BindingTests_a.Create(intV: 1);
                tb1.DataContext = vm1;
                tb2.DataContext = vm2;
                BindingTestHelper.DoEvents(tb1);
                BindingTestHelper.DoEvents(tb2);
                Assert.AreEqual("3", tb1.Text);
                Assert.AreEqual("3", tb2.Text);
                tb1.Text = "4"; BindingTestHelper.DoEvents(tb1);
                tb2.Text = "4"; BindingTestHelper.DoEvents(tb2);
                Assert.AreEqual(2, vm1.IntProp);
                Assert.AreEqual(2, vm2.IntProp);
            };
            test(xaml1);
            test(xaml2);
        }
        [Test]
        public virtual void BindingInItemContainerStyle() {
            string xaml = @"
<TabControl ItemsSource=""{b:DXBinding}"">
    <TabControl.ItemContainerStyle>
        <Style TargetType=""TabItem"">
            <Setter Property=""Content"" Value=""{b:DXBinding}""/>
        </Style>
    </TabControl.ItemContainerStyle>
</TabControl>
";
            var itemsControl = BindingTestHelper.LoadXaml<ItemsControl>(xaml);
            BindingTestHelper.VisualTest(itemsControl, () => {
                itemsControl.DataContext = new[] { "A", "B", "C" };
                BindingTestHelper.DoEvents(itemsControl);
                Assert.AreEqual(3, itemsControl.Items.Count);
                var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(0) as TabItem;
                Assert.AreEqual("A", container.Content);
            });
        }
        [Test]
        public virtual void BindingInDataTrigger() {
            string xaml = @"
    <Grid>
        <Grid.Resources>
            <Style TargetType=""TextBox"">
                <Style.Triggers>
                    <DataTrigger Binding=""{b:DXBinding Expr=@s.Text}"" Value=""5"">
                        <Setter Property=""Background"" Value=""{b:DXBinding '`#FFFF0000`'}""/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Grid.Resources>
        <TextBox Text=""{b:DXBinding Mode=OneWay}""/>
    </Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            var tb1 = (TextBox)panel.Children[0];
            Assert.AreEqual(Brushes.White.Color, ((SolidColorBrush)tb1.Background).Color);
            tb1.DataContext = 5;
            BindingTestHelper.DoEvents(tb1);
            Assert.AreEqual(Brushes.Red.Color, ((SolidColorBrush)tb1.Background).Color);
        }
        [Test]
        public virtual void BindingInDataTrigger2() {
            string xaml = @"
    <Grid>
        <Grid.Resources>
            <Style TargetType=""TextBox"">
                <Style.Triggers>
                    <DataTrigger Binding=""{b:DXBinding Expr=@s.Text}"" Value=""{b:DXBinding}"">
                        <Setter Property=""Background"" Value=""Red""/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Grid.Resources>
        <TextBox Text=""{b:DXBinding Mode=OneWay}""/>
    </Grid>
";
            AssertHelper.AssertThrows<Exception>(() => {
                BindingTestHelper.LoadXaml<Grid>(xaml);
            }, x => BindingTestHelper.AssertException(x, "The DXBinding can only be set on a DependencyProperty of a DependencyObject."));
        }
        [Test]
        public virtual void BindingInDataTrigger3() {
            string xaml = @"
    <Grid>
        <Grid.Resources>
            <DataTemplate x:Key=""temp"">
                <TextBlock x:Name=""tb"" Text=""{b:DXBinding IntProp, Mode=OneWay}""/>
                <DataTemplate.Triggers>
                    <DataTrigger Binding=""{b:DXBinding @e(tb).Text}"" Value=""6"">
                        <Setter TargetName=""tb"" Property=""Background"" Value=""Blue""/>
                    </DataTrigger>
                </DataTemplate.Triggers>
            </DataTemplate>
        </Grid.Resources>
        <ContentControl Content=""{b:DXBinding}"" ContentTemplate=""{StaticResource temp}"" />
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            var cc = panel.Children[0];
            var vm = BindingTests_a.Create(intV: 1);
            BindingTestHelper.VisualTest(panel, () => {
                var cp = (ContentPresenter)VisualTreeHelper.GetChild(cc, 0);
                var tb = (TextBlock)VisualTreeHelper.GetChild(cp, 0);
                panel.DataContext = vm;
                BindingTestHelper.DoEvents(panel);
                Assert.AreEqual(null, tb.Background);
                vm.IntProp = 6;
                BindingTestHelper.DoEvents(panel);
                Assert.AreEqual(Brushes.Blue.Color, ((SolidColorBrush)tb.Background).Color);
            });
        }

        [Test]
        public virtual void CoerceEnums() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Visibility",
                "{b:DXBinding Expr='IntProp == 1 ? `Visible` : `Collapsed`', BackExpr='IntProp = @v == $Visibility.Collapsed ? 10 : 20', Mode=TwoWay}",
                Visibility.Collapsed, vm);
            tb.Visibility = Visibility.Hidden;
            Assert.AreEqual(20, vm.IntProp);
        }
        [Test]
        public virtual void CoerceBrush() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding Expr='IntProp == 1 ? `#ff00ff` : `#ff0000`'}",
                null, vm, false);
            var brush = (SolidColorBrush)tb.Foreground;
            Assert.AreEqual(Colors.Red, brush.Color);
        }
        [Test]
        public virtual void CoerceBrushWithoutProperty() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding '`#ff0000`'}",
                null, vm, false);
            var brush = (SolidColorBrush)tb.Foreground;
            Assert.AreEqual(Colors.Red, brush.Color);
        }
        [Test]
        public virtual void CoerceBrushInMultiBinding() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding Expr='IntProp == 1 or StringProp == `abc` ? `#ff00ff` : `#ff0000`'}",
                null, vm, false);
            var brush = (SolidColorBrush)tb.Foreground;
            Assert.AreEqual(Colors.Red, brush.Color);
        }
        [Test]
        public virtual void CoerceBrushInMultiBinding2() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding Expr='IntProp == 1 or StringProp == `abc` ? `Yellow` : `Red`'}",
                null, vm, false);
            var brush = (SolidColorBrush)tb.Foreground;
            Assert.AreEqual(Colors.Red, brush.Color);
        }
        [Test]
        public virtual void GetPropertyValueOfStringLiteral() {
            var vm = BindingTests_a.Create();
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding '`abc`.Length', Mode=OneWay}",
                "3", vm);
        }
        [Test]
        public virtual void BinaryStringConcat() {
            var vm = BindingTests_a.Create();
            vm.IntProp = 5;
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding `abc` + 5, Mode=OneWay}",
                "abc5", vm);
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding 5 + `abc`, Mode=OneWay}",
                "5abc", vm);
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding IntProp + `abc`, Mode=OneWay}",
                "5abc", vm);
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding `abc` + IntProp, Mode=OneWay}",
                "abc5", vm);
            vm.StringProp = "abc";
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding StringProp + IntProp, Mode=OneWay}",
                "abc5", vm);
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding IntProp + StringProp, Mode=OneWay}",
                "5abc", vm);
        }
        [Test]
        public virtual void TernaryOperator() {
            var vm = BindingTests_a.Create();
            vm.IntProp = 2;
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding Expr='false ? IntProp : 2 * IntProp', Mode=OneWay}",
                "4", vm);
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding Expr='false ? StringProp : ($object)IntProp', Mode=OneWay}",
                "2", vm);
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding Expr='false ? ($object)StringProp : IntProp', Mode=OneWay}",
                "2", vm);
        }
        [Test]
        public virtual void MethodOverloadingResolutionTests() {
            Action<string, string> assert = MethodOverloadingResolutionTests_Assert;

            assert("@c[``]", "this[string]");
            assert("@c[1]", "this[int:1]");
            assert("@c[``, 1]", "this[string, int]");
            assert("@c[0.1]", "this[double, double]");
            assert("@c[``, ``, ``, ``]", "this[string, params string[]]");

            assert("F()", "F()");
            assert("F(1)", "F(int)");
            assert("F(1.0)", "F(double)");
            assert("F(`abc`)", "F(object)");
            assert("F((double)1)", "F(double)");
            assert("F(1, 1)", "F(double, double)");
            assert("F(1, 1, 1)", "F(params double[] ds)");
            assert("F(`a`[0])", "F(int)");
            assert("F(true)", "F(bool, string)");
        }
        [Test]
        public virtual void MethodOverloadingResolutionTests2() {
            Action<string, string> assert = MethodOverloadingResolutionTests_Assert;
            assert("F((object)1)", "F(object)");
        }
        protected void MethodOverloadingResolutionTests_Assert(string expr, string expected) {
            var vm = new SharpSpecOverloadExamples();
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                string.Format("{{b:DXBinding '{0}', Mode=OneWay}}", expr),
                expected, vm);
        }
        protected class SharpSpecOverloadExamplesBase {
            public string F(char x) {
                return "F(char) base";
            }
            public string F(double x, double y, double z) {
                return "F(double, double, double) base";
            }
        }
        protected class SharpSpecOverloadExamples {
            public string this[int index] { get { return string.Format("this[int:{0}]", index); } }
            public string this[string index] { get { return "this[string]"; } }
            public string this[string index, int index2 = 0] { get { return "this[string, int]"; } }
            public string this[double index, double index2 = 0] { get { return "this[double, double]"; } }
            public string this[string a = "", params string[] strings] { get { return "this[string, params string[]]"; } }

            public string F() {
                return "F()";
            }
            public string F(object x) {
                return "F(object)";
            }
            public string F(int x) {
                return "F(int)";
            }
            public string F(double x) {
                return "F(double)";
            }
            public string F(double x, double y) {
                return "F(double, double)";
            }
            public string F(params double[] ds) {
                return "F(params double[] ds)";
            }
            public string F(bool x, string y = "") {
                return "F(bool, string)";
            }
        }
        [Test]
        public virtual void DontThrowWhenStaticPropertyNotFound_01_T360515() {
            try {
                var vm = BindingTests_a.Create();
                var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                    "{b:DXBinding $Brushes.Grey, Mode=OneWay}",
                    null, vm);
            } catch(XamlParseException xamlException) {
                var e = (DXBindingException)xamlException.InnerException;
                Assert.IsTrue(e.Message.Contains("Grey") && e.Message.Contains("Brushes"));
            }
        }
        [Test]
        public virtual void DontThrowWhenStaticPropertyNotFound_T360515() {
            try {
                var vm = BindingTests_a.Create();
                var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                    "{b:DXBinding $Brushes.NonExistingMethod(), Mode=OneWay}",
                    null, vm);
            } catch(XamlParseException xamlException) {
                var e = (DXBindingException)xamlException.InnerException;
                Assert.IsTrue(e.Message.Contains("NonExistingMethod") && e.Message.Contains("Brushes"));
            }
        }

        [Test]
        public virtual void NotRaiseExceptionInDesingMode1() {
            DXBindingBase.IsInDesingModeCore = true;
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding '@a($FrameworkElement).DataContext', BackExpr='@value'}");
        }

        [Test]
        public virtual void EqualityTest() {
            var vm = new BindingTests_a() { Visibility1 = Visibility.Visible };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                "{b:DXBinding 'Visibility1 == $Visibility.Visible'}", true, vm);
            vm = new BindingTests_a() { Visibility1 = Visibility.Collapsed };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                "{b:DXBinding 'Visibility1 == $Visibility.Visible'}", false, vm);
            vm = new BindingTests_a() { Visibility2 = Visibility.Visible };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                "{b:DXBinding 'Visibility2 == $Visibility.Visible'}", true, vm);
            vm = new BindingTests_a() { Visibility2 = Visibility.Collapsed };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                "{b:DXBinding 'Visibility2 == $Visibility.Visible'}", false, vm);

            vm = new BindingTests_a() { Obj = new BindingTests_a.SubObj() { Enum1 = BindingTests_a_Enum.Enum1 } };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                "{b:DXBinding 'Obj.Enum1 == $test:BindingTests_a_Enum.Enum1'}", true, vm);
            vm = new BindingTests_a() { Obj = new BindingTests_a.SubObj() { Enum1 = BindingTests_a_Enum.Enum2 } };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                "{b:DXBinding 'Obj.Enum1 == $test:BindingTests_a_Enum.Enum1'}", false, vm);
            vm = new BindingTests_a() { Obj = new BindingTests_a.SubObj() { Enum2 = BindingTests_a_Enum.Enum1 } };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                "{b:DXBinding 'Obj.Enum2 == $test:BindingTests_a_Enum.Enum1'}", true, vm);
            vm = new BindingTests_a() { Obj = new BindingTests_a.SubObj() { Enum2 = BindingTests_a_Enum.Enum2 } };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                "{b:DXBinding 'Obj.Enum2 == $test:BindingTests_a_Enum.Enum1'}", false, vm);

        }

        [Test]
        public virtual void NullableTest1() {
            string xaml = @"
<Grid>
    <CheckBox x:Name=""checkBox"" IsThreeState=""True"" IsChecked=""True""/>
    <TextBox Text=""{b:DXBinding '@e(checkBox).IsChecked.ToString()', Mode=OneWay}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            BindingTestHelper.DoEvents(panel);
            CheckBox cb = (CheckBox)panel.Children[0];
            TextBox tb = (TextBox)panel.Children[1];
            Assert.AreEqual("True", tb.Text);
            cb.IsChecked = false;
            Assert.AreEqual("False", tb.Text);
            cb.IsChecked = null;
            Assert.AreEqual("", tb.Text);
        }

        [Test]
        public virtual void NullableTest2() {
            var vm = new BindingTests_a() { NullableBoolean = true };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                    "{b:DXBinding 'GetSelf2().NullableBoolean.Value'}", true, vm);
            vm.NullableBoolean = false;
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                    "{b:DXBinding 'GetSelf2().NullableBoolean.Value'}", false, vm);
            vm.NullableBoolean = null;
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                    "{b:DXBinding 'GetSelf2().NullableBoolean.Value'}", true, vm);
        }

        [Test]
        public virtual void T713377() {
            var vm = new BindingTests_a() { DoubleProp = 0.1 };
            GridLengthConverter c = new GridLengthConverter();
            BindingTestHelper.BindAssert<RowDefinition>("RowDefinition", "Height",
                "{b:DXBinding 'DoubleProp'}", c.ConvertFrom("0.1"), vm);
            BindingTestHelper.BindAssert<RowDefinition>("RowDefinition", "Height",
                "{b:DXBinding 'DoubleProp'}", c.ConvertFrom(0.1), vm);
        }
    }

    [Platform("NET")]
    [TestFixture]
    public class BindingTests_Dynamics : BindingTests {
        [SetUp]
        public override void Init() {
            base.Init();
            BindingTestHelper.SetResolvingMode(DXBindingResolvingMode.DynamicTyping);
        }
        [TearDown]
        public override void TearDown() {
            base.TearDown();
            BindingTestHelper.ClearResolvingMode();
        }
        [Test]
        public override void MethodOverloadingResolutionTests2() {
            Action<string, string> assert = MethodOverloadingResolutionTests_Assert;
            assert("F((object)1)", "F(int)");
        }
        [Test]
        public void MethodNotFound() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding Get()}", null, 1);
            var res = BindingListener.GetError();
            Assert.That(res.Contains("The 'Get()' method is not found on object 'Int32'."));
        }
        [Test]
        public void MethodNotFound2() {
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding '$int.Get()'}", null, null);
            var res = BindingListener.GetError();
            Assert.That(res.Contains("The 'Get()' method is not found on object 'Int32'."));
        }
        [Test]
        public void PropertyNotFound() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding GetSelf().NotFoundProperty}", null, new ParserTests_a());
            var res = BindingListener.GetError();
            Assert.That(res.Contains("The 'NotFoundProperty' property is not found on object 'ParserTests_a'"));
        }
        [Test]
        public void PropertyNotFound2() {
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding '$int.Get'}", null, null);
            var res = BindingListener.GetError();
            Assert.That(res.Contains("The 'Get' property is not found on object 'Int32'."));
        }
        [Test]
        public void IndexerNotFound() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'GetSelf()[`test`]'}", null, new ParserTests_a());
            var res = BindingListener.GetError();
            Assert.That(res.Contains("The 'Indexer(String)' method is not found on object 'ParserTests_a'."));
        }
        [Test]
        public void DynamicTernary_T491236() {
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'Trigger ? Value1.Prop : Value2.Prop'}", "Prop1",
                new DynamicTernary_T491236_0() { Trigger = true });
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'Trigger ? Value1.Prop : Value2.Prop'}", "Prop2",
                new DynamicTernary_T491236_0() { Trigger = false });
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding '(Trigger ? Value1 : Value2).Prop'}", "Prop1",
               new DynamicTernary_T491236_0() { Trigger = true });
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding '(Trigger ? Value1 : Value2).Prop'}", "Prop2",
                new DynamicTernary_T491236_0() { Trigger = false });
        }
        class DynamicTernary_T491236_0 {
            public bool Trigger { get; set; }
            public DynamicTernary_T491236_1 Value1 { get; set; }
            public DynamicTernary_T491236_2 Value2 { get; set; }
            public DynamicTernary_T491236_0() {
                Value1 = new DynamicTernary_T491236_1();
                Value2 = new DynamicTernary_T491236_2();
            }
        }
        class DynamicTernary_T491236_1 {
            public string Prop { get { return "Prop1"; } }
        }
        class DynamicTernary_T491236_2 {
            public string Prop { get { return "Prop2"; } }
        }

        [Test]
        public void MemberSearcher_T495631() {
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding '$sys:String.IsNullOrEmpty(null)'}", true,
               new ParserTests_a());
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding '$sys:String.IsNullOrEmpty(GetObject(null))'}", true,
                new ParserTests_a());

            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding '$sys:String.IsNullOrEmpty(1)'}", null, null);
            Assert.That(BindingListener.GetError().Contains("The 'IsNullOrEmpty(Int32)' method is not found on object 'String'."));
            BindingListener.Reset();

            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'Method1(1)'}", "int", new MemberSearcher_T495631_a());
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'Method1($string.Empty)'}", "string", new MemberSearcher_T495631_a());
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'Method1(null)'}", "string", new MemberSearcher_T495631_a());

            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'Method2(1)'}", "int", new MemberSearcher_T495631_a());
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'Method2($string.Empty)'}", "string", new MemberSearcher_T495631_a());

            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'Method2(null)'}", null, new MemberSearcher_T495631_a());
            Assert.That(BindingListener.GetError().Contains("The 'Method2(null)' method is not found on object 'MemberSearcher_T495631_a'"));
        }
        class MemberSearcher_T495631_a {
            public string Method1(string @string) {
                return "string";
            }
            public string Method1(int @int) {
                return "int";
            }
            public string Method2(string @string) {
                return "string";
            }
            public string Method2(int @int) {
                return "int";
            }
            public string Method2(object @object) {
                return "@object";
            }
        }

        [Test]
        public void T497255() {
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'GetObject(1) > 0.2'}", true, new ParserTests_a());
        }

        [Test]
        public void IncorrectOperation() {
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding '1 * `str`'}", null, null);
            var res = BindingListener.GetError();
            Assert.That(res.Contains("Operator '*' cannot be applied to operands of type 'int' and 'string'"));
        }

        [Test]
        public void ExecutingOptimization() {
            var vm = new ParserTests_a();
            Assert.That(vm.Count == 0);
            BindingTestHelper.BindAssert<TextBox>("TextBox", "Tag", "{b:DXBinding 'false and IncreaseCount() == 1'}", false, vm);
            Assert.That(vm.Count == 0);
        }

        [Test]
        public void NewOperator() {
            var vm = new PerformanceTests_a() { DoubleProp = 2 };
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Margin",
                "{b:DXBinding Expr='new $Thickness(DoubleProp, 0, 0, 0)', BackExpr='DoubleProp=@v.Left', Mode=TwoWay}", null, vm);
            Assert.That(tb.Margin == new Thickness(2, 0, 0, 0));
            tb.Margin = new Thickness(3); BindingTestHelper.DoEvents(tb);
            Assert.That(vm.DoubleProp == 3);

            vm = new PerformanceTests_a() { DoubleProp = 2 };
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Margin",
                @"{b:DXBinding 
                    Expr='new $Thickness(DoubleProp, 0, 0, 0)', 
                    BackExpr='DoubleProp=new $test:PerformanceTests_a(@v.Left).DoubleProp', 
                    Mode=TwoWay}", null, vm);
            Assert.That(tb.Margin == new Thickness(2, 0, 0, 0));
            tb.Margin = new Thickness(3); BindingTestHelper.DoEvents(tb);
            Assert.That(vm.DoubleProp == 3);
        }

        [Test]
        public override void NullableTest2() {
            var vm = new BindingTests_a() { NullableBoolean = true };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                    "{b:DXBinding 'GetSelf2().NullableBoolean'}", true, vm);
            vm.NullableBoolean = false;
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                    "{b:DXBinding 'GetSelf2().NullableBoolean'}", false, vm);
            vm.NullableBoolean = null;
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                    "{b:DXBinding 'GetSelf2().NullableBoolean'}", true, vm);

            vm = new BindingTests_a() { NullableBoolean = true };
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                    "{b:DXBinding 'GetSelf().NullableBoolean'}", true, vm);
            vm.NullableBoolean = false;
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                    "{b:DXBinding 'GetSelf().NullableBoolean'}", false, vm);
            vm.NullableBoolean = null;
            BindingTestHelper.BindAssert<TextBox>("TextBox", "IsEnabled",
                    "{b:DXBinding 'GetSelf().NullableBoolean'}", true, vm);
        }

        [Test]
        public void BindingDoNothingTest() {
            var vm = ViewModelSource.Create<BindingDoNothingTestVM>();
            vm.DoNothing = false;
            vm.Text = "1";
            var tb = BindingTestHelper.BindAssert<TextBlock>("TextBlock", "Text",
                    "{b:DXBinding '!DoNothing ? Text : $Binding.DoNothing'}", "1", vm);
            vm.Text = "2";
            BindingTestHelper.DoEvents(tb);
            Assert.AreEqual("2", tb.Text);

            vm.DoNothing = true;
            vm.Text = "3";
            BindingTestHelper.DoEvents(tb);
            Assert.AreEqual("2", tb.Text);

            vm.Text = "4";
            BindingTestHelper.DoEvents(tb);
            Assert.AreEqual("2", tb.Text);

            vm.DoNothing = false;
            BindingTestHelper.DoEvents(tb);
            Assert.AreEqual("4", tb.Text);
        }
        public class BindingDoNothingTestVM {
            public virtual bool DoNothing { get; set; }
            public virtual string Text { get; set; }
        }

        [Test]
        public void T684394() {
            string xaml = @"
<Grid>
    <Grid.Resources>
        <sys:Int32 x:Key=""r1"">1</sys:Int32>
        <sys:Int32 x:Key=""r2"">2</sys:Int32>

        <DataTemplate x:Key=""temp"">
            <TextBlock Text=""{b:DXBinding '@c ? @r(r1) : @r(r2)'}""/>
        </DataTemplate>
    </Grid.Resources>
    <ContentControl Content=""{b:DXBinding}"" ContentTemplate=""{StaticResource temp}""/>
    <ContentControl Content=""{b:DXBinding}"" ContentTemplate=""{StaticResource temp}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            BindingTestHelper.VisualTest(panel, () => {
                var tb1 = LayoutTreeHelper.GetVisualChildren(panel.Children[0]).OfType<TextBlock>().First();
                var tb2 = LayoutTreeHelper.GetVisualChildren(panel.Children[1]).OfType<TextBlock>().First();
                Assert.AreEqual(string.Empty, tb1.Text);
                Assert.AreEqual(string.Empty, tb2.Text);

                panel.DataContext = true;
                BindingTestHelper.DoEvents(panel);
                Assert.AreEqual("1", tb1.Text);
                Assert.AreEqual("1", tb2.Text);

                panel.DataContext = false;
                BindingTestHelper.DoEvents(panel);
                Assert.AreEqual("2", tb1.Text);
                Assert.AreEqual("2", tb2.Text);
            });
        }

        [Test]
        public void T745460() {
            string xaml = @"
<DockPanel Visibility=""{DXBinding Expr='Items.Count > 0'}"" />
";
            var panel = BindingTestHelper.LoadXaml<DockPanel>(xaml);
            BindingTestHelper.VisualTest(panel, () => {
                Assert.AreEqual(Visibility.Visible, panel.Visibility);
                var vm = new T745460_Class();
                panel.DataContext = vm;
                BindingTestHelper.DoEvents(panel);
                Assert.AreEqual(Visibility.Visible, panel.Visibility);
                vm.Items.Add("test");
                BindingTestHelper.DoEvents(panel);
                Assert.AreEqual(Visibility.Visible, panel.Visibility);
            });
        }
        public class T745460_Class {
            public ObservableCollection<string> Items { get; set; }
            public T745460_Class() {
                Items = new ObservableCollection<string>();
            }
        }

        [Test]
        public void T813754() {
            var vm = new PerformanceTests_a() { DoubleProp = 2 };
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding Expr='DoubleProp == 2 ? $Brushes.Yellow : $Brushes.Red'}", null, vm);
            Assert.AreEqual(Colors.Yellow, (tb.Foreground as SolidColorBrush)?.Color);
            vm.DoubleProp = 3;
            BindingTestHelper.DoEvents(tb);
            Assert.AreEqual(Colors.Red, (tb.Foreground as SolidColorBrush)?.Color);

            vm = new PerformanceTests_a() { DoubleProp = 2 };
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding Expr='DoubleProp == 2 ? $Colors.Yellow : $Colors.Red'}", null, vm);
            Assert.AreEqual(Colors.Yellow, (tb.Foreground as SolidColorBrush)?.Color);
            vm.DoubleProp = 3;
            BindingTestHelper.DoEvents(tb);
            Assert.AreEqual(Colors.Red, (tb.Foreground as SolidColorBrush)?.Color);
        }

        [Test]
        public void T823303() {
            var vm = new T823303_VM() { Prop = 1 };
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
               "{b:DXBinding Expr='Convert(Prop)', BackExpr='Prop=$test:T823303_VM.ConvertBack($int.Parse(@v))'}", null, vm);
            Assert.AreEqual(1, vm.Prop);
            Assert.AreEqual("100", tb.Text);
            tb.Text = "200";
            BindingTestHelper.DoEvents(tb);
            Assert.AreEqual("200", tb.Text);
            Assert.AreEqual(2, vm.Prop);

            vm = new T823303_VM() { Prop = 1, Prop2 = 1 };
            tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
               "{b:DXBinding Expr='Prop2 + Convert(Prop)', BackExpr='Prop=$test:T823303_VM.ConvertBack($int.Parse(@v) - 1)'}", null, vm);
            Assert.AreEqual(1, vm.Prop);
            Assert.AreEqual("101", tb.Text);
            tb.Text = "201";
            BindingTestHelper.DoEvents(tb);
            Assert.AreEqual("201", tb.Text);
            Assert.AreEqual(2, vm.Prop);
            var b = BindingOperations.GetMultiBinding(tb, TextBox.TextProperty);
            var b1 = (Binding)b.Bindings[0];
            var b2 = (Binding)b.Bindings[1];
            var b3 = (Binding)b.Bindings[2];
            Assert.AreEqual(true, b1.Mode == BindingMode.OneWay);
            Assert.AreEqual(true, b1.Path.Path == "Prop2");
            Assert.AreEqual(true, b2.Mode == BindingMode.OneWay);
            Assert.AreEqual(true, b2.Path.Path == ".");
            Assert.AreEqual(true, b3.Mode == BindingMode.TwoWay);
            Assert.AreEqual(true, b3.Path.Path == "Prop");
        }
    }
    public class T823303_VM : BindableBase {
        public int Prop { get { return GetValue<int>(); } set { SetValue(value); } }
        public int Prop2 { get { return GetValue<int>(); } set { SetValue(value); } }
        public int Convert(int x) {
            return x * 100;
        }
        public static int ConvertBack(int x) {
            return x / 100;
        }
    }

    [Platform("NET")]
    [TestFixture]
    public class BindingTests_Performance {
        [SetUp]
        public virtual void Init() {
            BindingListener.Enable();
            BindingTestHelper.TestsSetUp();
        }
        [TearDown]
        public virtual void TearDown() {
            BindingTestHelper.TestsTearDown();
            BindingListener.Disable();
        }

#region xamls
        string standardXaml = @"
<TextBox>
    <TextBox.DataContext>
        <test:PerformanceTests_a IntProp=""1"" DoubleProp=""2.2"" StringProp=""test""/>
    </TextBox.DataContext>
    <TextBox.Text>
        <MultiBinding Converter=""{test:PerformanceTests_StandardConverter}"" Mode=""OneWay"">
            <MultiBinding.Bindings>
                <Binding Path=""IntProp""/>
                <Binding Path=""DoubleProp""/>
                <Binding Path=""StringProp""/>  
            </MultiBinding.Bindings>
        </MultiBinding>
    </TextBox.Text>
</TextBox>
";
        string dxXaml = @"
<TextBox>
    <TextBox.DataContext>
        <test:PerformanceTests_a IntProp=""1"" DoubleProp=""2.2"" StringProp=""test""/>
    </TextBox.DataContext>
    <TextBox.Text>
        <b:DXBinding Expr=""(IntProp + @c.GetSelf().DoubleProp).ToString() + GetSelf().StringProp"" Mode=""OneWay""/>
    </TextBox.Text>
</TextBox>
";
        string dxXaml_simple = @"
<TextBox>
    <TextBox.DataContext>
        <test:PerformanceTests_a IntProp=""1"" DoubleProp=""2.2"" StringProp=""test""/>
    </TextBox.DataContext>
    <TextBox.Text>
        <b:DXBinding Expr=""(IntProp + DoubleProp).ToString() + StringProp"" Mode=""OneWay""/>
    </TextBox.Text>
</TextBox>
";
#endregion
        void OnePerformanceTestCore(out long dxTime, out long standardTime) {
            Stopwatch w = new Stopwatch();
            Action standardTest = () => {
                var tb = BindingTestHelper.LoadXaml<TextBox>(standardXaml);
                Assert.AreEqual("3.2test", tb.Text);
            };
            Action dxTest = () => {
                var tb = BindingTestHelper.LoadXaml<TextBox>(dxXaml);
                Assert.AreEqual("3.2test", tb.Text);
            };

            standardTest();
            w.Start();
            for (int i = 0; i < 500; i++) {
                standardTest();
            }
            w.Stop();
            standardTime = w.ElapsedMilliseconds;
            w.Reset();

            dxTest();
            w.Start();
            for (int i = 0; i < 500; i++) {
                dxTest();
            }
            w.Stop();
            dxTime = w.ElapsedMilliseconds;
        }
        void PerformanceTestCore(out long dxTime, out long standardTime) {
            List<Tuple<long, long>> results = new List<Tuple<long, long>>();
            for (int i = 0; i < 10; i++) {
                long standardTimeLocal;
                long dxTimeLocal;
                OnePerformanceTestCore(out dxTimeLocal, out standardTimeLocal);
                results.Add(new Tuple<long, long>(dxTimeLocal, standardTimeLocal));
            }
            results.RemoveAt(results.IndexOf(x => x.Item1 == results.Min(y => y.Item1)));
            results.RemoveAt(results.IndexOf(x => x.Item2 == results.Min(y => y.Item2)));
            results.RemoveAt(results.IndexOf(x => x.Item1 == results.Max(y => y.Item1)));
            results.RemoveAt(results.IndexOf(x => x.Item2 == results.Max(y => y.Item2)));
            dxTime = results.Sum(x => x.Item1) / results.Count;
            standardTime = results.Sum(x => x.Item2) / results.Count;
        }

        [Ignore("Performance Test")]
        [Test]
        public virtual void PerformanceTest_Start() {
            long standardTime = 0;
            long dxTime = 0;
            try {
                BindingTestHelper.SetResolvingMode(DXBindingResolvingMode.LegacyStaticTyping);
                PerformanceTestCore(out dxTime, out standardTime);
            } finally {
                BindingTestHelper.ClearResolvingMode();
            }

            long standardTime2 = 0;
            long dxTime2 = 0;
            try {
                BindingTestHelper.SetResolvingMode(DXBindingResolvingMode.DynamicTyping);
                PerformanceTestCore(out dxTime2, out standardTime2);
            } finally {
                BindingTestHelper.ClearResolvingMode();
            }

            Assert.LessOrEqual(dxTime / 3, standardTime);
            Assert.LessOrEqual(dxTime2 / 2, standardTime2);
        }

        void OnePerformanceTestUpdateCore(out long dxTime, out long standardTime) {
            Stopwatch w = new Stopwatch();
            dxTime = 0;
            standardTime = 0;
            var tb = BindingTestHelper.LoadXaml<TextBox>(standardXaml);
            var vm = (PerformanceTests_a)tb.DataContext;

            Action<TextBox, PerformanceTests_a> test = (_tb, _vm) => {
                Assert.AreEqual("3.2test", _tb.Text);
                _vm.IntProp = 2; BindingTestHelper.DoEvents(_tb);
                Assert.AreEqual("4.2test", _tb.Text);
                _vm.DoubleProp = 2.3; BindingTestHelper.DoEvents(_tb);
                Assert.AreEqual("4.3test", _tb.Text);
                _vm.StringProp = "new"; BindingTestHelper.DoEvents(_tb);
                Assert.AreEqual("4.3new", _tb.Text);

                _vm.StringProp = "test"; BindingTestHelper.DoEvents(_tb);
                Assert.AreEqual("4.3test", _tb.Text);
                _vm.DoubleProp = 2.2; BindingTestHelper.DoEvents(_tb);
                Assert.AreEqual("4.2test", _tb.Text);
                _vm.IntProp = 1; BindingTestHelper.DoEvents(_tb);
            };

            w.Start();
            for (int i = 0; i < 500; i++)
                test(tb, vm);
            w.Stop();
            standardTime = w.ElapsedMilliseconds;
            w.Reset();

            tb = BindingTestHelper.LoadXaml<TextBox>(dxXaml_simple);
            vm = (PerformanceTests_a)tb.DataContext;
            w.Start();
            for (int i = 0; i < 500; i++)
                test(tb, vm);
            w.Stop();
            dxTime = w.ElapsedMilliseconds;
            w.Reset();
        }
        void PerformanceTestUpdateCore(out long dxTime, out long standardTime) {
            List<Tuple<long, long>> results = new List<Tuple<long, long>>();
            for (int i = 0; i < 10; i++) {
                long standardTimeLocal;
                long dxTimeLocal;
                OnePerformanceTestUpdateCore(out dxTimeLocal, out standardTimeLocal);
                results.Add(new Tuple<long, long>(dxTimeLocal, standardTimeLocal));
            }
            results.RemoveAt(results.IndexOf(x => x.Item1 == results.Min(y => y.Item1)));
            results.RemoveAt(results.IndexOf(x => x.Item2 == results.Min(y => y.Item2)));
            results.RemoveAt(results.IndexOf(x => x.Item1 == results.Max(y => y.Item1)));
            results.RemoveAt(results.IndexOf(x => x.Item2 == results.Max(y => y.Item2)));
            dxTime = results.Sum(x => x.Item1) / results.Count;
            standardTime = results.Sum(x => x.Item2) / results.Count;
        }
        [Ignore("Performance Test")]
        [Test]
        public virtual void PerformanceTest_Update() {
            long standardTime = 0;
            long dxTime = 0;
            try {
                BindingTestHelper.SetResolvingMode(DXBindingResolvingMode.LegacyStaticTyping);
                PerformanceTestUpdateCore(out dxTime, out standardTime);
            } finally {
                BindingTestHelper.ClearResolvingMode();
            }

            long standardTime2 = 0;
            long dxTime2 = 0;
            try {
                BindingTestHelper.SetResolvingMode(DXBindingResolvingMode.DynamicTyping);
                PerformanceTestUpdateCore(out dxTime2, out standardTime2);
            } finally {
                BindingTestHelper.ClearResolvingMode();
            }
            Assert.LessOrEqual(dxTime / 3, standardTime);
            Assert.LessOrEqual(dxTime2 / 2, standardTime2);
        }
    }

    public class ParserTests_a {
        public static int StaticIntProp { get; set; }
        public static int StaticIntField { get; set; }
        public static ParserTests_a StaticSelf { get; set; }
        public static void Static(int intV = 0) {
            StaticIntProp = StaticIntField = intV;
            StaticSelf = new ParserTests_a().CreateInstance();
            StaticSelf.IntProp = intV;
        }
        public static int StaticGetInt() {
            return StaticIntProp;
        }

        public int @AtProp { get; set; }
        public int @int { get; set; }
        public virtual int IntProp { get; set; }
        public int IntField;
        public virtual double DoubleProp { get; set; }
        public double DoubleField;
        public virtual string StringProp { get; set; }
        public string StringField;
        public string[] Array { get; set; }
        public Dictionary<Type, string> Dictionary { get; set; }
        public ParserTests_a Self { get { return this; } }
        public ParserTests_a(int intV = 0, double doubleV = 0, string stringV = "", string[] array = null, Dictionary<Type, string> dictionary = null) {
            AtProp = @int = IntProp = IntField = intV;
            DoubleProp = DoubleField = doubleV;
            StringProp = StringField = stringV;
            Array = array;
            Dictionary = dictionary;
        }
        public ParserTests_a GetSelf() {
            return this;
        }
        public ParserTests_a GetSelf(ParserTests_a a) {
            return a;
        }
        public int GetInt(int v) {
            return v;
        }
        public int GetInt(double v) {
            return (int)v;
        }
        public object GetObject(int v) {
            return v;
        }
        public object GetObject(double v) {
            return v;
        }
        public object GetObject(object v) {
            return v;
        }

        public T Generic1<T>(T v) {
            return v;
        }

        protected virtual ParserTests_a CreateInstance() {
            return new ParserTests_a();
        }

        public int Count { get; set; }
        public int IncreaseCount() {
            return Count++;
        }
    }

    public enum BindingTests_a_Enum { Enum1, Enum2 }
    public class BindingTests_a : ParserTests_a {
        public static readonly DependencyProperty AttachedPropertyProperty = DependencyProperty.RegisterAttached("AttachedProperty", typeof(int), typeof(BindingTests_a), new PropertyMetadata(null));
        public static int GetAttachedProperty(DependencyObject obj) { return (int)obj.GetValue(AttachedPropertyProperty); }
        public static void SetAttachedProperty(DependencyObject obj, int value) { obj.SetValue(AttachedPropertyProperty, value); }

        public static BindingTests_a Create(int intV = 0, double doubleV = 0, string stringV = "") {
            return ViewModelSource.Create(() => new BindingTests_a(intV, doubleV, stringV));
        }
        public BindingTests_a() { }
        public BindingTests_a(int intV = 0, double doubleV = 0, string stringV = "") : base(intV, doubleV, stringV) { }
        protected override ParserTests_a CreateInstance() {
            return Create();
        }

        public Visibility Visibility1 { get; set; }
        public object Visibility2 { get; set; }
        public bool? NullableBoolean { get;set; }

        public BindingTests_a GetSelf2() {
            return this;
        }

        public SubObj Obj { get; set; }
        public class SubObj {
            public BindingTests_a_Enum Enum1 { get; set; }
            public object Enum2 { get; set; }
        }
    }
    public class BindingTests_visual : Control {
        public BindingTests_visual(int attachedPropertyV = 0) {
            BindingTests_a.SetAttachedProperty(this, attachedPropertyV);
        }
    }

    public class BindingTests_converter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (double)value + 10;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (double)value - 10;
        }
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

    public class FallbackValueTests_a {
        public virtual int IntPropA { get; set; }
        public static FallbackValueTests_a Create() {
            return ViewModelSource.Create(() => new FallbackValueTests_a());
        }
        public FallbackValueTests_a GetSelf() {
            return this;
        }
    }
    public class FallbackValueTests_b {
        public virtual int IntPropB { get; set; }
        public static FallbackValueTests_b Create() {
            return ViewModelSource.Create(() => new FallbackValueTests_b());
        }
        public FallbackValueTests_b GetSelf() {
            return this;
        }
    }
    public class BindingTests_b {
        public virtual int IntProp1 { get; set; }
        public static BindingTests_b Create(int intV = 0) {
            return ViewModelSource.Create(() => new BindingTests_b(intV));
        }
        public BindingTests_b(int intV = 0) {
            IntProp1 = intV;
        }
    }

    public class PerformanceTests_a : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        int intProp;
        public int IntProp {
            get { return intProp; }
            set {
                if (intProp == value) return;
                intProp = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IntProp"));
            }
        }
        double doubleProp;
        public double DoubleProp {
            get { return doubleProp; }
            set {
                if (doubleProp == value) return;
                doubleProp = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DoubleProp"));
            }
        }
        string stringProp;
        public string StringProp {
            get { return stringProp; }
            set {
                if (stringProp == value) return;
                stringProp = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StringProp"));
            }
        }
        public PerformanceTests_a GetSelf() {
            return this;
        }
        public PerformanceTests_a() { }
        public PerformanceTests_a(double v) {
            DoubleProp = v;
        }
    }
    public class PerformanceTests_StandardConverter : MarkupExtension, IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            return ((int)values[0] + (double)values[1]).ToString() + values[2].ToString();
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}