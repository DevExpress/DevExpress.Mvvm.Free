using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceModel;
using System.Threading;

namespace DevExpress.Mvvm.UI.Native {
    public abstract class JumpActionsManagerBase {
        public const int DefaultMillisecondsTimeout = 30;
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
        protected struct InstancesProperties {
            public GuidData NameSuffix;
            public int InstancesCount;
        }
        [ServiceContract]
        protected interface IApplicationInstance {
            [OperationContract]
            void Execute(string command);
        }

        protected const string MainMutexName = "78E27AC7-D44D-4BC9-8035-B7E032F6E485_";
        protected const string InstancesPropertiesFileName = "F23F13DB-7891-4E3E-90EC-BE0F00F136F4_";
        protected const string InstanceNamePrefix = "20C14DA6-B82A-4370-8760-951ED22346CB_";
        protected const string InstancesFileNamePrefix = "4EED163C-BC17-4840-A543-14FAF6D1BCEC_";
        protected const string EndPointName = "Pipe";
        Mutex mainMutex;
        MemoryMappedFile instancesPropertiesFile;
        GuidData instancesFileNameSuffix = new GuidData(Guid.Empty);
        MemoryMappedFile instancesFile = null;

        public JumpActionsManagerBase(int millisecondsTimeout) {
            MillisecondsTimeout = millisecondsTimeout;
        }
        protected int MillisecondsTimeout { get; private set; }
        protected Mutex MainMutex {
            [SecuritySafeCritical]
            get {
                if(mainMutex == null)
                    mainMutex = new Mutex(false, MainMutexName + ApplicationId);
                return mainMutex;
            }
        }
        MemoryMappedFile InstancesPropertiesFile {
            [SecuritySafeCritical]
            get {
                if(instancesPropertiesFile == null) {
                    try {
                        instancesPropertiesFile = MemoryMappedFile.OpenExisting(InstancesPropertiesFileName + ApplicationId);
                    } catch(FileNotFoundException) {
                        instancesPropertiesFile = MemoryMappedFile.CreateNew(InstancesPropertiesFileName + ApplicationId, Marshal.SizeOf(typeof(InstancesProperties)));
                        UpdateInstancesFile(new GuidData[] { });
                    }
                }
                return instancesPropertiesFile;
            }
        }
        protected string GetServiceUri(GuidData applicationInstance) {
            return string.Format("net.pipe://localhost/{0}{1}", InstanceNamePrefix, applicationInstance.AsGuid);
        }
        [SecuritySafeCritical]
        protected GuidData[] GetApplicationInstances() {
            Tuple<MemoryMappedFile, int> instancesFile = GetInstancesFile();
            if(instancesFile.Item2 == 0) return new GuidData[] { };
            GuidData[] instances = new GuidData[instancesFile.Item2];
            using(var accessor = instancesFile.Item1.CreateViewAccessor())
                accessor.ReadArray(0, instances, 0, instances.Length);
            return instances;
        }
        [SecuritySafeCritical]
        protected Tuple<MemoryMappedFile, int> GetInstancesFile() {
            InstancesProperties listProperties;
            using(var acccessor = InstancesPropertiesFile.CreateViewAccessor())
                acccessor.Read(0, out listProperties);
            if(listProperties.NameSuffix.AsGuid != instancesFileNameSuffix.AsGuid) {
                if(instancesFile != null)
                    instancesFile.Dispose();
                instancesFileNameSuffix = listProperties.NameSuffix;
                instancesFile = instancesFileNameSuffix.AsGuid == Guid.Empty ? null : MemoryMappedFile.OpenExisting(GetInstancesFileName(instancesFileNameSuffix));
            }
            return new Tuple<MemoryMappedFile, int>(instancesFile, listProperties.InstancesCount);
        }
        [SecuritySafeCritical]
        protected void UpdateInstancesFile(GuidData[] instances) {
            if(instancesFile != null)
                instancesFile.Dispose();
            if(instances.Length != 0) {
                instancesFileNameSuffix = new GuidData(Guid.NewGuid());
                instancesFile = MemoryMappedFile.CreateNew(GetInstancesFileName(instancesFileNameSuffix), instances.Length * Marshal.SizeOf(typeof(GuidData)));
                using(var accessor = instancesFile.CreateViewAccessor())
                    accessor.WriteArray(0, instances, 0, instances.Length);
            } else {
                instancesFile = null;
                instancesFileNameSuffix = new GuidData(Guid.Empty);
            }
            InstancesProperties listProperties = new InstancesProperties() { InstancesCount = instances.Length, NameSuffix = instancesFileNameSuffix };
            using(var acccessor = InstancesPropertiesFile.CreateViewAccessor())
                acccessor.Write(0, ref listProperties);
        }
        string GetInstancesFileName(GuidData instancesFileNameSuffix) {
            return InstancesFileNamePrefix + instancesFileNameSuffix.AsGuid.ToString();
        }
        protected void WaitOne(WaitHandle waitHandle) {
            if(!waitHandle.WaitOne(MillisecondsTimeout))
                throw new TimeoutException();
        }
        protected abstract string ApplicationId { get; }
    }
    public class JumpActionsManagerClient : JumpActionsManagerBase {
        string applicationId;

        public JumpActionsManagerClient(int millisecondsTimeout = DefaultMillisecondsTimeout) : base(millisecondsTimeout) { }
        public void Run(string[] args, Action<ProcessStartInfo> startProcess) {
            if(args.Length != 4 && args.Length != 5) throw new ArgumentException("", "args");
            applicationId = args[0];
            WaitOne(MainMutex);
            try {
                GuidData[] registeredApplicationInstances = GetApplicationInstances();
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
                MainMutex.ReleaseMutex();
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