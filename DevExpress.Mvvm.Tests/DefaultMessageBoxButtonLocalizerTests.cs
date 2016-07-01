using System.Windows;
using NUnit.Framework;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class DefaultMessageButtonLocalizerTests {
        [Test]
        public void Test() {
            DefaultMessageButtonLocalizer localizer = new DefaultMessageButtonLocalizer();
            Assert.AreEqual("OK", localizer.Localize(MessageResult.OK));
            Assert.AreEqual("Cancel", localizer.Localize(MessageResult.Cancel));
            Assert.AreEqual("Yes", localizer.Localize(MessageResult.Yes));
            Assert.AreEqual("No", localizer.Localize(MessageResult.No));
            Assert.AreEqual(string.Empty, localizer.Localize(MessageResult.None));
        }
    }
}