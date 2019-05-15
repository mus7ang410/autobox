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
        void Load(string directory);
        Task ImportAsync(LibraryMetadata library);
        LibraryMetadata Export();

        Task AddTrackAsync(TrackMetadata track);
        Task UpdateTrackAsync(TrackMetadata track);
        Task DeleteTrackAsync(TrackMetadata track);

        // ##### Properties
        Dictionary<string, TrackMetadata> TrackList { get; }
        TagCollection TagList { get; }
    }
}
