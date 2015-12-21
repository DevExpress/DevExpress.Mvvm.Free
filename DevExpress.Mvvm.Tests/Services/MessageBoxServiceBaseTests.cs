using NUnit.Framework;
using System;
using System.Windows;
using Moq;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class MessageBoxServiceTests {
        [Test]
        public void MessageBoxExtensionsTest() {
            var mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", string.Empty, MessageButton.OK, MessageIcon.None, MessageResult.None)).
                Returns((string text, string caption, MessageButton button, MessageIcon image, MessageResult none) => { return MessageResult.OK; });
            Assert.AreEqual(true, mock.Object.ShowMessage("Test"));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageButton.OK, MessageIcon.None, MessageResult.None)).
                Returns((string text, string caption, MessageButton button, MessageIcon image, MessageResult none) => { return MessageResult.OK; });
            Assert.AreEqual(true, mock.Object.ShowMessage("Test", "Caption"));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageButton.OKCancel, MessageIcon.None, MessageResult.None)).
                Returns((string text, string caption, MessageButton button, MessageIcon image, MessageResult none) => { return MessageResult.Cancel; });
            Assert.AreEqual(MessageResult.Cancel, mock.Object.ShowMessage("Test", "Caption", MessageButton.OKCancel));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageButton.YesNoCancel, MessageIcon.Warning, MessageResult.None)).
                Returns((string text, string caption, MessageButton button, MessageIcon image, MessageResult none) => { return MessageResult.No; });
            Assert.AreEqual(MessageResult.No, mock.Object.ShowMessage("Test", "Caption", MessageButton.YesNoCancel, MessageIcon.Warning));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageButton.YesNoCancel, MessageIcon.Warning, MessageResult.Yes)).
                Returns((string text, string caption, MessageButton button, MessageIcon image, MessageResult none) => { return MessageResult.Yes; });
            Assert.AreEqual(MessageResult.Yes, mock.Object.ShowMessage("Test", "Caption", MessageButton.YesNoCancel, MessageIcon.Warning, MessageResult.Yes));
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullService1() {
            IMessageBoxService service = null;
            service.Show("");
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullService2() {
            IMessageBoxService service = null;
            service.Show("", "");
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullService3() {
            IMessageBoxService service = null;
            service.ShowMessage("", "", MessageButton.OKCancel);
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullService4() {
            IMessageBoxService service = null;
            service.ShowMessage("", "", MessageButton.OKCancel, MessageIcon.Warning);
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullService11() {
            IMessageBoxService service = null;
            service.ShowMessage("");
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullService12() {
            IMessageBoxService service = null;
            service.ShowMessage("", "");
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullService13() {
            IMessageBoxService service = null;
            service.ShowMessage("", "", MessageButton.OKCancel);
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullService14() {
            IMessageBoxService service = null;
            service.ShowMessage("", "", MessageButton.OKCancel, MessageIcon.Warning);
        }
    }
}