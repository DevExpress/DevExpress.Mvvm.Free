using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.DataAnnotations;
using System.Windows.Media;
using System.Windows.Media.Imaging;


using NUnit.Framework;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class EnumHelperTests {
        [SetUp]
        public void SetUp() {
            UI.Tests.ApplicationJumpListServiceTestsImageSourceHelper.RegisterPackScheme();
        }

        #region simple
        public enum EnumSimple {
            MemberOne,
            MemberTwo,
        }
        class TestEnumValueConverter : IValueConverter {
            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                return value.ToString() + "_";
            }
            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                throw new NotImplementedException();
            }
        }
        [Test]
        public void IsEnumTypeTest() {
            var source = EnumSourceHelper.GetEnumSource(typeof(string));
            var source2 = EnumSourceHelper.GetEnumSource(null);
            source.AreEqual<IEnumerable<EnumMemberInfo>>(Enumerable.Empty<EnumMemberInfo>());
            source2.AreEqual<IEnumerable<EnumMemberInfo>>(Enumerable.Empty<EnumMemberInfo>());
        }
        [Test]
        public void SimpleEnumMetadataTest() {
            var source = EnumSourceHelper.GetEnumSource(typeof(EnumSimple));
            source.AreEqual(x => x.Count(), 2);
            source.ElementAt(0)
                .AreEqual(x => x.Id, 0)
                .AreEqual(x => x.Name, "MemberOne")
                .IsNull(x => x.Image);
            source.ElementAt(1)
                .AreEqual(x => x.Id, 1)
                .AreEqual(x => x.Name, "MemberTwo")
                .IsNull(x => x.Image);

            source = EnumSourceHelper.GetEnumSource(typeof(EnumSimple), false, null, true);
            source.AreEqual(x => x.Count(), 2);
            source.ElementAt(0)
                .AreEqual(x => x.Id, EnumSimple.MemberOne)
                .AreEqual(x => x.Name, "Member One")
                .IsNull(x => x.Image);
            source.ElementAt(1)
                .AreEqual(x => x.Id, EnumSimple.MemberTwo)
                .AreEqual(x => x.Name, "Member Two")
                .IsNull(x => x.Image);

            source = EnumSourceHelper.GetEnumSource(typeof(EnumSimple), false, new TestEnumValueConverter(), true);
            source.AreEqual(x => x.Count(), 2);
            source.ElementAt(0)
                .AreEqual(x => x.Id, EnumSimple.MemberOne)
                .AreEqual(x => x.Name, "MemberOne_")
                .IsNull(x => x.Image);
            source.ElementAt(1)
                .AreEqual(x => x.Id, EnumSimple.MemberTwo)
                .AreEqual(x => x.Name, "MemberTwo_")
                .IsNull(x => x.Image);
        }
        #endregion
        #region type converter
        [TypeConverter(typeof(EnumWithTypeConverterConverter))]
        public enum EnumWithTypeConverter{
            MemberOne,
            MemberTwo,
        }
        public class EnumWithTypeConverterConverter : TypeConverter {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
                return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
            }
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if(destinationType == typeof(string))
                    return value.ToString() + "#";
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
        [Test]
        public void EnumWithTypeConverterMetadataTest() {
            var source = EnumSourceHelper.GetEnumSource(typeof(EnumWithTypeConverter));
            source.AreEqual(x => x.Count(), 2);
            source.ElementAt(0)
                .AreEqual(x => x.Id, 0)
                .AreEqual(x => x.Name, "MemberOne#")
                .IsNull(x => x.Image);
            source.ElementAt(1)
                .AreEqual(x => x.Id, 1)
                .AreEqual(x => x.Name, "MemberTwo#")
                .IsNull(x => x.Image);

            source = EnumSourceHelper.GetEnumSource(typeof(EnumWithTypeConverter), false, null, true);
            source.AreEqual(x => x.Count(), 2);
            source.ElementAt(0)
                .AreEqual(x => x.Id, EnumWithTypeConverter.MemberOne)
                .AreEqual(x => x.Name, "Member One#")
                .IsNull(x => x.Image);
            source.ElementAt(1)
                .AreEqual(x => x.Id, EnumWithTypeConverter.MemberTwo)
                .AreEqual(x => x.Name, "Member Two#")
                .IsNull(x => x.Image);

            source = EnumSourceHelper.GetEnumSource(typeof(EnumWithTypeConverter), false, new TestEnumValueConverter(), true);
            source.AreEqual(x => x.Count(), 2);
            source.ElementAt(0)
                .AreEqual(x => x.Id, EnumWithTypeConverter.MemberOne)
                .AreEqual(x => x.Name, "MemberOne_")
                .IsNull(x => x.Image);
            source.ElementAt(1)
                .AreEqual(x => x.Id, EnumWithTypeConverter.MemberTwo)
                .AreEqual(x => x.Name, "MemberTwo_")
                .IsNull(x => x.Image);
        }
        #endregion
        #region display name description and image
        const string UriPrefix = "pack://application:,,,/DevExpress.Mvvm.Tests.Free;component/Icons/";
        const string SvgUriPrefix = "pack://application:,,,/DevExpress.Mvvm.Tests.Free;component/Images/";
        public enum EnumWithDisplayName {
            [Display(ShortName = "OneMember")]
            [Image(UriPrefix + "icon1.ico")]
            [DXImage(SmallImageUri = UriPrefix + "icon2.ico")]
            MemberOne,

            [Display(Name = "TwoMember", ShortName = "ignore", Description = "desc")]
            [DXImage(SmallImageUri = UriPrefix + "icon2.ico", LargeImageUri = UriPrefix + "icon3.ico")]
            MemberTwo,

            [Display(AutoGenerateField = false)]
            MemberThree,
        }
        [EnumMetadataType(typeof(EnumWithDisplayNameMetadata))]
        public enum EnumWithDisplayName_ExternalMetadata1 {
            MemberOne,
            MemberTwo,
            MemberThree,
        }
        [EnumMetadataType(typeof(EnumWithDisplayNameMetadata))]
        public enum EnumWithDisplayName_ExternalMetadata2 {
            MemberOne,
            MemberTwo,
            MemberThree,
        }
        public enum EnumWithSvgImage {
            [Image(UriPrefix + "icon1.ico")]
            MemberOne,

            [Image(SvgUriPrefix + "redoTestSvg.sVg")]
            MemberTwo,
            MemberThree,
        }
        public class EnumWithDisplayNameMetadata : IEnumMetadataProvider<EnumWithDisplayName_ExternalMetadata1> {
            public static void BuildMetadata(EnumMetadataBuilder<EnumWithDisplayName_ExternalMetadata2> builder) {
                builder
                    .Member(EnumWithDisplayName_ExternalMetadata2.MemberOne)
                        .DisplayName("OneMember")
                        .ImageUri(UriPrefix + "icon1.ico")
                    .EndMember()
                    .Member(EnumWithDisplayName_ExternalMetadata2.MemberTwo)
                        .DisplayName("TwoMember")
                        .ImageUri(UriPrefix + "icon2.ico")
                        .Description("desc")
                    .EndMember()
                    .Member(EnumWithDisplayName_ExternalMetadata2.MemberThree)
                        .NotAutoGenerated()
                    .EndMember();
            }
            void IEnumMetadataProvider<EnumWithDisplayName_ExternalMetadata1>.BuildMetadata(EnumMetadataBuilder<EnumWithDisplayName_ExternalMetadata1> builder) {
                builder
                    .Member(EnumWithDisplayName_ExternalMetadata1.MemberOne)
                        .DisplayName("OneMember")
                        .ImageUri(UriPrefix + "icon1.ico")
                    .EndMember()
                    .Member(EnumWithDisplayName_ExternalMetadata1.MemberTwo)
                        .DisplayName("TwoMember")
                        .ImageUri(UriPrefix + "icon2.ico")
                        .Description("desc")
                    .EndMember()
                    .Member(EnumWithDisplayName_ExternalMetadata1.MemberThree)
                        .NotAutoGenerated()
                    .EndMember();
            }
        }
        public enum EnumWithDisplayName_ExternalMetadata3 {
            MemberOne,
            MemberTwo,
            MemberThree,
        }
        public class EnumWithDisplayNameExternalMetadata  {
            public static void BuildMetadata(EnumMetadataBuilder<EnumWithDisplayName_ExternalMetadata3> builder) {
                builder
                    .Member(EnumWithDisplayName_ExternalMetadata3.MemberOne)
                        .DisplayName("OneMember")
                        .ImageUri(UriPrefix + "icon1.ico")
                    .EndMember()
                    .Member(EnumWithDisplayName_ExternalMetadata3.MemberTwo)
                        .DisplayName("TwoMember")
                        .ImageUri(UriPrefix + "icon2.ico")
                        .Description("desc")
                    .EndMember()
                    .Member(EnumWithDisplayName_ExternalMetadata3.MemberThree)
                        .NotAutoGenerated()
                    .EndMember();
            }
        }
        [Test]
        public void EnumWithDisplayNameTest() {
            AssertEnumWithDisplayName(typeof(EnumWithDisplayName));
        }
        [Test]
        public void EnumWithDisplayNameTest_ExternalMetadata1() {
            AssertEnumWithDisplayName(typeof(EnumWithDisplayName_ExternalMetadata1));
        }
        [Test]
        public void EnumWithDisplayNameTest_ExternalMetadata2() {
            AssertEnumWithDisplayName(typeof(EnumWithDisplayName_ExternalMetadata2));
        }
        [Test]
        public void EnumWithDisplayNameTest_MetadataLocator() {
            try {
                MetadataLocator.Default = MetadataLocator.Create()
                    .AddMetadata<EnumWithDisplayNameExternalMetadata>();
                AssertEnumWithDisplayName(typeof(EnumWithDisplayName_ExternalMetadata3));
            } finally {
                MetadataLocator.Default = null;
            }
        }
        [Test]
        public void SvgImageTest() {
            Func<string, ImageSource> getSvgImageSourceForTest = uri => (ImageSource)new ImageSourceConverter().ConvertFrom(UriPrefix + "icon2.ico");
            var source = EnumSourceHelperCore.GetEnumSource(typeof(EnumWithSvgImage), true, null, false, EnumMembersSortMode.Default,
                (x, y) => { throw new InvalidOperationException(); }, getSvgImageSource: getSvgImageSourceForTest);
            Assert.AreEqual(GetImageUri(source.ElementAt(0).Image).ToString(), UriPrefix + "icon1.ico");
            Assert.AreEqual(GetImageUri(source.ElementAt(1).Image).ToString(), UriPrefix + "icon2.ico");
        }
        [Test]
        public void SvgImageExceptionTest() {
            var source = EnumSourceHelperCore.GetEnumSource(typeof(EnumWithSvgImage), true, null, false, EnumMembersSortMode.Default,
                (x, y) => { throw new InvalidOperationException(); });
            Assert.AreEqual(GetImageUri(source.ElementAt(0).Image).ToString(), UriPrefix + "icon1.ico");
            Assert.Throws(typeof(ArgumentException), () => { var imageSource = source.ElementAt(1).Image; });
        }

        [Test, NUnit.Framework.Description("T521914")]
        public void LazyInitTest() {
            var source = EnumSourceHelperCore.GetEnumSource(typeof(EnumWithDisplayName), true, null, false, EnumMembersSortMode.Default,
                (x, y) => { throw new InvalidOperationException(); });
            Assert.IsFalse(GetLazyImageSource(source.ElementAt(0)).IsValueCreated);
            var image = new System.Windows.Controls.Image() { Source = source.ElementAt(0).Image };
            Assert.IsTrue(GetLazyImageSource(source.ElementAt(0)).IsValueCreated);
        }
        public static Lazy<ImageSource> GetLazyImageSource(EnumMemberInfo enumMemberInfo) {
            return (Lazy<ImageSource>)enumMemberInfo.GetType().GetField("image", System.Reflection.BindingFlags.NonPublic |
                         System.Reflection.BindingFlags.Instance).GetValue(enumMemberInfo);
        }
        static void AssertEnumWithDisplayName(Type enumType) {
            var source = EnumSourceHelper.GetEnumSource(enumType);
            source.AreEqual(x => x.Count(), 2);
            source.ElementAt(0)
                .AreEqual(x => x.Name, "OneMember")
                .AreEqual(x => GetImageUri(x.Image), UriPrefix + "icon1.ico")
                .IsNull(x => x.Description);
            source.ElementAt(1)
                .AreEqual(x => x.Name, "TwoMember")
                .AreEqual(x => GetImageUri(x.Image).ToString(), UriPrefix + "icon2.ico")
                .AreEqual(x => x.Description, "desc");

            source = EnumSourceHelper.GetEnumSource(enumType, false, null, true);
            source.ElementAt(0)
                .AreEqual(x => x.Name, "OneMember");
            source.ElementAt(1)
                .AreEqual(x => x.Name, "TwoMember");

            source = EnumSourceHelper.GetEnumSource(enumType, false, new TestEnumValueConverter(), true);
            source.ElementAt(0)
                .AreEqual(x => x.Name, "MemberOne_");
            source.ElementAt(1)
                .AreEqual(x => x.Name, "MemberTwo_");
        }
#endregion

#region sort mode
        public enum EnumWithSortMode{
            B,
            CCC,
            AA,
        }
        public enum OrderedEnumWithSortMode {
            B,
            CCC,
            [Display(Order = 1)]
            DDDD,
            [Display(Order = -3)]
            VV,
            [Display(Order = 9999)]
            EEE,
            [Display(Order = 10015)]
            GGGGGGG,
            [Display(Order = 10001)]
            XXX,
            AA,
        }
        [Test]
        public void EnumWithSortModeTest() {
            EnumSourceHelperCore.GetEnumSource(typeof(EnumWithSortMode), true, null, false, EnumMembersSortMode.Default, (x, y) => { throw new InvalidOperationException(); })
                .Select(x => x.Name).SequenceEqual(new[] { "B", "CCC", "AA" }).IsTrue();
            EnumSourceHelper.GetEnumSource(typeof(EnumWithSortMode), true, null, false, EnumMembersSortMode.DisplayName)
                .Select(x => x.Name).SequenceEqual(new[] { "AA", "B", "CCC" }).IsTrue();
            EnumSourceHelper.GetEnumSource(typeof(EnumWithSortMode), true, null, false, EnumMembersSortMode.DisplayNameDescending)
                .Select(x => x.Name).SequenceEqual(new[] { "CCC", "B", "AA" }).IsTrue();
            EnumSourceHelper.GetEnumSource(typeof(EnumWithSortMode), true, null, false, EnumMembersSortMode.DisplayNameLength)
                .Select(x => x.Name).SequenceEqual(new[] { "B", "AA", "CCC" }).IsTrue();
            EnumSourceHelper.GetEnumSource(typeof(EnumWithSortMode), true, null, false, EnumMembersSortMode.DisplayNameLengthDescending)
                .Select(x => x.Name).SequenceEqual(new[] { "CCC", "AA", "B" }).IsTrue();
        }
        [Test]
        public void OrderedEnumWithSortModeTest() {
            EnumSourceHelperCore.GetEnumSource(typeof(OrderedEnumWithSortMode), true, null, false, EnumMembersSortMode.Default, (x, y) => { throw new InvalidOperationException(); })
                .Select(x => x.Name).SequenceEqual(new[] { "VV", "DDDD", "EEE", "B", "CCC", "AA", "XXX", "GGGGGGG" }).IsTrue();
            EnumSourceHelper.GetEnumSource(typeof(OrderedEnumWithSortMode), true, null, false, EnumMembersSortMode.DisplayName)
                .Select(x => x.Name).SequenceEqual(new[] { "VV", "DDDD", "EEE", "AA", "B", "CCC", "XXX", "GGGGGGG" }).IsTrue();
            EnumSourceHelper.GetEnumSource(typeof(OrderedEnumWithSortMode), true, null, false, EnumMembersSortMode.DisplayNameDescending)
                .Select(x => x.Name).SequenceEqual(new[] { "VV", "DDDD", "EEE", "CCC", "B", "AA", "XXX", "GGGGGGG" }).IsTrue();
            EnumSourceHelper.GetEnumSource(typeof(OrderedEnumWithSortMode), true, null, false, EnumMembersSortMode.DisplayNameLength)
                .Select(x => x.Name).SequenceEqual(new[] { "VV", "DDDD", "EEE", "B", "AA", "CCC", "XXX", "GGGGGGG" }).IsTrue();
            EnumSourceHelper.GetEnumSource(typeof(OrderedEnumWithSortMode), true, null, false, EnumMembersSortMode.DisplayNameLengthDescending)
                .Select(x => x.Name).SequenceEqual(new[] { "VV", "DDDD", "EEE", "CCC", "AA", "B", "XXX", "GGGGGGG" }).IsTrue();
        }
        static string GetImageUri(ImageSource image) {
            return image.ToString();
        }
        #endregion
    }
}