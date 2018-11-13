using System;

namespace DevExpress {
    public static class MemoryLeaksHelper {
        public static void EnsureCollected(params WeakReference[] references) {
#if !NETFX_CORE
            DispatcherHelper.DoEvents();
#endif
            GCTestHelper.EnsureCollected(references);
        }
        public static void CollectOptional(params WeakReference[] references) {
#if !NETFX_CORE
            DispatcherHelper.DoEvents();
#endif
            GCTestHelper.CollectOptional(references);
        }
        /*
        public static void GarbageCollect() {
            DispatcherHelper.DoEvents();
            GC.GetTotalMemory(true);
            GC.WaitForPendingFinalizers();
            GC.GetTotalMemory(true);
        }
         */
    }
}