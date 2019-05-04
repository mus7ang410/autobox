using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autobox.Core.Data;
using Autobox.Core.Services;
using Autobox.Desktop.Services;
using Autobox.Desktop.Activities.Controls;

namespace Autobox.Desktop.Activities.Panels
{
    /// <summary>
    /// Interaction logic for TrackPreviewPanel.xaml
    /// </summary>
    public partial class TrackPreviewPanel : UserControl
    {
        public TrackPreviewPanel()
        {
            InitializeComponent();
            DataContext = this;
            Library = ServiceProvider.GetService<ITrackLibrary>();
            LoadTrack(null);
        }

        public void LoadTrack(Track track)
        {
            SelectedTrack = track;
            if (SelectedTrack != null)
            {
                TitleTextBox.Text = SelectedTrack.Title;
                TitleTextBox.IsEnabled = true;
                RatingPanel.CanRate = true;
                RatingPanel.Rating = SelectedTrack.Rating;

                TrackPlayer.CurrentTrack = SelectedTrack;
                if (State != EState.Idle)
                {
                    TrackPlayer.Pause();
                    State = EState.Idle;
                    PlayButton.OpacityMask = FindResource("IconButton.Brushes.Play") as Brush;
                }
            }
            else
            {
                TrackPlayer.CurrentTrack = null;
                TitleTextBox.Text = string.Empty;
                TitleTextBox.IsEnabled = false;
                RatingPanel.CanRate = false;
            }
        }

        private Track UnloadTrack()
        {
            TitleTextBox.Text = string.Empty;
            TitleTextBox.IsEnabled = false;
            RatingPanel.CanRate = false;
            Track unloadedTrack = SelectedTrack;
            SelectedTrack = null;
            return unloadedTrack;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTrack != null)
            {
                if (State == EState.Idle)
                {
                    TrackPlayer.Play(0);
                    State = EState.Playing;
                    PlayButton.OpacityMask = FindResource("IconButton.Brushes.Pause") as Brush;
                }
                else
                {
                    TrackPlayer.Pause();
                    State = EState.Idle;
                    PlayButton.OpacityMask = FindResource("IconButton.Brushes.Play") as Brush;
                }
            }
        }

        private void MediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedTrack != null)
            {
                TrackPlayer.Volume = SoundSlider.Value;
            }
        }

        private void MediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            State = EState.Failed;
            PlayButton.OpacityMask = FindResource("IconButton.Brushes.Play") as Brush;
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            State = EState.Idle;
            PlayButton.OpacityMask = FindResource("IconButton.Brushes.Pause") as Brush;
        }

        private void SoundSlider_ValueChanged(object sender, double e)
        {
            TrackPlayer.Volume = SoundSlider.Value;
        }

        private async void RatingPanel_RatingChanged(object sender, RatingChangedEventArgs e)
        {
            if (SelectedTrack != null)
            {
                SelectedTrack.Rating = e.Rating;
                await Library.UpdateTrackAsync(SelectedTrack);
            }
        }

        private async void TitleTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    SelectedTrack.Title = TitleTextBox.Text;
                    await Library.UpdateTrackAsync(SelectedTrack);
                    TrackUpdated?.Invoke(this, new TrackEventArgs(SelectedTrack));
                }
                catch (Exception exception)
                {
                    TitleTextBox.Text = SelectedTrack.Title;
                    MessageBox.Show(
                        exception.Message,
                        "Cannot set track title",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTrack != null)
            {
                MessageBoxResult result = MessageBox.Show(
                $"Delete track {SelectedTrack.Title}?",
                "Delete a track",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    Track deletedTrack = UnloadTrack();
                    await Library.DeleteTrackAsync(deletedTrack);
                    TrackDeleted?.Invoke(this, new TrackEventArgs(deletedTrack));
                }
            }
        }

        // ##### Events
        public EventHandler<TrackEventArgs> TrackDeleted { get; set; }
        public EventHandler<TrackEventArgs> TrackUpdated { get; set; }
        // ##### Attributes
        private readonly ITrackLibrary Library;
        private Track SelectedTrack = null;
        private enum EState { Idle, Playing, Failed }
        private EState State = EState.Idle;
    }
}
