using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm.UI.Native {
    public class NotificationPositioner<T> where T : class {
        class ItemInfo {
            public T value;
            public Size size;
        }

        internal NotificationPosition position;
        internal int maxCount;

        Rect screen;
        const double verticalMargin = 10;
        const double verticalScreenMargin = 20;
        readonly List<ItemInfo> items = new List<ItemInfo>();
        public List<T> Items { get { return CloneItemsCollection().Select(i => i == null ? null : i.value).ToList(); } }
        double itemWidth;
        double itemHeight;

        public void Update(Rect screen) {
            Update(screen, position, maxCount);
        }

        public void Update(Rect screen, NotificationPosition position, int maxCount) {
            if(this.screen == screen && this.position == position && this.maxCount == maxCount)
                return;
            this.screen = screen;
            this.position = position;
            this.maxCount = maxCount;
            List<ItemInfo> visible = CloneItemsCollection().Where(i => i != null).ToList();
            items.Clear();
            foreach(ItemInfo info in visible) {
                Add(info.value, info.size.Width, info.size.Height);
            }
        }

        public Point GetItemPosition(T item) {
            ItemInfo info = CloneItemsCollection().FirstOrDefault(i => i != null && i.value == item);
            if (info == null)
                return new Point(-1, -1);
            int index = items.IndexOf(info);
            double y = 0;
            if(position == NotificationPosition.TopRight) {
                y = screen.Y + verticalScreenMargin + index * (info.size.Height + verticalMargin);
            } else {
                y = screen.Height + screen.Y - info.size.Height - verticalScreenMargin - index * (info.size.Height + verticalMargin);
            }
            return new Point(screen.X + screen.Width - info.size.Width, y);
        }

        public Point Add(T item, double width, double height) {
            itemWidth = width;
            itemHeight = height;
            ReplaceSlotValue(null, new ItemInfo { value = item, size = new Size(width, height) });
            return GetItemPosition(item);
        }

        public void Remove(T item) {
            ItemInfo info = CloneItemsCollection().First(i => i != null && i.value == item);
            ReplaceSlotValue(info, null);
        }

        public bool HasEmptySlot() {
            var sourceItems = CloneItemsCollection();
            bool hasEmptySlot = sourceItems.Count < maxCount || sourceItems.Any(i => i == null);
            int visibleCount = sourceItems.Where(i => i != null).Count();
            double margins = 2 * verticalScreenMargin + (visibleCount <= 1 ? 0 : (visibleCount - 1) * verticalMargin);
            bool hasEnoughSpace = (1 + visibleCount) * itemHeight + margins <= screen.Height;
            return hasEmptySlot && hasEnoughSpace;
        }

        void ReplaceSlotValue(ItemInfo oldInfo, ItemInfo newInfo) {
            for(int i = 0; i < items.Count; i++) {
                if(items[i] == oldInfo) {
                    items[i] = newInfo;
                    return;
                }
            }
            items.Add(newInfo);
        }
        List<ItemInfo> CloneItemsCollection() {
            return new List<ItemInfo>(items);
        }
    }
}