using System;
using System.Globalization;

namespace DevExpress.Mvvm.Native {
    public class TypeConverter {
        public bool CanConvertFrom(Type sourceType) {
            return this.CanConvertFrom(null, sourceType);
        }
        public virtual bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return false;
        }
        public bool CanConvertTo(Type destinationType) {
            return this.CanConvertTo(null, destinationType);
        }
        public virtual bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return false;
        }
        public object ConvertFrom(object value) {
            return this.ConvertFrom(null, CultureInfo.CurrentCulture, value);
        }
        public virtual object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            throw new NotImplementedException("ConvertFrom not implemented in base TypeConverter.");
        }
        public object ConvertFromString(string text) {
            return this.ConvertFrom(null, null, text);
        }
        public object ConvertTo(object value, Type destinationType) {
            return this.ConvertTo(null, CultureInfo.CurrentCulture, value, destinationType);
        }
        public virtual object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            throw new NotImplementedException("ConvertTo not implemented in base TypeConverter.");
        }
        public string ConvertToString(object value) {
            return (string)this.ConvertTo(null, CultureInfo.CurrentCulture, value, typeof(string));
        }
        public TypeConverter GetStandardValues() {
            throw new NotImplementedException();
        }
        public class StandardValuesCollection {
            public StandardValuesCollection(TypeConverter typeConverter) {
                throw new NotImplementedException();
            }
        }

        public string ConvertToInvariantString(ITypeDescriptorContext context, object value) {
            return (string)this.ConvertTo(context, CultureInfo.InvariantCulture, value, typeof(string));
        }
        public bool GetCreateInstanceSupported() {
            return this.GetCreateInstanceSupported(null);
        }
        public virtual bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
            return false;
        }
        public virtual bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return false;
        }
        public virtual bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return false;
        }
        public bool GetPropertiesSupported() {
            return this.GetPropertiesSupported(null);
        }
        public virtual bool GetPropertiesSupported(ITypeDescriptorContext context) {
            return false;
        }
        public object ConvertFromInvariantString(string text) {
            return this.ConvertFromString(null, CultureInfo.InvariantCulture, text);
        }
        public object ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string text) {
            return this.ConvertFrom(context, culture, text);
        }
        public string ConvertToInvariantString(object value) {
            return this.ConvertToString(null, CultureInfo.InvariantCulture, value);
        }
        public string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return (string)this.ConvertTo(context, culture, value, typeof(string));
        }
    }
    public interface ITypeDescriptorContext {
    }
}