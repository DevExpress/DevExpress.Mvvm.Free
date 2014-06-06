#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageBoxButton = DevExpress.Mvvm.DXMessageBoxButton;
#else
using NUnit.Framework;
#endif
using System.Windows;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class DefaultMessageBoxButtonLocalizerTests {
        [Test]
        public void Test() {
            DefaultMessageBoxButtonLocalizer localizer = new DefaultMessageBoxButtonLocalizer();
            Assert.AreEqual("OK", localizer.Localize(MessageBoxResult.OK));
            Assert.AreEqual("Cancel", localizer.Localize(MessageBoxResult.Cancel));
            Assert.AreEqual("Yes", localizer.Localize(MessageBoxResult.Yes));
            Assert.AreEqual("No", localizer.Localize(MessageBoxResult.No));
            Assert.AreEqual(string.Empty, localizer.Localize(MessageBoxResult.None));
        }
    }
}