#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageBoxButton = DevExpress.Mvvm.DXMessageBoxButton;
#elif NETFX_CORE
using DevExpress.TestFramework.NUnit;
#else
using NUnit.Framework;
#endif
using System.Windows;

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
    }
}