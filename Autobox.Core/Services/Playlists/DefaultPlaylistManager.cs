using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autobox.Core.Data;

namespace Autobox.Core.Services.Playlists
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
            TrackList = TrackLibrary.TrackList.Values.ToList();
            int n = TrackList.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Track value = TrackList[k];
                TrackList[k] = TrackList[n];
                TrackList[n] = value;
            }
            CurrentTrackIndex = 0;
        }

        // ##### Forward
        // Go to next track or to first one if list is over
        public void Forward()
        {
            ++CurrentTrackIndex;
            if (CurrentTrackIndex >= TrackList.Count)
            {
                CurrentTrackIndex = 0;
            }
        }

        // ##### Attributes
        private readonly ITrackLibrary TrackLibrary;
        private List<Track> TrackList = new List<Track>();
        private int CurrentTrackIndex = 0;
        public Track PreviousTrack => CurrentTrackIndex > 0 ? TrackList[CurrentTrackIndex - 1] : TrackList[TrackList.Count - 1];
        public Track CurrentTrack => TrackList[CurrentTrackIndex];
        public Track NextTrack => CurrentTrackIndex < TrackList.Count - 1 ? TrackList[CurrentTrackIndex + 1] : TrackList[0];
    }
}
