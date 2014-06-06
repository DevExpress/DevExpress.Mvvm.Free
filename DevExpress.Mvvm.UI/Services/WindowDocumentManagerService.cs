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
#if SILVERLIGHT
using Window = DevExpress.Xpf.Core.DXWindow;
#endif

namespace DevExpress.Mvvm.UI {
    [TargetTypeAttribute(typeof(UserControl))]
    [TargetTypeAttribute(typeof(Window))]
    public class WindowedDocumentUIService : DocumentUIServiceBase, IDocumentManagerService {
        public class WindowDocument : IDocument {
            readonly object documentContentView;
            internal readonly Window window;
            bool destroyOnClose = true;
            readonly WindowedDocumentUIService owner;
            public WindowDocument(WindowedDocumentUIService owner, Window window, object documentContentView) {
                this.owner = owner;
                this.window = window;
                this.documentContentView = documentContentView;
                window.Closing += window_Closing;
                window.Closed += window_Closed;
            }
            void window_Closed(object sender, EventArgs e) {
                RemoveFromWindowsList();
                window.Closing -= window_Closing;
                window.Closed -= window_Closed;
            }
            void window_Closing(object sender, CancelEventArgs e) {
                IDocumentViewModel documentContent = GetContent() as IDocumentViewModel;
                bool cancel = documentContent != null && !documentContent.Close();
                if(destroyOnClose) {
                    e.Cancel = cancel;
                } else {
                    e.Cancel = true;
                    if(!cancel)
                        window.Hide();
                }
            }
            void IDocument.Close(bool force) {
                if(force) {
                    window.Closing -= window_Closing;
                }
                window.Close();
            }
            bool IDocument.DestroyOnClose {
                get { return destroyOnClose; }
                set { destroyOnClose = value; }
            }
            void IDocument.Show() {
                switch(owner.DocumentShowMode) {
                    case WindowShowMode.Dialog: window.ShowDialog();
                        break;
                    default: window.Show();
                        break;
                }
            }
            void IDocument.Hide() {
                window.Hide();
            }

            object IDocument.Id {
                get;
                set;
            }

            object IDocument.Title {
                get { return window.Title; }
                set { window.Title = Convert.ToString(value); }
            }

            object IDocument.Content {
                get { return GetContent(); }
            }

            void RemoveFromWindowsList() {
                owner.windows.Remove(window);
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
#endif
        public Type WindowType {
            get { return (Type)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }
        public WindowShowMode DocumentShowMode {
            get { return (WindowShowMode)GetValue(DocumentShowModeProperty); }
            set { SetValue(DocumentShowModeProperty, value); }
        }

        protected virtual Window CreateWindow(object view) {
            Window window = (Window)Activator.CreateInstance(WindowType);
            UpdateThemeName(window);
            window.Content = view;
#if SILVERLIGHT
            window.Style = WindowStyle;
#else
            window.Owner = Window.GetWindow(AssociatedObject);
            window.Style = GetDocumentContainerStyle(window, view, WindowStyle, WindowStyleSelector);
            window.WindowStartupLocation = this.WindowStartupLocation;
#endif
            return window;
        }
        IDocument IDocumentManagerService.CreateDocument(string documentType, object viewModel, object parameter, object parentViewModel) {
            object view = CreateAndInitializeView(documentType, viewModel, parameter, parentViewModel);
            Window window = CreateWindow(view);
            windows.Add(window);
            SubscribeWindow(window);
            IDocument document = new WindowDocument(this, window, view);
            SetDocument(window, document);
            SetTitleBinding(view, Window.TitleProperty, window, true);
            return document;
        }

        IList<Window> windows = new List<Window>();
        IEnumerable<IDocument> IDocumentManagerService.Documents { get { return windows.Select(w => GetDocument(w)); } }


        public event ActiveDocumentChangedEventHandler ActiveDocumentChanged;

        void SubscribeWindow(Window window) {
            window.Activated += OnWindowActivated;
#if !SILVERLIGHT
            window.Deactivated += OnWindowDeactivated;
#endif
            window.Closed += OnWindowClosed;
        }
        void UnsubscribeWindow(Window window) {
            window.Activated -= OnWindowActivated;
#if !SILVERLIGHT
            window.Deactivated -= OnWindowDeactivated;
#endif
            window.Closed -= OnWindowClosed;
        }
        void OnWindowClosed(object sender, EventArgs e) {
            if(windows.Count == 1)
                ActiveDocument = null;
            UnsubscribeWindow((Window)sender);
        }
        void OnWindowActivated(object sender, EventArgs e) {
            ActiveDocument = GetDocument((Window)sender);
        }
#if !SILVERLIGHT
        void OnWindowDeactivated(object sender, EventArgs e) {
            if(ActiveDocument == GetDocument((Window)sender))
                ActiveDocument = null;
        }
#endif
        void OnActiveDocumentChanged(IDocument oldValue, IDocument newValue) {
            if((WindowDocument)newValue != null) {
                ((WindowDocument)newValue).window.Activate();
            }
            if(ActiveDocumentChanged != null)
                ActiveDocumentChanged(this, new ActiveDocumentChangedEventArgs(oldValue, newValue));
        }
    }
}