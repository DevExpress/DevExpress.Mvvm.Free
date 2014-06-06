using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DevExpress {
    public static class Assert {
        public static string error = string.Empty;
        public static void AreEqual<T>(T expected, T actual) {
            if(typeof(T).IsArray) {
                AreArraysEqual(expected as Array, actual as Array);
            } else {
                AreEqual<T>(expected, actual, string.Empty, null);
            }
        }

        public static void AreEqual(object expected, object actual) {
            if(expected is Array && actual is Array) {
                AreArraysEqual((Array)expected, (Array)actual);
            } else {
                AreEqual(expected, actual, string.Empty, null);
            }
        }

        public static void AreEqual<T>(T expected, T actual, string message) {
            AreEqual<T>(expected, actual, message, null);
        }

        public static void AreEqual(double expected, double actual, double delta) {
            AreEqual(expected, actual, delta, string.Empty, null);
        }

        public static void AreEqual(object expected, object actual, string message) {
            AreEqual(expected, actual, message, null);
        }

        public static void AreEqual(float expected, float actual, float delta) {
            AreEqual(expected, actual, delta, string.Empty, null);
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase) {
            AreEqual(expected, actual, ignoreCase, string.Empty, (object[])null);
        }

        public static void AreEqual(double expected, double actual, double delta, string message) {
            AreEqual(expected, actual, delta, message, null);
        }

        public static void AreEqual(object expected, object actual, string message, params object[] parameters) {
            AreEqual<object>(expected, actual, message, parameters);
        }

        public static void AreEqual(float expected, float actual, float delta, string message) {
            AreEqual(expected, actual, delta, message, null);
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, CultureInfo culture) {
            AreEqual(expected, actual, ignoreCase, culture, string.Empty, null);
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, string message) {
            AreEqual(expected, actual, ignoreCase, message, (object[])null);
        }

        static bool IsArray<T>(T obj) {
            if(obj == null) return false;
            return obj.GetType().IsArray;
        }

        public static void Fail(object expected, object actual) {
            string finalMessage = FrameworkMessages.AreEqualFailMsg(new object[] { "", ReplaceNulls(expected), ReplaceNulls(actual) });
            Fail(finalMessage);
        }

        static string DifferentLengthFailedMessage(int expected, int actual) {
            return string.Format("Specified arrays have different length. Expected: &lt;{0}&gt;. Actual: &lt;{0}&gt;");
        }

        public static void AreArraysEqual(Array expected, Array actual) {
            if(object.ReferenceEquals(expected, null) && object.ReferenceEquals(actual, null)) return;
            if(object.ReferenceEquals(expected, null) || object.ReferenceEquals(actual, null))
                Fail(expected, actual);
            if(expected.Length != actual.Length)
                Fail(DifferentLengthFailedMessage(expected.Length, actual.Length));
            for(int i = 0; i < actual.Length; i++) {
                AreEqual(expected.GetValue(i), actual.GetValue(i), string.Format("Array differs at index &lt;{0}&gt;", i));
            }
        }

        public static void AreEqual<T>(T expected, T actual, string message, params object[] parameters) {
            if(object.Equals(CheckValue(expected), CheckValue(actual)))
                return;
            string finalMessage;
            if(((actual != null) && (expected != null)) && !actual.GetType().Equals(expected.GetType())) {
                finalMessage = FrameworkMessages.AreEqualDifferentTypesFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), ReplaceNulls(expected), expected.GetType().FullName, ReplaceNulls(actual), actual.GetType().FullName });
            } else {
                finalMessage = FrameworkMessages.AreEqualFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), ReplaceNulls(expected), ReplaceNulls(actual) });
            }
            HandleFail("Assert.AreEqual", finalMessage, parameters);
        }
        static object CheckValue(object value) {
            if(value is int)
                return Convert.ToDecimal(value);
            return value;
        }

        public static void AreEqual(double expected, double actual, double delta, string message, params object[] parameters) {
            if(Math.Abs((double)(expected - actual)) > delta) {
                string finalMessage = FrameworkMessages.AreEqualDeltaFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), expected.ToString(CultureInfo.CurrentCulture.NumberFormat), actual.ToString(CultureInfo.CurrentCulture.NumberFormat), delta.ToString(CultureInfo.CurrentCulture.NumberFormat) });
                HandleFail("Assert.AreEqual", finalMessage, parameters);
            }
        }

        public static void AreEqual(float expected, float actual, float delta, string message, params object[] parameters) {
            if(Math.Abs((float)(expected - actual)) > delta) {
                string finalMessage = FrameworkMessages.AreEqualDeltaFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), expected.ToString(CultureInfo.CurrentCulture.NumberFormat), actual.ToString(CultureInfo.CurrentCulture.NumberFormat), delta.ToString(CultureInfo.CurrentCulture.NumberFormat) });
                HandleFail("Assert.AreEqual", finalMessage, parameters);
            }
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, CultureInfo culture, string message) {
            AreEqual(expected, actual, ignoreCase, culture, message, null);
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, string message, params object[] parameters) {
            AreEqual(expected, actual, ignoreCase, CultureInfo.InvariantCulture, message, parameters);
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters) {
            CheckParameterNotNull(culture, "Assert.AreEqual", "culture", string.Empty, new object[0]);
            CompareOptions compareOptions = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            if(string.Compare(expected, actual, culture, compareOptions) != 0) {
                string finalMessage;
                if(!ignoreCase && (string.Compare(expected, actual, culture, CompareOptions.IgnoreCase) == 0)) {
                    finalMessage = FrameworkMessages.AreEqualCaseFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), ReplaceNulls(expected), ReplaceNulls(actual) });
                } else {
                    finalMessage = FrameworkMessages.AreEqualFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), ReplaceNulls(expected), ReplaceNulls(actual) });
                }
                HandleFail("Assert.AreEqual", finalMessage, parameters);
            }
        }

        public static void AreNotEqual<T>(T notExpected, T actual) {
            AreNotEqual<T>(notExpected, actual, string.Empty, null);
        }

        public static void AreNotEqual(object notExpected, object actual) {
            AreNotEqual(notExpected, actual, string.Empty, null);
        }

        public static void AreNotEqual(double notExpected, double actual, double delta) {
            AreNotEqual(notExpected, actual, delta, string.Empty, null);
        }

        public static void AreNotEqual(object notExpected, object actual, string message) {
            AreNotEqual(notExpected, actual, message, null);
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string message) {
            AreNotEqual<T>(notExpected, actual, message, null);
        }

        public static void AreNotEqual(float notExpected, float actual, float delta) {
            AreNotEqual(notExpected, actual, delta, string.Empty, null);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase) {
            AreNotEqual(notExpected, actual, ignoreCase, string.Empty, (object[])null);
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string message, params object[] parameters) {
            if(object.Equals(notExpected, actual)) {
                string finalMessage = FrameworkMessages.AreNotEqualFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), ReplaceNulls(notExpected), ReplaceNulls(actual) });
                HandleFail("Assert.AreNotEqual", finalMessage, parameters);
            }
        }

        public static void AreNotEqual(double notExpected, double actual, double delta, string message) {
            AreNotEqual(notExpected, actual, delta, message, null);
        }

        public static void AreNotEqual(object notExpected, object actual, string message, params object[] parameters) {
            AreNotEqual<object>(notExpected, actual, message, parameters);
        }

        public static void AreNotEqual(float notExpected, float actual, float delta, string message) {
            AreNotEqual(notExpected, actual, delta, message, null);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, CultureInfo culture) {
            AreNotEqual(notExpected, actual, ignoreCase, culture, string.Empty, null);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, string message) {
            AreNotEqual(notExpected, actual, ignoreCase, message, (object[])null);
        }

        public static void AreNotEqual(double notExpected, double actual, double delta, string message, params object[] parameters) {
            if(Math.Abs((double)(notExpected - actual)) <= delta) {
                string finalMessage = FrameworkMessages.AreNotEqualDeltaFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), notExpected.ToString(CultureInfo.CurrentCulture.NumberFormat), actual.ToString(CultureInfo.CurrentCulture.NumberFormat), delta.ToString(CultureInfo.CurrentCulture.NumberFormat) });
                HandleFail("Assert.AreNotEqual", finalMessage, parameters);
            }
        }

        public static void AreNotEqual(float notExpected, float actual, float delta, string message, params object[] parameters) {
            if(Math.Abs((float)(notExpected - actual)) <= delta) {
                string finalMessage = FrameworkMessages.AreNotEqualDeltaFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), notExpected.ToString(CultureInfo.CurrentCulture.NumberFormat), actual.ToString(CultureInfo.CurrentCulture.NumberFormat), delta.ToString(CultureInfo.CurrentCulture.NumberFormat) });
                HandleFail("Assert.AreNotEqual", finalMessage, parameters);
            }
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message) {
            AreNotEqual(notExpected, actual, ignoreCase, culture, message, null);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, string message, params object[] parameters) {
            AreNotEqual(notExpected, actual, ignoreCase, CultureInfo.InvariantCulture, message, parameters);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters) {
            CheckParameterNotNull(culture, "Assert.AreNotEqual", "culture", string.Empty, new object[0]);
            if(string.Compare(notExpected, actual, culture, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) == 0) {
                string finalMessage = FrameworkMessages.AreNotEqualFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), ReplaceNulls(notExpected), ReplaceNulls(actual) });
                HandleFail("Assert.AreNotEqual", finalMessage, parameters);
            }
        }

        public static void AreNotSame(object notExpected, object actual) {
            AreNotSame(notExpected, actual, string.Empty, null);
        }

        public static void AreNotSame(object notExpected, object actual, string message) {
            AreNotSame(notExpected, actual, message, null);
        }

        public static void AreNotSame(object notExpected, object actual, string message, params object[] parameters) {
            if(object.ReferenceEquals(notExpected, actual)) {
                HandleFail("Assert.AreNotSame", message, parameters);
            }
        }

        public static void AreSame(object expected, object actual) {
            AreSame(expected, actual, string.Empty, null);
        }

        public static void AreSame(object expected, object actual, string message) {
            AreSame(expected, actual, message, null);
        }

        public static void AreSame(object expected, object actual, string message, params object[] parameters) {
            if(!object.ReferenceEquals(expected, actual)) {
                string finalMessage = message;
                if((expected is ValueType) && (actual is ValueType)) {
                    finalMessage = FrameworkMessages.AreSameGivenValues(new object[] { (message == null) ? string.Empty : ReplaceNulls(message) });
                }
                HandleFail("Assert.AreSame", finalMessage, parameters);
            }
        }

        internal static void CheckParameterNotNull(object param, string assertionName, string parameterName, string message, params object[] parameters) {
            if(param == null) {
                HandleFail(assertionName, FrameworkMessages.NullParameterToAssert(new object[] { parameterName, message }), parameters);
            }
        }

        public static void Fail() {
            Fail(string.Empty, null);
        }

        public static void Fail(string message) {
            Fail(message, null);
        }

        public static void Fail(string message, params object[] parameters) {
            HandleFail("Assert.Fail", message, parameters);
        }

        internal static void HandleFail(string assertionName, string message, params object[] parameters) {
            string finalMessage = string.Empty;
            if(!string.IsNullOrEmpty(message)) {
                if(parameters == null) {
                    finalMessage = ReplaceNulls(message);
                } else {
                    finalMessage = string.Format(CultureInfo.CurrentCulture, ReplaceNulls(message), parameters);
                }
            }
            throw new AssertFailedException(FrameworkMessages.AssertionFailed(new object[] { assertionName, finalMessage }));
        }

        public static void Inconclusive() {
            Inconclusive(string.Empty, null);
        }

        public static void Inconclusive(string message) {
            Inconclusive(message, null);
        }

        public static void Inconclusive(string message, params object[] parameters) {
            string finalMessage = string.Empty;
            if(!string.IsNullOrEmpty(message)) {
                if(parameters == null) {
                    finalMessage = ReplaceNulls(message);
                } else {
                    finalMessage = string.Format(CultureInfo.CurrentCulture, ReplaceNulls(message), parameters);
                }
            }
            throw new AssertInconclusiveException(FrameworkMessages.AssertionFailed(new object[] { "Assert.Inconclusive", finalMessage }));
        }

        public static void IsFalse(bool condition) {
            IsFalse(condition, string.Empty, null);
        }

        public static void IsFalse(bool condition, string message) {
            IsFalse(condition, message, null);
        }

        public static void IsFalse(bool condition, string message, params object[] parameters) {
            if(condition) {
                HandleFail("Assert.IsFalse", message, parameters);
            }
        }

        public static void IsInstanceOf(Type expectedType, object value) {
            IsInstanceOfType(expectedType, value);
        }

        public static void IsInstanceOfType(Type expectedType, object value) {
            IsInstanceOfType(expectedType, value, string.Empty, null);
        }

        public static void IsInstanceOfType(Type expectedType, object value, string message) {
            IsInstanceOfType(expectedType, value, message, null);
        }

        public static void IsInstanceOfType(Type expectedType, object value, string message, params object[] parameters) {
            if(expectedType == null) {
                HandleFail("Assert.IsInstanceOfType", message, parameters);
            }
            if(!expectedType.IsInstanceOfType(value)) {
                string finalMessage = FrameworkMessages.IsInstanceOfFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), expectedType.ToString(), (value == null) ? "(null)" : value.GetType().ToString() });
                HandleFail("Assert.IsInstanceOfType", finalMessage, parameters);
            }
        }

        public static void IsNotInstanceOfType(object value, Type wrongType) {
            IsNotInstanceOfType(value, wrongType, string.Empty, null);
        }

        public static void IsNotInstanceOfType(object value, Type wrongType, string message) {
            IsNotInstanceOfType(value, wrongType, message, null);
        }

        public static void IsNotInstanceOfType(object value, Type wrongType, string message, params object[] parameters) {
            if(wrongType == null) {
                HandleFail("Assert.IsNotInstanceOfType", message, parameters);
            }
            if((value != null) && wrongType.IsInstanceOfType(value)) {
                string finalMessage = FrameworkMessages.IsNotInstanceOfFailMsg(new object[] { (message == null) ? string.Empty : ReplaceNulls(message), wrongType.ToString(), value.GetType().ToString() });
                HandleFail("Assert.IsNotInstanceOfType", finalMessage, parameters);
            }
        }

        public static void IsNotNull(object value) {
            IsNotNull(value, string.Empty, null);
        }

        public static void IsNotNull(object value, string message) {
            IsNotNull(value, message, null);
        }

        public static void IsNotNull(object value, string message, params object[] parameters) {
            if(value == null) {
                HandleFail("Assert.IsNotNull", message, parameters);
            }
        }

        public static void IsNull(object value) {
            IsNull(value, string.Empty, null);
        }

        public static void IsNull(object value, string message) {
            IsNull(value, message, null);
        }

        public static void IsNull(object value, string message, params object[] parameters) {
            if(value != null) {
                HandleFail("Assert.IsNull", message, parameters);
            }
        }

        public static void IsTrue(bool condition) {
            IsTrue(condition, string.Empty, null);
        }

        public static void IsTrue(bool condition, string message) {
            IsTrue(condition, message, null);
        }

        public static void IsTrue(bool condition, string message, params object[] parameters) {
            if(!condition) {
                HandleFail("Assert.IsTrue", message, parameters);
            }
        }

        public static void IsEmpty(ICollection value) {
            IsEmpty(value, string.Empty);
        }

        public static void IsEmpty(string value) {
            IsEmpty(value, string.Empty);
        }

        public static void IsEmpty(ICollection value, string message) {
            IsEmpty(value, message, null);
        }

        public static void IsEmpty(string value, string message) {
            IsEmpty(value, message, null);
        }

        public static void IsEmpty(ICollection value, string message, params object[] parameters) {
            if(value.Count != 0)
                HandleFail("Assert.IsEmpty", message, parameters);
        }

        public static void IsEmpty(string value, string message, params object[] parameters) {
            if(value != string.Empty)
                HandleFail("Assert.IsEmpty", message, parameters);
        }

        public static void IsNotEmpty(ICollection value) {
            IsNotEmpty(value, string.Empty);
        }

        public static void IsNotEmpty(string value) {
            IsNotEmpty(value, string.Empty);
        }

        public static void IsNotEmpty(ICollection value, string message) {
            IsNotEmpty(value, message, null);
        }

        public static void IsNotEmpty(string value, string message) {
            IsNotEmpty(value, message, null);
        }

        public static void IsNotEmpty(ICollection value, string message, params object[] parameters) {
            if(value.Count == 0)
                HandleFail("Assert.IsNotEmpty", message, parameters);
        }

        public static void IsNotEmpty(string value, string message, params object[] parameters) {
            if(value == string.Empty)
                HandleFail("Assert.IsNotEmpty", message, parameters);
        }

        public static string ReplaceNullChars(string input) {
            if(string.IsNullOrEmpty(input)) {
                return input;
            }
            List<int> zeroPos = new List<int>();
            for(int i = 0; i < input.Length; i++) {
                if(input[i] == '\0') {
                    zeroPos.Add(i);
                }
            }
            if(zeroPos.Count <= 0) {
                return input;
            }
            StringBuilder sb = new StringBuilder(input.Length + zeroPos.Count);
            int start = 0;
            foreach(int index in zeroPos) {
                sb.Append(input.Substring(start, index - start));
                sb.Append(@"\0");
                start = index + 1;
            }
            sb.Append(input.Substring(start));
            return sb.ToString();
        }

        internal static string ReplaceNulls(object input) {
            if(input == null) {
                return "(null)".ToString();
            }
            string inputString = input.ToString();
            if(inputString == null) {
                return "(object)".ToString();
            }
            return ReplaceNullChars(inputString);
        }
    }
}