using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Autobox.Core.Services
{
    public class CacheFile
    {
        public static TCache Load<TCache>(string filename) where TCache : new()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filepath = Path.Combine(new string[] { dir, CacheDirName, filename });
            if (File.Exists(filepath))
            {
                string json = File.ReadAllText(Path.Combine(dir, filename));
                TCache cache = JsonConvert.DeserializeObject<TCache>(json);
                return cache;
            }
            return new TCache();
        }

        public static void Save<TCache>(string filename, TCache cache) where TCache : new()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filepath = Path.Combine(new string[] { dir, CacheDirName, filename });
            if (!Directory.Exists(Path.Combine(dir, CacheDirName)))
            {
                Directory.CreateDirectory(Path.Combine(dir, CacheDirName));
            }
            string json = JsonConvert.SerializeObject(cache);
            File.WriteAllText(filepath, json);
        }

        private static readonly string CacheDirName = "Autobox";
    }
}
