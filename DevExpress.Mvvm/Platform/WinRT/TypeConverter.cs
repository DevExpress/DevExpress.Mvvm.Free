using System;
using System.Globalization;
using System.ComponentModel;
using System.Collections;

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
    }
    public interface ITypeDescriptorContext {
    }
}