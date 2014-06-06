using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace DevExpress.Mvvm.UI {
    public class DXSplashScreenService : ViewServiceBase, ISplashScreenService {
        public static readonly DependencyProperty SplashScreenTypeProperty =
           DependencyProperty.Register("SplashScreenType", typeof(Type), typeof(DXSplashScreenService),
           new PropertyMetadata(null));
        public static readonly DependencyProperty UseFadeEffectProperty =
            DependencyProperty.Register("UseFadeEffect", typeof(bool?), typeof(DXSplashScreenService),
            new PropertyMetadata(null));
        public Type SplashScreenType {
            get { return (Type)GetValue(SplashScreenTypeProperty); }
            set { SetValue(SplashScreenTypeProperty, value); }
        }
        public bool? UseFadeEffect {
            get { return (bool?)GetValue(UseFadeEffectProperty); }
            set { SetValue(UseFadeEffectProperty, value); }
        }

        bool ISplashScreenService.IsSplashScreenActive {
            get { return DXSplashScreen.IsActive; }
        }
        Task ISplashScreenService.ShowSplashScreen(string documentType) {
            if(SplashScreenType != null) {
                if(!string.IsNullOrEmpty(documentType) || ViewTemplate != null || ViewLocator != null)
                    throw new InvalidOperationException(DXSplashScreenExceptions.ServiceException1);
                if(typeof(ISplashScreen).IsAssignableFrom(SplashScreenType))
                    return DXSplashScreen.Show(SplashScreenType, UseFadeEffect);
                if(typeof(UIElement).IsAssignableFrom(SplashScreenType)) {
                    SplashScreenViewModel = new SplashScreenViewModel();
                    SplashScreen = (UIElement)Activator.CreateInstance(SplashScreenType);
                    SplashScreen.With(x => x as FrameworkElement).Do(x => x.DataContext = SplashScreenViewModel);
                    return DXSplashScreen.Show(SplashScreen, UseFadeEffect);
                }
                return null;
            }
            SplashScreenViewModel = new SplashScreenViewModel();
            SplashScreen = (UIElement)CreateAndInitializeView(documentType, SplashScreenViewModel, null, null);
            return DXSplashScreen.Show(SplashScreen, UseFadeEffect);
        }
        void ISplashScreenService.HideSplashScreen() {
            if(!DXSplashScreen.IsActive) return;
            DXSplashScreen.Close();
            SplashScreenViewModel = null;
            SplashScreen = null;
        }
        void ISplashScreenService.SetSplashScreenProgress(double progress, double maxProgress) {
            Dispatcher.BeginInvoke(() => {
                if(SplashScreenType != null && typeof(ISplashScreen).IsAssignableFrom(SplashScreenType)) {
                    DXSplashScreen.Progress(progress);
                    return;
                }
                var splashScreenViewModel = SplashScreenViewModel;
                if(splashScreenViewModel == null) return;
                splashScreenViewModel.IsIndeterminate = false;
                splashScreenViewModel.MaxProgress = maxProgress;
                splashScreenViewModel.Progress = progress;
            });
        }
        void ISplashScreenService.SetSplashScreenState(object state) {
            Dispatcher.BeginInvoke(() => {
                if(SplashScreenType != null && typeof(ISplashScreen).IsAssignableFrom(SplashScreenType))
                    throw new InvalidOperationException(DXSplashScreenExceptions.ServiceException3);
                if(!DXSplashScreen.IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.ServiceException2);
                var splashScreenViewModel = SplashScreenViewModel;
                if(splashScreenViewModel == null) return;
                splashScreenViewModel.State = state;
            });
        }
        UIElement SplashScreen = null;
        ISupportSplashScreen SplashScreenViewModel = null;
    }
}