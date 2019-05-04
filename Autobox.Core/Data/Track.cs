using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autobox.Core.Data
{
    public class Track
    {
        // ##### MatchFilter
        // Check if this track contains none of excluded list and a given list of included
        // EIncludeMatchType.Optional: Track does not need to match includes tags
        // EIncludeMatchType.Any: Track needs to match at least one of included tags
        // EIncludeMatchType.All: Track needs to match the whole list of included tags
        // Empty include list is not checked
        public enum EIncludeMatchType { Optional, Any, All }
        public bool MatchFilter(HashSet<string> toExclude, HashSet<string> toInclude, EIncludeMatchType matchType)
        {
            if (toExclude != null && Tags.Intersect(toExclude).Any())
            {
                return false;
            }
            switch (matchType)
            {
                case EIncludeMatchType.Any:
                    if (toInclude != null && toInclude.Count > 0 && Tags.Intersect(toInclude).Count() == 0)
                    {
                        return false;
                    }
                    break;

                case EIncludeMatchType.All:
                    if (toInclude != null && toInclude.Count > 0 && Tags.Intersect(toInclude).Count() != toInclude.Count)
                    {
                        return false;
                    }
                    break;

                default:
                    break;
            }

            return true;
        }

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
