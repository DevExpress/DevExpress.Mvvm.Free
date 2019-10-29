#if !DXCORE3
using DevExpress.Mvvm.UI.Native;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture, Platform("NET")]
    public class JumpActionsManagerTests : AsyncTestFixture {
        string applicationID;

        [SetUp]
        public void SetUp() {
            applicationID = Guid.NewGuid().ToString();
            TestProcess.AddApplication("launcher.exe", p => {
                using(JumpActionsManagerClient client = new JumpActionsManagerClient(MillisecondsTimeout, p)) {
                    GC.SuppressFinalize(client);
                    client.Run(p.CommandLineArgs.Skip(1).ToArray(), s => new TestProcess(applicationID, this, s, p.ProcessID + " launcher").Start());
                }
            });
        }
        [TearDown]
        public void TearDown() {
            TestProcess.ClearApplications();
            JumpActionsManagerBase.ClearMemoryMappedFiles();
            applicationID = null;
        }
        [Test]
        public void RegisterJumpItem_ActivateIt_CheckActionExecuted() {
            try {
                TestJumpAction jumpAction = new TestJumpAction();
                TestProcess.AddApplication("test.exe", p => {
                    using(JumpActionsManager jumpActionsManager = new JumpActionsManager(p, MillisecondsTimeout)) {
                        GC.SuppressFinalize(jumpActionsManager);
                        jumpAction.CommandId = "Run Command!";
                        jumpAction.Action = () => {
                            p.GetBreakpoint("action").Reach();
                        };
                        jumpActionsManager.BeginUpdate();
                        try {
                            jumpActionsManager.RegisterAction(jumpAction, "/DO=", () => "launcher.exe");
                        } finally {
                            jumpActionsManager.EndUpdate();
                        }
                        p.GetBreakpoint("registered").Reach();
                        p.DoEvents();
                    }
                });
                TestProcess process = new TestProcess(applicationID, this, new ProcessStartInfo("test.exe"), "1");
                process.Start();
                process.GetBreakpoint("registered").WaitAndContinue();
                new TestProcess(applicationID, this, jumpAction.StartInfo, "2").Start();
                process.GetBreakpoint("action").WaitAndContinue();
            } finally {
                TestProcess.WaitAllProcessesExit();
            }
        }
        [Test]
        public void RegisterJumpItem_CloseProgram_ActivateItem_CheckActionExecuted() {
            try {
                TestJumpAction jumpAction = new TestJumpAction();
                TestProcess.AddApplication("test.exe", p => {
                    using(JumpActionsManager jumpActionsManager = new JumpActionsManager(p, MillisecondsTimeout)) {
                        GC.SuppressFinalize(jumpActionsManager);
                        jumpAction.CommandId = "Run Command!";
                        jumpAction.Action = () => {
                            p.GetBreakpoint("action").Reach();
                        };
                        jumpActionsManager.BeginUpdate();
                        try {
                            jumpActionsManager.RegisterAction(jumpAction, "/DO=", () => "launcher.exe");
                        } finally {
                            jumpActionsManager.EndUpdate();
                        }
                        p.GetBreakpoint("registered").Reach();
                        p.DoEvents();
                    }
                });
                TestProcess process = new TestProcess(applicationID, this, new ProcessStartInfo("test.exe"), "13");
                process.Start();
                process.GetBreakpoint("registered").WaitAndContinue();
                process.SendCloseMessage();
                process.WaitExit();
                new TestProcess(applicationID, this, jumpAction.StartInfo, "14").Start();
                process = TestProcess.WaitProcessStart("test.exe", this);
                process.GetBreakpoint("registered").WaitAndContinue();
                process.GetBreakpoint("action").WaitAndContinue();
            } finally {
                TestProcess.WaitAllProcessesExit();
            }
        }
        [Test]
        public void RegisterJumpItem_StartAnotherInstance_KillAnotherInstance_CloseProgram() {
            try {
                int actionIndex = 0;
                TestJumpAction[] jumpActions = new TestJumpAction[] { new TestJumpAction(), new TestJumpAction() };
                TestProcess.AddApplication("test.exe", p => {
                    JumpActionsManager jumpActionsManager = new JumpActionsManager(p, MillisecondsTimeout);
                    GC.SuppressFinalize(jumpActionsManager);
                    try {
                        TestJumpAction jumpAction = jumpActions[actionIndex++];
                        jumpAction.CommandId = "Run Command!";
                        jumpAction.Action = () => {
                            p.GetBreakpoint("action").Reach();
                        };
                        jumpActionsManager.BeginUpdate();
                        try {
                            jumpActionsManager.RegisterAction(jumpAction, "/DO=", () => "launcher.exe");
                            if(p.CommandLineArgs.Skip(1).Any())
                                p.EnvironmentExit();
                        } finally {
                            if(!p.DoEnvironmentExit)
                                jumpActionsManager.EndUpdate();
                        }
                        p.GetBreakpoint("registered").Reach();
                        p.DoEvents();
                    } finally {
                        if(!p.DoEnvironmentExit)
                            jumpActionsManager.Dispose();
                    }
                });
                TestProcess process = new TestProcess(applicationID, this, new ProcessStartInfo("test.exe"), "11");
                process.Start();
                process.GetBreakpoint("registered").WaitAndContinue();
                TestProcess processToKill = new TestProcess(applicationID, this, new ProcessStartInfo("test.exe", "kill"), "12");
                processToKill.Start();
                processToKill.WaitExit();
                process.SendCloseMessage();
                process.WaitExit();
            } finally {
                TestProcess.WaitAllProcessesExit();
            }
        }
        [Test]
        public void RegisterJumpItem_KillProcess_ActivateItem_CheckActionExecuted() {
            try {
                TestJumpAction jumpAction = new TestJumpAction();
                TestProcess.AddApplication("test.exe", p => {
                    JumpActionsManager jumpActionsManager = new JumpActionsManager(p, MillisecondsTimeout);
                    GC.SuppressFinalize(jumpActionsManager);
                    try {
                        jumpAction.CommandId = "Run Command!";
                        jumpAction.Action = () => {
                            p.GetBreakpoint("action").Reach();
                        };
                        jumpActionsManager.BeginUpdate();
                        try {
                            jumpActionsManager.RegisterAction(jumpAction, "/DO=", () => "launcher.exe");
                        } finally {
                            jumpActionsManager.EndUpdate();
                        }
                        p.GetBreakpoint("registered").Reach();
                        if(!p.CommandLineArgs.Skip(1).Any())
                            p.EnvironmentExit();
                        p.DoEvents();
                    } finally {
                        if(!p.DoEnvironmentExit)
                            jumpActionsManager.Dispose();
                    }
                });
                TestProcess process = new TestProcess(applicationID, this, new ProcessStartInfo("test.exe"), "9");
                process.Start();
                process.GetBreakpoint("registered").WaitAndContinue();
                process.WaitExit();
                new TestProcess(applicationID, this, jumpAction.StartInfo, "10").Start();
                process = TestProcess.WaitProcessStart("test.exe", this);
                process.GetBreakpoint("registered").WaitAndContinue();
                process.GetBreakpoint("action").WaitAndContinue();
            } finally {
                TestProcess.WaitAllProcessesExit();
            }
        }
        [Test]
        public void StartTwoInstances_RegisterJumpItem_KillFirstInstance_ActivateItem_CheckActionExecuted() {
            try {
                int actionIndex = 0;
                TestJumpAction[] jumpActions = new TestJumpAction[] { new TestJumpAction(), new TestJumpAction() };
                TestProcess.AddApplication("test.exe", p => {
                    JumpActionsManager jumpActionsManager = new JumpActionsManager(p, MillisecondsTimeout);
                    GC.SuppressFinalize(jumpActionsManager);
                    try {
                        TestJumpAction jumpAction = jumpActions[actionIndex++];
                        jumpAction.CommandId = "Run Command!";
                        jumpAction.Action = () => {
                            p.GetBreakpoint("action").Reach();
                        };
                        jumpActionsManager.BeginUpdate();
                        try {
                            jumpActionsManager.RegisterAction(jumpAction, "/DO=", () => "launcher.exe");
                        } finally {
                            jumpActionsManager.EndUpdate();
                        }
                        p.GetBreakpoint("registered").Reach();
                        if(!p.CommandLineArgs.Skip(1).Any())
                            p.EnvironmentExit();
                        p.DoEvents();
                    } finally {
                        if(!p.DoEnvironmentExit)
                            jumpActionsManager.Dispose();
                    }
                });
                TestProcess process1 = new TestProcess(applicationID, this, new ProcessStartInfo("test.exe"), "3");
                TestProcess process2 = new TestProcess(applicationID, this, new ProcessStartInfo("test.exe", "do_not_kill"), "4");
                process1.Start();
                process2.Start();
                process1.GetBreakpoint("registered").Wait();
                process2.GetBreakpoint("registered").Wait();
                process1.GetBreakpoint("registered").Continue();
                process2.GetBreakpoint("registered").Continue();
                process1.WaitExit();
                new TestProcess(applicationID, this, jumpActions[1].StartInfo, "8").Start();
                process2.GetBreakpoint("action").WaitAndContinue();
                process2.SendCloseMessage();
                process2.WaitExit();
            } finally {
                TestProcess.WaitAllProcessesExit();
            }
        }
        [Test]
        public void StartTwoInstances_RegisterJumpItem_KillSecondInstance_ActivateItem_CheckActionExecuted() {
            try {
                int actionIndex = 0;
                TestJumpAction[] jumpActions = new TestJumpAction[] { new TestJumpAction(), new TestJumpAction() };
                TestProcess.AddApplication("test.exe", p => {
                    JumpActionsManager jumpActionsManager = new JumpActionsManager(p, MillisecondsTimeout);
                    GC.SuppressFinalize(jumpActionsManager);
                    try {
                        TestJumpAction jumpAction = jumpActions[actionIndex++];
                        jumpAction.CommandId = "Run Command!";
                        jumpAction.Action = () => {
                            p.GetBreakpoint("action").Reach();
                        };
                        jumpActionsManager.BeginUpdate();
                        try {
                            jumpActionsManager.RegisterAction(jumpAction, "/DO=", () => "launcher.exe");
                        } finally {
                            jumpActionsManager.EndUpdate();
                        }
                        p.GetBreakpoint("registered").Reach();
                        if(!p.CommandLineArgs.Skip(1).Any())
                            p.EnvironmentExit();
                        p.DoEvents();
                    } finally {
                        if(!p.DoEnvironmentExit)
                            jumpActionsManager.Dispose();
                    }
                });
                TestProcess process1 = new TestProcess(applicationID, this, new ProcessStartInfo("test.exe", "do_not_kill"), "5");
                TestProcess process2 = new TestProcess(applicationID, this, new ProcessStartInfo("test.exe"), "6");
                process1.Start();
                process2.Start();
                process1.GetBreakpoint("registered").Wait();
                process2.GetBreakpoint("registered").Wait();
                process1.GetBreakpoint("registered").Continue();
                process2.GetBreakpoint("registered").Continue();
                process2.WaitExit();
                new TestProcess(applicationID, this, jumpActions[0].StartInfo, "7").Start();
                process1.GetBreakpoint("action").WaitAndContinue();
                process1.SendCloseMessage();
                process1.WaitExit();
            } finally {
                TestProcess.WaitAllProcessesExit();
            }
        }
    }
    public class TestJumpAction : IJumpAction {
        public string CommandId { get; set; }
        public string ApplicationPath { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public ProcessStartInfo StartInfo { get; private set; }
        public Action Action { get; set; }
        public void SetStartInfo(string applicationPath, string arguments) {
            StartInfo = new ProcessStartInfo(applicationPath, arguments);
        }
        public void Execute() {
            if(Action != null)
                Action();
        }
    }
    public class TestBreakpointNotReachedException : Exception {
        public TestBreakpointNotReachedException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class TestBreakpoint : AsyncTestObjectBase {
        readonly ManualResetEvent e1 = new ManualResetEvent(false);
        readonly ManualResetEvent e2 = new ManualResetEvent(false);
        readonly TestProcess process;
        readonly string name;

        public TestBreakpoint(string name, AsyncTestFixture fixture, TestProcess process)
            : base(fixture) {
            this.name = name;
            this.process = process;
        }
        public void Reach() {
            e1.Set();
            Fixture.WaitOne(e2);
        }
        public void Wait() {
            try {
                try {
                    Fixture.WaitOne(e1);
                } catch(TimeoutException e) {
                    throw new TestBreakpointNotReachedException(name, e);
                } finally {
                    try {
                        process.Check();
                    } catch(Exception f) {
                        throw new Exception(string.Format("Exception in TestBreakpoint.Wait() ({0})", name), f);
                    }
                }
            } catch {
                process.SendCloseMessage();
                throw;
            }
        }
        public void Continue() {
            e2.Set();
        }
        public void WaitAndContinue() {
            Wait();
            Continue();
        }
    }
    public class TestProcess : AsyncTestObjectBase, ICurrentProcess {
        static readonly Dictionary<string, Action<TestProcess>> registeredApplications = new Dictionary<string, Action<TestProcess>>();

        public static void ClearApplications() {
            registeredApplications.Clear();
        }
        public static void AddApplication(string fileName, Action<TestProcess> entryPoint) {
            registeredApplications.Add(fileName, entryPoint);
        }

        class TestProcessExitException : Exception { }

        readonly ProcessStartInfo startInfo;
        readonly Action<TestProcess> entryPoint;
        readonly ConcurrentDictionary<string, TestBreakpoint> breakpoints;
        Exception exception;
        ManualResetEvent waitProcess;
        volatile bool stopMainLoop = false;
        static Dictionary<TestProcess, bool> runningProcesses = new Dictionary<TestProcess, bool>();
        static Dictionary<string, Tuple<AutoResetEvent, AutoResetEvent>> waitStart = new Dictionary<string, Tuple<AutoResetEvent, AutoResetEvent>>();
        Thread thread = null;
        readonly string applicationID;

        public TestProcess(string applicationID, AsyncTestFixture fixture, ProcessStartInfo startInfo, string processID)
            : base(fixture) {
            this.applicationID = applicationID;
            ProcessID = processID;
            this.startInfo = startInfo;
            this.entryPoint = registeredApplications[startInfo.FileName];
            breakpoints = new ConcurrentDictionary<string, TestBreakpoint>();
        }
        public TestBreakpoint GetBreakpoint(string name) { return breakpoints.GetOrAdd(name, s => new TestBreakpoint(name, Fixture, this)); }
        public string ExecutablePath { get { return this.startInfo.FileName; } }
        public string ProcessID { get; private set; }
        public IEnumerable<string> CommandLineArgs {
            get {
                yield return startInfo.FileName;
                foreach(string arg in Regex.Matches(startInfo.Arguments, @"[^ ""]+|""[^""]*""").Cast<Match>().Select(m => m.Value).ToArray()) {
                    if(arg.Length >= 2 && arg[0] == '\"' && arg[arg.Length - 1] == '\"')
                        yield return arg.Substring(1, arg.Length - 2);
                    else
                        yield return arg;
                }
            }
        }
        public bool DoEnvironmentExit { get; private set; }
        public void EnvironmentExit() {
            DoEnvironmentExit = true;
            throw new TestProcessExitException();
        }
        public static TestProcess WaitProcessStart(string applicationName, AsyncTestFixture fixture) {
            AutoResetEvent wait, cont;
            lock(runningProcesses) {
                TestProcess runningProcess = runningProcesses.Keys.Where(p => string.Equals(p.startInfo.FileName, applicationName, StringComparison.Ordinal)).FirstOrDefault();
                if(runningProcess != null) return runningProcess;
                wait = new AutoResetEvent(false);
                cont = new AutoResetEvent(false);
                waitStart.Add(applicationName, new Tuple<AutoResetEvent, AutoResetEvent>(wait, cont));
            }
            fixture.WaitOne(wait);
            TestProcess process = runningProcesses.Keys.Where(p => string.Equals(p.startInfo.FileName, applicationName, StringComparison.Ordinal)).Single();
            cont.Set();
            return process;
        }
        public static void WaitAllProcessesExit() {
            try {
                List<TestProcess> runningProcessesList;
                lock(runningProcesses) {
                    runningProcessesList = new List<TestProcess>(runningProcesses.Keys);
                }
                foreach(TestProcess process in runningProcessesList)
                    process.SendCloseMessage();
                foreach(TestProcess process in runningProcessesList)
                    process.WaitExit();
                Assert.AreEqual(0, runningProcesses.Count);
            } finally {
                List<TestProcess> runningProcessesList;
                lock(runningProcesses) {
                    runningProcessesList = new List<TestProcess>(runningProcesses.Keys);
                }
                foreach(TestProcess process in runningProcessesList) {
                    if(process.thread != null)
                        process.thread.Abort();
                }
            }
        }
        public void Start() {
            lock(runningProcesses) {
                if(thread != null)
                    throw new InvalidOperationException();
                waitProcess = new ManualResetEvent(false);
                thread = new Thread(() => {
                    try {
                        entryPoint(this);
                    } catch(TestProcessExitException) {
                        JumpActionsManagerBase.EmulateProcessKill(this);
                    } catch(Exception e) {
                        exception = e;
                    } finally {
                        lock(runningProcesses)
                            runningProcesses.Remove(this);
                        waitProcess.Set();
                    }
                });
                thread.Start();
                runningProcesses.Add(this, true);
                Tuple<AutoResetEvent, AutoResetEvent> onStart;
                if(waitStart.TryGetValue(startInfo.FileName, out onStart)) {
                    onStart.Item1.Set();
                    Fixture.WaitOne(onStart.Item2);
                    waitStart.Remove(startInfo.FileName);
                }
            }
        }
        public void Check() {
            if(exception != null)
                throw new AggregateException(exception.Message, exception);
        }
        public void SendCloseMessage() {
            stopMainLoop = true;
        }
        public void WaitExit() {
            if(waitProcess != null)
                Fixture.WaitOne(waitProcess);
            Check();
        }
        public void DoEvents() {
            DateTime start = DateTime.Now;
            while(Fixture.TestTimeout > DateTime.Now - start) {
                if(stopMainLoop) return;
                DispatcherHelper.DoEvents();
            }
            throw new Exception(string.Format("Test process timeout ({0})", ProcessID));
        }
        string ICurrentProcess.ApplicationId { get { return applicationID + Uri.EscapeDataString(ExecutablePath); } }
    }
}
#endif