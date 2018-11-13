using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using DevExpress.Mvvm.Xpf;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class DialogServiceTests {
        public class TestDialogService : IDialogService {
            public UICommand ShowDialog(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel) {
                ViewModel = viewModel;
                ParentViewModel = parentViewModel;
                Parameter = parameter;
                DocumentType = documentType;
                Title = title;
                Commands = dialogCommands;
                return null;
            }

            public IEnumerable<UICommand> Commands { get; set; }
            public string Title { get; set; }
            public string DocumentType { get; set; }
            public object ViewModel { get; set; }
            public object Parameter { get; set; }
            public object ParentViewModel { get; set; }
        }
        [Test]
        public void ExtenstionTests1() {
            var service = new TestDialogService();
            var viewModel = new TestViewModel();
            var parentViewModel = new TestViewModel();

            service = new TestDialogService();
            service.ShowDialog(MessageBoxButton.OKCancel, "title1", viewModel);
            Assert.AreEqual(2, service.Commands.Count());
            Assert.AreEqual(MessageBoxResult.OK, service.Commands.ElementAt(0).Tag);
            Assert.AreEqual(MessageBoxResult.Cancel, service.Commands.ElementAt(1).Tag);
            Assert.AreEqual("title1", service.Title);
            Assert.AreEqual(null, service.DocumentType);
            Assert.AreEqual(viewModel, service.ViewModel);
            Assert.AreEqual(null, service.Parameter);
            Assert.AreEqual(null, service.ParentViewModel);

            service = new TestDialogService();
            service.ShowDialog(MessageBoxButton.OK, "title2", "docType1", viewModel);
            Assert.AreEqual(1, service.Commands.Count());
            Assert.AreEqual(MessageBoxResult.OK, service.Commands.ElementAt(0).Tag);
            Assert.AreEqual("title2", service.Title);
            Assert.AreEqual("docType1", service.DocumentType);
            Assert.AreEqual(null, service.Parameter);
            Assert.AreEqual(viewModel, service.ViewModel);
            Assert.AreEqual(null, service.ParentViewModel);

            service = new TestDialogService();
            service.ShowDialog(MessageBoxButton.YesNo, "title3", "docType2", "param", parentViewModel);
            Assert.AreEqual(2, service.Commands.Count());
            Assert.AreEqual(MessageBoxResult.Yes, service.Commands.ElementAt(0).Tag);
            Assert.AreEqual(MessageBoxResult.No, service.Commands.ElementAt(1).Tag);
            Assert.AreEqual("title3", service.Title);
            Assert.AreEqual("docType2", service.DocumentType);
            Assert.AreEqual("param", service.Parameter);
            Assert.AreEqual(parentViewModel, service.ParentViewModel);
            Assert.AreEqual(null, service.ViewModel);
        }
        [Test]
        public void ExtenstionTest2() {
            var service = new TestDialogService();
            var viewModel = new TestViewModel();
            var parentViewModel = new TestViewModel();
            var commands = new List<UICommand>();

            service = new TestDialogService();
            service.ShowDialog(commands, "title1", viewModel);
            Assert.AreEqual(commands, service.Commands);
            Assert.AreEqual("title1", service.Title);
            Assert.AreEqual(null, service.DocumentType);
            Assert.AreEqual(null, service.Parameter);
            Assert.AreEqual(viewModel, service.ViewModel);
            Assert.AreEqual(null, service.ParentViewModel);

            service = new TestDialogService();
            commands = new List<UICommand>();
            service.ShowDialog(commands, "title1", "docType", viewModel);
            Assert.AreEqual(commands, service.Commands);
            Assert.AreEqual("title1", service.Title);
            Assert.AreEqual("docType", service.DocumentType);
            Assert.AreEqual(null, service.Parameter);
            Assert.AreEqual(viewModel, service.ViewModel);
            Assert.AreEqual(null, service.ParentViewModel);


            service = new TestDialogService();
            service.ShowDialog(commands, "title3", "docType2", "param", parentViewModel);
            Assert.AreEqual(commands, service.Commands);
            Assert.AreEqual("title3", service.Title);
            Assert.AreEqual("docType2", service.DocumentType);
            Assert.AreEqual("param", service.Parameter);
            Assert.AreEqual(null, service.ViewModel);
            Assert.AreEqual(parentViewModel, service.ParentViewModel);
        }

        [Test]
        public void NullService() {
            IDialogService service = null;
            Assert.Throws<ArgumentNullException>(() => { service.ShowDialog(MessageBoxButton.OK, "Title", new TestViewModel()); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowDialog(MessageBoxButton.OK, "Title", "docType", new TestViewModel()); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowDialog(MessageBoxButton.OK, "Title", "docType", 1, new TestViewModel()); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowDialog(new List<UICommand>(), "Title", new TestViewModel()); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowDialog(new List<UICommand>(), "Title", "docType", new TestViewModel()); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowDialog(new List<UICommand>(), "Title", "docType", 1, new TestViewModel()); });
        }
        public class TestDialogService2 : TestDialogService, IMessageBoxButtonLocalizer {
            public string Localize(MessageBoxResult button) {
                return "test";
            }
        }
        [Test]
        public void CustomLocalizerTest() {
            var service = new TestDialogService2();
            service.ShowDialog(MessageButton.OKCancel, "test", "test");
            Assert.AreEqual("test", service.Commands.ElementAt(0).Caption);
            Assert.AreEqual("test", service.Commands.ElementAt(1).Caption);
        }
    }
}