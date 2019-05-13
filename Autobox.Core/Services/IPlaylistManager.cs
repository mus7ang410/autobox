using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autobox.Core.Data;

namespace Autobox.Core.Services
{
    public interface IPlaylistManager
    {
        Task Shuffle();

        // ##### Attributes
        PlaylistSettings Settings { get; }
        List<TrackMetadata> TrackList { get; }
    }
}
