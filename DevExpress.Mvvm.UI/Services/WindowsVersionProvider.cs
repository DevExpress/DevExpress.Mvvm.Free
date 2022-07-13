using System;
using System.Globalization;

namespace DevExpress.Utils {
    public static class WindowsVersionProvider {
        static readonly WindowsVersionInfo DefaultWindowsVersionInfo = new WindowsVersionInfo();
        static WindowsVersionInfo info;
        static WindowsVersionInfo WinVersionInfo {
            get {
                if(info == null && !WindowsVersionInfo.TryCreate(out info))
                    return DefaultWindowsVersionInfo;
                return info;
            }
        }
        public static bool IsWindows10 {
            get {
                if(string.IsNullOrEmpty(WinVersionInfo.ProductName)) 
                    return false;
                return WinVersionInfo.ProductName.IndexOf("windows 10", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }
        public static bool IsWindows11 {
            get {
                if(string.IsNullOrEmpty(WinVersionInfo.ProductName))
                    return false;
                return WinVersionInfo.ProductName.IndexOf("windows 10", StringComparison.OrdinalIgnoreCase) >= 0 && WinVersionInfo.CurrentBuild >= 22000;
            }
        }
        public static bool IsWin10AnniversaryUpdateOrHigher {
            get { return WinVersionInfo.CurrentMajorVersionNumber > 10 || (WinVersionInfo.CurrentMajorVersionNumber == 10 && WinVersionInfo.CurrentBuild >= 14393); }
        }
        public static bool IsWin10FallCreatorsUpdateOrHigher {
            get { return WinVersionInfo.ReleaseID >= 1709; }
        }
        public static bool IsWin10SpringCreatorsUpdateOrHigher {
            get { return WinVersionInfo.ReleaseID >= 1803; }
        }
        public static bool IsWinSupportsAcrylicEffect {
            get { return WinVersionInfo.ReleaseID >= 1803 && WinVersionInfo.CurrentBuild >= 17064; }
        }
        public static bool IsWindows10Build1903OrHigher {
            get { return WinVersionInfo.ReleaseID >= 1903; }
        }
        public static bool IsWindows10Build1809OrHigher {
            get { return WinVersionInfo.ReleaseID >= 1809; }
        }
        sealed class WindowsVersionInfo {
            const string RegistryVersionKey = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
            public int CurrentBuild { get; set; }
            public int CurrentMajorVersionNumber { get; set; }
            public int CurrentMinorVersionNumber { get; set; }
            public Version CurrentVersion { get; set; }
            public string ProductName { get; set; }
            public int ReleaseID { get; set; }
            public static bool TryCreate(out WindowsVersionInfo vi) {
                vi = null;
                try {
                    using(var versionKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(RegistryVersionKey, false)) {
                        if(versionKey != null) {
                            WindowsVersionInfo versionInfo = new WindowsVersionInfo();
                            versionInfo.CurrentBuild = GetRegistryKeyValue<int>(versionKey, "CurrentBuild", ConvertToInt32FromString, int.MinValue);
                            versionInfo.CurrentMajorVersionNumber = GetRegistryKeyValue<int>(versionKey, "CurrentMajorVersionNumber", ConvertToInt32FromHex, int.MinValue);
                            versionInfo.CurrentMinorVersionNumber = GetRegistryKeyValue<int>(versionKey, "CurrentMinorVersionNumber", ConvertToInt32FromHex, int.MinValue);
                            versionInfo.CurrentVersion = GetRegistryKeyValue<Version>(versionKey, "CurrentVersion", ConvertToVersion, null);
                            versionInfo.ProductName = GetRegistryKeyValue<string>(versionKey, "ProductName", ConvertToString, string.Empty);
                            versionInfo.ReleaseID = GetRegistryKeyValue<int>(versionKey, "ReleaseId", ConvertToInt32FromString, int.MinValue);
                            vi = versionInfo;
                            return true;
                        }
                    }
                }
                catch { }
                return false;
            }
            static string ConvertToString(object value) {
                return (value != null) ? value.ToString() : string.Empty;
            }
            static int ConvertToInt32FromString(object value) {
                return (value != null) ? int.Parse(value.ToString()) : int.MinValue;
            }
            static int ConvertToInt32FromHex(object value) {
                return (value != null) ? int.Parse(value.ToString(), NumberStyles.HexNumber) : int.MinValue;
            }
            static Version ConvertToVersion(object value) {
                Version ver = new Version();
                if(value != null && Version.TryParse(value.ToString(), out ver))
                    return ver;
                return ver;
            }
            static T GetRegistryKeyValue<T>(Microsoft.Win32.RegistryKey key, string subKeyName, Func<object, T> convert, T defaultVal) {
                if(key != null) {
                    object keyValue = key.GetValue(subKeyName);
                    if(keyValue != null)
                        return convert(keyValue);
                }
                return defaultVal;
            }
        }
    }
}
