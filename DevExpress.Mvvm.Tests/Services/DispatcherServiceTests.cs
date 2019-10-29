using System.Windows.Controls;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class DispatcherServiceTests : BaseWpfFixture {
        [Test, Asynchronous]
        public void DispatcherServiceTest() {
            TestVM vm = new TestVM();
            UserControl control = new UserControl() { DataContext = vm };
            DispatcherService service = new DispatcherService();
            Interactivity.Interaction.GetBehaviors(control).Add(service);
            Window.Content = control;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.IsFalse(vm.IsProgress);
                vm.Calculate();
                Assert.IsTrue(vm.IsProgress);
            });
            EnqueueWait(() => vm.Task.IsCompleted);
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                DispatcherHelper.DoEvents();
                Assert.IsFalse(vm.IsProgress);
                Assert.IsTrue(vm.IsCompleted);
            });
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
        public void InvokeAsync_Test00() {
            var service = new DispatcherService();
            service.Delay = TimeSpan.FromMilliseconds(200);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            service.BeginInvoke(() => sw.Stop());
            EnqueueWait(() => !sw.IsRunning);
            Assert.IsTrue(sw.ElapsedMilliseconds >= 180);
        }
        [Test, Asynchronous]
        public void InvokeAsync_Test01() {
            var service = new DispatcherService();
            service.Delay = TimeSpan.FromMilliseconds(200);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Task.Factory.StartNew(() => service.BeginInvoke(() => sw.Stop()));
            EnqueueWait(() => !sw.IsRunning);
            Assert.IsTrue(sw.Elapsed.TotalMilliseconds >= 180, sw.ElapsedMilliseconds.ToString());
        }
        [Test, Asynchronous]
        public void InvokeAsync_Test02() {
            var service = new DispatcherService();
            bool isStopped = false;
            service.BeginInvoke(() => { isStopped = true; });
            Assert.IsFalse(isStopped);
            DispatcherHelper.DoEvents();
            Assert.IsTrue(isStopped);
        }
        [Test, Asynchronous]
        public void InvokeAsync_Test03() {
            var service = new DispatcherService();
            bool isStopped = false;
            Task.Factory.StartNew(() => service.BeginInvoke(() => { isStopped = true; }));
            Assert.IsFalse(isStopped);
            EnqueueWait(() => isStopped);
            Assert.IsTrue(isStopped);
        }
        [Test, Asynchronous]
        public void InvokeAsync_Test05() {
            var service = new DispatcherService();
            bool val1 = false;
            bool complete = false;
            service.BeginInvoke(() => {
                Assert.IsFalse(val1);
                val1 = true;
            }).ContinueWith(o => {
                val1 = true;
                complete = true;
            });
            EnqueueWait(() => complete);
            Assert.IsTrue(val1);
        }
        [Test, Asynchronous]
        public void InvokeAsync_Test06() {
            var service = new DispatcherService();
            service.Delay = TimeSpan.FromMilliseconds(100);
            bool val1 = false;
            bool complete = false;
            service.BeginInvoke(() => {
                Assert.IsFalse(val1);
                val1 = true;
            }).ContinueWith(o => {
                val1 = true;
                complete = true;
            });
            EnqueueWait(() => complete);
            Assert.IsTrue(val1);
        }
        [Test]
        public void Invoke_Test00() {
            var service = new DispatcherService();
            bool val1 = false;
            service.Invoke(() => val1 = true);
            Assert.IsTrue(val1);
        }
        [Test]
        public void Invoke_Test01() {
            var service = new DispatcherService();
            bool val1 = false;
            bool checkValue = false;
            bool isSet = false;
            Task.Factory.StartNew(() => {
                service.Invoke(() => val1 = true);
                checkValue = val1;
                isSet = true;
            });
            EnqueueWait(() => isSet);
            Assert.IsTrue(checkValue);
        }

        class TestVM : ViewModelBase {
            public IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
            public bool IsProgress { get; private set; }
            public bool IsCompleted { get; private set; }
            public TestVM() {
                IsCompleted = false;
                IsProgress = false;
            }
            public Task Task { get; private set; }
            public void Calculate() {
                IsProgress = true;
                Task = Task.Factory.StartNew(CalcCore).ContinueWith(x => {
                    DispatcherService.BeginInvoke(new Action(() => IsProgress = false));
                });
            }
            void CalcCore() {
                DispatcherService.BeginInvoke(() => {
                    IsCompleted = true;
                });
            }
        }
    }
}