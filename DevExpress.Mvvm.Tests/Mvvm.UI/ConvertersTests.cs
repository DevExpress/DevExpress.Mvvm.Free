#if !SILVERLIGHT
using NUnit.Framework;
#endif
using System;
using System.Windows;
using DevExpress.Mvvm.UI;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class ConvertersTest {
        [Test]
        public void BooleanToVisibilityConverter() {
            var converter = new BooleanToVisibilityConverter();

            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(false, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(new Nullable<bool>(true), typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(new Nullable<bool>(false), typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(new Nullable<bool>(), typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(null, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert("test", typeof(Visibility), null, null));
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Visible, typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack("test", typeof(bool), null, null));
            Assert.AreEqual(new bool?(true), converter.ConvertBack(Visibility.Visible, typeof(bool?), null, null));
            Assert.AreEqual(new bool?(false), converter.ConvertBack(Visibility.Collapsed, typeof(bool?), null, null));
#if !SILVERLIGHT
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null));
            Assert.AreEqual(new bool?(false), converter.ConvertBack(Visibility.Hidden, typeof(bool?), null, null));
#endif

            converter.Inverse = true;
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(false, typeof(Visibility), null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Visible, typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, null));
#if !SILVERLIGHT
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null));
#endif

            converter.Inverse = false;
            converter.HiddenInsteadOfCollapsed = true;
            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Visible, typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, null));
#if !SILVERLIGHT
            Assert.AreEqual(Visibility.Hidden, converter.Convert(false, typeof(Visibility), null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null));
#endif
        }
        [Test]
        public void NegationConverter_Convert_NoTargetType() {
            var converter = new BooleanNegationConverter();
            Assert.AreEqual(null, converter.Convert(null, null, null, null));
            Assert.AreEqual(false, converter.Convert(true, null, null, null));
            Assert.AreEqual(true, converter.Convert(false, null, null, null));
            Assert.AreEqual(false, converter.Convert(new Nullable<bool>(true), null, null, null));
            Assert.AreEqual(true, converter.Convert(new Nullable<bool>(false), null, null, null));
            Assert.AreEqual(null, converter.Convert(new Nullable<bool>(), null, null, null));
            Assert.AreEqual(null, converter.Convert("test", null, null, null));
        }
        [Test]
        public void NegationConverter_Convert_TargetTypeBool() {
            var converter = new BooleanNegationConverter();
            Assert.AreEqual(true, converter.Convert(null, typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(true, typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert(false, typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(new Nullable<bool>(true), typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert(new Nullable<bool>(false), typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert(new Nullable<bool>(), typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert("test", typeof(bool), null, null));
        }
        [Test]
        public void NegationConverter_Convert_TargetTypeNullableBool() {
            var converter = new BooleanNegationConverter();
            Assert.AreEqual(null, converter.Convert(null, typeof(bool?), null, null));
            Assert.AreEqual(false, converter.Convert(true, typeof(bool?), null, null));
            Assert.AreEqual(true, converter.Convert(false, typeof(bool?), null, null));
            Assert.AreEqual(false, converter.Convert(new Nullable<bool>(true), typeof(bool?), null, null));
            Assert.AreEqual(true, converter.Convert(new Nullable<bool>(false), typeof(bool?), null, null));
            Assert.AreEqual(null, converter.Convert(new Nullable<bool>(), typeof(bool?), null, null));
            Assert.AreEqual(null, converter.Convert("test", typeof(bool?), null, null));
        }
        [Test]
        public void NegationConverter_ConvertBack_NoTargetType() {
            var converter = new BooleanNegationConverter();
            Assert.AreEqual(null, converter.ConvertBack(null, null, null, null));
            Assert.AreEqual(false, converter.ConvertBack(true, null, null, null));
            Assert.AreEqual(true, converter.ConvertBack(false, null, null, null));
            Assert.AreEqual(false, converter.ConvertBack(new Nullable<bool>(true), null, null, null));
            Assert.AreEqual(true, converter.ConvertBack(new Nullable<bool>(false), null, null, null));
            Assert.AreEqual(null, converter.ConvertBack(new Nullable<bool>(), null, null, null));
            Assert.AreEqual(null, converter.ConvertBack("test", null, null, null));
        }
        [Test]
        public void NegationConverter_ConvertBack_TargetTypeBool() {
            var converter = new BooleanNegationConverter();
            Assert.AreEqual(true, converter.ConvertBack(null, typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack(true, typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack(false, typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack(new Nullable<bool>(true), typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack(new Nullable<bool>(false), typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack(new Nullable<bool>(), typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack("test", typeof(bool), null, null));
        }
        [Test]
        public void NegationConverter_ConvertBack_TargetTypeNullableBool() {
            var converter = new BooleanNegationConverter();
            Assert.AreEqual(null, converter.ConvertBack(null, typeof(bool?), null, null));
            Assert.AreEqual(false, converter.ConvertBack(true, typeof(bool?), null, null));
            Assert.AreEqual(true, converter.ConvertBack(false, typeof(bool?), null, null));
            Assert.AreEqual(false, converter.ConvertBack(new Nullable<bool>(true), typeof(bool?), null, null));
            Assert.AreEqual(true, converter.ConvertBack(new Nullable<bool>(false), typeof(bool?), null, null));
            Assert.AreEqual(null, converter.ConvertBack(new Nullable<bool>(), typeof(bool?), null, null));
            Assert.AreEqual(null, converter.ConvertBack("test", typeof(bool?), null, null));
        }
        [Test]
        public void DefaultBooleanToBooleanConverter_Convert_NoTargetType() {
            var converter = new DefaultBooleanToBooleanConverter();
            Assert.AreEqual(null, converter.Convert(null, null, null, null));
            Assert.AreEqual(true, converter.Convert(true, null, null, null));
            Assert.AreEqual(false, converter.Convert(false, null, null, null));
            Assert.AreEqual(true, converter.Convert(new Nullable<bool>(true), null, null, null));
            Assert.AreEqual(false, converter.Convert(new Nullable<bool>(false), null, null, null));
            Assert.AreEqual(null, converter.Convert(new Nullable<bool>(), null, null, null));
            Assert.AreEqual(null, converter.Convert("test", null, null, null));
        }
        [Test]
        public void DefaultBooleanToBooleanConverter_Convert_TargetTypeBool() {
            var converter = new DefaultBooleanToBooleanConverter();
            Assert.AreEqual(false, converter.Convert(null, typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert(true, typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(false, typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert(new Nullable<bool>(true), typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(new Nullable<bool>(false), typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(new Nullable<bool>(), typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert("test", typeof(bool), null, null));
        }
        [Test]
        public void DefaultBooleanToBooleanConverter_Convert_TargetTypeNullableBool() {
            var converter = new DefaultBooleanToBooleanConverter();
            Assert.AreEqual(null, converter.Convert(null, typeof(bool?), null, null));
            Assert.AreEqual(true, converter.Convert(true, typeof(bool?), null, null));
            Assert.AreEqual(false, converter.Convert(false, typeof(bool?), null, null));
            Assert.AreEqual(true, converter.Convert(new Nullable<bool>(true), typeof(bool?), null, null));
            Assert.AreEqual(false, converter.Convert(new Nullable<bool>(false), typeof(bool?), null, null));
            Assert.AreEqual(null, converter.Convert(new Nullable<bool>(), typeof(bool?), null, null));
            Assert.AreEqual(null, converter.Convert("test", typeof(bool?), null, null));
        }
        [Test]
        public void DefaultBooleanToBooleanConverter_ConvertBack_TargetTypeBool() {
            var converter = new DefaultBooleanToBooleanConverter();
            Assert.AreEqual(false, converter.ConvertBack(null, typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack(true, typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack(false, typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack(new Nullable<bool>(true), typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack(new Nullable<bool>(false), typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack(new Nullable<bool>(), typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack("test", typeof(bool), null, null));
        }
        [Test]
        public void DefaultBooleanToBooleanConverter_ConvertBack_TargetTypeNullableBool() {
            var converter = new DefaultBooleanToBooleanConverter();
            Assert.AreEqual(null, converter.ConvertBack(null, typeof(bool?), null, null));
            Assert.AreEqual(true, converter.ConvertBack(true, typeof(bool?), null, null));
            Assert.AreEqual(false, converter.ConvertBack(false, typeof(bool?), null, null));
            Assert.AreEqual(true, converter.ConvertBack(new Nullable<bool>(true), typeof(bool?), null, null));
            Assert.AreEqual(false, converter.ConvertBack(new Nullable<bool>(false), typeof(bool?), null, null));
            Assert.AreEqual(null, converter.ConvertBack(new Nullable<bool>(), typeof(bool?), null, null));
            Assert.AreEqual(null, converter.ConvertBack("test", typeof(bool?), null, null));
        }
        enum MyColorEnum {
            Red, Green, Blue
        }
        [Test]
        public void ObjectToObjectConverter() {
            var converter = new ObjectToObjectConverter();

            Assert.AreEqual(null, converter.Convert(10, typeof(object), null, null));
            Assert.AreEqual(null, converter.ConvertBack(10, typeof(object), null, null));

            var instance1 = new MyClass("same");
            var instance2 = new MyClass("same");
            string converted1 = "converted";
            string converted2 = "converted";

            converter.Map.Add(new MapItem { Source = instance1, Target = "converted" });
            Assert.AreEqual(converted1, converter.Convert(instance1, typeof(object), null, null));
            Assert.AreEqual(converted1, converter.Convert(instance2, typeof(object), null, null));
            Assert.AreEqual(converted2, converter.Convert(instance1, typeof(object), null, null));
            Assert.AreEqual(converted2, converter.Convert(instance2, typeof(object), null, null));
            Assert.AreEqual(instance1, converter.ConvertBack(converted1, typeof(object), null, null));
            Assert.AreEqual(instance1, converter.ConvertBack(converted2, typeof(object), null, null));
            Assert.AreEqual(instance2, converter.ConvertBack(converted1, typeof(object), null, null));
            Assert.AreEqual(instance2, converter.ConvertBack(converted2, typeof(object), null, null));

            converter.Map.Add(new MapItem { Source = null, Target = "nullto" });
            converter.Map.Add(new MapItem { Source = "nullfrom", Target = null });
            Assert.AreEqual("nullto", converter.Convert(null, typeof(object), null, null));
            Assert.AreEqual("nullfrom", converter.ConvertBack(null, typeof(object), null, null));

            converter.DefaultSource = "defsource";
            converter.DefaultTarget = "deftarget";
            Assert.AreEqual("deftarget", converter.Convert("nonexistent", typeof(object), null, null));
            Assert.AreEqual("defsource", converter.ConvertBack("nonexistent", typeof(object), null, null));

            converter = new ObjectToObjectConverter();
            converter.Map.Add(new MapItem { Source = "true", Target = 1 });
            converter.Map.Add(new MapItem { Source = "FALSE", Target = 15 });
            Assert.AreEqual(1, converter.Convert(true, typeof(int), null, null));
            Assert.AreEqual(15, converter.Convert(false, typeof(int), null, null));

            converter = new ObjectToObjectConverter();
            converter.DefaultTarget = Visibility.Visible;
            converter.Map.Add(new MapItem { Source = 0, Target = Visibility.Collapsed });
            Assert.AreEqual(Visibility.Visible, converter.Convert(null, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(10, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(0, typeof(Visibility), null, null));
        }
        [Test]
        public void ObjectToObjectCoercions() {
            var converter = new ObjectToObjectConverter();
            converter.Map.Add(new MapItem { Source = "Red", Target = "1" });
            converter.Map.Add(new MapItem { Source = "Green", Target = "2" });
            converter.Map.Add(new MapItem { Source = MyColorEnum.Blue, Target = "3" });
            Assert.AreEqual("1", converter.Convert("Red", typeof(string), null, null));
            Assert.AreEqual("2", converter.Convert(MyColorEnum.Green, typeof(string), null, null));
            Assert.AreEqual("3", converter.Convert("Blue", typeof(string), null, null));
            Assert.AreEqual(null, converter.Convert(null, typeof(string), null, null));
            converter.DefaultTarget = "def";
            Assert.AreEqual("def", converter.Convert(null, typeof(string), null, null));

            converter.Map.Add(new MapItem { Source = null, Target = "nullvalue" });
            Assert.AreEqual("nullvalue", converter.Convert(null, typeof(string), null, null));
        }
        [Test]
        public void FormatStringConverter() {
            var savedCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            try {
                FormatStringConverter converter = new FormatStringConverter();
                converter.FormatString = "C0";
                var changedCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentUICulture.Clone();
                changedCulture.NumberFormat.CurrencySymbol = "#test currency symbol#";
                System.Threading.Thread.CurrentThread.CurrentUICulture = changedCulture;
                string s = (string)converter.Convert(13, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture);
                Assert.IsTrue(s.Contains("#test currency symbol#"));
            } finally {
                System.Threading.Thread.CurrentThread.CurrentUICulture = savedCulture;
            }
        }
        class MyClass {
            string id;
            public MyClass(string id) {
                this.id = id;
            }
            public override bool Equals(object obj) {
                if(obj is MyClass) {
                    return ((MyClass)obj).id == id;
                }
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return id.GetHashCode();
            }
        }
        [Test]
        public void NotNullObjectConverter() {
            var converter = new ObjectToBooleanConverter();
            Assert.AreEqual(true, converter.Convert("not null", typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(null, typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert(new object(), typeof(bool), null, null));
        }
        [Test]
        public void NotEmptyStringConverter() {
            var converter = new StringToBooleanConverter();
            Assert.AreEqual(true, converter.Convert("not null", typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(null, typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert("", typeof(bool), null, null));
            Assert.AreEqual(null, converter.ConvertBack(null, typeof(bool), null, null));
        }
        [Test]
        public void NotZeroBooleanConverter() {
            var converter = new NumericToBooleanConverter();
            Assert.AreEqual(false, converter.Convert(null, typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(0, typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(0f, typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(0d, typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert(10, typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert(-10, typeof(bool), null, null));
            Assert.AreEqual(true, converter.Convert(5d, typeof(bool), null, null));
        }
        [Test]
        public void BooleanToObjectConverter() {
            var converter = new BooleanToObjectConverter();
            Assert.AreEqual(null, converter.Convert(null, typeof(string), null, null));
            Assert.AreEqual(null, converter.Convert("s1", typeof(string), null, null));
            converter.TrueValue = "trueValue";
            converter.FalseValue = "falseValue";
            Assert.AreEqual("trueValue", converter.Convert(true, typeof(string), null, null));
            Assert.AreEqual("falseValue", converter.Convert(false, typeof(string), null, null));
            Assert.AreEqual(null, converter.Convert("garbage", typeof(string), null, null));
            converter.NullValue = "nullvalue";
            Assert.AreEqual("nullvalue", converter.Convert(null, typeof(string), null, null));
        }

        [Test]
        public void NumericToBooleanConverter() {
            NumericToBooleanConverter converter = new NumericToBooleanConverter();
            Assert.AreEqual(false, converter.Convert(0, null, null, null));
            Assert.AreEqual(true, converter.Convert(-1, null, null, null));
            Assert.AreEqual(true, converter.Convert(1, null, null, null));
            Assert.AreEqual(true, converter.Convert(2, null, null, null));
        }
        [Test]
        public void NumericToVisibilityConverter() {
            NumericToVisibilityConverter converter = new NumericToVisibilityConverter();
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(0, null, null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(-1, null, null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(1, null, null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(2, null, null, null));

            converter.Inverse = true;
            Assert.AreEqual(Visibility.Visible, converter.Convert(0, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(-1, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(1, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(2, null, null, null));
        }
    }
}