using DevExpress.Mvvm.POCO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm.UI;
using System.Windows;
using System.ComponentModel;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class DocumentManagerServiceTests {
        public class TestDocument : FrameworkElement, IDocument, IViewLocator {
#region Dependency Properties
            public static readonly DependencyProperty TitleProperty =
                DependencyProperty.Register("Title", typeof(string), typeof(TestDocument), new PropertyMetadata(null));
#endregion

            public TestDocument(string documentType, object viewModel, object parameter, object parentViewModel) {
                ViewModel = viewModel;
                ParentViewModel = parentViewModel;
                Parameter = parameter;
                DocumentType = documentType;
            }
            public void Close(bool force) {
            }
            public void Show() {
            }
            public void Hide() {
            }

            public object Id { get; set; }
            public object Title {
                get { return GetValue(TitleProperty); }
                set { SetValue(TitleProperty, value); }
            }
            public object Content { get; internal set; }
            public bool DestroyOnClose { get; set; }

            public string DocumentType { get; private set; }
            public object Parameter { get; private set; }
            public object ParentViewModel { get; private set; }
            public object ViewModel { get; private set; }
            object IViewLocator.ResolveView(string name) { return this; }
            Type IViewLocator.ResolveViewType(string viewName) {
                throw new NotImplementedException();
            }
            string IViewLocator.GetViewTypeName(Type type) {
                throw new NotImplementedException();
            }
            public DocumentState State {
                get { throw new NotImplementedException(); }
            }
        }
        public class TestDocumentManagerService : IDocumentManagerService {
            List<IDocument> docs = new List<IDocument>();
            IDocument IDocumentManagerService.CreateDocument(string documentType, object viewModel, object parameter, object parentViewModel) {
                TestDocument doc = new TestDocument(documentType, viewModel, parameter, parentViewModel);
                ViewHelper.CreateAndInitializeView(doc, string.Empty, viewModel, parameter, parentViewModel);
                var vm = new TestSupportServices();
                vm.ParentViewModel = parentViewModel;
                DocumentUIServiceBase.SetTitleBinding(doc, TestDocument.TitleProperty, doc);
                vm.Parameter = parameter;
                doc.Content = vm;
                docs.Add(doc);
                return doc;
            }
            IDocument activeDocument;
            IDocument IDocumentManagerService.ActiveDocument {
                get { return activeDocument; }
                set {activeDocument = value;}
            }
            public event ActiveDocumentChangedEventHandler ActiveDocumentChanged;

            public void OnActiveDocumentChanged(ActiveDocumentChangedEventArgs e) {
                if(ActiveDocumentChanged != null) {
                    ActiveDocumentChanged(this, e);
                }
            }

            IEnumerable<IDocument> IDocumentManagerService.Documents {
                get { return docs; }
            }
        }
        public class TestDocumentContent : IDocumentContent {
            public IDocumentOwner DocumentOwner { get;set; }
            public virtual object Title { get; set; }
            void IDocumentContent.OnClose(CancelEventArgs e) { }
            object IDocumentContent.Title { get { return Title; } }
            void IDocumentContent.OnDestroy() { }
        }
        [Test]
        public void ExtenstionTests_FindById() {
            IDocumentManagerService service = new TestDocumentManagerService();
            TestDocument document1 = (TestDocument)service.CreateDocument("Type", null);
            document1.Id = "doc1";
            Assert.IsNull(service.FindDocumentById("none"));
            Assert.AreEqual(document1, service.FindDocumentById("DOC1".ToLower()));

            Assert.AreEqual(document1, service.FindDocumentByIdOrCreate("doc1", x => { throw new NotImplementedException(); }));
            IDocument document2 = service.FindDocumentByIdOrCreate("doc2", x => {
                var doc = x.CreateDocument(null);
                doc.Title = "title";
                return doc;
            });
            Assert.AreEqual("title", document2.Title);
            Assert.AreEqual("doc2", document2.Id);
            Assert.AreEqual(document2, service.FindDocumentById("doc2"));
        }
        [Test]
        public void ExtenstionTests() {
            IDocumentManagerService service = new TestDocumentManagerService();
            TestDocument document1 = (TestDocument)service.CreateDocument("Type", null);
            Assert.AreEqual("Type", document1.DocumentType);
            Assert.AreEqual(null, document1.Parameter);
            Assert.AreEqual(null, document1.ParentViewModel);

            TestDocument document2 = (TestDocument)service.CreateDocument("Type", "Model");
            Assert.AreEqual("Type", document2.DocumentType);
            Assert.AreEqual("Model", document2.ViewModel);
            Assert.AreEqual(null, document2.Parameter);
            Assert.AreEqual(null, document2.ParentViewModel);


            IDocument document3 = (TestDocument)service.CreateDocument("Type", "Param", "Model");
            Assert.AreEqual("Type", ((TestDocument)document3).DocumentType);
            Assert.AreEqual(null, ((TestDocument)document3).ViewModel);
            Assert.AreEqual("Param", ((TestDocument)document3).Parameter);
            Assert.AreEqual("Model", ((TestDocument)document3).ParentViewModel);


            var byViewModel = service.GetDocumentsByParentViewModel("Model");
            Assert.AreEqual(1, byViewModel.Count());
            Assert.AreEqual(document3, byViewModel.First());

            Assert.AreEqual(document3, service.FindDocument("Param", "Model"));

            IDocument oldDocument = document3;
            service.CreateDocumentIfNotExistsAndShow(ref document3, "Type", "X", "Y", "title");
            Assert.AreSame(oldDocument, document3);

            IDocument newDocument = null;
            service.CreateDocumentIfNotExistsAndShow(ref newDocument, "Type", "X", "Y", "title");
            Assert.AreEqual("Type", ((TestDocument)newDocument).DocumentType);
            Assert.AreEqual("X", ((TestDocument)newDocument).Parameter);
            Assert.AreEqual("Y", ((TestDocument)newDocument).ParentViewModel);

            Assert.IsNull(service.FindDocument("content"));
            document1.Content = "content";
            Assert.AreEqual(document1, service.FindDocument("content"));

            var viewModel = new TestViewModel();
            var documentByViewModel = service.CreateDocument(viewModel);
            Assert.AreEqual(null, ((TestDocument)documentByViewModel).DocumentType);
            Assert.AreEqual(null, ((TestDocument)documentByViewModel).Parameter);
            Assert.AreEqual(viewModel, ((TestDocument)documentByViewModel).ViewModel);
            Assert.AreEqual(null, ((TestDocument)documentByViewModel).ParentViewModel);
        }
        [Test]
        public void NullService() {
            IDocumentManagerService service = null;
            IDocument newDocument = null;
            Assert.Throws<ArgumentNullException>(() => { service.CreateDocumentIfNotExistsAndShow(ref newDocument, "Type", "X", "Y", "title"); });
            Assert.Throws<ArgumentNullException>(() => { service.CreateDocument(new TestViewModel()); });
            Assert.Throws<ArgumentNullException>(() => { service.FindDocument("X", "Y"); });
            Assert.Throws<ArgumentNullException>(() => { service.GetDocumentsByParentViewModel("X"); });
        }
        [Test]
        public void SetTitleBindingTest() {
            var viewModel = ViewModelSource.Create(() => new TestDocumentContent());
            viewModel.Title = "a";
            IDocumentManagerService service = new TestDocumentManagerService();
            IDocument document = service.CreateDocument("doc", viewModel);
            Assert.AreEqual("a", document.Title);
            viewModel.Title = "b";
            Assert.AreEqual("b", document.Title);
        }
    }
}