using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using VideoLibrary;

using Autobox.Core.Data;

namespace Autobox.Core.Services
{
    public class YouTubeTrackCreator : ITrackCreator
    {
        public YouTubeTrackCreator(ITrackLibrary library)
        {
            Client = YouTube.Default;
            Library = library;
        }

        // ##### CreateTrack
        // Create a track from a YouTube link and add it to the linked library
        // Throw an exception if track already exists in library
        public async Task<TrackMetadata> CreateTrackAsync(string link)
        {
            string id = ProcessTrackId(link);
            Video video = await Client.GetVideoAsync(link);
            string title = ProcessTrackTitle(video);

            TrackMetadata track = new TrackMetadata
            {
                Id = id,
                Title = title,
                Ext = video.FileExtension
            };

            await DownloadVideoFile(video, id);
            await DownloadThumbnailFile(id);
            await Library.AddTrackAsync(track);
            return track;
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
        private async Task DownloadVideoFile(Video video, string id)
        {
            string filepath = Library.BuildVideoFilePath(id, video.FileExtension);

            using (FileStream stream = File.Create(filepath))
            {
                byte[] bytes = await video.GetBytesAsync();
                await stream.WriteAsync(bytes, 0, bytes.Count());
            }
        }

        // ##### DownloadThumbnailFile
        // Download the thumbnail associated to a youtube video id
        // Returns the thumbnail filename
        private async Task DownloadThumbnailFile(string id)
        {
            string filepath = Library.BuildThumbnailFilePath(id);

            using (WebClient client = new WebClient())
            using (FileStream stream = File.Create(filepath))
            {
                byte[] bytes = await client.DownloadDataTaskAsync($"http://img.youtube.com/vi/{id}/hqdefault.jpg");
                await stream.WriteAsync(bytes, 0, bytes.Count());
            }
        }

        // ##### Attributes
        private readonly YouTube Client;
        private readonly ITrackLibrary Library;
    }
}
