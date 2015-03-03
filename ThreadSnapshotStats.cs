using System.Collections.Generic;
using System.Linq;

namespace cpu_analyzer {
    class ThreadSnapshotStats {

        public long TotalKernelTime { get; set; }
        public long TotalUserTime { get; set; }
        public int ThreadId { get; set; } 

        public List<string> CommonStack { get; set; }

        public static ThreadSnapshotStats FromSnapshots(IEnumerable<ThreadSnapshot> snapshots) {
            ThreadSnapshotStats stats = new ThreadSnapshotStats();

            stats.ThreadId = snapshots.First().Id;
            stats.TotalKernelTime = snapshots.Last().KernelTime - snapshots.First().KernelTime;
            stats.TotalUserTime = snapshots.Last().UserTime - snapshots.First().UserTime;

            stats.CommonStack = snapshots.First().StackTrace.ToList();

           
            foreach (var stack in snapshots.Select(_ => _.StackTrace.ToList())) {
                while (stats.CommonStack.Count > stack.Count) {
                    stats.CommonStack.RemoveAt(0); 
                }

                while (stats.CommonStack.Count > 0 && stack.Count > 0 && stats.CommonStack[0] != stack[0]) {
                    stats.CommonStack.RemoveAt(0);
                    stack.RemoveAt(0);
                }
            }

            return stats;
        } 

    }
}