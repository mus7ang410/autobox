using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
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
using Autobox.Desktop.Services;

namespace Autobox.Desktop.Activities.Panels
{
    /// <summary>
    /// Interaction logic for TrackPlayerPanel.xaml
    /// </summary>
    public partial class TrackPlayerPanel : UserControl
    {
        public TrackPlayerPanel()
        {
            InitializeComponent();
            Library = ServiceProvider.GetService<ITrackLibrary>();
            Playlist = ServiceProvider.GetService<IPlaylistManager>();
            ScreenPanel.TrackTransitionStarted += ScreenPanel_TrackTransitionStarted;
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            Playlist.Shuffle();
            ScreenPanel.Init(Playlist.PreviousTrack, Playlist.CurrentTrack, Playlist.NextTrack);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (State == EState.Paused)
            {
                Play();
            }
            else if (State == EState.Playing)
            {
                Pause();
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            Skip();
        }

        private void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (ScreenPanel != null)
            {
                ScreenPanel.Volume = slider.Value;
            }
            
        }

        private void Play()
        {
            ScreenPanel.PlayCurrent();
            PlayButton.OpacityMask = FindResource("Player.Brushes.Pause") as Brush;
        }

        private void Pause()
        {
            ScreenPanel.PauseCurrent();
            PlayButton.OpacityMask = FindResource("Player.Brushes.Play") as Brush;
        }

        private void Skip()
        {
            ScreenPanel.PlayNext(Playlist.NextTrack);
            Playlist.Forward();
        }

        private void ScreenPanel_TrackTransitionStarted(object sender, EventArgs e)
        {
            ScreenPanel.PlayNext(Playlist.NextTrack);
            Playlist.Forward();
        }

        // ##### Attributes
        private readonly ITrackLibrary Library;
        private readonly IPlaylistManager Playlist;
        private enum EState { Paused, Playing };
        private EState State = EState.Paused;
    }
}
