#if !SILVERLIGHT
using NUnit.Framework;
#endif
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Mvvm.UI.Interactivity;

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
#if !SILVERLIGHT
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
        class TestViewLocator : IViewLocator {
            public FrameworkElement ResolvedView { get; private set; }
            object IViewLocator.ResolveView(string name) {
                ResolvedView = null;
                if(name == "foo") {
                    ResolvedView = new TestViewElement();
                }
                return ResolvedView;
            }
        }
        [Test]
        public void ResolveViewTest() {
            var parentViewModel = new ViewModel();
            ContentPresenter fallbackView;
#if !SILVERLIGHT
            fallbackView = (ContentPresenter)ViewHelper.CreateAndInitializeView(null, "TestView", null, "param", parentViewModel);
            Assert.AreEqual("\"TestView\" type not found.", ((TextBlock)fallbackView.Content).Text);
            Assert.IsNull(ViewLocator.Default);
            ViewLocator.Default = new ViewLocator(new[] { this.GetType().Assembly });
#endif
            try {
                var testView = ViewHelper.CreateAndInitializeView(null, "TestView", null, "param", parentViewModel);
                Assert.IsInstanceOf(typeof(TestView), testView);
                fallbackView = (ContentPresenter)ViewHelper.CreateAndInitializeView(null, "foo", null, "param", parentViewModel);
                Assert.AreEqual("\"foo\" type not found.", ((TextBlock)fallbackView.Content).Text);

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
    }
    public class TestView : ViewModelBase {
    }
    public class TestViewElement : FrameworkElement {
        public TestViewElement() {
            DataContext = new TestView();
        }
    }
}