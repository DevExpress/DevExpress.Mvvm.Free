using System.Windows;
using NUnit.Framework;
#if NETFX_CORE
using DevExpress.Mvvm.Native;
using DevExpress.TestUtils;
#endif

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
#if NETFX_CORE
            Assert.AreEqual("Abort", localizer.Localize(MessageResult.Abort));
            Assert.AreEqual("Close", localizer.Localize(MessageResult.Close));
            Assert.AreEqual("Ignore", localizer.Localize(MessageResult.Ignore));
            Assert.AreEqual("Retry", localizer.Localize(MessageResult.Retry));
#endif
            Assert.AreEqual(string.Empty, localizer.Localize(MessageResult.None));
        }
#if NETFX_CORE
        [Test]
        public void LocalizerTest() {
            LocalizerTester.TestLocalizer(typeof(MessageButtonPlatformLocalizer));
        }
#endif
    }
}
