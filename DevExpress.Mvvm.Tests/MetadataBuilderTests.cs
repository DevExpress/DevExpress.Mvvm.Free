#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using System.Text.RegularExpressions;
#endif
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using System.Security;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;


namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class MetadataBuilderTests {
        #region
        [MetadataType(typeof(ValidationEntityMetadata))]
        public class ValidationEntity {
            public static string StringProperty1_CustomErrorText_Required;
            public static string StringProperty1_CustomErrorText_MinLength;
            public static string StringProperty1_CustomErrorText_MaxLength;
            public static string StringProperty3_CustomErrorText;

            public static string PhoneProperty_CustomErrorText;
            public static string CreditCardProperty_CustomErrorText;
            public static string EmailAddressProperty_CustomErrorText;
            public static string UrlProperty_CustomErrorText;

            public static string DoubleRange_CustomErrorText;
            public static string StringRegExp_CustomErrorText;
            public static string CustomString_CustomErrorText;

            public string StringProperty0 { get; set; }
            public string StringProperty1 { get; set; }
            public string StringProperty1_CustomError { get; set; }
            public string StringProperty2 { get; set; }
            public string StringProperty3 { get; set; }
            public string StringProperty3_CustomError { get; set; }

            public string PhoneProperty { get; set; }
            public string PhoneProperty_CustomError { get; set; }
            public string CreditCardProperty { get; set; }
            public string CreditCardProperty_CustomError { get; set; }
            public string EmailAddressProperty { get; set; }
            public string EmailAddressProperty_CustomError { get; set; }
            public string UrlProperty { get; set; }
            public string UrlProperty_CustomError { get; set; }

            public double DoubleRange { get; set; }
            public double? DoubleRange_Nullable { get; set; }
            public double DoubleRange_CustomError { get; set; }
            public int IntRange { get; set; }
            public string StringRange { get; set; }
            public string StringRegExp { get; set; }
            public string StringRegExp_CustomError { get; set; }
            public int IntRegExp { get; set; }

            public string CustomString_CustomError { get; set; }
            public string CustomString { get; set; }

            public string TwoErrorsProperty { get; set; }
        }
        public class ValidationEntityMetadata : IMetadataProvider<ValidationEntity> {
            void IMetadataProvider<ValidationEntity>.BuildMetadata(MetadataBuilder<ValidationEntity> builder) {
                builder.Property(x => x.StringProperty1)
                    .Required()
                    .MinLength(2)
                    .MaxLength(5);
                builder.Property(x => x.StringProperty1_CustomError)
                    .Required(() => ValidationEntity.StringProperty1_CustomErrorText_Required)
                    .MinLength(2, () => ValidationEntity.StringProperty1_CustomErrorText_MinLength)
                    .MaxLength(5, () => ValidationEntity.StringProperty1_CustomErrorText_MaxLength);
                builder.Property(x => x.StringProperty2)
                    .Required()
                    .MinLength(2);
                builder.Property(x => x.StringProperty3)
                    .Required(allowEmptyStrings: true);
                builder.Property(x => x.StringProperty3_CustomError)
                    .Required(allowEmptyStrings: true, errorMessageAccessor: () => ValidationEntity.StringProperty3_CustomErrorText);

                builder.Property(x => x.PhoneProperty)
                    .PhoneNumberDataType();
                builder.Property(x => x.PhoneProperty_CustomError)
                    .PhoneNumberDataType(() => ValidationEntity.PhoneProperty_CustomErrorText);
                builder.Property(x => x.CreditCardProperty)
                    .CreditCardDataType();
                builder.Property(x => x.CreditCardProperty_CustomError)
                    .CreditCardDataType(() => ValidationEntity.CreditCardProperty_CustomErrorText);
                builder.Property(x => x.EmailAddressProperty)
                    .EmailAddressDataType();
                builder.Property(x => x.EmailAddressProperty_CustomError)
                    .EmailAddressDataType(() => ValidationEntity.EmailAddressProperty_CustomErrorText);
                builder.Property(x => x.UrlProperty)
                    .UrlDataType();
                builder.Property(x => x.UrlProperty_CustomError)
                    .UrlDataType(() => ValidationEntity.UrlProperty_CustomErrorText);

                builder.Property(x => x.DoubleRange)
                    .InRange(9, 13);
                builder.Property(x => x.DoubleRange_Nullable)
                    .InRange(9, 13);
                builder.Property(x => x.DoubleRange_CustomError)
                    .InRange(9, 13, () => ValidationEntity.DoubleRange_CustomErrorText);
                builder.Property(x => x.IntRange)
                    .InRange(9, 13);
                builder.Property(x => x.StringRange)
                    .InRange("B", "D");
                builder.Property(x => x.StringRegExp)
                    .MatchesRegularExpression(@"^[a-z]{1,2}$");
                builder.Property(x => x.StringRegExp_CustomError)
                    .MatchesRegularExpression(@"^[a-z]{1,2}$", () => ValidationEntity.StringRegExp_CustomErrorText);
                builder.Property(x => x.IntRegExp)
                    .MatchesRegularExpression(@"^[1-2]{1,2}$");
                builder.Property(x => x.CustomString)
                    .MatchesRule(x => x.Length <= 2);
                builder.Property(x => x.CustomString_CustomError)
                    .MatchesRule(x => x.Length <= 2, () => ValidationEntity.CustomString_CustomErrorText);

                builder.Property(x => x.TwoErrorsProperty)
                    .MinLength(10)
                    .MaxLength(1);
            }
        }
        [Test]
        public void Validation() {
            var entity = new ValidationEntity();

            string required = "The StringProperty1 field is required.";
            string minLength = "The field StringProperty1 must be a string or array type with a minimum length of '2'.";
            string maxLength = "The field StringProperty1 must be a string or array type with a maximum length of '5'.";

            var property1Validator = CreateValidator<ValidationEntity, string>(x => x.StringProperty1);
            Assert.AreEqual(required, property1Validator.GetErrorText(null, entity));
            Assert.AreEqual(minLength, property1Validator.GetErrorText("1", entity));
            Assert.AreEqual(maxLength, property1Validator.GetErrorText("123456", entity));
            Assert.AreEqual(string.Empty, property1Validator.GetErrorText("123", entity));

            var property1Validator_CustomError = CreateValidator<ValidationEntity, string>(x => x.StringProperty1_CustomError);
            ValidationEntity.StringProperty1_CustomErrorText_Required = "{0} property required";
            Assert.AreEqual("StringProperty1_CustomError property required", property1Validator_CustomError.GetErrorText(null, entity));
            ValidationEntity.StringProperty1_CustomErrorText_MinLength = "{0} min";
            Assert.AreEqual("StringProperty1_CustomError min", property1Validator_CustomError.GetErrorText("1", entity));
            ValidationEntity.StringProperty1_CustomErrorText_MaxLength= "{0} max";
            Assert.AreEqual("StringProperty1_CustomError max", property1Validator_CustomError.GetErrorText("123456", entity));


            var property2Validator = CreateValidator<ValidationEntity, string>(x => x.StringProperty2);
            Assert.AreEqual(required.Replace("StringProperty1", "StringProperty2") + " " + minLength.Replace("StringProperty1", "StringProperty2"), property2Validator.GetErrorText(string.Empty, entity));

            var property3Validator = CreateValidator<ValidationEntity, string>(x => x.StringProperty3);
            Assert.AreEqual(string.Empty, property3Validator.GetErrorText(string.Empty, entity));
            Assert.AreEqual(required.Replace("StringProperty1", "StringProperty3"), property3Validator.GetErrorText(null, entity));

            var property3Validator_CustomError = CreateValidator<ValidationEntity, string>(x => x.StringProperty3_CustomError);
            ValidationEntity.StringProperty3_CustomErrorText = "{0} property required and doesn't allow empty strings";
            Assert.AreEqual("StringProperty3_CustomError property required and doesn't allow empty strings", property3Validator_CustomError.GetErrorText(null, entity));

            var phoneValidator = CreateValidator<ValidationEntity, string>(x => x.PhoneProperty);
            Assert.AreEqual("The PhoneProperty field is not a valid phone number.", phoneValidator.GetErrorText("abc", entity));
            phoneValidator = CreateValidator<ValidationEntity, string>(x => x.PhoneProperty_CustomError);
            ValidationEntity.PhoneProperty_CustomErrorText = "{0} phone";
            Assert.AreEqual("PhoneProperty_CustomError phone", phoneValidator.GetErrorText("abc", entity));

            var creditCardValidator = CreateValidator<ValidationEntity, string>(x => x.CreditCardProperty);
            Assert.AreEqual("The CreditCardProperty field is not a valid credit card number.", creditCardValidator.GetErrorText("1234 5678 1234 5678", entity));
            Assert.AreEqual(string.Empty, creditCardValidator.GetErrorText("4012888888881881", entity));
            Assert.AreEqual(string.Empty, creditCardValidator.GetErrorText("4012 8888 8888 1881", entity));
            creditCardValidator = CreateValidator<ValidationEntity, string>(x => x.CreditCardProperty_CustomError);
            ValidationEntity.CreditCardProperty_CustomErrorText= "{0} card";
            Assert.AreEqual("CreditCardProperty_CustomError card", creditCardValidator.GetErrorText("1234 5678 1234 5678", entity));

            var emailAddressPropertyValidator = CreateValidator<ValidationEntity, string>(x => x.EmailAddressProperty);
            Assert.AreEqual("The EmailAddressProperty field is not a valid e-mail address.", emailAddressPropertyValidator.GetErrorText("a@", entity));
            Assert.AreEqual(string.Empty, emailAddressPropertyValidator.GetErrorText("a@b.c", entity));
            emailAddressPropertyValidator = CreateValidator<ValidationEntity, string>(x => x.EmailAddressProperty_CustomError);
            ValidationEntity.EmailAddressProperty_CustomErrorText = "{0} mail";
            Assert.AreEqual("EmailAddressProperty_CustomError mail", emailAddressPropertyValidator.GetErrorText("a@", entity));

            var urlPropertyValidator = CreateValidator<ValidationEntity, string>(x => x.UrlProperty);
            Assert.AreEqual(string.Empty, urlPropertyValidator.GetErrorText("https://www.devexpress.com/", entity));
            Assert.AreEqual("The UrlProperty field is not a valid fully-qualified http, https, or ftp URL.", urlPropertyValidator.GetErrorText("abc", entity));
            urlPropertyValidator = CreateValidator<ValidationEntity, string>(x => x.UrlProperty_CustomError);
            ValidationEntity.UrlProperty_CustomErrorText = "{0} url";
            Assert.AreEqual("UrlProperty_CustomError url", urlPropertyValidator.GetErrorText("abc", entity));

            var doubleRangeValidator = CreateValidator<ValidationEntity, double>(x => x.DoubleRange);
            Assert.AreEqual(string.Empty, doubleRangeValidator.GetErrorText(10d, entity));
            Assert.AreEqual("The field DoubleRange must be between 9 and 13.", doubleRangeValidator.GetErrorText(8d, entity));
            Assert.AreEqual("The field DoubleRange must be between 9 and 13.", doubleRangeValidator.GetErrorText(14d, entity));
            doubleRangeValidator = CreateValidator<ValidationEntity, double?>(x => x.DoubleRange_Nullable);
            Assert.AreEqual(string.Empty, doubleRangeValidator.GetErrorText(10d, entity));
            Assert.AreEqual("The field DoubleRange_Nullable must be between 9 and 13.", doubleRangeValidator.GetErrorText(8d, entity));
            Assert.AreEqual("The field DoubleRange_Nullable must be between 9 and 13.", doubleRangeValidator.GetErrorText(14d, entity));
            Assert.AreEqual(string.Empty, doubleRangeValidator.GetErrorText(null, entity));
            doubleRangeValidator = CreateValidator<ValidationEntity, double>(x => x.DoubleRange_CustomError);
            ValidationEntity.DoubleRange_CustomErrorText = "{0} range {1} {2}";
            Assert.AreEqual("DoubleRange_CustomError range 9 13", doubleRangeValidator.GetErrorText(8d, entity));

            var intRangeValidator = CreateValidator<ValidationEntity, int>(x => x.IntRange);
            Assert.AreEqual(string.Empty, intRangeValidator.GetErrorText(10, entity));
            Assert.AreEqual("The field IntRange must be between 9 and 13.", intRangeValidator.GetErrorText(8, entity));
            Assert.AreEqual("The field IntRange must be between 9 and 13.", intRangeValidator.GetErrorText(14, entity));

            var stringRangeValidator = CreateValidator<ValidationEntity, string>(x => x.StringRange);
            Assert.AreEqual(string.Empty, stringRangeValidator.GetErrorText("Clown", entity));
            Assert.AreEqual(string.Empty, stringRangeValidator.GetErrorText(string.Empty, entity));
            Assert.AreEqual("The field StringRange must be between B and D.", stringRangeValidator.GetErrorText("Apple", entity));
            Assert.AreEqual("The field StringRange must be between B and D.", stringRangeValidator.GetErrorText("Express", entity));

            var stringRegExpValidator = CreateValidator<ValidationEntity, string>(x => x.StringRegExp);
            Assert.AreEqual(string.Empty, stringRegExpValidator.GetErrorText("cl", entity));
            Assert.AreEqual(@"The field StringRegExp must match the regular expression '^[a-z]{1,2}$'.", stringRegExpValidator.GetErrorText("Apple", entity));
            stringRegExpValidator = CreateValidator<ValidationEntity, string>(x => x.StringRegExp_CustomError);
            ValidationEntity.StringRegExp_CustomErrorText = "{0} regexp {1}";
            Assert.AreEqual(@"StringRegExp_CustomError regexp ^[a-z]{1,2}$", stringRegExpValidator.GetErrorText("Apple", entity));

            var intRegExpValidator = CreateValidator<ValidationEntity, int>(x => x.IntRegExp);
            Assert.AreEqual(string.Empty, intRegExpValidator.GetErrorText(1, entity));
            Assert.AreEqual(@"The field IntRegExp must match the regular expression '^[1-2]{1,2}$'.", intRegExpValidator.GetErrorText(3, entity));

            var customStringValidator = CreateValidator<ValidationEntity, string>(x => x.CustomString);
            Assert.AreEqual(string.Empty, customStringValidator.GetErrorText("12", entity));
            Assert.AreEqual("CustomString is not valid.", customStringValidator.GetErrorText("123", entity));
            customStringValidator = CreateValidator<ValidationEntity, string>(x => x.CustomString_CustomError);
            ValidationEntity.CustomString_CustomErrorText = "{0} custom";
            Assert.AreEqual("CustomString_CustomError custom", customStringValidator.GetErrorText("123", entity));

            var twoErrorsValidator = CreateValidator<ValidationEntity, string>(x => x.TwoErrorsProperty);
            Assert.AreEqual("The field TwoErrorsProperty must be a string or array type with a minimum length of '10'. The field TwoErrorsProperty must be a string or array type with a maximum length of '1'.", twoErrorsValidator.GetErrorText("123", entity));
            Assert.AreEqual("The field TwoErrorsProperty must be a string or array type with a minimum length of '10'.", twoErrorsValidator.GetErrors("123", entity).ElementAt(0));
            Assert.AreEqual("The field TwoErrorsProperty must be a string or array type with a maximum length of '1'.", twoErrorsValidator.GetErrors("123", entity).ElementAt(1));
            Assert.AreEqual(2, twoErrorsValidator.GetErrors("123", entity).Count());

        }
        static PropertyValidator CreateValidator<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
            string propertyName = BindableBase.GetPropertyNameFast(propertyExpression);
            return PropertyValidator.FromAttributes(MetadataHelper.GetExtenalAndFluentAPIAttrbutes(typeof(T), propertyName), propertyName);
        }
#if !SILVERLIGHT
        [Test]
        public void ResourceStringsTest() {
            foreach(var property in typeof(DataAnnotationsResources).GetProperties(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.PropertyType == typeof(string))) {
                Assert.AreEqual(DataAnnotationsResourcesResolver.AnnotationsResourceManager.GetString(property.Name), property.GetValue(null, null));
            }
        }
        [Test]
        public void RegexTest() {
            CheckRegex(typeof(PhoneAttribute), typeof(ValidationAttribute).Assembly.GetType(typeof(ValidationAttribute).Namespace + ".PhoneAttribute"));
            CheckRegex(typeof(UrlAttribute), typeof(ValidationAttribute).Assembly.GetType(typeof(ValidationAttribute).Namespace + ".UrlAttribute"));
            CheckRegex(typeof(EmailAddressAttribute), typeof(ValidationAttribute).Assembly.GetType(typeof(ValidationAttribute).Namespace + ".EmailAddressAttribute"));
        }
        static void CheckRegex(Type dxType, Type annotationsType) {
            Regex dxRegex = (Regex)dxType.GetField("regex", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            Regex regex = (Regex)annotationsType.GetField("_regex", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            Assert.AreEqual(GetPatternFromRegex(regex), GetPatternFromRegex(dxRegex));
        }
        static string GetPatternFromRegex(Regex regex) {
            return (string)typeof(Regex).GetField("pattern", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(regex);
        }
#endif
        #endregion

        #region

        #region
        [MetadataType(typeof(BaseMetadata))]
        public class BaseClass {
            public class BaseMetadata {
                [ReadOnly(true)]
                public string BaseProperty { get; set; }
            }
            public string BaseProperty { get; set; }
        }
        [MetadataType(typeof(Metadata))]
        public class Class : BaseClass {
            public class Metadata {
                [ReadOnly(true)]
                public string Property { get; set; }
            }
            public string Property { get; set; }
        }
        [Test]
        public void SeveralExternalMetadata() {
            Assert.IsNotNull(MetadataHelper.GetExtenalAndFluentAPIAttrbutes(typeof(Class), "Property").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(MetadataHelper.GetExtenalAndFluentAPIAttrbutes(typeof(Class), "BaseProperty").OfType<ReadOnlyAttribute>().Single());
        }
        #endregion

        #endregion

        #region
        [Test]
        public void SecuritySafeCriticalClassesHasNoInternalAutogeneratedClassesWhichImplementInterfaces() {
            foreach(Type type in Assembly.GetExecutingAssembly().GetTypes()) {
                if(type.IsNested || !IsSequritySafeCriticalType(type))
                    continue;
                CheckNestedTypes(type);
            }
        }
        void CheckNestedTypes(Type type) {
            foreach(Type nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)) {
                if(!IsSequritySafeCriticalType(nestedType))
                    Assert.IsFalse(nestedType.GetInterfaces().Any(), nestedType.FullName + " type is not SequritySafeCritical");
                CheckNestedTypes(nestedType);
            }
        }
        static bool IsSequritySafeCriticalType(Type type) {
            return type.GetCustomAttributes(false).OfType<SecuritySafeCriticalAttribute>().Any();
        }
        #endregion

        #region
        class StrictPropertiesAndMethodExpressionEntity {
            public int Prop { get; set; }
            public void Method() { }
        }
        public int StringProperty0 { get; set; }
        public void SomeMethod() { }
        public bool SomeMethod2() { return false; }
        [Test]
        public void StrictPropertiesAndMethodExpression() {
            var builder = new MetadataBuilder<StrictPropertiesAndMethodExpressionEntity>();
            TestHelper.AssertThrows<ArgumentException>(() => builder.Property(x => StringProperty0));
            TestHelper.AssertThrows<ArgumentException>(() => builder.CommandFromMethod(x => SomeMethod()));
            TestHelper.AssertThrows<ArgumentException>(() => builder.Property(x => x.Prop).OnPropertyChangedCall(x => SomeMethod()));
            TestHelper.AssertThrows<ArgumentException>(() => builder.Property(x => x.Prop).OnPropertyChangingCall(x => SomeMethod()));
            TestHelper.AssertThrows<ArgumentException>(() => builder.CommandFromMethod(x => x.Method()).CanExecuteMethod(x => SomeMethod2()));
        }
        #endregion
    }
}