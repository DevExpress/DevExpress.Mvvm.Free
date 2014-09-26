#if !SILVERLIGHT
using NUnit.Framework;
#else
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System.Windows.Controls;
using System.Threading.Tasks;
using System;
using System.Windows.Media;
using System.Threading;
using System.Windows.Data;
using System.Windows;
using System.ComponentModel;

namespace DevExpress.Mvvm.UI.Tests {
    public interface ITestViewServiceBase {
        [EditorBrowsable(EditorBrowsableState.Never)]
        void CreateChildView(string documentType, object viewModel, object parameter, object parentViewModel);
    }
    public static class TestViewServiceBaseExtensions {
        public static void CreateChildView(this ITestViewServiceBase service, object viewModel) {
            service.CreateChildView(null, viewModel, null, null);
        }
        public static void CreateChildView(this ITestViewServiceBase service, string documentType, object viewModel) {
            service.CreateChildView(documentType, viewModel, null, null);
        }
        public static void CreateChildView(this ITestViewServiceBase service, string documentType, object parameter, object parentViewModel) {
            service.CreateChildView(documentType, null, parameter, parentViewModel);
        }
    }
    public class TestViewServiceBase : ViewServiceBase, ITestViewServiceBase {
        public TestViewServiceBase() {
            ViewLocator = new ViewHelperTests.TestViewLocator();
        }

        void ITestViewServiceBase.CreateChildView(string documentType, object viewModel, object parameter, object parentViewModel) {
            var view = base.CreateAndInitializeView(documentType, viewModel, parameter, parentViewModel);
            ((Panel)AssociatedObject).Children.Add((UIElement)view);
        }
    }
    [TestFixture]
    public class ViewHelperTests : BaseWpfFixture {
        public class TestViewLocator : IViewLocator {
            public object ResolveView(string name) {
                if(name == "testView")
                    return new Grid();
                if(name == "testViewWithViewModel")
                    return new Grid() { DataContext = new ChildViewModel() };
                throw new NotImplementedException();
            }
        }
        public class ParentViewModel : ViewModelBase {
            public object TestParameter {
                get { return GetProperty(() => TestParameter); }
                set { SetProperty(() => TestParameter, value); }
            }
        }
        public class ChildViewModel : ViewModelBase {
        }


        [Test, Asynchronous]
        public void ViewModelExtensions1() {
            var parentVM = new ParentViewModel() { TestParameter = "parameter" };
            var childVM = new ChildViewModel();
            Grid parentView = new Grid() { DataContext = parentVM };
            Grid childView = new Grid() { DataContext = childVM };
            childView.SetBinding(ViewModelExtensions.ParameterProperty, new Binding("DataContext.TestParameter") { Source = parentView });
            childView.SetBinding(ViewModelExtensions.ParentViewModelProperty, new Binding("DataContext") { Source = parentView });
            parentView.Children.Add(childView);
            Window.Content = parentView;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
                parentVM.TestParameter = "parameter2";
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
                parentVM = new ParentViewModel() { TestParameter = "VM2test" };
                parentView.DataContext = parentVM;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
                parentVM.TestParameter = "parameter2";
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void ViewModelExtensions2() {
            var parentVM = new ParentViewModel() { TestParameter = "parameter" };
            var childVM = new ChildViewModel();
            Grid parentView = new Grid();
            Grid childView = new Grid();
            childView.SetBinding(ViewModelExtensions.ParameterProperty, new Binding("DataContext.TestParameter") { Source = parentView });
            childView.SetBinding(ViewModelExtensions.ParentViewModelProperty, new Binding("DataContext") { Source = parentView });
            Window.Content = parentView;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                parentView.Children.Add(childView);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                childView.DataContext = childVM;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                parentView.DataContext = parentVM;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
                parentVM.TestParameter = "parameter2";
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
                parentVM = new ParentViewModel() { TestParameter = "VM2test" };
                parentView.DataContext = parentVM;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
                parentVM.TestParameter = "parameter2";
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
            });
            EnqueueTestComplete();
        }


        [Test, Asynchronous]
        public void ViewModelExtensionsWithService1() {
            var parentVM = new ParentViewModel() { TestParameter = "parameter" };
            var childVM = new ChildViewModel();
            Grid parentView = new Grid();
            Grid childView;
            TestViewServiceBase service = new TestViewServiceBase();
            ITestViewServiceBase iService = service;
            Interactivity.Interaction.GetBehaviors(parentView).Add(service);
            Window.Content = parentView;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                ((ISupportParameter)childVM).Parameter = parentVM.TestParameter;
                ((ISupportParentViewModel)childVM).ParentViewModel = parentVM;
                iService.CreateChildView("testView", childVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                childView = parentView.Children[0] as Grid;
                Assert.AreSame(childVM, childView.DataContext);
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void ViewModelExtensionsWithService2() {
            var parentVM = new ParentViewModel() { TestParameter = "parameter" };
            ChildViewModel childVM;
            Grid parentView = new Grid();
            Grid childView;
            TestViewServiceBase service = new TestViewServiceBase();
            ITestViewServiceBase iService = service;
            Interactivity.Interaction.GetBehaviors(parentView).Add(service);
            Window.Content = parentView;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.CreateChildView("testViewWithViewModel", parentVM.TestParameter, parentVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                childView = parentView.Children[0] as Grid;
                childVM = childView.DataContext as ChildViewModel;
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void ViewModelExtensionsWithService3() {
            var parentVM = new ParentViewModel() { TestParameter = "parameter" };
            ChildViewModel childVM = new ChildViewModel();
            Grid parentView = new Grid();
            Grid childView;
            TestViewServiceBase service = new TestViewServiceBase();
            ITestViewServiceBase iService = service;
            Interactivity.Interaction.GetBehaviors(parentView).Add(service);
            Window.Content = parentView;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.CreateChildView("testView", childVM, parentVM.TestParameter, parentVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                childView = parentView.Children[0] as Grid;
                Assert.AreEqual(childVM, childView.DataContext);
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
            });
            EnqueueTestComplete();
        }
    }
}