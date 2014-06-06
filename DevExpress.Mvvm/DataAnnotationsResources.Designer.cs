

namespace DevExpress.Mvvm {
    using System;


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DataAnnotationsResources {

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DataAnnotationsResources() {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DevExpress.Mvvm.DataAnnotationsResources", typeof(DataAnnotationsResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The {0} field is not a valid credit card number..
        /// </summary>
        internal static string CreditCardAttribute_Invalid {
            get {
                return ResourceManager.GetString("CreditCardAttribute_Invalid", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The {0} field is not a valid e-mail address..
        /// </summary>
        internal static string EmailAddressAttribute_Invalid {
            get {
                return ResourceManager.GetString("EmailAddressAttribute_Invalid", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to MaxLengthAttribute must have a Length value that is greater than zero. Use MaxLength() without parameters to indicate that the string or array can have the maximum allowable length..
        /// </summary>
        internal static string MaxLengthAttribute_InvalidMaxLength {
            get {
                return ResourceManager.GetString("MaxLengthAttribute_InvalidMaxLength", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The field {0} must be a string or array type with a maximum length of &apos;{1}&apos;..
        /// </summary>
        internal static string MaxLengthAttribute_ValidationError {
            get {
                return ResourceManager.GetString("MaxLengthAttribute_ValidationError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to MinLengthAttribute must have a Length value that is zero or greater..
        /// </summary>
        internal static string MinLengthAttribute_InvalidMinLength {
            get {
                return ResourceManager.GetString("MinLengthAttribute_InvalidMinLength", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The field {0} must be a string or array type with a minimum length of &apos;{1}&apos;..
        /// </summary>
        internal static string MinLengthAttribute_ValidationError {
            get {
                return ResourceManager.GetString("MinLengthAttribute_ValidationError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The {0} field is not a valid phone number..
        /// </summary>
        internal static string PhoneAttribute_Invalid {
            get {
                return ResourceManager.GetString("PhoneAttribute_Invalid", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The field {0} must be between {1} and {2}..
        /// </summary>
        internal static string RangeAttribute_ValidationError {
            get {
                return ResourceManager.GetString("RangeAttribute_ValidationError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The field {0} must match the regular expression &apos;{1}&apos;..
        /// </summary>
        internal static string RegexAttribute_ValidationError {
            get {
                return ResourceManager.GetString("RegexAttribute_ValidationError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The {0} field is not a valid fully-qualified http, https, or ftp URL..
        /// </summary>
        internal static string UrlAttribute_Invalid {
            get {
                return ResourceManager.GetString("UrlAttribute_Invalid", resourceCulture);
            }
        }
    }
}