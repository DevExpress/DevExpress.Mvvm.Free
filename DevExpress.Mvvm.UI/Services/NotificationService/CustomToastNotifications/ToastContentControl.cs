using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI.Native {
    public class ToastContentControl : ContentControl {
        public static readonly DependencyProperty CommandProperty;
        public static readonly DependencyProperty TimerPausedProperty;

        static ToastContentControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToastContentControl), new FrameworkPropertyMetadata(typeof(ToastContentControl)));
            CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ToastContentControl), new PropertyMetadata(null));
            TimerPausedProperty = DependencyProperty.Register("TimerPaused", typeof(bool), typeof(ToastContentControl), new PropertyMetadata(false, (s, e) => ((ToastContentControl)s).TimerPausedChanged()));
        }

        VisualState vsActivated;
        VisualState vsDismissed;
        VisualState vsAppeared;
        VisualState vsTimedOut;
        bool isPressed;
        bool isMouseInBounds;
        bool stopStateChanges;
        bool isTimedOut;

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public bool TimerPaused {
            get { return (bool)GetValue(TimerPausedProperty); }
            set { SetValue(TimerPausedProperty, value); }
        }

        void TimerPausedChanged() {
            if(TimerPaused) {
                Toast.StopTimer();
            } else {
                Toast.ResetTimer();
            }
        }

        public ICommand DismissCommand { get; private set; }
        public ICommand ActivateCommand { get; private set; }
        public ICommand TimeOutCommand { get; private set; }

        void OnVisualStateChanged(Action doAfterChange, VisualState state) {
            if(state == null) {
                doAfterChange();
            } else {
                stopStateChanges = true;
                state.Storyboard.Completed += (s, e) => doAfterChange();
                VisualStateManager.GoToState(this, state.Name, true);
            }
        }

        void OnDismiss() {
            OnVisualStateChanged(() => {
                if(!isTimedOut) {
                    Toast.Dismiss();
                }
            }, vsDismissed);
        }

        void OnActivate() {
            OnVisualStateChanged(() => {
                if(!isTimedOut) {
                    Toast.Activate();
                }
            }, vsActivated);
        }

        void OnTimeOut() {
            isTimedOut = true;
            OnVisualStateChanged(() => Toast.TimeOut(), vsTimedOut);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            vsActivated = (VisualState)GetTemplateChild("Activated");
            vsDismissed = (VisualState)GetTemplateChild("Dismissed");
            vsAppeared = (VisualState)GetTemplateChild("Appeared");
            vsTimedOut = (VisualState)GetTemplateChild("TimedOut");

            var timedOutDuration = new Duration(new TimeSpan(0, 0, 0, 1, 0));
            var appearedDuration = new Duration(new TimeSpan(0, 0, 0, 0, 100));

            (GetTemplateChild("PART_TimedOutAnimation") as DoubleAnimation).Do(x => x.Duration = timedOutDuration);
            (GetTemplateChild("PART_AppearedAnimation1") as DoubleAnimation).Do(x => x.Duration = appearedDuration);
            (GetTemplateChild("PART_AppearedAnimation2") as DoubleAnimation).Do(x => x.Duration = appearedDuration);

            if(vsAppeared != null) {
                vsAppeared.Storyboard.Completed += (s, e) => ChangeVisualState(true);
                VisualStateManager.GoToState(this, vsAppeared.Name, true);
            }
        }

        void ChangeVisualState(bool useTransitions) {
            if(stopStateChanges)
                return;
            if(!IsEnabled) {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            } else if(isPressed && isMouseInBounds) {
                VisualStateManager.GoToState(this, "Pressed", useTransitions);
            } else if(isMouseInBounds && IsMouseOver) {
                VisualStateManager.GoToState(this, "MouseOver", useTransitions);
            } else {
                VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
        }

        public ToastContentControl() {
            MouseDown += (s, e) => {
                if(e.ChangedButton != MouseButton.Left)
                    return;
                CaptureMouse();
                isPressed = true;
                ChangeVisualState(true);
            };
            MouseUp += (s, e) => {
                if(e.ChangedButton != MouseButton.Left)
                    return;
                isPressed = false;
                ReleaseMouseCapture();
                ChangeVisualState(true);
                if(isMouseInBounds && IsMouseOver && Command != null && Command.CanExecute(this)) {
                    Command.Execute(this);
                }
            };
            MouseMove += (s, e) => {
                ChangeVisualState(true);
                isMouseInBounds = GetIsMouseInBounds(e);
            };
            MouseLeave += (s, e) => ChangeVisualState(true);

            DismissCommand = new DelegateCommand(OnDismiss);
            ActivateCommand = new DelegateCommand(OnActivate);
            TimeOutCommand = new DelegateCommand(OnTimeOut);
        }

        bool GetIsMouseInBounds(MouseEventArgs e) {
            Rect bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            return bounds.Contains(e.GetPosition(this));
        }

        internal CustomNotification Toast { get; set; }
    }
}