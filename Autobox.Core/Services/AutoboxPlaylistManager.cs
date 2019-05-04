using System;
using System.Collections.Generic;
using System.Text;

using Autobox.Core.Data;

namespace Autobox.Core.Services
{
    public class AutoboxPlaylistManager : IPlaylistManager
    {
        public AutoboxPlaylistManager(ITrackLibrary trackLibrary)
        {
            TrackLibrary = trackLibrary;
            Shuffle();
        }

        // ##### Shuffle
        // Create a randomized list containing all library
        public void Shuffle()
        {

        }

        // ##### Attributes
        private readonly ITrackLibrary TrackLibrary;
        public List<Track> TrackList { get; private set; } = new List<Track>();
    }
}
