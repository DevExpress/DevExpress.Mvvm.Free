using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;

namespace DevExpress.Mvvm.UI.Native {
    public abstract class JumpActionsManagerBase : IDisposable {
        public const int DefaultMillisecondsTimeout = 500;
        [StructLayout(LayoutKind.Sequential)]
        protected struct GuidData {
            public GuidData(Guid guid) : this(guid.ToByteArray()) { }
            public GuidData(byte[] bytes) {
                Byte0 = bytes[0]; Byte1 = bytes[1]; Byte2 = bytes[2]; Byte3 = bytes[3];
                Byte4 = bytes[4]; Byte5 = bytes[5]; Byte6 = bytes[6]; Byte7 = bytes[7];
                Byte8 = bytes[8]; Byte9 = bytes[9]; ByteA = bytes[10]; ByteB = bytes[11];
                ByteC = bytes[12]; ByteD = bytes[13]; ByteE = bytes[14]; ByteF = bytes[15];
            }
            public byte Byte0; public byte Byte1; public byte Byte2; public byte Byte3;
            public byte Byte4; public byte Byte5; public byte Byte6; public byte Byte7;
            public byte Byte8; public byte Byte9; public byte ByteA; public byte ByteB;
            public byte ByteC; public byte ByteD; public byte ByteE; public byte ByteF;

            public byte[] AsBytesArray {
                get {
                    return new byte[16] {
                        Byte0, Byte1, Byte2, Byte3,
                        Byte4, Byte5, Byte6, Byte7,
                        Byte8, Byte9, ByteA, ByteB,
                        ByteC, ByteD, ByteE, ByteF
                    };
                }
            }
            public Guid AsGuid { get { return new Guid(AsBytesArray); } }
        }
        [StructLayout(LayoutKind.Sequential)]
        protected struct InstancesProperties {
            public GuidData NameSuffix;
            public int InstancesCount;
        }
        [ServiceContract]
        protected interface IApplicationInstance {
            [OperationContract]
            void Execute(string command);
        }
        protected class NamedFile {
            public NamedFile(Tuple<IntPtr, IntPtr> file, GuidData nameSuffix) {
                File = file;
                NameSuffix = nameSuffix;
            }
            public Tuple<IntPtr, IntPtr> File { get; private set; }
            public GuidData NameSuffix { get; private set; }
        }

        protected const string MainMutexName = "C4339FDC-8943-4AA6-8DB9-644D68462BC7_";
        protected const string InstancesPropertiesFileName = "7BA4B4E1-BBB3-484F-89A5-75089CAA2B65_";
        protected const string InstanceNamePrefix = "5BCE503C-DB8D-440A-BEEC-9C963A364DBF_";
        protected const string InstancesFileNamePrefix = "E1E74EA7-B062-425F-AFFB-6517A3B5B075_";
        protected const string EndPointName = "Pipe";
        Mutex mainMutex;
        Tuple<IntPtr, IntPtr> instancesPropertiesFile = null;
        NamedFile instancesFile = new NamedFile(null, new GuidData(Guid.Empty));
        volatile bool disposed = false;

        public JumpActionsManagerBase(int millisecondsTimeout) {
            MillisecondsTimeout = millisecondsTimeout;
        }
        ~JumpActionsManagerBase() {
            Dispose(false);
        }
        public void Dispose() {
            if(disposed) return;
            disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            Mutex mainMutex = GetMainMutex(!disposing);
            WaitOne(mainMutex);
            try {
                DisposeInstancesFile();
                DisposeInstancesPropertiesFile();
            } finally {
                mainMutex.ReleaseMutex();
            }
        }
        [SecuritySafeCritical]
        void DisposeInstancesPropertiesFile() {
            if(instancesPropertiesFile == null) return;
            UnmapViewAndCloseFileMapping(instancesPropertiesFile);
            instancesPropertiesFile = null;
        }
        [SecuritySafeCritical]
        void DisposeInstancesFile() {
            if(instancesFile.File == null) return;
            UnmapViewAndCloseFileMapping(instancesFile.File);
            instancesFile = new NamedFile(null, new GuidData(Guid.Empty));
        }
        protected int MillisecondsTimeout { get; private set; }
        [SecuritySafeCritical]
        protected Mutex GetMainMutex(bool safe) {
            if(safe) return GetMainMutexCore(true);
            if(mainMutex == null)
                mainMutex = GetMainMutexCore(false);
            return mainMutex;
        }
        [SecuritySafeCritical]
        protected Mutex GetMainMutexCore(bool safe) {
            return new Mutex(false, MainMutexName + ApplicationId);
        }
        [SecuritySafeCritical]
        Tuple<IntPtr, IntPtr> GetInstancesPropertiesFile() {
            if(instancesPropertiesFile == null) {
                bool alreadyExists;
                instancesPropertiesFile = CreateFileMappingAndMapView(Marshal.SizeOf(typeof(InstancesProperties)), InstancesPropertiesFileName + ApplicationId, out alreadyExists);
                if(!alreadyExists) {
                    try {
                        UpdateInstancesFile(new GuidData[] { }, new GuidData(Guid.Empty));
                    } catch {
                        DisposeInstancesPropertiesFile();
                        throw;
                    }
                }
            }
            return instancesPropertiesFile;
        }
        protected string GetServiceUri(GuidData applicationInstance) {
            return string.Format("net.pipe://localhost/{0}{1}", InstanceNamePrefix, applicationInstance.AsGuid);
        }
        [SecuritySafeCritical]
        protected GuidData[] GetApplicationInstances(bool isCurrentProcessApplicationInstance) {
            Tuple<NamedFile, int> instancesFile = GetInstancesFile(!isCurrentProcessApplicationInstance);
            if(instancesFile.Item2 == 0) return new GuidData[] { };
            GuidData[] instances = new GuidData[instancesFile.Item2];
            for(int i = 0; i < instances.Length; ++i)
                instances[i] = (GuidData)Marshal.PtrToStructure(instancesFile.Item1.File.Item2 + i * Marshal.SizeOf(typeof(GuidData)), typeof(GuidData));
            return instances;
        }
        [SecuritySafeCritical]
        protected Tuple<NamedFile, int> GetInstancesFile(bool isFileCreatedByAnotherProcess) {
            Tuple<IntPtr, IntPtr> instancesPropertiesFile = GetInstancesPropertiesFile();
            InstancesProperties listProperties = (InstancesProperties)Marshal.PtrToStructure(instancesPropertiesFile.Item2, typeof(InstancesProperties));
            if(listProperties.NameSuffix.AsGuid != this.instancesFile.NameSuffix.AsGuid) {
                DisposeInstancesFile();
                if(listProperties.NameSuffix.AsGuid != Guid.Empty) {
                    bool alreadyExists;
                    instancesFile = new NamedFile(CreateFileMappingAndMapView(isFileCreatedByAnotherProcess ? 1 : 0, GetInstancesFileName(listProperties.NameSuffix), out alreadyExists), listProperties.NameSuffix);
                }
            }
            return new Tuple<NamedFile, int>(instancesFile, listProperties.InstancesCount);
        }
        [SecuritySafeCritical]
        protected void UpdateInstancesFile(GuidData[] instances) {
            DisposeInstancesFile();
            if(instances.Length != 0) {
                GuidData instancesFileNameSuffix = new GuidData(Guid.NewGuid());
                bool alreadyExists;
                instancesFile = new NamedFile(CreateFileMappingAndMapView(instances.Length * Marshal.SizeOf(typeof(GuidData)), GetInstancesFileName(instancesFileNameSuffix), out alreadyExists), instancesFileNameSuffix);
                for(int i = 0; i < instances.Length; ++i)
                    Marshal.StructureToPtr(instances[i], instancesFile.File.Item2 + i * Marshal.SizeOf(typeof(GuidData)), false);
            }
            UpdateInstancesFile(instances, instancesFile.NameSuffix);
        }
        [SecuritySafeCritical]
        protected void UpdateInstancesFile(GuidData[] instances, GuidData instancesFileNameSuffix) {
            InstancesProperties listProperties = new InstancesProperties() { InstancesCount = instances.Length, NameSuffix = instancesFileNameSuffix };
            Marshal.StructureToPtr(listProperties, GetInstancesPropertiesFile().Item2, false);
        }
        static string GetInstancesFileName(GuidData instancesFileNameSuffix) {
            return InstancesFileNamePrefix + instancesFileNameSuffix.AsGuid.ToString();
        }
        protected void WaitOne(WaitHandle waitHandle) {
            if(!waitHandle.WaitOne(MillisecondsTimeout))
                throw new TimeoutException();
        }
        protected abstract string ApplicationId { get; }

        [SecurityCritical]
        Tuple<IntPtr, IntPtr> CreateFileMappingAndMapView(int dwMaximumSizeLow, string lpName, out bool alreadyExists) {
            if(dwMaximumSizeLow == 0) {
                dwMaximumSizeLow = 1;
            }
            IntPtr fileObject = Import.CreateFileMapping(Import.InvalidHandleValue, IntPtr.Zero, Import.PageReadwrite, 0, (uint)dwMaximumSizeLow, lpName);
            if(fileObject == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            alreadyExists = Marshal.GetLastWin32Error() == Import.ERROR_ALREADY_EXISTS;
            IntPtr fileView = Import.MapViewOfFile(fileObject, Import.FileMapAllAccess, 0, 0, UIntPtr.Zero);
            if(fileView == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return new Tuple<IntPtr, IntPtr>(fileObject, fileView);
        }
        [SecurityCritical]
        void UnmapViewAndCloseFileMapping(Tuple<IntPtr, IntPtr> file) {
            Import.UnmapViewOfFile(file.Item2);
            Import.CloseHandle(file.Item1);
        }
        static class Import {
            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern IntPtr CreateFileMapping(
                IntPtr lpBaseAddress,
                IntPtr lpFileMappingAttributes,
                uint flProtect,
                uint dwMaximumSizeHigh,
                uint dwMaximumSizeLow,
                string lpName);

            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern IntPtr MapViewOfFile(
                IntPtr hFileMappingObject,
                uint dwDesiredAccess,
                uint dwFileOffsetHigh,
                uint dwFileOffsetLow,
                UIntPtr dwNumberOfBytesToMap);

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hObject);

            public const uint FileMapAllAccess = 0xF001F;
            public const uint PageReadwrite = 0x04;
            public static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
            public const int ERROR_ALREADY_EXISTS = 183;
        }
    }
    public class JumpActionsManagerClient : JumpActionsManagerBase {
        string applicationId;

        public JumpActionsManagerClient(int millisecondsTimeout = DefaultMillisecondsTimeout) : base(millisecondsTimeout) { }
        public void Run(string[] args, Action<ProcessStartInfo> startProcess) {
            if(args.Length != 4 && args.Length != 5) throw new ArgumentException("", "args");
            applicationId = args[0];
            Mutex mainMutex = GetMainMutex(false);
            WaitOne(mainMutex);
            try {
                GuidData[] registeredApplicationInstances = GetApplicationInstances(false);
                if(registeredApplicationInstances.Length == 0) {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = Uri.UnescapeDataString(args[2]);
                    startInfo.Arguments = Uri.UnescapeDataString(args[3]);
                    if(args.Length == 5)
                        startInfo.WorkingDirectory = args[4];
                    startProcess(startInfo);
                } else {
                    SendExecuteMessage(registeredApplicationInstances, Uri.UnescapeDataString(args[1]));
                }
            } finally {
                mainMutex.ReleaseMutex();
            }
        }
        protected override string ApplicationId { get { return applicationId; } }
        void SendExecuteMessage(GuidData[] applicationInstances, string command) {
            List<Exception> exceptions = null;
            foreach(GuidData applicationInstanceId in applicationInstances) {
                try {
                    using(ChannelFactory<IApplicationInstance> pipeFactory = new ChannelFactory<IApplicationInstance>(new NetNamedPipeBinding(), new EndpointAddress(string.Format("{0}/{1}", GetServiceUri(applicationInstanceId), EndPointName)))) {
                        IApplicationInstance pipeProxy = pipeFactory.CreateChannel();
                        pipeProxy.Execute(command);
                    }
                } catch(CommunicationException) {
                } catch(Exception e) {
                    if(exceptions == null)
                        exceptions = new List<Exception>();
                    exceptions.Add(e);
                }
            }
            if(exceptions != null)
                throw new InvalidOperationException("", new AggregateException(exceptions.ToArray()));
        }
    }
}