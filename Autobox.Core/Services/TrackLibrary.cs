using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

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
                    TagList.UnionWith(track.Tags);
                }
            }

            return Task.CompletedTask;
        }

        // ##### ProcessLoadedTrack
        // Process a newly loaded track to ensure is content is compatible
        private Track ProcessLoadedTrack(Track track)
        {
            track.Tags = new TagCollection(track.Tags.ToList().ConvertAll(str => str.ToUpper()));
            return track;
        }

        // ##### AddTrackAsync
        // Add a new track to the collection
        // Throw an exception if track already exists in library
        public async Task AddTrackAsync(Track track)
        {
            if (TrackList.ContainsKey(track.Title))
            {
                throw new TrackAlreadyExistsException(track.Title);
            }

            await UpdateTrackMetadataAsync(track);
            TrackList.Add(track.Title, track);
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
        public TagCollection CreateTagList(string text)
        {
            TagCollection tags = new TagCollection();
            string[] tokens = text.Split(' ');
            foreach (string token in tokens)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                string computed = rgx.Replace(token, "").ToUpper();
                if (computed.Length >= 3)
                {
                    tags.Add(computed);
                }
            }
            TagList.UnionWith(tags);
            return tags;
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

        // ##### Attributes
        // library configuration
        private readonly string RootPath;
        // library files
        public List<string> MetadataFiles = new List<string>();
        public Dictionary<string, Track> TrackList { get; private set; } = new Dictionary<string, Track>();
        public TagCollection TagList { get; protected set; } = new TagCollection();
    }
}
