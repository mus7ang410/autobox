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
            IEnumerable<string> filepaths = Directory.EnumerateFiles(RootPath, "*" + MetadataFileExt);

            JsonSerializer serializer = new JsonSerializer();
            foreach (string filepath in filepaths)
            {
                string json = File.ReadAllText(filepath);
                TrackMetadata track = ProcessLoadedTrack(JsonConvert.DeserializeObject<TrackMetadata>(json));
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
        private TrackMetadata ProcessLoadedTrack(TrackMetadata track)
        {
            track.Tags = new TagCollection(track.Tags.ToList().ConvertAll(str => str.ToUpper()));
            return track;
        }

        // ##### AddTrackAsync
        // Add a new track to the collection
        // Throw an exception if track already exists in library
        public async Task AddTrackAsync(TrackMetadata track)
        {
            if (TrackList.ContainsKey(track.Title))
            {
                throw new TrackAlreadyExistsException(track.Title);
            }

            await UpdateTrackMetadataAsync(track);
            TrackList.Add(track.Title, track);
            TagList.UnionWith(track.Tags);
        }

        // ##### UpdateTrackAsync
        // Update track metadata
        public Task UpdateTrackAsync(TrackMetadata track) { return UpdateTrackMetadataAsync(track); }

        // ##### DeleteTrackAsync
        // Delete a track and its files
        public Task DeleteTrackAsync(TrackMetadata track)
        {
            File.Delete(GetMetadataFilepath(track));
            File.Delete(GetVideoFilepath(track));
            File.Delete(GetThumbnailFilepath(track));
            TrackList.Remove(track.Title);
            return Task.CompletedTask;
        }

        private string GetMetadataFilepath(TrackMetadata track)
        {
            return Path.Combine(new string[] { Directory.GetCurrentDirectory(), RootPath, track.Id + MetadataFileExt });
        }

        public string BuildThumbnailFilePath(string trackId)
        {
            return Path.Combine(new string[] { Directory.GetCurrentDirectory(), RootPath, trackId + ThumbnailFileExt });
        }
        public string GetThumbnailFilepath(TrackMetadata track)
        {
            return BuildThumbnailFilePath(track.Id);
        }
        public string BuildVideoFilePath(string trackId, string ext)
        {
            return Path.Combine(new string[] { Directory.GetCurrentDirectory(), RootPath, trackId + ext });
        }
        public string GetVideoFilepath(TrackMetadata track)
        {
            return BuildVideoFilePath(track.Id, track.Ext);
        }

        // ##### UpdateTrackMetadataAsync
        // Update a single track metadata
        private async Task UpdateTrackMetadataAsync(TrackMetadata track)
        {
            using (FileStream stream = File.Open(GetMetadataFilepath(track), FileMode.Create))
            {
                string json = JsonConvert.SerializeObject(track, Formatting.Indented);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                TagList.UnionWith(track.Tags);
            }
        }

        // ##### Configuration
        public static readonly string MetadataFileExt = ".metadata.json";
        public static readonly string ThumbnailFileExt = ".thumbnail.jpg";
        // ##### Attributes
        // library configuration
        private readonly string RootPath;
        // library files
        public Dictionary<string, TrackMetadata> TrackList { get; private set; } = new Dictionary<string, TrackMetadata>();
        public TagCollection TagList { get; protected set; } = new TagCollection();
    }
}
