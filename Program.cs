using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Samples.Debugging.MdbgEngine;
using System.Diagnostics;
using System.Threading;

namespace cpu_analyzer {

    class Program {

        static void Main(string[] args) {

            var appOptions = new AppOptions();
            if (!CommandLine.Parser.Default.ParseArguments(args, appOptions)) {
                // error parsing
                return;
            }

            var validPids = new List<int>();

            // options parsed OK
            Console.WriteLine("----------------------------------------");
            Console.WriteLine();
            Console.WriteLine(".Net CPU Analyzer");
            Console.WriteLine("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
            Console.WriteLine();
            Console.WriteLine("ANALYSIS OPTIONS:");
            foreach (var proc in appOptions.ProcessesOrPids) {
                var pidsForThisProc = GetValidPids(proc);
                Console.WriteLine(" Process:  \"{0}\" => PID {1}",
                    proc,
                    pidsForThisProc.Any() ? string.Join(", ", pidsForThisProc ) : "Not Found"
                    );
                validPids.AddRange(pidsForThisProc);
            }
            Console.WriteLine(" Samples:  {0}", appOptions.Samples);
            Console.WriteLine(" Interval: {0} ms", appOptions.SampleIntervalMs);
            Console.WriteLine();

            var samplingParams = validPids.Select(pid => new SamplingParams {
                Pid = pid,
                Samples = appOptions.Samples,
                SampleInterval = TimeSpan.FromMilliseconds(appOptions.SampleIntervalMs)
            });

            foreach (var sp in samplingParams)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("  PID {0}", sp.Pid);
                Console.WriteLine("----------------------------------------");
                var stats = CollectStats(sp);
                Analyze(stats);
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("----------------------------------------");
            Console.WriteLine();
        }


        private static Dictionary<int, List<ThreadSnapshot>> CollectStats(SamplingParams sampling) {
            var stats = new Dictionary<int, List<ThreadSnapshot>>();
            var debugger = new MDbgEngine();

            MDbgProcess attached = null;
            try {
                attached = debugger.Attach(sampling.Pid);
            }
            catch (Exception e) {
                Console.WriteLine("Error: failed to attach to process: " + e);
                return stats;
            }

            attached.Go().WaitOne();

            for (int i = 0; i < sampling.Samples; i++) {
                foreach (MDbgThread thread in attached.Threads) {
                    try {
                        var snapshot = ThreadSnapshot.GetThreadSnapshot(thread);
                        List<ThreadSnapshot> snapshots;
                        if (!stats.TryGetValue(snapshot.Id, out snapshots))
                        {
                            snapshots = new List<ThreadSnapshot>();
                            stats[snapshot.Id] = snapshots;
                        }

                        snapshots.Add(snapshot);
                    }
                    catch (Exception ex) {
                        Console.WriteLine("Exception getting sample #{0} from PID {1} : {2}", i, sampling.Pid, ex.ToString());
                    }
                    
                }

                attached.Go();
                Thread.Sleep(sampling.SampleInterval);
                attached.AsyncStop().WaitOne();
            }

            attached.Detach().WaitOne();

            return stats;
        }

        private static void Analyze(Dictionary<int, List<ThreadSnapshot>> stats) {
            // perform basic analysis to see which are the top N stack traces observed, 
            // weighted on cost 

            Dictionary<Guid, long> costs = new Dictionary<Guid, long>();
            Dictionary<Guid, string> stacks = new Dictionary<Guid, string>();

            foreach (var stat in stats.Values) {
                long prevTime = -1;
                foreach (var snapshot in stat) {
                    long time = snapshot.KernelTime + snapshot.UserTime;
                    if (prevTime != -1) {
                        foreach (var tuple in snapshot.StackHashes) {
                            if (costs.ContainsKey(tuple.Item1)) {
                                costs[tuple.Item1] += time - prevTime;
                            }
                            else {
                                costs[tuple.Item1] = time - prevTime;
                                stacks[tuple.Item1] = tuple.Item2;
                            }
                        }
                    }
                    prevTime = time;
                }
            }

            Console.WriteLine("Most expensive stacks");
            Console.WriteLine("------------------------------------");
            foreach (var grp in costs.OrderByDescending(p => p.Value).GroupBy(p => p.Value)) {
                List<string> stacksToShow = new List<string>();

                foreach (var pair in grp.OrderByDescending(p => stacks[p.Key].Length)) {
                    if (!stacksToShow.Any(s => s.Contains(stacks[pair.Key]))) {
                        stacksToShow.Add(stacks[pair.Key]);
                    }
                }

                foreach (var stack in stacksToShow) {
                    Console.WriteLine(stack);
                    Console.WriteLine("===> Cost ({0})", grp.Key);
                    Console.WriteLine();
                }
            }


            var offenders = stats.Values
                .Select(_ => ThreadSnapshotStats.FromSnapshots(_))
                .OrderBy(stat => stat.TotalKernelTime + stat.TotalUserTime)
                .Reverse();

            foreach (var stat in offenders) {
                Console.WriteLine("------------------------------------");
                Console.WriteLine(stat.ThreadId);
                Console.WriteLine("Kernel: {0} User: {1}", stat.TotalKernelTime, stat.TotalUserTime);
                foreach (var method in stat.CommonStack) {
                    Console.WriteLine(method);
                }
                Console.WriteLine("Other Stacks:");
                var prev = new List<string>();
                foreach (var trace in stats[stat.ThreadId].Select(_ => _.StackTrace)) {
                    if (!prev.SequenceEqual(trace)) {
                        Console.WriteLine();
                        foreach (var method in trace) {
                            Console.WriteLine(method);
                        }
                    }
                    else {
                        Console.WriteLine("<skipped>");
                    }
                    prev = trace;
                }
                Console.WriteLine("------------------------------------");
            }
        }

        public static List<int> GetValidPids(string pidOrProcess)
        {
            List<int> pids = new List<int>();

            // Find processes matching by name
            var processes = Process.GetProcessesByName(pidOrProcess);

            if (processes.Any())
            {
                pids.AddRange(processes.Select(p => p.Id).ToArray());
            }
            else
            {
                // assume a single numeric value was provided
                int pid;
                if (Int32.TryParse(pidOrProcess, out pid))
                {
                    // does this PID exist right now?
                    bool PidIsValid = Process.GetProcesses().Any(p => p.Id == pid);
                    if (PidIsValid)
                    {
                        pids.Add(pid);
                    }
                }
            }

            return pids;
        }
    }
}
