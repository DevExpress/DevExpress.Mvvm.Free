using NUnit.Framework;
using DevExpress.Mvvm.Tests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class IWindowSurrogateTests {
        [Test]
        public void WindowProxyTest() {
            Window w = new Window();
            var ws1 = WindowProxy.GetWindowSurrogate(w);
            var ws2 = WindowProxy.GetWindowSurrogate(w);
            Assert.AreSame(ws1, ws2);
        }
    }

    [TestFixture]
    public class WindowedDocumentUIServiceTests : BaseWpfFixture {
        public class View1 : FrameworkElement { }
        public class View2 : FrameworkElement { }
        public class EmptyView : FrameworkElement { }
        protected class TestStyleSelector : StyleSelector {
            public Style Style { get; set; }
            public override Style SelectStyle(object item, DependencyObject container) { return Style; }
        }
        protected class ViewModelWithNullTitle : IDocumentContent {
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
            try {
                WindowedDocumentUIService service = Interaction.GetBehaviors(Window).OfType<WindowedDocumentUIService>().FirstOrDefault();
                if (service != null) {
                    while (service.Documents.Any()) {
                        service.Documents.First().DestroyOnClose = true;
                        service.Documents.First().Close(true);
                    }
                }
                Interaction.GetBehaviors(Window).Clear();
            } catch { }
            ViewLocator.Default = null;
            base.TearDownCore();
        }

        [Test]
        public void WindowStyle() {
            EnqueueShowWindow();
            Window.Show();
            IDocument document = null;
            DispatcherHelper.DoEvents();

            WindowedDocumentUIService service = CreateService();
            Interaction.GetBehaviors(Window).Add(service);
            service.WindowType = typeof(Window);
            Style windowStyle = new Style(typeof(Window));
            windowStyle.Setters.Add(new Setter(FrameworkElement.TagProperty, "Style Tag"));
            service.WindowStyle = windowStyle;
            document = service.CreateDocument("EmptyView", new object());
            DispatcherHelper.DoEvents();

            var windowDocument = (WindowedDocumentUIService.WindowDocument)document;
            Assert.AreEqual("Style Tag", windowDocument.Window.RealWindow.Tag);
        }



        public class UITestViewModel {
            public virtual string Title { get; set; }
            public virtual int Parameter { get; set; }
        }
        [Test]
        public void ActivateWhenDocumentShow() {
            Window.Show();
            IDocument document = null;
            WindowedDocumentUIService service = CreateService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;

            DispatcherHelper.DoEvents();
            document = iService.CreateDocument("View1",
                    ViewModelSource.Create(() => new UITestViewModel() { Title = "Title1", Parameter = 1 }));
            document.Show();

            DispatcherHelper.DoEvents();
            Assert.AreEqual(iService.ActiveDocument, document);
            Assert.IsNotNull(document.Content);
            Assert.AreEqual(document.Content, ViewHelper.GetViewModelFromView(service.ActiveView));
            Assert.AreEqual(document.Content, ((WindowedDocumentUIService.WindowDocument)document).Window.RealWindow.DataContext);
            document.Close();
        }


        [Test]
        public void UnActiveDocumentClose() {
            Window.Show();
            DispatcherHelper.DoEvents();
            IDocument document1 = null;
            IDocument document2 = null;
            WindowedDocumentUIService service = CreateService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;
            int counter = 0;

            iService.ActiveDocumentChanged += (s, e) => counter++;
            document1 = iService.CreateDocument("View1",
                ViewModelSource.Create(() => new UITestViewModel() { Title = "Title1", Parameter = 1 }));
            document2 = iService.CreateDocument("View2",
                ViewModelSource.Create(() => new UITestViewModel() { Title = "Title2", Parameter = 2 }));
            document1.Show();
            Assert.AreEqual(1, counter);
            document2.Show();
            DispatcherHelper.DoEvents();

            Assert.AreEqual(iService.ActiveDocument, document2);
            Assert.IsNotNull(document2.Content);
            Assert.AreEqual(document2.Content, ViewHelper.GetViewModelFromView(service.ActiveView));
            document1.Close();
            DispatcherHelper.DoEvents();

            Assert.AreEqual(iService.ActiveDocument, document2);
            Assert.IsNotNull(document2.Content);
            Assert.AreEqual(document2.Content, ViewHelper.GetViewModelFromView(service.ActiveView));
            Assert.AreEqual(3, counter);
            document2.Close();
        }

        [Test]
        public void SettingActiveDocument() {
            Window.Show();
            DispatcherHelper.DoEvents();
            IDocument document1 = null;
            IDocument document2 = null;
            WindowedDocumentUIService service = CreateService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;
            int counter = 0;

            iService.ActiveDocumentChanged += (s, e) => counter++;
            document1 = iService.CreateDocument("View1",
                ViewModelSource.Create(() => new UITestViewModel() { Title = "Title1", Parameter = 1 }));
            document2 = iService.CreateDocument("View2",
                ViewModelSource.Create(() => new UITestViewModel() { Title = "Title2", Parameter = 2 }));
            document1.Show();
            document2.Show();
            DispatcherHelper.DoEvents();

            Assert.AreEqual(3, counter);
            iService.ActiveDocument = document1;
            DispatcherHelper.DoEvents();

            Assert.AreEqual(iService.ActiveDocument, document1);
            Assert.AreEqual(4, counter);
            document1.Close();
            document2.Close();
        }

#if !DXCORE3
        [Test, Retry(3)]
        public void ActivateDocumentsWhenClosed() {
            Window.Show();
            DispatcherHelper.DoEvents();
            List<IDocument> documents = new List<IDocument>();
            WindowedDocumentUIService service = CreateService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;
            int counter = 0;

            service.ActiveDocumentChanged += (s, e) => counter++;
            for (int i = 0; i < 4; i++) {
                documents.Add(iService.CreateDocument("View" + i,
                ViewModelSource.Create(() => new UITestViewModel() { Title = "Title" + i, Parameter = i })));
                documents[i].Show();
            }
            iService.ActiveDocument = documents[1];
            DispatcherHelper.DoEvents();

            counter = 0;
            documents[1].Close();
            DispatcherHelper.DoEvents();

            Assert.AreEqual(2, counter, "#1");
            Assert.AreEqual(iService.ActiveDocument, documents[3]);
            iService.ActiveDocument = documents[3];
            DispatcherHelper.DoEvents();

            counter = 0;
            documents[3].Close();
            DispatcherHelper.DoEvents();

            Assert.AreEqual(iService.ActiveDocument, documents[2]);
            Assert.AreEqual(2, counter, "#2");
            documents[0].Close();
            documents[2].Close();
        }
#endif


        public class BindingTestClass : DependencyObject {
            public static readonly DependencyProperty ActiveDocumentProperty =
                DependencyProperty.Register("ActiveDocument", typeof(IDocument), typeof(BindingTestClass), new PropertyMetadata(null));
            public IDocument ActiveDocument {
                get { return (IDocument)GetValue(ActiveDocumentProperty); }
                set { SetValue(ActiveDocumentProperty, value); }
            }
            static readonly DependencyPropertyKey ReadonlyActiveDocumentPropertyKey
                = DependencyProperty.RegisterReadOnly("ReadonlyActiveDocument", typeof(IDocument), typeof(BindingTestClass), new PropertyMetadata(null));
            public static readonly DependencyProperty ReadonlyActiveDocumentProperty = ReadonlyActiveDocumentPropertyKey.DependencyProperty;

            public IDocument ReadonlyActiveDocument {
                get { return (IDocument)GetValue(ReadonlyActiveDocumentProperty); }
                private set { SetValue(ReadonlyActiveDocumentPropertyKey, value); }
            }
        }

        [Test]
        public void TwoWayBinding() {
            Window.Show();
            DispatcherHelper.DoEvents();
            BindingTestClass testClass = new BindingTestClass();
            WindowedDocumentUIService service = CreateService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;

            IDocument document1 = null;
            IDocument document2 = null;
            BindingOperations.SetBinding(service, WindowedDocumentUIService.ActiveDocumentProperty,
                new Binding() { Path = new PropertyPath(BindingTestClass.ActiveDocumentProperty), Source = testClass, Mode = BindingMode.Default });

            document1 = iService.CreateDocument("View1", ViewModelSource.Create(() => new UITestViewModel() { Title = "Title1", Parameter = 1 }));
            document1.Show();
            DispatcherHelper.DoEvents();
            document2 = iService.CreateDocument("View2", ViewModelSource.Create(() => new UITestViewModel() { Title = "Title2", Parameter = 2 }));
            document2.Show();
            DispatcherHelper.DoEvents();
            DispatcherHelper.DoEvents();

            Assert.AreSame(document2, service.ActiveDocument);
            Assert.AreSame(document2, testClass.ActiveDocument);
            testClass.ActiveDocument = document1;
            DispatcherHelper.DoEvents();
            Assert.AreSame(document1, service.ActiveDocument);
            DispatcherHelper.DoEvents();
        }

        class TestDocumentContent : IDocumentContent {
            public Action onClose = null;
            public IDocumentOwner DocumentOwner { get; set; }
            public void OnClose(CancelEventArgs e) {
                onClose();
            }
            public void OnDestroy() {

            }
            public object Title {
                get { return ""; }
            }
        }


        [Test]
        public void ClosingDocumentOnWindowClosingShouldntThrow() {
            Window.Show();
            DispatcherHelper.DoEvents();
            BindingTestClass testClass = new BindingTestClass();
            WindowedDocumentUIService service = CreateService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;

            IDocument document = null;
            WindowedDocumentUIService.WindowDocument typedDocument = null;
            var content = new TestDocumentContent();

            document = iService.CreateDocument("View1", content);
            typedDocument = (WindowedDocumentUIService.WindowDocument)document;
            document.Show();
            DispatcherHelper.DoEvents();

            Window window = typedDocument.Window.RealWindow;
            content.onClose = () => document.Close();
            window.Close();
            DispatcherHelper.DoEvents();
        }

        [Test]
        public void TwoWayBindingReadonlyProperty() {
            BindingTestClass testClass = new BindingTestClass();
            WindowedDocumentUIService service = CreateService();
            Interaction.GetBehaviors(Window).Add(service);
            IDocumentManagerService iService = service;
            Assert.Throws<InvalidOperationException>(() => {
                BindingOperations.SetBinding(service, WindowedDocumentUIService.ActiveDocumentProperty,
                new Binding() { Path = new PropertyPath(BindingTestClass.ReadonlyActiveDocumentProperty), Source = testClass, Mode = BindingMode.Default });
            });
        }

        [Test]
        public void WindowStyleSelector() {
            WindowedDocumentUIService service = CreateService();
            Interaction.GetBehaviors(Window).Add(service);
            service.WindowType = typeof(Window);
            Style windowStyle = new Style(typeof(Window));
            windowStyle.Setters.Add(new Setter(System.Windows.Window.TagProperty, "Style Selector Tag"));
            service.WindowStyleSelector = new TestStyleSelector() { Style = windowStyle };
            IDocument document = service.CreateDocument("EmptyView", new object());
            var windowDocument = (WindowedDocumentUIService.WindowDocument)document;
            Assert.AreEqual("Style Selector Tag", windowDocument.Window.RealWindow.Tag);
        }

        protected virtual WindowedDocumentUIService CreateService() {
            return new WindowedDocumentUIService() { WindowType = typeof(Window) };
        }
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
            try {
                WindowedDocumentUIService service = Interaction.GetBehaviors(Window).OfType<WindowedDocumentUIService>().FirstOrDefault();
                if (service != null) {
                    while (service.Documents.Any()) {
                        service.Documents.First().DestroyOnClose = true;
                        service.Documents.First().Close(true);
                    }
                }
                Interaction.GetBehaviors(Window).Clear();
            } catch { }
            ViewLocator.Default = null;
            base.TearDownCore();
        }
        protected virtual WindowedDocumentUIService CreateService() {
            return new WindowedDocumentUIService() { WindowType = typeof(Window) };
        }

        void DoCloseTest(bool allowClose, bool destroyOnClose, bool destroyed, Action<IDocument> closeMethod) {
            DoCloseTest((bool?)allowClose, destroyOnClose, destroyed, (d, b) => closeMethod(d));
        }
        void DoCloseTest(bool? allowClose, bool destroyOnClose, bool destroyed, Action<IDocument, bool> closeMethod) {
            Window.Show();
            DispatcherHelper.DoEvents();
            IDocumentManagerService service = null;
            bool closeChecked = false;
            bool destroyCalled = false;
            IDocument document = null;

            WindowedDocumentUIService windowedDocumentUIService = CreateService();
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
            DispatcherHelper.DoEvents();

            document.DestroyOnClose = destroyOnClose;
            closeMethod(document, allowClose == null);
            Assert.AreEqual(allowClose != null, closeChecked);
            Assert.AreEqual(destroyed, destroyCalled);
            Assert.AreEqual(!destroyed, service.Documents.Contains(document));
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
        [Test]
        public void IDocumentViewModelCloseTest1() {
            DoCloseTest(allowClose: false, destroyOnClose: false, destroyed: false, closeMethod: CloseDocument);
        }
        [Test]
        public void IDocumentViewModelCloseTest2() {
            DoCloseTest(allowClose: false, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test]
        public void IDocumentViewModelCloseTest12() {
            DoCloseTest(allowClose: false, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithClosingWindow);
        }
        [Test]
        public void IDocumentViewModelCloseTest3() {
            DoCloseTest(allowClose: false, destroyOnClose: true, destroyed: false, closeMethod: CloseDocument);
        }
        [Test]
        public void IDocumentViewModelCloseTest4() {
            DoCloseTest(allowClose: false, destroyOnClose: true, destroyed: false, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test]
        public void IDocumentViewModelCloseTest14() {
            DoCloseTest(allowClose: false, destroyOnClose: true, destroyed: false, closeMethod: CloseDocumentWithClosingWindow);
        }
        [Test]
        public void IDocumentViewModelCloseTest5() {
            DoCloseTest(allowClose: true, destroyOnClose: false, destroyed: false, closeMethod: CloseDocument);
        }
        [Test]
        public void IDocumentViewModelCloseTest6() {
            DoCloseTest(allowClose: true, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test]
        public void IDocumentViewModelCloseTest16() {
            DoCloseTest(allowClose: true, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithClosingWindow);
        }
        [Test]
        public void IDocumentViewModelCloseTest7() {
            DoCloseTest(allowClose: true, destroyOnClose: true, destroyed: true, closeMethod: CloseDocument);
        }
        [Test]
        public void IDocumentViewModelCloseTest8() {
            DoCloseTest(allowClose: true, destroyOnClose: true, destroyed: true, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test]
        public void IDocumentViewModelCloseTest18() {
            DoCloseTest(allowClose: true, destroyOnClose: true, destroyed: true, closeMethod: CloseDocumentWithClosingWindow);
        }
        [Test]
        public void IDocumentViewModelCloseTest9() {
            DoCloseTest(allowClose: null, destroyOnClose: false, destroyed: false, closeMethod: CloseDocument);
        }
        [Test]
        public void IDocumentViewModelCloseTest10() {
            DoCloseTest(allowClose: null, destroyOnClose: false, destroyed: false, closeMethod: CloseDocumentWithDocumentOwner);
        }
        [Test]
        public void IDocumentViewModelCloseTest11() {
            DoCloseTest(allowClose: null, destroyOnClose: true, destroyed: true, closeMethod: CloseDocument);
        }
        [Test]
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
            types = GetBaseTypes(ownerType).SelectMany(x => x.GetNestedTypes(BindingFlags.Public)).ToDictionary(t => t.Name, t => t);
        }
        object IViewLocator.ResolveView(string name) {
            Type type;
            return types.TryGetValue(name, out type) ? Activator.CreateInstance(type) : innerViewLocator.With(i => i.ResolveView(name));
        }
        Type IViewLocator.ResolveViewType(string name) {
            throw new NotImplementedException();
        }
        IEnumerable<Type> GetBaseTypes(Type type) {
            for(var baseType = type; baseType != null; baseType = baseType.BaseType)
                yield return baseType;
        }
        string IViewLocator.GetViewTypeName(Type type) {
            throw new NotImplementedException();
        }
    }
}