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