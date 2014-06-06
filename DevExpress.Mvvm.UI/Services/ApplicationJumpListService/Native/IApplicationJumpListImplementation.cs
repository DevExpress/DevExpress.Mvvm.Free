using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.UI.Native {
    public interface IApplicationJumpListImplementation {
        ApplicationJumpTaskInfo Find(string commandId);
        bool AddOrReplace(ApplicationJumpTaskInfo jumpTask);
        void AddItem(ApplicationJumpItemInfo item);
        void InsertItem(int index, ApplicationJumpItemInfo item);
        void SetItem(int index, ApplicationJumpItemInfo item);
        bool RemoveItem(ApplicationJumpItemInfo item);
        void RemoveItemAt(int index);
        void ClearItems();
        ApplicationJumpItemInfo GetItem(int index);
        int IndexOfItem(ApplicationJumpItemInfo item);
        bool ContainsItem(ApplicationJumpItemInfo item);
        IEnumerable<ApplicationJumpItemInfo> GetItems();
        int ItemsCount();
    }
}