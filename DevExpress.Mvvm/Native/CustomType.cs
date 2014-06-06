using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;



namespace DevExpress.Mvvm.Native {
    class CustomType : Type {
        static Type[] GetParameterTypes(Type[] types) {
            return types ?? EmptyTypes;
        }

        readonly Type type;
        readonly IEnumerable<PropertyInfo> customProperties;
        public CustomType(Type type, IEnumerable<PropertyInfo> customProperties) {
            this.type = type;
            this.customProperties = customProperties;
        }
        public override Assembly Assembly { get { return type.Assembly; } }
        public override string AssemblyQualifiedName { get { return type.AssemblyQualifiedName; } }
        public override Type BaseType { get { return type.BaseType; } }
        public override string FullName { get { return type.FullName; } }
        public override Guid GUID { get { return type.GUID; } }
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) { return type.GetConstructors(); }
        public override Type GetElementType() { return type.GetElementType(); }
        public override EventInfo GetEvent(string name, BindingFlags bindingAttr) { return type.GetEvent(name, bindingAttr); }
        public override EventInfo[] GetEvents(BindingFlags bindingAttr) { return type.GetEvents(bindingAttr); }
        public override FieldInfo GetField(string name, BindingFlags bindingAttr) { return type.GetField(name, bindingAttr); }
        public override FieldInfo[] GetFields(BindingFlags bindingAttr) { return type.GetFields(bindingAttr); }
        public override Type GetInterface(string name, bool ignoreCase) { return type.GetInterface(name, ignoreCase); }
        public override Type[] GetInterfaces() { return type.GetInterfaces(); }
        public override MemberInfo[] GetMembers(BindingFlags bindingAttr) { return type.GetMembers(bindingAttr); }
        public override MethodInfo[] GetMethods(BindingFlags bindingAttr) { return type.GetMethods(bindingAttr); }
        public override Type GetNestedType(string name, BindingFlags bindingAttr) { return type.GetNestedType(name, bindingAttr); }
        public override Type[] GetNestedTypes(BindingFlags bindingAttr) { return type.GetNestedTypes(bindingAttr); }
        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) { return type.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters); }
        public override Module Module { get { return type.Module; } }
        public override string Namespace { get { return type.Namespace; } }
        public override Type UnderlyingSystemType { get { return type.UnderlyingSystemType; } }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) { return type.GetCustomAttributes(attributeType, inherit); }
        public override object[] GetCustomAttributes(bool inherit) { return type.GetCustomAttributes(inherit); }
        public override bool IsDefined(Type attributeType, bool inherit) { return type.IsDefined(attributeType, inherit); }
        public override string Name { get { return type.Name; } }

        protected override TypeAttributes GetAttributeFlagsImpl() { throw new NotImplementedException(); }
        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) { return type.GetConstructor(bindingAttr, binder, types, modifiers); }
        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) { return type.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers); }
        protected override bool HasElementTypeImpl() { return type.HasElementType; }
        protected override bool IsArrayImpl() { return type.IsArray; }
        protected override bool IsByRefImpl() { return type.IsByRef; }
        protected override bool IsCOMObjectImpl() { return type.IsCOMObject; }
        protected override bool IsPointerImpl() { return type.IsPointer; }
        protected override bool IsPrimitiveImpl() { return type.IsPrimitive; }


        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) {
            PropertyInfo[] result = type.GetProperties(bindingAttr);
            if(customProperties.Any() && IsVaidBindingAttr(bindingAttr))
                result = result.Concat(customProperties).ToArray();
            return result;
        }
        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
            types = GetParameterTypes(types);
            PropertyInfo result = type.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
            if(result == null && IsVaidBindingAttr(bindingAttr) && types.Length == 0)
                result = customProperties.FirstOrDefault(x => x.Name == name);
            return result;
        }
        bool IsVaidBindingAttr(BindingFlags bindingAttr) {
            return bindingAttr.HasFlag(BindingFlags.Instance) && bindingAttr.HasFlag(BindingFlags.Public);
        }
    }
}