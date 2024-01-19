using System;

namespace DevExpress.Mvvm.DataAnnotations {
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public sealed class EnumMetadataTypeAttribute : Attribute {
        public Type MetadataClassType { get; private set; }

        public EnumMetadataTypeAttribute(Type metadataClassType) {
            this.MetadataClassType = metadataClassType;
        }

    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false)]
    public class ImageAttribute : Attribute {
        public ImageAttribute() {
        }
        public ImageAttribute(string imageUri) {
            this.ImageUri = imageUri;
        }
        public string ImageUri { get; internal set; }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false)]
    internal
    class DXImageAttribute : Attribute {
        public DXImageAttribute() {
        }
        public DXImageAttribute(string imageName) {
            this.ImageName = imageName;
        }
        public string ImageName { get; internal set; }
        public string LargeImageUri { get; set; }
        public string SmallImageUri { get; set; }
    }
}