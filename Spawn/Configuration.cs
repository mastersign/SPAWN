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
        public string Application { get; set; }

        public int MaxConcurrency { get; set; }

        public string ArgumentFormat { get; set; }

        public string AbsoluteApplicationPath
        {
            get
            {
                return Path.IsPathRooted(Application)
                           ? Application
                           : Path.Combine(
                                Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? "", 
                                Application);
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

        public static int DefaultMaxConcurrency { get { return Environment.ProcessorCount; } }

        public static string DefaultArgumentFormat { get { return "\"{0}\""; } }
    }
}
