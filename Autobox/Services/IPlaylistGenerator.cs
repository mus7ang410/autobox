using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autobox.Data;

namespace Autobox.Services
{
    public interface IPlaylistGenerator
    {
        Task Shuffle();

        // ##### Attributes
        PlaylistSettings Settings { get; }
        List<TrackMetadata> TrackList { get; }
    }
}
