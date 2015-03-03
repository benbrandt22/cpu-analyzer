#Cpu analyzer
forked from jitbit's code, see: https://github.com/jitbit/cpu-analyzer

Updated the command line parameter structure to allow multiple processes. If the system has multiple processes of the same name, all instances will be analyzed.

Usage:

`cpu-analyzer -p ProcessName|PID [options]`

-s indicates how many samples to take (default:10)

-i the interval between samples in milliseconds (default:1000)

Example: `cpu-analyzer -p w3wp -s 60 -i 500` - "Take 60 samples once every 500 milliseconds for all processes named 'w3wp'"

The tool output can be quite lengthy, so use it like this:

`cpu-analyzer.exe w3wp >> log.txt`

