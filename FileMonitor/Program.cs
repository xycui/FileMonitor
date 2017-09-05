using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileMonitor
{
    internal class Program
    {
        private static string _commandPattern;

        internal static void Main(string[] args)
        {
            string path;
            string fileFilter;
            _commandPattern = string.Empty;
            if (args.Length == 0)
            {
                Console.WriteLine("Enter the path you want to monitor:");
                path = Console.ReadLine();
                Console.WriteLine("Enter the file filter for the monitor: (eg: *.txt or test.txt)");
                fileFilter = Console.ReadLine();
                Console.WriteLine("Enter the command you want to execute after your file is changed. ");
                _commandPattern = Console.ReadLine();
            }
            else if (args.Length != 3)
            {
                Console.WriteLine("Arguments count is not correct");
                Console.ReadLine();
                return;
            }
            else
            {
                path = args[0];
                fileFilter = args[1];
                _commandPattern = args[2];
            }

            var fileWatcher = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = fileFilter
            };
            fileWatcher.Changed +=FileWatcherOnChanged;
            
            fileWatcher.EnableRaisingEvents = true;

            Console.WriteLine("Press \'q\'to quit the sample.");
            while (Console.Read() != 'q')
            {
            }
        }

        private static void FileWatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            FileSystemWatcher watcher = null;
            try
            {
                watcher = (FileSystemWatcher) sender;
                watcher.EnableRaisingEvents = false;

                var path = fileSystemEventArgs.FullPath;
                if (path.Contains(' '))
                {
                    path = $"\"{path}\"";
                }
                var cmd = string.Format(_commandPattern, path).Trim();
                var spaceIndex = cmd.IndexOf(' ');
                var fileName = cmd;
                var arguments = string.Empty;
                if (spaceIndex != -1)
                {
                    fileName = cmd.Substring(0, spaceIndex);
                    arguments = cmd.Substring(spaceIndex + 1, cmd.Length - spaceIndex - 1);
                }
                var procStartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var proc = Process.Start(procStartInfo);
                proc.OutputDataReceived += (o, args) =>
                {
                    Console.WriteLine(args.Data);
                };

                proc.ErrorDataReceived += (o, args) =>
                {
                    Console.WriteLine(args.Data);
                };
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
            }
            finally
            {
                watcher.EnableRaisingEvents = true;
            }
        }
    }
}
