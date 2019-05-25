using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using Autobox.Data;

namespace Autobox.Services
{
    public class MusicBrainzTrackTagger : ITrackTagger
    {
        public MusicBrainzTrackTagger()
        {
            Client = new HttpClient();
        }

        public async Task TagTrackAsync(TrackMetadata track)
        {
            JObject recordings = await GetRecording(track);
            NormalizeTitle(track, recordings);
            ExtractPeriod(track, recordings);
            ExtractTags(track, recordings);
        }

        // ##### GetRecording
        // Get associated track recordings from MusicBrainz
        // Or null if no recordings could be found
        private async Task<JObject> GetRecording(TrackMetadata track)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string computed = rgx.Replace(track.Title, "");
            string uri = $"https://musicbrainz.org/ws/2/recording?query={computed}&fmt=json&limit=10";
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                request.Headers.Add("User-Agent", "Autobox/beta (cfx.corporation@gmail.com)");
                using (HttpResponseMessage response = await Client.SendAsync(request))
                {
                    string content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<JObject>(content);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        // ##### GetBestRecording
        // Get the recording with the maximum score
        private JObject GetBestRecording(JObject recordings)
        {
            JObject bestRecording = null;
            int bestScore = 0;
            foreach (JObject recording in recordings["recordings"])
            {
                int score = int.Parse(recording["score"].ToString());
                if (score > bestScore)
                {
                    bestScore = score;
                    bestRecording = recording;
                }
            };
            return bestRecording;
        }

        // ##### NormalizeTitle
        // Attempt to normalize title based on track title and artists
        // Add artist names to tag list
        private void NormalizeTitle(TrackMetadata track, JObject recordings)
        {
            try
            {
                string artists = string.Empty;
                foreach (JObject artist in GetBestRecording(recordings)["artist-credit"])
                {
                    track.Tags.Add(new Tag(Tag.ECategory.Artist, artist["artist"]["name"].ToString()));
                    artists += artist["artist"]["name"].ToString();
                    if (artist.ContainsKey("joinphrase"))
                    {
                        artists += artist["joinphrase"].ToString();
                    }
                }

                string title = GetBestRecording(recordings)["title"].ToString();
                track.Title = $"{title} - {artists}";
            }
            catch (Exception) { }
        }

        // ##### ExtractPeriod
        // Generate period tag from recordings
        private void ExtractPeriod(TrackMetadata track, JObject recordings)
        {
            try
            {
                int MaxYear = 9999;
                int minYear = MaxYear;
                foreach (JObject recording in recordings["recordings"])
                {
                    if (int.Parse(recording["score"].ToString()) > 50)
                    {
                        if (recording.ContainsKey("date") && recording["date"].ToString().Length > 4)
                        {
                            int year = int.Parse(recording["date"].ToString().Substring(0, 4));
                            if (year < minYear)
                            {
                                minYear = year;
                            }
 
                        }

                        foreach (JObject release in recording["releases"])
                        {
                            if (release.ContainsKey("date") && release["date"].ToString().Length > 4)
                            {
                                int year = int.Parse(release["date"].ToString().Substring(0, 4));
                                if (year < minYear)
                                {
                                    minYear = year;
                                }
                            }
                        }
                    }
                }

                if (minYear < MaxYear)
                {
                    int decade = (minYear % 100) / 10;
                    track.Tags.Add(new Tag(Tag.ECategory.Period, $"{decade}0S"));
                }
            }
            catch (Exception) { }
        }

        // ##### ExtractTags
        // Extract tag from recordings releases
        private void ExtractTags(TrackMetadata track, JObject recordings)
        {
            try
            {
                foreach (JObject recording in recordings["recordings"])
                {
                    if (int.Parse(recording["score"].ToString()) > 50)
                    {
                        if (recording.ContainsKey("tags"))
                        {
                            foreach (JObject tag in recording["tags"])
                            {
                                track.Tags.Add(new Tag(Tag.ECategory.Genre, tag["name"].ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        // ##### Attributes
        private HttpClient Client;
    }
}
