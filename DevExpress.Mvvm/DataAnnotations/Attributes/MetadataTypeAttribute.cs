using System;

namespace DevExpress.Mvvm.DataAnnotations {
#if SILVERLIGHT || NETFX_CORE
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MetadataTypeAttribute : Attribute {
        public MetadataTypeAttribute(Type metadataClassType) {
            if(Object.ReferenceEquals(metadataClassType, null))
                throw new ArgumentNullException("metadataClassType");
            this.MetadataClassType = metadataClassType;
        }
        public Type MetadataClassType { get; private set; }
    }
#endif
}