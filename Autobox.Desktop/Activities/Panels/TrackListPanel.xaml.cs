using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autobox.Core.Data;
using Autobox.Desktop.Services;

namespace Autobox.Desktop.Activities.Panels
{
    /// <summary>
    /// Interaction logic for TrackListPanel.xaml
    /// </summary>
    public partial class TrackListPanel : UserControl
    {
        public TrackListPanel()
        {
            InitializeComponent();
        }

        private void TrackListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTrackChanged?.Invoke(this, TrackListView.SelectedItem as Track);
        }

        // ##### Properties
        // TrackSource
        public static readonly DependencyProperty TrackSourceProperty = DependencyProperty.Register(
            "TrackSource",
            typeof(IEnumerable),
            typeof(TrackListPanel),
            new PropertyMetadata());

        public IEnumerable TrackSource
        {
            get { return (IEnumerable)GetValue(TrackSourceProperty); }
            set
            {
                SetValue(TrackSourceProperty, value);
                TrackListView.ItemsSource = value;
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

        // ##### Events
        // SelectedTrackChanged
        public static readonly DependencyProperty SelectedTrackChangedProperty = DependencyProperty.Register("SelectedTrackChanged",
            typeof(EventHandler<Track>),
            typeof(AddYouTubePanel),
            new PropertyMetadata());

        public EventHandler<Track> SelectedTrackChanged
        {
            get { return (EventHandler<Track>)GetValue(SelectedTrackChangedProperty); }
            set { SetValue(SelectedTrackChangedProperty, value); }
        }
    }
}
