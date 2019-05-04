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
        public Task Shuffle()
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

            return Task.CompletedTask;
        }

        // ##### Attributes
        private readonly ITrackLibrary TrackLibrary;
        public List<Track> TrackList { get; private set; } = new List<Track>();
    }
}
