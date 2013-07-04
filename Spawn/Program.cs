using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace de.mastersign.spawn
{
    class Program
    {
        private static Configuration config;

        static int Main(string[] args)
        {
            LoadConfiguration();
            if (args.Length == 0 || config == null)
            {
                return InitializeConfiguration() ? 0 : -1;
            }

            if (!CheckApplication() || 
                !CheckArgumentFormat())
            {
                return -1;
            }

            RunTasks(args);

            return 0;
        }

        static string ConfigFilePath
        {
            get { return Path.ChangeExtension(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath, ".cfg"); }
        }

        static void LoadConfiguration()
        {
            config = Configuration.LoadFromFile(ConfigFilePath);
        }

        static string ReadParameter(string prompt, string def = null)
        {
            Console.Write(def != null
                ? string.Format("{0} [{1}]: ", prompt, def)
                : string.Format("{0}: ", prompt));
            var res = Console.ReadLine();
            if (res != null && res.Trim() == string.Empty) res = null;
            return res ?? def;
        }

        static bool CheckApplication()
        {
            if (!File.Exists(config.AbsoluteApplicationPath))
            {
                Console.WriteLine("Could not find application:\n\t" + config.AbsoluteApplicationPath);
                return false;
            }
            return true;
        }

        static bool CheckArgumentFormat()
        {
            try
            {
                var tmp = string.Format(config.ArgumentFormat, "task");
                return true;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        static bool InitializeConfiguration()
        {
            config = config ?? new Configuration();
            config.Application = ReadParameter(
                "Application", config.Application ?? Configuration.DefaultApplication);
            config.ArgumentFormat = ReadParameter(
                "Argument Format", config.ArgumentFormat ?? Configuration.DefaultArgumentFormat);
            config.MaxConcurrency = int.Parse(ReadParameter(
                "Max Concurrency", Configuration.DefaultMaxConcurrency.ToString()));

            if (!CheckApplication() ||
                !CheckArgumentFormat())
            {
                return false;
            }

            config.SaveToFile(ConfigFilePath);
            return true;
        }

        private static ConcurrentQueue<string> tasks;

        private static void RunTasks(IList<string> args)
        {
            tasks = new ConcurrentQueue<string>();
            foreach (var s in args)
            {
                tasks.Enqueue(s);
            }

            Thread[] worker;
            if (config.MaxConcurrency > 0)
            {
                worker = Enumerable
                    .Range(0, config.MaxConcurrency)
                    .Select(i => new Thread(() => Worker(i)))
                    .ToArray();
            }
            else
            {
                worker = Enumerable
                    .Range(0, args.Count)
                    .Select(i => new Thread(() => RunTask(i, args[i])))
                    .ToArray();
            }
            foreach (var thread in worker) thread.Start();
            foreach (var thread in worker) thread.Join();
        }

        private static void Worker(int id)
        {
            while (tasks.Count > 0)
            {
                string task;
                if (!tasks.TryDequeue(out task)) continue;
                RunTask(id, task);
            }
        }

        private static void RunTask(int workerId, string task)
        {
            var p = Process.Start(
                config.AbsoluteApplicationPath,
                string.Format(config.ArgumentFormat, task));
            if (p != null)
            {
                Console.WriteLine("Worker {0} started task:\n\t{1}", workerId, task);
                p.WaitForExit();
            }
        }
    }
}
