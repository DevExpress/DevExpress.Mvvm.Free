using NUnit.Framework;
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
            if(((Panel)AssociatedObject).Children.Contains((UIElement)view)) return;
            ((Panel)AssociatedObject).Children.Add((UIElement)view);
        }
    }
    [TestFixture]
    public class ViewHelperTests : BaseWpfFixture {
        public class TestViewLocator : IViewLocator {
            Grid testView = null;
            Grid testViewWithViewModel = null;
            public bool AllowCaching = false;
            public object ResolveView(string name) {
                if(name == "testView")
                    return CreateTestView();
                if(name == "testViewWithViewModel")
                    return CreateTestViewWithViewModel();
                throw new NotImplementedException();
            }
            Grid CreateTestView() {
                if(!AllowCaching) return new Grid();
                return testView ?? (testView = new Grid());
            }
            Grid CreateTestViewWithViewModel() {
                if(!AllowCaching) return new Grid() { DataContext = new ChildViewModel() };
                return testViewWithViewModel ?? (testViewWithViewModel = new Grid() { DataContext = new ChildViewModel() });
            }


            public Type ResolveViewType(string name) {
                throw new NotImplementedException();
            }
            string IViewLocator.GetViewTypeName(Type type) {
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

        void Init_ViewModelExtensionsWithService_TestView(out ITestViewServiceBase service, out ChildViewModel childVM, out ParentViewModel parentVM) {
            parentVM = new ParentViewModel() { TestParameter = "parameter" };
            childVM = new ChildViewModel();
            Grid parentView = new Grid();
            service = new TestViewServiceBase();
            Interactivity.Interaction.GetBehaviors(parentView).Add((TestViewServiceBase)service);
            Window.Content = parentView;
        }
        void FinalCheck_ViewModelExtensionsWithService_TestView(ChildViewModel childVM, ParentViewModel parentVM) {
            Grid parentView = (Grid)Window.Content;
            Grid childView = parentView.Children[0] as Grid;
            Assert.AreSame(childVM, childView.DataContext);
            Assert.AreSame(parentVM, ((ISupportParentViewModel)childVM).ParentViewModel);
            Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childVM).Parameter);
        }
        [Test, Asynchronous]
        public void ViewModelExtensionsWithService1() {
            ParentViewModel parentVM; ChildViewModel childVM;
            ITestViewServiceBase iService;
            Init_ViewModelExtensionsWithService_TestView(out iService, out childVM, out parentVM);
            EnqueueShowWindow();
            EnqueueCallback(() => {
                ((ISupportParameter)childVM).Parameter = parentVM.TestParameter;
                ((ISupportParentViewModel)childVM).ParentViewModel = parentVM;
                iService.CreateChildView("testView", childVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                FinalCheck_ViewModelExtensionsWithService_TestView(childVM, parentVM);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void ViewModelExtensionsWithService3() {
            ParentViewModel parentVM; ChildViewModel childVM;
            ITestViewServiceBase iService;
            Init_ViewModelExtensionsWithService_TestView(out iService, out childVM, out parentVM);
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.CreateChildView("testView", childVM, parentVM.TestParameter, parentVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                FinalCheck_ViewModelExtensionsWithService_TestView(childVM, parentVM);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void ViewModelExtensionsWithService4_T155859() {
            ParentViewModel parentVM; ChildViewModel childVM;
            ITestViewServiceBase iService;
            Init_ViewModelExtensionsWithService_TestView(out iService, out childVM, out parentVM);
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.CreateChildView("testView", childVM, parentVM.TestParameter, parentVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                parentVM.TestParameter = "parameter2";
                iService.CreateChildView("testView", childVM, parentVM.TestParameter, parentVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                FinalCheck_ViewModelExtensionsWithService_TestView(childVM, parentVM);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void ViewModelExtensionsWithService5_T155859() {
            ParentViewModel parentVM; ChildViewModel childVM;
            ITestViewServiceBase iService;
            Init_ViewModelExtensionsWithService_TestView(out iService, out childVM, out parentVM);
            (((TestViewServiceBase)iService).ViewLocator as TestViewLocator).AllowCaching = true;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.CreateChildView("testView", childVM, parentVM.TestParameter, parentVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                parentVM.TestParameter = "parameter2";
                iService.CreateChildView("testView", childVM, parentVM.TestParameter, parentVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                FinalCheck_ViewModelExtensionsWithService_TestView(childVM, parentVM);
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
        public void ViewModelExtensionsWithService6_T155859() {
            var parentVM = new ParentViewModel() { TestParameter = "parameter" };
            ChildViewModel childVM = new ChildViewModel();
            Grid parentView = new Grid();
            Grid childView;
            TestViewServiceBase service = new TestViewServiceBase();
            ((TestViewLocator)service.ViewLocator).AllowCaching = true;
            ITestViewServiceBase iService = service;
            Interactivity.Interaction.GetBehaviors(parentView).Add(service);
            Window.Content = parentView;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                iService.CreateChildView("testViewWithViewModel", null, parentVM.TestParameter, parentVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                parentVM.TestParameter = "parameter2";
                iService.CreateChildView("testViewWithViewModel", null, parentVM.TestParameter, parentVM);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                childView = parentView.Children[0] as Grid;
                Assert.AreSame(parentVM, ((ISupportParentViewModel)childView.DataContext).ParentViewModel);
                Assert.AreEqual(parentVM.TestParameter, ((ISupportParameter)childView.DataContext).Parameter);
            });
            EnqueueTestComplete();
        }
    }
}