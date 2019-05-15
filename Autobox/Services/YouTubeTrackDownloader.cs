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

using Autobox.Data;

namespace Autobox.Services
{
    public class YouTubeTrackDownloader : ITrackDownloader
    {
        public YouTubeTrackDownloader()
        {
            Client = YouTube.Default;
        }

        // ##### DownloadTrackAsync
        // Create a track from a YouTube link and add it to the linked library
        // Throw an exception if track already exists in library
        public async Task<TrackMetadata> DownloadTrackAsync(string link)
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

            await Task.WhenAll(
                DownloadVideoFile(video, id),
                DownloadThumbnailFile(id)
                );

            return track;
        }

        // ##### DownloadLibraryAsync
        // Download a whole library
        public async Task DownloadLibraryAsync(LibraryMetadata library)
        {
            List<Task> tasks = new List<Task>();
            foreach (TrackMetadata track in library.Tracks)
            {
                Video video = await Client.GetVideoAsync($"https://www.youtube.com/watch?v={track.Id}");
                string title = ProcessTrackTitle(video);

                tasks.Add(DownloadVideoFile(video, track.Id));
                tasks.Add(DownloadThumbnailFile(track.Id));
            }
            await Task.WhenAll(tasks);
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
            string filepath = ServiceProvider.BuildVideoFilePath(id, video.FileExtension);

            if (!File.Exists(filepath))
            {
                using (FileStream stream = File.Create(filepath))
                {
                    byte[] bytes = await video.GetBytesAsync();
                    await stream.WriteAsync(bytes, 0, bytes.Count());
                }
            }
        }

        // ##### DownloadThumbnailFile
        // Download the thumbnail associated to a youtube video id
        // Returns the thumbnail filename
        private async Task DownloadThumbnailFile(string id)
        {
            string filepath = ServiceProvider.BuildThumbnailFilePath(id);

            if (!File.Exists(filepath))
            {
                using (WebClient client = new WebClient())
                using (FileStream stream = File.Create(filepath))
                {
                    byte[] bytes = await client.DownloadDataTaskAsync($"http://img.youtube.com/vi/{id}/hqdefault.jpg");
                    await stream.WriteAsync(bytes, 0, bytes.Count());
                }
            }
        }

        // ##### Attributes
        private readonly YouTube Client;
    }
}
