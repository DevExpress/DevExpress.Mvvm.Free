using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace DevExpress.Mvvm.UI {
    [TargetTypeAttribute(typeof(Window)), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class WindowFadeAnimationBehavior : Behavior<FrameworkElement> {
        public static readonly DependencyProperty EnableAnimationProperty =
            DependencyProperty.RegisterAttached("EnableAnimation", typeof(bool), typeof(WindowFadeAnimationBehavior),
            new PropertyMetadata(false, OnEnableAnimationChanged));
        public static bool GetEnableAnimation(Window obj) {
            return (bool)obj.GetValue(EnableAnimationProperty);
        }
        public static void SetEnableAnimation(Window obj, bool value) {
            obj.SetValue(EnableAnimationProperty, value);
        }
        static void OnEnableAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            Window w = d as Window;
            BehaviorCollection col = Interaction.GetBehaviors(w);
            WindowFadeAnimationBehavior b = (WindowFadeAnimationBehavior)col.FirstOrDefault(x => x is WindowFadeAnimationBehavior);
            col.Remove(b);
            if((bool)e.NewValue)
                col.Add(new WindowFadeAnimationBehavior());
        }

        public static readonly DependencyProperty WindowProperty =
            DependencyProperty.Register("Window", typeof(Window), typeof(WindowFadeAnimationBehavior),
            new PropertyMetadata(null, (d, e) => ((WindowFadeAnimationBehavior)d).OnWindowChanged((Window)e.OldValue)));
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(TimeSpan), typeof(WindowFadeAnimationBehavior), new PropertyMetadata(TimeSpan.FromSeconds(0.2)));
        public static readonly DependencyProperty EnableShowAnimationProperty =
            DependencyProperty.Register("EnableShowAnimation", typeof(bool), typeof(WindowFadeAnimationBehavior), new PropertyMetadata(true));
        public static readonly DependencyProperty EnableCloseAnimationProperty =
            DependencyProperty.Register("EnableCloseAnimation", typeof(bool), typeof(WindowFadeAnimationBehavior), new PropertyMetadata(true));
        public Window Window {
            get { return (Window)GetValue(WindowProperty); }
            set { SetValue(WindowProperty, value); }
        }
        public TimeSpan AnimationDuration {
            get { return (TimeSpan)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }
        public bool EnableShowAnimation {
            get { return (bool)GetValue(EnableShowAnimationProperty); }
            set { SetValue(EnableShowAnimationProperty, value); }
        }
        public bool EnableCloseAnimation {
            get { return (bool)GetValue(EnableCloseAnimationProperty); }
            set { SetValue(EnableCloseAnimationProperty, value); }
        }

        protected Window ActualWindow { get { return Window ?? AssociatedObject as Window; } }
        protected override void OnAttached() {
            base.OnAttached();
            Initialize();
        }
        protected override void OnDetaching() {
            Uninitialize(ActualWindow);
            base.OnDetaching();
        }
        void OnWindowChanged(Window oldValue) {
            Uninitialize(AssociatedObject as Window);
            Uninitialize(oldValue);
            Initialize();
        }
        void Initialize() {
            Uninitialize(ActualWindow);
            if(ActualWindow == null) return;
            ActualWindow.Closing += OnWindowClosing;
            ActualWindow.Loaded += OnWindowLoaded;
        }
        void Uninitialize(Window window) {
            if(window == null) return;
            window.Closing -= OnWindowClosing;
            window.Loaded -= OnWindowLoaded;
        }

        void OnWindowLoaded(object sender, RoutedEventArgs e) {
            Window w = (Window)sender;
            w.Loaded -= OnWindowLoaded;
            if(!EnableShowAnimation) return;
            Storyboard st = CreateStoryboard(w, 0, 1, AnimationDuration);
            st.Begin();
        }
        void OnWindowClosing(object sender, CancelEventArgs e) {
            if(e.Cancel)
                return;
            Window w = (Window)sender;
            w.Closing -= OnWindowClosing;
            if(!EnableCloseAnimation) return;
            Storyboard st = CreateStoryboard(w, 1, 0, AnimationDuration);
            st.Completed += (dd, ee) => {
                w.Close();
            };
            e.Cancel = true;
            st.Begin();
        }

        Storyboard CreateStoryboard(Window w, double from, double to, TimeSpan duration) {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation() {
                From = from,
                To = to,
                Duration = duration
            };
            Storyboard.SetTarget(animation, w);
            Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.OpacityProperty));
            animation.Freeze();
            storyboard.Children.Add(animation);
            return storyboard;
        }
    }
}