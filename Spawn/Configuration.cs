using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace de.mastersign.spawn
{
    public class Configuration
    {
        public int MaxConcurrency { get; set; }

        public string Application { get; set; }

        public string ArgumentFormat { get; set; }

        public bool FullCommands { get; set; }

        public bool TasksFromStandardInput { get; set; }

        private string GetEnvironmentedPath(string relativePath)
        {
            var basePaths = new[] { Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) };
            var commandPaths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(new[] { Path.PathSeparator });
            var paths = basePaths.Concat(commandPaths);
            return (
                from bp in paths
                let ap = Path.Combine(bp, relativePath)
                where File.Exists(ap)
                select bp
            ).FirstOrDefault();
        }

        public string AbsoluteApplicationPath
        {
            get
            {
                return Path.IsPathRooted(Application)
                           ? Application
                           : Path.Combine(GetEnvironmentedPath(Application) ?? "", Application);
            }
        }

        public static Configuration LoadFromFile(string file)
        {
            if (!File.Exists(file)) return null;
            var xs = new XmlSerializer(typeof(Configuration));
            using (var s = File.OpenRead(file))
            {
                return xs.Deserialize(s) as Configuration;
            }
        }

        public void SaveToFile(string file)
        {
            var xs = new XmlSerializer(typeof(Configuration));
            using (var s = File.Open(file, FileMode.Create))
            {
                xs.Serialize(s, this);
            }
        }

        public static int DefaultMaxConcurrency { get { return Environment.ProcessorCount; } }

        public static string DefaultApplication
        {
            get
            {
                var myPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
                var dir = Path.GetDirectoryName(myPath) ?? "";
                var executables = Directory.EnumerateFiles(dir, "*.exe")
                                           .Concat(Directory.EnumerateFiles(dir, "*.bat"))
                                           .Concat(Directory.EnumerateFiles(dir, "*.cmd"))
                                           .Where(p => !myPath.Equals(p, StringComparison.InvariantCultureIgnoreCase))
                                           .OrderBy(p => p);
                return Path.GetFileName(executables.FirstOrDefault());
            }
        }

        public static string DefaultArgumentFormat { get { return "\"{0}\""; } }

        public static bool DefaultFullCommands { get { return false; } }

        public static bool DefaultTasksFromStandardInput { get { return false; } }
    }
}
