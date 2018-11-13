using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DevExpress.Mvvm.Native;
using DevExpress.Utils;

namespace DevExpress.Mvvm.UI.Native {
    public static class NativeResourceManager {
        public const string AppDataEnvironmentVariable = "%_DATA_%";
        public const string CompanyNameEnvironmentVariable = "%_COMPANY_%";
        public const string ProductNameEnvironmentVariable = "%_PRODUCT_%";
        public const string VersionEnvironmentVariable = "%_VERSION_%";
        public const string ResourcesFolderName = "RDB817";
        static object resourcesFolderLock = new object();
        static string companyNameOverride;
        static string productNameOverride;
        static string versionOverride;
        static DateTime? applicationCreateTime = null;
        static string applicationExecutablePath;
        static string applicationIdHash;
        static Dictionary<string, Func<string>> variables;
        static object variablesLock = new object();
        static Tuple<string, string, string, string> configurationPathParts;
        static Assembly entryAssembly;

        public static string CompanyNameOverride {
            get {
                lock(resourcesFolderLock) {
                    return companyNameOverride;
                }
            }
            set {
                lock(resourcesFolderLock) {
                    companyNameOverride = value;
                }
            }
        }
        public static string ProductNameOverride {
            get {
                lock(resourcesFolderLock) {
                    return productNameOverride;
                }
            }
            set {
                lock(resourcesFolderLock) {
                    productNameOverride = value;
                }
            }
        }
        public static string VersionOverride {
            get {
                lock(resourcesFolderLock) {
                    return versionOverride;
                }
            }
            set {
                lock(resourcesFolderLock) {
                    versionOverride = value;
                }
            }
        }
        public static string ApplicationExecutablePath {
            get {
                if(applicationExecutablePath == null)
                    applicationExecutablePath = EntryAssembly.Return(a => a.Location, () => Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[0]));
                return applicationExecutablePath;
            }
        }
        public static string ApplicationIdHash {
            get {
                if(applicationIdHash == null) {
                    using(SHA1 sha1 = SHA1.Create()) {
                        applicationIdHash = "H" + NativeResourceManager.CreateFileName(Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(ApplicationExecutablePath))));
                    }
                }
                return applicationIdHash;
            }
        }
        public static string ResourcesFolder { get { return Path.Combine(AppDataEnvironmentVariable, CompanyNameEnvironmentVariable, ProductNameEnvironmentVariable, VersionEnvironmentVariable, ResourcesFolderName); } }
        public static Dictionary<string, Func<string>> Variables {
            get {
                if(variables == null) {
                    lock(variablesLock) {
                        if(variables == null) {
                            variables = new Dictionary<string, Func<string>>();
                            variables.Add(AppDataEnvironmentVariable, () => ConfigurationPathParts.Item1);
                            variables.Add(CompanyNameEnvironmentVariable, () => CompanyNameOverride ?? ConfigurationPathParts.Item2);
                            variables.Add(ProductNameEnvironmentVariable, () => ProductNameOverride ?? ConfigurationPathParts.Item3);
                            variables.Add(VersionEnvironmentVariable, () => VersionOverride ?? ConfigurationPathParts.Item4);
                        }
                    }
                }
                return variables;
            }
        }
        static Tuple<string, string, string, string> ConfigurationPathParts {
            get {
                lock(resourcesFolderLock) {
                    if(configurationPathParts == null) {
                        string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\";
                        try {
                            object configurationManager = typeof(ConfigurationException).Assembly.GetType("System.Configuration.ConfigurationManagerInternalFactory").GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
                            var directory = (string)configurationManager.GetType().GetInterface("System.Configuration.Internal.IConfigurationManagerInternal").GetProperty("ExeLocalConfigDirectory").GetValue(configurationManager, null);
                            if(!directory.StartsWith(appDataDirectory, StringComparison.InvariantCultureIgnoreCase))
                                throw new IndexOutOfRangeException();
                            directory = directory.Substring(appDataDirectory.Length);
                            string[] pathParts = directory.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                            if(pathParts.Length < 3)
                                throw new IndexOutOfRangeException();
                            if(pathParts.Length == 3)
                                configurationPathParts = new Tuple<string, string, string, string>(appDataDirectory, pathParts[0], pathParts[1], pathParts[2]);
                            else
                                configurationPathParts = new Tuple<string, string, string, string>(Path.Combine(appDataDirectory, Path.Combine(pathParts.Take(pathParts.Length - 1).ToArray())), string.Empty, string.Empty, pathParts[pathParts.Length - 1]);
                        } catch {
                            string companyName = EntryAssembly.With(a => a.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).Cast<AssemblyCompanyAttribute>().SingleOrDefault().With(t => t.Company));
                            string productName = EntryAssembly.With(a => a.GetCustomAttributes(typeof(AssemblyProductAttribute), false).Cast<AssemblyProductAttribute>().SingleOrDefault().With(t => t.Product));
                            string version = EntryAssembly.With(a => a.GetName().Version.ToString(4));
                            companyName = companyName == null || !companyName.Where(c => char.IsLetterOrDigit(c)).Any() ? ApplicationIdHash : NativeResourceManager.CreateFileName(companyName);
                            productName = productName == null || !productName.Where(c => char.IsLetterOrDigit(c)).Any() ? ApplicationIdHash : NativeResourceManager.CreateFileName(productName);
                            version = version == null || !version.Where(c => char.IsLetterOrDigit(c)).Any() ? ApplicationIdHash : NativeResourceManager.CreateFileName(version);
                            configurationPathParts = new Tuple<string, string, string, string>(appDataDirectory, companyName, productName, version);
                        }
                    }
                    return configurationPathParts;
                }
            }
        }
        public static Assembly EntryAssembly {
            get {
                if(entryAssembly == null)
                    entryAssembly = Assembly.GetEntryAssembly();
                return entryAssembly;
            }
        }
        public static string ExpandVariables(string name) {
            return Environment.ExpandEnvironmentVariables(ExpandVariablesCore(name, Variables));
        }
        public static string ExpandVariablesCore(string name, Dictionary<string, Func<string>> variables) {
            if(name == null) throw new ArgumentNullException("name");
            StringBuilder result = new StringBuilder(name.Length);
            for(int index = 0; index < name.Length; ) {
                int variableStartIndex = name.IndexOf('%', index);
                if(variableStartIndex < 0) {
                    result.Append(name, index, name.Length - index);
                    break;
                }
                result.Append(name, index, variableStartIndex - index);
                index = variableStartIndex + 1;
                int variableEndIndex = index == name.Length ? -1 : name.IndexOf('%', index);
                if(variableEndIndex < 0) {
                    result.Append(name, variableStartIndex, name.Length - variableStartIndex);
                    break;
                }
                index = variableEndIndex + 1;
                string variableName = name.Substring(variableStartIndex, variableEndIndex + 1 - variableStartIndex).ToUpperInvariant();
                Func<string> variableValue;
                if(variables.TryGetValue(variableName, out variableValue))
                    result.Append(variableValue());
                else
                    result.Append(variableName);
            }
            return result.ToString();
        }
        public static DateTime GetApplicationCreateTime() {
            if(applicationCreateTime == null) {
                applicationCreateTime = DateTime.Now;
            }
            return applicationCreateTime.Value;
        }
        public static DateTime GetFileTime(string filePath) {
            DateTime creationTime = File.GetCreationTimeUtc(filePath);
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            return creationTime > lastWriteTime ? creationTime : lastWriteTime;
        }
        public static string CreateFileName(string source) {
            string invalidChars = string.Format("%{0}{1}", new string(Path.GetInvalidPathChars()), new string(Path.GetInvalidFileNameChars()));
            return new Regex(string.Format("[{0}]", Regex.Escape(invalidChars))).Replace(source, "_");
        }
    }
}