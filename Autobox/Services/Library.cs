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
        public void Load(string directory)
        {
            List<string> metadataFiles = Directory.EnumerateFiles(directory, "*" + ServiceProvider.MetadataFileExt).ToList();

            foreach (string metadataFile in metadataFiles)
            {
                string json = File.ReadAllText(metadataFile);
                TrackMetadata track = ProcessLoadedTrack(JsonConvert.DeserializeObject<TrackMetadata>(json));
                if (!TrackList.ContainsKey(track.Id))
                {
                    TrackList.Add(track.Id, track);
                    TagList.UnionWith(track.Tags);
                }
            }
        }

        // ##### ImportAsync
        // Import library from metadata
        public async Task ImportAsync(LibraryMetadata library)
        {
            List<Task> tasks = new List<Task>();
            foreach (TrackMetadata track in library.Tracks)
            {
                if (TrackList.ContainsKey(track.Id))
                {
                    TagList.UnionWith(track.Tags);
                    tasks.Add(UpdateTrackAsync(track));
                }
                else
                {
                    TrackList[track.Id].Tags.UnionWith(track.Tags);
                    TagList.UnionWith(track.Tags);
                    tasks.Add(UpdateTrackAsync(track));
                }
            }
            await Task.WhenAll(tasks);
        }

        // ##### Export
        // Export library metadata
        public LibraryMetadata Export()
        {
            return new LibraryMetadata
            {
                Tracks = TrackList.Values.ToList()
            };
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
            if (TrackList.ContainsKey(track.Id))
            {
                throw new TrackAlreadyExistsException(track.Id);
            }

            await UpdateTrackMetadataAsync(track);
            TrackList.Add(track.Id, track);
            TagList.UnionWith(track.Tags);
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
            TrackList.Remove(track.Id);
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
        // library files
        public Dictionary<string, TrackMetadata> TrackList { get; private set; } = new Dictionary<string, TrackMetadata>();
        public TagCollection TagList { get; protected set; } = new TagCollection();
    }
}
