using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json;
using VideoLibrary;

using Autobox.Core.Data;

namespace Autobox.Core.Services.VideoLibrary
{
    public class TrackLibrary : ITrackLibrary
    {
        // ### Public API
        public TrackLibrary(string rootPath)
        {
            Client = YouTube.Default;
            RootPath = rootPath;
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }
        }

        // ##### LoadAllAsync
        // Load the whole library
        public Task LoadAllAsync()
        {
            Index.Load(RootPath);
            JsonSerializer serializer = new JsonSerializer();
            foreach (string metadataFile in Index.MetadataFiles)
            {
                string json = File.ReadAllText(metadataFile);
                Track track = ProcessLoadedTrack(JsonConvert.DeserializeObject<Track>(json));
                if (!TrackList.ContainsKey(track.Title))
                {
                    TrackList.Add(track.Title, track);
                    UpdateTagList(track.Tags);
                }
            }

            return Task.CompletedTask;
        }

        // ##### ProcessLoadedTrack
        // Process a newly loaded track to ensure is content is compatible
        private Track ProcessLoadedTrack(Track track)
        {
            track.Tags = new HashSet<string>(track.Tags.ToList().ConvertAll(str => str.ToUpper()));
            return track;
        }

        // ##### CreateTrack
        // Create a track from a YouTube link
        // Throw an exception if track already exists in library
        public async Task<Track> CreateTrackAsync(string link)
        {
            string id = ProcessTrackId(link);
            Video video = await Client.GetVideoAsync(link);
            string title = ProcessTrackTitle(video);
            string videoFilePath = BuildTrackVideoFilePath(video, id);
            string metadataFilePath = BuildTrackMetadataFilePath(id);
            using (FileStream stream = File.Create(videoFilePath))
            {
                byte[] bytes = await video.GetBytesAsync();
                await stream.WriteAsync(bytes, 0, bytes.Count());
            }

            if (TrackList.ContainsKey(title))
            {
                throw new TrackAlreadyExistsException(title);
            }

            Track track = new Track
            {
                Id = id,
                Title = title,
                VideoFilePath = videoFilePath,
                MetadataFilePath = metadataFilePath
            };

            await UpdateTrackMetadataAsync(track);
            TrackList.Add(track.Title, track);
            return track;
        }

        // ##### UpdateTrackAsync
        // Update track metadata
        public Task UpdateTrackAsync(Track track) { return UpdateTrackMetadataAsync(track); }

        // ##### CreateTagList
        // Create a tag list from a row text
        // Return the list of tag ids
        public HashSet<string> CreateTagList(string text)
        {
            HashSet<string> values = Tag.ExtractTagValues(text);
            UpdateTagList(values);
            return values;
        }

        // ### Dal
        private async Task UpdateTrackMetadataAsync(Track track)
        {
            using (FileStream stream = File.Open(track.MetadataFilePath, FileMode.Create))
            {
                string json = JsonConvert.SerializeObject(track, Formatting.Indented);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        // ### Internal track stuff
        private string ProcessTrackId(string link)
        {
            Regex regex = new Regex(@"https:\/\/www\.youtube\.com\/watch\?v=(?<id>[a-zA-Z0-9_\-]+)");
            Match match = regex.Match(link);
            if (!match.Success)
            {
                throw new InvalidTrackLinkException(link);
            }
            return match.Groups["id"].ToString();
        }
        private string ProcessTrackTitle(Video video) { return video.Title.Replace("- YouTube", "").TrimEnd(' '); }
        private string BuildTrackVideoFilePath(Video video, string id) { return Path.Combine(RootPath, id + video.FileExtension); }
        private string BuildTrackMetadataFilePath(string id) { return Path.Combine(RootPath, id + Index.MetadataFileExt); }

        // ### Internal tag stuff
        private void UpdateTagList(HashSet<string> values)
        {
            Random random = new Random();
            foreach (string value in values)
            {
                if (!TagList.ContainsKey(value))
                {
                    Tag tag = new Tag
                    {
                        Value = value,
                    };
                    TagList.Add(value, tag);
                }
            }
        }

        // ##### Attributes
        // library configuration
        private readonly YouTube Client;
        private readonly string RootPath;
        // library files
        private readonly LibraryIndex Index = new LibraryIndex();
        public Dictionary<string, Track> TrackList { get; private set; } = new Dictionary<string, Track>();
        public Dictionary<string, Tag> TagList { get; protected set; } = new Dictionary<string, Tag>();
    }
}
