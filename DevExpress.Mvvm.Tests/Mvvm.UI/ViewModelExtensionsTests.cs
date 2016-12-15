using System.Windows.Controls;
using System.Windows.Data;
using NUnit.Framework;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Linq;

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
        [Test]
        public void OldScaffoldedCode_SetParameterTest_T191302() {
            var viewModel = new ViewModelSupportedParameter();
            var button = new Button() { DataContext = viewModel };
            ViewModelExtensions.SetParameter(button, null);
            Assert.IsTrue(viewModel.ParameterChanged);
        }
        public class ViewModelSupportedParameter : ISupportParameter {
            public bool ParameterChanged;
            object ISupportParameter.Parameter {
                get { return null; }
                set {
                    ParameterChanged = true;
                }
            }
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
            viewModel.Title = "title2";
            Assert.AreEqual("title2", textBlock.Text);
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
            string IViewLocator.GetViewTypeName(Type type) {
                throw new NotImplementedException();
            }
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
        [Test]
        public void ResolveViewTest() {
            var parentViewModel = new ViewModel();
            DependencyObject fallbackView;
            TextBlock fallbackElement;
            fallbackView = (DependencyObject)ViewHelper.CreateAndInitializeView(null, "TestView", null, "param", parentViewModel);
            fallbackElement = LayoutTreeHelper.GetVisualChildren(fallbackView).OfType<TextBlock>().First();
            Assert.AreEqual("\"TestView\" type not found.", fallbackElement.Text);
            ViewLocator.Default = new ViewLocator(new[] { this.GetType().Assembly });
            try {
                var testView = ViewHelper.CreateAndInitializeView(null, "TestView", null, "param", parentViewModel);
                AssertHelper.IsInstanceOf(typeof(TestView), testView);
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
        [Test]
        public void CreateViewThrowExceptionOnNullArgumentsTest() {
            Action<InvalidOperationException> checkExceptionAction = x => {
                Assert.IsTrue(x.Message.Contains(ViewHelper.Error_CreateViewMissArguments));
                Assert.IsTrue(x.Message.Contains(ViewHelper.HelpLink_CreateViewMissArguments));
                Assert.AreEqual(ViewHelper.HelpLink_CreateViewMissArguments, x.HelpLink);
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
        class TestVM : ISupportParameter, ISupportParentViewModel, IDocumentContent {
            public int ParameterChangingCounter { get; private set; }
            public int ParentViewModelChangingCounter { get; private set; }
            public int DocumentOwnerChangingCounter { get; private set; }
            public object Parameter { get { return parameter; } set { ParameterChangingCounter++; parameter = value; } }
            public object ParentViewModel { get { return parentViewModel; } set { ParentViewModelChangingCounter++; parentViewModel = value; } }
            public IDocumentOwner DocumentOwner { get { return documentOwner; } set { DocumentOwnerChangingCounter++; documentOwner = value; } }
            object parameter;
            object parentViewModel;
            IDocumentOwner documentOwner;

            public object Title { get { throw new NotImplementedException(); } }
            public void OnClose(CancelEventArgs e) { throw new NotImplementedException(); }
            public void OnDestroy() { throw new NotImplementedException(); }
        }
        class TestDocumentOwner : IDocumentOwner {
            public void Close(IDocumentContent documentContent, bool force = true) { throw new NotImplementedException(); }
        }
        [Test]
        public void SetParameterTest() {
            var viewModel = new TestVM();
            Button button = new Button() { DataContext = viewModel };
            ViewModelExtensions.SetParameter(button, 13);
            Assert.AreEqual(13, viewModel.Parameter);
            Assert.AreEqual(1, viewModel.ParameterChangingCounter);
        }
        [Test]
        public void SetParentViewModelTest() {
            var viewModel = new TestVM();
            Button button = new Button() { DataContext = viewModel };
            ViewModelExtensions.SetParentViewModel(button, 13);
            Assert.AreEqual(13, viewModel.ParentViewModel);
            Assert.AreEqual(1, viewModel.ParentViewModelChangingCounter);
        }
        [Test]
        public void SetDocumentOwnerTest() {
            var viewModel = new TestVM();
            Button button = new Button() { DataContext = viewModel };
            var documentOwner = new TestDocumentOwner();
            ViewModelExtensions.SetDocumentOwner(button, documentOwner);
            Assert.AreSame(documentOwner, viewModel.DocumentOwner);
            Assert.AreEqual(1, viewModel.DocumentOwnerChangingCounter);
        }

        [Test]
        public void T370425() {
            var vm1 = new ViewModel();
            var vm2 = new ViewModel();
            var vm3 = new ViewModel();
            var vm4 = new ViewModel();
            ViewHelper.InitializeView(null, vm2, null, vm1);
            ViewHelper.InitializeView(null, vm3, null, vm2);
            ViewHelper.InitializeView(null, vm4, null, vm3);
            ViewHelper.InitializeView(null, vm2, null, vm4);

            Assert.AreEqual(vm1, ((ISupportParentViewModel)vm2).ParentViewModel);
            Assert.AreEqual(vm2, ((ISupportParentViewModel)vm3).ParentViewModel);
            Assert.AreEqual(vm3, ((ISupportParentViewModel)vm4).ParentViewModel);
        }
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