#if NETFX_CORE
using DevExpress.TestFramework.NUnit;
using Windows.UI.Xaml.Controls;
#else
using NUnit.Framework;
using System.Windows.Controls;
#endif
using System;
using System.Threading.Tasks;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class DispatcherServiceTests : BaseWpfFixture {
        [Test, Asynchronous]
#if NETFX_CORE
        public async Task DispatcherServiceTest() {
#else
        public void DispatcherServiceTest() {
#endif
            TestVM vm = new TestVM();
            UserControl control = new UserControl() { DataContext = vm };
            DispatcherService service = new DispatcherService();
            Interactivity.Interaction.GetBehaviors(control).Add(service);
            Window.Content = control;
#if NETFX_CORE
            await EnqueueShowWindow();
#else
            EnqueueShowWindow();
#endif
            EnqueueCallback(() => {
                Assert.IsFalse(vm.IsProgress);
                vm.Calculate();
                Assert.IsTrue(vm.IsProgress);
            });
#if NETFX_CORE
            await WaitConditional(() => vm.Task.IsCompleted);
#else
            EnqueueWait(() => vm.Task.IsCompleted);
#endif
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
#if !NETFX_CORE
                DispatcherHelper.DoEvents();
#endif
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