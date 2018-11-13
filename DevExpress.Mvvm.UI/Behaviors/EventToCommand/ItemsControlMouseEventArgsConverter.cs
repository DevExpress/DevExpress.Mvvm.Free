using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.Native;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MouseEventArgs = Windows.UI.Xaml.RoutedEventArgs;
#endif

namespace DevExpress.Mvvm.UI {
    public class ItemsControlMouseEventArgsConverter : EventArgsConverterBase<MouseEventArgs> {
#if !NETFX_CORE
        public static object GetDataRow(ItemsControl sender, MouseEventArgs args) {
            var converter = new ItemsControlMouseEventArgsConverter();
            return converter.Convert(sender, args);
        }
#endif
        protected static Dictionary<Type, Type> itemsTypes = new Dictionary<Type, Type>() { 
            { typeof(ListBox), typeof(ListBoxItem) },            
            { typeof(ListView), typeof(ListViewItem) },
#if !NETFX_CORE
            { typeof(TreeView), typeof(TreeViewItem) },
            { typeof(TreeViewItem), typeof(TreeViewItem) },
            { typeof(TabControl), typeof(TabItem) },
            { typeof(StatusBar), typeof(StatusBarItem) },
            { typeof(Menu), typeof(MenuItem) },
#else
            { typeof(GridView), typeof(GridViewItem) },
            { typeof(FlipView), typeof(FlipViewItem) },
#endif
            //{ typeof(ComboBox), typeof(ComboBoxItem) },
            //{ typeof(MenuItem), typeof(MenuItem) },
        };
#if !NETFX_CORE
        public static readonly DependencyProperty ItemTypeProperty =
            DependencyProperty.Register("ItemType", typeof(Type), typeof(ItemsControlMouseEventArgsConverter), new PropertyMetadata(null));
        public Type ItemType {
            get { return (Type)GetValue(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }
#else
        public static readonly DependencyProperty ItemTypeProperty =
                DependencyProperty.Register("ItemType", typeof(String), typeof(ItemsControlMouseEventArgsConverter), new PropertyMetadata(null));
        public String ItemType {
            get { return (String)GetValue(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }
#endif
        protected override object Convert(object sender, MouseEventArgs args) {
            return ConvertCore(sender, (DependencyObject)args.OriginalSource);
        }
        protected object ConvertCore(object sender, DependencyObject originalSource) {
            var element = FindParent(sender, originalSource);
#if !NETFX_CORE
            return element != null ? element.DataContext : null;
#else
            if(element == null)
                return null;
            if(element.DataContext != null)
                return element.DataContext;
            return (sender as IItemContainerMapping).With(x => x.ItemFromContainer(element));
#endif
        }
        protected virtual FrameworkElement FindParent(object sender, DependencyObject originalSource) {
            return LayoutTreeHelper.GetVisualParents(originalSource, (DependencyObject)sender)
                .Where(d => d.GetType() == GetItemType(sender))
                .FirstOrDefault() as FrameworkElement;
        }
        protected virtual Type GetItemType(object sender) {
#if !NETFX_CORE
            Type itemType = ItemType;
#else
            Type itemType = null; //TODO
#endif
            if(itemType == typeof(FrameworkElement) || (ItemType != null && itemType.IsSubclassOf(typeof(FrameworkElement))))
                return itemType;
            Type result = null;
            itemsTypes.TryGetValue(sender.GetType(), out result);
            return result;
        }
    }
}
