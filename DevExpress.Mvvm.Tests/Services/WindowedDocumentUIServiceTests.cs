#if SILVERLIGHT
using Window = DevExpress.Xpf.Core.DXWindow;
using Microsoft.Silverlight.Testing;
#else
using NUnit.Framework;
#endif
using DevExpress.Mvvm.Tests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        class ViewModelWithNullTitle : IDocumentContent {
            public IDocumentOwner DocumentOwner { get; set; }
            void IDocumentContent.OnClose(CancelEventArgs e) { }
            object IDocumentContent.Title { get { return null; } }
            void IDocumentContent.OnDestroy() { }
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
                Assert.AreEqual("Style Tag", windowDocument.Window.RealWindow.Tag);
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
            Assert.AreEqual("Style Selector Tag", windowDocument.Window.RealWindow.Tag);
        }
#endif
    }
    [TestFixture]
    public class WindowedDocumentUIServiceIDocumentContentCloseTests : BaseWpfFixture {
        public class EmptyView : FrameworkElement { }
        class TestDocumentContent : IDocumentContent {
            Func<bool> close;
            Action onDestroy;

            public TestDocumentContent(Func<bool> close, Action onDestroy = null) {
                this.close = close;
                this.onDestroy = onDestroy;
            }
            public IDocumentOwner DocumentOwner { get; set; }
            public void OnClose(CancelEventArgs e) { e.Cancel = !close(); }
            public object Title { get { return null; } }
            void IDocumentContent.OnDestroy() {
                if(onDestroy != null)
                    onDestroy();
            }
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

        void DoCloseTest(bool allowClose, bool destroyOnClose, bool destroyed, Action<IDocument> closeMethod) {
            DoCloseTest((bool?)allowClose, destroyOnClose, destroyed, (d, b) => closeMethod(d));
        }
        void DoCloseTest(bool? allowClose, bool destroyOnClose, bool destroyed, Action<IDocument, bool> closeMethod) {
            EnqueueShowWindow();
            IDocumentManagerService service = null;
            bool closeChecked = false;
            bool destroyCalled = false;
            IDocument document = null;
            EnqueueCallback(() => {
                WindowedDocumentUIService windowedDocumentUIService = new WindowedDocumentUIService();
                Interaction.GetBehaviors(Window).Add(windowedDocumentUIService);
                service = windowedDocumentUIService;
                TestDocumentContent viewModel = new TestDocumentContent(() => {
                    closeChecked = true;
                    return allowClose != null && allowClose.Value;
                }, () => {
                    destroyCalled = true;
                });
                document = service.CreateDocument("EmptyView", viewModel);
                document.Show();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                document.DestroyOnClose = destroyOnClose;
                closeMethod(document, allowClose == null);
                Assert.AreEqual(allowClose != null, closeChecked);
                Assert.AreEqual(destroyed, destroyCalled);
                Assert.AreEqual(!destroyed, service.Documents.Contains(document));
            });
            EnqueueTestComplete();
        }
        void CloseDocument(IDocument document, bool force) {
            document.Close(force);
        }
        void CloseDocumentWithDocumentOwner(IDocument document, bool force) {
            IDocumentContent documentContent = (IDocumentContent)document.Content;
            documentContent.DocumentOwner.Close(documentContent, force);
        }
        void CloseDocumentWithClosingWindow(IDocument document) {
            Window window = ((WindowedDocumentUIService.WindowDocument)document).Window.RealWindow;
            window.Close();
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest1() {
            DoCloseTest(allowClose: false, destroyOnClose: false, destroyed: false, closeMethod: CloseDocument);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest2() {
            DoCloseTest(allowClose: false, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest12() {
            DoCloseTest(allowClose: false, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithClosingWindow);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest3() {
            DoCloseTest(allowClose: false, destroyOnClose: true, destroyed: false, closeMethod: CloseDocument);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest4() {
            DoCloseTest(allowClose: false, destroyOnClose: true, destroyed: false, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest14() {
            DoCloseTest(allowClose: false, destroyOnClose: true, destroyed: false, closeMethod: CloseDocumentWithClosingWindow);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest5() {
            DoCloseTest(allowClose: true, destroyOnClose: false, destroyed: false, closeMethod: CloseDocument);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest6() {
            DoCloseTest(allowClose: true, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest16() {
            DoCloseTest(allowClose: true, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithClosingWindow);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest7() {
            DoCloseTest(allowClose: true, destroyOnClose: true, destroyed: true, closeMethod: CloseDocument);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest8() {
            DoCloseTest(allowClose: true, destroyOnClose: true, destroyed: true, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest18() {
            DoCloseTest(allowClose: true, destroyOnClose: true, destroyed: true, closeMethod: CloseDocumentWithClosingWindow);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest9() {
            DoCloseTest(allowClose: null, destroyOnClose: false, destroyed: false, closeMethod: CloseDocument);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest10() {
            DoCloseTest(allowClose: null, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest11() {
            DoCloseTest(allowClose: null, destroyOnClose: true, destroyed: true, closeMethod: CloseDocument);
        }
        [Test, Asynchronous]
        public void IDocumentViewModelCloseTest13() {
            DoCloseTest(allowClose: null, destroyOnClose: true, destroyed: true, closeMethod: CloseDocumentWithDocumentOwner);
        }
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
        Type IViewLocator.ResolveViewType(string name) {
            throw new NotImplementedException();
        }
    }
}