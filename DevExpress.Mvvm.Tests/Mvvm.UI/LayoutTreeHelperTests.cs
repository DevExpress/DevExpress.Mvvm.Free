using NUnit.Framework;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.UI.Native;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class LayoutTreeHelperTests : BaseWpfFixture {
        public LayoutTreeHelperTests() {
            textBoxTemplate = (DataTemplate)XamlReader.Parse(
@"
<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
<TextBox/>
</DataTemplate>");

            textBlockTemplate = (DataTemplate)XamlReader.Parse(
@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
<TextBlock><Run>Text</Run></TextBlock>
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

                var presenter = LayoutTreeHelper.GetVisualChildren(contentControl).First();
                Assert.IsTrue(new[] { presenter }.SequenceEqual(LayoutTreeHelper.GetVisualParents(textBox, presenter)));
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
    }
}