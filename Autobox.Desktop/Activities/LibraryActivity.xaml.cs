using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autobox.Core.Data;
using Autobox.Core.Services;
using Autobox.Desktop.Services;

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
            Library = ServiceProvider.GetService<ITrackLibrary>();
            FilteredTrackListPanel.TrackSource = FilteredTrackList;
        }

        public void OnActivated()
        {
            UpdateFilteredList();
        }

        public void OnDeactivated()
        {

        }

        private void FilteredTrackListPanel_SelectedTrackChanged(object sender, Track track)
        {
            SelectedTrack = track;
            PreviewPanel.LoadTrack(track);
            SelectedTrackTagPanel.TagSource = SelectedTrack?.Tags;
        }

        private void ExcludedTagPanel_TagListChanged(object sender, HashSet<string> tagList)
        {
            ExcludedTagList = tagList;
            UpdateFilteredList();
        }

        private void IncludedTagPanel_TagListChanged(object sender, HashSet<string> tagList)
        {
            IncludedTagList = tagList;
            UpdateFilteredList();
        }

        private void PreviewPanel_TrackDeleted(object sender, TrackEventArgs e)
        {
            FilteredTrackList.Remove(e.Track);
        }

        private async void SelectedTrackPanel_TagListChanged(object sender, HashSet<string> tagList)
        {
            if (SelectedTrack != null)
            {
                await ServiceProvider.GetService<ITrackLibrary>()?.UpdateTrackAsync(SelectedTrack);
            }
        }

        private void AddYouTubePanel_CreateTrack(object sender, Track track)
        {
            FilteredTrackList.Add(track);
            FilteredTrackListPanel.SelectedTrack = track;
        }

        private void UpdateFilteredList()
        {
            List<Track> filtered = Library.TrackList.Values.Where(track => track.MatchFilter(ExcludedTagList, IncludedTagList, Track.EIncludeMatchType.Any)).ToList();
            FilteredTrackList.SetTrackRange(filtered);
        }

        // ##### Attributes
        private readonly ITrackLibrary Library;
        private readonly TrackCollection FilteredTrackList = new TrackCollection();
        private Track SelectedTrack = null;
        private HashSet<string> ExcludedTagList = new HashSet<string>();
        private HashSet<string> IncludedTagList = new HashSet<string>();
    }
}
