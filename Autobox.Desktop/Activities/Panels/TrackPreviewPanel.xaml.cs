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
                MediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + "/" +SelectedTrack.VideoFilePath);
                if (State != EState.Idle)
                {
                    MediaPlayer.Pause();
                    State = EState.Idle;
                    PlayButton.OpacityMask = FindResource("TrackPreview.PlayButton.ImageBrush") as Brush;
                }
            }
            else
            {
                TitleLabel.Content = "NO SELECTED TRACK";
            }
        }

        

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTrack != null)
            {
                if (State == EState.Idle)
                {
                    MediaPlayer.Play();
                    State = EState.Playing;
                    PlayButton.OpacityMask = FindResource("Player.Brushes.Pause") as Brush;
                }
                else
                {
                    MediaPlayer.Pause();
                    State = EState.Idle;
                    PlayButton.OpacityMask = FindResource("Player.Brushes.Play") as Brush;
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
            PlayButton.OpacityMask = FindResource("Player.Brushes.Play") as Brush;
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            State = EState.Idle;
            PlayButton.OpacityMask = FindResource("Player.Brushes.Pause") as Brush;
        }

        private void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            MediaPlayer.Volume = slider.Value;
        }

        // ##### Attributes
        private Track SelectedTrack = null;
        private enum EState { Idle, Playing, Failed }
        private EState State = EState.Idle;
    }
}
