#if !NETFX_CORE
using System.Windows;

namespace DevExpress.Mvvm.Tests {
    public partial class App : Application {
        public App() {
            Startup += new StartupEventHandler(App_Startup);
            InitializeComponent();
        }
        void App_Startup(object sender, StartupEventArgs e) {
            DevExpress.TestHelper.RunTests(this);
        }
    }
}
#else
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace DevExpress.Mvvm.Tests {
    sealed partial class App : Application {
        public App() {
            this.UnhandledException += DevExpress.TestFramework.TestRunner.AppUnhandledException;
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args) {
            DevExpress.TestFramework.TestRunner.RunTests(this.GetType());
        }
        private void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
#endif