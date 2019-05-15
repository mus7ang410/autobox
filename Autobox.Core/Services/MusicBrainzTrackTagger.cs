using System;
using System.Collections.Generic;
using System.Text;
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
            JObject recording = await GetRecording(track);
            ExtractArtists(track, recording);
            ExtractPeriod(track, recording);
            ExtractTags(track, recording);
        }

        // ##### GetRecording
        // Get associated track recording from MusicBrainz
        // Or null if no recording could be found
        private async Task<JObject> GetRecording(TrackMetadata track)
        {
            List<JObject> releases = new List<JObject>();
            string uri = $"https://musicbrainz.org/ws/2/release?query={track.Title}&fmt=json";
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

        // ##### ExtractArtists
        // Extract artists from recording releases
        private void ExtractArtists(TrackMetadata track, JObject recording)
        {
            try
            {
                JToken artists = recording["releases"].First["artist-credit"];
                foreach (JObject artist in artists)
                {
                    track.Tags.Add(artist["artist"]["name"].ToString().ToLower());
                }
            }
            catch (Exception) { }
        }

        // ##### ExtractPeriod
        // Generate period tag from recording
        private void ExtractPeriod(TrackMetadata track, JObject recording)
        {
            try
            {
                JToken date = recording["releases"].First["date"];
                int year = int.Parse(date.ToString().Substring(0, 4));
                int decade = (year % 100) / 10;
                track.Tags.Add($"{decade}0s");
            }
            catch (Exception) { }
        }

        // ##### ExtractTags
        // Extract tag from recording releases
        private void ExtractTags(TrackMetadata track, JObject recording)
        {
            try
            {
                foreach (JObject release in recording["releases"])
                {
                    if (int.Parse(release["score"].ToString()) > 50)
                    {
                        if (release.ContainsKey("tags"))
                        {
                            foreach (JObject tag in release["tags"])
                            {
                                track.Tags.Add(tag["name"].ToString().ToLower());
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
