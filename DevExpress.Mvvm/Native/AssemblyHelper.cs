using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Security;
using System.Windows;
using System.Text;
using System.Linq;

using System.IO.Compression;

using DevExpress.Internal;

namespace DevExpress.Utils {
    public static class AssemblyHelper {
        static Dictionary<Assembly, string> defaultNamespaces = new Dictionary<Assembly, string>();
        const string PublicKeyTokenString = "PublicKeyToken";
        static Assembly entryAssembly;
        public static Assembly EntryAssembly {
            get {
                if(entryAssembly == null)
                    entryAssembly = Assembly.GetEntryAssembly();
                return entryAssembly;
            }
            set { entryAssembly = value; }
        }
        static Assembly GetReflectionOnlyLoadedAssembly(string asmName) {
            try {
                return Assembly.ReflectionOnlyLoad(asmName);
            } catch {
                return null;
            }
        }
        const int PublicKeyTokenBytesLength = 8;

        static byte[] StringToBytes(string str) {
            int bytesLength = (int)(str.Length / 2);

            byte[] bytes = new byte[bytesLength];
            for(int i = 0; i < bytesLength; i++) {
                bytes[i] = byte.Parse(str.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return bytes;
        }
        public static string GetAssemblyFullName(string name, string version, System.Globalization.CultureInfo culture, string publicKeyToken) {
            AssemblyName asmName = new AssemblyName();
            asmName.Name = name;
            asmName.Version = new Version(version);

            asmName.CultureInfo = culture;
            if(publicKeyToken != null && publicKeyToken.Length == 2 * PublicKeyTokenBytesLength) {
                asmName.SetPublicKeyToken(StringToBytes(publicKeyToken));
            }
            return asmName.FullName;
        }
        public static Assembly GetThemeAssembly(string themeName) {
            string assemblyName = GetThemeAssemblyName(themeName);
            Assembly assembly = AssemblyHelper.GetLoadedAssembly(assemblyName);
            if(assembly != null)
                return assembly;
            return LoadDXAssembly(assemblyName);
        }
        public static string GetShortNameWithoutVersion(Assembly assembly) {
            return GetShortNameWithoutVersion(assembly.FullName);
        }
        public static string GetShortNameWithoutVersion(string fullName) {
            string name = fullName.Split(',')[0];
            string nameWithoutVSuffix = name.Replace(AssemblyInfo.VSuffix, string.Empty);
            return nameWithoutVSuffix;
        }

        public static bool HasAttribute(string assemblyName, Type attributeType) {
            return HasAttribute(GetLoadedAssembly(assemblyName), attributeType);
        }
        public static bool HasAttribute(Assembly assembly, Type attributeType) {
            if(assembly != null) {
                return Attribute.IsDefined(assembly, attributeType);
            }
            return false;
        }
        public static bool IsLoadedAssembly(string assemblyName) {
            return GetLoadedAssembly(assemblyName) != null;
        }
        public static Assembly GetLoadedAssembly(string asmName) {
            IEnumerable assemblies = GetLoadedAssemblies();
            foreach(Assembly asm in assemblies) {
                if(PartialNameEquals(asm.FullName, asmName))
                    return asm;
            }
            return null;
        }
        public static IEnumerable<Assembly> GetLoadedAssemblies() {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
        public static string GetThemeAssemblyFullName(string themeName) {
            return GetThemeAssemblyName(themeName) + GetFullNameAppendix();
        }
        public static string GetThemeAssemblyName(string themeName) {
            return AssemblyInfo.SRAssemblyXpfPrefix + AssemblyInfo.ThemePrefix + themeName.Split(';').First() + AssemblyInfo.VSuffix;
        }
        public static Assembly LoadDXAssembly(string assemblyName) {
            Assembly assembly = null;
            try {
                assembly = Assembly.Load(assemblyName + GetFullNameAppendix());
            } catch {
            }
            return assembly;
        }
        public static string GetFullNameAppendix() {
            return ", Version=" + AssemblyInfo.Version + ", Culture=neutral, " + PublicKeyTokenString + "=" + GetCoreAssemblyPublicKeyToken();
        }
        public static string GetCoreAssemblyPublicKeyToken() {
            string coreAssemblyName = GetDXAssemblyFullName();
            int publicKeyTokenIndex = coreAssemblyName.IndexOf(PublicKeyTokenString, StringComparison.Ordinal);
            int publicKeyTokenLenght = "31bf3856ad364e35".Length;
            return typeof(AssemblyHelper).Assembly.FullName.Substring(publicKeyTokenIndex + (PublicKeyTokenString + "=").Length, publicKeyTokenLenght);
        }
        static string GetDXAssemblyFullName() {
            Assembly assemblyHelperAssembly = typeof(AssemblyHelper).Assembly;
            string assemblyFullName = assemblyHelperAssembly.FullName;
            if(NameContains(Assembly.GetExecutingAssembly().FullName, AssemblyInfo.SRAssemblyData)) return assemblyFullName;
            if(NameContains(Assembly.GetExecutingAssembly().FullName, AssemblyInfo.SRAssemblyXpfMvvm)) return assemblyFullName;
            throw new NotSupportedException(string.Format("Wrong DX assembly: {0}", assemblyFullName));
        }
        public static Assembly GetAssembly(string assemblyFullName, Func<string, Assembly> loadAssemblyHandler = null) {
            Assembly assembly = GetLoadedAssembly(assemblyFullName);
            if(assembly != null) return assembly;
            return loadAssemblyHandler?.Invoke(assemblyFullName) ?? Assembly.Load(assemblyFullName);
        }
        public static bool NameContains(string assemblyFullName, string assemblyName) {
            return AssertAssemblyName(assemblyFullName, assemblyName);
        }
        public static bool NameContains(Assembly assembly, string assemblyName) {
            return AssertAssemblyName(assembly.FullName, assemblyName);
        }
        public static bool NameContains(AssemblyName assembly, string assemblyName) {
            return AssertAssemblyName(assembly.FullName, assemblyName);
        }
        public static bool IsDXProductAssembly(Assembly assembly) {
            return NameContains(assembly, AssemblyInfo.SRAssemblyXpfPrefix) && !IsDXThemeAssembly(assembly);
        }
        public static bool IsDXProductAssembly(string assemblyFullName) {
            return NameContains(assemblyFullName, AssemblyInfo.SRAssemblyXpfPrefix) && !IsDXThemeAssembly(assemblyFullName);
        }
        public static bool IsDXThemeAssembly(Assembly assembly) {
            return NameContains(assembly, AssemblyInfo.ThemePrefixWithoutSeparator);
        }
        public static bool IsDXThemeAssembly(string assemblyName) {
            return NameContains(assemblyName, AssemblyInfo.ThemePrefixWithoutSeparator);
        }
        public static bool IsEntryAssembly(Assembly assembly) {
            Assembly entryAssembly = EntryAssembly;
            return entryAssembly == assembly;
        }
        public static bool IsEntryAssembly(string assemblyName) {
            Assembly entryAssembly = EntryAssembly;
            if(entryAssembly == null)
                return false;
            return NameContains(entryAssembly, assemblyName);
        }
        static bool AssertAssemblyName(string fullName, string assemblyName) {
            if(string.IsNullOrEmpty(assemblyName))
                return false;
            return fullName.ToLowerInvariant().Contains(assemblyName.ToLowerInvariant());
        }
        public static string CombineUri(params string[] parts) {
            if(parts.Length == 0) return string.Empty;
            string ret = parts[0];
            for(int i = 1; i < parts.Length; ++i) {
                ret += "/" + parts[i];
            }
            ret = ret.Replace('\\', '/');
            while(ret.Contains("//"))
                ret = ret.Replace("//", "/");
            if(ret.Length > 1 && ret[ret.Length - 1] == '/') ret = ret.Remove(ret.Length - 1);
            if((parts[0].Length == 0 || parts[0][0] != '/') && ret.Length != 0 && ret[0] == '/') ret = ret.Substring(1);
            return ret;
        }
        public static bool PartialNameEquals(string asmName0, string asmName1) {
            return string.Equals(GetPartialName(asmName0), GetPartialName(asmName1), StringComparison.InvariantCultureIgnoreCase);
        }
        public static string GetPartialName(string asmName) {
            int nameEnd = asmName.IndexOf(',');
            return nameEnd < 0 ? asmName : asmName.Remove(nameEnd);
        }
        public static string GetPartialName(Assembly assembly) {
            return GetPartialName(assembly.FullName);
        }
        public static ResourceSet GetResources(Assembly assembly) {
            string baseName = GetPartialName(assembly) + ".g";
            ResourceManager resourceManager = new ResourceManager(baseName, assembly);
            return resourceManager.GetResourceSet(System.Globalization.CultureInfo.InvariantCulture, true, false);
        }
        public static IDictionaryEnumerator GetResourcesEnumerator(Assembly assembly) {
            ResourceSet rs = GetResources(assembly);
            return rs == null ? null : rs.GetEnumerator();
        }
        public static Uri GetResourceUri(Assembly assembly, string path) {
            return new Uri(string.Format("{0}/{1};component/{2}", "pack://application:,,,", AssemblyHelper.GetPartialName(assembly), path));
        }
        public static Stream GetResourceStream(Assembly assembly, string path, bool pathIsFull) {
            path = path.ToLowerInvariant();
            Stream stream = GetResourceStreamCore(assembly, path, pathIsFull);
            if(stream == null) {
                stream = GetResourceStreamCore(assembly, path + ".zip", pathIsFull);
                if(stream != null)
                    stream = new GZipStream(stream, CompressionMode.Decompress);
            }
            return stream;
        }
        public static Stream GetEmbeddedResourceStream(Assembly assembly, string name, bool nameIsFull) {
            name = name.Replace('/', '.');
            Stream stream = GetEmbeddedResourceStreamCore(assembly, name, nameIsFull);
            if(stream == null) {
                stream = GetEmbeddedResourceStreamCore(assembly, name + ".zip", nameIsFull);
                if(stream != null)
                    stream = new GZipStream(stream, CompressionMode.Decompress);
            }
            return stream;
        }
        public static string GetNamespace(Type type) {
            string typeName = type.FullName;
            int d = typeName.LastIndexOf('.');
            return d < 0 ? string.Empty : typeName.Remove(d);
        }
        public static string GetDefaultNamespace(Assembly assembly) {
            string defaultNamespace = null;
            if(!defaultNamespaces.TryGetValue(assembly, out defaultNamespace)) {
                defaultNamespace = GetDefaultNamespaceCore(assembly);
                defaultNamespaces.Add(assembly, defaultNamespace);
            }
            return defaultNamespace;
        }
        public static string GetCommonPart(string[] strings, string[] excludedSuffixes) {
            List<string> filteredStrings = strings.Where(s => excludedSuffixes.Where(e => s.EndsWith(e, StringComparison.Ordinal)).FirstOrDefault() == null).ToList();
            if(filteredStrings.Count == 0) return string.Empty;
            StringBuilder commonPart = new StringBuilder(filteredStrings[0].Length);
            for(int i = 0; ; ++i) {
                char? c = null;
                foreach(string s in filteredStrings) {
                    if(i >= s.Length) return commonPart.ToString();
                    if(c == null)
                        c = s[i];
                    if(s[i] != c) return commonPart.ToString();
                }
                commonPart.Append(c);
            }
        }
        static Stream GetResourceStreamCore(Assembly assembly, string path, bool pathIsFull) {
            if(pathIsFull) {
                ResourceSet resources = GetResources(assembly);
                if(resources == null) return null;
                return resources.GetObject(path, false) as Stream;
            } else {
                IDictionaryEnumerator enumerator = GetResourcesEnumerator(assembly);
                if(enumerator == null) return null;
                while(enumerator.MoveNext()) {
                    string resourceName = (string)enumerator.Key;
                    if(resourceName == path || resourceName.EndsWith("/" + path, StringComparison.Ordinal))
                        return enumerator.Value as Stream;
                }
                return null;
            }
        }
        static Stream GetEmbeddedResourceStreamCore(Assembly assembly, string name, bool nameIsFull) {
            string nameSpace = GetDefaultNamespace(assembly);
            string fullName = nameSpace + name;
            Stream stream = assembly.GetManifestResourceStream(fullName);
            if(stream != null || nameIsFull) return stream;
            foreach(string resourceName in assembly.GetManifestResourceNames()) {
                if(resourceName.EndsWith("." + name, StringComparison.Ordinal))
                    return assembly.GetManifestResourceStream(resourceName);
            }
            return null;
        }
        static string GetDefaultNamespaceCore(Assembly assembly) {
            string[] names = assembly.GetManifestResourceNames();
            if(names.Length == 0) return string.Empty;
            if(names.Length == 1) return GetPartialName(assembly) + ".";
            string[] excludedSuffixes = new string[] { ".csdl", ".ssdl", ".msl" };
            return GetCommonPart(names, excludedSuffixes);
        }
    }
}