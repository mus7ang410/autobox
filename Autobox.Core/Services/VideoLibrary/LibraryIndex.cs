using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autobox.Core.Services.VideoLibrary
{
    public class LibraryIndex
    {
        public void Load(string root)
        {
            MetadataFiles = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(root, "*" + MetadataFileExt))
            {
                MetadataFiles.Add(filename);
            }
        }

        // ##### Attributes
        public readonly string MetadataFileExt = ".metadata.json";
        public List<string> MetadataFiles = new List<string>();
    }
}
