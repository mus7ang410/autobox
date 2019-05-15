using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

using Autobox.Data;

namespace Autobox.Services
{
    public class TrackLibraryInvalidLinkException : Exception
    {
        public TrackLibraryInvalidLinkException(string link) : base($"<{link}> is not a valid track link")
        {

        }
    }

    public class Library : ILibrary
    {
        // ##### Load
        // Load the whole library
        public void Load(string filepath, LibraryMetadata metadata)
        {
            MetadataFilepath = filepath;
            Metadata = metadata;
            TrackList = Metadata.Tracks.ToDictionary(track => track.Id, track => track);
            foreach (TrackMetadata track in TrackList.Values)
            {
                TagList.UnionWith(track.Tags);
            }
        }

        // ##### Save
        // Save metadata file
        private void Save()
        {
            string json = JsonConvert.SerializeObject(Metadata, Formatting.Indented);
            File.WriteAllText(MetadataFilepath, json);
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
            Metadata.Tracks.Add(track);
            Save();
        }

        // ##### UpdateTrackAsync
        // Update track metadata
        public Task UpdateTrackAsync(TrackMetadata track) { return UpdateTrackMetadataAsync(track); }

        // ##### DeleteTrackAsync
        // Delete a track and its files
        public Task DeleteTrackAsync(TrackMetadata track)
        {
            File.Delete(ServiceProvider.GetMetadataFilepath(track));
            File.Delete(ServiceProvider.GetVideoFilepath(track));
            File.Delete(ServiceProvider.GetThumbnailFilepath(track));
            TrackList.Remove(track.Title);
            Metadata.Tracks.Remove(track);
            Save();
            return Task.CompletedTask;
        }

        // ##### UpdateTrackMetadataAsync
        // Update a single track metadata
        private async Task UpdateTrackMetadataAsync(TrackMetadata track)
        {
            using (FileStream stream = File.Open(ServiceProvider.GetMetadataFilepath(track), FileMode.Create))
            {
                string json = JsonConvert.SerializeObject(track, Formatting.Indented);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                TagList.UnionWith(track.Tags);
            }
        }

        // ##### Attributes
        // library configuration
        private string MetadataFilepath;
        public LibraryMetadata Metadata { get; private set; } = new LibraryMetadata();
        // library files
        public Dictionary<string, TrackMetadata> TrackList { get; private set; } = new Dictionary<string, TrackMetadata>();
        public TagCollection TagList { get; protected set; } = new TagCollection();
    }
}
