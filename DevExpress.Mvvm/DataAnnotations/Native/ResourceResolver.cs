using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.Security;

namespace DevExpress.Mvvm.Native {
    [SecuritySafeCritical]
    public class DataAnnotationsResourcesResolver {
        static ResourceManager annotationsResourceManager;
        internal static ResourceManager AnnotationsResourceManager {
            get {
                if(object.ReferenceEquals(annotationsResourceManager, null)) {
                    if(typeof(ValidationAttribute).Assembly.FullName.StartsWith("System.ComponentModel.DataAnnotations,"))
                        annotationsResourceManager = new ResourceManager("System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources", typeof(ValidationAttribute).Assembly);
                    else
                        annotationsResourceManager = new ResourceManager("FxResources.System.ComponentModel.Annotations.SR", typeof(ValidationAttribute).Assembly);
                }
                return annotationsResourceManager;
            }
        }
        public static string MinLengthAttribute_ValidationError { get { return GetResourceString("MinLengthAttribute_ValidationError"); } }
        public static string MinLengthAttribute_InvalidMinLength { get { return GetResourceString("MinLengthAttribute_InvalidMinLength"); } }
        public static string MaxLengthAttribute_InvalidMaxLength { get { return GetResourceString("MaxLengthAttribute_InvalidMaxLength"); } }
        public static string MaxLengthAttribute_ValidationError { get { return GetResourceString("MaxLengthAttribute_ValidationError"); } }
        public static string PhoneAttribute_Invalid { get { return GetResourceString("PhoneAttribute_Invalid"); } }
        public static string CreditCardAttribute_Invalid { get { return GetResourceString("CreditCardAttribute_Invalid"); } }
        public static string EmailAddressAttribute_Invalid { get { return GetResourceString("EmailAddressAttribute_Invalid"); } }
        public static string UrlAttribute_Invalid { get { return GetResourceString("UrlAttribute_Invalid"); } }
        public static string RangeAttribute_ValidationError { get { return GetResourceString("RangeAttribute_ValidationError"); } }
        public static string RegexAttribute_ValidationError { get { return GetResourceString("RegexAttribute_ValidationError"); } }
        public static string CustomValidationAttribute_ValidationError { get { return GetResourceString("CustomValidationAttribute_ValidationError"); } }
        public static string RequiredAttribute_ValidationError { get { return GetResourceString("RequiredAttribute_ValidationError"); } }

        static string GetResourceString(string resourceName) {
            return AnnotationsResourceManager.GetString(resourceName) ?? DataAnnotationsResources.ResourceManager.GetString(resourceName);
        }
    }
}