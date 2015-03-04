using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Samples.Debugging.MdbgEngine;

namespace cpu_analyzer {
    class ThreadSnapshot {

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetThreadTimes(IntPtr handle, out long creation, out long exit, out long kernel, out long user);


        private  ThreadSnapshot ()
        {
        }

        public int Id { get; set; }
        public DateTime Time { get; set; }
        public long KernelTime { get; set; }
        public long UserTime { get; set; }

        public List<string> StackTrace {get; set;}

        static MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();

        public static Guid GetMD5(string str)
        {
            lock (md5Provider)
            {
                return new Guid(md5Provider.ComputeHash(Encoding.Unicode.GetBytes(str)));
            }
        }

        public IEnumerable<Tuple<Guid, string>> StackHashes
        {
            get 
            {
                List<Tuple<Guid, string>> rval = new List<Tuple<Guid, string>>();

                List<string> trace = new List<string>();

                foreach (var item in ((IEnumerable<string>)StackTrace).Reverse())
                {
                    trace.Insert(0, item);
                    var traceString = string.Join(Environment.NewLine, trace);
                    yield return Tuple.Create(GetMD5(traceString), traceString);
                }
            }
        }

        public static ThreadSnapshot GetThreadSnapshot(MDbgThread thread) {
            var snapshot = new ThreadSnapshot();

            snapshot.Id = thread.Id;

            long creation = 0, exit = 0, kernel = 0, user = 0;
            
            try {
                GetThreadTimes(thread.CorThread.Handle, out creation, out exit, out kernel, out user); 
            }
            catch (Exception ex) {
                Console.WriteLine(" Exception on GetThreadTimes for thread ID {0} : {1}", thread.Id, ex);
            }

            snapshot.KernelTime = kernel;
            snapshot.UserTime = user;
            snapshot.StackTrace = new List<string>();

            foreach (MDbgFrame frame in thread.Frames) {
                try {
                    snapshot.StackTrace.Add(frame.Function.FullName);
                } catch {
                    // no frame, so ignore
                }
            }

            return snapshot;
        }
    }
}