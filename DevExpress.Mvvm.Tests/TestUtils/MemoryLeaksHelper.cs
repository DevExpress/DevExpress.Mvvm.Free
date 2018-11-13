using System;

namespace DevExpress {
    public static class MemoryLeaksHelper {
        public static void EnsureCollected(params WeakReference[] references) {
            DispatcherHelper.DoEvents();
            GCTestHelper.EnsureCollected(references);
        }
        public static void CollectOptional(params WeakReference[] references) {
            DispatcherHelper.DoEvents();
            GCTestHelper.CollectOptional(references);
        }
    }
}