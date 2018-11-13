using System; 
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Linq;
#if !DEMO && !FREE
using DevExpress.Utils;
#endif
using System.Windows.Interop;

#if DEMO
namespace DevExpress.Internal {
#elif MVVM
namespace DevExpress.Mvvm.UI.Native {
#else
namespace DevExpress.Xpf.Utils {
#endif
#if !FREE
    static class ApplicationHelper {
        public const string ManifestExtension = ".manifest";

        const int ExtensionLenth = 0;

        static string relativeManifestName;
        static string relativePath;

        static string EntryManifestName { get { return AssemblyHelper.EntryAssembly.ManifestModule + ManifestExtension; } }
#if !DEMO
        readonly static EnvironmentStrategy environmentStrategy;

        static ApplicationHelper() {
            environmentStrategy = GetEnvironmentStrategy();
            Environment.Initialize();
        }
        static EnvironmentStrategy Environment { get { return environmentStrategy; } }
        static EnvironmentStrategy GetEnvironmentStrategy() {
            if(BrowserInteropHelper.IsBrowserHosted) {
                if(DevExpress.Xpf.Core.ThemeManager.IgnoreManifest) {
                    return new IgnoreManifestStrategy();
                }
                return new BrowserStrategy();
            }
            return new IgnoreManifestStrategy();
        }
        public static List<string> GetAvailableAssemblies() {
            List<string> assemblies = new List<string>(AppDomain.CurrentDomain.GetAssemblies().Length);
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                assemblies.Add(assembly.FullName);
            }
            Assembly entryAssembly = AssemblyHelper.EntryAssembly;
            if(entryAssembly == null)
                return assemblies;

            AssemblyName[] assemblyNames = entryAssembly.GetReferencedAssemblies();
            foreach(AssemblyName asmName in assemblyNames) {
                assemblies.Add(asmName.FullName);
            }

            Environment.AddAvailableAssemblies(ref assemblies);
            return assemblies;
        }
#endif
        public static string RelativeManifestName {
            get {
                if(relativeManifestName == null) {
                    relativeManifestName = GetRelativeManifestName();
                }
                return relativeManifestName;
            }
        }
        public static string RelativePath {
            get {
                if(relativePath == null)
                    relativePath = GetRelativePath();
                return relativePath;
            }
        }

        static string GetRelativePath() {
            string escapedName = RelativeManifestName.Replace('\\', '/');
            return escapedName.Substring(0, escapedName.LastIndexOf('/') + 1);
        }

        static string GetRelativeManifestName() {
            if(BrowserInteropHelper.Source == null)
                return EntryManifestName;
            try {
                string xbapFileName = BrowserInteropHelper.Source.Segments[BrowserInteropHelper.Source.Segments.Length - 1];
                StreamResourceInfo info = Application.GetRemoteStream(new Uri(xbapFileName, UriKind.Relative));

                Func<XElement, bool> predicate = delegate(XElement xElement) {
                    return xElement.Name.LocalName == "dependentAssembly";
                };
                Func<XElement, string> selectCondition = delegate(XElement element) {
                    return element.Attribute("codebase").Value;
                };
                using(Stream stream = info.Stream) {
                    using(StreamReader reader = new StreamReader(stream)) {
                        return XmlHelper.GetElements<string>(predicate, selectCondition, reader.ReadToEnd()).Single();
                    }
                }
            } catch {
                return EntryManifestName;
            }
        }

        static string[] dependentAssembliesCache = null;

        public static bool IsManifestAvailable { get { return GetDependentAssemblies().Length > 0; } }

        public static string[] GetDependentAssemblies() {
            if(dependentAssembliesCache == null)
                dependentAssembliesCache = PopulateDependentAssemblies(RelativeManifestName);
            return dependentAssembliesCache;
        }
        internal static string[] GetDependentAssemblies(string manifestString) {
            return GetDependentAssemblies(manifestString, false);
        }
        internal static string[] GetDependentAssemblies(string manifestString, bool extensionOnly) {
            Func<XElement, bool> predicate = delegate(XElement xElement) {
                return xElement.Name.LocalName == "assemblyIdentity";
            };
            Func<XElement, string> selectCondition = delegate(XElement element) {
                return element.Attribute("name").Value;
            };
            try {
                IEnumerable<string> enumerable = XmlHelper.GetDescendants<string>(predicate, selectCondition, manifestString);
                List<string> result = new List<string>();
                foreach(string asmName in enumerable) {
                    if(asmName.Length > ExtensionLenth)
                        result.Add(asmName.Substring(0, asmName.Length - ExtensionLenth));
                }
                return result.ToArray();
            } catch {
                return new string[0];
            }
        }
        internal static string[] PopulateDependentAssemblies(string manifestRelativeFileName) {
            StreamResourceInfo manifestStreamInfo = null;
            try {
                manifestStreamInfo = Application.GetRemoteStream(new Uri(manifestRelativeFileName, UriKind.Relative));
            } catch { }
            if(manifestStreamInfo == null || manifestStreamInfo.Stream == null)
                return new string[0];
            string manifestString = manifestStreamInfo.Stream.ToStringWithDispose();
            return GetDependentAssemblies(manifestString);
        }
        abstract class EnvironmentStrategy {
            const string ResourceString = ".resource";

            public abstract void AddAvailableAssemblies(ref List<string> assemblies);
            public virtual void Initialize() { }

            protected static void Add(IEnumerable<string> addingAssemblies, ref List<string> targetAssemblies) {
                foreach(string asmName in addingAssemblies) {
                    if(!targetAssemblies.Contains(asmName)) {
                        targetAssemblies.Add(asmName);
                    }
                }
            }
        }
        class BrowserStrategy : EnvironmentStrategy {
            public override void AddAvailableAssemblies(ref List<string> assemblies) {
                Add(ApplicationHelper.GetDependentAssemblies(), ref assemblies);
            }
        }
        class IgnoreManifestStrategy : BrowserStrategy {
            public override void AddAvailableAssemblies(ref List<string> assemblies) { }
        }
    }

    static class Extenstions {
        internal static Queue<T> AsQueue<T>(this IEnumerable<T> collecetion) {
            return new Queue<T>(collecetion);
        }
    }
    internal static class XmlHelper {
        public static List<T> GetElements<T>(Func<XElement, bool> predicate, Func<XElement, T> selectCondition, string str) {
            List<XElement> source = System.Xml.Linq.Extensions.Elements<XElement>(XDocument.Parse(str).Root.Elements()).ToList<XElement>();
            return source.Where<XElement>(predicate).Select<XElement, T>(selectCondition).ToList<T>();
        }
        public static List<T> GetDescendants<T>(Func<XElement, bool> predicate, Func<XElement, T> selectCondition, string str) {
            List<XElement> source = System.Xml.Linq.Extensions.Elements<XElement>(XDocument.Parse(str).Root.Descendants()).ToList<XElement>();
            return source.Where<XElement>(predicate).Select<XElement, T>(selectCondition).ToList<T>();
        }
    }
#endif
    public static class StreamHelper {
        public static string ToStringWithDispose(this Stream stream) {
            using(stream) {
                using(StreamReader reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }
        public static byte[] CopyAllBytes(this Stream stream) {
            if (!stream.CanRead)
                return null;
            if (stream.CanSeek)
                stream.Seek(0L, SeekOrigin.Begin);
            List<byte> list = new List<byte>();
            byte[] buffer = new byte[1024];
            int num;
            while ((num = stream.Read(buffer, 0, 1024)) > 0) {
                for (int index = 0; index < num; ++index)
                    list.Add(buffer[index]);
            }
            return list.ToArray();
        }
    }
}