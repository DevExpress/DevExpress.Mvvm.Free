using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DevExpress.Mvvm.Native;
using System.Windows.Controls;
using DevExpress.Mvvm.POCO;
using System;
using System.Windows;
using NUnit.Framework;
using DevExpress.Mvvm.UI;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class ConvertersTest {
        [Test]
        public void ReflectionConverterShouldNotPassNullIntoContructorIfTargetTypeIsNotValueType() {
            var converter1 = new ReflectionConverter();
            Assert.AreEqual(null, (FromString)converter1.Convert(null, typeof(FromString), null, null));
        }
        [Test]
        public void ReflectionConverterTest() {
            var converter1 = new ReflectionConverter();
            Assert.IsNull(converter1.Convert(null, null, null, null));
            Assert.IsNull(converter1.ConvertBack(null, null, null, null));
            Assert.AreEqual(15, converter1.Convert(15, null, null, null));
            Assert.AreEqual(15, converter1.ConvertBack(15, null, null, null));
            Assert.AreEqual("abc", ((FromString)converter1.Convert("abc", typeof(FromString), null, null)).S);
            Assert.AreEqual("abc", ((FromString)converter1.ConvertBack("abc", typeof(FromString), null, null)).S);
            var converter2 = new ReflectionConverter() { ConvertMethod = "ToString", ConvertBackMethodOwner = typeof(FromString) };
            Assert.AreEqual("15", converter2.Convert(new FromString("15"), null, null, null));
            Assert.AreEqual("abc", ((FromString)converter2.ConvertBack("abc", null, null, null)).S);
            var converter3 = new ReflectionConverter() { ConvertMethodOwner = typeof(FromString), ConvertBackMethod = "ToString", ConvertBackMethodOwner = null };
            Assert.AreEqual("abc", ((FromString)converter3.Convert("abc", null, null, null)).S);
            Assert.AreEqual("15", converter3.ConvertBack(new FromString("15"), null, null, null));
            var converter4 = new ReflectionConverter() { ConvertMethodOwner = typeof(ReflectionConverterTestMethods), ConvertMethod = "IntToString", ConvertBackMethod = "StringToInt" };
            Assert.AreEqual("S15", converter4.Convert(15, null, null, null));
            Assert.AreEqual(15, converter4.ConvertBack("S15", null, null, null));
        }
        [Test]
        public void ReflectionConverter_ConvertBackMethodOwnerTest() {
            Assert.AreEqual(typeof(int), new ReflectionConverter() { ConvertMethodOwner = typeof(int) }.ConvertBackMethodOwner);
            Assert.AreEqual(null, new ReflectionConverter() { ConvertMethodOwner = typeof(int), ConvertBackMethodOwner = null }.ConvertBackMethodOwner);
            Assert.AreEqual(typeof(int), new ReflectionConverterExtension() { ConvertMethodOwner = typeof(int) }.ConvertBackMethodOwner);
            Assert.AreEqual(null, new ReflectionConverterExtension() { ConvertMethodOwner = typeof(int), ConvertBackMethodOwner = null }.ConvertBackMethodOwner);
            Assert.AreEqual(typeof(int), new ReflectionConverterExtension() { ConvertMethodOwner = typeof(int) }.ProvideValue(null).With(x => (ReflectionConverter)x).ConvertBackMethodOwner);
            Assert.AreEqual(null, new ReflectionConverterExtension() { ConvertMethodOwner = typeof(int), ConvertBackMethodOwner = null }.ProvideValue(null).With(x => (ReflectionConverter)x).ConvertBackMethodOwner);
        }
        [Test]
        public void ReflectionConverter_ConvertMethodParametersTest() {
            ReflectionConvertTestMockStatic.Convert01Function = (object v) => ReflectionConverter_Result(v, null);
            ReflectionConvertTestMockStatic.Convert02Function = (int v) => ReflectionConverter_Result(v, null);
            ReflectionConverter_ConvertMethodParametersTestCore("Convert01", false); ReflectionConverter_ConvertMethodParametersTestCore("Convert02", false);
            ReflectionConvertTestMockStatic.Convert03Function = (int v, int p) => ReflectionConverter_Result(v, p);
            ReflectionConvertTestMockStatic.Convert04Function = (int v, object p) => ReflectionConverter_Result(v, p);
            ReflectionConverter_ConvertMethodParametersTestCore("Convert03", true); ReflectionConverter_ConvertMethodParametersTestCore("Convert04", true);
            ReflectionConvertTestMockStatic.Convert05Function = (object v, int p) => ReflectionConverter_Result(v, p);
            ReflectionConvertTestMockStatic.Convert06Function = (object v, Type t, int p) => ReflectionConverter_Result(v, p);
            ReflectionConverter_ConvertMethodParametersTestCore("Convert05", true); ReflectionConverter_ConvertMethodParametersTestCore("Convert06", true);
            ReflectionConvertTestMockStatic.Convert07Function = (object v, Type t, object p) => ReflectionConverter_Result(v, p);
            ReflectionConvertTestMockStatic.Convert08Function = (int v, Type t, object p) => ReflectionConverter_Result(v, p);
            ReflectionConverter_ConvertMethodParametersTestCore("Convert07", true); ReflectionConverter_ConvertMethodParametersTestCore("Convert08", true);
            ReflectionConvertTestMockStatic.Convert17Function = (object v, Type t) => ReflectionConverter_Result(v, null);
            ReflectionConvertTestMockStatic.Convert18Function = (int v, Type t) => ReflectionConverter_Result(v, null);
            ReflectionConverter_ConvertMethodParametersTestCore("Convert17", false); ReflectionConverter_ConvertMethodParametersTestCore("Convert18", false);
            ReflectionConvertTestMockStatic.Convert09Function = (object v, CultureInfo c) => ReflectionConverter_Result(v, null);
            ReflectionConvertTestMockStatic.Convert10Function = (int v, CultureInfo c) => ReflectionConverter_Result(v, null);
            ReflectionConverter_ConvertMethodParametersTestCore("Convert09", false); ReflectionConverter_ConvertMethodParametersTestCore("Convert10", false);
            ReflectionConvertTestMockStatic.Convert11Function = (int v, int p, CultureInfo c) => ReflectionConverter_Result(v, p);
            ReflectionConvertTestMockStatic.Convert12Function = (int v, object p, CultureInfo c) => ReflectionConverter_Result(v, p);
            ReflectionConverter_ConvertMethodParametersTestCore("Convert11", true); ReflectionConverter_ConvertMethodParametersTestCore("Convert12", true);
            ReflectionConvertTestMockStatic.Convert13Function = (object v, int p, CultureInfo c) => ReflectionConverter_Result(v, p);
            ReflectionConvertTestMockStatic.Convert14Function = (object v, Type t, int p, CultureInfo c) => ReflectionConverter_Result(v, p);
            ReflectionConverter_ConvertMethodParametersTestCore("Convert13", true); ReflectionConverter_ConvertMethodParametersTestCore("Convert14", true);
            ReflectionConvertTestMockStatic.Convert15Function = (object v, Type t, object p, CultureInfo c) => ReflectionConverter_Result(v, p);
            ReflectionConvertTestMockStatic.Convert16Function = (int v, Type t, object p, CultureInfo c) => ReflectionConverter_Result(v, p);
            ReflectionConverter_ConvertMethodParametersTestCore("Convert15", true); ReflectionConverter_ConvertMethodParametersTestCore("Convert16", true);
        }
        void ReflectionConverter_ConvertMethodParametersTestCore(string method, bool withParameter) {
            IValueConverter reflectionConverter = new ReflectionConverter() { ConvertMethodOwner = typeof(ReflectionConvertTestMockStatic), ConvertMethod = method };
            Assert.AreEqual(withParameter ? "36" : "22", reflectionConverter.Convert(22, typeof(string), 14, CultureInfo.InvariantCulture));
        }
        string ReflectionConverter_Result(object value, object parameter) {
            int p = parameter == null ? 0 : (int)parameter;
            return ((int)value + p).ToString();
        }

        [Test]
        public void EnumerableConverter_ConvertToEnumerableTest() {
            var converter = new EnumerableConverter() { TargetItemType = typeof(string), ItemConverter = new ToStringConverter() };
            Assert.AreEqual(null, converter.Convert(null, null, null, null));
            AssertHelper.AssertEnumerablesAreEqual(new string[] { }, converter.Convert(Enumerable.Empty<int>(), null, null, null).With(x => (IEnumerable<string>)x).With(x => x.ToArray()));
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "0", "1", "2" }, (IEnumerable<string>)converter.Convert(new int[] { 0, 1, 2 }, null, null, null));
        }
        IEnumerable<int> GetInfiniteEnumerable() {
            for(int i = 0; ; ++i)
                yield return i;
        }
        [Test]
        public void EnumerableConverter_ConvertInfiniteEnumerableTest() {
            foreach(var targetType in new Type[] { null, typeof(IEnumerable<string>) }) {
                var converter = new EnumerableConverter() { TargetItemType = typeof(string), ItemConverter = new ToStringConverter() };
                var result = (IEnumerable<string>)converter.Convert(GetInfiniteEnumerable(), null, null, null);
                string[] array500 = result.Take(500).ToArray();
                for(int i = 0; i < 500; ++i)
                    Assert.AreEqual(i.ToString(), array500[i]);
            }
        }
        [Test]
        public void EnumerableConverter_ConvertToCollectionTest() {
            var converter = new EnumerableConverter() { TargetItemType = typeof(string), ItemConverter = new ToStringConverter() };
            TestEnumerableConverter<IList<string>>(converter);
            TestEnumerableConverter<ICollection<string>>(converter);
            TestEnumerableConverter<IEnumerable<string>>(converter);
            TestEnumerableConverter<List<string>>(converter);
            TestEnumerableConverter<Collection<string>>(converter);
            TestEnumerableConverter<ObservableCollection<string>>(converter);
            TestEnumerableConverter<ReadOnlyCollection<string>>(converter);
            TestEnumerableConverter<StringCollection>(converter);
            TestEnumerableConverter<ArrayList>(converter);
            TestEnumerableConverter<Queue>(converter);
            TestEnumerableConverter<Queue<string>>(converter);
        }
        [Test]
        public void EnumerableConverter_TryConvertToAbstractCollectionTest() {
            var converter = new EnumerableConverter() { TargetItemType = typeof(string), ItemConverter = new ToStringConverter() };
            AssertHelper.AssertThrows<NotSupportedCollectionException>(() => {
                converter.Convert(new int[] { 0, 1, 2 }, typeof(ReadOnlyCollectionBase), null, null);
            }, e => {
                Assert.AreEqual("Cannot create an abstract class.", e.InnerException.Message);
            });
        }
        [Test]
        public void EnumerableConverter_TryConvertToInvalidCollectionTest() {
            var converter = new EnumerableConverter() { TargetItemType = typeof(string), ItemConverter = new ToStringConverter() };
            Assert.Throws<NotSupportedCollectionException>(() => {
                converter.Convert(new int[] { 0, 1, 2 }, Enumerable.Empty<string>().GetType(), null, null);
            });
        }
        [Test]
        public void EnumerableConverter_DetectTargetItemTypeTest() {
            var converter = new EnumerableConverter() { ItemConverter = new ToStringConverter() };
            TestEnumerableConverter<IList<string>>(converter);
            TestEnumerableConverter<ICollection<string>>(converter);
            TestEnumerableConverter<IEnumerable<string>>(converter);
            TestEnumerableConverter<List<string>>(converter);
            TestEnumerableConverter<Collection<string>>(converter);
            TestEnumerableConverter<ObservableCollection<string>>(converter);
            TestEnumerableConverter<ReadOnlyCollection<string>>(converter);
        }
        [Test]
        public void EnumerableConverter_DetectTargetItemTypeFailTest() {
            var converter = new EnumerableConverter() { ItemConverter = new ToStringConverter() };
            Assert.Throws<InvalidOperationException>(() => {
                converter.Convert(new int[] { 0, 1, 2 }, typeof(StringCollection), null, null);
            });
        }
        void TestEnumerableConverter<TCollection>(IValueConverter converter) where TCollection : IEnumerable {
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "0", "1", "2" }, (TCollection)converter.Convert(new int[] { 0, 1, 2 }, typeof(TCollection), null, null));
        }
        [Test]
        public void BooleanToVisibilityConverter() {
            var converter = new DevExpress.Mvvm.UI.BooleanToVisibilityConverter();

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
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null));
            Assert.AreEqual(new bool?(false), converter.ConvertBack(Visibility.Hidden, typeof(bool?), null, null));

            converter.Inverse = true;
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(false, typeof(Visibility), null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Visible, typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null));

            converter.Inverse = false;
            converter.HiddenInsteadOfCollapsed = true;
            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Visible, typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, null));
            Assert.AreEqual(Visibility.Hidden, converter.Convert(false, typeof(Visibility), null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null));
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
        public void ObjectToObjectConverterTest() {
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
        public void ObjectToObjectColorBrushConvertion() {
            var converter = new ObjectToObjectConverter();
            converter.Map.Add(new MapItem { Source = "123", Target = "#ff0000" });
            Assert.AreEqual("#ff0000", converter.Convert("123", typeof(string), null, null));

            var color = (Color)converter.Convert("123", typeof(Color), null, null);
            Assert.AreEqual(0xff, color.R);
            Assert.AreEqual(0x00, color.G);
            Assert.AreEqual(0x00, color.B);

            color = ((SolidColorBrush)converter.Convert("123", typeof(SolidColorBrush), null, null)).Color;
            Assert.AreEqual(0xff, color.R);
            Assert.AreEqual(0x00, color.G);
            Assert.AreEqual(0x00, color.B);

            color = ((SolidColorBrush)converter.Convert("123", typeof(Brush), null, null)).Color;
            Assert.AreEqual(0xff, color.R);
            Assert.AreEqual(0x00, color.G);
            Assert.AreEqual(0x00, color.B);

            converter.Map.Add(new MapItem { Source = "abc", Target = "ff0000" });
            Assert.AreEqual("ff0000", (string)converter.Convert("abc", typeof(Brush), null, null));

            converter.Map.Add(new MapItem { Source = "xyz", Target = "#ff0000ff" });
            color = ((SolidColorBrush)converter.Convert("xyz", typeof(Brush), null, null)).Color;
            Assert.AreEqual(0xff, color.A);
            Assert.AreEqual(0x00, color.R);
            Assert.AreEqual(0x00, color.G);
            Assert.AreEqual(0xff, color.B);

            converter.Map.Add(new MapItem { Source = "1", Target = "Red" });
            color = ((SolidColorBrush)converter.Convert("1", typeof(Brush), null, null)).Color;
            Assert.AreEqual(Colors.Red, color);

            converter.Map.Add(new MapItem { Source = "2", Target = "Green" });
            color = ((SolidColorBrush)converter.Convert("2", typeof(Brush), null, null)).Color;
            Assert.AreEqual(Colors.Green, color);
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
            Assert.AreEqual(new GridLengthConverter().ConvertFrom(0.1), ObjectToObjectConverter.Coerce(0.1, typeof(GridLength), true));
            Assert.AreEqual(new GridLengthConverter().ConvertFrom("0.1"), ObjectToObjectConverter.Coerce(0.1, typeof(GridLength), true));
        }
        [Test]
        public void BooleanSupport() {
            var converter = new ObjectToObjectConverter();
            converter.Map.Add(new MapItem { Source = 1, Target = "True" });
            converter.Map.Add(new MapItem { Source = 2, Target = "False" });
            Assert.AreEqual(true, converter.Convert(1, typeof(bool), null, null));
            Assert.AreEqual(false, converter.Convert(2, typeof(bool), null, null));
        }
        [Test]
        public void NullableSupport() {
            var converter = new ObjectToObjectConverter();
            converter.Map.Add(new MapItem { Source = 1, Target = "True" });
            converter.Map.Add(new MapItem { Source = 2, Target = "False" });
            converter.Map.Add(new MapItem { Source = 3, Target = null });
            converter.Map.Add(new MapItem { Source = 4, Target = 10 });
            Assert.AreEqual((bool?)true, converter.Convert(1, typeof(bool?), null, null));
            Assert.AreEqual((bool?)false, converter.Convert(2, typeof(bool?), null, null));
            Assert.AreEqual(null, converter.Convert(3, typeof(bool?), null, null));
            Assert.AreEqual((int?)10, converter.Convert(4, typeof(int?), null, null));
            Assert.AreEqual(null, converter.Convert(3, typeof(int?), null, null));
        }
        class T491851_Convertible : T491851_Convertible_Base { }
        class T491851_Convertible_Base : IConvertible {
            public int toTypeCalled = 0;
            public TypeCode GetTypeCode() {
                throw new NotImplementedException();
            }

            public bool ToBoolean(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public byte ToByte(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public char ToChar(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public DateTime ToDateTime(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public decimal ToDecimal(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public double ToDouble(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public short ToInt16(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public int ToInt32(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public long ToInt64(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public sbyte ToSByte(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public float ToSingle(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public string ToString(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public object ToType(Type conversionType, IFormatProvider provider) {
                toTypeCalled++;
                throw new NotImplementedException();
            }

            public ushort ToUInt16(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public uint ToUInt32(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public ulong ToUInt64(IFormatProvider provider) {
                throw new NotImplementedException();
            }
        }
        [Test]
        public void DontConvertAlreadyCorrectTypes() {
            var converter = new ObjectToObjectConverter();
            var target = new T491851_Convertible();
            converter.Map.Add(new MapItem { Source = 1, Target = target });
            Assert.AreEqual(target, converter.Convert(1, typeof(T491851_Convertible_Base), null, null));
            Assert.AreEqual(0, target.toTypeCalled);
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
        [Test]
        public void FormatStringConverterOutStringCase() {
            FormatStringConverter converter = new FormatStringConverter() { FormatString = "MMMM" };
            string s1 = (string)converter.Convert(DateTime.MinValue, typeof(string), null, null);
            converter.OutStringCaseFormat = UI.FormatStringConverter.TextCaseFormat.Lower;
            string s2 = (string)converter.Convert(DateTime.MinValue, typeof(string), null, null);
            converter.OutStringCaseFormat = UI.FormatStringConverter.TextCaseFormat.Upper;
            string s3 = (string)converter.Convert(DateTime.MinValue, typeof(string), null, null);

            Assert.AreNotEqual(s1, s2);
            Assert.AreNotEqual(s1, s3);
            Assert.AreEqual(s1.ToLower(), s2);
            Assert.AreEqual(s1.ToUpper(), s3);
        }
        [Test]
        public void FormatStringConverterSplitPascalCase() {
            FormatStringConverter converter = new FormatStringConverter() { SplitPascalCase = true };
            Assert.AreEqual("This Is Test", (string)converter.Convert("ThisIsTest", typeof(string), null, null));
            Assert.AreEqual("This Is Test", (string)converter.Convert("This Is Test", typeof(string), null, null));
            Assert.AreEqual("This  Is  Test", (string)converter.Convert("This  Is  Test", typeof(string), null, null));
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

        [Test]
        public void T711615() {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var currentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            try {
                System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("de-DE");
                System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
                Assert.AreEqual(0.0, ObjectToObjectConverter.Coerce("0.0", typeof(double), true));
            } finally {
                System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = currentUICulture;
            }
        }


        [Test]
        public void DelegateConverterTestBase() {
            var c1 = DelegateConverterFactory.CreateValueConverter<string, string>(x => x + "1");
            Assert.AreEqual("11", c1.Convert("1", null, null, null));
            Assert.AreEqual("1", c1.Convert(null, null, null, null));
            Assert.AreEqual(Binding.DoNothing, c1.Convert(DependencyProperty.UnsetValue, null, null, null));
            Assert.Throws<InvalidCastException>(() => {
                c1.Convert(new object(), null, null, null);
            });
            Assert.Throws<InvalidOperationException>(() => {
                c1.ConvertBack(null, null, null, null);
            });

            var c2 = DelegateConverterFactory.CreateValueConverter<int, int>(x => x + 1);
            Assert.AreEqual(2, c2.Convert(1, null, null, null));
            Assert.AreEqual(Binding.DoNothing, c2.Convert(DependencyProperty.UnsetValue, null, null, null));
            Assert.Throws<NullReferenceException>(() => {
                c2.Convert(null, null, null, null);
            });
            Assert.Throws<InvalidCastException>(() => {
                c2.Convert(new object(), null, null, null);
            });
            Assert.Throws<InvalidOperationException>(() => {
                c2.ConvertBack(null, null, null, null);
            });

            var c3 = DelegateConverterFactory.CreateValueConverter<string, string>(x => x + "1", x => x + "2");
            Assert.AreEqual("12", c3.ConvertBack("1", null, null, null));
            Assert.AreEqual("2", c3.ConvertBack(null, null, null, null));
            Assert.Throws<InvalidCastException>(() => {
                c3.ConvertBack(new object(), null, null, null);
            });
            Assert.Throws<InvalidCastException>(() => {
                c3.ConvertBack(DependencyProperty.UnsetValue, null, null, null);
            });

            var c4 = DelegateConverterFactory.CreateValueConverter(x => x.ToString() + "1", x => x.ToString() + "2" );
            Assert.AreEqual("12", c3.ConvertBack("1", null, null, null));
            Assert.AreEqual("2", c3.ConvertBack(null, null, null, null));
            Assert.Throws<InvalidCastException>(() => {
                c3.ConvertBack(new object(), null, null, null);
            });
            Assert.Throws<InvalidCastException>(() => {
                c3.ConvertBack(DependencyProperty.UnsetValue, null, null, null);
            });

            int assertCount = 0;
            Action<object, Type, object, CultureInfo> assert = (x, t, p, c) => {
                Assert.AreEqual("1", x);
                Assert.AreEqual(typeof(string), t);
                Assert.AreEqual(1, p);
                Assert.AreEqual(CultureInfo.InvariantCulture, c);
                assertCount++;
            };
            var c5 = DelegateConverterFactory.CreateValueConverter((x, t, p, c) => { assert(x, t, p, c); return "1"; }, (x, t, p, c) => { assert(x, t, p, c); return "1"; });
            Assert.AreEqual("1", c5.Convert("1", typeof(string), 1, CultureInfo.InvariantCulture));
            Assert.AreEqual(1, assertCount);
            Assert.AreEqual("1", c5.ConvertBack("1", typeof(string), 1, CultureInfo.InvariantCulture));
            Assert.AreEqual(2, assertCount);

            var c6 = DelegateConverterFactory.CreateValueConverter<string, string>((x, p, c) => { assert(x, typeof(string), p, c); return "1"; }, (x, p, c) => { assert(x, typeof(string), p, c); return "1"; });
            Assert.AreEqual("1", c6.Convert("1", typeof(string), 1, CultureInfo.InvariantCulture));
            Assert.AreEqual(3, assertCount);
            Assert.AreEqual("1", c6.ConvertBack("1", typeof(string), 1, CultureInfo.InvariantCulture));
            Assert.AreEqual(4, assertCount);


            var c7 = DelegateConverterFactory.CreateValueConverter(x => x);
            var c8 = DelegateConverterFactory.CreateValueConverter((x, t, p, c) => x);
            Assert.AreEqual(DependencyProperty.UnsetValue, c7.Convert(DependencyProperty.UnsetValue, null, null, null));
            Assert.AreEqual(DependencyProperty.UnsetValue, c8.Convert(DependencyProperty.UnsetValue, null, null, null));

            var c9 = DelegateConverterFactory.CreateValueConverter<int, int>(x => x);
            var c10 = DelegateConverterFactory.CreateValueConverter<int, int>((x, p, c) => x);
            Assert.AreEqual(Binding.DoNothing, c9.Convert(DependencyProperty.UnsetValue, null, null, null));
            Assert.AreEqual(Binding.DoNothing, c10.Convert(DependencyProperty.UnsetValue, null, null, null));
        }
        [Test]
        public void DelegateMultiConverterTestBase() {
            int assertCount = 0;
            Action<object[], Type, object, CultureInfo> assert1 = (x, t, p, c) => {
                Assert.AreEqual("1", x[0]);
                Assert.AreEqual(typeof(string), t);
                Assert.AreEqual(1, p);
                Assert.AreEqual(CultureInfo.InvariantCulture, c);
                assertCount++;
            };
            Action<object, Type[], object, CultureInfo> assertBack1 = (x, t, p, c) => {
                Assert.AreEqual("1", x);
                Assert.AreEqual(typeof(string), t[0]);
                Assert.AreEqual(typeof(int), t[1]);
                Assert.AreEqual(1, p);
                Assert.AreEqual(CultureInfo.InvariantCulture, c);
                assertCount++;
            };
            Action<object> assertBack = (x) => {
                Assert.AreEqual("1", x);
                assertCount++;
            };
            Action<string, int> assert2 = (x1, x2) => {
                Assert.AreEqual("1", x1);
                Assert.AreEqual(2, x2);
                assertCount++;
            };
            Action<string, int, double> assert3 = (x1, x2, x3) => {
                Assert.AreEqual("1", x1);
                Assert.AreEqual(2, x2);
                Assert.AreEqual(3.1, x3);
                assertCount++;
            };
            Action<string, int, double, float> assert4 = (x1, x2, x3, x4) => {
                Assert.AreEqual("1", x1);
                Assert.AreEqual(2, x2);
                Assert.AreEqual(3.1, x3);
                Assert.AreEqual(4f, x4);
                assertCount++;
            };

            object xx5 = new object();
            Action<string, int, double, float, object> assert5 = (x1, x2, x3, x4, x5) => {
                Assert.AreEqual("1", x1);
                Assert.AreEqual(2, x2);
                Assert.AreEqual(3.1, x3);
                Assert.AreEqual(4f, x4);
                Assert.AreEqual(xx5, x5);
                assertCount++;
            };

            object xx6 = new object();
            Action<string, int, double, float, object, object> assert6 = (x1, x2, x3, x4, x5, x6) => {
                Assert.AreEqual("1", x1);
                Assert.AreEqual(2, x2);
                Assert.AreEqual(3.1, x3);
                Assert.AreEqual(4f, x4);
                Assert.AreEqual(xx5, x5);
                Assert.AreEqual(xx6, x6);
                assertCount++;
            };

            var c1 = DelegateConverterFactory.CreateMultiValueConverter((x, t, p, c) => { assert1(x, t, p, c); return "1"; }, (x, t, p, c) => { assertBack1(x, t, p, c); return new object[] { "1", 2 }; });
            Assert.AreEqual("1", c1.Convert(new object[] { "1", 2 }, typeof(string), 1, CultureInfo.InvariantCulture));
            Assert.AreEqual(1, assertCount);
            Assert.AreEqual(new object[] { "1", 2 }, c1.ConvertBack("1", new[] { typeof(string), typeof(int) }, 1, CultureInfo.InvariantCulture));
            Assert.AreEqual(2, assertCount);
            c1 = null;

            var c2 = DelegateConverterFactory.CreateMultiValueConverter<string, int, string>((x1, x2) => { assert2(x1, x2); return "1"; }, (x) => { assertBack(x); return Tuple.Create("1", 2); });
            Assert.AreEqual("1", c2.Convert(new object[] { "1", 2 }, null, null, null));
            Assert.AreEqual(3, assertCount);
            Assert.AreEqual(new object[] { "1", 2 }, c2.ConvertBack("1", null, null, null));
            Assert.AreEqual(4, assertCount);
            c2 = null;

            var c3 = DelegateConverterFactory.CreateMultiValueConverter<string, int, double, string>((x1, x2, x3) => { assert3(x1, x2, x3); return "1"; }, (x) => { assertBack(x); return Tuple.Create("1", 2, 3.1); });
            Assert.AreEqual("1", c3.Convert(new object[] { "1", 2, 3.1 }, null, null, null));
            Assert.AreEqual(5, assertCount);
            Assert.AreEqual(new object[] { "1", 2, 3.1 }, c3.ConvertBack("1", null, null, null));
            Assert.AreEqual(6, assertCount);
            c3 = null;

            var c4 = DelegateConverterFactory.CreateMultiValueConverter<string, int, double, float, string>((x1, x2, x3, x4) => { assert4(x1, x2, x3, x4); return "1"; }, (x) => { assertBack(x); return Tuple.Create("1", 2, 3.1, 4f); });
            Assert.AreEqual("1", c4.Convert(new object[] { "1", 2, 3.1, 4f }, null, null, null));
            Assert.AreEqual(7, assertCount);
            Assert.AreEqual(new object[] { "1", 2, 3.1, 4f }, c4.ConvertBack("1", null, null, null));
            Assert.AreEqual(8, assertCount);
            c4 = null;

            var c5 = DelegateConverterFactory.CreateMultiValueConverter<string, int, double, float, object, string>((x1, x2, x3, x4, x5) => { assert5(x1, x2, x3, x4, x5); return "1"; }, (x) => { assertBack(x); return Tuple.Create("1", 2, 3.1, 4f, xx5); });
            Assert.AreEqual("1", c5.Convert(new object[] { "1", 2, 3.1, 4f, xx5 }, null, null, null));
            Assert.AreEqual(9, assertCount);
            Assert.AreEqual(new object[] { "1", 2, 3.1, 4f, xx5 }, c5.ConvertBack("1", null, null, null));
            Assert.AreEqual(10, assertCount);
            c5 = null;

            var c6 = DelegateConverterFactory.CreateMultiValueConverter<string, int, double, float, object, object, string>((x1, x2, x3, x4, x5, x6) => { assert6(x1, x2, x3, x4, x5, x6); return "1"; }, (x) => { assertBack(x); return Tuple.Create("1", 2, 3.1, 4f, xx5, xx6); });
            Assert.AreEqual("1", c6.Convert(new object[] { "1", 2, 3.1, 4f, xx5, xx6 }, null, null, null));
            Assert.AreEqual(11, assertCount);
            Assert.AreEqual(new object[] { "1", 2, 3.1, 4f, xx5, xx6 }, c6.ConvertBack("1", null, null, null));
            Assert.AreEqual(12, assertCount);
            Assert.Throws<System.Reflection.TargetParameterCountException>(() => {
                c6.Convert(new object[] { "1", 2, 3.1, 4f, xx5 }, null, null, null);
            });
            c6 = null;

            var c7 = DelegateConverterFactory.CreateMultiValueConverter(x => x[0]);
            var c8 = DelegateConverterFactory.CreateMultiValueConverter((x, t, p, c) => x[0]);
            Assert.AreEqual(DependencyProperty.UnsetValue, c7.Convert(new object[] { DependencyProperty.UnsetValue }, null, null, null));
            Assert.AreEqual(DependencyProperty.UnsetValue, c8.Convert(new object[] { DependencyProperty.UnsetValue }, null, null, null));

            var c9 = DelegateConverterFactory.CreateMultiValueConverter<int, int, int>((x1, x2) => x1);
            Assert.AreEqual(Binding.DoNothing, c9.Convert(new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue }, null, null, null));
        }
        [Test]
        public void DelegateConverterTest() {
            int count = 0;
            var c = DelegateConverterFactory.CreateValueConverter<string, string>(x => { count++; return x + "1"; });

            TextBlock control = new TextBlock() { Text = "a" };
            Assert.AreEqual(0, count);
            Assert.AreEqual("a", control.Text);

            var b = new Binding("Prop") { Converter = c, Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(control, TextBlock.TextProperty, b);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, control.Text);

            control.DataContext = "1";
            DispatcherHelper.DoEvents();
            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, control.Text);

            var vm = ViewModelSource.Create<VM>();
            control.DataContext = vm;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, count);
            Assert.AreEqual("1", control.Text);

            vm.Prop = "a";
            DispatcherHelper.DoEvents();
            Assert.AreEqual(2, count);
            Assert.AreEqual("a1", control.Text);

            control.DataContext = null;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(2, count);
            Assert.AreEqual(string.Empty, control.Text);
        }
        [Test]
        public void DelegateConverterTest_SubProperty() {
            int count = 0;
            var c = DelegateConverterFactory.CreateValueConverter<string, string>(x => { count++; return x + "1"; });

            TextBlock control = new TextBlock() { Text = "a" };
            Assert.AreEqual(0, count);
            Assert.AreEqual("a", control.Text);

            var b = new Binding("SubVM.Prop") { Converter = c, Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(control, TextBlock.TextProperty, b);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, control.Text);

            var vm = ViewModelSource.Create<VM>();
            control.DataContext = vm;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, control.Text);

            vm.SubVM = ViewModelSource.Create<VM>();
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, count);
            Assert.AreEqual("1", control.Text);

            vm.SubVM.Prop = "a";
            DispatcherHelper.DoEvents();
            Assert.AreEqual(2, count);
            Assert.AreEqual("a1", control.Text);

            control.DataContext = null;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(2, count);
            Assert.AreEqual(string.Empty, control.Text);
        }
        [Test]
        public void DelegateConverterTest_NotNull() {
            int count = 0;
            var c = DelegateConverterFactory.CreateValueConverter<int, string>(x => { count++; return x.ToString() + "1"; });

            TextBlock control = new TextBlock() { Text = "a" };
            Assert.AreEqual(0, count);
            Assert.AreEqual("a", control.Text);

            var b = new Binding("NullableInt") { Converter = c, Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(control, TextBlock.TextProperty, b);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, control.Text);

            var vm = ViewModelSource.Create<VM>();
            Assert.Throws<NullReferenceException>(() => {
                control.DataContext = vm;
                DispatcherHelper.DoEvents();
            });
            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, control.Text);

            vm.NullableInt = 1;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, count);
            Assert.AreEqual("11", control.Text);

            control.DataContext = null;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, count);
            Assert.AreEqual(string.Empty, control.Text);
        }
        [Test]
        public void DelegateConverterTest_SubProperty_NotNull() {
            int count = 0;
            var c = DelegateConverterFactory.CreateValueConverter<int, string>(x => { count++; return x.ToString() + "1"; });

            TextBlock control = new TextBlock() { Text = "a" };
            Assert.AreEqual(0, count);
            Assert.AreEqual("a", control.Text);

            var b = new Binding("SubVM.NullableInt") { Converter = c, Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(control, TextBlock.TextProperty, b);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, control.Text);

            var vm = ViewModelSource.Create<VM>();
            control.DataContext = vm;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, control.Text);

            Assert.Throws<NullReferenceException>(() => {
                vm.SubVM = ViewModelSource.Create<VM>();
                DispatcherHelper.DoEvents();
            });
            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, control.Text);

            vm.SubVM.NullableInt = 1;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, count);
            Assert.AreEqual("11", control.Text);

            control.DataContext = null;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, count);
            Assert.AreEqual(string.Empty, control.Text);
        }
        [Test]
        public void DelegateConverterTest_ConvertBack() {
            int count = 0;
            int backCount = 0;
            var c = DelegateConverterFactory.CreateValueConverter<string, string>(
                x => { count++; return x + "1"; },
                x => { backCount++; return "2"; });

            TextBlock control = new TextBlock() { Text = "a" };
            Assert.AreEqual(0, count);
            Assert.AreEqual("a", control.Text);

            var b = new Binding("Prop") { Converter = c, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            BindingOperations.SetBinding(control, TextBlock.TextProperty, b);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(0, count);
            Assert.AreEqual(0, backCount);
            Assert.AreEqual(string.Empty, control.Text);
            control.Text = "3";
            Assert.AreEqual(0, count);
            Assert.AreEqual(0, backCount);
            Assert.AreEqual("3", control.Text);

            var vm = ViewModelSource.Create<VM>();
            control.DataContext = vm;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, count);
            Assert.AreEqual(0, backCount);
            Assert.AreEqual("1", control.Text);

            control.Text = "3";
            Assert.AreEqual(2, count);
            Assert.AreEqual(1, backCount);
            Assert.AreEqual("21", control.Text);
            Assert.AreEqual("2", vm.Prop);

            vm.Prop = "a";
            DispatcherHelper.DoEvents();
            Assert.AreEqual(3, count);
            Assert.AreEqual(1, backCount);
            Assert.AreEqual("a1", control.Text);
            Assert.AreEqual("a", vm.Prop);

            control.DataContext = null;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(3, count);
            Assert.AreEqual(1, backCount);
            Assert.AreEqual(string.Empty, control.Text);
        }

        public class VM {
            public virtual string Prop { get; set; }
            public virtual int? NullableInt { get; set; }
            public virtual VM SubVM { get; set; }
        }
    }
    public class ToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
    public class FromString {
        public FromString(string s) {
            S = s;
        }
        public string S { get; set; }
        public override string ToString() {
            return S;
        }
    }
    public static class ReflectionConverterTestMethods {
        public static string IntToString(int i) { return "S" + i.ToString(); }
        public static int StringToInt(string s) { return int.Parse(s.Substring(1)); }
    }
    public static class ReflectionConvertTestMockStatic {
        public delegate string Convert01Func(object value);
        public delegate string Convert02Func(int value);
        public delegate string Convert03Func(int value, int parameter);
        public delegate string Convert04Func(int value, object parameter);
        public delegate string Convert05Func(object value, int parameter);
        public delegate string Convert06Func(object value, Type targetType, int parameter);
        public delegate string Convert07Func(object value, Type targetType, object parameter);
        public delegate string Convert08Func(int value, Type targetType, object parameter);
        public delegate string Convert09Func(object value, CultureInfo culture);
        public delegate string Convert10Func(int value, CultureInfo culture);
        public delegate string Convert11Func(int value, int parameter, CultureInfo culture);
        public delegate string Convert12Func(int value, object parameter, CultureInfo culture);
        public delegate string Convert13Func(object value, int parameter, CultureInfo culture);
        public delegate string Convert14Func(object value, Type targetType, int parameter, CultureInfo culture);
        public delegate string Convert15Func(object value, Type targetType, object parameter, CultureInfo culture);
        public delegate string Convert16Func(int value, Type targetType, object parameter, CultureInfo culture);
        public delegate string Convert17Func(object value, Type targetType);
        public delegate string Convert18Func(int value, Type targetType);

        public static Convert01Func Convert01Function;
        public static Convert02Func Convert02Function;
        public static Convert03Func Convert03Function;
        public static Convert04Func Convert04Function;
        public static Convert05Func Convert05Function;
        public static Convert06Func Convert06Function;
        public static Convert07Func Convert07Function;
        public static Convert08Func Convert08Function;
        public static Convert09Func Convert09Function;
        public static Convert10Func Convert10Function;
        public static Convert11Func Convert11Function;
        public static Convert12Func Convert12Function;
        public static Convert13Func Convert13Function;
        public static Convert14Func Convert14Function;
        public static Convert15Func Convert15Function;
        public static Convert16Func Convert16Function;
        public static Convert17Func Convert17Function;
        public static Convert18Func Convert18Function;

        public static string Convert01(object value) { return Convert01Function(value); }
        public static string Convert02(int value) { return Convert02Function(value); }
        public static string Convert03(int value, int parameter) { return Convert03Function(value, parameter); }
        public static string Convert04(int value, object parameter) { return Convert04Function(value, parameter); }
        public static string Convert05(object value, int parameter) { return Convert05Function(value, parameter); }
        public static string Convert06(object value, Type targetType, int parameter) { return Convert06Function(value, targetType, parameter); }
        public static string Convert07(object value, Type targetType, object parameter) { return Convert07Function(value, targetType, parameter); }
        public static string Convert08(int value, Type targetType, object parameter) { return Convert08Function(value, targetType, parameter); }
        public static string Convert09(object value, CultureInfo culture) { return Convert09Function(value, culture); }
        public static string Convert10(int value, CultureInfo culture) { return Convert10Function(value, culture); }
        public static string Convert11(int value, int parameter, CultureInfo culture) { return Convert11Function(value, parameter, culture); }
        public static string Convert12(int value, object parameter, CultureInfo culture) { return Convert12Function(value, parameter, culture); }
        public static string Convert13(object value, int parameter, CultureInfo culture) { return Convert13Function(value, parameter, culture); }
        public static string Convert14(object value, Type targetType, int parameter, CultureInfo culture) { return Convert14Function(value, targetType, parameter, culture); }
        public static string Convert15(object value, Type targetType, object parameter, CultureInfo culture) { return Convert15Function(value, targetType, parameter, culture); }
        public static string Convert16(int value, Type targetType, object parameter, CultureInfo culture) { return Convert16Function(value, targetType, parameter, culture); }
        public static string Convert17(object value, Type targetType) { return Convert17Function(value, targetType); }
        public static string Convert18(int value, Type targetType) { return Convert18Function(value, targetType); }
    }
}