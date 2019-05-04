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

using Autobox.Core.Data;
using Autobox.Desktop.Services;

namespace Autobox.Desktop.Activities.Panels
{
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
            SelectedTrackChanged?.Invoke(this, new TrackEventArgs(TrackListView.SelectedItem as Track));
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
            if (string.IsNullOrEmpty(TitleFilter))
            {
                e.Accepted = true;
                return;
            }

            Track track = e.Item as Track;
            if (track.Title.ToUpper().Contains(TitleFilter.ToUpper()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        // ##### Events
        // SelectedTrackChanged
        public static readonly DependencyProperty SelectedTrackChangedProperty = DependencyProperty.Register("SelectedTrackChanged",
            typeof(EventHandler<TrackEventArgs>),
            typeof(AddYouTubePanel),
            new PropertyMetadata());

        public EventHandler<TrackEventArgs> SelectedTrackChanged
        {
            get { return (EventHandler<TrackEventArgs>)GetValue(SelectedTrackChangedProperty); }
            set { SetValue(SelectedTrackChangedProperty, value); }
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
            typeof(Track),
            typeof(TrackListPanel),
            new PropertyMetadata());

        public Track SelectedTrack
        {
            get { return GetValue(SelectedTrackProperty) as Track; }
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
