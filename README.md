SPAWN
=====

SPAWN is a small command line tool for Windows to execute a number of tasks in 
parallel.

Usage
-----

You want to process a number of files with a command line tool of your choice,
but you don't have the time to process each file after another and but you don't
want to spin up more processes then, lets say, the number of your CPU cores.

1. Copy `spawn.exe` in the folder of your command line tool. 
   E.g. a Windows batch file.
2. Run `spawn.exe` without any command line arguments and answer the 
   following questions (entering nothing takes the default value):
	1. Application: The path to your tool.
	2. Argument Format: The command line arguments you want to pass to 
	   your tool, where `{0}` stands for the file to process.
	3. Max Concurrency: The maximum number of parallel working tasks, 
	   0 for all tasks in parallel.
3. Pass all files you want to process as command line arguments to `spawn.exe`,
   e.g. by Drag&Drop the files from the Windows Explorer on `spawn.exe`.

### Example

	> spawn.exe
	   Application: mytool.bat
	   Argument Format ["{0}"]: 
	   Max Concurrency [8]: 4
	> spawn.exe file1 file2 file3 file4 file5 file6

The result is SPAWN running `mytool.bat "file1"`, `mytool.bat "file2"`, until
`file4` in parallel, waits for on of the `mytool.bat` instances to finish 
and immediately starts `mytool.bat` with the next waiting file until all
given files are processed.  

Prerequisites
-------------

* Microsoft Windows
* Microsoft .NET 4.0 or higher
