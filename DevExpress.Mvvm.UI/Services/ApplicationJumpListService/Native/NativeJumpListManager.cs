using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Shell;

namespace DevExpress.Mvvm.UI.Native {
    public interface INativeJumpList : IList<JumpItem>, INotifyCollectionChanged {
        bool ShowFrequentCategory { get; set; }
        bool ShowRecentCategory { get; set; }
        void AddToRecentCategory(string path);
        IEnumerable<Tuple<JumpItem, JumpItemRejectionReason>> Apply();
        JumpItem Find(string id);
    }
    public class NativeJumpList : ObservableCollection<JumpItem>, INativeJumpList {
        Func<JumpItem, string> getIdFunc;
        JumpList jumpList = new JumpList();
        Dictionary<string, JumpItem> dictionary = new Dictionary<string, JumpItem>();
        IDisposable jumpListBinding;

        public NativeJumpList(Func<JumpItem, string> getIdFunc) {
            this.getIdFunc = getIdFunc;
            SetJumpListBinding();
        }
        public bool ShowFrequentCategory {
            get { return jumpList.ShowFrequentCategory; }
            set { jumpList.ShowFrequentCategory = value; }
        }
        public bool ShowRecentCategory {
            get { return jumpList.ShowRecentCategory; }
            set { jumpList.ShowRecentCategory = value; }
        }
        public JumpItem Find(string id) {
            JumpItem item;
            return dictionary.TryGetValue(id, out item) ? item : null;
        }
        public IEnumerable<Tuple<JumpItem, JumpItemRejectionReason>> Apply() {
            jumpListBinding.Dispose();
            IEnumerable<Tuple<JumpItem, JumpItemRejectionReason>> rejectedItems = ApplyOverride(jumpList);
            SetJumpListBinding();
            return rejectedItems;
        }
        public void AddToRecentCategory(string path) { AddToRecentCategoryOverride(path); }
        protected virtual IEnumerable<Tuple<JumpItem, JumpItemRejectionReason>> ApplyOverride(JumpList jumpList) {
            IEnumerable<Tuple<JumpItem, JumpItemRejectionReason>> rejectedItems = null;
            EventHandler<JumpItemsRejectedEventArgs> onJumpItemsRejectedHandler = (s, e) => rejectedItems =
                e.RejectedItems.Zip(e.RejectionReasons, (i, r) => new Tuple<JumpItem, JumpItemRejectionReason>(i, r)).ToArray();
            jumpList.JumpItemsRejected += onJumpItemsRejectedHandler;
            jumpList.Apply();
            jumpList.JumpItemsRejected -= onJumpItemsRejectedHandler;
            return rejectedItems;
        }
        protected virtual void AddToRecentCategoryOverride(string path) { JumpList.AddToRecentCategory(path); }
        protected override void ClearItems() {
            dictionary.Clear();
            base.ClearItems();
        }
        protected override void InsertItem(int index, JumpItem item) {
            string id = getIdFunc(item);
            if(id != null)
                dictionary.Add(id, item);
            base.InsertItem(index, item);
        }
        protected override void RemoveItem(int index) {
            string id = getIdFunc(this[index]);
            if(id != null)
                dictionary.Remove(id);
            base.RemoveItem(index);
        }
        protected override void SetItem(int index, JumpItem item) {
            string id = getIdFunc(this[index]);
            if(id != null)
                dictionary.Remove(id);
            id = getIdFunc(item);
            if(id != null)
                dictionary.Add(id, item);
            base.SetItem(index, item);
        }
        void SetJumpListBinding() {
            jumpListBinding = SyncCollectionHelper.TwoWayBind(this, jumpList.JumpItems, x => x, x => x);
        }
    }
}