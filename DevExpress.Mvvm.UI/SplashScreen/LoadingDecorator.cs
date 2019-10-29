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
        const string LOADING_CHILD_SET_TWICE_EXCEPTION = "The LoadingChild and LoadingChildTemplate properties cannot be used simultaneously.";
        const string LOADING_CHILD_WRONG_TEMPLATE_EXCEPTION = "The LoadingChild template must contain FrameworkElement as the visual root.";

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
        public static readonly DependencyProperty SplashScreenWindowStyleProperty;
        public static readonly DependencyProperty LoadingChildTemplateProperty;

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
            SplashScreenWindowStyleProperty = DependencyProperty.Register("SplashScreenWindowStyle", typeof(Style), typeof(LoadingDecorator),
                new PropertyMetadata(null, (d, e) => ((LoadingDecorator)d).OnIsSplashScreenWindowStyleChanged()));
            LoadingChildTemplateProperty = DependencyProperty.Register("LoadingChildTemplate", typeof(DataTemplate), typeof(LoadingDecorator),
                new PropertyMetadata(null, (d, e) => ((LoadingDecorator)d).OnLoadingChildTemplateChanged()));
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
        public Style SplashScreenWindowStyle {
            get { return (Style)GetValue(SplashScreenWindowStyleProperty); }
            set { SetValue(SplashScreenWindowStyleProperty, value); }
        }
        public DataTemplate LoadingChildTemplate {
            get { return (DataTemplate)GetValue(LoadingChildTemplateProperty); }
            set { SetValue(LoadingChildTemplateProperty, value); }
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

        public FrameworkElement LoadingChild {
            get { return loadingChild; }
            set {
                if(loadingChild == value)
                    return;

                loadingChild = value;
                ValidateLoadingChild();
                OnLoadingChildChanged();
            }
        }
        protected bool IsActive { get { return splashContainer != null && splashContainer.IsActive; } }
        DXSplashScreen.SplashScreenContainer splashContainer = null;
        DXSplashScreen.SplashScreenContainer SplashContainer {
            get {
                if(splashContainer == null)
                    splashContainer = new DXSplashScreen.SplashScreenContainer();
                return splashContainer;
            }
        }
        FrameworkElement loadingChild = null;

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
        void OnIsSplashScreenWindowStyleChanged() {
            SplashScreenWindowStyle.Do(x => x.Seal());
        }
        void OnSplashScreenTemplateChanged() {
            SplashScreenTemplate.Do(x => x.Seal());
        }
        void OnLoaded(object sender, RoutedEventArgs e) {
            if(!IsLoaded)
                return;

            Loaded -= OnLoaded;
            if(ViewModelBase.IsInDesignMode)
                return;
            OnLoadingChildChanged();
        }
        void OnUnloaded(object sender, RoutedEventArgs e) {
            CloseSplashScreenOnLoading();
        }
        void ValidateLoadingChild() {
            if(LoadingChild != null && LoadingChildTemplate != null && !ViewModelBase.IsInDesignMode)
                throw new InvalidOperationException(LOADING_CHILD_SET_TWICE_EXCEPTION);
        }
        void OnLoadingChildTemplateChanged() {
            ValidateLoadingChild();
            LoadingChildTemplate.Do(x => x.Seal());
            OnLoadingChildChanged();
        }
        void OnLoadingChildChanged() {
            if(ViewModelBase.IsInDesignMode) {
                LoadChildInDesignTime();
                return;
            }
            Child = null;
            if((LoadingChild == null && LoadingChildTemplate == null) || !IsLoaded)
                return;
            if(IsSplashScreenShown == null && IsVisible)
                ShowSplashScreen();

            SplashScreenHelper.InvokeAsync(this, LoadChild);
        }
        void LoadChild() {
            if(LoadingChild != null) {
                LoadingChild.Loaded += OnLoadingChildLoaded;
                Child = LoadingChild;
            } else if (LoadingChildTemplate != null) {
                var feChild = LoadingChildTemplate.LoadContent() as FrameworkElement;
                if(feChild != null) {
                    feChild.Loaded += OnLoadingChildLoaded;
                    Child = feChild;
                } else
                    throw new InvalidOperationException(LOADING_CHILD_WRONG_TEMPLATE_EXCEPTION);
            }
        }
        void LoadChildInDesignTime() {
            if(LoadingChild != null && LoadingChildTemplate != null)
                Child = CreateFallbackView(LOADING_CHILD_SET_TWICE_EXCEPTION);
            else if(LoadingChild != null)
                Child = LoadingChild;
            else if(LoadingChildTemplate != null) {
                var feChild = LoadingChildTemplate.LoadContent() as FrameworkElement;
                if(feChild != null)
                    Child = feChild;
                else
                    Child = CreateFallbackView(LOADING_CHILD_WRONG_TEMPLATE_EXCEPTION);
            }
        }
        UIElement CreateFallbackView(string errorMessage) {
            return new TextBlock() {
                Text = errorMessage,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 25,
                Foreground = new SolidColorBrush(Colors.Red),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
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

        protected virtual void ShowSplashScreen() {
            if(!UseSplashScreen || IsActive)
                return;

            SplashContainer.Show(CreateSplashScreenWindow, CreateSplashScreen, GetSplashScreenCreatorParams(), new object[] { SplashScreenTemplate, SplashScreenDataContext }, null);
        }
        protected virtual void CloseSplashScreen() {
            if(!IsActive)
                return;
            SplashContainer.Close();
        }
        void CloseSplashScreenOnLoading() {
            if(IsSplashScreenShown != null || !IsActive)
                return;

            SplashScreenHelper.InvokeAsync(this, CloseSplashScreen, DispatcherPriority.Render);
        }
        object[] GetSplashScreenCreatorParams() {
            return new object[] { UseFadeEffect, new WindowArrangerContainer(this, SplashScreenLocation), OwnerLock,
                FadeInDuration, FadeOutDuration, FlowDirection, SplashScreenWindowStyle };
        }
        internal virtual SplashScreenArrangeMode ArrangeMode() { return SplashScreenArrangeMode.Default; }
        static Window CreateSplashScreenWindow(object parameter) {
            object[] parameters = (object[])parameter;
            bool useFadeEffect = (bool)parameters[0];
            WindowArrangerContainer owner = (WindowArrangerContainer)parameters[1];
            SplashScreenLock lockMode = (SplashScreenLock)parameters[2];
            IList<TimeSpan> durations = SplashScreenHelper.FindParameters<TimeSpan>(parameter);
            FlowDirection flowDirection = SplashScreenHelper.FindParameter<FlowDirection>(parameter);
            Style windowStyle = SplashScreenHelper.FindParameter<Style>(parameter);
            var window = new LoadingDecoratorWindowFree(owner, lockMode);
            if(windowStyle != null)
                window.Style = windowStyle;
            else
                window.ApplyDefaultSettings();
            window.SetCurrentValue(FlowDirectionProperty, flowDirection);
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

        internal class LoadingDecoratorWindowFree : DXSplashScreen.SplashScreenWindow {
            protected ContainerLocker ParentLocker { get; private set; }
            public LoadingDecoratorWindowFree(WindowArrangerContainer parentContainer, SplashScreenLock lockMode) {
                WindowStartupLocation = WindowStartupLocation.Manual;
                SetWindowStartupPosition(parentContainer.ControlStartupPosition.IsEmpty ? parentContainer.WindowStartupPosition : parentContainer.ControlStartupPosition);
                CreateLocker(parentContainer, lockMode);
                ClearValue(ShowActivatedProperty);
            }

            protected override void OnClosed(EventArgs e) {
                ReleaseLocker();
                base.OnClosed(e);
            }

            internal void ApplyDefaultSettings() {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                AllowsTransparency = true;
                Topmost = false;
                Focusable = false;
                ShowInTaskbar = false;
                ShowActivated = false;
                Background = new SolidColorBrush(Colors.Transparent);
                SizeToContent = SizeToContent.WidthAndHeight;
            }
            void SetWindowStartupPosition(Rect bounds) {
                if(bounds.IsEmpty)
                    return;

                Left = bounds.Left;
                Top = bounds.Top;
                Width = bounds.Width;
                Height = bounds.Height;
            }
            void ReleaseLocker() {
                ParentLocker.Release(IsActiveOnClosing);
                ParentLocker = null;
            }
            void CreateLocker(WindowContainer parentContainer, SplashScreenLock lockMode) {
                ParentLocker = new ContainerLocker(parentContainer, lockMode);
            }
        }
    }
}