using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevExpress {
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class DXWindowBase : ContentControl {

        public static DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(DXWindowBase),
            new PropertyMetadata((d, e) => ((DXWindowBase)d).PropertyChangedTitle()));
        public static DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(double), typeof(DXWindowBase),
            new PropertyMetadata((d, e) => ((DXWindowBase)d).PropertyChangedLeft()));
        public static DependencyProperty TopProperty =
            DependencyProperty.Register("Top", typeof(double), typeof(DXWindowBase),
            new PropertyMetadata((d, e) => ((DXWindowBase)d).PropertyChangedTop()));
        public static DependencyProperty CloseOnEscapeProperty =
            DependencyProperty.Register("CloseOnEscape", typeof(bool), typeof(DXWindowBase),
            new PropertyMetadata(true, (d, e) => ((DXWindowBase)d).PropertyChangedCloseOnEscape()));
        public bool IsModal { get; protected set; }
        public bool IsVisible { get { return Popup != null; } }
        public virtual bool KeepPosition { get; set; }

        public object Title {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public double Left {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }
        public double Top {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }
        public bool CloseOnEscape {
            get { return (bool)GetValue(CloseOnEscapeProperty); }
            set { SetValue(CloseOnEscapeProperty, value); }
        }

        public event CancelEventHandler Closing;
        public event EventHandler Closed;
        public event EventHandler Opened;
        public event EventHandler Activated;

        protected internal Popup Popup { get; private set; }
        Control prevFocusedControl;
        static Point previousPosition = new Point();
        CancelEventArgs closingEventArgs;

        public DXWindowBase() {
            this.Visibility = Visibility.Collapsed;
            WindowManager.Default.Register(this);
        }

        public static void HideAllWindows() {
            WindowManager.Default.HideAllWindows();
        }
        public virtual void Show() {
            Show(new Popup());
        }
        public virtual void Show(Popup popup) {
            if(IsVisible) return;
            ApplyTemplate();
            if(Parent is Panel)
                (Parent as Panel).Children.Remove(this);
            this.Visibility = Visibility.Visible;
            prevFocusedControl = FocusManager.GetFocusedElement() as Control;
            if(prevFocusedControl != null) popup.FlowDirection = prevFocusedControl.FlowDirection;
            Popup = popup;
            popup.Child = this;
            popup.HorizontalOffset = Left;
            popup.VerticalOffset = Top;
            popup.IsOpen = true;
            WindowManager.Default.SetWindowActive(this);
            RaiseActivated();
            OnOpened();
        }
        public virtual void ShowDialog() {
            if(IsVisible) return;
            IsModal = true;
            ShowVeil();
            Show();
        }
        public virtual void Close() {
            Hide();
        }
        public virtual void Hide() {
            if(!IsVisible) return;
            if(closingEventArgs == null || !closingEventArgs.Cancel)
                OnClosing();
        }
        public void Activate() {
            if(IsModal || WindowManager.Default.ActiveWindow == null || !WindowManager.Default.ActiveWindow.IsAlive || WindowManager.Default.ActiveWindow.Target == this || Popup == null)
                return;
            WindowManager.Default.SetWindowActive(this);
            Popup.IsOpen = false;
            Popup.IsOpen = true;
            RaiseActivated();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            GotFocus += OnGotFocus;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            Activate();
        }
        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if(!e.Handled && e.Key == Key.Escape && CloseOnEscape)
                OnClosing();
        }
        void OnGotFocus(object sender, RoutedEventArgs e) {
            WindowManager.Default.SetWindowActive(this);
            RaiseActivated();
        }

        protected internal virtual void SetActive(bool isActive) {
        }
        protected virtual void OnOpened() {
            UpdateWindowPos();
            RaiseOpened();
        }
        protected virtual void OnClosed() {
            if(KeepPosition)
                previousPosition = new Point(Left, Top);
            RaiseClosed();
            Left = 0;
            Top = 0;
        }
        protected virtual bool OnClosing() {
            bool isClosing = RaiseClosing();
            if(isClosing == true)
                CloseCore();
            closingEventArgs = new CancelEventArgs();
            return isClosing;
        }
        protected virtual void CloseCore() {
            if(Popup == null)
                return;
            Popup.IsOpen = false;
            Popup.Child = null;
            Popup = null;
            HideVeil();
            OnClosed();
            IsModal = false;
            if(prevFocusedControl != null) {
                prevFocusedControl.Focus();
                prevFocusedControl = null;
            }
        }
        protected virtual void ShowVeil() {
        }
        protected virtual void HideVeil() {

        }

        protected void UpdateWindowPos() {
            if(KeepPosition && previousPosition != new Point()) {
                Left = previousPosition.X;
                Top = previousPosition.Y;
            }
            if(Left == 0 && Top == 0) {
                UpdateLayout();
                Size res = DesiredSize;
                Size appSize = GetHostSize();
                Left = Math.Floor((appSize.Width - res.Width) / 2);
                if(FlowDirection == FlowDirection.RightToLeft) Left = Left + res.Width;
                Top = Math.Floor((appSize.Height - res.Height) / 2);
                if(Top < 0) Top = 0;
                if(Left < 0) Left = 0;
            }
        }
        Size GetHostSize() {
            return new Size(
                Application.Current.Host.Content.ActualWidth / Application.Current.Host.Content.ZoomFactor,
                Application.Current.Host.Content.ActualHeight / Application.Current.Host.Content.ZoomFactor);
        }

        protected virtual void PropertyChangedTitle() { }
        protected virtual void PropertyChangedLeft() {
            if(IsVisible)
                Popup.HorizontalOffset = Left;
        }
        protected virtual void PropertyChangedTop() {
            if(IsVisible)
                Popup.VerticalOffset = Top;
        }
        protected virtual void PropertyChangedCloseOnEscape() { }

        protected virtual bool RaiseClosing() {
            closingEventArgs = new CancelEventArgs();
            OnClosing(closingEventArgs);
            return !closingEventArgs.Cancel;
        }
        protected virtual void OnClosing(CancelEventArgs e) {
            if(Closing != null)
                Closing(this, closingEventArgs);
        }
        protected virtual void RaiseClosed() {
            if(Closed != null)
                Closed(this, EventArgs.Empty);
        }
        protected virtual void RaiseActivated() {
            if(Activated != null)
                Activated(this, EventArgs.Empty);
        }
        protected virtual void RaiseOpened() {
            if(Opened != null)
                Opened(this, EventArgs.Empty);
        }
    }
}