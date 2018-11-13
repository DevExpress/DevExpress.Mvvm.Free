using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace DevExpress.Data.Controls.WinrtToastNotifier.WinApi {
    static class WindowsVersion {
        public static int Major;
        public static int Minor;
        public static int Build;

        public static bool IsWin10AnniversaryOrNewer {
            get {
                return Major > 10 || (Major == 10 && Build >= 14393);
            }
        }

        static WindowsVersion() {
            var regex = new Regex(@"^(\d+)\.(\d+)\.(\d+)$");
            var match = regex.Match(GetOSVersionString());
            if(match.Success) {
                Major = int.Parse(match.Groups[1].Value);
                Minor = int.Parse(match.Groups[2].Value);
                Build = int.Parse(match.Groups[3].Value);
            } else {

                Major = Environment.OSVersion.Version.Major;
                Minor = Environment.OSVersion.Version.Minor;
                Build = Environment.OSVersion.Version.Build;
            }
        }

        [SecuritySafeCritical]
        static string GetOSVersionString() {
            string version = "";
            try {
                var searcher = new ManagementObjectSearcher("SELECT Version FROM Win32_OperatingSystem");
                foreach(var os in searcher.Get()) {
                    version = os["Version"].ToString();
                }
            } catch { }
            return version;
        }
    }
}