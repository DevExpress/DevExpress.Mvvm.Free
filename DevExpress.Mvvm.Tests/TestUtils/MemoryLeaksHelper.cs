using System;
using System.Windows.Threading;

namespace DevExpress {
    public static class MemoryLeaksHelper {
        public static void EnsureCollected(params WeakReference[] references) {
            DispatcherHelper.DoEvents(DispatcherPriority.ApplicationIdle);
            GCTestHelper.EnsureCollected(references);
        }
        public static void CollectOptional(params WeakReference[] references) {
            DispatcherHelper.DoEvents(DispatcherPriority.ApplicationIdle);
            GCTestHelper.CollectOptional(references);
        }
    }
}