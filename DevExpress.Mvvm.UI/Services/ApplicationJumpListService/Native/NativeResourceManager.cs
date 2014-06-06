using System;
using System.Collections.Generic;
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
        public const string CompanyNameEnvironmentVariable = "%COMPANYNAME%";
        public const string ProductNameEnvironmentVariable = "%PRODUCTNAME%";
        public const string ResourcesFolderName = "R-3B849FCF-333E-41D2-A306-5DE5A24DB817";
        static object resourcesFolderLock = new object();
        static string companyNameOverride;
        static string productNameOverride;
        static DateTime? applicationCreateTime = null;
        static string applicationExecutablePath;
        static string applicationId;
        static string applicationIdHash;
        static Dictionary<string, Func<string>> variables;
        static object variablesLock = new object();

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
        public static string ApplicationExecutablePath {
            get {
                if(applicationExecutablePath == null)
                    applicationExecutablePath = AssemblyHelper.EntryAssembly.Return(a => a.Location, () => Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[0]));
                return applicationExecutablePath;
            }
        }
        public static string ApplicationId {
            get {
                if(applicationId == null)
                    applicationId = "A" + Uri.EscapeDataString(ApplicationExecutablePath);
                return applicationId;
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
        public static string ResourcesFolder { get { return Path.Combine("%LOCALAPPDATA%", CompanyNameEnvironmentVariable, ProductNameEnvironmentVariable, ResourcesFolderName); } }
        public static Dictionary<string, Func<string>> Variables {
            get {
                if(variables == null) {
                    lock(variablesLock) {
                        if(variables == null) {
                            variables = new Dictionary<string, Func<string>>();
                            variables.Add(CompanyNameEnvironmentVariable, GetCompanyName);
                            variables.Add(ProductNameEnvironmentVariable, GetProductName);
                        }
                    }
                }
                return variables;
            }
        }
        public static string GetCompanyName() {
            lock(resourcesFolderLock) {
                string companyName = companyNameOverride;
                if(companyName == null)
                    companyName = Environment.GetEnvironmentVariable(CompanyNameEnvironmentVariable, EnvironmentVariableTarget.Process);
                if(companyName == null)
                    companyName = AssemblyHelper.EntryAssembly.With(a => a.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).Cast<AssemblyCompanyAttribute>().SingleOrDefault().With(t => t.Company));
                return companyName == null || !companyName.Where(c => char.IsLetterOrDigit(c)).Any() ? ApplicationIdHash : NativeResourceManager.CreateFileName(companyName);
            }
        }
        public static string GetProductName() {
            lock(resourcesFolderLock) {
                string productName = productNameOverride;
                if(productName == null)
                    productName = Environment.GetEnvironmentVariable(ProductNameEnvironmentVariable, EnvironmentVariableTarget.Process);
                if(productName == null)
                    productName = AssemblyHelper.EntryAssembly.With(a => a.GetCustomAttributes(typeof(AssemblyProductAttribute), false).Cast<AssemblyProductAttribute>().SingleOrDefault().With(t => t.Product));
                return productName == null || !productName.Where(c => char.IsLetterOrDigit(c)).Any() ? ApplicationIdHash : NativeResourceManager.CreateFileName(productName);
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
                try {
                    applicationCreateTime = GetFileTime(ApplicationExecutablePath);
                } catch {
                    applicationCreateTime = DateTime.Now;
                }
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