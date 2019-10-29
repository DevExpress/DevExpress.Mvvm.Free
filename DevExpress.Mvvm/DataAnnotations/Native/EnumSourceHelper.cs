using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DevExpress.Mvvm.Native {
    public static class EnumSourceHelperCore {
        public static readonly string ValueMemberName = BindableBase.GetPropertyName(() => new EnumMemberInfo(null, null, null, null).Id);
        public static readonly string DisplayMemberName = BindableBase.GetPropertyName(() => new EnumMemberInfo(null, null, null, null).Name);
        const int DefaultDisplayOrder = 10000;
        public static int GetEnumCount(Type enumType) {
            return Enum.GetValues(enumType).Length;
        }
        public static IEnumerable<EnumMemberInfo> GetEnumSource(Type enumType, bool useUnderlyingEnumValue = true, IValueConverter nameConverter = null,
            bool splitNames = false, EnumMembersSortMode sortMode = EnumMembersSortMode.Default, Func<string, bool, string> getKnownImageUriCallback = null,
            bool showImage = true, bool showName = true, Func<string, ImageSource> getSvgImageSource = null) {
            if(enumType == null || !enumType.IsEnum)
                return Enumerable.Empty<EnumMemberInfo>();
            Func<string, ImageSource> getImageSource = uri => (ImageSource)new ImageSourceConverter().ConvertFrom(uri);
            var result = enumType.GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(field => DataAnnotationsAttributeHelper.GetAutoGenerateField(field))
                .Select(field => {
                    Enum value = (Enum)field.GetValue(null);
                    string name = GetEnumName(field, value, nameConverter, splitNames);

                    var imageInfo = GetImageInfo(MetadataHelper.GetAttribute<ImageAttribute>(field), MetadataHelper.GetAttribute<DXImageAttribute>(field), null, getKnownImageUriCallback);
                    string imageUri = ViewModelBase.IsInDesignMode ? null : (imageInfo.Item1 ?? imageInfo.Item2);
                    Func<ImageSource> getImage = () => imageUri.With(TryGetImageSource(CanCreateSvgImageSource(getSvgImageSource, imageUri)
                        ? getSvgImageSource : getImageSource));
                    return new EnumMemberInfo(name, DataAnnotationsAttributeHelper.GetFieldDescription(field), useUnderlyingEnumValue ? GetUnderlyingEnumValue(value) : value,
                        showImage, showName, getImage, DataAnnotationsAttributeHelper.GetFieldOrder(field));
                });
            switch(sortMode) {
                case EnumMembersSortMode.DisplayName:
                    result = result.OrderBy(x => x.Name);
                    break;
                case EnumMembersSortMode.DisplayNameDescending:
                    result = result.OrderByDescending(x => x.Name);
                    break;
                case EnumMembersSortMode.DisplayNameLength:
                    result = result.OrderBy(x => x.Name.Length);
                    break;
                case EnumMembersSortMode.DisplayNameLengthDescending:
                    result = result.OrderByDescending(x => x.Name.Length);
                    break;
            }
            return result.OrderBy(x => (x.Order != null) ? x.Order : DefaultDisplayOrder).ToArray();
        }
        static string GetEnumName(FieldInfo field, Enum value, IValueConverter nameConverter, bool splitNames) {
            if(nameConverter != null)
                return nameConverter.Convert(value, typeof(string), null, CultureInfo.CurrentCulture) as string;
            string displayName = DataAnnotationsAttributeHelper.GetFieldDisplayName(field);
            string name = displayName ?? TypeDescriptor.GetConverter(value.GetType()).ConvertTo(value, typeof(string)) as string;
            if(splitNames && displayName == null)
                return SplitStringHelper.SplitPascalCaseString(name);
            return name;
        }
        static object GetUnderlyingEnumValue(Enum value) {
            Type enumType = Enum.GetUnderlyingType(value.GetType());
            return Convert.ChangeType(value, enumType, System.Globalization.CultureInfo.CurrentCulture);
        }
 static Tuple<string, string> GetImageInfo(ImageAttribute image, DXImageAttribute dxImage, string defaultImageName, Func<string, bool, string> getKnownImageUriCallback) {
            if(image != null)
                return Tuple.Create(image.ImageUri, (string)null);
            string imageName = dxImage.With(x => x.ImageName) ?? defaultImageName;
            return Tuple.Create(dxImage.With(x => x.SmallImageUri) ?? GetKnownImageUri(getKnownImageUriCallback, imageName, false),
                                dxImage.With(x => x.LargeImageUri) ?? GetKnownImageUri(getKnownImageUriCallback, imageName, true));
        }
        static string GetKnownImageUri(Func<string, bool, string> getKnownImageUriCallback, string imageName, bool large) {
            if(getKnownImageUriCallback == null || string.IsNullOrEmpty(imageName))
                return null;
            return getKnownImageUriCallback(imageName, large);
        }
        static bool CanCreateSvgImageSource(Func<string, ImageSource> getSvgImageSource, string imageUri) {
            return getSvgImageSource != null && !string.IsNullOrEmpty(imageUri) && imageUri.EndsWith(".svg", StringComparison.InvariantCultureIgnoreCase);
        }
        static Func<string, ImageSource> TryGetImageSource(Func<string, ImageSource> getImageSource) {
            return uri => {
                try {
                    return getImageSource(uri);
                } catch {
                    throw new ArgumentException(string.Format("The Uri {0} cannot be converted to an image.", uri));
                }
            };
        }
    }
}
namespace DevExpress.Mvvm {
    public static class EnumSourceHelper {
        public static IEnumerable<EnumMemberInfo> GetEnumSource(Type enumType, bool useUnderlyingEnumValue = true, IValueConverter nameConverter = null, bool splitNames = false, EnumMembersSortMode sortMode = EnumMembersSortMode.Default, Func<string, ImageSource> getSvgImageSource = null) {
            return EnumSourceHelperCore.GetEnumSource(enumType, useUnderlyingEnumValue, nameConverter, splitNames, sortMode, null, getSvgImageSource: getSvgImageSource);
        }
    }
    public class EnumMemberInfo {
        public EnumMemberInfo(string value, string description, object id, ImageSource image)
            : this(value, description, id, image, true, true) {
        }
        public EnumMemberInfo(string value, string description, object id, ImageSource image, bool showImage, bool showName, int? order = null)
            : this(value, description, id, showImage, showName, () => image, order) {
        }
        public EnumMemberInfo(string value, string description, object id, bool showImage, bool showName, Func<ImageSource> getImage, int? order = null) {
            this.Name = value;
            this.Description = description;
            this.Id = id;
            this.image = new Lazy<ImageSource>(getImage);
            this.ShowImage = showImage;
            this.ShowName = showName;
            this.Order = order;
        }
        Lazy<ImageSource> image;

        public string Name { get; private set; }
        public bool ShowName { get; private set; }
        public object Id { get; private set; }
        public string Description { get; private set; }
        public ImageSource Image { get { return image.Value; } }
        public bool ShowImage { get; private set; }
        public int? Order { get; private set; }
        public override string ToString() {
            return Name.ToString();
        }
        public override bool Equals(object obj) {
            return object.Equals(Id, (obj as EnumMemberInfo).Return(o => o.Id, () => null));
        }
        public override int GetHashCode() {
            return Id.GetHashCode();
        }
    }
    public enum EnumMembersSortMode {
        Default,
        DisplayName,
        DisplayNameDescending,
        DisplayNameLength,
        DisplayNameLengthDescending,
    }
}