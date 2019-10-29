#if !DXCORE3
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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
        [ServiceContract]
        protected interface IApplicationInstance {
            [OperationContract]
            void Execute(string command);
        }

        const string MainMutexName = "C4339FDC-8943-4AA6-8DB9-644D68462BC7_";
        protected const string InstancesFileName = "HB42A04D-FFA1-4755-9854-CB8DC81AAE89_";
        protected const string InstanceNamePrefix = "5BCE503C-DB8D-440A-BEEC-9C963A364DBF_";
        protected const string EndPointName = "Pipe";
        Mutex mainMutex;
        Tuple<IntPtr, IntPtr> instancesFile = null;
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
            try {
                Mutex mainMutex = WaitMainMutex(!disposing);
                try {
                    DisposeInstancesFile();
                } finally {
                    mainMutex.ReleaseMutex();
                }
            } catch(TimeoutException) {
                if(disposing)
                    throw;
            }
        }
#if DEBUG
        protected abstract object CurrentProcessTag { get; }
#endif
        [SecuritySafeCritical]
        void DisposeInstancesFile() {
            if(instancesFile == null) return;
            UnmapViewAndCloseFileMapping(instancesFile);
            instancesFile = null;
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
        const byte MaxInstancesCount = 99;
        [SecuritySafeCritical]
        Tuple<IntPtr, IntPtr> GetInstancesFile() {
            if(instancesFile == null) {
                bool alreadyExists;
                instancesFile = CreateFileMappingAndMapView((1 + MaxInstancesCount) * Marshal.SizeOf(typeof(GuidData)), InstancesFileName + ApplicationId, out alreadyExists);
                if(!alreadyExists) {
                    try {
                        UpdateInstancesFile(new GuidData[] { });
                    } catch {
                        DisposeInstancesFile();
                        throw;
                    }
                }
            }
            return instancesFile;
        }
        protected static string GetServiceUri(GuidData applicationInstance) {
            return string.Format("net.pipe://localhost/{0}{1}", InstanceNamePrefix, applicationInstance.AsGuid);
        }
        protected static string GetIsAliveFlagFileName(GuidData applicationInstance) {
            return string.Format("IsAlive_{0}{1}", InstanceNamePrefix, applicationInstance.AsGuid);
        }
        [SecuritySafeCritical]
        protected GuidData[] GetApplicationInstances(bool isCurrentProcessApplicationInstance) {
            Tuple<IntPtr, IntPtr> instancesFile = GetInstancesFile();
            GuidData listProperties = (GuidData)Marshal.PtrToStructure(instancesFile.Item2, typeof(GuidData));
            int instancesCount = listProperties.Byte0;
            GuidData[] instances = new GuidData[instancesCount];
            for(int i = 0; i < instancesCount; ++i)
                instances[i] = (GuidData)Marshal.PtrToStructure(instancesFile.Item2 + (1 + i) * Marshal.SizeOf(typeof(GuidData)), typeof(GuidData));
            return instances;
        }
        [SecuritySafeCritical]
        protected bool IsAlive(GuidData instance) {
            bool isAliveFlagFileExists = false;
            Tuple<IntPtr, IntPtr> file = CreateFileMappingAndMapView(1, GetIsAliveFlagFileName(instance), out isAliveFlagFileExists);
            UnmapViewAndCloseFileMapping(file);
            return isAliveFlagFileExists;
        }
        [SecuritySafeCritical]
        protected void UpdateInstancesFile(GuidData[] instances) {
            Tuple<IntPtr, IntPtr> instancesFile = GetInstancesFile();
            byte instancesCount = CorceInstancesCount(instances.Length);
            for(int i = 0; i < instancesCount; ++i)
                Marshal.StructureToPtr(instances[i], instancesFile.Item2 + (1 + i) * Marshal.SizeOf(typeof(GuidData)), false);
            GuidData listProperties = new GuidData() { Byte0 = instancesCount };
            Marshal.StructureToPtr(listProperties, instancesFile.Item2, false);
        }
        static byte CorceInstancesCount(int instancesCount) {
            return (byte)(instancesCount % MaxInstancesCount);
        }
        protected void WaitOne(WaitHandle waitHandle) {
            if(!waitHandle.WaitOne(MillisecondsTimeout))
                throw new TimeoutException();
        }
        protected Mutex WaitMainMutex(bool safe) {
            Mutex mainMutex = GetMainMutex(safe);
            try {
                WaitOne(mainMutex);
            } catch (AbandonedMutexException) { }
            return mainMutex;
        }
        protected abstract string ApplicationId { get; }

#if DEBUG
        class MemoryMappedFileInfo {
            public MemoryMappedFileInfo(IntPtr fileObject, IntPtr fileView, object owner) {
                FileObject = fileObject;
                FileView = fileView;
                Owner = owner;
            }
            public IntPtr FileObject { get; private set; }
            public IntPtr FileView { get; private set; }
            public object Owner { get; private set; }
        }
        static Dictionary<string, HashSet<MemoryMappedFileInfo>> memoryMappedFiles = new Dictionary<string, HashSet<MemoryMappedFileInfo>>();
        public static void ClearMemoryMappedFiles() {
            memoryMappedFiles.Clear();
        }
        [SecuritySafeCritical]
        public static void EmulateProcessKill(object processTag) {
            MemoryMappedFileInfo[] openFiles = memoryMappedFiles.SelectMany(x => x.Value).Where(m => object.Equals(m.Owner, processTag)).ToArray();
            foreach(var openFile in openFiles)
                UnmapViewAndCloseFileMapping(new Tuple<IntPtr, IntPtr>(openFile.FileObject, openFile.FileView));
        }
#endif
        [SecurityCritical]
        protected Tuple<IntPtr, IntPtr> CreateFileMappingAndMapView(int dwMaximumSizeLow, string lpName, out bool alreadyExists) {
            if(dwMaximumSizeLow == 0) {
                dwMaximumSizeLow = 1;
#if DEBUG
                if(!memoryMappedFiles.ContainsKey(lpName))
                    throw new FileNotFoundException(lpName, lpName);
#endif
            }
            IntPtr fileObject = Import.CreateFileMapping(Import.InvalidHandleValue, IntPtr.Zero, Import.PageReadwrite, 0, (uint)dwMaximumSizeLow, lpName);
            if(fileObject == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            alreadyExists = Marshal.GetLastWin32Error() == Import.ERROR_ALREADY_EXISTS;
            IntPtr fileView = Import.MapViewOfFile(fileObject, Import.FileMapAllAccess, 0, 0, UIntPtr.Zero);
            if(fileView == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
#if DEBUG
            HashSet<MemoryMappedFileInfo> refs;
            if(!memoryMappedFiles.TryGetValue(lpName, out refs)) {
                refs = new HashSet<MemoryMappedFileInfo>();
                memoryMappedFiles.Add(lpName, refs);
            }
            refs.Add(new MemoryMappedFileInfo(fileObject, fileView, CurrentProcessTag));
#endif
            return new Tuple<IntPtr, IntPtr>(fileObject, fileView);
        }
        [SecurityCritical]
        protected static void UnmapViewAndCloseFileMapping(Tuple<IntPtr, IntPtr> file) {
#if DEBUG
            var fileData = memoryMappedFiles.Where(p => p.Value.RemoveWhere(f => f.FileObject == file.Item1) == 1).Single();
            if(fileData.Value.Count == 0)
                memoryMappedFiles.Remove(fileData.Key);
#endif
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
}
#endif