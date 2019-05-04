using System;
using System.Collections.Generic;
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
using Autobox.Core.Services.VideoLibrary;
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
            DataContext = this;
            FilteredTrackListPanel.TrackSource = ServiceProvider.GetService<ITrackFilter>()?.FilteredTrackList;
        }

        public void OnActivated()
        {

        }

        public void OnDeactivated()
        {

        }

        private void FilteredTrackListPanel_SelectedTrackChanged(object sender, Track track)
        {
            SelectedTrack = track;
            TrackPreviewPanel.LoadTrack(track);
            SelectedTrackTagPanel.TagSource = SelectedTrack?.Tags;
        }

        private void ExcludedTagPanel_TagListChanged(object sender, HashSet<string> tagList)
        {
            ExcludedTagList = tagList;
            ServiceProvider.GetService<ITrackFilter>()?.UpdateFilter(ExcludedTagList, IncludedTagList);
        }

        private void IncludedTagPanel_TagListChanged(object sender, HashSet<string> tagList)
        {
            IncludedTagList = tagList;
            ServiceProvider.GetService<ITrackFilter>()?.UpdateFilter(ExcludedTagList, IncludedTagList);
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
            ServiceProvider.GetService<ITrackFilter>()?.AddFilteredTrack(track);
            FilteredTrackListPanel.SelectedTrack = track;
        }

        // ##### Attributes
        private Track SelectedTrack = null;
        private HashSet<string> ExcludedTagList = new HashSet<string>();
        private HashSet<string> IncludedTagList = new HashSet<string>();
    }
}
