using NUnit.Framework;
using System.Windows.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Reflection;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class ViewLocatorTests : BaseWpfFixture {
        protected override void TearDownCore() {
            base.TearDownCore();
            ViewLocator.Default = null;
        }

        [Test]
        public void DefaultViewLocator() {
            Assert.IsNotNull(ViewLocator.Instance);
            Assert.AreSame(ViewLocator.Instance, ViewLocator.Default);
            ViewLocator vl = new ViewLocator(new Assembly[] { });
            ViewLocator.Default = vl;
            Assert.AreSame(vl, ViewLocator.Default);
            ViewLocator.Default = null;
            Assert.AreSame(ViewLocator.Instance, ViewLocator.Default);
        }
        [Test]
        public void ResolveViewTest_01() {
            IViewLocator viewLocator = ViewLocator.Default;
            var viewType = ViewLocator.Default.ResolveViewType(null);
            Assert.IsNull(viewType);
            var view = ViewLocator.Default.ResolveView(string.Empty);
            Assert.AreEqual(typeof(ViewLocatorExtensions.FallbackView), view.GetType());
        }
        [Test]
        public void ResolveViewTest_00() {
            ViewLocator.Default = new ViewLocator(new Assembly[] { typeof(ViewLocatorTests).Assembly });
            var viewType = ViewLocator.Default.ResolveViewType("TestView1");
            Assert.AreEqual(typeof(TestView1), viewType);

            var view = ViewLocator.Default.ResolveView("TestView1");
            Assert.AreEqual(typeof(TestView1), view.GetType());
        }
        [Test]
        public void NullService() {
            IViewLocator vl = null;
            AssertHelper.AssertThrows<ArgumentNullException>(() => { vl.CreateViewTemplate(string.Empty); });
            AssertHelper.AssertThrows<ArgumentNullException>(() => { vl.CreateViewTemplate(typeof(string)); });
        }
        [Test, Asynchronous]
        public void FallbackViewTest() {
            ViewLocator.Default = new ViewLocator(new Assembly[] { typeof(ViewLocatorTests).Assembly });
            FallbackViewTestCore(ViewLocator.Default.CreateViewTemplate("ViewLocatorTests"), "Cannot create DataTemplate from the \"ViewLocatorTests\" view type.");
            FallbackViewTestCore(ViewLocator.Default.CreateViewTemplate("InvalidViewName"), "\"InvalidViewName\" type not found.");
        }
        void FallbackViewTestCore(DataTemplate dataTemplate, string errorText) {
            Window.Content = new ContentPresenter() { ContentTemplate = dataTemplate };
            EnqueueShowWindow();
            EnqueueCallback(() => {
                var view = LayoutTreeHelper.GetVisualChildren((DependencyObject)Window.Content).OfType<TextBlock>().FirstOrDefault();
                Assert.IsNotNull(view);
                Assert.AreEqual(errorText, view.Text);
                Assert.IsTrue(view.ActualWidth > 10);
                Assert.IsTrue(view.ActualHeight > 10);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void CreateTemplateTest() {
            ViewLocator.Default = new ViewLocator(new Assembly[] { typeof(ViewLocatorTests).Assembly });
            CreateTemplateTestCore(ViewLocator.Default.CreateViewTemplate("TestView1"));
            CreateTemplateTestCore(ViewLocator.Default.CreateViewTemplate(typeof(TestView1)));
        }
        void CreateTemplateTestCore(DataTemplate dataTemplate) {
            ContentControl content = new ContentControl() { ContentTemplate = dataTemplate };
            Window.Content = content;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.IsNotNull(LayoutTreeHelper.GetVisualChildren(content).OfType<TestView1>().FirstOrDefault());
            });
            EnqueueTestComplete();
        }
    }
    public class TestView1 : Grid { }
    public class TestView2 : Grid { }
}