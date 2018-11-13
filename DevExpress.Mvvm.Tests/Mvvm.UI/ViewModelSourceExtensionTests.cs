using NUnit.Framework;
using DevExpress.Mvvm.POCO;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class ViewModelSourceExtensionTests {
        public class POCOViewModel {
            public virtual string Property { get; set; }
        }
        [Test]
        public void ProvideValueTest() {
            var source = new ViewModelSourceExtension() { Type = typeof(POCOViewModel) };
            var viewModel = (POCOViewModel)source.ProvideValue(null);
            Assert.IsTrue(viewModel is IPOCOViewModel);

            source = new ViewModelSourceExtension(typeof(POCOViewModel));
            viewModel = (POCOViewModel)source.ProvideValue(null);
            Assert.IsTrue(viewModel is IPOCOViewModel);

            source = new ViewModelSourceExtension();
            Assert.IsNull(source.ProvideValue(null));
        }
    }
}