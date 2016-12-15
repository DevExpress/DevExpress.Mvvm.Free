using NUnit.Framework;
using DevExpress.Mvvm.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;
namespace DevExpress {
    public class SetNotEqualsException : AssertionException {
        public SetNotEqualsException(string message) : base(message) { }
    }
    [System.Diagnostics.DebuggerNonUserCode]
    public static class AssertExtension {
        public static void AssertSequenceEqual(this IEnumerable expected, IEnumerable actual) {
            Assert.AreEqual(true, expected.Cast<object>().SequenceEqual(actual.Cast<object>()));
        }
        public static void AssertSequenceNotEqual(this IEnumerable expected, IEnumerable actual) {
            Assert.AreEqual(false, expected.Cast<object>().SequenceEqual(actual.Cast<object>()));
        }
    }
    [System.Diagnostics.DebuggerNonUserCode]
    public static class FluentAssert {
        public static TInput IsNull<TInput>(this TInput obj, Func<TInput, object> valueEvaluator = null) where TInput : class {
            Assert.IsNull(GetActualValue(obj, valueEvaluator));
            return obj;
        }
        public static TInput IsNotNull<TInput>(this TInput obj, Func<TInput, object> valueEvaluator = null) where TInput : class {
            Assert.IsNotNull(GetActualValue(obj, valueEvaluator));
            return obj;
        }
        public static TInput AreEqual<TInput>(this TInput obj, object expectedValue) {
            Assert.AreEqual(expectedValue, obj);
            return obj;
        }
        public static TInput AreEqual<TInput>(this TInput obj, Func<TInput, object> valueEvaluator, object expectedValue) {
            Assert.AreEqual(expectedValue, valueEvaluator(obj));
            return obj;
        }
        public static TInput AreNotEqual<TInput>(this TInput obj, object expectedValue) {
            Assert.AreNotEqual(expectedValue, obj);
            return obj;
        }
        public static TInput AreNotEqual<TInput>(this TInput obj, Func<TInput, object> valueEvaluator, object expectedValue) {
            Assert.AreNotEqual(expectedValue, valueEvaluator(obj));
            return obj;
        }
        public static TInput IsTrue<TInput>(this TInput obj, Func<TInput, bool> valueEvaluator) {
            Assert.IsTrue(valueEvaluator(obj));
            return obj;
        }
        public static TInput IsFalse<TInput>(this TInput obj, Func<TInput, bool> valueEvaluator) {
            Assert.IsFalse(valueEvaluator(obj));
            return obj;
        }
        public static bool IsTrue(this bool val) {
            Assert.IsTrue(val);
            return val;
        }
        public static bool IsFalse(this bool val) {
            Assert.IsFalse(val);
            return val;
        }
        public static TInput IsInstanceOfType<TInput>(this TInput obj, Type expectedType) where TInput : class {
            DevExpress
            .AssertHelper.IsInstanceOf(expectedType, obj);
            return obj;
        }
        public static TInput IsInstanceOfType<TInput>(this TInput obj, Func<TInput, object> valueEvaluator, Type expectedType) where TInput : class {
            DevExpress
            .AssertHelper.IsInstanceOf(expectedType, valueEvaluator(obj));
            return obj;
        }
        static object GetActualValue<TInput>(TInput obj, Func<TInput, object> valueEvaluator) {
            return valueEvaluator == null ? obj : valueEvaluator(obj);
        }
    }
    public class AssertHelper {
#pragma warning disable 612, 618
        public static void IsInstanceOf(Type type, object target) {
            Assert.IsInstanceOf(type, target);
        }
        public static void IsInstanceOf(Type type, object target, string message) {
            Assert.IsInstanceOf(type, target, message);
        }
        public static void IsInstanceOf(Type expected, object actual, string message, params object[] args) {
            Assert.IsInstanceOf(expected, actual, message, args);
        }
#pragma warning restore 612, 618
        public static Constraint HasCount(int count) {
            var prop = typeof(Has).GetProperty("Count", BindingFlags.Public | BindingFlags.Static);
            if(prop != null) {
                var countConstraint = prop.GetValue(null, null);
                var methodEqualTo = countConstraint.GetType().GetMethod("EqualTo", BindingFlags.Public | BindingFlags.Instance);
                return methodEqualTo.Invoke(countConstraint, new object[] { count }) as Constraint;
            }
            var method = typeof(Has).GetMethod("Count", BindingFlags.Public | BindingFlags.Static);
            return method.Invoke(null, new object[] { count }) as Constraint;
        }

        public static void AssertAllPropertiesAreEqual(object expected, object actual, bool compareTypes = true) {
            AssertAllPropertiesAreEqual(expected, actual, new string[] { }, compareTypes);
        }
        public static void AssertAllPropertiesAreEqual(object expected, object actual, IEnumerable<string> skipProperties, bool compareTypes = true) {
            if(expected == null || actual == null)
                Assert.AreEqual(expected, actual);
            if(compareTypes)
                Assert.AreEqual(expected.GetType(), actual.GetType());
            foreach(PropertyInfo expectedProperty in expected.GetType().GetProperties()) {
                if(skipProperties.Contains(expectedProperty.Name)) continue;
                MethodInfo setter = expectedProperty.GetSetMethod();
                if(setter == null || !setter.IsPublic) continue;
                PropertyInfo actualProperty = actual.GetType().GetProperty(expectedProperty.Name);
                Assert.AreEqual(expectedProperty.Name, actualProperty != null ? actualProperty.Name : null);
                Assert.AreEqual(expectedProperty.GetValue(expected, null), actualProperty.GetValue(actual, null));
            }
        }
        public static void AssertEnumerablesAreEqual(IEnumerable expected, IEnumerable actual) {
            AssertEnumerablesAreEqual(expected, actual, false, null, true);
        }
        public static void AssertEnumerablesAreEqual(IEnumerable expected, IEnumerable actual, bool compareByProperties, bool compareTypes = true) {
            AssertEnumerablesAreEqual(expected, actual, compareByProperties, new string[] { }, compareTypes);
        }
        public static void AssertEnumerablesAreEqual(IEnumerable expected, IEnumerable actual, bool compareByProperties, IEnumerable<string> skipProperties, bool compareTypes = true) {
            object[] expectedArray = expected.Cast<object>().ToArray();
            object[] actualArray = actual.Cast<object>().ToArray();
            Assert.AreEqual(expectedArray.Length, actual.Cast<object>().Count());
            for(int i = 0; i < expectedArray.Length; i++) {
                if(compareByProperties)
                    AssertAllPropertiesAreEqual(expectedArray[i], actualArray[i], skipProperties, compareTypes);
                else
                    Assert.AreEqual(expectedArray[i], actualArray[i]);
            }
        }
        public static void AssertSetsAreEqual(IEnumerable expected, IEnumerable actual, bool compareByProperties = false, bool compareTypes = true) {
            List<object> expectedArray = expected.Cast<object>().ToList();
            List<object> actualArray = actual.Cast<object>().ToList();
            Assert.AreEqual(expectedArray.Count, actualArray.Count, "Length of sets does not equal");
            for(int expectedIndex = expectedArray.Count; --expectedIndex >= 0; ) {
                object expectedItem = expectedArray[expectedIndex];
                int? actualIndex = actualArray
                    .Select((x, i) => new { value = x, index = (int?)i })
                    .Where(x => AreEqual(expectedItem, x.value, compareByProperties, compareTypes))
                    .Select(x => x.index)
                    .FirstOrDefault();
                if(actualIndex == null) continue;
                actualArray.RemoveAt(actualIndex.Value);
                expectedArray.RemoveAt(expectedIndex);
            }
            if(actualArray.Count == 0 && expectedArray.Count == 0) return;
            string message = "";
            if(actualArray.Count != 0) {
                message += "Does not expected: ";
                foreach(object item in actualArray) {
                    message += "{" + item + "} ";
                }
                message += "\n";
            }
            if(expectedArray.Count != 0) {
                message += "Expected, but not found: ";
                foreach(object item in expectedArray) {
                    message += "{" + item + "} ";
                }
                message += "\n";
            }
            throw new SetNotEqualsException(message);
        }
        public static void AssertThrows<T>(Action action, Action<T> checkException = null) where T : Exception {
            try {
                action();
            } catch(T e) {
                if(checkException != null)
                    checkException(e);
                return;
            } catch(Exception e) {
                Assert.Fail(string.Format("A wrong type of exception was thrown: {0} ({1} was expected)",
                    e.GetType().Name, typeof(T).Name));
            }
            Assert.Fail(string.Format("No exception was thrown ({0} was expected)", typeof(T).Name));
        }

        public static void AssertDoesNotThrow(Action action) {
            try {
                action();
                return;
            } catch(Exception e) {
                Assert.Fail(string.Format("Expected: No Exception to be thrown\nBut was: {0}", e));
            }
        }

        static bool AreEqual(object a, object b, bool compareByProperties, bool compareTypes) {
            if(!compareByProperties) return object.Equals(a, b);
            if(a == null && b == null) return true;
            if(a == null || b == null) return false;
            if(compareTypes && a.GetType() != b.GetType()) return false;
            foreach(PropertyInfo aProperty in a.GetType().GetProperties()) {
                PropertyInfo bProperty = b.GetType().GetProperty(aProperty.Name);
                if(bProperty == null) return false;
                if(!object.Equals(aProperty.GetValue(a, null), bProperty.GetValue(b, null))) return false;
            }
            return true;
        }
    }
}