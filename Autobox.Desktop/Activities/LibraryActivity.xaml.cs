using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Win32;

using Autobox.Data;
using Autobox.Services;
using Autobox.Desktop.Activities.Panels;

namespace Autobox.Desktop.Activities
{
    /// <summary>
    /// Interaction logic for LibraryActivity.xaml
    /// </summary>
    public partial class LibraryActivity : UserControl, IActivity
    {
        public LibraryActivity()
        {
            InitializeComponent();
            IncludedTagList.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) { UpdateFilteredList(); };
            IncludedTagPanel.TagSource = IncludedTagList;
            ExcludedTagList.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) { UpdateFilteredList(); };
            ExcludedTagPanel.TagSource = ExcludedTagList;
            FilteredTrackListPanel.TrackSource = FilteredTrackList;
        }

        public void OnActivated()
        {
            UpdateFilteredList();
        }

        public void OnDeactivated()
        {

        }


        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = $"Libary file (*{ServiceProvider.LibraryMetadataFileExt})| *{ServiceProvider.LibraryMetadataFileExt}",
                DefaultExt = ServiceProvider.LibraryMetadataFileExt,
                InitialDirectory = ServiceProvider.LibraryDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                ServiceProvider.LoadLibrary(Path.GetDirectoryName(dialog.FileName));
            }
        }



        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = $"Autobox Libary File (*{ServiceProvider.LibraryMetadataFileExt})| *{ServiceProvider.LibraryMetadataFileExt}",
                InitialDirectory = ServiceProvider.LibraryDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                ServiceProvider.ExportLibrary(dialog.FileName);
            }
        }

        private void FilteredTrackListPanel_TrackSelectionChanged(object sender, TrackSelectionChangedEventArgs e)
        {
            SelectedTracks = e.SelectedTracks;

            if (SelectedTracks.Count == 0)
            {
                PreviewPanel.UnloadTrack();
                SelectedTrackTagPanel.IsMultipleSelection = false;
                SelectedTrackTagPanel.TagSource = null;
            }
            else if (SelectedTracks.Count == 1)
            {
                PreviewPanel.LoadTrack(e.SelectedTracks.First());
                SelectedTrackTagPanel.IsMultipleSelection = false;
                SelectedTrackTagPanel.TagSource = SelectedTracks.First()?.Tags;

                Random r = new Random();
                ActivityBackgroundChanged?.Invoke(this, new ActivityBackgroundChangedEventArgs(
                    Color.FromArgb(255, (byte)r.Next(1, 255), (byte)r.Next(1, 255), (byte)r.Next(1, 255))
                    ));
            }
            else
            {
                MultipleTagList = new TagCollection(SelectedTracks.First().Tags);
                foreach (TrackMetadata track in e.SelectedTracks.GetRange(1, e.SelectedTracks.Count - 1))
                {
                    MultipleTagList.IntersectWith(track.Tags);
                }
                SelectedTrackTagPanel.IsMultipleSelection = true;
                SelectedTrackTagPanel.TagSource = MultipleTagList;
            }
        }

        private void PreviewPanel_TrackDeleted(object sender, TrackMetadataEventArgs e)
        {
            FilteredTrackList.Remove(e.Track);
        }

        private async void SelectedTrackPanel_TagAdded(object sender, TagAddedEventArgs e)
        {
            if (SelectedTracks.Count == 1)
            {
                await ServiceProvider.Library.UpdateTrackAsync(SelectedTracks.First());
            }
            else
            {
                List<Task> tasks = new List<Task>();
                foreach (TrackMetadata track in SelectedTracks)
                {
                    track.Tags.UnionWith(e.TagList);
                    tasks.Add(ServiceProvider.Library.UpdateTrackAsync(track));
                }
                await Task.WhenAll(tasks);
            }

            UpdateFilteredList();
        }

        private async void SelectedTrackPanel_TagRemoved(object sender, TagRemovedEventArgs e)
        {
            if (SelectedTracks.Count == 1)
            {
                await ServiceProvider.Library.UpdateTrackAsync(SelectedTracks.First());
            }
            else
            {
                List<Task> tasks = new List<Task>();
                foreach (TrackMetadata track in SelectedTracks)
                {
                    track.Tags.Remove(e.Tag);
                    tasks.Add(ServiceProvider.Library.UpdateTrackAsync(track));
                }
                await Task.WhenAll(tasks);
            }

            UpdateFilteredList();
        }

        private void AddYouTubePanel_CreateTrack(object sender, TrackMetadataEventArgs e)
        {
            FilteredTrackList.Add(e.Track);
            FilteredTrackListPanel.SelectedTrack = e.Track;
        }

        private void UpdateFilteredList()
        {
            List<TrackMetadata> filtered = ServiceProvider.Library.TrackList.Values.Where(track => track.MatchFilter(ExcludedTagList, IncludedTagList, TrackMetadata.EIncludeMatchType.Any)).ToList();
            FilteredTrackList.SetTrackRange(filtered);
        }

        // ##### Events
        public EventHandler<ActivityBackgroundChangedEventArgs> ActivityBackgroundChanged { get; set; }
        // ##### Attributes
        private readonly TrackCollection FilteredTrackList = new TrackCollection();
        private List<TrackMetadata> SelectedTracks = null;
        private TagCollection ExcludedTagList = new TagCollection();
        private TagCollection IncludedTagList = new TagCollection();
        private TagCollection MultipleTagList = new TagCollection();
    }
}
