using System;

namespace cpu_analyzer
{
    public class SamplingParams
    {
        public int Pid { get; set; }
        public int Samples { get; set; }
        public TimeSpan SampleInterval { get; set; }
    }
}
