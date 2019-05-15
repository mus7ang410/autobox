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
        void Load(string directory, LibraryMetadata metadata);

        Task AddTrackAsync(TrackMetadata track);
        Task UpdateTrackAsync(TrackMetadata track);
        Task DeleteTrackAsync(TrackMetadata track);

        // ##### Properties
        LibraryMetadata Metadata { get; }
        Dictionary<string, TrackMetadata> TrackList { get; }
        TagCollection TagList { get; }
    }
}
