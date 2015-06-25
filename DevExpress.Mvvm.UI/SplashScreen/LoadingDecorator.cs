using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.UI {

    [ContentProperty("LoadingChild")]
    public class LoadingDecorator : Decorator {
        const string Exception1 = "LoadingDecorator shows its SplashScreen in a separate thread, so it is impossible to put a DependencyObject into the SplashScreenDataContext.";

        #region Static
        public static readonly DependencyProperty UseFadeEffectProperty;
        public static readonly DependencyProperty FadeInDurationProperty;
        public static readonly DependencyProperty FadeOutDurationProperty;
        public static readonly DependencyProperty UseSplashScreenProperty;
        public static readonly DependencyProperty SplashScreenTemplateProperty;
        public static readonly DependencyProperty SplashScreenDataContextProperty;
        public static readonly DependencyProperty IsSplashScreenShownProperty;
        public static readonly DependencyProperty OwnerLockProperty;
        public static readonly DependencyProperty SplashScreenLocationProperty;

        static LoadingDecorator() {
            UseFadeEffectProperty = DependencyProperty.Register("UseFadeEffect", typeof(bool), typeof(LoadingDecorator),
                new PropertyMetadata(true));
            FadeInDurationProperty = DependencyProperty.Register("FadeInDuration", typeof(TimeSpan), typeof(LoadingDecorator),
                new PropertyMetadata(TimeSpan.FromSeconds(0.2)));
            FadeOutDurationProperty = DependencyProperty.Register("FadeOutDuration", typeof(TimeSpan), typeof(LoadingDecorator),
                new PropertyMetadata(TimeSpan.FromSeconds(0.2)));
            UseSplashScreenProperty = DependencyProperty.Register("UseSplashScreen", typeof(bool), typeof(LoadingDecorator),
                new PropertyMetadata(true));
            SplashScreenTemplateProperty = DependencyProperty.Register("SplashScreenTemplate", typeof(DataTemplate), typeof(LoadingDecorator),
                new PropertyMetadata(null, (d, e) => ((LoadingDecorator)d).OnSplashScreenTemplateChanged()));
            SplashScreenDataContextProperty = DependencyProperty.Register("SplashScreenDataContext", typeof(object), typeof(LoadingDecorator),
                new PropertyMetadata(null));
            IsSplashScreenShownProperty = DependencyProperty.Register("IsSplashScreenShown", typeof(bool?), typeof(LoadingDecorator),
                new PropertyMetadata(null, (d, e) => ((LoadingDecorator)d).OnIsSplashScreenShownChanged()));
            OwnerLockProperty = DependencyProperty.Register("OwnerLock", typeof(SplashScreenLock), typeof(LoadingDecorator),
                new PropertyMetadata(SplashScreenLock.Full));
            SplashScreenLocationProperty = DependencyProperty.Register("SplashScreenLocation", typeof(SplashScreenLocation), typeof(LoadingDecorator),
                new PropertyMetadata(SplashScreenLocation.CenterContainer));
        }
        #endregion

        #region Dependency Properties
        public bool UseFadeEffect {
            get { return (bool)GetValue(UseFadeEffectProperty); }
            set { SetValue(UseFadeEffectProperty, value); }
        }
        public TimeSpan FadeInDuration {
            get { return (TimeSpan)GetValue(FadeInDurationProperty); }
            set { SetValue(FadeInDurationProperty, value); }
        }
        public TimeSpan FadeOutDuration {
            get { return (TimeSpan)GetValue(FadeOutDurationProperty); }
            set { SetValue(FadeOutDurationProperty, value); }
        }
        public bool UseSplashScreen {
            get { return (bool)GetValue(UseSplashScreenProperty); }
            set { SetValue(UseSplashScreenProperty, value); }
        }
        public DataTemplate SplashScreenTemplate {
            get { return (DataTemplate)GetValue(SplashScreenTemplateProperty); }
            set { SetValue(SplashScreenTemplateProperty, value); }
        }
        public object SplashScreenDataContext {
            get { return (object)GetValue(SplashScreenDataContextProperty); }
            set { SetValue(SplashScreenDataContextProperty, value); }
        }
        public bool? IsSplashScreenShown {
            get { return (bool?)GetValue(IsSplashScreenShownProperty); }
            set { SetValue(IsSplashScreenShownProperty, value); }
        }
        public SplashScreenLock OwnerLock {
            get { return (SplashScreenLock)GetValue(OwnerLockProperty); }
            set { SetValue(OwnerLockProperty, value); }
        }
        public SplashScreenLocation SplashScreenLocation {
            get { return (SplashScreenLocation)GetValue(SplashScreenLocationProperty); }
            set { SetValue(SplashScreenLocationProperty, value); }
        }
        #endregion

        #region Props
        DXSplashScreen.SplashScreenContainer splashContainer = null;
        DXSplashScreen.SplashScreenContainer SplashContainer {
            get {
                if(splashContainer == null)
                    splashContainer = new DXSplashScreen.SplashScreenContainer();
                return splashContainer;
            }
        }
        FrameworkElement loadingChild = null;
        public FrameworkElement LoadingChild {
            get { return loadingChild; }
            set {
                if(loadingChild == value)
                    return;
                loadingChild = value;
                if(ViewModelBase.IsInDesignMode) {
                    Child = loadingChild;
                    return;
                }
                OnLoadingChildChanged();
            }
        }
        #endregion

        public LoadingDecorator() {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        void OnIsSplashScreenShownChanged() {
            if(IsSplashScreenShown == true) {
                if(IsLoaded)
                    ShowSplashScreen();
                else
                    SplashScreenHelper.InvokeAsync(this, () => { if(IsSplashScreenShown == true) ShowSplashScreen(); }, DispatcherPriority.Render);
            }
            if(IsSplashScreenShown == false)
                CloseSplashScreen();
        }
        void OnSplashScreenTemplateChanged() {
            if(SplashScreenTemplate != null)
                SplashScreenTemplate.Seal();
        }
        void OnLoaded(object sender, RoutedEventArgs e) {
            Loaded -= OnLoaded;
            if(ViewModelBase.IsInDesignMode)
                return;
            OnLoadingChildChanged();
        }
        void OnUnloaded(object sender, RoutedEventArgs e) {
            CloseSplashScreenOnLoading();
        }
        void OnLoadingChildChanged() {
            Child = null;
            if(LoadingChild == null || !IsLoaded)
                return;
            LoadingChild.Loaded += OnLoadingChildLoaded;
            if(IsSplashScreenShown == null && IsVisible)
                ShowSplashScreen();

            SplashScreenHelper.InvokeAsync(this, () => Child = LoadingChild);
        }
        void OnLoadingChildLoaded(object sender, RoutedEventArgs e) {
            FrameworkElement child = (FrameworkElement)sender;
            child.Loaded -= OnLoadingChildLoaded;
            CloseSplashScreenOnLoading();
        }
        void SplashScreenDataContextChanged() {
            if(SplashScreenDataContext is DependencyObject)
                throw new InvalidOperationException(Exception1);
        }

        void ShowSplashScreen() {
            if(!UseSplashScreen || SplashContainer.IsActive)
                return;

            SplashContainer.Show(CreateSplashScreenWindow, CreateSplashScreen, GetSplashScreenCreatorParams(), new object[] { SplashScreenTemplate, SplashScreenDataContext });
        }
        void CloseSplashScreen() {
            if(!SplashContainer.IsActive)
                return;
            SplashContainer.Close();
        }
        void CloseSplashScreenOnLoading() {
            if(IsSplashScreenShown != null || !SplashContainer.IsActive)
                return;

            SplashScreenHelper.InvokeAsync(this, CloseSplashScreen, DispatcherPriority.Render);
        }
        object[] GetSplashScreenCreatorParams() {
            return new object[] { UseFadeEffect, new WindowArrangerContainer(this, SplashScreenLocation), OwnerLock, FadeInDuration, FadeOutDuration };
        }
        static Window CreateSplashScreenWindow(object parameter) {
            object[] parameters = (object[])parameter;
            bool useFadeEffect = (bool)parameters[0];
            WindowArrangerContainer owner = (WindowArrangerContainer)parameters[1];
            SplashScreenLock lockMode = (SplashScreenLock)parameters[2];
            IList<TimeSpan> durations = SplashScreenHelper.FindParameters<TimeSpan>(parameter);
            var window = new LoadingDecoratorWindow(owner, lockMode);
            if(useFadeEffect && durations.Any(x => x.TotalMilliseconds > 0))
                Interaction.GetBehaviors(window).Add(new WindowFadeAnimationBehavior() { FadeInDuration = durations[0], FadeOutDuration = durations[1] });

            return window;
        }
        static object CreateSplashScreen(object parameter) {
            object[] parameters = (object[])parameter;
            DataTemplate splashScreenTemplate = (DataTemplate)parameters[0];
            object splashScreenDataContext = parameters[1];
            if(splashScreenTemplate == null) {
                return new WaitIndicator() {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    DeferedVisibility = true,
                    ShowShadow = false,
                    Margin = new Thickness(),
                    ContentPadding = new Thickness()
                };
            } else {
                var res = splashScreenTemplate.LoadContent();
                (res as FrameworkElement).Do(x => x.DataContext = splashScreenDataContext);
                return res;
            }
        }

        class LoadingDecoratorWindow : DXSplashScreen.SplashScreenWindow {
            WindowLocker locker;
            public LoadingDecoratorWindow(WindowArrangerContainer parentContainer, SplashScreenLock lockMode) {
                WindowStyle = WindowStyle.None;
                AllowsTransparency = true;
                Topmost = false;
                Focusable = false;
                ShowInTaskbar = false;
                Background = new SolidColorBrush(Colors.Transparent);
                SizeToContent = SizeToContent.WidthAndHeight;
                WindowStartupLocation = WindowStartupLocation.Manual;
                SetWindowStartupPosition(parentContainer.ControlStartupPosition.IsEmpty ? parentContainer.WindowStartupPosition : parentContainer.ControlStartupPosition);
                CreateLocker(parentContainer, lockMode);
            }

            protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
                if(!e.Cancel)
                    ReleaseBorderDecorator();
                base.OnClosing(e);
            }
            protected override void OnClosed(EventArgs e) {
                ReleaseBorderDecorator();
                ReleaseLocker();
                base.OnClosed(e);
            }

            void SetWindowStartupPosition(Rect bounds) {
                if(bounds.IsEmpty)
                    return;

                Left = bounds.Left;
                Top = bounds.Top;
                Width = bounds.Width;
                Height = bounds.Height;
            }
            #region Helpers
            void ReleaseLocker() {
                locker.Release(IsActiveOnClosing);
                locker = null;
            }
            void CreateLocker(WindowContainer parentContainer, SplashScreenLock lockMode) {
                locker = new WindowLocker(parentContainer, lockMode);
            }
            void ReleaseBorderDecorator() {
            }
            #endregion
        }
    }
}