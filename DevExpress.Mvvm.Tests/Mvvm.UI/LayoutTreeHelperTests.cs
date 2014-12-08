#if SILVERLIGHT
using Microsoft.Silverlight.Testing;
#elif NETFX_CORE
using DevExpress.TestFramework.NUnit;
#else
using NUnit.Framework;
#endif
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.UI.Native;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
#else
using System.Windows.Controls;
using System.Windows.Markup;
#endif

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class LayoutTreeHelperTests : BaseWpfFixture {
        public LayoutTreeHelperTests() {
#if !SILVERLIGHT && !NETFX_CORE
            textBoxTemplate = (DataTemplate)XamlReader.Parse(
#else
            textBoxTemplate = (DataTemplate)XamlReader.Load(
#endif
@"
<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
<TextBox/>
</DataTemplate>");

#if !NETFX_CORE && !SILVERLIGHT
            textBlockTemplate = (DataTemplate)XamlReader.Parse(
@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
<TextBlock><Run>Text</Run></TextBlock>
</DataTemplate>");
#endif
        }
        DataTemplate textBoxTemplate;
#if !NETFX_CORE
        [Test, Asynchronous]
#if NETFX_CORE
        public async Task GetVisualParents() {
#else
        public void GetVisualParents() {
#endif
            Grid grid = new Grid();
            ContentControl contentControl = new ContentControl();
            TextBox textBox;
            Window.Content = grid;
            grid.Children.Add(contentControl);
            contentControl.ContentTemplate = textBoxTemplate;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                textBox = (TextBox)LayoutHelper.FindElement(contentControl, x => x is TextBox);
                Assert.AreSame(contentControl, LayoutTreeHelper.GetVisualParents(textBox).Where(x => x is ContentControl).First());
                Assert.AreSame(grid, LayoutTreeHelper.GetVisualParents(textBox).Where(x => x is Grid).First());
                Assert.AreSame(Window, LayoutTreeHelper.GetVisualParents(textBox).Where(x => x.GetType() == Window.GetType()).First());

                Assert.AreSame(contentControl, LayoutTreeHelper.GetVisualParents(textBox, contentControl).Where(x => x is ContentControl).First());
                Assert.IsNull(LayoutTreeHelper.GetVisualParents(textBox, contentControl).Where(x => x is Grid).FirstOrDefault());
            });
            EnqueueTestComplete();
        }
#endif
        [Test, Asynchronous]
#if NETFX_CORE
        public async Task GetVisualChildren() {
#else
        public void GetVisualChildren() {
#endif
            Grid grid = new Grid();
            ContentControl contentControl = new ContentControl();
            TextBox textBox;
            Window.Content = grid;
            grid.Children.Add(contentControl);
            contentControl.ContentTemplate = textBoxTemplate;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                textBox = (TextBox)LayoutHelper.FindElement(contentControl, x => x is TextBox);

                Assert.AreSame(contentControl, LayoutTreeHelper.GetVisualChildren(grid).Where(x => x is ContentControl).FirstOrDefault());
                Assert.AreSame(textBox, LayoutTreeHelper.GetVisualChildren(grid).Where(x => x is TextBox).FirstOrDefault());
            });
            EnqueueTestComplete();
        }

#if !NETFX_CORE && !SILVERLIGHT
        DataTemplate textBlockTemplate;
        [Test, Asynchronous]
        public void GetVisualParents2() {
            Grid grid = new Grid();
            ContentControl contentControl = new ContentControl();
            TextBlock textBox;
            System.Windows.Documents.Inline textBoxContent = null;
            Window.Content = grid;
            grid.Children.Add(contentControl);
            contentControl.ContentTemplate = textBlockTemplate;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                textBox = (TextBlock)LayoutHelper.FindElement(contentControl, x => x is TextBlock);
                textBoxContent = textBox.Inlines.First();

                Assert.AreSame(contentControl, LayoutTreeHelper.GetVisualParents(textBoxContent).Where(x => x is ContentControl).First());
                Assert.AreSame(grid, LayoutTreeHelper.GetVisualParents(textBoxContent).Where(x => x is Grid).First());
                Assert.AreSame(Window, LayoutTreeHelper.GetVisualParents(textBoxContent).Where(x => x.GetType() == Window.GetType()).First());

                Assert.AreSame(contentControl, LayoutTreeHelper.GetVisualParents(textBoxContent, contentControl).Where(x => x is ContentControl).First());
                Assert.IsNull(LayoutTreeHelper.GetVisualParents(textBoxContent, contentControl).Where(x => x is Grid).FirstOrDefault());
            });
            EnqueueTestComplete();
        }
#endif
    }
}