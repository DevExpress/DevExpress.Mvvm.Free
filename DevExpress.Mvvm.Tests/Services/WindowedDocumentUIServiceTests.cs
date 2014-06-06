#if SILVERLIGHT
using Window = DevExpress.Xpf.Core.DXWindow;
using Microsoft.Silverlight.Testing;
#else
using NUnit.Framework;
#endif
using DevExpress.Mvvm.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.POCO;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class WindowedDocumentUIServiceTests : BaseWpfFixture {
        public class EmptyView : FrameworkElement { }
#if !SILVERLIGHT
        class TestStyleSelector : StyleSelector {
            public Style Style { get; set; }
            public override Style SelectStyle(object item, DependencyObject container) { return Style; }
        }
#endif
        class ViewModelWithNullTitle : IDocumentViewModel {
            bool IDocumentViewModel.Close() { return true; }
            object IDocumentViewModel.Title { get { return null; } }
        }
        class TestDocumentViewModel : IDocumentViewModel {
            Func<bool> close;

            public TestDocumentViewModel(Func<bool> close) {
                this.close = close;
            }
            public bool Close() { return close(); }
            public object Title { get { return null; } }
        }

        protected override void SetUpCore() {
            base.SetUpCore();
            ViewLocator.Default = new TestViewLocator(this);
        }
        protected override void TearDownCore() {
            Interaction.GetBehaviors(Window).Clear();
            ViewLocator.Default = null;
            base.TearDownCore();
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest() {
            EnqueueShowWindow();
            IDocumentManagerService service = null;
            bool close = false;
            bool closeChecked = false;
            IDocument document = null;
            EnqueueCallback(() => {
                WindowedDocumentUIService windowedDocumentUIService = new WindowedDocumentUIService();
                Interaction.GetBehaviors(Window).Add(windowedDocumentUIService);
                service = windowedDocumentUIService;
                TestDocumentViewModel viewModel = new TestDocumentViewModel(() => {
                    closeChecked = true;
                    return close;
                });
                document = service.CreateDocument("EmptyView", viewModel);
                document.Show();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                document.DestroyOnClose = false;
                close = false;
                closeChecked = false;
                document.Close(false);
                Assert.IsTrue(closeChecked);
                Assert.IsTrue(service.Documents.Contains(document));
                close = true;
                closeChecked = false;
                document.Close(false);
                Assert.IsTrue(closeChecked);
                document.Show();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                document.DestroyOnClose = true;
                close = false;
                closeChecked = false;
                document.Close(false);
                Assert.IsTrue(closeChecked);
                Assert.IsTrue(service.Documents.Contains(document));
                close = true;
                closeChecked = false;
                document.Close(false);
                Assert.IsTrue(closeChecked);
                Assert.IsFalse(service.Documents.Contains(document));
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void WindowStyle() {
            EnqueueShowWindow();
            IDocument document = null;
            EnqueueCallback(() => {
                WindowedDocumentUIService service = new WindowedDocumentUIService();
                Interaction.GetBehaviors(Window).Add(service);
                service.WindowType = typeof(Window);
                Style windowStyle = new Style(typeof(Window));
                windowStyle.Setters.Add(new Setter(FrameworkElement.TagProperty, "Style Tag"));
                service.WindowStyle = windowStyle;
                document = service.CreateDocument("EmptyView", new object());
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                var windowDocument = (WindowedDocumentUIService.WindowDocument)document;
                Assert.AreEqual("Style Tag", windowDocument.window.Tag);
            });
            EnqueueTestComplete();
        }



        public class UITestViewModel {
            public virtual string Title { get; set; }
            public virtual int Parameter { get; set; }
        }
        [Test, Asynchronous]
        public void ActivateWhenDocumentShow() {
            EnqueueShowWindow();
            IDocument document = null;
            WindowedDocumentUIService service = new WindowedDocumentUIService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;
            EnqueueCallback(() => {
                document = iService.CreateDocument("View1",
                    ViewModelSource.Create(() => new UITestViewModel() { Title = "Title1", Parameter = 1 }));
                document.Show();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(iService.ActiveDocument, document);
                document.Close();
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void UnActiveDocumentClose() {
            EnqueueShowWindow();
            IDocument document1 = null;
            IDocument document2 = null;
            WindowedDocumentUIService service = new WindowedDocumentUIService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;
            int counter = 0;
            EnqueueCallback(() => {

                iService.ActiveDocumentChanged += (s, e) => counter++;
                document1 = iService.CreateDocument("View1",
                    ViewModelSource.Create(() => new UITestViewModel() { Title = "Title1", Parameter = 1 }));
                document2 = iService.CreateDocument("View2",
                    ViewModelSource.Create(() => new UITestViewModel() { Title = "Title2", Parameter = 2 }));
                document1.Show();
                Assert.AreEqual(1, counter);
                document2.Show();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(iService.ActiveDocument, document2);
                document1.Close();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(iService.ActiveDocument, document2);
#if !SILVERLIGHT
                Assert.AreEqual(3, counter);
#else
                Assert.AreEqual(2, counter);
#endif
                document2.Close();
            });
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
        public void SettingActiveDocument() {
            EnqueueShowWindow();
            IDocument document1 = null;
            IDocument document2 = null;
            WindowedDocumentUIService service = new WindowedDocumentUIService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;
            int counter = 0;
            EnqueueCallback(() => {
                iService.ActiveDocumentChanged += (s, e) => counter++;
                document1 = iService.CreateDocument("View1",
                    ViewModelSource.Create(() => new UITestViewModel() { Title = "Title1", Parameter = 1 }));
                document2 = iService.CreateDocument("View2",
                    ViewModelSource.Create(() => new UITestViewModel() { Title = "Title2", Parameter = 2 }));
                document1.Show();
                document2.Show();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
#if !SILVERLIGHT
                Assert.AreEqual(3, counter);
#else
                Assert.AreEqual(2, counter);
#endif
                iService.ActiveDocument = document1;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(iService.ActiveDocument, document1);
#if !SILVERLIGHT
                Assert.AreEqual(4, counter);
#else
                Assert.AreEqual(3, counter);
#endif
                document1.Close();
                document2.Close();
            });
            EnqueueTestComplete();
        }
#if !SILVERLIGHT
        [Test, Asynchronous]
        public void ActivateDocumentsWhenClosed() {
            EnqueueShowWindow();
            List<IDocument> documents = new List<IDocument>();
            WindowedDocumentUIService service = new WindowedDocumentUIService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;
            int counter = 0;
            EnqueueCallback(() => {
                service.ActiveDocumentChanged += (s, e) => counter++;
                for(int i = 0; i < 4; i++) {
                    documents.Add(iService.CreateDocument("View" + i,
                    ViewModelSource.Create(() => new UITestViewModel() { Title = "Title" + i, Parameter = i })));
                    documents[i].Show();
                }
                iService.ActiveDocument = documents[1];

            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                counter = 0;
                documents[1].Close();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, counter);
                Assert.AreEqual(iService.ActiveDocument, documents[3]);
                iService.ActiveDocument = documents[3];
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                counter = 0;
                documents[3].Close();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(iService.ActiveDocument, documents[2]);
                Assert.AreEqual(2, counter);
                documents[0].Close();
                documents[2].Close();
            });
            EnqueueTestComplete();
        }
#endif
#if !SILVERLIGHT
        [Test]
        public void WindowStyleSelector() {
            WindowedDocumentUIService service = new WindowedDocumentUIService();
            Interaction.GetBehaviors(Window).Add(service);
            service.WindowType = typeof(Window);
            Style windowStyle = new Style(typeof(Window));
            windowStyle.Setters.Add(new Setter(System.Windows.Window.TagProperty, "Style Selector Tag"));
            service.WindowStyleSelector = new TestStyleSelector() { Style = windowStyle };
            IDocument document = service.CreateDocument("EmptyView", new object());
            var windowDocument = (WindowedDocumentUIService.WindowDocument)document;
            Assert.AreEqual("Style Selector Tag", windowDocument.window.Tag);
        }
#endif
    }
    public class TestViewLocator : IViewLocator {
        Dictionary<string, Type> types;
        IViewLocator innerViewLocator;

        public TestViewLocator(object ownerTypeInstance, IViewLocator innerViewLocator = null) : this(ownerTypeInstance.GetType(), innerViewLocator) { }
        public TestViewLocator(Type ownerType, IViewLocator innerViewLocator = null) {
            this.innerViewLocator = innerViewLocator;
            types = ownerType.GetNestedTypes(BindingFlags.Public).ToDictionary(t => t.Name, t => t);
        }
        object IViewLocator.ResolveView(string name) {
            Type type;
            return types.TryGetValue(name, out type) ? Activator.CreateInstance(type) : innerViewLocator.With(i => i.ResolveView(name));
        }
    }
}