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
        void Shuffle();
        void Forward();

        // ##### Attributes
        int Count { get; }
        Track CurrentTrack { get; }
        List<Track> NextTracks { get; }
    }
}
