using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autobox.Core.Data
{
    public class Track
    {
        // ##### Configuration
        public static readonly string MetadataFileExt = ".metadata.json";
        public static readonly string ThumbnailFileExt = ".thumbnail.jpg";

        // ##### Attributes
        public string Id { get; set; }
        public string Title { get; set; }
        public int Rating { get; set; } = 0;
        public string VideoFilename { get; set; }
        public string ThumbnailFilename { get; set; }
        public string MetadataFilename { get; set; }
        public HashSet<string> Tags { get; set; } = new HashSet<string>();
    }
}
