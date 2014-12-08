#if !SILVERLIGHT && !NETFX_CORE
using NUnit.Framework;
#endif
using System;
using System.Windows;
using DevExpress.Mvvm.UI;
#if NETFX_CORE
using DevExpress.TestFramework.NUnit;
using Windows.UI.Xaml;
using System.Globalization;
using Windows.UI.Xaml.Data;
#else
using System.Globalization;
using System.Windows.Data;
#endif

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class ConvertersTest {
#if !NETFX_CORE && !FREE
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
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert01(22)).Returns((int v) => ReflectionConverter_Result(v, null)), "Convert01", false);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert02(22)).Returns((object v) => ReflectionConverter_Result(v, null)), "Convert02", false);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert03(22, 14)).Returns((int v, int p) => ReflectionConverter_Result(v, p)), "Convert03", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert04(22, 14)).Returns((int v, object p) => ReflectionConverter_Result(v, p)), "Convert04", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert05(22, 14)).Returns((object v, object p) => ReflectionConverter_Result(v, p)), "Convert05", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert06(22, typeof(string), 14)).Returns((object v, Type t, int p) => ReflectionConverter_Result(v, p)), "Convert06", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert07(22, typeof(string), 14)).Returns((object v, Type t, object p) => ReflectionConverter_Result(v, p)), "Convert07", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert08(22, typeof(string), 14)).Returns((int v, Type t, object p) => ReflectionConverter_Result(v, p)), "Convert08", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert17(22, typeof(string))).Returns((object v, Type t) => ReflectionConverter_Result(v, null)), "Convert17", false);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert18(22, typeof(string))).Returns((int v, Type t) => ReflectionConverter_Result(v, null)), "Convert18", false);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert09(22, CultureInfo.InvariantCulture)).Returns((object v, CultureInfo c) => ReflectionConverter_Result(v, null)), "Convert09", false);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert10(22, CultureInfo.InvariantCulture)).Returns((int v, CultureInfo c) => ReflectionConverter_Result(v, null)), "Convert10", false);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert11(22, 14, CultureInfo.InvariantCulture)).Returns((int v, int p, CultureInfo c) => ReflectionConverter_Result(v, p)), "Convert11", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert12(22, 14, CultureInfo.InvariantCulture)).Returns((int v, object p, CultureInfo c) => ReflectionConverter_Result(v, p)), "Convert12", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert13(22, 14, CultureInfo.InvariantCulture)).Returns((object v, int p, CultureInfo c) => ReflectionConverter_Result(v, p)), "Convert13", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert14(22, typeof(string), 14, CultureInfo.InvariantCulture)).Returns((object v, Type t, int p, CultureInfo c) => ReflectionConverter_Result(v, p)), "Convert14", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert15(22, typeof(string), 14, CultureInfo.InvariantCulture)).Returns((object v, Type t, object p, CultureInfo c) => ReflectionConverter_Result(v, p)), "Convert15", true);
            ReflectionConverter_ConvertMethodParametersTest(m => m.Setup(c => c.Convert16(22, typeof(string), 14, CultureInfo.InvariantCulture)).Returns((int v, Type t, object p, CultureInfo c) => ReflectionConverter_Result(v, p)), "Convert16", true);
        }
        void ReflectionConverter_ConvertMethodParametersTest(Action<Mock<IReflectionConverterTestMock>> setup, string method, bool withParameter) {
            var mock = new Mock<IReflectionConverterTestMock>(MockBehavior.Strict);
            ReflectionConvertTestMockStatic.Mock = mock.Object;
            setup(mock);
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
        [Test, ExpectedException(typeof(NotSupportedCollectionException))]
        public void EnumerableConverter_TryConvertToInvalidCollectionTest() {
            var converter = new EnumerableConverter() { TargetItemType = typeof(string), ItemConverter = new ToStringConverter() };
            converter.Convert(new int[] { 0, 1, 2 }, Enumerable.Empty<string>().GetType(), null, null);
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
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EnumerableConverter_DetectTargetItemTypeFailTest() {
            var converter = new EnumerableConverter() { ItemConverter = new ToStringConverter() };
            converter.Convert(new int[] { 0, 1, 2 }, typeof(StringCollection), null, null);
        }
        void TestEnumerableConverter<TCollection>(IValueConverter converter) where TCollection : IEnumerable {
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "0", "1", "2" }, (TCollection)converter.Convert(new int[] { 0, 1, 2 }, typeof(TCollection), null, null));
        }
        [Test]
        public void CriteriaOperatorConverter_ToUpperCaseTest() {
            var converter = new CriteriaOperatorConverter() { Expression = "Upper(This)" };
            Assert.AreEqual("ABCD", converter.Convert("abcd", null, null, null));
        }
#endif
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
#if !SILVERLIGHT && !NETFX_CORE
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null));
            Assert.AreEqual(new bool?(false), converter.ConvertBack(Visibility.Hidden, typeof(bool?), null, null));
#endif

            converter.Inverse = true;
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(false, typeof(Visibility), null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Visible, typeof(bool), null, null));
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, null));
#if !SILVERLIGHT && !NETFX_CORE
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null));
#endif

            converter.Inverse = false;
            converter.HiddenInsteadOfCollapsed = true;
            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(true, converter.ConvertBack(Visibility.Visible, typeof(bool), null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, null));
#if !SILVERLIGHT && !NETFX_CORE
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
#if NETFX_CORE
        [Test]
        public void FormatStringConverter() {
            var savedCulture = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
            try {
                FormatStringConverter converter = new FormatStringConverter();
                converter.FormatString = "C0";
                System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol = "#test currency symbol#";
                string s = (string)converter.Convert(13, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture.NativeName);
                Assert.IsTrue(s.Contains("#test currency symbol#"));
            } finally {
                System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol = savedCulture;
            }
        }
#else
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
#endif
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
    public class ToStringConverter : IValueConverter {
#if !NETFX_CORE
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
#else
        public object Convert(object value, Type targetType, object parameter, string language) {
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
#endif
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
    public interface IReflectionConverterTestMock {
        string Convert01(object value);
        string Convert02(int value);
        string Convert03(int value, int parameter);
        string Convert04(int value, object parameter);
        string Convert05(object value, int parameter);
        string Convert06(object value, Type targetType, int parameter);
        string Convert07(object value, Type targetType, object parameter);
        string Convert08(int value, Type targetType, object parameter);
        string Convert09(object value, CultureInfo culture);
        string Convert10(int value, CultureInfo culture);
        string Convert11(int value, int parameter, CultureInfo culture);
        string Convert12(int value, object parameter, CultureInfo culture);
        string Convert13(object value, int parameter, CultureInfo culture);
        string Convert14(object value, Type targetType, int parameter, CultureInfo culture);
        string Convert15(object value, Type targetType, object parameter, CultureInfo culture);
        string Convert16(int value, Type targetType, object parameter, CultureInfo culture);
        string Convert17(object value, Type targetType);
        string Convert18(int value, Type targetType);
    }
    public static class ReflectionConvertTestMockStatic {
        public static IReflectionConverterTestMock Mock { get; set; }
        public static string Convert01(object value) { return Mock.Convert01(value); }
        public static string Convert02(int value) { return Mock.Convert02(value); }
        public static string Convert03(int value, int parameter) { return Mock.Convert03(value, parameter); }
        public static string Convert04(int value, object parameter) { return Mock.Convert04(value, parameter); }
        public static string Convert05(object value, int parameter) { return Mock.Convert05(value, parameter); }
        public static string Convert06(object value, Type targetType, int parameter) { return Mock.Convert06(value, targetType, parameter); }
        public static string Convert07(object value, Type targetType, object parameter) { return Mock.Convert07(value, targetType, parameter); }
        public static string Convert08(int value, Type targetType, object parameter) { return Mock.Convert08(value, targetType, parameter); }
        public static string Convert09(object value, CultureInfo culture) { return Mock.Convert09(value, culture); }
        public static string Convert10(int value, CultureInfo culture) { return Mock.Convert10(value, culture); }
        public static string Convert11(int value, int parameter, CultureInfo culture) { return Mock.Convert11(value, parameter, culture); }
        public static string Convert12(int value, object parameter, CultureInfo culture) { return Mock.Convert12(value, parameter, culture); }
        public static string Convert13(object value, int parameter, CultureInfo culture) { return Mock.Convert13(value, parameter, culture); }
        public static string Convert14(object value, Type targetType, int parameter, CultureInfo culture) { return Mock.Convert14(value, targetType, parameter, culture); }
        public static string Convert15(object value, Type targetType, object parameter, CultureInfo culture) { return Mock.Convert15(value, targetType, parameter, culture); }
        public static string Convert16(int value, Type targetType, object parameter, CultureInfo culture) { return Mock.Convert16(value, targetType, parameter, culture); }
        public static string Convert17(object value, Type targetType) { return Mock.Convert17(value, targetType); }
        public static string Convert18(int value, Type targetType) { return Mock.Convert18(value, targetType); }
    }
}