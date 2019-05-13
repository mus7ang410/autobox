﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Autobox.Core.Services;
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
            Creator = ServiceProvider.AddService<ITrackCreator, YouTubeTrackCreator>(new YouTubeTrackCreator(Library));
            Playlist = ServiceProvider.AddService<IPlaylistManager, AutoboxPlaylistManager>(new AutoboxPlaylistManager(Library));
        }

        // ##### Application singletons
        private readonly ITrackLibrary Library;
        private readonly ITrackCreator Creator;
        private readonly IPlaylistManager Playlist;
    }
}
