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
        Task AddTrackAsync(Track track);
        Task UpdateTrackAsync(Track track);
        Task DeleteTrackAsync(Track track);
        TagCollection CreateTagList(string text);

        string GetFilePath(string filename);

        Dictionary<string, Track> TrackList { get; }
        TagCollection TagList { get; }
    }
}
