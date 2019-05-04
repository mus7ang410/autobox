using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autobox.Core.Data;

namespace Autobox.Core.Services
{
    public class DefaultPlaylistManager : IPlaylistManager
    {
        public DefaultPlaylistManager(ITrackLibrary trackLibrary)
        {
            TrackLibrary = trackLibrary;
            Shuffle();
        }

        // ##### Shuffle
        // Create a randomized list containing all library
        public void Shuffle()
        {
            Random random = new Random();
            NextTracks = TrackLibrary.TrackList.Values.ToList();
            int n = NextTracks.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Track value = NextTracks[k];
                NextTracks[k] = NextTracks[n];
                NextTracks[n] = value;
            }
            Forward();
        }

        // ##### Forward
        // Go to next track or to first one if list is over
        public void Forward()
        {
            if (NextTracks.Count > 0)
            {
                CurrentTrack = NextTracks.First();
                NextTracks.RemoveAt(0);
            }
        }

        // ##### Attributes
        private readonly ITrackLibrary TrackLibrary;
        public int Count => CurrentTrack != null ? 1 + (NextTracks != null ? NextTracks.Count : 0) : 0;
        public Track CurrentTrack { get; private set; }
        public List<Track> NextTracks { get; private set; } = new List<Track>();
    }
}
