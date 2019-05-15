using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

using Autobox.Data;

namespace Autobox.Services
{
    public static class ServiceProvider
    {
        public static void Init(string libraryName)
        {
            LoadLibrary(libraryName);
        }

        public static string GetMetadataFilepath(TrackMetadata track)
        {
            return Path.Combine(new string[] { Directory.GetCurrentDirectory(), LibraryDirectory, track.Id + MetadataFileExt });
        }

        public static string BuildThumbnailFilePath(string trackId)
        {
            return Path.Combine(new string[] { Directory.GetCurrentDirectory(), LibraryDirectory, trackId + ThumbnailFileExt });
        }
        public static string GetThumbnailFilepath(TrackMetadata track)
        {
            return BuildThumbnailFilePath(track.Id);
        }
        public static string BuildVideoFilePath(string trackId, string ext)
        {
            return Path.Combine(new string[] { Directory.GetCurrentDirectory(), LibraryDirectory, trackId + ext });
        }
        public static string GetVideoFilepath(TrackMetadata track)
        {
            return BuildVideoFilePath(track.Id, track.Ext);
        }

        public static void LoadLibrary(string libraryName)
        {
            LibraryDirectory = Path.Combine(Directory.GetCurrentDirectory(), libraryName);
            if (!Directory.Exists(LibraryDirectory))
            {
                Directory.CreateDirectory(LibraryDirectory);
            }

            LibraryMetadata metadata = null;
            string metadataFilePath = Path.Combine(LibraryDirectory, LibraryMetadataFileName);
            if (!File.Exists(metadataFilePath))
            {
                metadata = new LibraryMetadata();
                File.WriteAllText(metadataFilePath, JsonConvert.SerializeObject(metadata));
            }
            else
            {
                string json = File.ReadAllText(metadataFilePath);
                metadata = JsonConvert.DeserializeObject<LibraryMetadata>(json);
            }
            Library.Load(metadataFilePath, metadata);
        }

        // ##### Configuration
        public static readonly string LibraryMetadataFileName = "library.json";
        public static readonly string MetadataFileExt = ".metadata.json";
        public static readonly string ThumbnailFileExt = ".thumbnail.jpg";
        // ##### Services
        private static string LibraryDirectory;
        public static readonly ILibrary Library = new Library();
        public static readonly IPlaylistGenerator Generator = new AutoboxPlaylistGenerator(Library);
    }
}
