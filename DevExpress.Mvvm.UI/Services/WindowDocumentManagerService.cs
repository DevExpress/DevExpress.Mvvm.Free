using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Native;
#if SILVERLIGHT
using WindowBase = DevExpress.Xpf.Core.DXWindowBase;
#else
using WindowBase = System.Windows.Window;
#endif

namespace DevExpress.Mvvm.UI {
    [TargetTypeAttribute(typeof(UserControl))]
    [TargetTypeAttribute(typeof(Window))]
    public class WindowedDocumentUIService : DocumentUIServiceBase, IDocumentManagerService, IDocumentOwner {
        public class WindowDocument : IDocument {
            readonly object documentContentView;
            bool destroyOnClose = true;
            readonly WindowedDocumentUIService owner;

            public WindowDocument(WindowedDocumentUIService owner, IWindowSurrogate window, object documentContentView) {
                this.owner = owner;
                Window = window;
                this.documentContentView = documentContentView;
                Window.RealWindow.Closing += window_Closing;
                Window.RealWindow.Closed += window_Closed;
            }
            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            public IWindowSurrogate Window { get; private set; }
            void window_Closed(object sender, EventArgs e) {
                RemoveFromWindowsList();
                Window.RealWindow.Closing -= window_Closing;
                Window.RealWindow.Closed -= window_Closed;
                DocumentViewModelHelper.OnDestroy(GetContent());
            }
            void window_Closing(object sender, CancelEventArgs e) {
                DocumentViewModelHelper.OnClose(GetContent(), e);
                if(!destroyOnClose && !e.Cancel) {
                    e.Cancel = true;
                    HideWindow(Window.RealWindow);
                }
            }
            void IDocument.Close(bool force) {
                if(!force) {
                    Window.RealWindow.Close();
                    return;
                }
                Window.RealWindow.Closing -= window_Closing;
                if(!destroyOnClose)
                    HideWindow(Window.RealWindow);
                else
                    Window.RealWindow.Close();
            }
            void HideWindow(WindowBase window) {
#if !SILVERLIGHT
                window.Hide();
#else
                window.Closed -= window_Closed;
                window.Hide();
                window.Closed += window_Closed;
#endif
            }
            bool IDocument.DestroyOnClose {
                get { return destroyOnClose; }
                set { destroyOnClose = value; }
            }
            void IDocument.Show() {
                switch(owner.DocumentShowMode) {
                    case WindowShowMode.Dialog:
                        Window.ShowDialog();
                        break;
                    default:
                        Window.Show();
                        break;
                }
            }
            void IDocument.Hide() {
                HideWindow(Window.RealWindow);
            }

            object IDocument.Id {
                get;
                set;
            }

            object IDocument.Title {
                get { return Window.RealWindow.Title; }
                set { Window.RealWindow.Title = Convert.ToString(value); }
            }

            object IDocument.Content {
                get { return GetContent(); }
            }

            void RemoveFromWindowsList() {
                owner.windows.Remove(Window);
            }
            object GetContent() {
                return ViewHelper.GetViewModelFromView(documentContentView);
            }
        }
        public static readonly DependencyProperty WindowStartupLocationProperty =
            DependencyProperty.Register("WindowStartupLocation", typeof(WindowStartupLocation), typeof(WindowedDocumentUIService), new PropertyMetadata(WindowStartupLocation.CenterScreen));
        public static readonly DependencyProperty WindowStyleProperty =
            DependencyProperty.Register("WindowStyle", typeof(Style), typeof(WindowedDocumentUIService), new PropertyMetadata(null));
#if !SILVERLIGHT
        public static readonly DependencyProperty WindowStyleSelectorProperty =
            DependencyProperty.Register("WindowStyleSelector", typeof(StyleSelector), typeof(WindowedDocumentUIService), new PropertyMetadata(null));
        public static readonly DependencyProperty SetWindowOwnerProperty =
            DependencyProperty.Register("SetWindowOwner", typeof(bool), typeof(WindowedDocumentUIService), new PropertyMetadata(true));
#endif
        public static readonly DependencyProperty WindowTypeProperty =
            DependencyProperty.Register("WindowType", typeof(Type), typeof(WindowedDocumentUIService), new PropertyMetadata(typeof(Window)));
        public static readonly DependencyProperty DocumentShowModeProperty =
            DependencyProperty.Register("DocumentShowMode", typeof(WindowShowMode), typeof(WindowedDocumentUIService),
            new PropertyMetadata(WindowShowMode.Default));
        public static readonly DependencyProperty ActiveDocumentProperty =
            DependencyProperty.Register("ActiveDocument", typeof(IDocument), typeof(WindowedDocumentUIService),
            new PropertyMetadata(null, (d, e) => ((WindowedDocumentUIService)d).OnActiveDocumentChanged(e.OldValue as IDocument, e.NewValue as IDocument)));

        public WindowStartupLocation WindowStartupLocation {
            get { return (WindowStartupLocation)GetValue(WindowStartupLocationProperty); }
            set { SetValue(WindowStartupLocationProperty, value); }
        }
        public Style WindowStyle {
            get { return (Style)GetValue(WindowStyleProperty); }
            set { SetValue(WindowStyleProperty, value); }
        }
        public IDocument ActiveDocument {
            get { return (IDocument)GetValue(ActiveDocumentProperty); }
            set { SetValue(ActiveDocumentProperty, value); }
        }
#if !SILVERLIGHT
        public StyleSelector WindowStyleSelector {
            get { return (StyleSelector)GetValue(WindowStyleSelectorProperty); }
            set { SetValue(WindowStyleSelectorProperty, value); }
        }
        public bool SetWindowOwner {
            get { return (bool)GetValue(SetWindowOwnerProperty); }
            set { SetValue(SetWindowOwnerProperty, value); }
        }
#endif
        public Type WindowType {
            get { return (Type)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }
        public WindowShowMode DocumentShowMode {
            get { return (WindowShowMode)GetValue(DocumentShowModeProperty); }
            set { SetValue(DocumentShowModeProperty, value); }
        }

        protected virtual IWindowSurrogate CreateWindow(object view) {
            IWindowSurrogate window = WindowProxy.GetWindowSurrogate(Activator.CreateInstance(WindowType));
            UpdateThemeName(window.RealWindow);
            window.RealWindow.Content = view;
#if SILVERLIGHT
            window.RealWindow.Style = WindowStyle;
#else
            if(SetWindowOwner) window.RealWindow.Owner = Window.GetWindow(AssociatedObject);
            window.RealWindow.Style = GetDocumentContainerStyle(window.RealWindow, view, WindowStyle, WindowStyleSelector);
            window.RealWindow.WindowStartupLocation = this.WindowStartupLocation;
#endif
            return window;
        }
        IDocument IDocumentManagerService.CreateDocument(string documentType, object viewModel, object parameter, object parentViewModel) {
            object view = CreateAndInitializeView(documentType, viewModel, parameter, parentViewModel, this);
            IWindowSurrogate window = CreateWindow(view);
            windows.Add(window);
            SubscribeWindow(window.RealWindow);
            IDocument document = new WindowDocument(this, window, view);
            SetDocument(window.RealWindow, document);
            SetTitleBinding(view, WindowBase.TitleProperty, window.RealWindow, true);
            return document;
        }
        void IDocumentOwner.Close(IDocumentContent documentContent, bool force) {
            CloseDocument(this, documentContent, force);
        }

        IList<IWindowSurrogate> windows = new List<IWindowSurrogate>();
        IEnumerable<IDocument> IDocumentManagerService.Documents { get { return windows.Select(w => GetDocument(w.RealWindow)); } }


        public event ActiveDocumentChangedEventHandler ActiveDocumentChanged;

        void SubscribeWindow(WindowBase window) {
            window.Activated += OnWindowActivated;
#if !SILVERLIGHT
            window.Deactivated += OnWindowDeactivated;
#endif
            window.Closed += OnWindowClosed;
        }
        void UnsubscribeWindow(WindowBase window) {
            window.Activated -= OnWindowActivated;
#if !SILVERLIGHT
            window.Deactivated -= OnWindowDeactivated;
#endif
            window.Closed -= OnWindowClosed;
        }
        void OnWindowClosed(object sender, EventArgs e) {
            if(windows.Count == 1)
                ActiveDocument = null;
            UnsubscribeWindow((WindowBase)sender);
        }
        void OnWindowActivated(object sender, EventArgs e) {
            ActiveDocument = GetDocument((WindowBase)sender);
        }
#if !SILVERLIGHT
        void OnWindowDeactivated(object sender, EventArgs e) {
            if(ActiveDocument == GetDocument((Window)sender))
                ActiveDocument = null;
        }
#endif
        void OnActiveDocumentChanged(IDocument oldValue, IDocument newValue) {
            WindowDocument newDocument = (WindowDocument)newValue;
            if(newDocument != null)
                newDocument.Window.RealWindow.Activate();
            if(ActiveDocumentChanged != null)
                ActiveDocumentChanged(this, new ActiveDocumentChangedEventArgs(oldValue, newValue));
        }
    }
}