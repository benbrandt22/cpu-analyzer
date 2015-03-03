using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace cpu_analyzer
{
    public class AppOptions
    {
        [OptionArray('p',"Processes or PIDs", HelpText = "Process Names and/or PIDs to analyze, separated by spaces", Required = true)]
        public string[] ProcessesOrPids { get; set; }

        [Option('s',"Samples", DefaultValue = 10, HelpText = "Number of CPU samples to take", Required = false)]
        public int Samples { get; set; }

        [Option('i', "Interval", DefaultValue = 1000, HelpText = "Interval between samples in milliseconds", Required = false)]
        public int SampleIntervalMs { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo(".Net CPU Analyzer"),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddOptions(this);
            return help;
        }
    }
}
