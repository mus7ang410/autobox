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
            Playlist = ServiceProvider.GetService<IPlaylistManager>();
            Player.Volume = SoundSlider.Value;
        }

        public void Play()
        {
            Player.Play();
            PlayButton.OpacityMask = FindResource("IconButton.Brushes.Pause") as Brush;
            State = EState.Playing;
        }

        public void Pause()
        {
            Player.Pause();
            PlayButton.OpacityMask = FindResource("IconButton.Brushes.Play") as Brush;
            State = EState.Paused;
        }

        private async void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            await Playlist.Shuffle();
            Player.Load(Playlist.TrackList);
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
            Player.Skip();
        }

        private void SoundSlider_ValueChanged(object sender, double e)
        {
            if (Player != null)
            {
                Player.Volume = e;
            }
        }

        // ##### Attributes
        private readonly IPlaylistManager Playlist;
        private enum EState { Playing, Paused };
        private EState State = EState.Paused;
    }
}
