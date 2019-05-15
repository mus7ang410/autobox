using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autobox.Data;

namespace Autobox.Services
{
    public interface ILibrary
    {
        // ##### Interface
        Task LoadAllAsync();
        Task AddTrackAsync(TrackMetadata track);
        Task UpdateTrackAsync(TrackMetadata track);
        Task DeleteTrackAsync(TrackMetadata track);

        string BuildThumbnailFilePath(string trackId);
        string GetThumbnailFilepath(TrackMetadata track);
        string BuildVideoFilePath(string trackId, string ext);
        string GetVideoFilepath(TrackMetadata track);

        // ##### Properties
        Dictionary<string, TrackMetadata> TrackList { get; }
        TagCollection TagList { get; }
    }
}
