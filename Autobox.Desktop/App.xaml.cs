using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Autobox.Core.Services;
using Autobox.Core.Services.Playlists;
using Autobox.Core.Services.VideoLibrary;

using Autobox.Core.Data;
using Autobox.Desktop.Services;

namespace Autobox.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Library = ServiceProvider.AddService<ITrackLibrary, TrackLibrary>(new TrackLibrary("Library"));
            Library.LoadAllAsync().Wait();
            Filter = ServiceProvider.AddService<ITrackFilter, TrackFilter>(new TrackFilter(Library));
            Filter.UpdateFilter(null, null);
            Playlist = ServiceProvider.AddService<IPlaylistManager, DefaultPlaylistManager>(new DefaultPlaylistManager(Library));
        }

        // ##### Application singletons
        private readonly ITrackLibrary Library;
        private readonly ITrackFilter Filter;
        private readonly IPlaylistManager Playlist;
    }
}
