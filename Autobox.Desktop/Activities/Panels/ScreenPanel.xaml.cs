using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
    /// Interaction logic for ScreenPanel.xaml
    /// </summary>
    public partial class ScreenPanel : UserControl
    {
        public ScreenPanel()
        {
            InitializeComponent();
            Library = ServiceProvider.GetService<ITrackLibrary>();
            ScreenCanvas.SizeChanged += ScreenCanvas_SizeChanged;
            for (int i = 0; i < 3; i++)
            {
                MediaPlayerContainer mediaPlayer = new MediaPlayerContainer
                {
                    Container = new Grid(),
                    Player = new MediaElement()
                };
                mediaPlayer.Player.LoadedBehavior = MediaState.Manual;
                mediaPlayer.Container.Children.Add(mediaPlayer.Player);
                ScreenCanvas.Children.Add(mediaPlayer.Container);
                Canvas.SetTop(mediaPlayer.Container, 0);
                MediaPlayers.Add(mediaPlayer);
            }
            CurrentOffset = 0.5;

            TravelingTimer = new DispatcherTimer();
            TravelingTimer.Tick += TravelingTimer_Tick;
            TravelingTimer.Interval = TimeSpan.FromMilliseconds(16);

            TransitionTimer = new DispatcherTimer();
            TransitionTimer.Tick += TransitionTimer_Tick;

            TrackTimer = new DispatcherTimer();
            TrackTimer.Tick += TrackTimer_Tick;
        }

        private void ScreenCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (MediaPlayerContainer mediaPlayer in MediaPlayers)
            {
                mediaPlayer.Container.Height = ScreenCanvas.ActualHeight;
                mediaPlayer.Container.Width = ScreenCanvas.ActualWidth / 2;
            }
        }

        public void Init(Track previousTrack, Track currentTrack, Track nextTrack)
        {
            PreviousMediaPlayer.Player.Source = new Uri(Library.GetFilePath(previousTrack.VideoFilename));
            CurrentMediaPlayer.Player.Source = new Uri(Library.GetFilePath(currentTrack.VideoFilename));
            NextMediaPlayer.Player.Source = new Uri(Library.GetFilePath(nextTrack.VideoFilename));
        }

        public void PlayCurrent()
        {
            State = EState.Playing;
            CurrentMediaPlayer.Player.Play();
            TravelingTimer.Start();
            ConfigureCurrentTrack();
            if (TransitionTimer.Interval != TimeSpan.Zero)
            {
                TransitionTimer.Start();
            }
        }

        private void ConfigureCurrentTrack()
        {
            if (CurrentMediaPlayer.Player.NaturalDuration != Duration.Automatic)
            {
                ConfigureTrackTimer(true);
            }
            else
            {
                CurrentMediaPlayer.Player.MediaOpened += CurrentMediaPlayer_MediaOpened;
            }
        }

        private void ConfigureTrackTimer(bool start)
        {
            double duration = CurrentMediaPlayer.Player.NaturalDuration.TimeSpan.TotalMilliseconds * (1 - TransitionDuration) - CurrentMediaPlayer.Player.Position.TotalMilliseconds;
            ExpectedTerminatedTime = DateTime.UtcNow + TimeSpan.FromMilliseconds(duration);
            TrackTimer.Interval = TimeSpan.FromMilliseconds(duration);
            if (start)
            {
                TrackTimer.Start();
            }
        }

        private void CurrentMediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            CurrentMediaPlayer.Player.MediaOpened -= CurrentMediaPlayer_MediaOpened;
            ConfigureTrackTimer(true);
            if (TransitionTimer.Interval != TimeSpan.Zero)
            {
                TransitionTimer.Start();
            }
        }

        public void PlayNext(Track nextTrack)
        {
            if (++CurrentMediaPlayerIndex > MediaPlayers.Count - 1)
            {
                CurrentMediaPlayerIndex = 0;
            }
            CurrentOffset += 0.5;
            NextMediaPlayer.Player.Source = new Uri(Library.GetFilePath(nextTrack.VideoFilename));
            CurrentMediaPlayer.Player.Play();
            ConfigureCurrentTrack();

            double duration = PreviousMediaPlayer.Player.NaturalDuration.TimeSpan.TotalMilliseconds * TransitionDuration / TransitionStepCount;
            TransitionStep = TransitionStepCount;
            TransitionTimer.Interval = TimeSpan.FromMilliseconds(duration);
            TransitionTimer.Start();
        }

        public void PauseCurrent()
        {
            State = EState.Paused;
            CurrentMediaPlayer.Player.Pause();
            TravelingTimer.Stop();
            TrackTimer.Stop();
            TransitionTimer.Stop();
        }

        private void TravelingTimer_Tick(object sender, EventArgs e)
        {
            if (State == EState.Playing && CurrentMediaPlayer.Player.NaturalDuration != Duration.Automatic)
            {
                if (ExpectedTerminatedTime >= DateTime.UtcNow)
                {
                    double totalWidth = ScreenCanvas.ActualWidth;
                    double remainingWidth = 1 - CurrentOffset;
                    TimeSpan remainingTime = ExpectedTerminatedTime - DateTime.UtcNow;
                    double delta = (remainingWidth * TravelingTickInterval.TotalMilliseconds) / remainingTime.TotalMilliseconds;
                    CurrentOffset -= delta;
                    double x = CurrentOffset * ScreenCanvas.ActualWidth;

                    Canvas.SetLeft(PreviousMediaPlayer.Container, x - totalWidth / 2.0);
                    Canvas.SetLeft(CurrentMediaPlayer.Container, x);
                    Canvas.SetLeft(NextMediaPlayer.Container, x + totalWidth / 2.0);
                }
            }
        }

        private void TransitionTimer_Tick(object sender, EventArgs e)
        {
            if (--TransitionStep <= 0)
            {
                TransitionTimer.Stop();
                PreviousMediaPlayer.Player.Pause();
            }
            else
            {
                PreviousMediaPlayer.Player.Volume = TransitionStep / TransitionStepCount;
            }
        }

        private void TrackTimer_Tick(object sender, EventArgs e)
        {
            TrackTimer.Interval = TimeSpan.Zero;
            TrackTimer.Stop();
            TrackTransitionStarted?.Invoke(this, null);
        }

        // ##### Configuration
        private TimeSpan TravelingTickInterval = TimeSpan.FromMilliseconds(16);
        private double TransitionDuration = 0.02;
        private double TransitionStepCount = 50;
        // ##### Properties
        private double _Volume = 0.5;
        public double Volume
        {
            get { return _Volume; }
            set
            {
                _Volume = value;
                foreach (MediaPlayerContainer mediaPlayer in MediaPlayers)
                {
                    mediaPlayer.Player.Volume = _Volume;
                }
            }
        }

        // ##### Events
        public EventHandler TrackTransitionStarted;
        // ##### Attributes
        private readonly ITrackLibrary Library;
        private class MediaPlayerContainer
        {
            public Grid Container { get; set; }
            public MediaElement Player { get; set; }
        }
        private readonly List<MediaPlayerContainer> MediaPlayers = new List<MediaPlayerContainer>();
        private double CurrentOffset;
        private int CurrentMediaPlayerIndex = 0;
        private MediaPlayerContainer PreviousMediaPlayer { get { return MediaPlayers[CurrentMediaPlayerIndex <= 0 ? MediaPlayers.Count -1 : CurrentMediaPlayerIndex - 1]; } }
        private MediaPlayerContainer CurrentMediaPlayer { get { return MediaPlayers[CurrentMediaPlayerIndex]; } }
        private MediaPlayerContainer NextMediaPlayer { get { return MediaPlayers[CurrentMediaPlayerIndex >= MediaPlayers.Count - 1 ? 0 : CurrentMediaPlayerIndex + 1]; } }
        private enum EState { Paused, Playing };
        private EState State = EState.Paused;
        private DispatcherTimer TravelingTimer;
        private DateTime ExpectedTerminatedTime;
        private DispatcherTimer TransitionTimer;
        private double TransitionStep;
        private DispatcherTimer TrackTimer;
    }
}
