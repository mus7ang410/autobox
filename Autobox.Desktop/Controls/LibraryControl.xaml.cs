using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
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
using Autobox.Core.Services;

namespace Autobox.Desktop.Controls
{
    /// <summary>
    /// Interaction logic for LibraryControl.xaml
    /// </summary>
    public partial class LibraryControl : UserControl
    {
        public LibraryControl(ITrackLibrary library)
        {
            InitializeComponent();
            DataContext = this;
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;

            Library = library;
            FilteredTracks = new ObservableCollection<Track>(Library.TrackList.Values.ToList());
            if (FilteredTracks.Count > 0)
            {
                TrackListView.SelectedIndex = 0;
            }
        }

        private void TagsWrapPanel_Populate()
        {
            TagsWrapPanel.Children.Clear();

            foreach (string tag in SelectedTrack.Tags)
            {
                Button button = new Button
                {
                    Content = "  " + tag + "  ",
                    Style = FindResource("FlatLabel") as Style
                };
                button.Click += RemoveTagButton_Click;
                TagsWrapPanel.Children.Add(button);
            }
        }

        private void TrackListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTrack = TrackListView.SelectedItem as Track;

            MediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + "/" + SelectedTrack.VideoFilePath);
            MediaPlayer.Volume = SoundSlider.Value;
            MediaPlayer.Play();
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            SelectedTrackTitleLabel.Content = SelectedTrack.Title;
            SelectedTrackDurationLabel.Content = MediaPlayer.NaturalDuration.TimeSpan.ToString();
            TagsWrapPanel_Populate();
        }

        private void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MediaPlayer != null)
            {
                MediaPlayer.Volume = SoundSlider.Value;
            }
        }

        private async void AddTagsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTrack != null)
            {
                HashSet<string> tags = Library.CreateTagList(TagsTextBox.Text);
                SelectedTrack.Tags.UnionWith(tags);
                await Library.UpdateTrackAsync(SelectedTrack);
                TagsWrapPanel_Populate();
                TagsTextBox.Text = string.Empty;
            }
        }

        private async void RemoveTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTrack != null)
            {
                Button button = sender as Button;
                string tag = button.Content.ToString().TrimStart(' ').TrimEnd(' ');
                SelectedTrack.Tags.Remove(tag);
                await Library.UpdateTrackAsync(SelectedTrack);
                TagsWrapPanel.Children.Remove(button);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor previousCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Track track = await Library.CreateTrackAsync(LinkTextBox.Text);
                FilteredTracks.Add(track);
                TrackListView.SelectedItem = track;
                TrackListView.ScrollIntoView(track);

                LinkTextBox.Text = string.Empty;
                Mouse.OverrideCursor = previousCursor;
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = previousCursor;
                MessageBox.Show(
                    ex.Message,
                    "Could not add a song",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ##### Attributes
        private readonly ITrackLibrary Library;
        public ObservableCollection<Track> FilteredTracks { get; set; } = new ObservableCollection<Track>();
        private Track SelectedTrack = null;
    }
}
