using System;
using System.Linq;
using System.Collections.Generic;

namespace DevExpress {
    public class GCTestHelperException : Exception {
        public GCTestHelperException(string message) : base(message) {
        }
    }

    public static class GCTestHelper {
        static WeakReference Obtain(Func<object> obtainer) {
            return new WeakReference(obtainer());
        }
        static WeakReference[] Obtain(Func<object[]> obtainer) {
            return obtainer().Select(o => new WeakReference(o)).ToArray();
        }
        public static void EnsureCollected(Func<object> obtainer) {
            EnsureCollected(Obtain(obtainer));
        }
        public static void EnsureCollected(Func<object[]> obtainer) {
            EnsureCollected(Obtain(obtainer));
        }
        public static void EnsureCollected(params WeakReference[] references) {
            EnsureCollected(references.AsEnumerable());
        }
        public static void EnsureCollected(IEnumerable<WeakReference> references) {
            AssertCollectedCore(references, -1);
        }
        static void AssertCollectedCore(IEnumerable<WeakReference> references, int alreadyCollectedGen) {
            int maxGeneration;
            List<WeakReference> nextIterationHolder = CollectExistingData(references, out maxGeneration);
            if(nextIterationHolder.Count == 0)
                return;
            if(maxGeneration <= alreadyCollectedGen) {
                SlowButSureAssertCollected(nextIterationHolder);
            } else {
                GC.Collect(maxGeneration, GCCollectionMode.Forced);
                AssertCollectedCore(nextIterationHolder, maxGeneration);
            }
        }
        static List<WeakReference> CollectExistingData(IEnumerable<WeakReference> references, out int maxGeneration) {
            maxGeneration = -1;
            var nextIterationHolder = new List<WeakReference>();
            foreach(var wr in references) {
                object t = wr.Target;
                if(t == null)
                    continue;
                nextIterationHolder.Add(wr);
                int gen = GC.GetGeneration(t);
                if(gen > maxGeneration)
                    maxGeneration = gen;
            }
            return nextIterationHolder;
        }
        static void SlowButSureAssertCollected(IList<WeakReference> nextIterationHolder) {
            GC.GetTotalMemory(true);
            if(nextIterationHolder.All(wr => !wr.IsAlive))
                return;
            GC.Collect();
            if(nextIterationHolder.All(wr => !wr.IsAlive))
                return;
            GC.GetTotalMemory(true);
            var notCollected = nextIterationHolder.Select(wr => wr.Target).Where(t => t != null).ToArray();
            if(notCollected.Length == 0)
                return;
            var objectsReport = string.Join("\n", notCollected.GroupBy(o => o.GetType()).OrderBy(gr => gr.Key.FullName)
                .Select(gr => string.Format("\t{0} object(s) of type {1}:\n{2}", gr.Count(), gr.Key.FullName
                    , string.Join("\n", gr.Select(o => o.ToString()).OrderBy(s => s).Select(s => string.Format("\t\t{0}", s)))
                    )));
            throw new GCTestHelperException(string.Format("{0} garbage object(s) not collected:\n{1}", notCollected.Length, objectsReport));
        }
        public static void CollectOptional(params WeakReference[] references) {
            CollectOptional(references.AsEnumerable());
        }
        public static bool? HardOptional;
        static Random rnd = new Random();
        static bool IsHardOptional() {
            if(HardOptional.HasValue)
                return HardOptional.Value;
            lock(rnd) {
                return rnd.Next(100) < 5;
            }
        }
        public static void CollectOptional(IEnumerable<WeakReference> references) {
            if(IsHardOptional()) {
                GC.Collect();
                GC.GetTotalMemory(true);
            } else {
                int? maxGeneration = references.Select(wr => wr.Target).Where(t => t != null).Select(t => GC.GetGeneration(t)).Max(gen => (int?)gen);
                if(maxGeneration.HasValue) {
                    GC.Collect(maxGeneration.Value, GCCollectionMode.Forced);
                }
            }
        }
    }
}