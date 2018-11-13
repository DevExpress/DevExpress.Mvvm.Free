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
#if !FREE
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Utils.Themes;
using DXDecorator = DevExpress.Xpf.Core.HandleDecorator.Decorator;
using WindowDecorator = DevExpress.Xpf.Core.HandleDecorator.FormHandleDecorator;

namespace DevExpress.Xpf.Core {
#else

namespace DevExpress.Mvvm.UI {
#endif

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
#if !FREE
        public static readonly DependencyProperty BorderEffectProperty;
        public static readonly DependencyProperty BorderEffectColorProperty;
#endif

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
#if !FREE
            BorderEffectProperty = DependencyProperty.Register("BorderEffect", typeof(BorderEffect), typeof(LoadingDecorator),
                new PropertyMetadata(BorderEffect.None));
            BorderEffectColorProperty = DependencyProperty.Register("BorderEffectColor", typeof(SolidColorBrush), typeof(LoadingDecorator),
                new PropertyMetadata(null));
#endif
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
#if !FREE
        public BorderEffect BorderEffect {
            get { return (BorderEffect)GetValue(BorderEffectProperty); }
            set { SetValue(BorderEffectProperty, value); }
        }
        public SolidColorBrush BorderEffectColor {
            get { return (SolidColorBrush)GetValue(BorderEffectColorProperty); }
            set { SetValue(BorderEffectColorProperty, value); }
        }
#endif
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
            //T384922 - Loaded event is raised twice
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

            //T238780: don't show, if LoadingDecorator is hidden
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
#if !DEBUGTEST
            CloseSplashScreenOnLoading();
        }
#else
            if(!Test_SkipAutomaticCloseOnChildLoaded)
                CloseSplashScreenOnLoading();
        }

        internal bool Test_SkipAutomaticCloseOnChildLoaded { get; set; }
        internal bool Test_IsSplashContainerEmpty { get { return splashContainer == null; } }
        internal DXSplashScreen.SplashScreenContainer Test_SplashContainer { get { return SplashContainer; } }
#endif
        void SplashScreenDataContextChanged() {
            if(SplashScreenDataContext is DependencyObject)
                throw new InvalidOperationException(Exception1);
        }

        protected virtual void ShowSplashScreen() {
            if(!UseSplashScreen || IsActive)
                return;

            SplashContainer.Show(CreateSplashScreenWindow, CreateSplashScreen, GetSplashScreenCreatorParams(), new object[] { SplashScreenTemplate, SplashScreenDataContext });
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
#if !FREE
            string themeName = ThemeHelper.GetWindowThemeName(this);
            if(string.IsNullOrEmpty(themeName))
                themeName = ApplicationThemeHelper.ApplicationThemeName;

            Color? borderColor = BorderEffectColor.Return(x => (Color?)x.Color, null);
            return new object[] { UseFadeEffect, new WindowArrangerContainer(this, SplashScreenLocation) { ArrangeMode = ArrangeMode() }, OwnerLock, themeName,
                BorderEffect, borderColor, FadeInDuration, FadeOutDuration, FlowDirection, SplashScreenWindowStyle };
#else
            return new object[] { UseFadeEffect, new WindowArrangerContainer(this, SplashScreenLocation), OwnerLock, 
                FadeInDuration, FadeOutDuration, FlowDirection, SplashScreenWindowStyle };
#endif
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
#if !FREE
            string themeName = (string)parameters[3];
            BorderEffect borderEffect = (BorderEffect)parameters[4];
            Color? borderColor = (Color?)parameters[5];
            var brush = borderColor.HasValue ? new SolidColorBrush(borderColor.Value) : null;

            var window = new LoadingDecoratorWindow(owner, lockMode, themeName, borderEffect, brush);
#else
            var window = new LoadingDecoratorWindowFree(owner, lockMode);
#endif
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
#if !FREE
        internal class LoadingDecoratorWindow : LoadingDecoratorWindowFree {
#if DEBUGTEST
            internal ContainerLocker Test_GetWindowLocker() { return ParentLocker; }
            internal DXDecorator Test_GetBorderDecorator() { return decorator; }
#endif
            DXDecorator decorator;

            public LoadingDecoratorWindow(WindowArrangerContainer parentContainer, SplashScreenLock lockMode,
                    string themeName, BorderEffect borderEffect, SolidColorBrush borderBrush) : base(parentContainer, lockMode) {
                CreateBorderDecorator(borderEffect, borderBrush, themeName);
                ThemeManager.SetThemeName(this, themeName);
            }

            protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
                if(!e.Cancel)
                    ReleaseBorderDecorator();
                base.OnClosing(e);
            }
            protected override void OnClosed(EventArgs e) {
                ReleaseBorderDecorator();
                base.OnClosed(e);
            }

            void ReleaseBorderDecorator() {
                if(decorator != null) {
                    decorator.Hide();
                    decorator.Dispose();
                    decorator = null;
                }
            }
            void CreateBorderDecorator(BorderEffect borderEffect, SolidColorBrush brush, string themeName) {
                if(borderEffect == BorderEffect.None)
                    return;

                if(!string.IsNullOrEmpty(themeName) && themeName.Contains(";Touch"))
                    themeName = themeName.Substring(0, themeName.IndexOf(";Touch"));
                Thickness? offset = FindDxWindowResource<Thickness?>(DXWindowThemeKey.BorderEffectOffset, themeName);
                Thickness? leftMargin = FindDxWindowResource<Thickness?>(DXWindowThemeKey.BorderEffectLeftMargins, themeName);
                Thickness? rightMargin = FindDxWindowResource<Thickness?>(DXWindowThemeKey.BorderEffectRightMargins, themeName);
                Thickness? topMargin = FindDxWindowResource<Thickness?>(DXWindowThemeKey.BorderEffectTopMargins, themeName);
                Thickness? bottomMargin = FindDxWindowResource<Thickness?>(DXWindowThemeKey.BorderEffectBottomMargins, themeName);
                brush = brush ?? FindDxWindowResource<SolidColorBrush>(DXWindowThemeKey.BorderEffectActiveColor, themeName);

                decorator = new WindowDecorator(brush, brush, offset.HasValue ? offset.Value : new Thickness(),
                    new HandleDecorator.StructDecoratorMargins() {
                        LeftMargins = leftMargin.HasValue ? leftMargin.Value : new Thickness(),
                        RightMargins = rightMargin.HasValue ? rightMargin.Value : new Thickness(),
                        TopMargins = topMargin.HasValue ? topMargin.Value : new Thickness(),
                        BottomMargins = bottomMargin.HasValue ? bottomMargin.Value : new Thickness()
                    }, true);
                decorator.Control = this;
            }
            T FindDxWindowResource<T>(DXWindowThemeKey resourceKey, string themeName) {
                DXWindowThemeKeyExtension ex = new DXWindowThemeKeyExtension() {
                    ThemeName = themeName,
                    ResourceKey = resourceKey
                };
                return (T)TryFindResource(ex);
            }
        }
#endif
    }
}