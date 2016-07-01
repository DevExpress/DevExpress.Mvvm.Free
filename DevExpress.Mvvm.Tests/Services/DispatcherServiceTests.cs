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