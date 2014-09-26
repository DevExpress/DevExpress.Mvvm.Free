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
        #endregion

        #region numeric
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
        #endregion

        static PropertyMetadataBuilder<T, TProperty> SetDataTypeCore<T, TProperty>(this PropertyMetadataBuilder<T, TProperty> builder, PropertyDataType dataType) {
            return DataAnnotationsAttributeHelper.SetDataTypeCore(builder, dataType);
        }

        public static PropertyMetadataBuilder<T, TProperty> InRange<T, TProperty>(this PropertyMetadataBuilder<T, TProperty> builder, TProperty minimum, TProperty maximum, Func<string> errorMessageAccessor = null) where TProperty : IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, errorMessageAccessor));
        }
        public static PropertyMetadataBuilder<T, TProperty?> InRange<T, TProperty>(this PropertyMetadataBuilder<T, TProperty?> builder, TProperty? minimum, TProperty? maximum, Func<string> errorMessageAccessor = null) where TProperty : struct, IComparable {
            return builder.AddOrReplaceAttribute(new RangeAttribute(minimum, maximum, errorMessageAccessor));
        }
    }

    [CLSCompliant(false)]
    public static class NumericUnsignedPropertyMetadataBuilderExtensions {
        public static PropertyMetadataBuilder<T, uint> CurrencyDataType<T>(this PropertyMetadataBuilder<T, uint> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, uint?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, uint?> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, ushort> CurrencyDataType<T>(this PropertyMetadataBuilder<T, ushort> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, ushort?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, ushort?> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, ulong> CurrencyDataType<T>(this PropertyMetadataBuilder<T, ulong> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
        public static PropertyMetadataBuilder<T, ulong?> CurrencyDataType<T>(this PropertyMetadataBuilder<T, ulong?> builder) { return PropertyMetadataBuilderExtensions.CurrencyDataTypeCore(builder); }
    }
}