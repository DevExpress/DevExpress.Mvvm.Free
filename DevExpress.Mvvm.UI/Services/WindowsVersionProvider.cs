using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DevExpress.Utils {
    class WindowsVersionInfo {
        public int CurrentBuild { get; set; }
        public int CurrentMajorVersionNumber { get; set; }
        public int CurrentMinorVersionNumber { get; set; }
        public Version CurrentVersion { get; set; }
        public string ProductName { get; set; }
        public int ReleaseID { get; set; }
    }
    public static class WindowsVersionProvider {
        static string RegistryVersionKey = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
        static WindowsVersionInfo info;
        static WindowsVersionInfo WinVersionInfo {
            get {
                if(info == null) {
                    if(!TryGetReleaseAndBuildVersion(out info)) return new WindowsVersionInfo();
                }

                return info;
            }
        }
        public static bool IsWindows10 {
            get { return WinVersionInfo.ProductName.ToLower().Contains("windows 10"); }
        }
        public static bool IsWin10AnniversaryUpdateOrHigher {
            get {
                return WinVersionInfo.CurrentMajorVersionNumber > 10 || (WinVersionInfo.CurrentMajorVersionNumber == 10 && WinVersionInfo.CurrentBuild >= 14393);
            }
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
        static bool TryGetReleaseAndBuildVersion(out WindowsVersionInfo vi) {
            bool success = false;
            vi = null;
            try {
                using(Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(RegistryVersionKey, false)) {
                    if(rk != null) {
                        WindowsVersionInfo versionInfo = new WindowsVersionInfo();
                        versionInfo.CurrentBuild = GetRegistryKeyValue<int>(rk, "CurrentBuild", ConvertToInt32FromString, int.MinValue);
                        versionInfo.CurrentMajorVersionNumber = GetRegistryKeyValue<int>(rk, "CurrentMajorVersionNumber", ConvertToInt32FromHex, int.MinValue);
                        versionInfo.CurrentMinorVersionNumber = GetRegistryKeyValue<int>(rk, "CurrentMinorVersionNumber", ConvertToInt32FromHex, int.MinValue);
                        versionInfo.CurrentVersion = GetRegistryKeyValue<Version>(rk, "CurrentVersion", ConvertToVersion, null);
                        versionInfo.ProductName = GetRegistryKeyValue<string>(rk, "ProductName", ConvertToString, string.Empty);
                        versionInfo.ReleaseID = GetRegistryKeyValue<int>(rk, "ReleaseId", ConvertToInt32FromString, int.MinValue);
                        vi = versionInfo;
                        success = true;
                    }
                }
            }
            catch {
                success = false;
            }
            return success;
        }
        static string ConvertToString(object value) {
            return value != null ? value.ToString() : string.Empty;
        }
        static int ConvertToInt32FromString(object value) {
            return value != null ? int.Parse(value.ToString()) : int.MinValue;
        }
        static int ConvertToInt32FromHex(object value) {
            if(value != null) return int.Parse(value.ToString(), NumberStyles.HexNumber);
            return int.MinValue;
        }
        static Version ConvertToVersion(object value) {
            Version ver = new Version();
            if(value != null) {
                if(Version.TryParse(value.ToString(), out ver)) {
                    return ver;
                }
            }
            return ver;
        }
        static T GetRegistryKeyValue<T>(Microsoft.Win32.RegistryKey key, string subKeyName, Func<object, T> convert, T defaultVal) {
            if(key != null) {
                object keyValue = key.GetValue(subKeyName);
                if(keyValue != null) return convert(keyValue);
            }
            return defaultVal;
        }
    }
}