using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autobox.Core.Data
{
    public class Track
    {
        // ##### Attribute
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> Artists { get; set; } = new List<string>();
        public string VideoFilePath { get; set; }
        public string MetadataFilePath { get; set; }
        public HashSet<string> Tags { get; set; } = new HashSet<string>();
    }
}
