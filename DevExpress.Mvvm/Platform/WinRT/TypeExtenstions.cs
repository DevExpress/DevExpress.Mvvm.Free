using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

namespace DevExpress.Mvvm.Native {
    public static class TypeExtensions {
        public static Type GetBaseType(this Type sourceType) {
            if(sourceType == null)
                throw new ArgumentException("sourceType");
            return sourceType.GetTypeInfo().BaseType;
        }
        static bool CheckParametersTypes(ParameterInfo[] parameters, Type[] types) {
            if(parameters.Length != types.Length)
                return false;
            for(int i = 0; i < types.Length; i++)
                if(parameters[i].ParameterType != types[i])
                    return false;
            return true;
        }
        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo) {
            if(propertyInfo == null)
                throw new ArgumentException("propertyInfo");
            return propertyInfo.GetMethod;
        }
        public static bool GetIsEnum(this Type sourceType) {
            if(sourceType == null)
                throw new ArgumentException("sourceType");
            return sourceType.GetTypeInfo().IsEnum;
        }
        public static bool GetIsPrimitive(this Type sourceType) {
            if(sourceType == null)
                throw new ArgumentException("sourceType");
            return sourceType.GetTypeInfo().IsPrimitive;
        }
        public static bool GetIsValueType(this Type sourceType) {
            if(sourceType == null)
                throw new ArgumentException("sourceType");
            return sourceType.GetTypeInfo().IsValueType;
        }

        public static FieldInfo[] GetPublicStaticFields(this Type type) {
            return type.GetTypeInfo().DeclaredFields.Where(fi => fi.IsStatic).ToArray();
        }
        public static bool IsSubclassOf(this Type type, Type c) {
            return type.GetTypeInfo().IsSubclassOf(type);
        }
        public static bool IsDefined(this Type type, Type attributeType, bool inherit) {
            return type.GetTypeInfo().IsDefined(attributeType, inherit);
        }
        public static bool IsClass(this Type type) {
            return type.GetTypeInfo().IsClass;
        }
        public static Assembly GetAssembly(this Type type) {
            return type.GetTypeInfo().Assembly;
        }
        public static Type GetReflectedType(this MethodInfo mi) {
            return mi.DeclaringType;
        }
        public static Type[] GetExportedTypes(this Assembly assembly) {
            return assembly.ExportedTypes.ToArray();
        }
        public static MethodInfo GetSetMethod(this PropertyInfo pi) {
            return pi.SetMethod;
        }
        static bool IsInstanceProperty(PropertyInfo pi) {
            return pi != null && pi.GetMethod != null && !pi.GetMethod.IsStatic;
        }
        public static bool IsGenericType(this Type type) {
            return type.GetTypeInfo().IsGenericType;
        }
        public static bool IsPrimitive(this Type type) {
            return type.GetTypeInfo().IsPrimitive;
        }
        public static bool IsValueType(this Type type) {
            return type.GetTypeInfo().IsValueType;
        }
        public static bool IsPublic(this Type type) {
            return type.GetTypeInfo().IsPublic;
        }
        public static bool IsNestedPublic(this Type type) {
            return type.GetTypeInfo().IsNestedPublic;
        }
        public static bool IsEnum(this Type type) {
            return type.GetTypeInfo().IsEnum;
        }
        public static bool IsInterface(this Type type) {
            return type.GetTypeInfo().IsInterface;
        }
        static Dictionary<Type, TypeCode> typeCodes;
        static Dictionary<TypeCode, Type> types;
        public static TypeCode GetTypeCode(this Type type) {
            if (type == null)
                return TypeCode.Empty;
            if (typeCodes == null) {
                typeCodes = new Dictionary<Type, TypeCode>();
                typeCodes[typeof(Boolean)] = TypeCode.Boolean;
                typeCodes[typeof(Byte)] = TypeCode.Byte;
                typeCodes[typeof(Char)] = TypeCode.Char;
                typeCodes[typeof(DateTime)] = TypeCode.DateTime;
                typeCodes[typeof(Decimal)] = TypeCode.Decimal;
                typeCodes[typeof(DBNull)] = TypeCode.DBNull;
                typeCodes[typeof(Double)] = TypeCode.Double;
                typeCodes[typeof(Int16)] = TypeCode.Int16;
                typeCodes[typeof(Int32)] = TypeCode.Int32;
                typeCodes[typeof(Int64)] = TypeCode.Int64;
                typeCodes[typeof(SByte)] = TypeCode.SByte;
                typeCodes[typeof(Single)] = TypeCode.Single;
                typeCodes[typeof(String)] = TypeCode.String;
                typeCodes[typeof(UInt16)] = TypeCode.UInt16;
                typeCodes[typeof(UInt32)] = TypeCode.UInt32;
                typeCodes[typeof(UInt64)] = TypeCode.UInt64;
            }
            TypeCode typeCode;
            if (typeCodes.TryGetValue(type, out typeCode))
                return typeCode;
            return TypeCode.Object;
        }
        public static Type ToType(this TypeCode typeCode) {
            if (types == null) {
                types = new Dictionary<TypeCode, Type>();
                types[TypeCode.Boolean] = typeof(Boolean);
                types[TypeCode.Byte] = typeof(Byte);
                types[TypeCode.Char] = typeof(Char);
                types[TypeCode.DateTime] = typeof(DateTime);
                types[TypeCode.Decimal] = typeof(Decimal);
                types[TypeCode.DBNull] = typeof(DBNull);
                types[TypeCode.Double] = typeof(Double);
                types[TypeCode.Int16] = typeof(Int16);
                types[TypeCode.Int32] = typeof(Int32);
                types[TypeCode.Int64] = typeof(Int64);
                types[TypeCode.SByte] = typeof(SByte);
                types[TypeCode.Single] = typeof(Single);
                types[TypeCode.String] = typeof(String);
                types[TypeCode.UInt16] = typeof(UInt16);
                types[TypeCode.UInt32] = typeof(UInt32);
                types[TypeCode.UInt64] = typeof(UInt64);
            }
            Type type;
            if (types.TryGetValue(typeCode, out type))
                return type;
            return typeof(object);
        }
        internal static bool ImplementInterface(this Type type, Type t) {
            while (type != null) {
                Type[] interfaces = type.GetTypeInfo().ImplementedInterfaces.ToArray();
                if (interfaces == null) {
                    for (int i = 0; i < interfaces.Length; i++) {
                        if (interfaces[i] == t || (interfaces[i] != null && interfaces[i].ImplementInterface(t))) {
                            return true;
                        }
                    }
                }
                type = type.GetTypeInfo().BaseType;
            }
            return false;
        }
        #region event
        const string EventAddMethodPrefix = "add_";
        const string EventRemoveMethodPrefix = "remove_";

        public static ParameterInfo[] GetEventHandlerParameters(this EventInfo eventInfo) {
            return eventInfo.EventHandlerType.GetTypeInfo().GetDeclaredMethod("Invoke").GetParameters();
        }
        public static object AddEventHandlerEx(this EventInfo eventInfo, object target, Delegate handler) {
            MethodInfo addEventHandlerMethod = eventInfo.DeclaringType.GetRuntimeMethod(EventAddMethodPrefix + eventInfo.Name, new Type[] { eventInfo.EventHandlerType });
            return addEventHandlerMethod.Invoke(target, new object[] { handler }) ?? handler;
        }
        public static void RemoveEventHandlerEx(this EventInfo eventInfo, object target, object handlerRegistrationToken) {
            MethodInfo removeEventHandlerMethod =
                eventInfo.DeclaringType.GetRuntimeMethod(EventRemoveMethodPrefix + eventInfo.Name, new Type[] { typeof(EventRegistrationToken) }) ??
                eventInfo.DeclaringType.GetRuntimeMethod(EventRemoveMethodPrefix + eventInfo.Name, new Type[] { eventInfo.EventHandlerType });
            removeEventHandlerMethod.Invoke(target, new object[] { handlerRegistrationToken });
        }
        #endregion
    }
    public static class StringExtensions {
        public static StringComparison InvariantCultureComparison { get { return StringComparison.Ordinal; } }
        public static StringComparison InvariantCultureIgnoreCaseComparison { get { return StringComparison.OrdinalIgnoreCase; } }
        public static string ToLowerInvariantCulture(this string str) {
            return str.ToLowerInvariant();
        }
        public static string ToUpperInvariantCulture(this string str) {
            return str.ToUpperInvariant();
        }
        public static string ToLowerCurrentCulture(this string str) {
            return str.ToLower();
        }
        public static string ToUpperCurrentCulture(this string str) {
            return str.ToUpper();
        }
        public static UnicodeCategory GetUnicodeCategory(this char c) {
            return CharUnicodeInfo.GetUnicodeCategory(c);
        }
    }
    public enum TypeCode {
        Empty = 0,
        Object = 1,
        DBNull = 2,
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 15,
        DateTime = 16,
        String = 18,
    }
    [AttributeUsage(AttributeTargets.All)]
    public sealed class BrowsableAttribute : Attribute {
        public static readonly BrowsableAttribute Default = Yes;
        public static readonly BrowsableAttribute No = new BrowsableAttribute(false);
        public static readonly BrowsableAttribute Yes = new BrowsableAttribute(true);

        private bool browsable = true;

        public BrowsableAttribute(bool browsable) {
            this.browsable = browsable;
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            BrowsableAttribute attribute = obj as BrowsableAttribute;
            return ((attribute != null) && (attribute.Browsable == this.browsable));
        }
        public override int GetHashCode() {
            return this.browsable.GetHashCode();
        }

        public bool Browsable { get { return this.browsable; } }
    }
}