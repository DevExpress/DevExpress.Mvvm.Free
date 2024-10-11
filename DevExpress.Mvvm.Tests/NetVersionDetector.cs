using System;

#if NET
using System.Runtime.InteropServices;
#endif

namespace DevExpress.Xpf.Core.Utils {
    public static class NetVersionDetector {
        public static bool IsNet7() {
            return IsVersion(7);
        }

        static bool IsVersion(int major) {
#if NET
            return RuntimeInformation.FrameworkDescription.ToLower().StartsWith(".net") && Environment.Version.Major >= major;
#else
            var version = FrameworkInfoHelper.GetFrameworkInfo().Version;
            return version != null && version.Major >= major;
#endif
        }
    }
}
