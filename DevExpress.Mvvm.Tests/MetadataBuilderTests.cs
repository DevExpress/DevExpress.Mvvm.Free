using NUnit.Framework;
using System.Text.RegularExpressions;
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

using PhoneAttribute = DevExpress.Mvvm.Native.PhoneAttribute;
using UrlAttribute = DevExpress.Mvvm.Native.UrlAttribute;
using EmailAddressAttribute = DevExpress.Mvvm.Native.EmailAddressAttribute;

namespace DevExpress.Mvvm.Tests {
    public class MetadataBuilderTestsBase {
        #region MetadataHelper
        protected virtual bool UseFilteringAttributes { get { return false; } }
        public IEnumerable<Attribute> GetExternalAndFluentAPIAttributes(Type componentType, string propertyName) {
            if(!UseFilteringAttributes)
                return MetadataHelper.GetExternalAndFluentAPIAttributes(componentType, propertyName);
            else return MetadataHelper.GetExternalAndFluentAPIFilteringAttributes(componentType, propertyName);
        }
        public IAttributesProvider GetAttributesProvider(Type componentType, IMetadataLocator locator) {
            if(!UseFilteringAttributes)
                return MetadataHelper.GetAttributesProvider(componentType, locator);
            else return MetadataHelper.GetFilteringAttributesProvider(componentType, locator);
        }
        #endregion
    }
    [TestFixture]
    public class MetadataBuilderTests : MetadataBuilderTestsBase {

        #region validation
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
            public string CustomString_CustomError2 { get; set; }
            public string CustomString_CustomError3 { get; set; }
            public string CustomString { get; set; }

            public string TwoErrorsProperty { get; set; }


            public int MatchesRulePropery { get; set; }
            public int MatchesInstanceRulePropery { get; set; }
        }
        public class ValidationEntityMetadata : IMetadataProvider<ValidationEntity> {
            public static bool IncludeValueToError = false;
            void IMetadataProvider<ValidationEntity>.BuildMetadata(MetadataBuilder<ValidationEntity> builder) {
                builder.Property(x => x.StringProperty1)
                    .Required()
                    .MinLength(2)
                    .MaxLength(5);
                if(!IncludeValueToError) {
                    builder.Property(x => x.StringProperty1_CustomError)
                        .Required(() => ValidationEntity.StringProperty1_CustomErrorText_Required)
                        .MinLength(2, () => ValidationEntity.StringProperty1_CustomErrorText_MinLength)
                        .MaxLength(5, () => ValidationEntity.StringProperty1_CustomErrorText_MaxLength);
                } else {
                    builder.Property(x => x.StringProperty1_CustomError)
                        .Required(() => ValidationEntity.StringProperty1_CustomErrorText_Required)
                        .MinLength(2, x => ValidationEntity.StringProperty1_CustomErrorText_MinLength + x.ToString())
                        .MaxLength(5, x => ValidationEntity.StringProperty1_CustomErrorText_MaxLength + x.ToString());
                }
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
                if(!IncludeValueToError) {
                    builder.Property(x => x.DoubleRange_CustomError)
                        .InRange(9, 13, () => ValidationEntity.DoubleRange_CustomErrorText);
                } else {
                    builder.Property(x => x.DoubleRange_CustomError)
                        .InRange(9, 13, x => ValidationEntity.DoubleRange_CustomErrorText + x.ToString());
                }
                builder.Property(x => x.IntRange)
                    .InRange(9, 13);
                builder.Property(x => x.StringRange)
                    .InRange("B", "D");
                builder.Property(x => x.StringRegExp)
                    .MatchesRegularExpression(@"^[a-z]{1,2}$");
                if(!IncludeValueToError) {
                    builder.Property(x => x.StringRegExp_CustomError)
                        .MatchesRegularExpression(@"^[a-z]{1,2}$", () => ValidationEntity.StringRegExp_CustomErrorText);
                } else {
                    builder.Property(x => x.StringRegExp_CustomError)
                        .MatchesRegularExpression(@"^[a-z]{1,2}$", x => ValidationEntity.StringRegExp_CustomErrorText + x.ToString());
                }
                builder.Property(x => x.IntRegExp)
                    .MatchesRegularExpression(@"^[1-2]{1,2}$");
                builder.Property(x => x.CustomString)
                    .MatchesRule(x => x.Length <= 2);
                if(!IncludeValueToError) {
                    builder.Property(x => x.CustomString_CustomError)
                        .MatchesRule(x => x.Length <= 2, () => ValidationEntity.CustomString_CustomErrorText);
                    builder.Property(x => x.CustomString_CustomError3)
                        .MatchesInstanceRule((value, instance) => instance.CustomString_CustomError2.Length <= 2 || value.Length <= 2, () => ValidationEntity.CustomString_CustomErrorText);
                } else {
                    builder.Property(x => x.CustomString_CustomError)
                        .MatchesRule(x => x.Length <= 2, x => ValidationEntity.CustomString_CustomErrorText + x.ToString());
                    builder.Property(x => x.CustomString_CustomError3)
                        .MatchesInstanceRule((value, instance) => instance.CustomString_CustomError2.Length <= 2 || value.Length <= 2, (x, y) => ValidationEntity.CustomString_CustomErrorText + x.ToString() + y.ToString());
                }
#pragma warning disable 618
                builder.Property(x => x.CustomString_CustomError2)
                    .MatchesInstanceRule(x => x.CustomString_CustomError2.Length <= 2, () => ValidationEntity.CustomString_CustomErrorText);
#pragma warning restore 618

                builder.Property(x => x.TwoErrorsProperty)
                    .MinLength(10)
                    .MaxLength(1);

                builder.Property(x => x.MatchesRulePropery)
                   .MatchesRule(x => x >= 0, x => "Cannot be less than 0: " + x.ToString())
                   .MatchesRule(x => x <= 2, x => "Cannot be greater than 2: " + x.ToString());
                builder.Property(x => x.MatchesInstanceRulePropery)
                    .MatchesInstanceRule((x, y) => x >= 0, (x, y) => "Cannot be less than 0: " + x.ToString())
                    .MatchesInstanceRule((x, y) => x <= 2, (x, y) => "Cannot be greater than 2: " + x.ToString());

            }
        }
        [Test]
        public void Validation() {
            try {
                ValidationEntityMetadata.IncludeValueToError = false;
                MetadataLocator.Default = MetadataLocator.Create().AddMetadata<ValidationEntity, ValidationEntityMetadata>();
                ValidationCore((x, y, e) => e);
            } finally {
                MetadataLocator.Default = null;
                ValidationEntityMetadata.IncludeValueToError = false;
                MetadataHelper.ClearMetadata();
            }
        }
        [Test]
        public void Validation2() {
            try {
                ValidationEntityMetadata.IncludeValueToError = true;
                MetadataLocator.Default = MetadataLocator.Create().AddMetadata<ValidationEntity, ValidationEntityMetadata>();
                ValidationCore((x, y, e) => {
                    if(y != null) return e + x.ToString() + y.ToString();
                    else return e + x.ToString();
                });
            } finally {
                MetadataLocator.Default = null;
                ValidationEntityMetadata.IncludeValueToError = false;
                MetadataHelper.ClearMetadata();
            }
        }
        [Test]
        public void MatchesRuleTest() {
            try {
                MetadataLocator.Default = MetadataLocator.Create().AddMetadata<ValidationEntity, ValidationEntityMetadata>();
                var entity = new ValidationEntity();
                var v = CreateValidator<ValidationEntity, int>(x => x.MatchesRulePropery);
                Assert.AreEqual("Cannot be less than 0: -2", v.GetErrorText(-2, entity));
                Assert.AreEqual("Cannot be greater than 2: 3", v.GetErrorText(3, entity));

                v = CreateValidator<ValidationEntity, int>(x => x.MatchesInstanceRulePropery);
                Assert.AreEqual("Cannot be less than 0: -2", v.GetErrorText(-2, entity));
                Assert.AreEqual("Cannot be greater than 2: 3", v.GetErrorText(3, entity));
            } finally {
                MetadataLocator.Default = null;
                ValidationEntityMetadata.IncludeValueToError = false;
                MetadataHelper.ClearMetadata();
            }
        }
        [Test]
        public void MatchesRuleTest_Null() {
            try {
                MetadataLocator.Default = MetadataLocator.Create().AddMetadata<ValidationEntity, ValidationEntityMetadata>();
                var entity = new ValidationEntity();
                var v = CreateValidator<ValidationEntity, int>(x => x.MatchesRulePropery);
                Assert.AreEqual(string.Empty, v.GetErrorText(null, entity));
            } finally {
                MetadataLocator.Default = null;
                ValidationEntityMetadata.IncludeValueToError = false;
                MetadataHelper.ClearMetadata();
            }
        }
        void ValidationCore(Func<object, object, string, string> getError) {
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
            Assert.AreEqual(getError("1", null, "StringProperty1_CustomError min"), property1Validator_CustomError.GetErrorText("1", entity));
            ValidationEntity.StringProperty1_CustomErrorText_MaxLength = "{0} max";
            Assert.AreEqual(getError("123456", null, "StringProperty1_CustomError max"), property1Validator_CustomError.GetErrorText("123456", entity));


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
            ValidationEntity.CreditCardProperty_CustomErrorText = "{0} card";
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
            Assert.AreEqual(getError(8d, null, "DoubleRange_CustomError range 9 13"), doubleRangeValidator.GetErrorText(8d, entity));

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
            Assert.AreEqual(getError("Apple", null, @"StringRegExp_CustomError regexp ^[a-z]{1,2}$"), stringRegExpValidator.GetErrorText("Apple", entity));

            var intRegExpValidator = CreateValidator<ValidationEntity, int>(x => x.IntRegExp);
            Assert.AreEqual(string.Empty, intRegExpValidator.GetErrorText(1, entity));
            Assert.AreEqual(@"The field IntRegExp must match the regular expression '^[1-2]{1,2}$'.", intRegExpValidator.GetErrorText(3, entity));

            var customStringValidator = CreateValidator<ValidationEntity, string>(x => x.CustomString);
            Assert.AreEqual(string.Empty, customStringValidator.GetErrorText("12", entity));
            Assert.AreEqual("CustomString is not valid.", customStringValidator.GetErrorText("123", entity));
            customStringValidator = CreateValidator<ValidationEntity, string>(x => x.CustomString_CustomError);
            ValidationEntity.CustomString_CustomErrorText = "{0} custom";
            Assert.AreEqual(getError("123", null, "CustomString_CustomError custom"), customStringValidator.GetErrorText("123", entity));

            entity.CustomString_CustomError2 = "123";
            customStringValidator = CreateValidator<ValidationEntity, string>(x => x.CustomString_CustomError2);
            Assert.AreEqual("CustomString_CustomError2 custom", customStringValidator.GetErrorText(null, entity));

            customStringValidator = CreateValidator<ValidationEntity, string>(x => x.CustomString_CustomError3);
            Assert.AreEqual(string.Empty, customStringValidator.GetErrorText(string.Empty, entity));
            Assert.AreEqual(getError("123", entity, "CustomString_CustomError3 custom"), customStringValidator.GetErrorText("123", entity));

            var twoErrorsValidator = CreateValidator<ValidationEntity, string>(x => x.TwoErrorsProperty);
            Assert.AreEqual("The field TwoErrorsProperty must be a string or array type with a minimum length of '10'. The field TwoErrorsProperty must be a string or array type with a maximum length of '1'.", twoErrorsValidator.GetErrorText("123", entity));
            Assert.AreEqual("The field TwoErrorsProperty must be a string or array type with a minimum length of '10'.", twoErrorsValidator.GetErrors("123", entity).ElementAt(0));
            Assert.AreEqual("The field TwoErrorsProperty must be a string or array type with a maximum length of '1'.", twoErrorsValidator.GetErrors("123", entity).ElementAt(1));
            Assert.AreEqual(2, twoErrorsValidator.GetErrors("123", entity).Count());
        }
        static PropertyValidator CreateValidator<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
            string propertyName = BindableBase.GetPropertyNameFast(propertyExpression);
            return PropertyValidator.FromAttributes(MetadataHelper.GetExternalAndFluentAPIAttributes(typeof(T), propertyName), propertyName);
        }
        public class ValidationEntityWithDisplayNameAttributes {
            [DisplayName("_PropertyWithDisplayNameAttribute_")]
            [StringLength(2)]
            public string PropertyWithDisplayNameAttribute { get; set; }

            [Display(Name = "_PropertyWithDisplayAttribute_", ShortName = "_______")]
            [DisplayName("________")]
            [StringLength(2)]
            public string PropertyWithDisplayAttribute { get; set; }

            public string PropertyWithDisplayAttribute_Fluent { get; set; }
        }
        [Test]
        public void ValidationEntityWithDisplayNameAttributesTest() {
            var entity = new ValidationEntityWithDisplayNameAttributes {
                PropertyWithDisplayNameAttribute = "asdf",
                PropertyWithDisplayAttribute = "asdf",
                PropertyWithDisplayAttribute_Fluent = "asdf",
            };
            Assert.AreEqual("The field _PropertyWithDisplayNameAttribute_ must be a string with a maximum length of 2.", IDataErrorInfoHelper.GetErrorText(entity, "PropertyWithDisplayNameAttribute"));
            Assert.AreEqual("The field _PropertyWithDisplayAttribute_ must be a string with a maximum length of 2.", IDataErrorInfoHelper.GetErrorText(entity, "PropertyWithDisplayAttribute"));
        }
        [Test]
        public void ResourceStringsTest() {
            foreach(var property in typeof(DataAnnotationsResources).GetProperties(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.PropertyType == typeof(string))) {
                Assert.AreEqual(DataAnnotationsResourcesResolver.AnnotationsResourceManager.GetString(property.Name), property.GetValue(null, null));
            }
        }
#if !DXCORE3
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

        #region MetadataHelper tests

        #region several external metadata
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
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(Class), "Property").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(Class), "BaseProperty").OfType<ReadOnlyAttribute>().Single());
        }
        #endregion

        #region metadata with Fluent API
        [MetadataType(typeof(MetadataWithFluentApiMetadata))]
        public class MetadataWithFluentApi {
            public static void BuildMetadata(MetadataBuilder<MetadataWithFluentApi> builder) {
                builder.Property(x => x.Property).Required();
            }
            public string Property { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
        }
        public class MetadataWithFluentApiMetadata : IMetadataProvider<MetadataWithFluentApi> {
            [Display(Name = "test")]
            public string Property2 { get; set; }

            void IMetadataProvider<MetadataWithFluentApi>.BuildMetadata(MetadataBuilder<MetadataWithFluentApi> builder) {
                builder.Property(x => x.Property3).MaxLength(10);
            }
        }
        [MetadataType(typeof(MetadataWithFluentApi))]
        public class MetadataWithFluentApiClient {
            public string Property { get; set; }
        }
        [Test]
        public void MetadataWithFluentApiTest() {
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(MetadataWithFluentApiClient), "Property").OfType<DXRequiredAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(MetadataWithFluentApiClient), "Property2").OfType<DisplayAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(MetadataWithFluentApiClient), "Property3").OfType<DXMaxLengthAttribute>().Single());
        }
        #endregion

        #endregion

        #region sequrity
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

        #region MetadataBuilder test
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
        [DisplayName("Test")]
        public class DisplayNameTestClass {
        }
        public class DisplayNameTestClassMetadataProvider : IMetadataProvider<DisplayNameTestClass> {
            void IMetadataProvider<DisplayNameTestClass>.BuildMetadata(MetadataBuilder<DisplayNameTestClass> builder) {
                builder.DisplayName("Test2");
            }
        }
        [Test]
        public void DisplayNameTest() {
            if(UseFilteringAttributes) return;
            MetadataLocator.Default = MetadataLocator.Create().AddMetadata<DisplayNameTestClassMetadataProvider>();
            try {
                var attributes = GetExternalAndFluentAPIAttributes(typeof(DisplayNameTestClass), null);
                var attr = GetExternalAndFluentAPIAttributes(typeof(DisplayNameTestClass), null).OfType<DisplayNameAttribute>().Single();
                Assert.AreEqual("Test2", attr.DisplayName);
            } finally {
                MetadataLocator.Default = null;
            }
        }
    }
    [TestFixture]
    public class InternalMetadataLocatorTests : MetadataBuilderTestsBase {
        [Test]
        public void InternalLocatorTest() {
            MetadataHelper.AddMetadata<TestDataMetadata>();
            try {
                Assert.IsFalse(GetExternalAndFluentAPIAttributes(typeof(TestData), "Prop1").OfType<DisplayAttribute>().Single().AutoGenerateField);
            } finally {
                MetadataHelper.ClearMetadata();
            }
        }
        [Test]
        public void MetadataShouldBePublic() {
            AssertHelper.AssertThrows<InvalidOperationException>(() => {
                MetadataHelper.AddMetadata(typeof(PrivateMetadata));
            }, x => x.Message.AreEqual("The PrivateMetadata type should be public"));
            AssertHelper.AssertThrows<InvalidOperationException>(() => {
                MetadataLocator.Create().AddMetadata(typeof(PrivateMetadata));
            }, x => x.Message.AreEqual("The PrivateMetadata type should be public"));
        }
        [Test]
        public void InternalAndDefaultLocatorsPriorityTest() {
            var defaultLocator = MetadataLocator.Create()
                .AddMetadata<TestDataMetadataDefault>()
            ;
            MetadataLocator.Default = defaultLocator;
            MetadataHelper.AddMetadata<TestDataMetadata>();
            try {
                Assert.IsTrue(GetExternalAndFluentAPIAttributes(typeof(TestData), "Prop1").OfType<DisplayAttribute>().Single().AutoGenerateField);
            } finally {
                MetadataHelper.ClearMetadata();
                MetadataLocator.Default = null;
            }
        }
        [Test]
        public void CombinedTest() {
            var defaultLocator = MetadataLocator.Create()
                .AddMetadata<TestDataMetadataDefault>()
            ;
            MetadataLocator.Default = defaultLocator;
            MetadataHelper.AddMetadata<TestDataMetadata>();
            try {
                Assert.IsTrue(GetExternalAndFluentAPIAttributes(typeof(TestData), "Prop1").OfType<DisplayAttribute>().Single().AutoGenerateField);
            } finally {
                MetadataHelper.ClearMetadata();
                MetadataLocator.Default = null;
            }
            MetadataHelper.AddMetadata<TestDataMetadata>();
            try {
                Assert.IsFalse(GetExternalAndFluentAPIAttributes(typeof(TestData), "Prop1").OfType<DisplayAttribute>().Single().AutoGenerateField);
            } finally {
                MetadataHelper.ClearMetadata();
            }
        }

        public class TestData {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
        }
        public class TestDataMetadata : IMetadataProvider<TestData> {
            public void BuildMetadata(MetadataBuilder<TestData> builder) {
                builder.Property(x => x.Prop1).AddOrModifyAttribute<DisplayAttribute>(x => x.AutoGenerateField = false);
            }
        }
        public class TestDataMetadataDefault : IMetadataProvider<TestData> {
            public void BuildMetadata(MetadataBuilder<TestData> builder) {
                builder.Property(x => x.Prop1).AddOrModifyAttribute<DisplayAttribute>(x => x.AutoGenerateField = true);
            }
        }
        class PrivateMetadata : TestDataMetadata { }
    }
    [TestFixture]
    public class FilteringMetadataBuilderTests : MetadataBuilderTests {
        protected override bool UseFilteringAttributes { get { return true; } }
    }
    [TestFixture]
    public class InternalFilteringMetadataLocatorTests : InternalMetadataLocatorTests {
        protected override bool UseFilteringAttributes { get { return true; } }
    }


}