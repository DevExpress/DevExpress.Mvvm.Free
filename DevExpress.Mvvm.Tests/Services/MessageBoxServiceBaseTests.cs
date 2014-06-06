#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
#endif
using System;
using System.Windows;
using Moq;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class MessageBoxServiceTests {
        [Test]
        public void MessageBoxExtensionsTest() {
#if !SILVERLIGHT
            var mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult none) => { return MessageBoxResult.OK; });
            Assert.AreEqual(MessageBoxResult.OK, mock.Object.Show("Test"));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult none) => { return MessageBoxResult.OK; });
            Assert.AreEqual(MessageBoxResult.OK, mock.Object.Show("Test", "Caption"));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageBoxButton.OKCancel, MessageBoxImage.None, MessageBoxResult.None)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult none) => { return MessageBoxResult.Cancel; });
            Assert.AreEqual(MessageBoxResult.Cancel, mock.Object.Show("Test", "Caption", MessageBoxButton.OKCancel));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.None)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult none) => { return MessageBoxResult.No; });
            Assert.AreEqual(MessageBoxResult.No, mock.Object.Show("Test", "Caption", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult none) => { return MessageBoxResult.Yes; });
            Assert.AreEqual(MessageBoxResult.Yes, mock.Object.Show("Test", "Caption", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes));
#else
            var mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", string.Empty, MessageBoxButton.OK, MessageBoxResult.None)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxResult none) => { return MessageBoxResult.OK; });
            Assert.AreEqual(MessageBoxResult.OK, mock.Object.Show("Test"));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageBoxButton.OK, MessageBoxResult.None)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxResult none) => { return MessageBoxResult.OK; });
            Assert.AreEqual(MessageBoxResult.OK, mock.Object.Show("Test", "Caption"));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageBoxButton.OKCancel, MessageBoxResult.None)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxResult none) => { return MessageBoxResult.Cancel; });
            Assert.AreEqual(MessageBoxResult.Cancel, mock.Object.Show("Test", "Caption", MessageBoxButton.OKCancel));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageBoxButton.OKCancel, MessageBoxResult.None)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxResult none) => { return MessageBoxResult.No; });
            Assert.AreEqual(MessageBoxResult.No, mock.Object.Show("Test", "Caption", MessageBoxButton.OKCancel));

            mock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            mock.Setup(foo => foo.Show("Test", "Caption", MessageBoxButton.OKCancel, MessageBoxResult.Yes)).
                Returns((string text, string caption, MessageBoxButton button, MessageBoxResult none) => { return MessageBoxResult.Yes; });
            Assert.AreEqual(MessageBoxResult.Yes, mock.Object.Show("Test", "Caption", MessageBoxButton.OKCancel, MessageBoxResult.Yes));
#endif
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
            service.Show("", "", MessageBoxButton.OKCancel);
        }
#if !SILVERLIGHT
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullService4() {
            IMessageBoxService service = null;
            service.Show("", "", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        }
#endif
    }
}