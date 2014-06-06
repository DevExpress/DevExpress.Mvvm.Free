using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

#if !SILVERLIGHT
using WindowBase = System.Windows.Window;
using System.Windows.Input;
#else
using WindowBase = DevExpress.Xpf.Core.DXWindowBase;
using System.Reflection;
#endif
namespace DevExpress.Mvvm.UI {
    public enum WindowShowMode { Dialog, Default };
    public class WindowService : ViewServiceBase, IWindowService {
#if !SILVERLIGHT
        static Type DefaultWindowType = typeof(Window);
#endif
#if !SILVERLIGHT
        const string WindowTypeException = "WindowType show be derived from the Window type";
#else
        const string WindowTypeException = "WindowType show be derived from the DXWindowBase type";
#endif

#if !SILVERLIGHT
        public static readonly DependencyProperty WindowStartupLocationProperty =
            DependencyProperty.Register("WindowStartupLocation", typeof(WindowStartupLocation), typeof(WindowService),
            new PropertyMetadata(WindowStartupLocation.CenterScreen));
        public static readonly DependencyProperty AllowSetWindowOwnerProperty =
            DependencyProperty.Register("AllowSetWindowOwner", typeof(bool), typeof(WindowService), new PropertyMetadata(true));
#endif
        public static readonly DependencyProperty WindowStyleProperty =
            DependencyProperty.Register("WindowStyle", typeof(Style), typeof(WindowService), new PropertyMetadata(null));
        public static readonly DependencyProperty WindowTypeProperty =
            DependencyProperty.Register("WindowType", typeof(Type), typeof(WindowService),
            new PropertyMetadata(DefaultWindowType, (d, e) => ((WindowService)d).OnWindowTypeChanged()));
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(WindowService),
            new PropertyMetadata(string.Empty, (d, e) => ((WindowService)d).OnTitleChanged()));
        public static readonly DependencyProperty WindowShowModeProperty =
            DependencyProperty.Register("WindowShowMode", typeof(WindowShowMode), typeof(WindowService),
            new PropertyMetadata(WindowShowMode.Default));

#if !SILVERLIGHT
        public WindowStartupLocation WindowStartupLocation {
            get { return (WindowStartupLocation)GetValue(WindowStartupLocationProperty); }
            set { SetValue(WindowStartupLocationProperty, value); }
        }
        public bool AllowSetWindowOwner {
            get { return (bool)GetValue(AllowSetWindowOwnerProperty); }
            set { SetValue(AllowSetWindowOwnerProperty, value); }
        }
#endif
        public Type WindowType {
            get { return (Type)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }
        public Style WindowStyle {
            get { return (Style)GetValue(WindowStyleProperty); }
            set { SetValue(WindowStyleProperty, value); }
        }
        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public WindowShowMode WindowShowMode {
            get { return (WindowShowMode)GetValue(WindowShowModeProperty); }
            set { SetValue(WindowShowModeProperty, value); }
        }
        protected virtual WindowBase CreateWindow(object view) {
            WindowBase window = (WindowBase)Activator.CreateInstance(WindowType ?? DefaultWindowType);
            UpdateThemeName(window);
            window.Content = view;
            window.Style = WindowStyle;
#if !SILVERLIGHT
            window.WindowStartupLocation = this.WindowStartupLocation;
            if(AllowSetWindowOwner && AssociatedObject != null)
                window.Owner = Window.GetWindow(AssociatedObject);
#endif
            return window;
        }
        void OnWindowTypeChanged() {
            if(WindowType == null) return;
            if(!typeof(Window).IsAssignableFrom(WindowType))
                throw new ArgumentException(WindowTypeException);
        }
        void OnTitleChanged() {
            if(window != null)
                window.Title = Title ?? string.Empty;
        }
        void SetTitleBinding() {
            if(string.IsNullOrEmpty(Title))
                DocumentUIServiceBase.SetTitleBinding(window.Content, WindowBase.TitleProperty, window, true);
        }
        void OnWindowClosing(object sender, CancelEventArgs e) {
            IDocumentViewModel documentViewModel = ViewHelper.GetViewModelFromView(window.Content) as IDocumentViewModel;
            if(documentViewModel != null)
                e.Cancel = !documentViewModel.Close();
        }
        void OnWindowClosed(object sender, EventArgs e) {
            window.Closing -= OnWindowClosing;
            window.Closed -= OnWindowClosed;
            window = null;
        }

        WindowBase window;
        bool IWindowService.IsWindowAlive {
            get { return window != null; }
        }
        void IWindowService.Show(string documentType, object viewModel, object parameter, object parentViewModel) {
            if(window != null) {
                window.Show();
                return;
            }
            object view = CreateAndInitializeView(documentType, viewModel, parameter, parentViewModel);
            window = CreateWindow(view);
            window.Title = Title ?? string.Empty;
            SetTitleBinding();
            window.Closing += OnWindowClosing;
            window.Closed += OnWindowClosed;
            if(WindowShowMode == WindowShowMode.Dialog)
                window.ShowDialog();
            else window.Show();
        }

        void IWindowService.Activate() {
            if(window != null) {
                window.Activate();
            }
        }
        void IWindowService.Restore() {
            if(window != null) {
#if !SILVERLIGHT
                window.Show();
#else
                window.Visibility = Visibility.Visible;

#endif
            }
        }
        void IWindowService.Hide() {
            if(window != null) {
#if !SILVERLIGHT
                window.Hide();
#else
                window.Visibility = Visibility.Collapsed;
#endif
            }
        }
        void IWindowService.Close() {
            if(window != null)
                window.Close();
        }
#if !SILVERLIGHT
        void IWindowService.SetWindowState(WindowState state) {
            if(window != null)
                window.WindowState = state;
        }
#endif
    }
}