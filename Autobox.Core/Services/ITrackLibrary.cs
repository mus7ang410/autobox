using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autobox.Core.Data;

namespace Autobox.Core.Services
{
    public interface ITrackLibrary
    {
        Task LoadAllAsync();
        Task<Track> CreateTrackAsync(string url);
        Task UpdateTrackAsync(Track track);
        HashSet<string> CreateTagList(string text);

        Dictionary<string, Track> TrackList { get; }
        Dictionary<string, Tag> TagList { get; }
    }
}
