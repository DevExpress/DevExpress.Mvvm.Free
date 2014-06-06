using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Client;
using System.Windows;
using System.Windows.Controls;

namespace DevExpress {
    class TestGrid : Grid, ITestPage {
        ITestPage TestPage { get; set; }
        public Panel TestPanel {
            get { return TestPage.TestPanel; }
        }
        public TestGrid(ComboBox comboBox, ITestPage testPage) {
            TestPage = testPage;
            RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            RowDefinitions.Add(new RowDefinition());
            if(comboBox != null) {
                Children.Add(comboBox);
                comboBox.SetValue(Grid.RowProperty, 0);
            }
            Children.Add((UIElement)testPage);
            ((UIElement)testPage).SetValue(Grid.RowProperty, 1);
        }
    }
}