#Cpu analyzer
forked from jitbit's code, see: https://github.com/jitbit/cpu-analyzer

Updated the command line parameter structure to allow multiple processes. If the system has multiple processes of the same name, all instances will be analyzed. Also applied some general refactoring to the code and file structure.

These changes were made to make it easy to analyze all instances of w3wp.exe (IIS .Net web apps) when CPU usage went high for extended periods of time. With multiple web apps running on a machine, this helped identify the source of the issue.

Usage:

`cpu-analyzer -p ProcessName|PID [options]`

-s indicates how many samples to take (default:10)

-i the interval between samples in milliseconds (default:1000)

Example: `cpu-analyzer -p w3wp -s 60 -i 500` - "Take 60 samples once every 500 milliseconds for all processes named 'w3wp'"

The tool output can be quite lengthy, so use it like this:

`cpu-analyzer.exe -p w3wp >> log.txt`