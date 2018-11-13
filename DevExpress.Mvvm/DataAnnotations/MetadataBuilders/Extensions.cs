using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;

namespace DevExpress.Mvvm.DataAnnotations {
    public enum DateTimeDisplayMode {
        Date,
        Time,
        DateTime
    }
    public static class PropertyMetadataBuilderExtensions {
        #region string
        public static PropertyMetadataBuilder<T, string> PasswordDataType<T>(this PropertyMetadataBuilder<T, string> builder) {
            return builder.SetDataTypeCore(PropertyDataType.Password);
        }
        public static PropertyMetadataBuilder<T, string> PhoneNumberDataType<T>(this PropertyMetadataBuilder<T, string> builder, Func<string> errorMessageAccessor = null) {
            return builder.AddOrReplaceAttribute(new PhoneAttribute(errorMessageAccessor));
        }
        public static PropertyMetadataBuilder<T, string> CreditCardDataType<T>(this PropertyMetadataBuilder<T, string> builder, Func<string> errorMessageAccessor = null) {
            return builder.AddOrReplaceAttribute(new CreditCardAttribute(errorMessageAccessor));
        }
        public static PropertyMetadataBuilder<T, string> EmailAddressDataType<T>(this PropertyMetadataBuilder<T, string> builder, Func<string> errorMessageAccessor = null) {
            return builder.AddOrReplaceAttribute(new EmailAddressAttribute(errorMessageAccessor));
        }
        public static PropertyMetadataBuilder<T, string> UrlDataType<T>(this PropertyMetadataBuilder<T, string> builder, Func<string> errorMessageAccessor = null) {
            return builder.AddOrReplaceAttribute(new UrlAttribute(errorMessageAccessor));
        }
        public static PropertyMetadataBuilder<T, string> MultilineTextDataType<T>(this PropertyMetadataBuilder<T, string> builder) {
            return builder.SetDataTypeCore(PropertyDataType.MultilineText);
        }
        public static PropertyMetadataBuilder<T, string> ImageUrlDataType<T>(this PropertyMetadataBuilder<T, string> builder) {
            return builder.SetDataTypeCore(PropertyDataType.ImageUrl);
        }
#if !FREE
        public static SimpleMaskBuilder<T, string, PropertyMetadataBuilder<T, string>> SimpleMask<T>(this PropertyMetadataBuilder<T, string> builder, string mask, bool useMaskAsDisplayFormat = false) {
            return new SimpleMaskBuilder<T, string, PropertyMetadataBuilder<T, string>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }
        public static RegularMaskBuilder<T, string, PropertyMetadataBuilder<T, string>> RegularMask<T>(this PropertyMetadataBuilder<T, string> builder, string mask, bool useMaskAsDisplayFormat = false) {
            return new RegularMaskBuilder<T, string, PropertyMetadataBuilder<T, string>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }
        public static RegExMaskBuilder<T, string, PropertyMetadataBuilder<T, string>> RegExMask<T>(this PropertyMetadataBuilder<T, string> builder, string mask, bool useMaskAsDisplayFormat = false) {
            return new RegExMaskBuilder<T, string, PropertyMetadataBuilder<T, string>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }
#endif
        #endregion

        #region numeric
#if !FREE        
        public static NumericMaskBuilder<T, double, PropertyMetadataBuilder<T, double>> NumericMask<T>(this PropertyMetadataBuilder<T, double> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, double?, PropertyMetadataBuilder<T, double?>> NumericMask<T>(this PropertyMetadataBuilder<T, double?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, decimal, PropertyMetadataBuilder<T, decimal>> NumericMask<T>(this PropertyMetadataBuilder<T, decimal> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, decimal?, PropertyMetadataBuilder<T, decimal?>> NumericMask<T>(this PropertyMetadataBuilder<T, decimal?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, float, PropertyMetadataBuilder<T, float>> NumericMask<T>(this PropertyMetadataBuilder<T, float> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, float?, PropertyMetadataBuilder<T, float?>> NumericMask<T>(this PropertyMetadataBuilder<T, float?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, int, PropertyMetadataBuilder<T, int>> NumericMask<T>(this PropertyMetadataBuilder<T, int> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, int?, PropertyMetadataBuilder<T, int?>> NumericMask<T>(this PropertyMetadataBuilder<T, int?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, short, PropertyMetadataBuilder<T, short>> NumericMask<T>(this PropertyMetadataBuilder<T, short> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, short?, PropertyMetadataBuilder<T, short?>> NumericMask<T>(this PropertyMetadataBuilder<T, short?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, byte, PropertyMetadataBuilder<T, byte>> NumericMask<T>(this PropertyMetadataBuilder<T, byte> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, long, PropertyMetadataBuilder<T, long>> NumericMask<T>(this PropertyMetadataBuilder<T, long> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, long?, PropertyMetadataBuilder<T, long?>> NumericMask<T>(this PropertyMetadataBuilder<T, long?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        internal static NumericMaskBuilder<T, TProperty, PropertyMetadataBuilder<T, TProperty>> NumericMaskOptionsCore<T, TProperty>(this PropertyMetadataBuilder<T, TProperty> builder, string mask, bool useMaskAsDisplayFormat) {
            return new NumericMaskBuilder<T, TProperty, PropertyMetadataBuilder<T, TProperty>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }

        public static PropertyMetadataBuilder<T, double> CurrencyDataType<T>(this PropertyMetadataBuilder<T, double> builder) { return CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, double?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, double?> builder) { return CurrencyDataTypeCore(builder); }

        public static PropertyMetadataBuilder<T, decimal> CurrencyDataType<T>(this PropertyMetadataBuilder<T, decimal> builder) { return CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, decimal?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, decimal?> builder) { return CurrencyDataTypeCore(builder); }

        public static PropertyMetadataBuilder<T, float> CurrencyDataType<T>(this PropertyMetadataBuilder<T, float> builder) { return CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, float?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, float?> builder) { return CurrencyDataTypeCore(builder); }

        public static PropertyMetadataBuilder<T, int> CurrencyDataType<T>(this PropertyMetadataBuilder<T, int> builder) { return CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, int?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, int?> builder) { return CurrencyDataTypeCore(builder); }

        public static PropertyMetadataBuilder<T, short> CurrencyDataType<T>(this PropertyMetadataBuilder<T, short> builder) { return CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, short?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, short?> builder) { return CurrencyDataTypeCore(builder); }

        public static PropertyMetadataBuilder<T, byte> CurrencyDataType<T>(this PropertyMetadataBuilder<T, byte> builder) { return CurrencyDataTypeCore(builder); }

        public static PropertyMetadataBuilder<T, long> CurrencyDataType<T>(this PropertyMetadataBuilder<T, long> builder) { return CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, long?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, long?> builder) { return CurrencyDataTypeCore(builder); }

#endif
        internal static PropertyMetadataBuilder<T, TProperty> CurrencyDataTypeCore<T, TProperty>(this PropertyMetadataBuilder<T, TProperty> builder) {
            return builder.SetDataTypeCore(PropertyDataType.Currency);
        }
        #endregion

        #region date time
        public static PropertyMetadataBuilder<T, DateTime> DateTimeDataType<T>(this PropertyMetadataBuilder<T, DateTime> builder, DateTimeDisplayMode displayMode = DateTimeDisplayMode.Date) {
            return builder.SetDataTypeCore(GetDataTypeByDateTimeDisplayMode(displayMode));
        }
        public static PropertyMetadataBuilder<T, DateTime?> DateTimeDataType<T>(this PropertyMetadataBuilder<T, DateTime?> builder, DateTimeDisplayMode displayMode = DateTimeDisplayMode.Date) {
            return builder.SetDataTypeCore(GetDataTypeByDateTimeDisplayMode(displayMode));
        }
        static PropertyDataType GetDataTypeByDateTimeDisplayMode(DateTimeDisplayMode displayMode) {
            switch(displayMode) {
                case DateTimeDisplayMode.Date:
                    return PropertyDataType.Date;
                case DateTimeDisplayMode.Time:
                    return PropertyDataType.Time;
                case DateTimeDisplayMode.DateTime:
                    return PropertyDataType.DateTime;
                default:
                    throw new NotSupportedException();
            }
        }
#if !FREE
        public static DateTimeMaskBuilder<T, PropertyMetadataBuilder<T, DateTime>> DateTimeMask<T>(this PropertyMetadataBuilder<T, DateTime> builder, string mask, bool useMaskAsDisplayFormat = true) {
            return new DateTimeMaskBuilder<T, PropertyMetadataBuilder<T, DateTime>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }
#endif
        #endregion

        #region enum
        public static PropertyMetadataBuilder<T, int> EnumDataType<T>(this PropertyMetadataBuilder<T, int> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, int?> EnumDataType<T>(this PropertyMetadataBuilder<T, int?> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, short> EnumDataType<T>(this PropertyMetadataBuilder<T, short> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, short?> EnumDataType<T>(this PropertyMetadataBuilder<T, short?> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, byte> EnumDataType<T>(this PropertyMetadataBuilder<T, byte> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, byte?> EnumDataType<T>(this PropertyMetadataBuilder<T, byte?> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, long> EnumDataType<T>(this PropertyMetadataBuilder<T, long> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, long?> EnumDataType<T>(this PropertyMetadataBuilder<T, long?> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }

        internal static PropertyMetadataBuilder<T, TProperty> EnumDataTypeCore<T, TProperty>(this PropertyMetadataBuilder<T, TProperty> builder, Type enumType) {
            return DataAnnotationsAttributeHelper.SetEnumDataTypeCore(builder, enumType);
        }
        #endregion

        static PropertyMetadataBuilder<T, TProperty> SetDataTypeCore<T, TProperty>(this PropertyMetadataBuilder<T, TProperty> builder, PropertyDataType dataType) {
            return DataAnnotationsAttributeHelper.SetDataTypeCore(builder, dataType);
        }

        public static PropertyMetadataBuilder<T, TProperty> InRange<T, TProperty>(this PropertyMetadataBuilder<T, TProperty> builder, TProperty minimum, TProperty maximum, Func<string> errorMessageAccessor = null) where TProperty : IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor)));
        }
        public static PropertyMetadataBuilder<T, TProperty> InRange<T, TProperty>(this PropertyMetadataBuilder<T, TProperty> builder, TProperty minimum, TProperty maximum, Func<TProperty, string> errorMessageAccessor) where TProperty : IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor)));
        }
        public static PropertyMetadataBuilder<T, TProperty?> InRange<T, TProperty>(this PropertyMetadataBuilder<T, TProperty?> builder, TProperty? minimum, TProperty? maximum, Func<string> errorMessageAccessor = null) where TProperty : struct, IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor)));
        }
        public static PropertyMetadataBuilder<T, TProperty?> InRange<T, TProperty>(this PropertyMetadataBuilder<T, TProperty?> builder, TProperty? minimum, TProperty? maximum, Func<TProperty, string> errorMessageAccessor) where TProperty : struct, IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor)));
        }

#if !FREE
        public static PropertyMetadataBuilder<T, IEnumerable<TProperty>> ScaffoldDetailCollection<T, TProperty>(this PropertyMetadataBuilder<T, IEnumerable<TProperty>> builder) {
            return builder.AddOrReplaceAttribute(new ScaffoldDetailCollectionAttribute());
        }
        public static PropertyMetadataBuilder<T, ICollection<TItem>> NewItemInitializer<T, TItem, TValue>(this PropertyMetadataBuilder<T, ICollection<TItem>> builder, Func<TValue> createDelegate, string name = null) {
            return builder.InitializerCore(createDelegate, name, (t, n, c) => new NewItemInstanceInitializerAttribute(t, n, c));
        }

#endif
    }
#if !FREE
    public static class FilteringPropertyMetadataBuilderExtensions {
#region string
        public static FilteringPropertyMetadataBuilder<T, string> PasswordDataType<T>(this FilteringPropertyMetadataBuilder<T, string> builder) {
            return builder.SetDataTypeCore(PropertyDataType.Password);
        }
        public static FilteringPropertyMetadataBuilder<T, string> PhoneNumberDataType<T>(this FilteringPropertyMetadataBuilder<T, string> builder, Func<string> errorMessageAccessor = null) {
            return builder.AddOrReplaceAttribute(new PhoneAttribute(errorMessageAccessor));
        }
        public static FilteringPropertyMetadataBuilder<T, string> CreditCardDataType<T>(this FilteringPropertyMetadataBuilder<T, string> builder, Func<string> errorMessageAccessor = null) {
            return builder.AddOrReplaceAttribute(new CreditCardAttribute(errorMessageAccessor));
        }
        public static FilteringPropertyMetadataBuilder<T, string> EmailAddressDataType<T>(this FilteringPropertyMetadataBuilder<T, string> builder, Func<string> errorMessageAccessor = null) {
            return builder.AddOrReplaceAttribute(new EmailAddressAttribute(errorMessageAccessor));
        }
        public static FilteringPropertyMetadataBuilder<T, string> UrlDataType<T>(this FilteringPropertyMetadataBuilder<T, string> builder, Func<string> errorMessageAccessor = null) {
            return builder.AddOrReplaceAttribute(new UrlAttribute(errorMessageAccessor));
        }
        public static FilteringPropertyMetadataBuilder<T, string> MultilineTextDataType<T>(this FilteringPropertyMetadataBuilder<T, string> builder) {
            return builder.SetDataTypeCore(PropertyDataType.MultilineText);
        }
        public static FilteringPropertyMetadataBuilder<T, string> ImageUrlDataType<T>(this FilteringPropertyMetadataBuilder<T, string> builder) {
            return builder.SetDataTypeCore(PropertyDataType.ImageUrl);
        }
#if !FREE
        public static SimpleMaskBuilder<T, string, FilteringPropertyMetadataBuilder<T, string>> SimpleMask<T>(this FilteringPropertyMetadataBuilder<T, string> builder, string mask, bool useMaskAsDisplayFormat = false) {
            return new SimpleMaskBuilder<T, string, FilteringPropertyMetadataBuilder<T, string>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }
        public static RegularMaskBuilder<T, string, FilteringPropertyMetadataBuilder<T, string>> RegularMask<T>(this FilteringPropertyMetadataBuilder<T, string> builder, string mask, bool useMaskAsDisplayFormat = false) {
            return new RegularMaskBuilder<T, string, FilteringPropertyMetadataBuilder<T, string>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }
        public static RegExMaskBuilder<T, string, FilteringPropertyMetadataBuilder<T, string>> RegExMask<T>(this FilteringPropertyMetadataBuilder<T, string> builder, string mask, bool useMaskAsDisplayFormat = false) {
            return new RegExMaskBuilder<T, string, FilteringPropertyMetadataBuilder<T, string>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }
#endif
#endregion

#region numeric
#if !FREE
        public static NumericMaskBuilder<T, double, FilteringPropertyMetadataBuilder<T, double>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, double> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, double?, FilteringPropertyMetadataBuilder<T, double?>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, double?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, decimal, FilteringPropertyMetadataBuilder<T, decimal>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, decimal> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, decimal?, FilteringPropertyMetadataBuilder<T, decimal?>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, decimal?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, float, FilteringPropertyMetadataBuilder<T, float>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, float> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, float?, FilteringPropertyMetadataBuilder<T, float?>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, float?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, int, FilteringPropertyMetadataBuilder<T, int>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, int> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, int?, FilteringPropertyMetadataBuilder<T, int?>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, int?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, short, FilteringPropertyMetadataBuilder<T, short>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, short> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, short?, FilteringPropertyMetadataBuilder<T, short?>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, short?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, byte, FilteringPropertyMetadataBuilder<T, byte>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, byte> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        public static NumericMaskBuilder<T, long, FilteringPropertyMetadataBuilder<T, long>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, long> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, long?, FilteringPropertyMetadataBuilder<T, long?>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, long?> builder, string mask, bool useMaskAsDisplayFormat = true) { return NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }

        internal static NumericMaskBuilder<T, TProperty, FilteringPropertyMetadataBuilder<T, TProperty>> NumericMaskOptionsCore<T, TProperty>(this FilteringPropertyMetadataBuilder<T, TProperty> builder, string mask, bool useMaskAsDisplayFormat) {
            return new NumericMaskBuilder<T, TProperty, FilteringPropertyMetadataBuilder<T, TProperty>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }

        public static FilteringPropertyMetadataBuilder<T, double> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, double> builder) { return CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, double?> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, double?> builder) { return CurrencyDataTypeCore(builder); }

        public static FilteringPropertyMetadataBuilder<T, decimal> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, decimal> builder) { return CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, decimal?> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, decimal?> builder) { return CurrencyDataTypeCore(builder); }

        public static FilteringPropertyMetadataBuilder<T, float> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, float> builder) { return CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, float?> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, float?> builder) { return CurrencyDataTypeCore(builder); }

        public static FilteringPropertyMetadataBuilder<T, int> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, int> builder) { return CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, int?> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, int?> builder) { return CurrencyDataTypeCore(builder); }

        public static FilteringPropertyMetadataBuilder<T, short> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, short> builder) { return CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, short?> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, short?> builder) { return CurrencyDataTypeCore(builder); }

        public static FilteringPropertyMetadataBuilder<T, byte> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, byte> builder) { return CurrencyDataTypeCore(builder); }

        public static FilteringPropertyMetadataBuilder<T, long> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, long> builder) { return CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, long?> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, long?> builder) { return CurrencyDataTypeCore(builder); }

#endif
        internal static FilteringPropertyMetadataBuilder<T, TProperty> CurrencyDataTypeCore<T, TProperty>(this FilteringPropertyMetadataBuilder<T, TProperty> builder) {
            return builder.SetDataTypeCore(PropertyDataType.Currency);
        }
#endregion

#region date time
        public static FilteringPropertyMetadataBuilder<T, DateTime> DateTimeDataType<T>(this FilteringPropertyMetadataBuilder<T, DateTime> builder, DateTimeDisplayMode displayMode = DateTimeDisplayMode.Date) {
            return builder.SetDataTypeCore(GetDataTypeByDateTimeDisplayMode(displayMode));
        }
        public static FilteringPropertyMetadataBuilder<T, DateTime?> DateTimeDataType<T>(this FilteringPropertyMetadataBuilder<T, DateTime?> builder, DateTimeDisplayMode displayMode = DateTimeDisplayMode.Date) {
            return builder.SetDataTypeCore(GetDataTypeByDateTimeDisplayMode(displayMode));
        }
        static PropertyDataType GetDataTypeByDateTimeDisplayMode(DateTimeDisplayMode displayMode) {
            switch(displayMode) {
                case DateTimeDisplayMode.Date:
                    return PropertyDataType.Date;
                case DateTimeDisplayMode.Time:
                    return PropertyDataType.Time;
                case DateTimeDisplayMode.DateTime:
                    return PropertyDataType.DateTime;
                default:
                    throw new NotSupportedException();
            }
        }
#if !FREE
        public static DateTimeMaskBuilder<T, FilteringPropertyMetadataBuilder<T, DateTime>> DateTimeMask<T>(this FilteringPropertyMetadataBuilder<T, DateTime> builder, string mask, bool useMaskAsDisplayFormat = true) {
            return new DateTimeMaskBuilder<T, FilteringPropertyMetadataBuilder<T, DateTime>>(builder).MaskCore(mask, useMaskAsDisplayFormat);
        }
#endif
        #endregion

        #region enum
        public static FilteringPropertyMetadataBuilder<T, int> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, int> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, int?> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, int?> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, short> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, short> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, short?> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, short?> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, byte> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, byte> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, byte?> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, byte?> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, long> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, long> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, long?> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, long?> builder, Type enumType) { return EnumDataTypeCore(builder, enumType); }

        internal static FilteringPropertyMetadataBuilder<T, TProperty> EnumDataTypeCore<T, TProperty>(this FilteringPropertyMetadataBuilder<T, TProperty> builder, Type enumType) {
            return DataAnnotationsAttributeHelper.SetEnumDataTypeCore(builder, enumType);
        }
        #endregion

        static FilteringPropertyMetadataBuilder<T, TProperty> SetDataTypeCore<T, TProperty>(this FilteringPropertyMetadataBuilder<T, TProperty> builder, PropertyDataType dataType) {
            return DataAnnotationsAttributeHelper.SetDataTypeCore(builder, dataType);
        }

        public static FilteringPropertyMetadataBuilder<T, TProperty> InRange<T, TProperty>(this FilteringPropertyMetadataBuilder<T, TProperty> builder, TProperty minimum, TProperty maximum, Func<string> errorMessageAccessor = null) where TProperty : IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor)));
        }
        public static FilteringPropertyMetadataBuilder<T, TProperty> InRange<T, TProperty>(this FilteringPropertyMetadataBuilder<T, TProperty> builder, TProperty minimum, TProperty maximum, Func<TProperty, string> errorMessageAccessor) where TProperty : IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor)));
        }
        public static FilteringPropertyMetadataBuilder<T, TProperty?> InRange<T, TProperty>(this FilteringPropertyMetadataBuilder<T, TProperty?> builder, TProperty? minimum, TProperty? maximum, Func<string> errorMessageAccessor = null) where TProperty : struct, IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor)));
        }
        public static FilteringPropertyMetadataBuilder<T, TProperty?> InRange<T, TProperty>(this FilteringPropertyMetadataBuilder<T, TProperty?> builder, TProperty? minimum, TProperty? maximum, Func<TProperty, string> errorMessageAccessor) where TProperty : struct, IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, DXValidationAttribute.ErrorMessageAccessor(errorMessageAccessor)));
        }
#if !FREE
        public static FilteringPropertyMetadataBuilder<T, IEnumerable<TProperty>> ScaffoldDetailCollection<T, TProperty>(this FilteringPropertyMetadataBuilder<T, IEnumerable<TProperty>> builder) {
            return builder.AddOrReplaceAttribute(new ScaffoldDetailCollectionAttribute());
        }
        public static FilteringPropertyMetadataBuilder<T, ICollection<TItem>> NewItemInitializer<T, TItem, TValue>(this FilteringPropertyMetadataBuilder<T, ICollection<TItem>> builder, Func<TValue> createDelegate, string name = null) {
            return builder.InitializerCore(createDelegate, name, (t, n, c) => new NewItemInstanceInitializerAttribute(t, n, c));
        }

#endif
    }
#endif
    [CLSCompliant(false)]
    public static class NumericUnsignedPropertyMetadataBuilderExtensions {
        public static PropertyMetadataBuilder<T, uint> CurrencyDataType<T>(this PropertyMetadataBuilder<T, uint> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, uint?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, uint?> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, ushort> CurrencyDataType<T>(this PropertyMetadataBuilder<T, ushort> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, ushort?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, ushort?> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, ulong> CurrencyDataType<T>(this PropertyMetadataBuilder<T, ulong> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, ulong?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, ulong?> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }

        public static PropertyMetadataBuilder<T, uint> EnumDataType<T>(this PropertyMetadataBuilder<T, uint> builder, Type enumType) { return PropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, uint?> EnumDataType<T>(this PropertyMetadataBuilder<T, uint?> builder, Type enumType) { return PropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, ushort> EnumDataType<T>(this PropertyMetadataBuilder<T, ushort> builder, Type enumType) { return PropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, ushort?> EnumDataType<T>(this PropertyMetadataBuilder<T, ushort?> builder, Type enumType) { return PropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, ulong> EnumDataType<T>(this PropertyMetadataBuilder<T, ulong> builder, Type enumType) { return PropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static PropertyMetadataBuilder<T, ulong?> EnumDataType<T>(this PropertyMetadataBuilder<T, ulong?> builder, Type enumType) { return PropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
#if !FREE
        public static NumericMaskBuilder<T, uint, PropertyMetadataBuilder<T, uint>> NumericMask<T>(this PropertyMetadataBuilder<T, uint> builder, string mask, bool useMaskAsDisplayFormat = true) { return PropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, uint?, PropertyMetadataBuilder<T, uint?>> NumericMask<T>(this PropertyMetadataBuilder<T, uint?> builder, string mask, bool useMaskAsDisplayFormat = true) { return PropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, ushort, PropertyMetadataBuilder<T, ushort>> NumericMask<T>(this PropertyMetadataBuilder<T, ushort> builder, string mask, bool useMaskAsDisplayFormat = true) { return PropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, ushort?, PropertyMetadataBuilder<T, ushort?>> NumericMask<T>(this PropertyMetadataBuilder<T, ushort?> builder, string mask, bool useMaskAsDisplayFormat = true) { return PropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, ulong, PropertyMetadataBuilder<T, ulong>> NumericMask<T>(this PropertyMetadataBuilder<T, ulong> builder, string mask, bool useMaskAsDisplayFormat = true) { return PropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, ulong?, PropertyMetadataBuilder<T, ulong?>> NumericMask<T>(this PropertyMetadataBuilder<T, ulong?> builder, string mask, bool useMaskAsDisplayFormat = true) { return PropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
#endif
    }
#if !FREE
    [CLSCompliant(false)]
    public static class NumericUnsignedFilteringPropertyMetadataBuilderExtensions {
        public static FilteringPropertyMetadataBuilder<T, uint> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, uint> builder) { return FilteringPropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, uint?> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, uint?> builder) { return FilteringPropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, ushort> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, ushort> builder) { return FilteringPropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, ushort?> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, ushort?> builder) { return FilteringPropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, ulong> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, ulong> builder) { return FilteringPropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static FilteringPropertyMetadataBuilder<T, ulong?> CurrencyDataType<T>(this FilteringPropertyMetadataBuilder<T, ulong?> builder) { return FilteringPropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }

        public static FilteringPropertyMetadataBuilder<T, uint> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, uint> builder, Type enumType) { return FilteringPropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, uint?> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, uint?> builder, Type enumType) { return FilteringPropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, ushort> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, ushort> builder, Type enumType) { return FilteringPropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, ushort?> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, ushort?> builder, Type enumType) { return FilteringPropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, ulong> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, ulong> builder, Type enumType) { return FilteringPropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
        public static FilteringPropertyMetadataBuilder<T, ulong?> EnumDataType<T>(this FilteringPropertyMetadataBuilder<T, ulong?> builder, Type enumType) { return FilteringPropertyMetadataBuilderExtensions.EnumDataTypeCore(builder, enumType); }
#if !FREE
        public static NumericMaskBuilder<T, uint, FilteringPropertyMetadataBuilder<T, uint>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, uint> builder, string mask, bool useMaskAsDisplayFormat = true) { return FilteringPropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, uint?, FilteringPropertyMetadataBuilder<T, uint?>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, uint?> builder, string mask, bool useMaskAsDisplayFormat = true) { return FilteringPropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, ushort, FilteringPropertyMetadataBuilder<T, ushort>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, ushort> builder, string mask, bool useMaskAsDisplayFormat = true) { return FilteringPropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, ushort?, FilteringPropertyMetadataBuilder<T, ushort?>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, ushort?> builder, string mask, bool useMaskAsDisplayFormat = true) { return FilteringPropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, ulong, FilteringPropertyMetadataBuilder<T, ulong>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, ulong> builder, string mask, bool useMaskAsDisplayFormat = true) { return FilteringPropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
        public static NumericMaskBuilder<T, ulong?, FilteringPropertyMetadataBuilder<T, ulong?>> NumericMask<T>(this FilteringPropertyMetadataBuilder<T, ulong?> builder, string mask, bool useMaskAsDisplayFormat = true) { return FilteringPropertyMetadataBuilderExtensions.NumericMaskOptionsCore(builder, mask, useMaskAsDisplayFormat); }
#endif
    }
#endif
}
