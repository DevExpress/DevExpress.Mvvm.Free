using System;

namespace DevExpress.Mvvm.Native {
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public sealed class TypeConverterAttribute : Attribute {
        public TypeConverterAttribute() { }
        public TypeConverterAttribute(Type type) { }
        public string ConverterTypeName { get; private set; }
    }
}