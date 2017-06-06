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

namespace DevExpress.Xpf.DXBinding.Tests {
    static class BindingTestHelper {
        public static TextBlock BindAssert(string property, string binding, object expected = null, object dataContext = null) {
            return BindAssert<TextBlock>("TextBlock", property, binding, expected, dataContext);
        }
        public static void DoEvents(FrameworkElement obj) {
            if(obj == null) return;
            obj.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
        public static T BindAssert<T>(string objToBind, string property, string binding, object expected = null, object dataContext = null, bool assert = true) {
            string xaml = string.Format("<{0} {1}=\"{2}\" />", objToBind, property, binding);
            T res = LoadXaml<T>(xaml);
            if(dataContext != null && res is FrameworkElement)
                (res as FrameworkElement).DataContext = dataContext;
            if(res is FrameworkElement)
                DoEvents(res as FrameworkElement);
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
            DXBindingExtension.DefaultUpdateSourceTrigger = UpdateSourceTrigger.Default;
            DXBindingExtension.IsInDesingModeCore = null;
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

    [TestFixture]
    public class BindingTests {
        [SetUp]
        public void Init() {
            BindingListener.Enable();
            BindingTestHelper.TestsSetUp();
        }
        [TearDown]
        public void TearDown() {
            BindingTestHelper.TestsTearDown();
            BindingListener.Disable();
        }
        [Test]
        public void ErrorBasic() {
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
        public void NoOperandsSimple() {
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
        public void NoOperandsComplex() {
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
        public void NoOperandWithBackConversion() {
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
        public void NoOperandIncorrectExpr() {
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
        public void Operators() {
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
        public void OneOperand() {
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
        public void OneOperandSimple() {
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
        public void OneOperandTwoWay() {
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
        public void OneOperandTwoWay2() {
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
        public void OneOperandOneWayToSource() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntProp, Mode=OneWayToSource}", string.Empty, vm);
            vm.IntProp = 3;
            Assert.AreEqual(string.Empty, tb.Text);
            tb.Text = "4";
            Assert.AreEqual(4, vm.IntProp);
        }

        [Test]
        public void TwoOperands() {
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
        public void TwoOperandsTwoWay() {
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
        public void PropertyAndMethod() {
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
        public void Type() {
            BindingTests_a.Static(1);
            BindingTestHelper.BindAssert("Text", "{b:DXBinding $test:BindingTests_a.StaticIntProp}", "1");
            BindingTestHelper.BindAssert("Text", "{b:DXBinding $test:BindingTests_a.StaticIntProp + $test:BindingTests_a.StaticIntField}", "2");
            BindingTestHelper.BindAssert("Text", "{b:DXBinding $test:BindingTests_a.StaticSelf.IntProp}", "1");
            BindingTestHelper.BindAssert("Text", "{b:DXBinding $test:BindingTests_a.StaticSelf.IntProp + $test:BindingTests_a.StaticGetInt()}", "2");
        }
        [Test]
        public void Attached() {
            BindingTestHelper.BindAssert("Text", "{b:DXBinding ($test:BindingTests_a.AttachedProperty)}", "1", new BindingTests_visual(1));
        }

        [Test]
        public void RelativeSourceSelf() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding @s.Tag}", null, null);
            tb.Tag = 1;
            Assert.AreEqual("1", tb.Text);
            tb.Text = "2";
            Assert.AreEqual("2", tb.Tag);
        }
        [Test]
        public void RelativeSourceElementName() {
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
        [Test, Category("T491236")]
        public void RelativeSourceElementName2() {
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
        public void RelativeSourceStaticResource() {
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
        public void NullOperand() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding IntPropB}",
                null, null);
            Assert.IsEmpty(BindingListener.GetError());
            Assert.AreEqual(string.Empty, tb.Text);
        }
        [Test]
        public void FallbackValue() {
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
        public void FallbackValue2() {
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
        public void FallbackValue3() {
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
        public void TargetNullValue() {
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding StringProp, TargetNullValue='test'}",
                null, BindingTests_a.Create(stringV: null));
            Assert.AreEqual("test", tb.Text);
        }

        [Test, Ignore("Ignore")]
        public void PerformanceTest() {
#region xamls
            string standardXaml = @"
<TextBox>
    <TextBox.DataContext>
        <test:BindingTests_a IntProp=""1"" DoubleProp=""2.2"" StringProp=""test""/>
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
        <test:BindingTests_a IntProp=""1"" DoubleProp=""2.2"" StringProp=""test""/>
    </TextBox.DataContext>
    <TextBox.Text>
        <b:DXBinding Expr=""(IntProp + @c.GetSelf().DoubleProp).ToString() + GetSelf().StringProp"" Mode=""OneWay""/>
    </TextBox.Text>
</TextBox>
";
#endregion
            long standardTime;
            long dxTime;
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
            for(int i = 0; i < 500; i++) {
                standardTest();
            }
            w.Stop();
            standardTime = w.ElapsedMilliseconds;
            w.Reset();

            dxTest();
            w.Start();
            for(int i = 0; i < 500; i++) {
                dxTest();
            }
            w.Stop();
            dxTime = w.ElapsedMilliseconds;
            Assert.LessOrEqual(dxTime - standardTime, standardTime);
            return;
        }

        [Test]
        public void BindingInDataTemplate() {
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
        public void BindingInStyleSetter() {
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
        public void BindingInItemContainerStyle() {
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
        public void BindingInDataTrigger() {
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
        public void BindingInDataTrigger2() {
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
        public void BindingInDataTrigger3() {
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
        public void CoerceEnums() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Visibility",
                "{b:DXBinding Expr='IntProp == 1 ? `Visible` : `Collapsed`', BackExpr='IntProp = @v == $Visibility.Collapsed ? 10 : 20', Mode=TwoWay}",
                Visibility.Collapsed, vm);
            tb.Visibility = Visibility.Hidden;
            Assert.AreEqual(20, vm.IntProp);
        }
        [Test]
        public void CoerceBrush() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding Expr='IntProp == 1 ? `#ff00ff` : `#ff0000`'}",
                null, vm, false);
            var brush = (SolidColorBrush)tb.Foreground;
            Assert.AreEqual(Colors.Red, brush.Color);
        }
        [Test]
        public void CoerceBrushWithoutProperty() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding '`#ff0000`'}",
                null, vm, false);
            var brush = (SolidColorBrush)tb.Foreground;
            Assert.AreEqual(Colors.Red, brush.Color);
        }
        [Test]
        public void CoerceBrushInMultibinding() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding Expr='IntProp == 1 or StringProp == `abc` ? `#ff00ff` : `#ff0000`'}",
                null, vm, false);
            var brush = (SolidColorBrush)tb.Foreground;
            Assert.AreEqual(Colors.Red, brush.Color);
        }
        [Test]
        public void CoerceBrushInMultibinding2() {
            var vm = BindingTests_a.Create(intV: 2);
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Foreground",
                "{b:DXBinding Expr='IntProp == 1 or StringProp == `abc` ? `Yellow` : `Red`'}",
                null, vm, false);
            var brush = (SolidColorBrush)tb.Foreground;
            Assert.AreEqual(Colors.Red, brush.Color);
        }
        [Test]
        public void GetPropertyValueOfStringLiteral() {
            var vm = BindingTests_a.Create();
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                "{b:DXBinding '`abc`.Length', Mode=OneWay}",
                "3", vm);
        }
        [Test]
        public void BinaryStringConcat() {
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
        public void TernaryOperator() {
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
        public void MethodOverloadingResolutionTests() {
            Action<string, string> assert = (expr, expected) => {
                var vm = new SharpSpecOverloadExamples();
                var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text",
                    string.Format("{{b:DXBinding '{0}', Mode=OneWay}}", expr),
                    expected, vm);
            };

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
            assert("F((object)1)", "F(object)");
            assert("F(1, 1)", "F(double, double)");
            assert("F(1, 1, 1)", "F(params double[] ds)");
            assert("F(`a`[0])", "F(int)");
            assert("F(true)", "F(bool, string)");
        }
        class SharpSpecOverloadExamplesBase {
            public string F(char x) {
                return "F(char) base";
            }
            public string F(double x, double y, double z) {
                return "F(double, double, double) base";
            }
        }
        class SharpSpecOverloadExamples {
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
        [Test, Category("T360515")]
        public void DontThrowWhenStaticPropertyNotFound_01() {
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
        [Test, Category("T360515")]
        public void DontThrowWhenStaticPropertyNotFound() {
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
        public void NotRaiseExceptionInDesingMode1() {
            DXBindingBase.IsInDesingModeCore = true;
            var tb = BindingTestHelper.BindAssert<TextBox>("TextBox", "Text", "{b:DXBinding '@a($FrameworkElement).DataContext', BackExpr='@value'}");
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
    }
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