using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using VideoLibrary;

using Autobox.Core.Data;

namespace Autobox.Core.Services
{
    public class TrackLibraryInvalidLinkException : Exception
    {
        public TrackLibraryInvalidLinkException(string link) : base($"<{link}> is not a valid track link")
        {

        }
    }

    public class TrackLibrary : ITrackLibrary
    {
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
            MetadataFiles = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(RootPath, "*" + Track.MetadataFileExt))
            {
                MetadataFiles.Add(filename);
            }

            JsonSerializer serializer = new JsonSerializer();
            foreach (string metadataFile in MetadataFiles)
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

            if (TrackList.ContainsKey(title))
            {
                throw new TrackAlreadyExistsException(title);
            }

            Track track = new Track
            {
                Id = id,
                Title = title,
                VideoFilename = await DownloadVideoFile(video, id),
                MetadataFilename = BuildMetadataFilemname(id),
                ThumbnailFilename = await DownloadThumbnailFile(id)
            };

            await UpdateTrackMetadataAsync(track);
            TrackList.Add(track.Title, track);
            return track;
        }

        // ##### UpdateTrackAsync
        // Update track metadata
        public Task UpdateTrackAsync(Track track) { return UpdateTrackMetadataAsync(track); }

        // ##### DeleteTrackAsync
        // Delete a track and its files
        public Task DeleteTrackAsync(Track track)
        {
            File.Delete(GetFilePath(track.MetadataFilename));
            File.Delete(GetFilePath(track.VideoFilename));
            File.Delete(GetFilePath(track.ThumbnailFilename));
            TrackList.Remove(track.Title);
            return Task.CompletedTask;
        }

        // ##### CreateTagList
        // Create a tag list from a row text
        // Return the list of tag ids
        public HashSet<string> CreateTagList(string text)
        {
            HashSet<string> values = Tag.ExtractTagValues(text);
            UpdateTagList(values);
            return values;
        }


        // ##### GetFilePath
        // Get absolute path for a given library file
        public string GetFilePath(string filename)
        {
            return Path.Combine(new string[] { Directory.GetCurrentDirectory(), RootPath, filename });
        }

        // ##### UpdateTrackMetadataAsync
        // Update a single track metadata
        private async Task UpdateTrackMetadataAsync(Track track)
        {
            using (FileStream stream = File.Open(GetFilePath(track.MetadataFilename), FileMode.Create))
            {
                string json = JsonConvert.SerializeObject(track, Formatting.Indented);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        // ##### ProcessTrackId
        // Extract video id from a YouTube link
        private string ProcessTrackId(string link)
        {
            Regex regex = new Regex(@"https:\/\/www\.youtube\.com\/watch\?v=(?<id>[a-zA-Z0-9_\-]+)");
            Match match = regex.Match(link);
            if (!match.Success)
            {
                throw new TrackLibraryInvalidLinkException(link);
            }
            return match.Groups["id"].ToString();
        }

        // ##### ProcessTrackTitle
        // Process track title to remove unwanted stuff
        private string ProcessTrackTitle(Video video) { return video.Title.Replace("- YouTube", "").TrimEnd(' '); }

        // ##### DownloadVideoFile
        // Download the music video from YouTube
        // Returns the video filename
        private async Task<string> DownloadVideoFile(Video video, string id)
        {
            string filename = id + video.FileExtension;
            string filepath = GetFilePath(filename);

            using (FileStream stream = File.Create(filepath))
            {
                byte[] bytes = await video.GetBytesAsync();
                await stream.WriteAsync(bytes, 0, bytes.Count());
            }

            return filename;
        }

        // ##### BuildMetadataFilemname
        // Create the filename for the metadata file
        private string BuildMetadataFilemname(string id)
        {
            return id + Track.MetadataFileExt;
        }

        // ##### DownloadThumbnailFile
        // Download the thumbnail associated to a youtube video id
        // Returns the thumbnail filename
        private async Task<string> DownloadThumbnailFile(string id)
        {
            string filename = id + Track.ThumbnailFileExt;
            string filepath = GetFilePath(filename);
            
            using (WebClient client = new WebClient())
            using (FileStream stream = File.Create(filepath))
            {
                byte[] bytes = await client.DownloadDataTaskAsync($"http://img.youtube.com/vi/{id}/hqdefault.jpg");
                await stream.WriteAsync(bytes, 0, bytes.Count());
            }

            return filename;
        }

        // ##### UpdateTagList
        // Update the list of loaded tags
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
        public List<string> MetadataFiles = new List<string>();
        public Dictionary<string, Track> TrackList { get; private set; } = new Dictionary<string, Track>();
        public Dictionary<string, Tag> TagList { get; protected set; } = new Dictionary<string, Tag>();
    }
}
