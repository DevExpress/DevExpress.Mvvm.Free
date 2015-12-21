using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace DevExpress.Mvvm.Native {
    public class DotNetNativeAssistant {
        #region properties
        static List<DotNetNativeAssistantElement> Elements { get; set; } = new List<DotNetNativeAssistantElement>();

        private static bool isEnabled = false;
        public static bool IsEnabled {
            get { return isEnabled; }
            set {
                isEnabled = value;
                Application.Current.Suspending -= OnApplicationSuspending;
                Elements.Clear();
                if(isEnabled)
                    Application.Current.Suspending += OnApplicationSuspending;
            }
        }
        #endregion
        #region add element methods
        public static void AddTypeElement(Type genericType, Type[] genericTypeParameters, DotNetNativePolicy activate = null, DotNetNativePolicy browse = null, DotNetNativePolicy dynamic = null,
            DotNetNativePolicy serialize = null, DotNetNativePolicy dataContractSerializer = null, DotNetNativePolicy dataContractJsonSerializer = null,
            DotNetNativePolicy xmlSerializer = null, DotNetNativePolicy marshalObject = null, DotNetNativePolicy marshalDelegate = null, DotNetNativePolicy marshalStructure = null) {
            if(!IsEnabled) return;
            AddTypeElement(GetGenericTypeName(genericType, genericTypeParameters), activate, browse, dynamic, serialize, dataContractSerializer, dataContractJsonSerializer, xmlSerializer, marshalObject, marshalDelegate, marshalStructure);

        }
        public static DotNetNativeAssistantTypeElement AddTypeElement(string typeName, DotNetNativePolicy activate = null, DotNetNativePolicy browse = null, DotNetNativePolicy dynamic = null,
            DotNetNativePolicy serialize = null, DotNetNativePolicy dataContractSerializer = null, DotNetNativePolicy dataContractJsonSerializer = null,
            DotNetNativePolicy xmlSerializer = null, DotNetNativePolicy marshalObject = null, DotNetNativePolicy marshalDelegate = null, DotNetNativePolicy marshalStructure = null) {
            if(!IsEnabled) return null;
            DotNetNativeAssistantTypeElement typeElement = FindTypeElement(typeName);
            if(typeElement == null) {
                typeElement = new DotNetNativeAssistantTypeElement() { TypeName = typeName };
                Elements.Add(typeElement);
            } else {
                if(!typeElement.IsEmpty)
                    typeElement.Clear();
            }
            typeElement.Activate = activate;
            typeElement.Browse = browse;
            typeElement.Dynamic = dynamic;
            typeElement.Serialize = serialize;
            typeElement.DataContractSerializer = dataContractJsonSerializer;
            typeElement.DataContractJsonSerializer = dataContractJsonSerializer;
            typeElement.XmlSerializer = xmlSerializer;
            typeElement.MarshalObject = marshalObject;
            typeElement.MarshalDelegate = marshalDelegate;
            typeElement.MarshalStructure = marshalStructure;
            return typeElement;
        }
        public static void AddMethodInstantiationElement(Type type, string genericMethodName, Type[] typeArguments, DotNetNativePolicy browse = null, DotNetNativePolicy dynamic = null) {
            if(!IsEnabled) return;
            string typeName = FixTypeName(type.FullName);
            StringBuilder builder = new StringBuilder();
            ConcatGenericArguments(typeArguments, builder);
            string arguments = builder.ToString();
            AddMethodInstantiationElement(typeName, genericMethodName, arguments, browse, dynamic);
        }
        public static void AddMethodInstantiationElement(string typeName, string methodName, string arguments, DotNetNativePolicy browse = null, DotNetNativePolicy dynamic = null) {
            DotNetNativeAssistantTypeElement element = FindTypeElement(typeName);
            if(element == null) {
                element = new DotNetNativeAssistantTypeElement() { TypeName = typeName };
                Elements.Add(element);
            } else {
                if(element.IsEmpty)
                    return;
            }
            DotNetNativeAssistantMethodInstantiationElement methodInstantiation = element.FindMethodInstantiationElement(typeName, arguments);
            if(methodInstantiation == null) {
                methodInstantiation = new DotNetNativeAssistantMethodInstantiationElement(element) { MethodName = methodName, Arguments = arguments };
                element.MethodInstantiationElements.Add(methodInstantiation);
            }
            methodInstantiation.Browse = browse;
            methodInstantiation.Dynamic = dynamic;
        }
        public static void AddTypeElement(object genericObject) {
            if(!IsEnabled) return;
            AddTypeElement(genericObject.GetType().GetGenericTypeDefinition(), genericObject.GetType().GetGenericArguments(), activate: DotNetNativePolicy.Public, dynamic: DotNetNativePolicy.RequiredPublic);
        }
        public static void AddPropertyElement(string typeName, string propertyName, DotNetNativeMemberPolicy browse = null, DotNetNativeMemberPolicy dynamic = null, DotNetNativeMemberPolicy serialize = null) {
            if(!IsEnabled) return;
            DotNetNativeAssistantTypeElement element = FindTypeElement(typeName);
            if(element == null) {
                element = new DotNetNativeAssistantTypeElement() { TypeName = typeName };
                Elements.Add(element);
            }
            else {
                if(element.IsEmpty)
                    return;
            }
            DotNetNativeAssistantPropertyElement property = element.FindPropertyElement(propertyName);
            if(property == null) {
                property = new DotNetNativeAssistantPropertyElement(element) { PropertyName = propertyName };
                element.PropertyElements.Add(property);
            }
            property.Browse = browse;
            property.Dynamic = dynamic;
            property.Serialize = serialize;
        }
        public static void AddFieldElement(string typeName, string fieldName, DotNetNativeMemberPolicy browse = null, DotNetNativeMemberPolicy dynamic = null, DotNetNativeMemberPolicy serialize = null) {
            if(!IsEnabled) return;
            DotNetNativeAssistantTypeElement element = FindTypeElement(typeName);
            if(element == null) {
                element = new DotNetNativeAssistantTypeElement() { TypeName = typeName };
                Elements.Add(element);
            }
            else {
                if(element.IsEmpty)
                    return;
            }
            DotNetNativeAssistantFieldElement field = element.FindFieldElement(fieldName);
            if(field == null) {
                field = new DotNetNativeAssistantFieldElement(element) { FieldName = fieldName };
                element.FieldElements.Add(field);
            }
            field.Browse = browse;
            field.Dynamic = dynamic;
            field.Serialize = serialize;
        }

        #endregion

        public static void WriteResults(TextWriter stream = null) {
            TextWriter actualStream = stream ?? new DebugTextWriter();
            if(actualStream is DebugTextWriter) {
                actualStream.WriteLine("==========DotNetNativeAssistant directives=======");
            }
            foreach(DotNetNativeAssistantElement element in Elements) {
                element.Write(actualStream);
            }
            if(actualStream is DebugTextWriter) {
                actualStream.WriteLine("==========DotNetNativeAssistant directives=======");
            }
        }

        private static void OnApplicationSuspending(object sender, SuspendingEventArgs e) {
            WriteResults();
        }

        static DotNetNativeAssistantTypeElement FindTypeElement(string typeName) {
            return Elements.OfType<DotNetNativeAssistantTypeElement>().FirstOrDefault(x => x.TypeName == typeName);
        }
        private static string GetGenericTypeName(Type genericType, Type[] genericTypeParameters) {
            StringBuilder builder = new StringBuilder();
            builder.Append(FixTypeName(genericType.FullName));
            builder.Append("{");
            ConcatGenericArguments(genericTypeParameters, builder);
            builder.Append("}");
            return builder.ToString();
        }
        static void ConcatGenericArguments(Type[] typeArguments, StringBuilder builder) {
            for(int i = 0; i < typeArguments.Length; i++) {
                builder.Append(FixParameterTypeName(typeArguments[i]));
                if(i != typeArguments.Length - 1)
                    builder.Append(",");
            }
        }
        static string FixTypeName(string typeName) {
            return typeName.Split('`').FirstOrDefault().Replace('+', '.');
        }
        static string FixParameterTypeName(Type type) {
            if(!type.IsGenericType())
                return FixTypeName(type.FullName);
            var builder = new StringBuilder();
            builder.Append(FixTypeName(type.GetGenericTypeDefinition().FullName));
            builder.Append("{");
            ConcatGenericArguments(type.GetGenericArguments(), builder);
            builder.Append("}");
            return builder.ToString();
        }
    }
    public class DotNetNativeAssistantException : InvalidOperationException {
        public DotNetNativeAssistantException(string message) : base(message) { }
    }
    public abstract class DotNetNativeAssistantElement {
        public DotNetNativePolicy Activate { get; set; }
        public DotNetNativePolicy Browse { get; set; }
        public DotNetNativePolicy Dynamic { get; set; }
        public DotNetNativePolicy Serialize { get; set; }
        public DotNetNativePolicy DataContractSerializer { get; set; }
        public DotNetNativePolicy DataContractJsonSerializer { get; set; }
        public DotNetNativePolicy XmlSerializer { get; set; }
        public DotNetNativePolicy MarshalObject { get; set; }
        public DotNetNativePolicy MarshalDelegate { get; set; }
        public DotNetNativePolicy MarshalStructure { get; set; }
        private void WritePolicy(TextWriter stream, string policyName, DotNetNativePolicy policyValue) {
            if(policyValue != null) {
                stream.Write(" ");
                stream.Write(policyName);
                stream.Write("=\"");
                string textValue = policyValue.ToString();
                if(textValue.StartsWith("Required") && textValue.Length != 8)
                    textValue = textValue.Insert(8, " ");
                stream.Write(textValue);
                stream.Write("\"");
            }
        }
        protected void WritePolicies(TextWriter stream) {
            WritePolicy(stream, "Activate", Activate);
            WritePolicy(stream, "Browse", Browse);
            WritePolicy(stream, "Dynamic", Dynamic);
            WritePolicy(stream, "Serialize", Serialize);
            WritePolicy(stream, "DataContractSerializer", DataContractSerializer);
            WritePolicy(stream, "DataContractJsonSerializer", DataContractJsonSerializer);
            WritePolicy(stream, "XmlSerializer", XmlSerializer);
            WritePolicy(stream, "MarshalObject", MarshalObject);
            WritePolicy(stream, "MarshalDelegate", MarshalDelegate);
            WritePolicy(stream, "MarshalStructure", MarshalStructure);
        }

        public abstract void Write(TextWriter stream);
    }
    public class DotNetNativeAssistantPropertyElement : DotNetNativeAssistantElement {
        public string PropertyName { get; set; }
        private DotNetNativeAssistantElement Element { get; set; }
        public DotNetNativeAssistantPropertyElement(DotNetNativeAssistantElement element) {
            Element = element;
        }
        public override void Write(TextWriter stream) {
            stream.Write("    <Property Name=\"");
            stream.Write(PropertyName);
            stream.Write("\"");
            WritePolicies(stream);
            stream.WriteLine(" />");
        }
    }
    public class DotNetNativeAssistantFieldElement : DotNetNativeAssistantElement {
        public string FieldName { get; set; }
        private DotNetNativeAssistantElement Element { get; set; }
        public DotNetNativeAssistantFieldElement(DotNetNativeAssistantElement element) {
            Element = element;
        }
        public override void Write(TextWriter stream) {
            stream.Write("    <Field Name=\"");
            stream.Write(FieldName);
            stream.Write("\"");
            WritePolicies(stream);
            stream.WriteLine(" />");
        }
    }
    public class DotNetNativeAssistantTypeElement : DotNetNativeAssistantElement {
        public string TypeName { get; set; }
        public bool IsEmpty { get { return PropertyElements.Count == 0 && MethodInstantiationElements.Count == 0 && FieldElements.Count == 0; } }
        public List<DotNetNativeAssistantFieldElement> FieldElements { get; private set; } = new List<DotNetNativeAssistantFieldElement>();
        public List<DotNetNativeAssistantPropertyElement> PropertyElements { get; private set; } = new List<DotNetNativeAssistantPropertyElement>();
        public List<DotNetNativeAssistantMethodInstantiationElement> MethodInstantiationElements { get; private set; } = new List<DotNetNativeAssistantMethodInstantiationElement>();
        public DotNetNativeAssistantPropertyElement FindPropertyElement(string propertyName) {
            return PropertyElements.FirstOrDefault(x => x.PropertyName == propertyName);
        }
        public void Clear() {
            MethodInstantiationElements.Clear();
            FieldElements.Clear();
            PropertyElements.Clear();
        }
        public DotNetNativeAssistantMethodInstantiationElement FindMethodInstantiationElement(string methodName, string arguments) {
            return MethodInstantiationElements.FirstOrDefault(x => x.Arguments == arguments && x.MethodName == methodName);
        }
        public DotNetNativeAssistantFieldElement FindFieldElement(string fieldName) {
            return FieldElements.FirstOrDefault(x => x.FieldName == fieldName);
        }
        public override void Write(TextWriter stream) {
            stream.Write("<Type Name=\"");
            stream.Write(TypeName);
            stream.Write("\"");
            WritePolicies(stream);
            if(IsEmpty) {
                stream.WriteLine(" />");
            }
            else {
                stream.WriteLine(">");
                foreach(DotNetNativeAssistantPropertyElement property in PropertyElements) {
                    property.Write(stream);
                }
                foreach(DotNetNativeAssistantMethodInstantiationElement methodInstantiation in MethodInstantiationElements) {
                    methodInstantiation.Write(stream);
                }
                foreach(DotNetNativeAssistantFieldElement field in FieldElements) {
                    field.Write(stream);
                }
                stream.WriteLine("</Type>");
            }
        }
    }
    public class DotNetNativeAssistantMethodInstantiationElement : DotNetNativeAssistantElement {
        private DotNetNativeAssistantElement Element { get; set; }
        public string MethodName { get; set; }
        public string Arguments { get; set; }
        public DotNetNativeAssistantMethodInstantiationElement(DotNetNativeAssistantElement element) {
            Element = element;
        }
        public override void Write(TextWriter stream) {
            stream.Write("    <MethodInstantiation Name=\"");
            stream.Write(MethodName);
            stream.Write("\" Arguments=\"");
            stream.Write(Arguments);
            stream.Write("\"");
            WritePolicies(stream);
            stream.WriteLine(" />");
        }
    }
    public class DotNetNativePolicyBase {
        internal readonly string name;
        public DotNetNativePolicyBase(string name) {
            this.name = name;
        }
        public override string ToString() {
            return name;
        }
    }
    public class DotNetNativePolicy : DotNetNativePolicyBase {
        public static DotNetNativePolicy All { get; } = new DotNetNativePolicy(nameof(All));
        public static DotNetNativePolicy Public { get; } = new DotNetNativePolicy(nameof(Public));
        public static DotNetNativePolicy PublicAndInternal { get; } = new DotNetNativePolicy(nameof(PublicAndInternal));
        public static DotNetNativePolicy RequiredPublic { get; } = new DotNetNativePolicy(nameof(RequiredPublic));
        public static DotNetNativePolicy RequiredPublicAndInternal { get; } = new DotNetNativePolicy(nameof(RequiredPublicAndInternal));
        public static DotNetNativePolicy RequiredAll { get; } = new DotNetNativePolicy(nameof(RequiredAll));
        public static DotNetNativePolicy Auto { get; } = new DotNetNativePolicy(nameof(Auto));
        public static DotNetNativePolicy Excluded { get; } = new DotNetNativePolicy(nameof(Excluded));
        public static DotNetNativePolicy Included { get; } = new DotNetNativePolicy(nameof(Included));
        public static DotNetNativePolicy Required { get; } = new DotNetNativePolicy(nameof(Required));
        public DotNetNativePolicy(string name) : base(name) { }
    }

    public class DotNetNativeMemberPolicy : DotNetNativePolicyBase {
        public static DotNetNativeMemberPolicy Auto { get; } = new DotNetNativeMemberPolicy(nameof(Auto));
        public static DotNetNativeMemberPolicy Excluded { get; } = new DotNetNativeMemberPolicy(nameof(Excluded));
        public static DotNetNativeMemberPolicy Included { get; } = new DotNetNativeMemberPolicy(nameof(Included));
        public static DotNetNativeMemberPolicy Required { get; } = new DotNetNativeMemberPolicy(nameof(Required));
        public DotNetNativeMemberPolicy(string name) : base(name) { }
        public static implicit operator DotNetNativePolicy(DotNetNativeMemberPolicy member) {
            if(member == null)
                return null;
            return new DotNetNativePolicy(member.name);
        }
    }

    public class DebugTextWriter : TextWriter {
         public override Encoding Encoding {
            get {
                return Encoding.Unicode;
            }
        }
        public override void Write(string value) {
            Debug.Write(value);
        }
        public override void Write(char value) {
            Debug.Write(value);
        }
        public override void WriteLine(string value) {
            Debug.WriteLine(value);
        }
    }
}