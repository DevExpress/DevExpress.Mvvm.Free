namespace DevExpress.Mvvm.Internal {
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    static class XmlSerializerHelper {
#pragma warning disable DX0008
        const DtdProcessing XxeSafeDtdProcessing = DtdProcessing.Prohibit;
        const XmlResolver SsrfSafeXmlResolver = (XmlResolver)null;
        internal static XmlTextReader CreateTextReader(Stream stream) {
            var reader = new XmlTextReader(stream);
            reader.DtdProcessing = XxeSafeDtdProcessing;
            reader.XmlResolver = SsrfSafeXmlResolver;
            return reader;
        }
        static XmlTextReader CreateTextReader(TextReader textReader) {
            var reader = new XmlTextReader(textReader);
            reader.DtdProcessing = XxeSafeDtdProcessing;
            reader.XmlResolver = SsrfSafeXmlResolver;
            return reader;
        }
        public static T Deserialize<T>(Stream stream, Type[] extraTypes = null)
            where T : class {
            return Deserialize(stream, typeof(T), extraTypes) as T;
        }
        public static T Deserialize<T>(string xmlString, Type[] extraTypes = null)
            where T : class {
            return Deserialize(xmlString, typeof(T), extraTypes) as T;
        }
        public static object Deserialize(Stream stream, Type type, Type[] extraTypes = null) {
            try {
                var serializer = CreateSerializerCore(type, extraTypes);
                var xmlTextReader = CreateTextReader(stream);
                return DeserializeCore(serializer, xmlTextReader.EnsureTextReaderForXmlSerializer());
            } catch { return null; }
        }
        public static object Deserialize(string xmlString, Type type, Type[] extraTypes = null) {
            try {
                using(var textReader = new StringReader(xmlString)) {
                    var serializer = CreateSerializerCore(type, extraTypes);
                    var xmlTextReader = CreateTextReader(textReader);
                    return DeserializeCore(serializer, xmlTextReader.EnsureTextReaderForXmlSerializer());
                }
            } catch { return null; }
        }
        public static void Serialize<T>(Stream stream, object root, Type[] extraTypes = null)
            where T : class {
            Serialize(stream, root, typeof(T), extraTypes);
        }
        public static void Serialize(Stream stream, object root, Type type, Type[] extraTypes = null) {
            var serializer = CreateSerializerCore(type, extraTypes);
            SerializeCore(stream, root, serializer);
        }
        public static string Serialize<T>(T root, Type type) {
            using(var stream = new MemoryStream()) {
                var serializer = CreateSerializerCore(type, null);
                SerializeCore(stream, root, serializer);
                stream.Seek(0, SeekOrigin.Begin);
                using(var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
        static XmlSerializer CreateSerializerCore(Type type, Type[] extraTypes) {
            return new XmlSerializer(type, null, extraTypes, null, null, null);
        }
        static void SerializeCore(Stream stream, object root, XmlSerializer serializer) {
            serializer.Serialize(stream, root);
        }
        static object DeserializeCore(XmlSerializer serializer, XmlReader xmlTextReader) {
            if(xmlTextReader == null || !serializer.CanDeserialize(xmlTextReader))
                return null;
            return serializer.Deserialize(xmlTextReader);
        }
        static XmlTextReader EnsureTextReaderForXmlSerializer(this XmlTextReader reader) {
            reader.WhitespaceHandling = WhitespaceHandling.Significant;
            reader.Normalization = true;
            return reader;
        }
#pragma warning restore DX0008
    }
    static class DataContractSerializerHelper {
        public static string Serialize<T>(T value) {
            var serializer = new DataContractSerializer(typeof(T));
            using(var stream = new MemoryStream()) {
                serializer.WriteObject(stream, value);
                stream.Seek(0, SeekOrigin.Begin);
                using(var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
        public static T Deserialize<T>(string value) {
            if(string.IsNullOrEmpty(value))
                return default(T);
            try {
                using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(value))) {
#pragma warning disable DX0011
                    var serializer = new DataContractSerializer(typeof(T));
                    using(var reader = XmlSerializerHelper.CreateTextReader(stream)) {
                        return (T)serializer.ReadObject(reader);
                    }
#pragma warning restore DX0011
                }
            } catch { return default(T); }
        }
    }
}