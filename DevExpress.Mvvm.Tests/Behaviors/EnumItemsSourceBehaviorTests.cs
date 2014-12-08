using DevExpress.Internal;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class EnumItemsSourceBehaviorTests : BaseWpfFixture {

        const string UriPrefix = "pack://application:,,,/DevExpress.Mvvm.Tests.Free;component/Behaviors/TestImages/";

        enum TestEnum1 {
            [Image(UriPrefix + "Cut.png")]
            [Display(Description = "CutDescription")]
            CutItem
        }
        enum TestEnum2 {
            [Image(UriPrefix + "Copy.png")]
            [Display(Description = "CopyDescription")]
            CopyItem,
            [Image(UriPrefix + "Delete.png")]
            [Display(Description = "DeleteDescription", Name = "CustomDeleteItem")]
            DeleteItem,
            CutItem
        }
        bool EnumMemberInfoComparer(EnumMemberInfo enumMemberInfo, string description, string id, string imageName, string name) {
            return description == enumMemberInfo.Description
                && (enumMemberInfo.Id == null) ? (id == null) : id == enumMemberInfo.Id.ToString()
                && (enumMemberInfo.Image == null) ? (imageName == null) : imageName == enumMemberInfo.Image.ToString()
                && enumMemberInfo.Name == name;
        }
        bool ItemsControlNameComparer(ItemsControl control, string firstName, string secondName, string lastName) {
            return ((EnumMemberInfo)control.Items.GetItemAt(0)).Name == firstName
                && ((EnumMemberInfo)control.Items.GetItemAt(1)).Name == secondName
                && ((EnumMemberInfo)control.Items.GetItemAt(2)).Name == lastName;
        }
        bool ItemsControlStringIdComparer(ItemsControl control, string firstId, string secondId, string lastId) {
            return ((EnumMemberInfo)control.Items.GetItemAt(0)).Id.ToString() == firstId
                && ((EnumMemberInfo)control.Items.GetItemAt(1)).Id.ToString() == secondId
                && ((EnumMemberInfo)control.Items.GetItemAt(2)).Id.ToString() == lastId;
        }
        class EnumNameConverter : IValueConverter {
            public object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {
                if(value == null) return null;
                return "^^" + value.ToString() + "**";
            }
            public object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void DefalutValues() {
            EnumItemsSourceBehavior behavior = new EnumItemsSourceBehavior();
            Assert.IsNull(behavior.ItemTemplate);
            Assert.IsNull(behavior.EnumType);
            Assert.IsFalse(behavior.UseNumericEnumValue);
            Assert.IsTrue(behavior.SplitNames);
            Assert.IsNull(behavior.NameConverter);
            Assert.AreEqual(EnumMembersSortMode.Default, behavior.SortMode);
        }
        [Test, ExpectedException(ExpectedMessage = "EnumType required")]
        public void EnumRequiredExceptionForCreate() {
            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior();
            ListBox listBoxControl = new ListBox();
            Interaction.GetBehaviors(listBoxControl).Add(listBoxBehavior);
        }
        [Test, ExpectedException(ExpectedMessage = "EnumType required")]
        public void EnumRequiredExceptionForSet() {
            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            ListBox listBoxControl = new ListBox();
            Interaction.GetBehaviors(listBoxControl).Add(listBoxBehavior);
            listBoxBehavior.EnumType = null;
        }
        [Test]
        public void BehaviorOnLoaded() {
            ListBox listBox = new ListBox();
            ComboBox comboBox = new ComboBox();
            ItemsControl itemsControl = new ItemsControl();

            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            EnumItemsSourceBehavior comboBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            EnumItemsSourceBehavior itemsControlBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };

            Interaction.GetBehaviors(listBox).Add(listBoxBehavior);
            Interaction.GetBehaviors(comboBox).Add(comboBoxBehavior);
            Interaction.GetBehaviors(itemsControl).Add(itemsControlBehavior);

            Assert.IsTrue(Interaction.GetBehaviors(listBox).Contains(listBoxBehavior));
            Assert.AreEqual(1, Interaction.GetBehaviors(listBox).Count);
            Assert.AreEqual(1, listBox.Items.Count);

            Assert.IsTrue(Interaction.GetBehaviors(comboBox).Contains(comboBoxBehavior));
            Assert.AreEqual(1, Interaction.GetBehaviors(comboBox).Count);
            Assert.AreEqual(3, comboBox.Items.Count);

            Assert.IsTrue(Interaction.GetBehaviors(itemsControl).Contains(itemsControlBehavior));
            Assert.AreEqual(1, Interaction.GetBehaviors(itemsControl).Count);
            Assert.AreEqual(3, itemsControl.Items.Count);

            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)listBox.Items.GetItemAt(0), "CutDescription", "CutItem",
               UriPrefix + "Cut.png", "Cut Item"));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)comboBox.Items.GetItemAt(0), "CopyDescription", "CopyItem",
               UriPrefix + "Copy.png", "Copy Item"));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)comboBox.Items.GetItemAt(1), "DeleteDescription", "DeleteItem",
               UriPrefix + "Delete.png", "Custom Delete Item"));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)comboBox.Items.GetItemAt(2), null, "CutItem",
               null, "Cut Item"));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)itemsControl.Items.GetItemAt(0), "CopyDescription", "CopyItem",
               UriPrefix + "Copy.png", "Copy Item"));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)itemsControl.Items.GetItemAt(1), "DeleteDescription", "DeleteItem",
               UriPrefix + "Delete.png", "Custom Delete Item"));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)itemsControl.Items.GetItemAt(2), null, "CutItem",
               null, "Cut Item"));
        }

        [Test]
        public void AssociatedObjectChanged() {
            ListBox itemsControl1 = new ListBox();
            ComboBox itemsControl2 = new ComboBox();
            EnumItemsSourceBehavior behavior1 = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            EnumItemsSourceBehavior behavior2 = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2), SplitNames = false, SortMode = EnumMembersSortMode.DisplayNameLengthDescending };

            Interaction.GetBehaviors(itemsControl1).Add(behavior1);
            Interaction.GetBehaviors(itemsControl2).Add(behavior2);
            Interaction.GetBehaviors(itemsControl1).Clear();
            Interaction.GetBehaviors(itemsControl2).Clear();

            Interaction.GetBehaviors(itemsControl1).Add(behavior2);
            Assert.IsTrue(Interaction.GetBehaviors(itemsControl1).Contains(behavior2));
            Assert.IsEmpty(Interaction.GetBehaviors(itemsControl2));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)itemsControl1.Items.GetItemAt(0), "DeleteDescription", "DeleteItem",
               UriPrefix + "Delete.png", "CustomDeleteItem"));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)itemsControl1.Items.GetItemAt(1), "CopyDescription", "CopyItem",
               UriPrefix + "Copy.png", "CopyItem"));
        }

        [Test]
        public void BehaviorSortModeChanged() {
            ListBox listBox = new ListBox();
            ComboBox comboBox = new ComboBox();
            ItemsControl itemsControl = new ItemsControl();

            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            EnumItemsSourceBehavior comboBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            EnumItemsSourceBehavior itemsControlBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };

            Interaction.GetBehaviors(listBox).Add(listBoxBehavior);
            Interaction.GetBehaviors(comboBox).Add(comboBoxBehavior);
            Interaction.GetBehaviors(itemsControl).Add(itemsControlBehavior);

            listBoxBehavior.SortMode = EnumMembersSortMode.DisplayNameDescending;
            comboBoxBehavior.SortMode = EnumMembersSortMode.DisplayNameDescending;
            itemsControlBehavior.SortMode = EnumMembersSortMode.DisplayNameDescending;
            Assert.IsTrue(ItemsControlNameComparer(listBox, "Cut Item", "Custom Delete Item", "Copy Item"));
            Assert.IsTrue(ItemsControlNameComparer(comboBox, "Cut Item", "Custom Delete Item", "Copy Item"));
            Assert.IsTrue(ItemsControlNameComparer(itemsControl, "Cut Item", "Custom Delete Item", "Copy Item"));

            listBoxBehavior.SortMode = EnumMembersSortMode.DisplayNameLength;
            comboBoxBehavior.SortMode = EnumMembersSortMode.DisplayNameLength;
            itemsControlBehavior.SortMode = EnumMembersSortMode.DisplayNameLength;
            Assert.IsTrue(ItemsControlNameComparer(listBox, "Cut Item", "Copy Item", "Custom Delete Item"));
            Assert.IsTrue(ItemsControlNameComparer(comboBox, "Cut Item", "Copy Item", "Custom Delete Item"));
            Assert.IsTrue(ItemsControlNameComparer(itemsControl, "Cut Item", "Copy Item", "Custom Delete Item"));

            listBoxBehavior.SortMode = EnumMembersSortMode.DisplayNameLengthDescending;
            comboBoxBehavior.SortMode = EnumMembersSortMode.DisplayNameLengthDescending;
            itemsControlBehavior.SortMode = EnumMembersSortMode.DisplayNameLengthDescending;
            Assert.IsTrue(ItemsControlNameComparer(listBox, "Custom Delete Item", "Copy Item", "Cut Item"));
            Assert.IsTrue(ItemsControlNameComparer(comboBox, "Custom Delete Item", "Copy Item", "Cut Item"));
            Assert.IsTrue(ItemsControlNameComparer(itemsControl, "Custom Delete Item", "Copy Item", "Cut Item"));
        }
        [Test]
        public void BehaviorNameConverterSet() {
            ListBox listBox = new ListBox();
            ComboBox comboBox = new ComboBox();
            ItemsControl itemsControl = new ItemsControl();
            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            EnumItemsSourceBehavior comboBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            EnumItemsSourceBehavior itemsControlBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            Interaction.GetBehaviors(listBox).Add(listBoxBehavior);
            Interaction.GetBehaviors(comboBox).Add(comboBoxBehavior);
            Interaction.GetBehaviors(itemsControl).Add(itemsControlBehavior);

            listBoxBehavior.NameConverter = new EnumNameConverter();
            comboBoxBehavior.NameConverter = new EnumNameConverter();
            itemsControlBehavior.NameConverter = new EnumNameConverter();
            Assert.IsTrue(ItemsControlNameComparer(listBox, "^^CopyItem**", "^^DeleteItem**", "^^CutItem**"));
            Assert.IsTrue(ItemsControlNameComparer(comboBox, "^^CopyItem**", "^^DeleteItem**", "^^CutItem**"));
            Assert.IsTrue(ItemsControlNameComparer(itemsControl, "^^CopyItem**", "^^DeleteItem**", "^^CutItem**"));

            listBoxBehavior.NameConverter = null;
            comboBoxBehavior.NameConverter = null;
            itemsControlBehavior.NameConverter = null;
            Assert.IsTrue(ItemsControlNameComparer(listBox, "Copy Item", "Custom Delete Item", "Cut Item"));
            Assert.IsTrue(ItemsControlNameComparer(comboBox, "Copy Item", "Custom Delete Item", "Cut Item"));
            Assert.IsTrue(ItemsControlNameComparer(itemsControl, "Copy Item", "Custom Delete Item", "Cut Item"));
        }
        [Test]
        public void BehaviorSplitNames() {
            ListBox listBox = new ListBox();
            ComboBox comboBox = new ComboBox();
            ItemsControl itemsControl = new ItemsControl();
            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            EnumItemsSourceBehavior comboBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            EnumItemsSourceBehavior itemsControlBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            Interaction.GetBehaviors(listBox).Add(listBoxBehavior);
            Interaction.GetBehaviors(comboBox).Add(comboBoxBehavior);
            Interaction.GetBehaviors(itemsControl).Add(itemsControlBehavior);

            listBoxBehavior.SplitNames = false;
            comboBoxBehavior.SplitNames = false;
            itemsControlBehavior.SplitNames = false;
            Assert.IsTrue(ItemsControlNameComparer(listBox, "CopyItem", "CustomDeleteItem", "CutItem"));
            Assert.IsTrue(ItemsControlNameComparer(comboBox, "CopyItem", "CustomDeleteItem", "CutItem"));
            Assert.IsTrue(ItemsControlNameComparer(itemsControl, "CopyItem", "CustomDeleteItem", "CutItem"));
        }
        [Test]
        public void UseNumericEnumValue() {
            ListBox listBox = new ListBox();
            ComboBox comboBox = new ComboBox();
            ItemsControl itemsControl = new ItemsControl();
            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            EnumItemsSourceBehavior comboBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            EnumItemsSourceBehavior itemsControlBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum2) };
            Interaction.GetBehaviors(listBox).Add(listBoxBehavior);
            Interaction.GetBehaviors(comboBox).Add(comboBoxBehavior);
            Interaction.GetBehaviors(itemsControl).Add(itemsControlBehavior);

            listBoxBehavior.UseNumericEnumValue = true;
            comboBoxBehavior.UseNumericEnumValue = true;
            itemsControlBehavior.UseNumericEnumValue = true;
            listBoxBehavior.SortMode = EnumMembersSortMode.DisplayNameLength;
            comboBoxBehavior.SortMode = EnumMembersSortMode.DisplayNameLength;
            itemsControlBehavior.SortMode = EnumMembersSortMode.DisplayNameLength;
            Assert.IsTrue(ItemsControlStringIdComparer(listBox, "2", "0", "1"));
            Assert.IsTrue(ItemsControlStringIdComparer(comboBox, "2", "0", "1"));
            Assert.IsTrue(ItemsControlStringIdComparer(itemsControl, "2", "0", "1"));

            listBoxBehavior.UseNumericEnumValue = false;
            comboBoxBehavior.UseNumericEnumValue = false;
            itemsControlBehavior.UseNumericEnumValue = false;
            Assert.IsTrue(ItemsControlStringIdComparer(listBox, "CutItem", "CopyItem", "DeleteItem"));
            Assert.IsTrue(ItemsControlStringIdComparer(comboBox, "CutItem", "CopyItem", "DeleteItem"));
            Assert.IsTrue(ItemsControlStringIdComparer(itemsControl, "CutItem", "CopyItem", "DeleteItem"));
        }
        [Test]
        public void EnumTypeChanged() {
            ListBox listBox = new ListBox();
            ComboBox comboBox = new ComboBox();
            ItemsControl itemsControl = new ItemsControl();
            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            EnumItemsSourceBehavior comboBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            EnumItemsSourceBehavior itemsControlBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            Interaction.GetBehaviors(listBox).Add(listBoxBehavior);
            Interaction.GetBehaviors(comboBox).Add(comboBoxBehavior);
            Interaction.GetBehaviors(itemsControl).Add(itemsControlBehavior);
            Assert.AreEqual(1, listBox.Items.Count);
            Assert.AreEqual(1, comboBox.Items.Count);
            Assert.AreEqual(1, comboBox.Items.Count);

            listBoxBehavior.EnumType = typeof(TestEnum2);
            comboBoxBehavior.EnumType = typeof(TestEnum2);
            itemsControlBehavior.EnumType = typeof(TestEnum2);
            Assert.AreEqual(3, listBox.Items.Count);
            Assert.AreEqual(3, comboBox.Items.Count);
            Assert.AreEqual(3, comboBox.Items.Count);
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)listBox.Items.GetItemAt(0), "CopyDescription", "CopyItem",
                UriPrefix + "Copy.png", "Copy Item"));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)comboBox.Items.GetItemAt(0), "CopyDescription", "CopyItem",
                UriPrefix + "Copy.png", "Copy Item"));
            Assert.IsTrue(EnumMemberInfoComparer((EnumMemberInfo)itemsControl.Items.GetItemAt(0), "CopyDescription", "CopyItem",
                UriPrefix + "Copy.png", "Copy Item"));
        }

        [Test]
        public void DefaultDataTemplateLoad() {
            ListBox listBox = new ListBox();
            ComboBox comboBox = new ComboBox();
            ItemsControl itemsControl = new ItemsControl();
            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            EnumItemsSourceBehavior comboBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            EnumItemsSourceBehavior itemsControlBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            Interaction.GetBehaviors(listBox).Add(listBoxBehavior);
            Interaction.GetBehaviors(comboBox).Add(comboBoxBehavior);
            Interaction.GetBehaviors(itemsControl).Add(itemsControlBehavior);
            Assert.IsNotNull(listBoxBehavior.ItemTemplate);
            Assert.IsNotNull(comboBox.ItemTemplate);
            Assert.IsNotNull(itemsControl.ItemTemplate);
            Assert.AreEqual(listBoxBehavior.defaultDataTemplate, listBox.ItemTemplate);
            Assert.AreEqual(comboBoxBehavior.defaultDataTemplate, comboBox.ItemTemplate);
            Assert.AreEqual(itemsControlBehavior.defaultDataTemplate, itemsControl.ItemTemplate);
        }
        [POCOViewModel]
        public class TestViewModel {
            public virtual DataTemplate NewTemplate { get; set; }
        }
        [Test]
        public void SetItemTemplateBinding() {
            ListBox control = new ListBox();
            DataTemplate testDataTemplate = new DataTemplate();
            TestViewModel testViewModel = ViewModelSource.Create<TestViewModel>();
            EnumItemsSourceBehavior listBoxBehavior = new EnumItemsSourceBehavior() { EnumType = typeof(TestEnum1) };
            BindingOperations.SetBinding(listBoxBehavior, EnumItemsSourceBehavior.ItemTemplateProperty,
                new Binding("NewTemplate") { Source = testViewModel, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            Interaction.GetBehaviors(control).Add(listBoxBehavior);
            Assert.AreEqual(listBoxBehavior.defaultDataTemplate, control.ItemTemplate);
            testViewModel.NewTemplate = testDataTemplate;
            Assert.AreEqual(testDataTemplate, control.ItemTemplate);
        }
    }
}