using System;
using System.Collections.Generic;

namespace DevExpress.Mvvm.UI.Native {
    public interface ICurrentProcess {
        string ExecutablePath { get; }
        string ApplicationId { get; }
        IEnumerable<string> CommandLineArgs { get; }
    }
    public class CurrentProcess : ICurrentProcess {
        public string ExecutablePath { get { return NativeResourceManager.ApplicationExecutablePath; } }
        public IEnumerable<string> CommandLineArgs { get { return Environment.GetCommandLineArgs(); } }
        public string ApplicationId { get { return NativeResourceManager.ApplicationIdHash; } }
    }
}