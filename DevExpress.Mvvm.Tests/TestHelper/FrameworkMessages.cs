namespace DevExpress {
    internal static class FrameworkMessages {
        private const string _ActualHasMismatchedElements = "The expected collection contains {1} occurrence(s) of &lt;{2}&gt;. The actual collection contains {3} occurrence(s). {0}";
        private const string _AllItemsAreUniqueFailMsg = "Duplicate item found:&lt;{1}&gt;. {0}";
        private const string _AreEqualCaseFailMsg = "Expected:&lt;{1}&gt;. Case is different for actual value:&lt;{2}&gt;. {0}";
        private const string _AreEqualDeltaFailMsg = "Expected a difference no greater than &lt;{3}&gt; between expected value &lt;{1}&gt; and actual value &lt;{2}&gt;. {0}";
        private const string _AreEqualDifferentTypesFailMsg = "Expected:&lt;{1} ({2})&gt;. Actual:&lt;{3} ({4})&gt;. {0}";
        private const string _AreEqualFailMsg = "Expected:&lt;{1}&gt;. Actual:&lt;{2}&gt;. {0}";
        private const string _AreNotEqualDeltaFailMsg = "Expected a difference greater than &lt;{3}&gt; between expected value &lt;{1}&gt; and actual value &lt;{2}&gt;. {0}";
        private const string _AreNotEqualFailMsg = "Expected any value except:&lt;{1}&gt;. Actual:&lt;{2}&gt;. {0}";
        private const string _AreSameGivenValues = "Do not pass value types to AreSame(). Values converted to Object will never be the same. Consider using AreEqual(). {0}";
        private const string _AssertionFailed = "{0} failed. {1}";
        private const string _BothCollectionsEmpty = "Both collections are empty. {0}";
        private const string _BothCollectionsSameReference = "Both collection references point to the same collection object. {0}";
        private const string _BothSameElements = "Both collections contain the same elements. {0}";
        private const string _CollectionEqualReason = "{0}({1})";
        private const string _ContainsFail = "String '{0}' does not contain string '{1}'. {2}.";
        private const string _ElementNumbersDontMatch = "The number of elements in the collections do not match. Expected:&lt;{1}&gt;. Actual:&lt;{2}&gt;.{0}";
        private const string _ElementsAtIndexDontMatch = "Element at index {0} do not match.";
        private const string _ElementTypesAtIndexDontMatch = "Element at index {1} is not of expected type. Expected type:&lt;{2}&gt;. Actual type:&lt;{3}&gt;.{0}";
        private const string _ElementTypesAtIndexDontMatch2 = "Element at index {1} is (null). Expected type:&lt;{2}&gt;.{0}";
        private const string _EndsWithFail = "String '{0}' does not end with string '{1}'. {2}.";
        private const string _ErrorInvalidCast = "Cannot convert object of type {0} to {1}.";
        private const string _InvalidParameterToAssert = "The parameter '{0}' is invalid. {1}.";
        private const string _InvalidPropertyType = "The property {0} has type {1}; expected type {2}.";
        private const string _IsInstanceOfFailMsg = "{0}Expected type:&lt;{1}&gt;. Actual type:&lt;{2}&gt;.";
        private const string _IsMatchFail = "String '{0}' does not match pattern '{1}'. {2}.";
        private const string _IsNotInstanceOfFailMsg = "Wrong Type:&lt;{1}&gt;. Actual type:&lt;{2}&gt;. {0}";
        private const string _IsNotMatchFail = "String '{0}' matches pattern '{1}'. {2}.";
        private const string _NullParameterToAssert = "The parameter '{0}' is invalid. The value cannot be null. {1}.";
        private const string _PrivateAccessorConstructorNotFound = "\r\n      The constructor with the specified signature could not be found. You might need to regenerate your private accessor,\r\n      or the member may be private and defined on a base class. If the latter is true, you need to pass the type\r\n      that defines the member into PrivateObject's constructor.\r\n    ";
        private const string _PrivateAccessorMemberNotFound = "\r\n      The member specified ({0}) could not be found. You might need to regenerate your private accessor,\r\n      or the member may be private and defined on a base class. If the latter is true, you need to pass the type\r\n      that defines the member into PrivateObject's constructor.\r\n    ";
        private const string _StartsWithFail = "String '{0}' does not start with string '{1}'. {2}.";
        public const string AccessStringInvalidSyntax = "Access string has invalid syntax.";
        public const string BothCollectionsSameElements = "Both collection contain same elements.";
        public const string Common_NullInMessages = "(null)";
        public const string Common_ObjectString = "(object)";
        public const string Equal_1_2 = "eq1 equals eq2";
        public const string Equal_1_n = "eq1 not equals null";
        public const string Equal_2_1 = "eq2 equals eq1";
        public const string Equal_2_n = "eq2 not equals null";
        public const string Equal_ch_n = "child not equals null";
        public const string Equal_d_n = "diff not equals null";
        public const string EqualsTesterInvalidArgs = "Invalid argument- EqualsTester can't use nulls.";
        public const string HashTesterHashMatch_Eq1_Eq2 = "Hash codes for eq1 and eq2 match.";
        public const string HashTesterHashNotMatch_Eq1 = "Hash codes for eq1 and diff match.";
        public const string HashTesterHashNotMatch_Eq2 = "Hash codes for eq2 and diff do not match.";
        public const string HashTesterTypeMisMatch_1_2 = "Invalid argument- different types of first and second.";
        public const string HashTesterTypeMisMatch_1_3 = "Invalid argument- different types of first and third.";
        public const string InternalObjectNotValid = "The internal object referenced is no longer valid.";
        public const string NotEqual_1_ch = "eq1 not equals child";
        public const string NotEqual_1_d = "eq1 not equals diff";
        public const string NotEqual_2_d = "eq2 not equals diff";
        public const string NotEqual_ch_1 = "child not equals eq1";
        public const string NotEqual_ch_d = "child not equals diff";
        public const string NotEqual_d_1 = "diff not equals eq1";
        public const string NotEqual_d_2 = "diff not equals eq2";
        public const string NotEqual_d_ch = "diff not equals child";
        public const string NumberOfElementsDiff = "Different number of elements.";
        public static string ActualHasMismatchedElements(params object[] p) {
            return string.Format("The expected collection contains {1} occurrence(s) of &lt;{2}&gt;. The actual collection contains {3} occurrence(s). {0}", p);
        }

        public static string AllItemsAreUniqueFailMsg(params object[] p) {
            return string.Format("Duplicate item found:&lt;{1}&gt;. {0}", p);
        }

        public static string AreEqualCaseFailMsg(params object[] p) {
            return string.Format("Expected:&lt;{1}&gt;. Case is different for actual value:&lt;{2}&gt;. {0}", p);
        }

        public static string AreEqualDeltaFailMsg(params object[] p) {
            return string.Format("Expected a difference no greater than &lt;{3}&gt; between expected value &lt;{1}&gt; and actual value &lt;{2}&gt;. {0}", p);
        }

        public static string AreEqualDifferentTypesFailMsg(params object[] p) {
            return string.Format("Expected:&lt;{1} ({2})&gt;. Actual:&lt;{3} ({4})&gt;. {0}", p);
        }

        public static string AreEqualFailMsg(params object[] p) {
            return string.Format("Expected:&lt;{1}&gt;. Actual:&lt;{2}&gt;. {0}", p);
        }

        public static string AreNotEqualDeltaFailMsg(params object[] p) {
            return string.Format("Expected a difference greater than &lt;{3}&gt; between expected value &lt;{1}&gt; and actual value &lt;{2}&gt;. {0}", p);
        }

        public static string AreNotEqualFailMsg(params object[] p) {
            return string.Format("Expected any value except:&lt;{1}&gt;. Actual:&lt;{2}&gt;. {0}", p);
        }

        public static string AreSameGivenValues(params object[] p) {
            return string.Format("Do not pass value types to AreSame(). Values converted to Object will never be the same. Consider using AreEqual(). {0}", p);
        }

        public static string AssertionFailed(params object[] p) {
            return string.Format("{0} failed. {1}", p);
        }

        public static string BothCollectionsEmpty(params object[] p) {
            return string.Format("Both collections are empty. {0}", p);
        }

        public static string BothCollectionsSameReference(params object[] p) {
            return string.Format("Both collection references point to the same collection object. {0}", p);
        }

        public static string BothSameElements(params object[] p) {
            return string.Format("Both collections contain the same elements. {0}", p);
        }

        public static string CollectionEqualReason(params object[] p) {
            return string.Format("{0}({1})", p);
        }

        public static string ContainsFail(params object[] p) {
            return string.Format("String '{0}' does not contain string '{1}'. {2}.", p);
        }

        public static string ElementNumbersDontMatch(params object[] p) {
            return string.Format("The number of elements in the collections do not match. Expected:&lt;{1}&gt;. Actual:&lt;{2}&gt;.{0}", p);
        }

        public static string ElementsAtIndexDontMatch(params object[] p) {
            return string.Format("Element at index {0} do not match.", p);
        }

        public static string ElementTypesAtIndexDontMatch(params object[] p) {
            return string.Format("Element at index {1} is not of expected type. Expected type:&lt;{2}&gt;. Actual type:&lt;{3}&gt;.{0}", p);
        }

        public static string ElementTypesAtIndexDontMatch2(params object[] p) {
            return string.Format("Element at index {1} is (null). Expected type:&lt;{2}&gt;.{0}", p);
        }

        public static string EndsWithFail(params object[] p) {
            return string.Format("String '{0}' does not end with string '{1}'. {2}.", p);
        }

        public static string ErrorInvalidCast(params object[] p) {
            return string.Format("Cannot convert object of type {0} to {1}.", p);
        }

        public static string InvalidParameterToAssert(params object[] p) {
            return string.Format("The parameter '{0}' is invalid. {1}.", p);
        }

        public static string InvalidPropertyType(params object[] p) {
            return string.Format("The property {0} has type {1}; expected type {2}.", p);
        }

        public static string IsInstanceOfFailMsg(params object[] p) {
            return string.Format("{0}Expected type:&lt;{1}&gt;. Actual type:&lt;{2}&gt;.", p);
        }

        public static string IsMatchFail(params object[] p) {
            return string.Format("String '{0}' does not match pattern '{1}'. {2}.", p);
        }

        public static string IsNotInstanceOfFailMsg(params object[] p) {
            return string.Format("Wrong Type:&lt;{1}&gt;. Actual type:&lt;{2}&gt;. {0}", p);
        }

        public static string IsNotMatchFail(params object[] p) {
            return string.Format("String '{0}' matches pattern '{1}'. {2}.", p);
        }

        public static string NullParameterToAssert(params object[] p) {
            return string.Format("The parameter '{0}' is invalid. The value cannot be null. {1}.", p);
        }

        public static string PrivateAccessorConstructorNotFound(params object[] p) {
            return string.Format("\r\n      The constructor with the specified signature could not be found. You might need to regenerate your private accessor,\r\n      or the member may be private and defined on a base class. If the latter is true, you need to pass the type\r\n      that defines the member into PrivateObject's constructor.\r\n    ", p);
        }

        public static string StartsWithFail(params object[] p) {
            return string.Format("String '{0}' does not start with string '{1}'. {2}.", p);
        }
    }
}