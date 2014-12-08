#if SILVERLIGHT
#elif NETFX_CORE
using DevExpress.TestFramework.NUnit;
#else
using NUnit.Framework;
#endif
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using DevExpress.Mvvm.UI.Interactivity;
using System.Linq;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Data;
using Moq;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class ViewModelExtensionsTests {
        class ViewModel : ViewModelBase, IDocumentContent {
            public IDocumentOwner DocumentOwner { get; set; }
            public void OnClose(CancelEventArgs e) {
                throw new NotImplementedException();
            }

            object title = "title";
            public object Title{
                get { return title; }
                set { SetProperty(ref title, value, () => Title); }
            }
            void IDocumentContent.OnDestroy() { }
        }
        class ViewModelWithAnotherTitle : ViewModelBase, IDocumentContent {
            public IDocumentOwner DocumentOwner { get; set; }
            public void OnClose(CancelEventArgs e) {
                throw new NotImplementedException();
            }

            object title = "bad title";
            public object Title {
                get { return title; }
                set { SetProperty(ref title, value, () => Title); }
            }
            object documentTitle = "title";
            public object DocumentTitle {
                get { return documentTitle; }
                set { SetProperty(ref documentTitle, value, () => DocumentTitle); }
            }
            object IDocumentContent.Title { get { return DocumentTitle; } }
            void IDocumentContent.OnDestroy() { }
        }
        class DObject : DependencyObject {
        }
        [Test]
        public void SetProperties() {
            var button1 = new Button();
            var parentViewModel = new ViewModel();
            var viewModel = new ViewModel();
            ViewModelExtensions.SetParameter(button1, "test");
            ViewModelExtensions.SetParentViewModel(button1, parentViewModel);

            var dObject = new DObject();
            ViewModelExtensions.SetParameter(dObject, "test");
            ViewModelExtensions.SetParentViewModel(dObject, parentViewModel);

            var button2 = new Button() { DataContext = viewModel };
            ViewModelExtensions.SetParameter(button2, "test");
            Assert.AreEqual("test", viewModel.With(x => x as ISupportParameter).Parameter);
            ViewModelExtensions.SetParentViewModel(button2, parentViewModel);
            Assert.AreEqual(parentViewModel, viewModel.With(x => x as ISupportParentViewModel).ParentViewModel);
        }
        public class DocumentViewModel : BindableBase, IDocumentContent {
            public IDocumentOwner DocumentOwner { get; set; }
            object IDocumentContent.Title {
                get { return "foo2"; }
            }
            void IDocumentContent.OnClose(CancelEventArgs e) {
                throw new NotImplementedException();
            }
            void IDocumentContent.OnDestroy() { }
        }
        [Test]
        public void ViewHelperTest() {
            var button = new Button();
            var viewModel = new ViewModel();
            var parentViewModel = new ViewModel();
            var textBlock = new TextBlock() { Text = "foo" };
            Assert.AreEqual(null, ViewHelper.GetViewModelFromView(button));
            ViewHelper.InitializeView(button, null, "test", parentViewModel);
            DocumentUIServiceBase.SetTitleBinding(button, TextBlock.TextProperty, textBlock);
            Assert.AreEqual("foo", textBlock.Text);
            button.DataContext = viewModel;
            Assert.AreEqual(viewModel, ViewHelper.GetViewModelFromView(button));
            ViewHelper.InitializeView(button, null, "test", parentViewModel);
            Assert.AreEqual("test", viewModel.With(x => x as ISupportParameter).Parameter);
            Assert.AreEqual(parentViewModel, viewModel.With(x => x as ISupportParentViewModel).ParentViewModel);
            DocumentUIServiceBase.SetTitleBinding(button, TextBlock.TextProperty, textBlock);
            Assert.AreEqual("title", textBlock.Text);
#if !SILVERLIGHT && !NETFX_CORE
            viewModel.Title = "title2";
            Assert.AreEqual("title2", textBlock.Text);
#endif
            var dObject = new DObject();
            Assert.AreEqual(null, ViewHelper.GetViewModelFromView(dObject));
            ViewHelper.InitializeView(button, null, "test", parentViewModel);

            var button2 = new Button();
            button2.DataContext = new DocumentViewModel();
            DocumentUIServiceBase.SetTitleBinding(button2, TextBlock.TextProperty, textBlock);
            Assert.AreEqual("foo2", textBlock.Text);

            ViewModelWithAnotherTitle anotherViewModel = new ViewModelWithAnotherTitle();
            ViewHelper.InitializeView(button, anotherViewModel, "test", null);
            Assert.AreEqual(anotherViewModel, button.DataContext);
            DocumentUIServiceBase.SetTitleBinding(button, TextBlock.TextProperty, textBlock);
            Assert.AreEqual("title", textBlock.Text);
            viewModel.Title = "title3";
            Assert.AreEqual("title", textBlock.Text);
        }
        [Test]
        public void ViewHelperShouldWrapDocumentOwnerIfDependencyObject_Test_T122209() {
            var button = new Button();
            var viewModel = new ViewModel();
            var parentViewModel = new ViewModel();
            IDocumentOwner owner = new TestDocumentOwner();
            button.DataContext = viewModel;
            ViewHelper.InitializeView(button, null, "test", parentViewModel, owner);
            Assert.AreEqual(owner, viewModel.DocumentOwner);
            Assert.AreEqual(owner, ViewModelExtensions.GetDocumentOwner(button));

            button = new Button();
            button.DataContext = viewModel;
            owner = new TestServiceDocumentOwner();
            ViewHelper.InitializeView(button, null, "test", parentViewModel, owner);
            ViewHelper.DocumentOwnerWrapper ownerWrapper = viewModel.DocumentOwner as ViewHelper.DocumentOwnerWrapper;
            Assert.IsNotNull(ownerWrapper);
            Assert.IsTrue(ownerWrapper.Equals(owner));
            Assert.AreEqual(owner, ownerWrapper.ActualOwner);
            Assert.AreEqual(ownerWrapper, ViewModelExtensions.GetDocumentOwner(button));
        }
        class TestViewLocator : IViewLocator {
            public FrameworkElement ResolvedView { get; private set; }
            object IViewLocator.ResolveView(string name) {
                ResolvedView = null;
                if(name == "foo") {
                    ResolvedView = new TestViewElement();
                }
                return ResolvedView;
            }
            Type IViewLocator.ResolveViewType(string name) {
                throw new NotImplementedException();
            }
        }
#if !NETFX_CORE
        [Test]
        public void ResolveViewTest() {
            var parentViewModel = new ViewModel();
            DependencyObject fallbackView;
            TextBlock fallbackElement;
#if !SILVERLIGHT
            fallbackView = (DependencyObject)ViewHelper.CreateAndInitializeView(null, "TestView", null, "param", parentViewModel);
            fallbackElement = LayoutTreeHelper.GetVisualChildren(fallbackView).OfType<TextBlock>().First();
            Assert.AreEqual("\"TestView\" type not found.", fallbackElement.Text);
            ViewLocator.Default = new ViewLocator(new[] { this.GetType().Assembly });
#endif
            try {
                var testView = ViewHelper.CreateAndInitializeView(null, "TestView", null, "param", parentViewModel);
                Assert.IsInstanceOf(typeof(TestView), testView);
                fallbackView = (DependencyObject)ViewHelper.CreateAndInitializeView(null, "foo", null, "param", parentViewModel);
                fallbackElement = LayoutTreeHelper.GetVisualChildren(fallbackView).OfType<TextBlock>().First();
                Assert.AreEqual("\"foo\" type not found.", fallbackElement.Text);

                TestViewElement testViewElement = (TestViewElement)ViewHelper.CreateAndInitializeView(null, "TestViewElement", null, "param", parentViewModel);
                ViewModelBase viewModel = ViewHelper.GetViewModelFromView(testViewElement).With(x => x as ViewModelBase);
                Assert.AreEqual("param", viewModel.With(x => x as ISupportParameter).Parameter);
                Assert.AreEqual(parentViewModel, viewModel.With(x => x as ISupportParentViewModel).ParentViewModel);

                testViewElement = (TestViewElement)ViewHelper.CreateAndInitializeView(new TestViewLocator(), "foo", null, "param", parentViewModel);
                viewModel = ViewHelper.GetViewModelFromView(testViewElement).With(x => x as ViewModelBase);
                Assert.AreEqual("param", viewModel.With(x => x as ISupportParameter).Parameter);
                Assert.AreEqual(parentViewModel, viewModel.With(x => x as ISupportParentViewModel).ParentViewModel);

                ViewLocator.Default = new TestViewLocator();
                testViewElement = (TestViewElement)ViewHelper.CreateAndInitializeView(null, "foo", null, "param", parentViewModel);
                viewModel = ViewHelper.GetViewModelFromView(testViewElement).With(x => x as ViewModelBase);
                Assert.AreEqual("param", viewModel.With(x => x as ISupportParameter).Parameter);
                Assert.AreEqual(parentViewModel, viewModel.With(x => x as ISupportParentViewModel).ParentViewModel);

                testViewElement = (TestViewElement)ViewHelper.CreateAndInitializeView(null, "foo", "param", "param", parentViewModel);
                Assert.AreEqual("param", testViewElement.DataContext);
            } finally {
                ViewLocator.Default = null;
            }
        }
#endif
#if !FREE && !NETFX_CORE
        class TestViewLocator2 : ViewLocatorBase {
            protected override IEnumerable<Assembly> Assemblies {
                get {
                    yield return typeof(DataController).Assembly;
                    yield return typeof(Button).Assembly;
                }
            }
        }
        [Test]
        public void ViewLocatorBaseTest() {
            IViewLocator locator = new TestViewLocator2();
            Button button1 = (Button)locator.ResolveView("Button");
            Button button2 = (Button)locator.ResolveView("Button");
            Assert.IsNotNull(button1);
            Assert.IsNotNull(button2);
            Assert.AreNotEqual(button1, button2);

            DataController dc1= (DataController)locator.ResolveView("DataController");
            DataController dc2 = (DataController)locator.ResolveView("DataController");
            Assert.IsNotNull(dc1);
            Assert.IsNotNull(dc2);
            Assert.AreNotEqual(dc1, dc2);

            locator = new ViewLocator(typeof(DataController).Assembly, typeof(Button).Assembly);
            button1 = (Button)locator.ResolveView("Button");
            button2 = (Button)locator.ResolveView("Button");
            Assert.IsNotNull(button1);
            Assert.IsNotNull(button2);
            Assert.AreNotEqual(button1, button2);

            dc1 = (DataController)locator.ResolveView("DataController");
            dc2 = (DataController)locator.ResolveView("DataController");
            Assert.IsNotNull(dc1);
            Assert.IsNotNull(dc2);
            Assert.AreNotEqual(dc1, dc2);
        }
#endif
        [Test]
        public void CreateViewThrowExceptionOnNullArgumentsTest() {
            Action<InvalidOperationException> checkExceptionAction = x => {
                Assert.IsTrue(x.Message.Contains(ViewHelper.Error_CreateViewMissArguments));
                Assert.IsTrue(x.Message.Contains(ViewHelper.HelpLink_CreateViewMissArguments));
            };
            AssertHelper.AssertThrows<InvalidOperationException>(() => ViewHelper.CreateView(ViewLocator.Default, null), checkExceptionAction);
            AssertHelper.AssertThrows<InvalidOperationException>(() => ViewHelper.CreateAndInitializeView(viewLocator: ViewLocator.Default, documentType: null, viewModel: null, parameter: null, parentViewModel: null, viewTemplate: null, viewTemplateSelector: null), checkExceptionAction);
            AssertHelper.AssertThrows<InvalidOperationException>(() => ViewHelper.CreateAndInitializeView(viewLocator: ViewLocator.Default, documentType: null, viewModel: null, parameter: null, parentViewModel: null, documentOwner: null, viewTemplate: null, viewTemplateSelector: null), checkExceptionAction);
        }
        [Test]
        public void CreateAndInitializeViewTest1() {
            TestView viewModel = new TestView();
            TestView parentViewModel = new TestView();
            TestView currentViewModel = null;
            TestViewLocator locator = new TestViewLocator();
            ViewLocator.Default = locator;

            ViewHelper.CreateAndInitializeView(ViewLocator.Default, "foo", viewModel, null, null);
            currentViewModel = (TestView)locator.ResolvedView.DataContext;
            Assert.AreEqual(currentViewModel, viewModel);

            ViewHelper.CreateAndInitializeView(ViewLocator.Default, "foo", viewModel, "1", parentViewModel);
            currentViewModel = (TestView)locator.ResolvedView.DataContext;
            Assert.AreEqual(currentViewModel, viewModel);
            Assert.AreEqual(((ISupportParameter)currentViewModel).Parameter, "1");
            Assert.AreEqual(((ISupportParentViewModel)currentViewModel).ParentViewModel, parentViewModel);

            ViewHelper.CreateAndInitializeView(ViewLocator.Default, "foo", null, "1", parentViewModel);
            currentViewModel = (TestView)locator.ResolvedView.DataContext;
            Assert.AreNotEqual(currentViewModel, viewModel);
            Assert.AreEqual(((ISupportParameter)currentViewModel).Parameter, "1");
            Assert.AreEqual(((ISupportParentViewModel)currentViewModel).ParentViewModel, parentViewModel);
            ViewLocator.Default = null;
        }

        [Test]
        public void TestQ536034() {
            object parameter = 1;

            TestViewElement view = new TestViewElement() { DataContext = null };
            BindingOperations.SetBinding(view, ViewModelExtensions.ParameterProperty, new Binding() { Source = parameter });
            TestView viewModel = new TestView();
            view.DataContext = viewModel;
            Assert.AreEqual(1, ((ISupportParameter)viewModel).Parameter);

            viewModel = new TestView();
            view.DataContext = viewModel;
            Assert.AreEqual(1, ((ISupportParameter)viewModel).Parameter);
            parameter = 2;

            ViewModelExtensions.SetParameter(view, 2);
            Assert.AreEqual(2, ((ISupportParameter)viewModel).Parameter);

            Assert.AreEqual(1, Interaction.GetBehaviors(view).Count);
        }
#if !NETFX_CORE
        [Test]
        public void SetParameterTest() {
            var viewModel = new Mock<ISupportParameter>(MockBehavior.Strict);
            viewModel.SetupSet(x => x.Parameter = It.IsAny<object>()).Verifiable();
            Button button = new Button() { DataContext = viewModel.Object };
            ViewModelExtensions.SetParameter(button, 13);
            viewModel.VerifySet(x => x.Parameter = 13, Times.Once());
        }
        [Test]
        public void SetParentViewModelTest() {
            var viewModel = new Mock<ISupportParentViewModel>(MockBehavior.Strict);
            viewModel.SetupSet(x => x.ParentViewModel = It.IsAny<object>()).Verifiable();
            Button button = new Button() { DataContext = viewModel.Object };
            ViewModelExtensions.SetParentViewModel(button, 13);
            viewModel.VerifySet(x => x.ParentViewModel = 13, Times.Once());
        }
        [Test]
        public void SetDocumentOwnerTest() {
            var viewModel = new Mock<IDocumentContent>(MockBehavior.Strict);
            viewModel.SetupSet(x => x.DocumentOwner = It.IsAny<IDocumentOwner>()).Verifiable();
            Button button = new Button() { DataContext = viewModel.Object };
            var documentOwner = new Mock<IDocumentOwner>(MockBehavior.Strict);
            ViewModelExtensions.SetDocumentOwner(button, documentOwner.Object);
            viewModel.VerifySet(x => x.DocumentOwner = documentOwner.Object, Times.Once());
        }
#endif
    }
    public class TestView : ViewModelBase {
    }
    class TestServiceDocumentOwner : ServiceBase, IDocumentOwner {
        public void Close(IDocumentContent documentContent, bool force = true) {
            throw new NotImplementedException();
        }
    }
    class TestDocumentOwner : IDocumentOwner {
        public void Close(IDocumentContent documentContent, bool force = true) {
            throw new NotImplementedException();
        }
    }
    public class TestViewElement : FrameworkElement {
        public TestViewElement() {
            DataContext = new TestView();
        }
    }
}