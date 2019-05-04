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

            Library = ServiceProvider.GetService<ITrackLibrary>();

            LoadTrack(null);
            MediaPlayer.Loaded += MediaPlayer_Loaded;
            MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        public void LoadTrack(Track track)
        {
            SelectedTrack = track;
            if (SelectedTrack != null)
            {
                TitleLabel.Content = SelectedTrack.Title.ToUpper();
                RatingPanel.CanRate = true;
                RatingPanel.Rating = SelectedTrack.Rating;
                ThumbnailPlaceholder.Source = new BitmapImage(new Uri(Library.GetFilePath(SelectedTrack.ThumbnailFilename)));
                ThumbnailPlaceholder.Stretch = Stretch.Uniform;
                MediaPlayer.Source = new Uri(Library.GetFilePath(SelectedTrack.VideoFilename));
                MediaPlayer.Visibility = Visibility.Hidden;
                if (State != EState.Idle)
                {
                    MediaPlayer.Pause();
                    State = EState.Idle;
                    PlayButton.OpacityMask = FindResource("IconButton.Brushes.Play") as Brush;
                }
            }
            else
            {
                TitleLabel.Content = "NO SELECTED TRACK";
                RatingPanel.CanRate = false;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTrack != null)
            {
                if (State == EState.Idle)
                {
                    MediaPlayer.Visibility = Visibility.Visible;
                    MediaPlayer.Play();
                    State = EState.Playing;
                    PlayButton.OpacityMask = FindResource("IconButton.Brushes.Pause") as Brush;
                }
                else
                {
                    MediaPlayer.Visibility = Visibility.Hidden;
                    MediaPlayer.Pause();
                    State = EState.Idle;
                    PlayButton.OpacityMask = FindResource("IconButton.Brushes.Play") as Brush;
                }
            }
        }

        private void MediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedTrack != null)
            {
                MediaPlayer.Volume = SoundSlider.Value;
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

        private async void RatingPanel_RatingChanged(object sender, int rating)
        {
            if (SelectedTrack != null)
            {
                SelectedTrack.Rating = rating;
                await Library.UpdateTrackAsync(SelectedTrack);
            }
        }

        private void SoundSlider_ValueChanged(object sender, double e)
        {
            SoundSlider slider = sender as SoundSlider;
            MediaPlayer.Volume = slider.Value;
        }

        // ##### Attributes
        private readonly ITrackLibrary Library;
        private Track SelectedTrack = null;
        private enum EState { Idle, Playing, Failed }
        private EState State = EState.Idle;
    }
}
