using System.Windows;

namespace Example {
    public partial class App : Application {
        public App() {
            Startup += OnStartup;
#if SILVERLIGHT
            InitializeComponent();
#endif
        }
        void OnStartup(object sender, StartupEventArgs e) {
#if SILVERLIGHT
            this.RootVisual = new MainPage();
#else
            MainWindow = new MainWindow();
            MainWindow.Show();
#endif
        }
    }
}
