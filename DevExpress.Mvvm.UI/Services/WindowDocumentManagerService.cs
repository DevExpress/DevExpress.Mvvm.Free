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
using WindowBase = System.Windows.Window;

namespace DevExpress.Mvvm.UI {
    [TargetTypeAttribute(typeof(UserControl))]
    [TargetTypeAttribute(typeof(Window))]
    public class WindowedDocumentUIService : DocumentUIServiceBase, IDocumentManagerService, IDocumentOwner {
        public class WindowDocument : IDocument, IDocumentInfo {
            public readonly object documentContentView;
            bool destroyOnClose = true;
            readonly WindowedDocumentUIService owner;
            DocumentState state = DocumentState.Hidden;
            string documentType;

            public WindowDocument(WindowedDocumentUIService owner, IWindowSurrogate window, object documentContentView, string documentType) {
                this.owner = owner;
                Window = window;
                this.documentContentView = documentContentView;
                this.documentType = documentType;
                Window.Closing += window_Closing;
                Window.Closed += window_Closed;
            }
            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            public IWindowSurrogate Window { get; private set; }
            void window_Closed(object sender, EventArgs e) {
                RemoveFromWindowsList();
                Window.Closing -= window_Closing;
                Window.Closed -= window_Closed;
                DocumentViewModelHelper.OnDestroy(GetContent());
            }
            bool onClosing = false;
            void window_Closing(object sender, CancelEventArgs e) {
                onClosing = true;
                DocumentViewModelHelper.OnClose(GetContent(), e);
                if(!destroyOnClose && !e.Cancel) {
                    e.Cancel = true;
                    Window.Hide();
                }
                state = destroyOnClose ? DocumentState.Destroyed : DocumentState.Hidden;
                onClosing = false;
            }
            void IDocument.Close(bool force) {
                if(!force) {
                    if(!onClosing) {
                        Window.Close();
                    }
                    return;
                }
                Window.Closing -= window_Closing;
                if(!destroyOnClose) {
                    if(!onClosing) {
                        Window.Hide();
                    }
                    state = DocumentState.Hidden;
                } else {
                    if(!onClosing) {
                        Window.Close();
                    }
                    state = DocumentState.Destroyed;
                }
            }
            public bool DestroyOnClose {
                get { return destroyOnClose; }
                set { destroyOnClose = value; }
            }
            void IDocument.Show() {
                state = DocumentState.Visible;
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
                Window.Hide();
                state = DocumentState.Hidden;
            }

            public object Id {
                get;
                set;
            }

            public object Title {
                get { return Window.RealWindow.Title; }
                set { Window.RealWindow.Title = Convert.ToString(value); }
            }

            public object Content {
                get { return GetContent(); }
            }

            DocumentState IDocumentInfo.State { get { return state; } }

            void RemoveFromWindowsList() {
                owner.windows.Remove(Window);
            }
            object GetContent() {
                return ViewHelper.GetViewModelFromView(documentContentView);
            }
            string IDocumentInfo.DocumentType {
                get { return documentType; }
            }
        }
        public static readonly DependencyProperty WindowStartupLocationProperty =
            DependencyProperty.Register("WindowStartupLocation", typeof(WindowStartupLocation), typeof(WindowedDocumentUIService), new PropertyMetadata(WindowStartupLocation.CenterScreen));
        public static readonly DependencyProperty WindowStyleProperty =
            DependencyProperty.Register("WindowStyle", typeof(Style), typeof(WindowedDocumentUIService), new PropertyMetadata(null));
        public static readonly DependencyProperty WindowStyleSelectorProperty =
            DependencyProperty.Register("WindowStyleSelector", typeof(StyleSelector), typeof(WindowedDocumentUIService), new PropertyMetadata(null));
        public static readonly DependencyProperty SetWindowOwnerProperty =
            DependencyProperty.Register("SetWindowOwner", typeof(bool), typeof(WindowedDocumentUIService), new PropertyMetadata(true));
        public static readonly DependencyProperty WindowTypeProperty =
            DependencyProperty.Register("WindowType", typeof(Type), typeof(WindowedDocumentUIService), new PropertyMetadata(null));

        public static readonly DependencyProperty DocumentShowModeProperty =
            DependencyProperty.Register("DocumentShowMode", typeof(WindowShowMode), typeof(WindowedDocumentUIService),
            new PropertyMetadata(WindowShowMode.Default));
        public static readonly DependencyProperty ActiveDocumentProperty =
            DependencyProperty.Register("ActiveDocument", typeof(IDocument), typeof(WindowedDocumentUIService),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((WindowedDocumentUIService)d).OnActiveDocumentChanged(e.OldValue as IDocument, e.NewValue as IDocument)));
        static readonly DependencyPropertyKey ActiveViewPropertyKey =
            DependencyProperty.RegisterReadOnly("ActiveView", typeof(object), typeof(WindowedDocumentUIService), new PropertyMetadata(null));
        public static readonly DependencyProperty ActiveViewProperty = ActiveViewPropertyKey.DependencyProperty;

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
        public object ActiveView {
            get { return GetValue(ActiveViewProperty); }
            private set { SetValue(ActiveViewPropertyKey, value); }
        }
        public StyleSelector WindowStyleSelector {
            get { return (StyleSelector)GetValue(WindowStyleSelectorProperty); }
            set { SetValue(WindowStyleSelectorProperty, value); }
        }
        public bool SetWindowOwner {
            get { return (bool)GetValue(SetWindowOwnerProperty); }
            set { SetValue(SetWindowOwnerProperty, value); }
        }
        public Type WindowType {
            get { return (Type)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }
        public WindowShowMode DocumentShowMode {
            get { return (WindowShowMode)GetValue(DocumentShowModeProperty); }
            set { SetValue(DocumentShowModeProperty, value); }
        }
        Type ActualWindowType { get { return WindowType ?? WindowService.GetDefaultWindowType(WindowStyle); } }

        protected virtual IWindowSurrogate CreateWindow(object view) {
            IWindowSurrogate window = WindowProxy.GetWindowSurrogate(Activator.CreateInstance(ActualWindowType));
            UpdateThemeName(window.RealWindow);
            window.RealWindow.Content = view;
            if(SetWindowOwner) window.RealWindow.Owner = Window.GetWindow(AssociatedObject);
            Style windowStyle = GetDocumentContainerStyle(window.RealWindow, view, WindowStyle, WindowStyleSelector);
            InitializeDocumentContainer(window.RealWindow, Window.ContentProperty, windowStyle);
            window.RealWindow.WindowStartupLocation = this.WindowStartupLocation;
            return window;
        }
        IDocument IDocumentManagerService.CreateDocument(string documentType, object viewModel, object parameter, object parentViewModel) {
            object view = CreateAndInitializeView(documentType, viewModel, parameter, parentViewModel, this);
            IWindowSurrogate window = CreateWindow(view);
            windows.Add(window);
            SubscribeWindow(window);
            IDocument document = new WindowDocument(this, window, view, documentType);
            SetDocument(window.RealWindow, document);
            SetTitleBinding(view, WindowBase.TitleProperty, window.RealWindow, true);
            return document;
        }
        void IDocumentOwner.Close(IDocumentContent documentContent, bool force) {
            CloseDocument(this, documentContent, force);
        }

        IList<IWindowSurrogate> windows = new List<IWindowSurrogate>();
        public IEnumerable<IDocument> Documents { get { return windows.Select(w => GetDocument(w.RealWindow)); } }


        public event ActiveDocumentChangedEventHandler ActiveDocumentChanged;

        void SubscribeWindow(IWindowSurrogate window) {
            window.Activated += OnWindowActivated;
            window.Deactivated += OnWindowDeactivated;
            window.Closed += OnWindowClosed;
        }
        void UnsubscribeWindow(IWindowSurrogate window) {
            window.Activated -= OnWindowActivated;
            window.Deactivated -= OnWindowDeactivated;
            window.Closed -= OnWindowClosed;
        }
        void OnWindowClosed(object sender, EventArgs e) {
            if(windows.Count == 1)
                ActiveDocument = null;
            UnsubscribeWindow(WindowProxy.GetWindowSurrogate(sender));
        }
        void OnWindowActivated(object sender, EventArgs e) {
            ActiveDocument = GetDocument((WindowBase)sender);
        }
        void OnWindowDeactivated(object sender, EventArgs e) {
            if(ActiveDocument == GetDocument((Window)sender))
                ActiveDocument = null;
        }
        void OnActiveDocumentChanged(IDocument oldValue, IDocument newValue) {
            WindowDocument newDocument = (WindowDocument)newValue;
            if(newDocument != null)
                newDocument.Window.Activate();
            ActiveView = newDocument.With(x => x.documentContentView);
            if(ActiveDocumentChanged != null)
                ActiveDocumentChanged(this, new ActiveDocumentChangedEventArgs(oldValue, newValue));
        }
    }
}