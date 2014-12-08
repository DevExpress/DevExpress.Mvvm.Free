using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Native;
#if SILVERLIGHT
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
#endif

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class ItemsControlMouseEventArgsConverterTests {
        public class ItemsControlMouseEventArgsConverterTester : ItemsControlMouseEventArgsConverter {
            public object TestConvert(object sender, DependencyObject originalSource) {
                return ConvertCore(sender, originalSource);
            }
            protected override FrameworkElement FindParent(object sender, DependencyObject originalSource) {
                return LayoutHelper.FindLayoutOrVisualParentObject(originalSource, x => CheckItemType(sender)(x), true, (DependencyObject)sender) as FrameworkElement;
            }
        }
        object itemViewModel1;
        ItemsControlMouseEventArgsConverterTester converter;
        FrameworkElement originalSource;
        [SetUp]
        public void SetUp() {
            itemViewModel1 = "ItemViewModel1";
            converter = new ItemsControlMouseEventArgsConverterTester();
            originalSource = new Border();
        }
        [Test]
        public void ListBoxTest() {
            var containter = new ListBoxItem() { DataContext = itemViewModel1 };
            containter.Content = originalSource;
            var itemsControl = new ListBox();
            itemsControl.Items.Add(containter);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.AreEqual(itemViewModel1, result);
        }
        [Test]
        public void ListBoxTestDataContextNullTest() {
            var containter = new ListBoxItem() { DataContext = null };
            containter.Content = originalSource;
            var itemsControl = new ListBox();
            itemsControl.Items.Add(containter);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.IsNull(result);
        }
#if !SILVERLIGHT
        [Test]
        public void ListViewTest() {
            var containter = new ListViewItem() { DataContext = itemViewModel1 };
            containter.Content = originalSource;
            var itemsControl = new ListView();
            itemsControl.Items.Add(containter);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.AreEqual(itemViewModel1, result);
        }
#endif
        public class CustomListBox : ListBox { }
        public class CustomListBoxItem : ListBoxItem { }
        [Test]
        public void CustomListBoxAdjustedItemTypeTest() {
            converter.ItemType = typeof(CustomListBoxItem);
            var containter = new CustomListBoxItem() { DataContext = itemViewModel1 };
            containter.Content = originalSource;
            var itemsControl = new CustomListBox();
            itemsControl.Items.Add(containter);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.AreEqual(itemViewModel1, result);
        }
        [Test]
        public void CustomListBoxNotAdjustedItemTypeTest() {
            var containter = new CustomListBoxItem() { DataContext = itemViewModel1 };
            containter.Content = originalSource;
            var itemsControl = new CustomListBox();
            itemsControl.Items.Add(containter);
            var result = converter.TestConvert(itemsControl, originalSource);
            Assert.IsNull(result);
        }
    }
}