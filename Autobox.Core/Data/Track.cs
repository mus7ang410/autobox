using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Autobox.Core.Data
{
    public class TrackInvalidTitleException : Exception
    {
        public TrackInvalidTitleException(string reason) :
            base(reason)
        {

        }
    }

    // ##### TrackEventArgs
    // Encapsulate a track to send as event
    public class TrackEventArgs : EventArgs
    {
        public TrackEventArgs(Track track)
        {
            Track = track;
        }

        public Track Track { get; set; }
    }

    public class Track : INotifyPropertyChanged
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

        // ##### MatchFilter
        // Check if this track title match the given string filter
        public bool MatchFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }

            if (Title.ToUpper().Contains(filter.ToUpper()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString() { return $"{Title} - ({Id})"; }

        // ##### Configuration
        public static readonly string MetadataFileExt = ".metadata.json";
        public static readonly string ThumbnailFileExt = ".thumbnail.jpg";

        // ##### Events
        public event PropertyChangedEventHandler PropertyChanged;
        // ##### Attributes
        public string Id { get; set; }
        [JsonIgnore]
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new TrackInvalidTitleException("Track title cannot be empty");
                }
                _Title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));
            }
        }
        public int Rating { get; set; } = 0;
        public string VideoFilename { get; set; }
        public string ThumbnailFilename { get; set; }
        public string MetadataFilename { get; set; }
        public TagCollection Tags { get; set; } = new TagCollection();
    }
}
