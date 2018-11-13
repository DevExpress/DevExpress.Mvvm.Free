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

#if !FREE
using DevExpress.Xpf.Core.Tests;
using DevExpress.Utils.Filtering;
using System.Windows;
#endif
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
#if !FREE
        [Test]
        public void ConvertToTest() {
            GetExternalAndFluentAPIAttributes(typeof(ConvertToTestObject), "Property").OfType<ConvertToAttribute>().Single()
                .IsTrue(x => x.Type == typeof(List<object>));
        }
        public class ConvertToTestObject {
            public string Property { get; set; }

            public static void BuildMetadata(MetadataBuilder<ConvertToTestObject> builder) {
                builder.Property(x => x.Property)
                    .ConvertTo(typeof(List<object>));
            }
        }
        #region EditorAttributes
        public class EditorAttributesEntity {
            public static void BuildMetadata(MetadataBuilder<EditorAttributesEntity> builder) {
                builder.Property(x => x.Property1)
                    .DefaultEditor("DefaultTemplate")
                    .GridEditor("GridTemplate")
                    .LayoutControlEditor("LayoutControlTemplate")
                    .PropertyGridEditor("PropertyGridTemplate");

                builder
                    .DefaultEditor("DefaultTemplate")
                    .GridEditor("GridTemplate")
                    .LayoutControlEditor("LayoutControlTemplate")
                    .PropertyGridEditor("PropertyGridTemplate");
            }
            public int Property1 { get; set; }
        }
        [Test]
        public void EditorAttributesTest() {
            var defaultEditorAttr = GetExternalAndFluentAPIAttributes(typeof(EditorAttributesEntity), "Property1").OfType<DefaultEditorAttribute>().Single();
            var gridEditorAttr = GetExternalAndFluentAPIAttributes(typeof(EditorAttributesEntity), "Property1").OfType<GridEditorAttribute>().Single();
            var layoutControlEditorAttr = GetExternalAndFluentAPIAttributes(typeof(EditorAttributesEntity), "Property1").OfType<LayoutControlEditorAttribute>().Single();
            var propertyGridEditorAttr = GetExternalAndFluentAPIAttributes(typeof(EditorAttributesEntity), "Property1").OfType<PropertyGridEditorAttribute>().Single();
            Assert.AreEqual("DefaultTemplate", defaultEditorAttr.TemplateKey);
            Assert.AreEqual("GridTemplate", gridEditorAttr.TemplateKey);
            Assert.AreEqual("LayoutControlTemplate", layoutControlEditorAttr.TemplateKey);
            Assert.AreEqual("PropertyGridTemplate", propertyGridEditorAttr.TemplateKey);

            defaultEditorAttr = GetExternalAndFluentAPIAttributes(typeof(EditorAttributesEntity), null).OfType<DefaultEditorAttribute>().Single();
            gridEditorAttr = GetExternalAndFluentAPIAttributes(typeof(EditorAttributesEntity), null).OfType<GridEditorAttribute>().Single();
            layoutControlEditorAttr = GetExternalAndFluentAPIAttributes(typeof(EditorAttributesEntity), null).OfType<LayoutControlEditorAttribute>().Single();
            propertyGridEditorAttr = GetExternalAndFluentAPIAttributes(typeof(EditorAttributesEntity), null).OfType<PropertyGridEditorAttribute>().Single();
            Assert.AreEqual("DefaultTemplate", defaultEditorAttr.TemplateKey);
            Assert.AreEqual("GridTemplate", gridEditorAttr.TemplateKey);
            Assert.AreEqual("LayoutControlTemplate", layoutControlEditorAttr.TemplateKey);
            Assert.AreEqual("PropertyGridTemplate", propertyGridEditorAttr.TemplateKey);
        }
        #endregion
#endif

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
#if !FREE
            public static void BuildMetadata(MetadataBuilder<ValidationEntityWithDisplayNameAttributes> builder) {
                builder.Property(x => x.PropertyWithDisplayAttribute_Fluent)
                    .DisplayShortName("_PropertyWithDisplayAttribute_Fluent_")
                    .MaxLength(2);
            }
#endif
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
#if !FREE
            Assert.AreEqual("The field _PropertyWithDisplayAttribute_Fluent_ must be a string or array type with a maximum length of '2'.", IDataErrorInfoHelper.GetErrorText(entity, "PropertyWithDisplayAttribute_Fluent"));
#endif
        }
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

#if !FREE
        #region metadata from static method
        public abstract class MetadataFromStaticMethodClassBase {
            public static void BuildMetadata(MetadataBuilder<MetadataFromStaticMethodClassBase> builder) {
                builder.Property(x => x.BaseProperty).ReadOnly();
            }
            public string BaseProperty { get; set; }
        }
        [MetadataType(typeof(Metadata)), CLSCompliant(false)]
        public class MetadataFromStaticMethodClass : MetadataFromStaticMethodClassBase {
            #region fakes
            public static void BuildMetadata(MetadataBuilder<Class> builder) {
                throw new NotImplementedException();
            }
            public static void BuildMetadata(out MetadataBuilder<MetadataFromStaticMethodClass> builder) {
                throw new NotImplementedException();
            }
            public static void BuildMetadata(int x) {
                throw new NotImplementedException();
            }
            public static void BuildMetadata() {
                throw new NotImplementedException();
            }
            public static void BuildMetadata(MetadataBuilder<MetadataFromStaticMethodClass> builder, int x) {
                throw new NotImplementedException();
            }
            public static void BuildMetadata2(MetadataBuilder<MetadataFromStaticMethodClass> builder) {
                throw new NotImplementedException();
            }
            #endregion

            public class Metadata : IMetadataProvider<MetadataFromStaticMethodClass> {
                void IMetadataProvider<MetadataFromStaticMethodClass>.BuildMetadata(MetadataBuilder<MetadataFromStaticMethodClass> builder) {
                    builder.Property(x => x.Property2).ReadOnly();
                }
                public static void BuildMetadata(MetadataBuilder<MetadataFromStaticMethodClass> builder) {
                    builder.Property(x => x.Property3).ReadOnly();
                }
            }
            public static void BuildMetadata(MetadataBuilder<MetadataFromStaticMethodClass> builder) {
                builder.Property(x => x.Property).ReadOnly();
            }
            public string Property { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
        }
        [MetadataType(typeof(Metadata))]
        public class MetadataFromStaticMethodClass2 {
            public static class Metadata {
                public static void BuildMetadata(MetadataBuilder<MetadataFromStaticMethodClass2> builder) {
                    builder.Property(x => x.Property).ReadOnly();
                }
            }
            public string Property { get; set; }
        }
        public class MetadataFromStaticMethodClass_Fake1 {
            internal static void BuildMetadata(MetadataBuilder<MetadataFromStaticMethodClass_Fake1> builder) {
                throw new NotImplementedException();
            }
            public string Property { get; set; }
        }
        public class MetadataFromStaticMethodClass_Fake2 {
            public static int BuildMetadata(MetadataBuilder<MetadataFromStaticMethodClass_Fake2> builder) {
                throw new NotImplementedException();
            }
            public string Property { get; set; }
        }
        [Test]
        public void GetMetadataFromStaticMethod() {
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(MetadataFromStaticMethodClass), "Property").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(MetadataFromStaticMethodClass), "Property2").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(MetadataFromStaticMethodClass), "Property3").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(MetadataFromStaticMethodClass), "BaseProperty").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(MetadataFromStaticMethodClass2), "Property").OfType<ReadOnlyAttribute>().Single());
            Assert.IsFalse(GetExternalAndFluentAPIAttributes(typeof(MetadataFromStaticMethodClass_Fake1), "Property").Any());
            Assert.IsFalse(GetExternalAndFluentAPIAttributes(typeof(MetadataFromStaticMethodClass_Fake2), "Property").Any());
        }
        #endregion
        #region Generic metadata
        [MetadataType(typeof(BaseGenericClassMetadata<>))]
        public class BaseGenericClass<T1> {
            public int BaseProperty1 { get; set; }
            public int BaseProperty2 { get; set; }
            public int BaseProperty3 { get; set; }
            public int BaseProperty4 { get; set; }
        }
        public class BaseGenericClassMetadata<T1> {
            public static void BuildMetadata(MetadataBuilder<BaseGenericClass<T1>> builder) {
                builder.Property(x => x.BaseProperty2).ReadOnly();
            }
            public static void BuildBaseMetadata<T>(MetadataBuilder<T> builder) where T : BaseGenericClass<T1> {
                builder.Property(x => x.BaseProperty4).ReadOnly();
            }
        }

        [MetadataType(typeof(GenericClassMetadata<,>))]
        public class GenericClass<T1, T2> : BaseGenericClass<T1> {
            public GenericClass<T1, int> Property1 { get; set; }
            public int Property2 { get; set; }
            public int Property3 { get; set; }
        }
        public class GenericClassMetadata<T1, T2> : IMetadataProvider<GenericClass<T1, T2>> {
            public static void BuildMetadata(MetadataBuilder<GenericClass<T1, T2>> builder) {
                builder.Property(x => x.Property1).ReadOnly();
                builder.Property(x => x.BaseProperty1).ReadOnly();
                builder.Property(x => x.BaseProperty3).ReadOnly();
                BaseGenericClassMetadata<T1>.BuildBaseMetadata(builder);
            }
            void IMetadataProvider<GenericClass<T1, T2>>.BuildMetadata(MetadataBuilder<GenericClass<T1, T2>> builder) {
                builder.Property(x => x.Property2).ReadOnly();
            }
            [ReadOnly(true)]
            public object Property3 { get; set; }
        }
        [Test]
        public void GetMetadataForGenericClass() {
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(GenericClass<int, string>), "Property1").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(GenericClass<string, string>), "Property1").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(GenericClass<string, int>), "Property2").OfType<ReadOnlyAttribute>().Single());

            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(GenericClass<int, string>), "BaseProperty1").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(GenericClass<int, string>), "BaseProperty2").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(GenericClass<int, string>), "BaseProperty3").OfType<ReadOnlyAttribute>().Single());
            Assert.IsNotNull(GetExternalAndFluentAPIAttributes(typeof(GenericClass<int, string>), "BaseProperty4").OfType<ReadOnlyAttribute>().Single());
            Assert.IsFalse(GetExternalAndFluentAPIAttributes(typeof(BaseGenericClass<int>), "BaseProperty3").Any());
            Assert.IsFalse(GetExternalAndFluentAPIAttributes(typeof(BaseGenericClass<int>), "BaseProperty4").Any());
        }

        #endregion
        #region local metadata locator
        public class LocalMetatadaEntity {
            [Display(Name = "attribute")]
            public string Name { get; set; }
        }
        public static class LocalMetadata {
            public static int BuildCount;
            public static void BuildMetadata(MetadataBuilder<LocalMetatadaEntity> builder) {
                builder.Property(x => x.Name).DisplayName("local metadata");
                BuildCount++;
            }
        }
        [Test]
        public void LocalMetadataLocator() {
            LocalMetadata.BuildCount = 0;
            var locator = MetadataLocator.Create().AddMetadata(typeof(LocalMetadata));
            var provider = GetAttributesProvider(typeof(LocalMetatadaEntity), locator);
            Assert.AreEqual("local metadata", provider.GetAttributes("Name").OfType<DisplayAttribute>().First().GetName());
            Assert.AreEqual("local metadata", provider.GetAttributes("Name").OfType<DisplayAttribute>().First().GetName());
            Assert.AreEqual(1, LocalMetadata.BuildCount);
        }
        #endregion
        public class BindingDirectionMetadata : IEnumMetadataProvider<BindingDirection> {
            public void BuildMetadata(EnumMetadataBuilder<BindingDirection> builder) {
                builder.DefaultEditor("BindingDirectionEditor");
            }
        }
        [Test]
        public void MetadataLocator_RegisterEnumMetadata() {
            var locator = MetadataLocator.Create().AddMetadata(typeof(BindingDirectionMetadata));
            var provider = GetAttributesProvider(typeof(BindingDirection), locator);
            Assert.AreEqual("BindingDirectionEditor", provider.GetAttributes(null).OfType<DefaultEditorAttribute>().Single().TemplateKey);
        }
#endif
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
#if !FREE
        #region Display Format and Mask
        [CLSCompliant(false)]
        public class DisplayFormatAndMaskEntity {
            public static readonly CultureInfo Culture = new CultureInfo("ru-RU");
            public static void BuildMetadata(MetadataBuilder<DisplayFormatAndMaskEntity> builder) {
                builder.Property(x => x.Property1)
                    .DoNotConvertEmptyStringToNull()
                    .DisplayFormatString("c")
                    .NullDisplayText("empty");
                builder.Property(x => x.Property2)
                    .DisplayFormatString("c", true);

                builder.Property(x => x.DateTimeProperty1)
                    .DateTimeMask("s")
                        .MaskCulture(Culture)
                        .MaskAutomaticallyAdvanceCaret()
                    .EndMask()
                    .ReadOnly();

                builder.Property(x => x.DateTimeProperty2)
                    .DateTimeMask("u", useMaskAsDisplayFormat: false);

                builder.Property(x => x.DoubleProperty).NumericMask("p");
                builder.Property(x => x.NullableDoubleProperty).NumericMask("P");

                builder.Property(x => x.DecimalProperty).NumericMask("p");
                builder.Property(x => x.NullableDecimalProperty).NumericMask("P");

                builder.Property(x => x.FloatProperty).NumericMask("p");
                builder.Property(x => x.NullableFloatProperty).NumericMask("P");

                builder.Property(x => x.ByteProperty).NumericMask("p");

                builder.Property(x => x.IntProperty).NumericMask("p").MaskCulture(Culture);
                builder.Property(x => x.NullableIntProperty).NumericMask("P", useMaskAsDisplayFormat: false);

                builder.Property(x => x.ShortProperty).NumericMask("p");
                builder.Property(x => x.NullableShortProperty).NumericMask("P");

                builder.Property(x => x.LongProperty).NumericMask("p");
                builder.Property(x => x.NullableLongProperty).NumericMask("P");

                builder.Property(x => x.UIntProperty).NumericMask("p").MaskCulture(Culture);
                builder.Property(x => x.UNullableIntProperty).NumericMask("P", false);

                builder.Property(x => x.UShortProperty).NumericMask("p");
                builder.Property(x => x.UNullableShortProperty).NumericMask("P");

                builder.Property(x => x.ULongProperty).NumericMask("p");
                builder.Property(x => x.UNullableLongProperty).NumericMask("P");

                builder.Property(x => x.SimpleMaskProperty)
                    .SimpleMask("s1")
                        .MaskDoNotIgnoreBlank()
                        .MaskPlaceHolder('x')
                        .MaskDoNotSaveLiteral();
                builder.Property(x => x.RegularMaskProperty)
                    .RegularMask("r1")
                        .MaskDoNotSaveLiteral();
                builder.Property(x => x.RegExMaskProperty)
                    .RegExMask("rx1")
                        .MaskDoNotShowPlaceHolders();
            }

            public int Property1 { get; set; }
            public int Property2 { get; set; }

            public DateTime DateTimeProperty1 { get; set; }
            public DateTime DateTimeProperty2 { get; set; }

            public double DoubleProperty { get; set; }
            public double? NullableDoubleProperty { get; set; }

            public decimal DecimalProperty { get; set; }
            public decimal? NullableDecimalProperty { get; set; }

            public float FloatProperty { get; set; }
            public float? NullableFloatProperty { get; set; }

            public byte ByteProperty { get; set; }

            public int IntProperty { get; set; }
            public int? NullableIntProperty { get; set; }

            public short ShortProperty { get; set; }
            public short? NullableShortProperty { get; set; }

            public long LongProperty { get; set; }
            public long? NullableLongProperty { get; set; }

            public uint UIntProperty { get; set; }
            public uint? UNullableIntProperty { get; set; }

            public ushort UShortProperty { get; set; }
            public ushort? UNullableShortProperty { get; set; }

            public ulong ULongProperty { get; set; }
            public ulong? UNullableLongProperty { get; set; }

            public string SimpleMaskProperty { get; set; }
            public string RegularMaskProperty { get; set; }
            public string RegExMaskProperty { get; set; }
        }
        [Test]
        public void RegExMaskAttributeTest() {
            var simpleMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "SimpleMaskProperty").OfType<SimpleMaskAttribute>().Single();
            Assert.AreEqual("s1", simpleMaskAttribute.Mask);
            Assert.AreEqual(false, simpleMaskAttribute.UseAsDisplayFormat);
            Assert.AreEqual(false, simpleMaskAttribute.IgnoreBlank);
            Assert.AreEqual('x', simpleMaskAttribute.PlaceHolder);
            Assert.AreEqual(false, simpleMaskAttribute.SaveLiteral);

            var regularMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "RegularMaskProperty").OfType<RegularMaskAttribute>().Single();
            Assert.AreEqual("r1", regularMaskAttribute.Mask);
            Assert.AreEqual(false, regularMaskAttribute.SaveLiteral);

            var regExMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "RegExMaskProperty").OfType<RegExMaskAttribute>().Single();
            Assert.AreEqual("rx1", regExMaskAttribute.Mask);
            Assert.AreEqual(false, regExMaskAttribute.ShowPlaceHolders);
        }
        [Test]
        public void NumericMaskAttributeTest() {
            var numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "IntProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);
            Assert.AreEqual(true, numericMaskAttribute.UseAsDisplayFormat);
            Assert.AreSame(DisplayFormatAndMaskEntity.Culture, numericMaskAttribute.CultureInfo);
            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "NullableIntProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("P", numericMaskAttribute.Mask);
            Assert.AreEqual(false, numericMaskAttribute.UseAsDisplayFormat);

            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "DoubleProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);
            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "NullableDoubleProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("P", numericMaskAttribute.Mask);

            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "DecimalProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);
            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "NullableDecimalProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("P", numericMaskAttribute.Mask);

            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "FloatProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);
            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "NullableFloatProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("P", numericMaskAttribute.Mask);

            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "ShortProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);
            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "NullableShortProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("P", numericMaskAttribute.Mask);

            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "ByteProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);

            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "LongProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);
            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "NullableLongProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("P", numericMaskAttribute.Mask);

            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "UIntProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);
            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "UNullableIntProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("P", numericMaskAttribute.Mask);

            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "UShortProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);
            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "UNullableShortProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("P", numericMaskAttribute.Mask);

            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "ULongProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("p", numericMaskAttribute.Mask);
            numericMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "UNullableLongProperty").OfType<NumericMaskAttribute>().Single();
            Assert.AreEqual("P", numericMaskAttribute.Mask);
        }
        [Test]
        public void DateTimeMaskAttributeTest() {
            GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "DateTimeProperty1").OfType<ReadOnlyAttribute>().Single();
            var dateTimeMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "DateTimeProperty1").OfType<DateTimeMaskAttribute>().Single();
            Assert.AreEqual("s", dateTimeMaskAttribute.Mask);
            Assert.AreEqual(true, dateTimeMaskAttribute.UseAsDisplayFormat);
            Assert.AreEqual(true, dateTimeMaskAttribute.AutomaticallyAdvanceCaret);
            Assert.AreSame(DisplayFormatAndMaskEntity.Culture, dateTimeMaskAttribute.CultureInfo);

            dateTimeMaskAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "DateTimeProperty2").OfType<DateTimeMaskAttribute>().Single();
            Assert.AreEqual("u", dateTimeMaskAttribute.Mask);
            Assert.AreEqual(false, dateTimeMaskAttribute.UseAsDisplayFormat);
            Assert.AreEqual(false, dateTimeMaskAttribute.AutomaticallyAdvanceCaret);
            Assert.IsNull(dateTimeMaskAttribute.CultureInfo);
        }
        [Test]
        public void DisplayFormatAttributeTest() {
            var defaultDisplayFormatAttribute = new DisplayFormatAttribute();
            Assert.AreEqual(false, defaultDisplayFormatAttribute.ApplyFormatInEditMode);
            Assert.AreEqual(true, defaultDisplayFormatAttribute.ConvertEmptyStringToNull);

            var displayFormatAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "Property1").OfType<DisplayFormatAttribute>().Single();
            Assert.AreEqual("empty", displayFormatAttribute.NullDisplayText);
            Assert.AreEqual("c", displayFormatAttribute.DataFormatString);
            Assert.AreEqual(false, displayFormatAttribute.ApplyFormatInEditMode);
            Assert.AreEqual(false, displayFormatAttribute.ConvertEmptyStringToNull);

            displayFormatAttribute = GetExternalAndFluentAPIAttributes(typeof(DisplayFormatAndMaskEntity), "Property2").OfType<DisplayFormatAttribute>().Single();
            Assert.AreEqual("c", displayFormatAttribute.DataFormatString);
            Assert.AreEqual(true, displayFormatAttribute.ApplyFormatInEditMode);
        }
        #endregion


        #region MetadataLocator
        public class ExternalMetadataEntity1Base {
            public string BaseProperty1 { get; set; }
        }
        public class ExternalMetadataEntity1 : ExternalMetadataEntity1Base {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
        }
        public class ExternalMetadata1Base {
            [ReadOnly(true)]
            public string BaseProperty1 { get; set; }
        }
        public class ExternalMetadata1 {
            [ReadOnly(true)]
            public string Property1 { get; set; }
        }
        public class ExternalMetadataEntity2 {
            public string Property1_ { get; set; }
            public string Property2_ { get; set; }
            public string Property3_ { get; set; }
        }
        public class ExternalMetadata2 {
            [ReadOnly(true)]
            public string Property1_ { get; set; }
        }
        public class CommonExternalMetadata1 : IMetadataProvider<ExternalMetadataEntity1>, IMetadataProvider<ExternalMetadataEntity2> {
            void IMetadataProvider<ExternalMetadataEntity1>.BuildMetadata(MetadataBuilder<ExternalMetadataEntity1> builder) {
                builder.Property(x => x.Property2).ReadOnly();
            }
            void IMetadataProvider<ExternalMetadataEntity2>.BuildMetadata(MetadataBuilder<ExternalMetadataEntity2> builder) {
                builder.Property(x => x.Property2_).ReadOnly();
            }
            public static void BuildMetadata(MetadataBuilder<ExternalMetadataEntity1> builder) {
                builder.Property(x => x.Property4).ReadOnly();
            }
        }
        public class CommonExternalMetadata2 {
            public static void BuildMetadata(MetadataBuilder<ExternalMetadataEntity1> builder) {
                builder.Property(x => x.Property3).ReadOnly();
            }
            public static void BuildMetadata(MetadataBuilder<ExternalMetadataEntity2> builder) {
                builder.Property(x => x.Property3_).ReadOnly();
            }
        }
        public class ExternalMetadataEntityGeneric<T> {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }
        public class ExternalMetadataGeneric<T> {
            public static void BuildMetadata(MetadataBuilder<ExternalMetadataEntityGeneric<T>> builder) {
                builder.Property(x => x.Property1).ReadOnly();
            }
        }
        public class ExternalMetadataGeneric2<T> : IMetadataProvider<ExternalMetadataEntityGeneric<T>> {
            void IMetadataProvider<ExternalMetadataEntityGeneric<T>>.BuildMetadata(MetadataBuilder<ExternalMetadataEntityGeneric<T>> builder) {
                builder.Property(x => x.Property2).ReadOnly();
            }
        }
        [Test]
        public void GetMetadataFromLocatorTest() {
            try {
                MetadataLocator.Default = MetadataLocator.Create()
                    .AddMetadata<ExternalMetadataEntity1, ExternalMetadata1>()
                    .AddMetadata<ExternalMetadataEntity1Base, ExternalMetadata1Base>()
                    .AddMetadata<ExternalMetadataEntity1, ExternalMetadata1>()
                    .AddMetadata<ExternalMetadataEntity2, ExternalMetadata2>()
                    .AddMetadata<CommonExternalMetadata1>()
                    .AddMetadata<CommonExternalMetadata2>()
                    .AddMetadata(typeof(ExternalMetadataGeneric<>))
                    .AddMetadata(typeof(ExternalMetadataGeneric2<>));
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntity1), "Property1").OfType<ReadOnlyAttribute>().Single();
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntity1), "BaseProperty1").OfType<ReadOnlyAttribute>().Single();
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntity1), "Property2").OfType<ReadOnlyAttribute>().Single();
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntity1), "Property3").OfType<ReadOnlyAttribute>().Single();
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntity1), "Property4").OfType<ReadOnlyAttribute>().Single();
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntity2), "Property1_").OfType<ReadOnlyAttribute>().Single();
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntity2), "Property2_").OfType<ReadOnlyAttribute>().Single();
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntity2), "Property3_").OfType<ReadOnlyAttribute>().Single();
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntityGeneric<int>), "Property1").OfType<ReadOnlyAttribute>().Single();
                GetExternalAndFluentAPIAttributes(typeof(ExternalMetadataEntityGeneric<string>), "Property2").OfType<ReadOnlyAttribute>().Single();
            } finally {
                MetadataLocator.Default = null;
            }
        }
        #endregion

        public class DetailEntity {
        }
        public class ScaffoldDetailCollectionAttributeEntity {
            public static void BuildMetadata(MetadataBuilder<ScaffoldDetailCollectionAttributeEntity> builder) {
                builder.Property(x => x.DoNotScaffold).DoNotScaffoldDetailCollection();
            }
            public ICollection<DetailEntity> DoNotScaffold { get; set; }
        }
        [Test]
        public void ScaffoldDetailCollectionAttributeTest() {
            var scaffoldDetailCollectionAttribute = GetExternalAndFluentAPIAttributes(typeof(ScaffoldDetailCollectionAttributeEntity), "DoNotScaffold").OfType<ScaffoldDetailCollectionAttribute>().Single();
            Assert.IsFalse(scaffoldDetailCollectionAttribute.Scaffold);
        }
        public class AutoGenerateEntity {
            public static void BuildMetadata(MetadataBuilder<AutoGenerateEntity> builder) {
                builder.Property(x => x.AutoGen).AutoGenerated();
                builder.Property(x => x.NoAutoGen).NotAutoGenerated();
            }
            public string AutoGen { get; set; }
            public string NoAutoGen { get; set; }
        }
        [Test]
        public void AutoGenerateTest() {
            var displayAttribute = GetExternalAndFluentAPIAttributes(typeof(AutoGenerateEntity), "NoAutoGen").OfType<DisplayAttribute>().Single();
            Assert.AreEqual(false, displayAttribute.GetAutoGenerateField());
            displayAttribute = GetExternalAndFluentAPIAttributes(typeof(AutoGenerateEntity), "AutoGen").OfType<DisplayAttribute>().Single();
            Assert.AreEqual(true, displayAttribute.GetAutoGenerateField());
        }

        #region TypeConverter
        class TestTypeDescriptorContext : ITypeDescriptorContext {
            IContainer ITypeDescriptorContext.Container { get { throw new NotImplementedException(); } }
            object ITypeDescriptorContext.Instance { get { return new EntityWithTypeConverter(); } }
            PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor { get { throw new NotImplementedException(); } }
            object IServiceProvider.GetService(Type serviceType) { throw new NotImplementedException(); }
            void ITypeDescriptorContext.OnComponentChanged() { throw new NotImplementedException(); }
            bool ITypeDescriptorContext.OnComponentChanging() { throw new NotImplementedException(); }
        }

        class TestTypeConverter : TypeConverter {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
                return sourceType == typeof(char) || base.CanConvertFrom(context, sourceType);
            }

            public int CreateInstanceCallCount { get; private set; }
            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues) {
                Assert.IsNotNull(context);
                Assert.IsNotNull(propertyValues);
                CreateInstanceCallCount++;
                return base.CreateInstance(context, propertyValues);
            }

            public int GetCreateInstanceSupportedCallCount { get; private set; }
            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
                Assert.IsNotNull(context);
                GetCreateInstanceSupportedCallCount++;
                return base.GetCreateInstanceSupported(context);
            }

            public int GetPropertiesCallCount { get; private set; }
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
                Assert.IsNotNull(context);
                Assert.AreEqual(9, value);
                Assert.IsNotNull(attributes);
                GetPropertiesCallCount++;
                return base.GetProperties(context, value, attributes);
            }

            public int GetPropertiesSupportedCallCount { get; private set; }
            public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
                Assert.IsNotNull(context);
                GetPropertiesSupportedCallCount++;
                return base.GetPropertiesSupported(context);
            }

            public int IsValidCallCount { get; private set; }
            public override bool IsValid(ITypeDescriptorContext context, object value) {
                Assert.IsNotNull(context);
                Assert.AreEqual(9, value);
                IsValidCallCount++;
                return base.IsValid(context, value);
            }
        }

        public class EntityWithTypeConverter {
            public static void BuildMetadata(MetadataBuilder<EntityWithTypeConverter> builder) {
                builder.Property(x => x.EmptyTypeConverterProperty)
                    .TypeConverter()
                    .EndTypeConverter()
                    .ReadOnly();

                builder.Property(x => x.Property1)
                    .TypeConverter()
                        .ConvertToRule((value, culture) => value.ToString(culture) + "*")
                        .ConvertToRule(x => (int)x * 10)

                        .ConvertFromRule((string x, CultureInfo culture, EntityWithTypeConverter context) => { Assert.IsNotNull(context); return double.Parse(x.Replace("*", string.Empty), culture); })
                        .ConvertFromRule((long x) => x / 10)
                        .StandardValuesProvider(() => new double[] { 9, 13, 117 }, true)
                    .EndTypeConverter();

                builder.Property(x => x.Property2)
                    .TypeConverter()
                        .ConvertToRule(x => (uint)x * 100)
                        .ConvertFromRule((ulong x) => x / 100)
                        .ConvertFromNullRule(() => 13)
                    .EndTypeConverter();

                builder.Property(x => x.Property3)
                    .TypeConverter()
                        .ConvertToRule((value, culture, context) => { Assert.IsNotNull(context); return value.ToString(culture) + "*"; })
                        .ConvertFromRule((string x, CultureInfo culture) => double.Parse(x.Replace("*", string.Empty), culture))
                        .ConvertFromNullRule(culture => double.Parse("9,5", culture))
                        .StandardValuesProvider(context => { Assert.IsNotNull(context); return new double[] { 9, 13, 117 }; }, true)
                    .EndTypeConverter();

                builder.Property(x => x.Property4)
                    .TypeConverter<TestTypeConverter>()
                    .TypeConverter()
                        .ConvertFromNullRule((culture, context) => { Assert.IsNotNull(context); return double.Parse("9,5", culture); })
                    .EndTypeConverter();

                builder.Property(x => x.Property5)
                    .TypeConverter<TestTypeConverter>()
                    .TypeConverter()
                        .PropertiesProvider(() => TypeDescriptor.GetProperties(typeof(TestObj)).Cast<PropertyDescriptor>())
                    .EndTypeConverter();
            }

            public EntityWithTypeConverter() {
                Property5 = new TestObj();
            }

            public string NoTypeConverterProperty { get; set; }
            public string EmptyTypeConverterProperty { get; set; }
            public double Property1 { get; set; }
            public double Property2 { get; set; }
            public double Property3 { get; set; }
            public double Property4 { get; set; }
            public TestObj Property5 { get; set; }
        }
        public class TestObj {
            public double Property1 { get; set; }
            public string Property2 { get; set; }
        }

        [Test]
        public void TypeConverterBuilderTest_BaseCalls() {
            TestTypeConverter baseConverter = new TestTypeConverter();
            var converter = GetTypeConverterWrapper<EntityWithTypeConverter>("Property1").WrapTypeConverter(baseConverter);

            converter.CreateInstance(new TestTypeDescriptorContext(), new Hashtable());
            Assert.AreEqual(1, baseConverter.CreateInstanceCallCount);

            converter.GetCreateInstanceSupported(new TestTypeDescriptorContext());
            Assert.AreEqual(1, baseConverter.GetCreateInstanceSupportedCallCount);

            converter.GetProperties(new TestTypeDescriptorContext(), 9, new Attribute[0]);
            Assert.AreEqual(1, baseConverter.GetPropertiesCallCount);

            converter.GetPropertiesSupported(new TestTypeDescriptorContext());
            Assert.AreEqual(1, baseConverter.GetPropertiesSupportedCallCount);

            converter.IsValid(new TestTypeDescriptorContext(), 9);
            Assert.AreEqual(1, baseConverter.IsValidCallCount);
        }

        [Test]
        public void TypeConverterBuilderTest_GetProperties() {
            var converter1 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property5").WrapTypeConverter(null).IsNotNull();
            var obj = new EntityWithTypeConverter();
            converter1
                .IsTrue(x => x.GetPropertiesSupported())
                .IsTrue(x => x.GetProperties(obj.Property5).Cast<PropertyDescriptor>().Select(p => p.Name).SequenceEqual(new string[] { "Property1", "Property2" }));
        }

        [Test]
        public void TypeConverterBuilderTest_StandardValues() {
            var converter1 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property1").WrapTypeConverter(null).IsNotNull();
            converter1
                .IsTrue(x => x.GetStandardValuesSupported())
                .IsTrue(x => x.GetStandardValuesExclusive())
                .IsTrue(x => x.GetStandardValues().Cast<double>().SequenceEqual(new double[] { 9, 13, 117 }));

            var converter2 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property2").WrapTypeConverter(converter1).IsNotNull();
            converter2
                .IsTrue(x => x.GetStandardValues().Cast<double>().SequenceEqual(new double[] { 9, 13, 117 }))
                .IsTrue(x => x.GetStandardValuesExclusive());

            var converter2_ = GetTypeConverterWrapper<EntityWithTypeConverter>("Property2").WrapTypeConverter(null).IsNotNull();
            converter2_
                .IsFalse(x => x.GetStandardValuesSupported())
                .IsNull(x => x.GetStandardValues())
                .IsFalse(x => x.GetStandardValuesExclusive());

            var converter3 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property3").WrapTypeConverter(null).IsNotNull();
            converter3
                .IsTrue(x => x.GetStandardValues(new TestTypeDescriptorContext()).Cast<double>().SequenceEqual(new double[] { 9, 13, 117 }))
                .IsTrue(x => x.GetStandardValuesExclusive());

            var converter2__ = GetTypeConverterWrapper<EntityWithTypeConverter>("Property2").WrapTypeConverter(converter3).IsNotNull();
            converter2__
                .IsTrue(x => x.GetStandardValues(new TestTypeDescriptorContext()).Cast<double>().SequenceEqual(new double[] { 9, 13, 117 }));
        }

        [Test]
        public void TypeConverterBuilderTest_ConvertFrom() {
            var converter1 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property1").WrapTypeConverter(null).IsNotNull();
            converter1
                .IsFalse(x => x.CanConvertFrom(typeof(int)))
                .IsTrue(x => x.CanConvertFrom(typeof(long)))

                .AreEqual(x => x.ConvertFrom((long)90), 9d)
                .AreEqual(x => x.ConvertFrom(new TestTypeDescriptorContext(), CultureInfo.GetCultureInfo("RU-ru"), "9,5*"), 9.5d);

            var converter2 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property2").WrapTypeConverter(converter1).IsNotNull();
            converter2
                .IsTrue(x => x.CanConvertFrom(typeof(long)))
                .IsTrue(x => x.CanConvertFrom(typeof(ulong)))
                .IsFalse(x => x.CanConvertFrom(typeof(object)))
                .AreEqual(x => x.ConvertFrom((ulong)100), 1d)
                .AreEqual(x => x.ConvertFrom((long)90), 9d)
                .AreEqual(x => x.ConvertFrom(new TestTypeDescriptorContext(), CultureInfo.GetCultureInfo("RU-ru"), "9,5*"), 9.5d)
                .AreEqual(x => x.ConvertFrom(null), 13d);

            var converter2_ = GetTypeConverterWrapper<EntityWithTypeConverter>("Property2").WrapTypeConverter(null).IsNotNull();
            converter2_
                .IsFalse(x => x.CanConvertFrom(typeof(long)))
                .IsTrue(x => x.CanConvertFrom(typeof(ulong)));

            var converter3 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property3").WrapTypeConverter(null).IsNotNull();
            converter3
                .AreEqual(x => x.ConvertFrom(null, CultureInfo.GetCultureInfo("RU-ru"), "9,5*"), 9.5d)
                .AreEqual(x => x.ConvertFrom(null, CultureInfo.GetCultureInfo("RU-ru"), null), 9.5d);

            var converter4 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property4").WrapTypeConverter(null).IsNotNull();
            converter4
                .IsTrue(x => x.CanConvertFrom(typeof(char)))
                .AreEqual(x => x.ConvertFrom(new TestTypeDescriptorContext(), CultureInfo.GetCultureInfo("RU-ru"), null), 9.5d);
        }

        [Test]
        public void TypeConverterBuilderTest_ConvertFromNull() {
            var converter1 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property1").WrapTypeConverter(null);
            Assert.Throws<NotSupportedException>(() => { converter1.ConvertFrom(null); });
        }

        [Test]
        public void TypeConverterBuilderTest_ConvertTo() {
            var converter1 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property1").WrapTypeConverter(null).IsNotNull();
            converter1
                .IsTrue(x => x.CanConvertTo(typeof(string)))
                .IsTrue(x => x.CanConvertTo(typeof(int)))
                .IsFalse(x => x.CanConvertTo(typeof(uint)))
                .IsFalse(x => x.CanConvertTo(GetType()))

                .AreEqual(x => x.ConvertTo(9d, typeof(string)), "9*")
                .AreEqual(x => x.ConvertTo(null, CultureInfo.GetCultureInfo("RU-ru"), 9.5d, typeof(string)), "9,5*")
                .AreEqual(x => x.ConvertTo(9d, typeof(int)), 90);

            var converter2 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property2").WrapTypeConverter(converter1).IsNotNull();
            converter2
                .IsTrue(x => x.CanConvertTo(typeof(int)))
                .IsTrue(x => x.CanConvertTo(typeof(uint)))
                .IsFalse(x => x.CanConvertTo(typeof(object)))
                .AreEqual(x => x.ConvertTo(9d, typeof(string)), "9*")
                .AreEqual(x => x.ConvertTo(null, CultureInfo.GetCultureInfo("RU-ru"), 9.5d, typeof(string)), "9,5*")
                .AreEqual(x => x.ConvertTo(9d, typeof(int)), 90)
                .AreEqual(x => x.ConvertTo(9d, typeof(uint)), (uint)900);

            var converter2_ = GetTypeConverterWrapper<EntityWithTypeConverter>("Property2").WrapTypeConverter(null).IsNotNull();
            converter2_
                .IsTrue(x => x.CanConvertTo(typeof(string)))
                .IsTrue(x => x.CanConvertTo(typeof(uint)))
                .IsFalse(x => x.CanConvertTo(typeof(object)))
                .AreEqual(x => x.ConvertTo(9d, typeof(string)), "9");

            var converter3 = GetTypeConverterWrapper<EntityWithTypeConverter>("Property3").WrapTypeConverter(null).IsNotNull();
            converter3
                .AreEqual(x => x.ConvertTo(new TestTypeDescriptorContext(), CultureInfo.GetCultureInfo("RU-ru"), 9.5d, typeof(string)), "9,5*");
        }

        [Test]
        public void TypeConverterBuilderTest_Empty() {
            GetTypeConverterWrapper<EntityWithTypeConverter>("NoTypeConverterProperty").IsNull().WrapTypeConverter(null).IsNull();
            TypeConverter baseConverter = new TypeConverter();
            GetTypeConverterWrapper<EntityWithTypeConverter>("EmptyTypeConverterProperty").IsNull().WrapTypeConverter(baseConverter).AreEqual(baseConverter);
            GetExternalAndFluentAPIAttributes(typeof(EntityWithTypeConverter), "EmptyTypeConverterProperty").OfType<ReadOnlyAttribute>().Single();
        }
        TypeConverterWrapperAttribute GetTypeConverterWrapper<TEntity>(string propertyName) {
            return GetExternalAndFluentAPIAttributes(typeof(TEntity), propertyName).OfType<TypeConverterWrapperAttribute>().SingleOrDefault();
        }
        #endregion

        #region T370995
        public class T370995_BaseClass {
            public string Test { get; set; }
        }
        public class T370995_JustClass : T370995_BaseClass { }
        public class T370995_BaseClassMetadata : IMetadataProvider<T370995_BaseClass> {
            void IMetadataProvider<T370995_BaseClass>.BuildMetadata(MetadataBuilder<T370995_BaseClass> builder) {
                builder.Property(x => x.Test).DisplayName("DisplayName1");
            }
        }
        public class T370995_JustClassMetadata : IMetadataProvider<T370995_JustClass> {
            void IMetadataProvider<T370995_JustClass>.BuildMetadata(MetadataBuilder<T370995_JustClass> builder) {
                builder.Property(x => x.Test).DisplayName("DisplayName2");
            }
        }
        [Test]
        public void T370995() {
            try {
                MetadataLocator.Default = MetadataLocator.Create().AddMetadata<T370995_JustClassMetadata>();
                var res = GetExternalAndFluentAPIAttributes(typeof(T370995_JustClass), "Test");
                Assert.AreEqual("DisplayName2", ((DisplayAttribute)res.First()).Name);
            } finally { MetadataLocator.Default = null; }
        }
        [Test]
        public void T370995_2() {
            try {
                MetadataLocator.Default = MetadataLocator.Create()
                    .AddMetadata<T370995_BaseClassMetadata>()
                    .AddMetadata<T370995_JustClassMetadata>();
                var res = GetExternalAndFluentAPIAttributes(typeof(T370995_JustClass), "Test");
                Assert.AreEqual("DisplayName2", ((DisplayAttribute)res.First()).Name);
            } finally { MetadataLocator.Default = null; }
        }
        #endregion
#endif
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
#if !FREE
        [Test]
        public void FilterRangeAttributeProxyTest01() {
            FilterRangeAttributeProxy proxy = new FilterRangeAttributeProxy() {
                MinOrMinMember = "Min", MaxOrMaxMember = "Max",
                FromName = "FromName", ToName = "ToName",
                EditorType = FilterRangeUIEditorType.Spin,
            };
            FilterRangeAttribute attr = ((IAttributeProxy)proxy).CreateRealAttribute() as FilterRangeAttribute;
            attr.AreEqual(x => x.MinimumMember, "Min").AreEqual(x => x.MaximumMember, "Max");
            attr.AreEqual(x => x.FromName, "FromName").AreEqual(x => x.ToName, "ToName");
            attr.EditorType.AreEqual(RangeUIEditorType.Spin);
        }
        [Test]
        public void FilterRangeAttributeProxyTest02() {
            FilterRangeAttributeProxy proxy = new FilterRangeAttributeProxy() {
                MinOrMinMember = 5, MaxOrMaxMember = 10,
                FromName = "FromName", ToName = "ToName",
                EditorType = FilterRangeUIEditorType.Default,
            };
            FilterRangeAttribute attr = ((IAttributeProxy)proxy).CreateRealAttribute() as FilterRangeAttribute;
            attr.AreEqual(x => x.Minimum, 5).AreEqual(x => x.Maximum, 10);
            attr.AreEqual(x => x.FromName, "FromName").AreEqual(x => x.ToName, "ToName");
            attr.EditorType.AreEqual(RangeUIEditorType.Default);
        }
        [Test]
        public void FilterDateTimeRangeAttributeProxyTest01() {
            FilterDateTimeRangeAttributeProxy proxy = new FilterDateTimeRangeAttributeProxy() {
                MinOrMinMember = "Min", MaxOrMaxMember = "Max",
                FromName = "FromName", ToName = "ToName",
                EditorType = FilterDateTimeRangeUIEditorType.Picker,
            };
            FilterDateTimeRangeAttribute attr = ((IAttributeProxy)proxy).CreateRealAttribute() as FilterDateTimeRangeAttribute;
            attr.AreEqual(x => x.MinimumMember, "Min").AreEqual(x => x.MaximumMember, "Max");
            attr.AreEqual(x => x.FromName, "FromName").AreEqual(x => x.ToName, "ToName");
            attr.EditorType.AreEqual(DateTimeRangeUIEditorType.Picker);
        }
        [Test]
        public void FilterDateTimeRangeAttributeProxyTest02() {
            FilterDateTimeRangeAttributeProxy proxy = new FilterDateTimeRangeAttributeProxy() {
                MinOrMinMember = new DateTime(2000, 1, 1), MaxOrMaxMember = new DateTime(2001, 1, 1),
                FromName = "FromName", ToName = "ToName",
                EditorType = FilterDateTimeRangeUIEditorType.Picker,
            };
            FilterDateTimeRangeAttribute attr = ((IAttributeProxy)proxy).CreateRealAttribute() as FilterDateTimeRangeAttribute;
            attr.AreEqual(x => x.Minimum, new DateTime(2000, 1, 1)).AreEqual(x => x.Maximum, new DateTime(2001, 1, 1));
            attr.AreEqual(x => x.FromName, "FromName").AreEqual(x => x.ToName, "ToName");
            attr.EditorType.AreEqual(DateTimeRangeUIEditorType.Picker);
        }
        [Test]
        public void FilterLookupAttributeProxyTest01() {
            FilterLookupAttributeProxy proxy = new FilterLookupAttributeProxy() {
                DataSourceOrDataSourceMember = "DataSource",
                DisplayMember = "DisplayMember", ValueMember = "ValueMember",
                TopOrTopMember = 5, MaxCountOrMaxCountMember = 10,
                SelectAllName = "SelectAllName", UseSelectAll = true,
                EditorType = FilterLookupUIEditorType.TokenBox,
            };
            FilterLookupAttribute attr = ((IAttributeProxy)proxy).CreateRealAttribute() as FilterLookupAttribute;
            attr.AreEqual(x => x.DataSourceMember, "DataSource");
            attr.AreEqual(x => x.DisplayMember, "DisplayMember").AreEqual(x => x.ValueMember, "ValueMember");
            attr.AreEqual(x => x.Top, 5).AreEqual(x => x.MaxCount, 10);
            attr.AreEqual(x => x.SelectAllName, "SelectAllName");
            attr.AreEqual(x => x.UseSelectAll, true);
            attr.EditorType.AreEqual(LookupUIEditorType.TokenBox);
        }
        [Test]
        public void FilterLookupAttributeProxyTest02() {
            FilterLookupAttributeProxy proxy = new FilterLookupAttributeProxy() {
                DataSourceOrDataSourceMember = "DataSource",
                DisplayMember = "DisplayMember", ValueMember = "ValueMember",
                TopOrTopMember = "TopMember", MaxCountOrMaxCountMember = "MaxCountMember",
                SelectAllName = "SelectAllName", UseSelectAll = true,
                EditorType = FilterLookupUIEditorType.TokenBox,
            };
            FilterLookupAttribute attr = ((IAttributeProxy)proxy).CreateRealAttribute() as FilterLookupAttribute;
            attr.AreEqual(x => x.DataSourceMember, "DataSource");
            attr.AreEqual(x => x.DisplayMember, "DisplayMember").AreEqual(x => x.ValueMember, "ValueMember");
            attr.AreEqual(x => x.TopMember, "TopMember").AreEqual(x => x.MaxCountMember, "MaxCountMember");
            attr.AreEqual(x => x.SelectAllName, "SelectAllName");
            attr.AreEqual(x => x.UseSelectAll, true);
            attr.EditorType.AreEqual(LookupUIEditorType.TokenBox);
        }
        [Test]
        public void FilterEnumChoiceAttributeProxyTest() {
            FilterEnumChoiceAttributeProxy proxy = new FilterEnumChoiceAttributeProxy() {
                SelectAllName = "SelectAllName", UseSelectAll = true, UseFlags = false,
                EditorType = FilterLookupUIEditorType.TokenBox,
            };
            FilterEnumChoiceAttribute attr = ((IAttributeProxy)proxy).CreateRealAttribute() as FilterEnumChoiceAttribute;
            attr.AreEqual(x => x.SelectAllName, "SelectAllName");
            attr.AreEqual(x => x.UseSelectAll, true);
            attr.EditorType.AreEqual(LookupUIEditorType.TokenBox);
        }
        [Test]
        public void FilterBooleanChoiceAttributeProxy() {
            FilterBooleanChoiceAttributeProxy proxy = new FilterBooleanChoiceAttributeProxy() {
                EditorType = FilterBooleanUIEditorType.List,
                DefaultName = "DefaultName", TrueName = "TrueName", FalseName = "FalseName"
            };
            FilterBooleanChoiceAttribute attr = ((IAttributeProxy)proxy).CreateRealAttribute() as FilterBooleanChoiceAttribute;
            attr.AreEqual(x => x.DefaultName, "DefaultName")
                .AreEqual(x => x.TrueName, "TrueName").AreEqual(x => x.FalseName, "FalseName");
            attr.EditorType.AreEqual(BooleanUIEditorType.List);
        }


        public class FilteringVM {
            public static void BuildMetadata(MetadataBuilder<FilteringVM> builder) {
                builder.Property(x => x.Price).DisplayName("A");
                builder.Property(x => x.Category).EnumDataType(typeof(DataType));
            }
            public static void BuildMetadata(FilteringMetadataBuilder<FilteringVM> builder) {
                builder.Property(x => x.Price).DisplayName("B")
                    .FilterRange(minOrMinMember: 5, maxOrMaxMember: 10,
                    fromName: "FromName", toName: "ToName", editorType: FilterRangeUIEditorType.Spin);
                builder.Property(x => x.Category)
                    .FilterEnumChoice(editorType: FilterLookupUIEditorType.List, enumDataType: typeof(Visibility));
                builder.Property(x => x.BooleanProp)
                    .FilterBooleanChoice("dd", "tt", "ff", FilterBooleanUIEditorType.Toggle, true, "test");

            }
            public double Price { get; set; }
            public int Category { get; set; }
            public bool BooleanProp { get; set; }
        }
        [Test]
        public void FilterRangeFluentAPITest() {
            var attributes = MetadataHelper.GetExternalAndFluentAPIFilteringAttributes(typeof(FilteringVM), "Price");
            attributes.OfType<DisplayAttribute>().Single()
                .AreEqual(x => x.Name, "B");
            attributes.OfType<FilterRangeAttribute>().Single()
                .AreEqual(x => x.Minimum, 5).AreEqual(x => x.Maximum, 10)
                .AreEqual(x => x.FromName, "FromName").AreEqual(x => x.ToName, "ToName")
                .AreEqual(x => x.EditorType, RangeUIEditorType.Spin);
            return;
        }
        [Test]
        public void FilterEnumFluentAPITest() {
            var attributes = MetadataHelper.GetExternalAndFluentAPIAttributes(typeof(FilteringVM), "Category");
            attributes.OfType<EnumDataTypeAttribute>().Single()
                .AreEqual(x => x.EnumType, typeof(DataType));

            var filterAttributes = MetadataHelper.GetExternalAndFluentAPIFilteringAttributes(typeof(FilteringVM), "Category");
            filterAttributes.OfType<FilterEnumChoiceAttribute>().Single()
                .AreEqual(x => x.EditorType, LookupUIEditorType.List);
            filterAttributes.OfType<EnumDataTypeAttribute>().Single()
                .AreEqual(x => x.EnumType, typeof(Visibility));
            return;
        }
        [Test]
        public void FilterBooleanFluentAPITest() {
            var attributes = MetadataHelper.GetExternalAndFluentAPIFilteringAttributes(typeof(FilteringVM), "BooleanProp");
            attributes.OfType<FilterBooleanChoiceAttribute>().Single()
                .AreEqual(x => x.DefaultName, "dd").AreEqual(x => x.TrueName, "tt").AreEqual(x => x.FalseName, "ff")
                .AreEqual(x => x.EditorType, BooleanUIEditorType.Toggle)
                .AreEqual(x => x.DefaultValue, true).AreEqual(x => x.DefaultValueMember, "test");
            return;
        }
#endif
    }
    [TestFixture]
    public class InternalFilteringMetadataLocatorTests : InternalMetadataLocatorTests {
        protected override bool UseFilteringAttributes { get { return true; } }
    }

#if !FREE
    [TestFixture]
    public class T364310 {
        public class Foo { }
        public class Bar : Foo { }

        public class FooMetadata : IMetadataProvider<Foo> {
            public void BuildMetadata(MetadataBuilder<Foo> builder) {
                builder.PropertyGridEditor("FOO");
            }
        }

        public class BarMetadata : IMetadataProvider<Bar> {
            public void BuildMetadata(MetadataBuilder<Bar> builder) {
                builder.PropertyGridEditor("BAR");
            }
        }

        [Test]
        public void GetExternalAndFluentAPIAttributes_ReturnsAttributeRegisteredForTheType() {
            MetadataHelper.AddMetadata<FooMetadata>();
            MetadataHelper.AddMetadata<BarMetadata>();

            var attribute = MetadataHelper
                .GetExternalAndFluentAPIAttributes(typeof(Bar), "")
                .OfType<PropertyGridEditorAttribute>()
                .FirstOrDefault();

            Assert.AreEqual("BAR", attribute.TemplateKey);
        }
    }
#endif

#if !FREE && !NETFX_CORE
    [TestFixture]
    public class DataColumnAttributesProviderTests {
        [DisplayName("Attribute")]
        public class Entity {
            [Display(
                AutoGenerateField = true,
                GroupName = "GroupAttribute",
                Name = "NameAttribute",
                ShortName = "ShortNameAttribute",
                Order = 1)]
            public string DisplayAttribute { get; set; }
            [DisplayFormat(ApplyFormatInEditMode = false, ConvertEmptyStringToNull = true, DataFormatString = "Attribute")]
            public string DisplayFormatAttribute { get; set; }
            [Editable(true)]
            public string EditableAttribute { get; set; }
        }
        public class EntityMetadataProvider : IMetadataProvider<Entity> {
            public void BuildMetadata(MetadataBuilder<Entity> builder) {
                builder.DisplayName("FluentAPI");
                builder.Property(x => x.DisplayAttribute)
                    .NotAutoGenerated()
                    .Description("DescriptionFluentAPI")
                    .LocatedAt(3);
                builder.Property(x => x.DisplayFormatAttribute).DisplayFormatString("FluentAPI", true);
                builder.Property(x => x.EditableAttribute).NotEditable();
            }
        }

        [SetUp]
        public void SetUp() {
            MetadataLocator.Default = MetadataLocator.Create().AddMetadata<EntityMetadataProvider>();
        }
        [TearDown]
        public void TearDown() {
            MetadataLocator.Default = null;
        }
        DevExpress.Entity.Model.DataColumnAttributes GetAttributes<T>(Expression<Func<T, object>> property) {
            return DevExpress.Xpf.Core.Mvvm.UI.ViewGenerator.Metadata.DataColumnAttributesProvider
                .GetAttributes(TypeDescriptor.GetProperties(typeof(T))[ExpressionHelper.GetPropertyName(property)],
                typeof(T));
        }
        [Test]
        public void DisplayAttributeTest() {
            var attributes = GetAttributes<Entity>(x => x.DisplayAttribute);
            Assert.AreEqual("GroupAttribute", attributes.GroupName);
            Assert.AreEqual("NameAttribute", attributes.Name);
            Assert.AreEqual("ShortNameAttribute", attributes.ShortName);

            Assert.AreEqual("DescriptionFluentAPI", attributes.Description);
            Assert.AreEqual(false, attributes.AutoGenerateField);
            Assert.AreEqual(3, attributes.Order);
        }
        [Test]
        public void DisplayFormatAttributeTest() {
            var attributes = GetAttributes<Entity>(x => x.DisplayFormatAttribute);
            Assert.AreEqual(true, attributes.ApplyFormatInEditMode);
            Assert.AreEqual("FluentAPI", attributes.DataFormatString);
        }
        [Test]
        public void EditableAttributeTest() {
            var attributes = GetAttributes<Entity>(x => x.EditableAttribute);
            Assert.AreEqual(false, attributes.AllowEdit);
        }
        [Test]
        public void DisplayNameTest() {
            var attr = AttributesHelper.GetAttributes(typeof(Entity));
            var res = ((DisplayNameAttribute)attr[typeof(DisplayNameAttribute)]).DisplayName;
            Assert.AreEqual("FluentAPI", ((DisplayNameAttribute)attr[typeof(DisplayNameAttribute)]).DisplayName);
        }
    }
#endif
}