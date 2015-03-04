#Cpu Analyzer
forked from jitbit's code, see: https://github.com/jitbit/cpu-analyzer

Updated the command line parameter structure to allow multiple processes. If the system has multiple processes of the same name, all instances will be analyzed. Also applied some general refactoring to the code and file structure.

These changes were made to make it easy to analyze all instances of w3wp.exe (IIS .Net web apps) when CPU usage went high for extended periods of time. With multiple web apps running on a machine, this helped identify the source of the issue.

Usage:

`cpu-analyzer -p ProcessName|PID [options]`

-s indicates how many samples to take (default:10)

-i the interval between samples in milliseconds (default:1000)

Example: `cpu-analyzer -p w3wp -s 60 -i 500` - "Take 60 samples once every 500 milliseconds for all processes named 'w3wp'"

The tool output can be quite lengthy, so you'll want to write the console output to a file like this:

`cpu-analyzer.exe -p w3wp >> log.txt`

To take it a step further, I was trying to capture a thread snapshot during intermittent CPU spikes. I set up a PowerShell script to run on a scheduled task once every minute. The script would check the current CPU load, and if it was above a preset threshold, it would launch the CPU Analyzer, and save the output to a time-stamped file. Here's an example script:

```PowerShell
$cpu = Get-Counter '\Processor(_Total)\% Processor Time' | Foreach-Object {$_.CounterSamples[0].CookedValue}
Write-Output "CPU = $cpu %"
$threshold = 50
Write-Output "Threshold = $threshold %"
$aboveThreshold = ($cpu -gt $threshold)

If($aboveThreshold){
    Write-Output "Above threshold, running CPU analyzer..."
    $sout = C:\cpu-analyzer.exe -p w3wp 2>$null
    $logFileName = ("C:\log\W3WP_" + $(get-date -f yyyy-MM-dd-HHmmss) + "_CPU-" + ($cpu -as [int]) + "pct.txt")
    Write-Output "Logging to: $logFileName"
    $sout | Out-File $logFileName
}
```
