using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            AssertCollected(GetLiveReferences(references));
        }
        public static List<WeakReference> GetLiveReferences(IEnumerable<WeakReference> references) {
            return GetLiveReferencesCore(references, -1);
        }
        static List<WeakReference> GetLiveReferencesCore(IEnumerable<WeakReference> references, int alreadyCollectedGen) {
            int maxGeneration;
            List<WeakReference> nextIterationHolder = CollectExistingData(references, out maxGeneration);
            if(nextIterationHolder.Count == 0)
                return new List<WeakReference>();
            if(maxGeneration <= alreadyCollectedGen) {
                return SlowButSureGetLiveReferencesCore(nextIterationHolder);
            }
            else {
                GC.Collect(maxGeneration, GCCollectionMode.Forced);
                return GetLiveReferencesCore(nextIterationHolder, maxGeneration);
            }
        }
        static List<WeakReference> CollectExistingData(IEnumerable<WeakReference> references, out int maxGeneration) {
            maxGeneration = -1;
            List<WeakReference> nextIterationHolder = new List<WeakReference>();
            foreach(WeakReference wr in references) {
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
        static List<WeakReference> SlowButSureGetLiveReferencesCore(IList<WeakReference> nextIterationHolder) {
            List<WeakReference> empty = new List<WeakReference>();
            GC.GetTotalMemory(true);
            if (nextIterationHolder.All(wr => !wr.IsAlive))
                return empty;
            GC.Collect();
            if (nextIterationHolder.All(wr => !wr.IsAlive))
                return empty;
            GC.GetTotalMemory(true);
            return nextIterationHolder.Where(t => t.Target != null).ToList();
        }

        static void AssertCollected(List<WeakReference> nextIterationHolder) {
            object[] notCollected = nextIterationHolder.Select(wr => wr.Target).Where(t => t != null).ToArray();
            if(notCollected.Length==0)
                return;
            throw new GCTestHelperException(BuildExceptionString(notCollected));
        }

        static string BuildExceptionString(object[] notCollected) {
            StringBuilder report = new StringBuilder();
            report.AppendLine($"{notCollected.Length} garbage object(s) not collected:{report}");
            report.AppendLine();
            foreach(IGrouping<Type, object> typeAndInstancesGrouping in notCollected.GroupBy(o => o.GetType()).OrderBy(gr => gr.Key.FullName)) {
                Type currentType = typeAndInstancesGrouping.Key;
                object[] typeAndInstances = typeAndInstancesGrouping.ToArray();
                IGrouping<string, string>[] currentInstancesFormatted = typeAndInstances.Select(x => x.ToString()).GroupBy(x => x).OrderBy(x => x.Key).ToArray();

                bool currentInstancesIsInformative = currentInstancesFormatted.Length > 1;

                report.AppendLine($"\t{typeAndInstances.Length} object(s) of type {currentType.FullName}{(currentInstancesIsInformative ? ":" : ";")}");
                if (currentInstancesIsInformative) {
                    string instancesReport = string.Join(",\r\n", currentInstancesFormatted.Select(x => $"\t\t[{x.Count()}] - {x.Key}"));
                    report.Append(instancesReport);
                    report.Append(';');
                    report.AppendLine();
                }
            }
            return report.ToString();
        }

        public static void CollectOptional(params WeakReference[] references) {
            CollectOptional(references.AsEnumerable());
        }
        public static bool? HardOptional;
        static Random rnd = new Random();
        static bool IsHardOptional() {
            if(HardOptional.HasValue)
                return HardOptional.Value;
            lock(rnd)
                return rnd.Next(100) < 5;
        }
        public static void CollectOptional(IEnumerable<WeakReference> references) {
            if(IsHardOptional()) {
                GC.Collect();
                GC.GetTotalMemory(true);
            }
            else {
                int? maxGeneration = references.Select(wr => wr.Target).Where(t => t != null).Select(t => GC.GetGeneration(t)).Max(gen => (int?)gen);
                if(maxGeneration.HasValue) {
                    GC.Collect(maxGeneration.Value, GCCollectionMode.Forced);
                }
            }
        }
    }
}