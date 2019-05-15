using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Autobox.Data;

namespace Autobox.Services
{
    public class AutoboxPlaylistManager : IPlaylistManager
    {
        public AutoboxPlaylistManager(ITrackLibrary library)
        {
            Library = library;
            Shuffle();
        }

        // ##### Shuffle
        // Create a randomized list based on input tags & track ratings
        public Task Shuffle()
        {
            return Task.Run(() =>
            {
                List<TrackMetadata> matchingTracks = GetMatchingTracks();
                // ordering
                Dictionary<ERatingValue, List<TrackMetadata>> sortedTracks = new Dictionary<ERatingValue, List<TrackMetadata>>
                {
                    { ERatingValue.High, Randomize(matchingTracks.Where(track => track.Rating == 0 || track.Rating >= 4).ToList()) },
                    { ERatingValue.Medium, Randomize(matchingTracks.Where(track => track.Rating > 2 && track.Rating < 4).ToList()) },
                    { ERatingValue.Low, Randomize(matchingTracks.Where(track => track.Rating > 0 && track.Rating <= 2).ToList()) }
                };

                int total = matchingTracks.Count;

                TrackList = new List<TrackMetadata>();
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

        private ERatingValue GetNextTrackRating(Dictionary<ERatingValue, List<TrackMetadata>> tracks)
        {
            int dice = Rnd.Next(TotalRatingChance);
            int highRatingChance = RatingChances[ERatingValue.High];
            int mediumRatingChance = RatingChances[ERatingValue.High] + RatingChances[ERatingValue.Medium];
            int lowRatingChance = RatingChances[ERatingValue.High] + RatingChances[ERatingValue.Medium] + RatingChances[ERatingValue.Low];

            if (tracks[ERatingValue.High].Count == 0)
            {
                highRatingChance = 0;
            }
            if (tracks[ERatingValue.Medium].Count == 0)
            {
                mediumRatingChance = highRatingChance;
            }
            if (tracks[ERatingValue.Low].Count == 0)
            {
                if (tracks[ERatingValue.Medium].Count != 0)
                {
                    mediumRatingChance = lowRatingChance;
                }
                else
                {
                    highRatingChance = lowRatingChance;
                }
            }

            if (dice < highRatingChance)
            {
                return ERatingValue.High;
            }

            if (dice < mediumRatingChance)
            {
                return ERatingValue.Medium;
            }
            else if (dice < lowRatingChance)
            {
                return ERatingValue.Low;
            }

            return ERatingValue.None;
        }

        private List<TrackMetadata> GetMatchingTracks()
        {
            HashSet<TrackMetadata> anyOf = new HashSet<TrackMetadata>(Library.TrackList.Values.Where(track => track.MatchFilter(Settings.NoneOfTagList, Settings.AnyOfTagList, TrackMetadata.EIncludeMatchType.Any)).ToList());
            HashSet<TrackMetadata> allOf = new HashSet<TrackMetadata>(Library.TrackList.Values.Where(track => track.MatchFilter(Settings.NoneOfTagList, Settings.AllOfTagList, TrackMetadata.EIncludeMatchType.All)).ToList());
            return anyOf.Intersect(allOf).ToList();
        }

        private List<TrackMetadata> Randomize(List<TrackMetadata> tracks)
        {
            int n = tracks.Count;
            while (n > 1)
            {
                n--;
                int k = Rnd.Next(n + 1);
                TrackMetadata value = tracks[k];
                tracks[k] = tracks[n];
                tracks[n] = value;
            }
            return tracks;
        }

        // ##### Properties
        public PlaylistSettings Settings {get; set; } = new PlaylistSettings();

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
        public List<TrackMetadata> TrackList { get; private set; } = new List<TrackMetadata>();
    }
}
