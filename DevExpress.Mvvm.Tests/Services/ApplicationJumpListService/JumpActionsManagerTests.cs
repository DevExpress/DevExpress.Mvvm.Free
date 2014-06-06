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
    [TestFixture]
    public class JumpActionsManagerTests : AsyncTestFixture {
        [SetUp]
        public void SetUp() {
            TestProcess.AddApplication("launcher.exe", p => {
                JumpActionsManagerClient client = new JumpActionsManagerClient(MillisecondsTimeout);
                client.Run(p.CommandLineArgs.Skip(1).ToArray(), s => new TestProcess(this, s).Start());
            });
        }
        [TearDown]
        public void TearDown() {
            TestProcess.ClearApplications();
        }
        [Test]
        public void RegisterJumpItem_ActivateIt_CheckActionExecuted() {
            try {
                TestJumpAction jumpAction = new TestJumpAction();
                TestProcess.AddApplication("test.exe", p => {
                    using(JumpActionsManager jumpActionsManager = new JumpActionsManager(p, MillisecondsTimeout)) {
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
                TestProcess process = new TestProcess(this, new ProcessStartInfo("test.exe"));
                process.Start();
                process.GetBreakpoint("registered").WaitAndContinue();
                new TestProcess(this, jumpAction.StartInfo).Start();
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
                TestProcess process = new TestProcess(this, new ProcessStartInfo("test.exe"));
                process.Start();
                process.GetBreakpoint("registered").WaitAndContinue();
                process.WaitExit();
                new TestProcess(this, jumpAction.StartInfo).Start();
                process = TestProcess.WaitProcessStart("test.exe", this);
                process.GetBreakpoint("registered").WaitAndContinue();
                process.GetBreakpoint("action").WaitAndContinue();
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
    public class TestBreakpoint : AsyncTestObjectBase {
        readonly ManualResetEvent e1 = new ManualResetEvent(false);
        readonly ManualResetEvent e2 = new ManualResetEvent(false);

        public TestBreakpoint(AsyncTestFixture fixture) : base(fixture) { }
        public void Reach() {
            e1.Set();
            Fixture.WaitOne(e2);
        }
        public void Wait() {
            Fixture.WaitOne(e1);
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
        volatile bool waitExit = false;
        static Dictionary<TestProcess, bool> runningProcesses = new Dictionary<TestProcess, bool>();
        static Dictionary<string, Tuple<AutoResetEvent, AutoResetEvent>> waitStart = new Dictionary<string, Tuple<AutoResetEvent, AutoResetEvent>>();
        Thread thread = null;

        public TestProcess(AsyncTestFixture fixture, ProcessStartInfo startInfo)
            : base(fixture) {
            this.startInfo = startInfo;
            this.entryPoint = registeredApplications[startInfo.FileName];
            breakpoints = new ConcurrentDictionary<string, TestBreakpoint>();
        }
        public TestBreakpoint GetBreakpoint(string name) { return breakpoints.GetOrAdd(name, s => new TestBreakpoint(Fixture)); }
        public string ExecutablePath { get { return this.startInfo.FileName; } }
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
                foreach(TestProcess process in runningProcessesList) {
                    process.WaitExit();
                }
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
        public void WaitExit() {
            waitExit = true;
            if(waitProcess != null)
                Fixture.WaitOne(waitProcess);
            if(exception != null)
                throw exception;
        }
        public void DoEvents() {
            DateTime start = DateTime.Now;
            while(Fixture.Timeout > DateTime.Now - start) {
                if(waitExit) return;
                DispatcherHelper.DoEvents();
            }
            throw new Exception("Timeout");
        }
        string ICurrentProcess.ApplicationId { get { return Uri.EscapeDataString(ExecutablePath); } }
    }
}