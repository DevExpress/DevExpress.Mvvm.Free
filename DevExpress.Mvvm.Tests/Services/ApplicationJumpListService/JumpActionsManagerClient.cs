#if !DXCORE3
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.Threading;

namespace DevExpress.Mvvm.UI.Native {
    class JumpActionsManagerClient : JumpActionsManagerBase {
        string applicationId;
#if DEBUG
        object currentProcessTag;
#endif
        public JumpActionsManagerClient(int millisecondsTimeout = DefaultMillisecondsTimeout, object currentProcessTag = null) : base(millisecondsTimeout) {
#if DEBUG
            this.currentProcessTag = currentProcessTag;
#endif
        }
        [SecuritySafeCritical]
        public void Run(string[] args, Action<ProcessStartInfo> startProcess) {
            if(args.Length != 4 && args.Length != 5) throw new ArgumentException("", "args");
            applicationId = args[0];
            Mutex mainMutex = WaitMainMutex(false);
            try {
                List<GuidData> aliveApplicationInstances = new List<GuidData>();
                foreach(GuidData instance in GetApplicationInstances(false)) {
                    if(IsAlive(instance))
                        aliveApplicationInstances.Add(instance);
                }
                if(aliveApplicationInstances.Count == 0) {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = Uri.UnescapeDataString(args[2]);
                    startInfo.Arguments = Uri.UnescapeDataString(args[3]);
                    if(args.Length == 5)
                        startInfo.WorkingDirectory = args[4];
                    startProcess(startInfo);
                } else {
                    SendExecuteMessage(aliveApplicationInstances, Uri.UnescapeDataString(args[1]));
                }
            } finally {
                mainMutex.ReleaseMutex();
            }
        }
        protected override string ApplicationId { get { return applicationId; } }
#if DEBUG
        protected override object CurrentProcessTag { get { return currentProcessTag; } }
#endif
        void SendExecuteMessage(IEnumerable<GuidData> applicationInstances, string command) {
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
#endif