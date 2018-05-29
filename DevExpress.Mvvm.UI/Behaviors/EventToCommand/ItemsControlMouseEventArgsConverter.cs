using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.Native;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DevExpress.Mvvm.UI {
    public class ItemsControlMouseEventArgsConverter : EventArgsConverterBase<MouseEventArgs> {
        public static object GetDataRow(ItemsControl sender, MouseEventArgs args) {
            var converter = new ItemsControlMouseEventArgsConverter();
            return converter.Convert(sender, args);
        }
        protected static Dictionary<Type, Type> itemsTypes = new Dictionary<Type, Type>() {
            { typeof(ListBox), typeof(ListBoxItem) },
            { typeof(ListView), typeof(ListViewItem) },
            { typeof(TreeView), typeof(TreeViewItem) },
            { typeof(TreeViewItem), typeof(TreeViewItem) },
            { typeof(TabControl), typeof(TabItem) },
            { typeof(StatusBar), typeof(StatusBarItem) },
            { typeof(Menu), typeof(MenuItem) },
        };
        public static readonly DependencyProperty ItemTypeProperty =
            DependencyProperty.Register("ItemType", typeof(Type), typeof(ItemsControlMouseEventArgsConverter), new PropertyMetadata(null));
        public Type ItemType {
            get { return (Type)GetValue(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }
        protected override object Convert(object sender, MouseEventArgs args) {
            return ConvertCore(sender, (DependencyObject)args.OriginalSource);
        }
        protected object ConvertCore(object sender, DependencyObject originalSource) {
            var element = FindParent(sender, originalSource);
            return element != null ? element.DataContext : null;
        }
        protected virtual FrameworkElement FindParent(object sender, DependencyObject originalSource) {
            return LayoutTreeHelper.GetVisualParents(originalSource, (DependencyObject)sender)
                .Where(d => d.GetType() == GetItemType(sender))
                .FirstOrDefault() as FrameworkElement;
        }
        protected virtual Type GetItemType(object sender) {
            Type itemType = ItemType;
            if(itemType == typeof(FrameworkElement) || (ItemType != null && itemType.IsSubclassOf(typeof(FrameworkElement))))
                return itemType;
            Type result = null;
            itemsTypes.TryGetValue(sender.GetType(), out result);
            return result;
        }
    }
}