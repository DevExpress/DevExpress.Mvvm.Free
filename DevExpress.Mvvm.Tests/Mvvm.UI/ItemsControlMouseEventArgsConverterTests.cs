using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Native;
using NUnit.Framework;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class ItemsControlMouseEventArgsConverterTests : BaseWpfFixture {
        public class ItemsControlMouseEventArgsConverterTester : ItemsControlMouseEventArgsConverter {
            public object TestConvert(object sender, DependencyObject originalSource) {
                return ConvertCore(sender, originalSource);
            }
        }
        object itemViewModel1;
        ItemsControlMouseEventArgsConverterTester converter;
        FrameworkElement originalSource;
        protected override void SetUpCore() {
            base.SetUpCore();
            itemViewModel1 = "ItemViewModel1";
            converter = new ItemsControlMouseEventArgsConverterTester();
            originalSource = new Border();
        }
        void ShowControl(Control control) {
            Window.Content = control;
            Window.Show();
        }
        MouseEventArgs CreateMouseArgs() {
            return new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Control.MouseDoubleClickEvent, Source = originalSource };
        }
        [Test]
        public void ListBoxTest() {
            var containter = new ListBoxItem() { DataContext = itemViewModel1 };
            containter.Content = originalSource;
            var itemsControl = new ListBox();
            itemsControl.Items.Add(containter);
            ShowControl(itemsControl);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.AreEqual(itemViewModel1, result);
            Assert.AreEqual(itemViewModel1, ItemsControlMouseEventArgsConverter.GetDataRow(itemsControl, CreateMouseArgs()));
        }

        [Test]
        public void ListBoxTestDataContextNullTest() {
            var containter = new ListBoxItem() { DataContext = null };
            containter.Content = originalSource;
            var itemsControl = new ListBox();
            itemsControl.Items.Add(containter);
            ShowControl(itemsControl);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.IsNull(result);
            Assert.AreEqual(null, ItemsControlMouseEventArgsConverter.GetDataRow(itemsControl, CreateMouseArgs()));
        }
        [Test]
        public void ListViewTest() {
            var containter = new ListViewItem() { DataContext = itemViewModel1 };
            containter.Content = originalSource;
            var itemsControl = new ListView();
            itemsControl.Items.Add(containter);
            ShowControl(itemsControl);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.AreEqual(itemViewModel1, result);
            Assert.AreEqual(itemViewModel1, ItemsControlMouseEventArgsConverter.GetDataRow(itemsControl, CreateMouseArgs()));
        }
        public class CustomListBox : ListBox { }
        public class CustomListBoxItem : ListBoxItem { }
        [Test]
        public void CustomListBoxAdjustedItemTypeTest() {
            converter.ItemType = typeof(CustomListBoxItem);
            var containter = new CustomListBoxItem() { DataContext = itemViewModel1 };
            containter.Content = originalSource;
            var itemsControl = new CustomListBox();
            itemsControl.Items.Add(containter);
            ShowControl(itemsControl);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.AreEqual(itemViewModel1, result);
        }
        [Test]
        public void CustomListBoxNotAdjustedItemTypeTest() {
            var containter = new CustomListBoxItem() { DataContext = itemViewModel1 };
            containter.Content = originalSource;
            var itemsControl = new CustomListBox();
            itemsControl.Items.Add(containter);
            ShowControl(itemsControl);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.IsNull(result);
            Assert.AreEqual(null, ItemsControlMouseEventArgsConverter.GetDataRow(itemsControl, CreateMouseArgs()));
        }
    }
}