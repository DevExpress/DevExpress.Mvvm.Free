#if !SILVERLIGHT
using NUnit.Framework;
#else
using Microsoft.Silverlight.Testing;
#endif
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.UI.Native;
using System.Windows.Markup;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class LayoutTreeHelperTests : BaseWpfFixture {
        public LayoutTreeHelperTests() {
#if !SILVERLIGHT
            textBoxTemplate = (DataTemplate)XamlReader.Parse(
#else
            textBoxTemplate = (DataTemplate)XamlReader.Load(
#endif
@"
<DataTemplate 
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
<TextBox/>
</DataTemplate>");
        }
        DataTemplate textBoxTemplate;
        [Test, Asynchronous]
        public void GetVisualParents() {
            Grid grid = new Grid();
            ContentControl contentControl = new ContentControl();
            TextBox textBox;
            Window.Content = grid;
            grid.Children.Add(contentControl);
            contentControl.ContentTemplate = textBoxTemplate;

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
        [Test, Asynchronous]
        public void GetVisualChildren() {
            Grid grid = new Grid();
            ContentControl contentControl = new ContentControl();
            TextBox textBox;
            Window.Content = grid;
            grid.Children.Add(contentControl);
            contentControl.ContentTemplate = textBoxTemplate;

            EnqueueShowWindow();
            EnqueueCallback(() => {
                textBox = (TextBox)LayoutHelper.FindElement(contentControl, x => x is TextBox);

                Assert.AreSame(contentControl, LayoutTreeHelper.GetVisualChildren(grid).Where(x => x is ContentControl).FirstOrDefault());
                Assert.AreSame(textBox, LayoutTreeHelper.GetVisualChildren(grid).Where(x => x is TextBox).FirstOrDefault());
            });
            EnqueueTestComplete();
        }
    }
}