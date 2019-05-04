using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Autobox.Core.Data;

namespace Autobox.Core.Services
{
    public class AutoboxPlaylistManager : IPlaylistManager
    {
        public AutoboxPlaylistManager(ITrackLibrary library)
        {
            Library = library;
            Shuffle();
        }

        // ##### Shuffle
        // Create a randomized list containing all library
        public Task Shuffle()
        {
            return Task.Run(() =>
            {
                List<Track> matchingTracks = GetMatchingTracks();
                // ordering
                Dictionary<ERatingValue, List<Track>> sortedTracks = new Dictionary<ERatingValue, List<Track>>
                {
                    { ERatingValue.High, Randomize(matchingTracks.Where(track => track.Rating == 0 || track.Rating >= 4).ToList()) },
                    { ERatingValue.Medium, Randomize(matchingTracks.Where(track => track.Rating > 0 && track.Rating <= 2).ToList()) },
                    { ERatingValue.Low, Randomize(matchingTracks.Where(track => track.Rating > 0 && track.Rating <= 2).ToList()) }
                };

                int total = matchingTracks.Count;

                TrackList = new List<Track>();
                for (int i = 0; i < total; i++)
                {
                    ERatingValue nextRating = GetNextTrackRating(sortedTracks);
                    if (nextRating == ERatingValue.None)
                    {
                        break;
                    }
                    else
                    {
                        TrackList.Add(sortedTracks[nextRating].First());
                        sortedTracks[nextRating].RemoveAt(0);
                    }
                }
            });
        }

        private ERatingValue GetNextTrackRating(Dictionary<ERatingValue, List<Track>> tracks)
        {
            int dice = Rnd.Next(TotalRatingChance);
            if (dice < RatingChances[ERatingValue.High] && tracks[ERatingValue.High].Count > 0)
            {
                return ERatingValue.High;
            }

            if (dice < RatingChances[ERatingValue.High] + RatingChances[ERatingValue.Medium] && tracks[ERatingValue.Medium].Count > 0)
            {
                return ERatingValue.Medium;
            }
            else if (tracks[ERatingValue.Low].Count > 0)
            {
                return ERatingValue.Low;
            }

            return ERatingValue.None;
        }

        private List<Track> GetMatchingTracks()
        {
            HashSet<Track> tracks = new HashSet<Track>(Library.TrackList.Values.Where(track => track.MatchFilter(ExcludedTagList, OptionalTagList, Track.EIncludeMatchType.Any)).ToArray());
            tracks.UnionWith(new HashSet<Track>(Library.TrackList.Values.Where(track => track.MatchFilter(ExcludedTagList, IncludedTagList, Track.EIncludeMatchType.All)).ToArray()));
            return tracks.ToList();
        }

        private List<Track> Randomize(List<Track> tracks)
        {
            int n = tracks.Count;
            while (n > 1)
            {
                n--;
                int k = Rnd.Next(n + 1);
                Track value = tracks[k];
                tracks[k] = tracks[n];
                tracks[n] = value;
            }
            return tracks;
        }

        // ##### Properties
        public readonly HashSet<string> ExcludedTagList = new HashSet<string>();
        public readonly HashSet<string> OptionalTagList = new HashSet<string>();
        public readonly HashSet<string> IncludedTagList = new HashSet<string>();

        // ##### Configuration
        private enum ERatingValue { High, Medium, Low, None };
        private readonly Dictionary<ERatingValue, int> RatingChances = new Dictionary<ERatingValue, int>
        {
            { ERatingValue.High, 60 },
            { ERatingValue.Medium, 30 },
            { ERatingValue.Low, 15 },
            { ERatingValue.None, 0 }
        };
        private int TotalRatingChance => RatingChances.Aggregate(0, (current, item) => { return current + item.Value; });

        // ##### Attributes
        private readonly ITrackLibrary Library;
        private readonly Random Rnd = new Random();
        public List<Track> TrackList { get; private set; } = new List<Track>();
    }
}
