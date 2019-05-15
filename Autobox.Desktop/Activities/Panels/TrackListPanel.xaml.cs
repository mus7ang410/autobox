using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autobox.Data;

namespace Autobox.Desktop.Activities.Panels
{
    public class TrackSelectionChangedEventArgs : EventArgs
    {
        public TrackSelectionChangedEventArgs(List<TrackMetadata> selectedTracks)
        {
            SelectedTracks = selectedTracks;
        }

        public readonly List<TrackMetadata> SelectedTracks;
    }

    /// <summary>
    /// Interaction logic for TrackListPanel.xaml
    /// </summary>
    public partial class TrackListPanel : UserControl, INotifyPropertyChanged
    {
        public TrackListPanel()
        {
            InitializeComponent();
            DataContext = this;
            FilteredCollection = new CollectionViewSource();
            FilteredCollection.Filter += TrackCollection_Filter;
            FilteredCollection.SortDescriptions.Add(new SortDescription("Title", CurrentSortDirection));
        }

        private void TrackListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<TrackMetadata> selectedTracks = new List<TrackMetadata>();
            foreach (object obj in TrackListView.SelectedItems)
            {
                selectedTracks.Add(obj as TrackMetadata);
            }
            TrackSelectionChanged?.Invoke(this, new TrackSelectionChangedEventArgs(selectedTracks));
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentSortDirection == ListSortDirection.Ascending)
            {
                SortButton.OpacityMask = FindResource("IconButton.Brushes.Down") as Brush;
                CurrentSortDirection = ListSortDirection.Descending;
            }
            else
            {
                SortButton.OpacityMask = FindResource("IconButton.Brushes.Up") as Brush;
                CurrentSortDirection = ListSortDirection.Ascending;
            }

            FilteredCollection.SortDescriptions.Clear();
            FilteredCollection.SortDescriptions.Add(new SortDescription("Title", CurrentSortDirection));
        }

        private void TrackCollection_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item as TrackMetadata).MatchFilter(TitleFilter);
        }

        // ##### Events
        // SelectedTrackChanged
        public static readonly DependencyProperty TrackSelectionChangedProperty = DependencyProperty.Register(
            "TrackSelectionChanged",
            typeof(EventHandler<TrackSelectionChangedEventArgs>),
            typeof(AddYouTubePanel),
            new PropertyMetadata());

        public EventHandler<TrackSelectionChangedEventArgs> TrackSelectionChanged
        {
            get { return (EventHandler<TrackSelectionChangedEventArgs>)GetValue(TrackSelectionChangedProperty); }
            set { SetValue(TrackSelectionChangedProperty, value); }
        }

        // ##### Properties
        // TrackSource
        public static readonly DependencyProperty TrackSourceProperty = DependencyProperty.Register(
            "TrackSource",
            typeof(TrackCollection),
            typeof(TrackListPanel),
            new PropertyMetadata());

        public TrackCollection TrackSource
        {
            get { return (TrackCollection)GetValue(TrackSourceProperty); }
            set
            {
                SetValue(TrackSourceProperty, value);
                FilteredCollection.Source = value;
            }
        }

        // SelectedTrack
        public static readonly DependencyProperty SelectedTrackProperty = DependencyProperty.Register(
            "SelectedTrack",
            typeof(TrackMetadata),
            typeof(TrackListPanel),
            new PropertyMetadata());

        public TrackMetadata SelectedTrack
        {
            get { return GetValue(SelectedTrackProperty) as TrackMetadata; }
            set
            {
                SetValue(SelectedTrackProperty, value);
                TrackListView.SelectedItem = value;
            }
        }

        public ICollectionView FilteredView { get { return FilteredCollection.View; } }
        public string _TitleFilter;
        public string TitleFilter
        {
            get { return _TitleFilter; }
            set
            {
                _TitleFilter = value;
                FilteredView.Refresh();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TitleFilter"));
            }
        }

        // ##### Attributes
        public event PropertyChangedEventHandler PropertyChanged;
        private CollectionViewSource FilteredCollection;
        private ListSortDirection CurrentSortDirection = ListSortDirection.Ascending;
    }
}
