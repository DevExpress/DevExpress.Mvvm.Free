#if !DXCORE3
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Security;
using System.Linq;
using DevExpress.Mvvm.UI.Native;
using System.IO;

namespace DevExpress {

    public class DependencyPropertiesConsistencyChecker {
        [SecuritySafeCritical]
        public static void CheckDependencyPropertiesConsistencyForAssembly(Assembly assembly, Dictionary<Type, Type[]> genericSubstitutes = null) {
            AppDomainSetup appDomainSetup = new AppDomainSetup();
            appDomainSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            AppDomain dom = AppDomain.CreateDomain("ConsistencyChecker", AppDomain.CurrentDomain.Evidence, appDomainSetup);
            dom.AssemblyResolve += dom_AssemblyResolve;
            dom.SetData("assembly", assembly);
            dom.SetData("genericSubstitutes", genericSubstitutes);
            try {
                dom.DoCallBack(DoCheck);
                dom.AssemblyResolve -= dom_AssemblyResolve;
            } finally {
                AppDomain.Unload(dom);
            }
        }

        static Assembly dom_AssemblyResolve(object sender, ResolveEventArgs args) {
            if(args.Name.ToLower().Contains("nunit.framework"))
                return Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, "nunit.framework.dll"));
            return null;
        }
        static void DoCheck() {
            new DependencyPropertiesConsistencyChecker().CheckDependencyPropertiesConsistencyForAssemblyCore((Assembly)AppDomain.CurrentDomain.GetData("assembly"), (Dictionary<Type, Type[]>)AppDomain.CurrentDomain.GetData("genericSubstitutes"));
        }
        List<string> errors = new List<string>();
        DependencyPropertiesConsistencyChecker() {
        }
        public void CheckDependencyPropertiesConsistencyForAssemblyCore(Assembly assembly, Dictionary<Type, Type[]> genericSubstitutes = null) {
            foreach(Type type in assembly.GetTypes()) {
                if(!type.IsPublic)
                    continue;
                Type actualType = type;
                if(type.IsGenericType && genericSubstitutes != null)
                    actualType = GenerateTypeFromGeneric(type, genericSubstitutes);
                CheckDependencyPropertiesConsistencyForType(actualType);
            }
            if(errors.Count > 0) {
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine();
                for(int i = 0; i < errors.Count; i++) {
                    messageBuilder.AppendLine(errors[i]);
                }
                Assert.Fail(messageBuilder.ToString());
            }
        }
        Type GenerateTypeFromGeneric(Type type, Dictionary<Type, Type[]> genericSubstitutes) {
            if(!genericSubstitutes.ContainsKey(type))
                return type;
            Type constructedType = type.MakeGenericType(genericSubstitutes[type]);
            return constructedType;
        }
        void CheckDependencyPropertiesConsistencyForType(Type type) {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach(var fieldInfo in fields) {
                if(fieldInfo.FieldType != typeof(DependencyProperty)) continue;
                if(fieldInfo.GetCustomAttributes(typeof(IgnoreDependencyPropertiesConsistencyCheckerAttribute), false).Any())
                    continue;
                if(!fieldInfo.IsStatic) {
                    DependencyPropertyFieldInfoShouldBeStatic(fieldInfo);
                    continue;
                }
                if(fieldInfo.DeclaringType.ContainsGenericParameters) {
                    DependencyPropertyFieldNotTestedInGenericClass(fieldInfo);
                    continue;
                }
                var accessModifier = (DPAccessModifierAttribute)fieldInfo.GetCustomAttributes(typeof(DPAccessModifierAttribute), false).FirstOrDefault();
                if(accessModifier != null)
                    CheckDependencyPropertyConsistencyForField(fieldInfo, accessModifier);
                else
                    CheckDependencyPropertyConsistencyForField(fieldInfo);
            }
        }
        void CheckDependencyPropertyConsistencyForField(FieldInfo fieldInfo, DPAccessModifierAttribute accessModifier) {
            DependencyPropertyFieldInfoShouldBeReadonly(fieldInfo);
            SetterVisibilityShouldBeStrongerThenOrEqualsToGetterVisibility(fieldInfo, accessModifier);
            DependencyPropertyFieldVisibilityShouldBeEqualsToGetterVisibility(fieldInfo, accessModifier);
            DependencyProperty property = DependencyPropertyShouldBeRegistered(fieldInfo);
            if(property == null) return;
            DependencyPropertyFieldInfoNameShouldMatchPropertyName(fieldInfo, property);
            CheckDependencyPropertyOwner(fieldInfo, property);
            CheckReadOnlyProperty(fieldInfo, property, accessModifier);
        }
        void CheckDependencyPropertyConsistencyForField(FieldInfo fieldInfo) {
            if(fieldInfo.Name == "IsActiveExProperty") return;
            DependencyPropertyFieldInfoShouldBePublic(fieldInfo);
            DependencyPropertyFieldInfoShouldBeReadonly(fieldInfo);
            DependencyProperty property = DependencyPropertyShouldBeRegistered(fieldInfo);
            if(property == null) return;
            DependencyPropertyFieldInfoNameShouldMatchPropertyName(fieldInfo, property);
            CheckDependencyPropertyOwner(fieldInfo, property);
            CheckReadOnlyProperty(fieldInfo, property, null);
            PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(fieldInfo.ReflectedType)[property.Name];
            if(propertyDescriptor != null) {
                PropertyInfo propertyInfo = fieldInfo.ReflectedType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                if(propertyInfo == null) {
                    DependencyPropertyOwnerShouldHavePublicProperty(fieldInfo);
                } else {
                    if(propertyInfo.PropertyType != property.PropertyType)
                        DependencyPropertyTypeShouldMatchGetterType(fieldInfo);
                    if(propertyInfo.GetSetMethod() == null && !property.ReadOnly) {
                        DependencyPropertyOwnerShouldHavePublicSetter(fieldInfo);
                    }
                    if(propertyInfo.GetSetMethod() != null && property.ReadOnly) {
                        DependencyPropertyOwnerShouldNotHavePublicSetter(fieldInfo);
                    }

                }
            } else {
                MethodInfo getMethod = fieldInfo.ReflectedType.GetMethod("Get" + property.Name, BindingFlags.Static | BindingFlags.Public);
                if(getMethod == null)
                    DependencyPropertyOwnerShouldHavePublicGetter(fieldInfo);
                else if(getMethod.ReturnType != property.PropertyType)
                    DependencyPropertyTypeShouldMatchGetterType(fieldInfo);
                MethodInfo setMethod = fieldInfo.ReflectedType.GetMethod("Set" + property.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if(setMethod == null) {
                    DependencyPropertyOwnerShouldHaveSetter(fieldInfo);
                } else {
                    if(!setMethod.IsPublic && !property.ReadOnly) {
                        DependencyPropertyOwnerShouldHavePublicSetter(fieldInfo);
                    }
                    if(setMethod.IsPublic && property.ReadOnly) {
                        DependencyPropertyOwnerShouldNotHavePublicSetter(fieldInfo);
                    }
                }
            }
        }
        void CheckReadOnlyProperty(FieldInfo fieldInfo, DependencyProperty property, DPAccessModifierAttribute accessModifier) {
            if(!property.ReadOnly) return;
            FieldInfo keyFieldInfo = fieldInfo.ReflectedType.GetField(fieldInfo.Name + "Key", BindingFlags.Static | BindingFlags.NonPublic);
            if(keyFieldInfo == null) {
                ReadonlyDependencyPropertyOwnerShouldHaveDependencyPropertyKey(fieldInfo);
            } else {
                DependencyPropertyKeyFieldInfoShouldBeReadonly(fieldInfo);
                if(accessModifier != null) {
                    DependencyPropertyKeyFieldInfoShouldBePrivate(keyFieldInfo);
                    ReadOnlyPropertySetterShouldNotBePublic(fieldInfo, accessModifier);
                }
            }
        }
        void SetterVisibilityShouldBeStrongerThenOrEqualsToGetterVisibility(FieldInfo fieldInfo, DPAccessModifierAttribute accessModifier) {
            if(accessModifier.GetterVisibility.IsStrongerThen(accessModifier.SetterVisibility))
                AddError(GetFieldInfoDescription(fieldInfo) + "SetterVisibilityShouldBeStrongerThenOrEqualsToGetterVisibility");
        }
        void DependencyPropertyFieldVisibilityShouldBeEqualsToGetterVisibility(FieldInfo fieldInfo, DPAccessModifierAttribute accessModifier) {
            if(GetFieldVisibility(fieldInfo) != accessModifier.GetterVisibility)
                AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyFieldVisibilityShouldBeEqualsToGetterVisibility");
        }
        void ReadOnlyPropertySetterShouldNotBePublic(FieldInfo fieldInfo, DPAccessModifierAttribute accessModifier) {
            if(accessModifier.SetterVisibility == MemberVisibility.Public)
                AddError(GetFieldInfoDescription(fieldInfo) + "ReadOnlyPropertySetterShouldNotBePublic");
        }
        static MemberVisibility GetFieldVisibility(FieldInfo fieldInfo) {
            if(fieldInfo.IsFamilyOrAssembly) return MemberVisibility.ProtectedInternal;
            if(fieldInfo.IsAssembly) return MemberVisibility.Internal;
            if(fieldInfo.IsFamily) return MemberVisibility.Protected;
            if(fieldInfo.IsPrivate) return MemberVisibility.Private;
            if(fieldInfo.IsPublic) return MemberVisibility.Public;
            throw new InvalidOperationException();
        }
        static Hashtable PropertyFromName;
        static Type keyType;
        void CheckDependencyPropertyOwner(FieldInfo fieldInfo, DependencyProperty property) {
            if(fieldInfo.ReflectedType != property.OwnerType) {
                ExtractDPropertyInternals(property);
                if(!PropertyFromName.ContainsKey(CreateDPropertyKey(property.Name, fieldInfo.ReflectedType)))
                    DependencyPropertyOwnerTypeShouldMatchContainerType(fieldInfo);
            }
        }
        object CreateDPropertyKey(string name, Type type) {
            return Activator.CreateInstance(keyType, name, type);
        }
        void ExtractDPropertyInternals(DependencyProperty property) {
            if(PropertyFromName != null)
                return;
            FieldInfo hashInfo = typeof(DependencyProperty).GetField("PropertyFromName", BindingFlags.NonPublic | BindingFlags.Static);
            PropertyFromName = (Hashtable)hashInfo.GetValue(property);
            foreach(object key in PropertyFromName.Keys) {
                keyType = key.GetType();
                break;
            }
        }
        DependencyProperty DependencyPropertyShouldBeRegistered(FieldInfo fieldInfo) {
            var property = (DependencyProperty)fieldInfo.GetValue(null);
            if(property == null)
                AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyShouldBeRegistered");
            return property;
        }
        void DependencyPropertyFieldInfoShouldBePublic(FieldInfo fieldInfo) {
            if(!fieldInfo.IsPublic)
                AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyFieldInfoShouldBePublic");
        }
        void DependencyPropertyFieldNotTestedInGenericClass(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyFieldNotTestedInGenericClass");
        }
        void DependencyPropertyFieldInfoShouldBeStatic(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyFieldInfoShouldBeStatic");
        }
        void DependencyPropertyFieldInfoShouldBeReadonly(FieldInfo fieldInfo) {
            if(!fieldInfo.IsInitOnly)
                AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyFieldInfoShouldBeReadonly");
        }
        void DependencyPropertyKeyFieldInfoShouldBeReadonly(FieldInfo fieldInfo) {
            if(!fieldInfo.IsInitOnly)
                AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyKeyFieldInfoShouldBeReadonly");
        }
        void DependencyPropertyKeyFieldInfoShouldBePrivate(FieldInfo fieldInfo) {
            if(!fieldInfo.IsPrivate)
                AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyKeyFieldInfoShouldBePrivate");
        }
        void DependencyPropertyFieldInfoNameShouldMatchPropertyName(FieldInfo fieldInfo, DependencyProperty property) {
            if(fieldInfo.Name != property.Name + "Property")
                AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyFieldInfoNameShouldMatchPropertyName");
        }
        void DependencyPropertyOwnerTypeShouldMatchContainerType(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyOwnerTypeShouldMatchGetterType");
        }
        void DependencyPropertyTypeShouldMatchGetterType(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyOwnerTypeShouldMatchContainerType");
        }
        void ReadonlyDependencyPropertyOwnerShouldHaveDependencyPropertyKey(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "ReadonlyDependencyPropertyOwnerShouldHavePrivateOrInternalDependencyPropertyKey");
        }
        void DependencyPropertyOwnerShouldHavePublicProperty(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyOwnerShouldHavePublicProperty");
        }
        void DependencyPropertyOwnerShouldHavePublicGetter(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyOwnerShouldHavePublicGetter");
        }
        void DependencyPropertyOwnerShouldHaveSetter(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyOwnerShouldHaveSetter");
        }
        void DependencyPropertyOwnerShouldHavePublicSetter(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyOwnerShouldHavePublicSetter");
        }
        void DependencyPropertyOwnerShouldNotHavePublicSetter(FieldInfo fieldInfo) {
            AddError(GetFieldInfoDescription(fieldInfo) + "DependencyPropertyOwnerShouldNotHavePublicSetter");
        }
        void AddError(string error) {
            errors.Add(error);
        }
        string GetFieldInfoDescription(FieldInfo fieldInfo) {
            return string.Format("Owner type: {0}, Name: {1}: ", fieldInfo.ReflectedType.FullName, fieldInfo.Name);
        }
    }

    [TestFixture]
    public class DependencyPropertiesConsistencyTests {
        [Test]
        public void DependencyPropertiesConsistencyTest() {
            Dictionary<Type, Type[]> genericSubstitutes = new Dictionary<Type, Type[]>();
            DependencyPropertiesConsistencyChecker.CheckDependencyPropertiesConsistencyForAssembly(Assembly.GetExecutingAssembly(), genericSubstitutes);
        }
    }
}
#endif