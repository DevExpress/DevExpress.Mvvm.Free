#if !NETFX_CORE
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
#else
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Example {
    sealed partial class App : Application {
        public App() {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }
        protected override void OnLaunched(LaunchActivatedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            if(rootFrame == null) {
                rootFrame = new Frame();
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }
            if(rootFrame.Content == null) {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            Window.Current.Activate();
        }
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        private void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
#endif