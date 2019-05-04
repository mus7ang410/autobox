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

namespace Autobox.Desktop.Activities.Controls
{
    /// <summary>
    /// Interaction logic for WideScreenPlayer.xaml
    /// </summary>
    public partial class WideScreenPlayer : UserControl
    {
        public WideScreenPlayer()
        {
            InitializeComponent();
            TravelingTimerInterval = TimeSpan.FromMilliseconds(60.0 / 1000.0);
            TravelingTimer = new DispatcherTimer();
            TravelingTimer.Interval = TravelingTimerInterval;
            TravelingTimer.Tick += TravelingTimer_Tick;
        }

        public void Load(List<Track> tracks)
        {
            if (tracks.Count > 0)
            {
                int index = 0;
                if (CurrentTrackPlayer == null || !CurrentTrackPlayer.IsTrackPlaying)
                {
                    CurrentTrackPlayer = null;
                    MoveToCurrentTrack(CreatePlayer(tracks.First()));
                    ++index;
                }

                QueuedTrackPlayers = new List<TrackPlayer>();
                foreach (Track track in tracks.GetRange(index, Math.Min(QueuedTrackCount, tracks.Count - index)))
                {
                    QueuedTrackPlayers.Add(CreatePlayer(track));
                }
                index += QueuedTrackPlayers.Count;
                if (tracks.Count > index)
                {
                    PendingTracks = new List<Track>(tracks.GetRange(index, tracks.Count - index));
                }
            }
            RecomputeScreen();
        }

        public void Play()
        {
            if (State == EState.Paused)
            {
                foreach (TrackPlayer fadingOutTrack in FadingTrackPlayers)
                {
                    fadingOutTrack.Play();
                }
                CurrentTrackPlayer?.Play(FadingDurationFactor);
                TravelingTimer.Start();
                PreviousTravelingTickDate = DateTime.UtcNow;
                State = EState.Playing;
            }
        }

        public void Pause()
        {
            if (State == EState.Playing)
            {
                foreach (TrackPlayer fadingOutTrack in FadingTrackPlayers)
                {
                    fadingOutTrack.Pause();
                }
                CurrentTrackPlayer?.Pause();
                TravelingTimer.Stop();
                State = EState.Paused;
            }
        }

        public void Skip()
        {
            Forward();
        }

        // ##### Forward
        // Move to the next track
        private void Forward()
        {
            if (QueuedTrackPlayers.Count > 0)
            {
                if (CurrentTrackPlayer != null && CurrentTrackPlayer.IsTrackFadingOut)
                {
                    FadingTrackPlayers.Add(CurrentTrackPlayer);
                }

                MoveToCurrentTrack(QueuedTrackPlayers.First());
                QueuedTrackPlayers.RemoveAt(0);
                if (PendingTracks.Count > 0)
                {
                    QueuedTrackPlayers.Add(CreatePlayer(PendingTracks.First()));
                    PendingTracks.RemoveAt(0);
                }
            }
            else if (CurrentTrackPlayer != null)
            {
                CurrentTrackPlayer.Skip(SkipFadingDuration);
                MoveToFadingTracks(CurrentTrackPlayer);
                CurrentTrackPlayer = null;
            }

            RecomputeScreen();
        }

        // ##### CreatePlayer
        // Create a new player from a track and initialize it
        private TrackPlayer CreatePlayer(Track track)
        {
            TrackPlayer player = new TrackPlayer
            {
                CurrentTrack = track,
                Volume = Volume
            };
            player.TrackLoaded += TrackPlayer_TrackLoaded;
            player.Height = ScreenCanvas.ActualHeight;
            Canvas.SetTop(player, 0);
            ScreenCanvas.Children.Add(player);

            return player;
        }

        // ##### MoveToCurrentTrack
        // 1. Move a track to the current one
        private void MoveToCurrentTrack(TrackPlayer player)
        {
            if (CurrentTrackPlayer != null && !CurrentTrackPlayer.IsTrackEnded && !CurrentTrackPlayer.IsTrackFadingOut)
            {
                CurrentTrackPlayer.Skip(SkipFadingDuration);
                MoveToFadingTracks(CurrentTrackPlayer);
            }

            CurrentTrackPlayer = player;
            CurrentTrackPlayer.Volume = Volume;
            CurrentTrackPlayer.TrackFadingOut += delegate (object sender, Track track) { Forward(); };
            if (State == EState.Playing)
            {
                CurrentTrackPlayer.Play(FadingDurationFactor);
            }
            CurrentTrackPlayer.TrackFadingOut += delegate (object sender, Track track)
            {
                MoveToFadingTracks(sender as TrackPlayer);
            };
        }

        // ##### MoveToFadingTracks
        // 2. Once no more the current track, keep it a fading out
        private void MoveToFadingTracks(TrackPlayer player)
        {
            FadingTrackPlayers.Insert(0, player);
            player.TrackEnded += delegate (object sender, Track track)
            {
                MoveToPreviousTracks(sender as TrackPlayer);
                FadingTrackPlayers.Remove(sender as TrackPlayer);
            };
        }

        // ##### MoveToPreviousTracks
        // 3. Finally, when the fading is done, keep as a previous track until out of screen
        private void MoveToPreviousTracks(TrackPlayer player)
        {
            PreviousTrackPlayers.Insert(0, player);
        }

        private void TrackPlayer_TrackLoaded(object sender, Track track)
        {
            RecomputeScreen();
        }

        private void TravelingTimer_Tick(object sender, EventArgs e)
        {
            RecomputeScreen();
            PreviousTravelingTickDate = DateTime.UtcNow;
        }

        private void RecomputeScreen()
        {
            if (CurrentTrackPlayer != null && CurrentTrackPlayer.IsTrackLoaded)
            {
                double leftCursorX = 0;
                if (CurrentTrackPlayer.NaturalDuration != Duration.Automatic)
                {
                    DateTime now = DateTime.UtcNow;
                    TimeSpan totalDuration = CurrentTrackPlayer.NaturalDuration.TimeSpan;
                    TimeSpan currentDuration = CurrentTrackPlayer.Position;
                    TimeSpan remainingDuration = totalDuration - currentDuration;
                    double offsetX = (ScreenCanvas.ActualWidth / 2);
                    double playerWidth = CurrentTrackPlayer.ActualWidth;
                    if (remainingDuration > TimeSpan.Zero)
                    {
                        TimeSpan tickDuration = now - PreviousTravelingTickDate;
                        double endX = offsetX - playerWidth;
                        double currentX = currentDuration.TotalMilliseconds * playerWidth / totalDuration.TotalMilliseconds;
                        double remainingX = endX - currentX;
                        double tickX = tickDuration.TotalMilliseconds * remainingX / remainingDuration.TotalMilliseconds;
                        leftCursorX = offsetX - currentX - tickX;
                    }
                    else
                    {
                        leftCursorX = offsetX - playerWidth;
                    }
                }
                else
                {
                    leftCursorX = (ScreenCanvas.ActualWidth / 2);
                }
                double rightCursorX = leftCursorX + CurrentTrackPlayer.ActualWidth;

                Canvas.SetLeft(CurrentTrackPlayer, leftCursorX);
                CurrentTrackPlayer.Height = ScreenCanvas.ActualHeight;

                foreach (TrackPlayer player in FadingTrackPlayers)
                {
                    leftCursorX -= player.ActualWidth;
                    Canvas.SetLeft(player, leftCursorX);
                    player.Height = ScreenCanvas.ActualHeight;
                }
                int outOfScreenIndex = -1;
                foreach (TrackPlayer player in PreviousTrackPlayers)
                {
                    leftCursorX -= player.ActualWidth;
                    if (leftCursorX + player.ActualWidth < 0)
                    {
                        outOfScreenIndex = PreviousTrackPlayers.IndexOf(player);
                        break;
                    }
                    Canvas.SetLeft(player, leftCursorX);
                    player.Height = ScreenCanvas.ActualHeight;
                }
                if (outOfScreenIndex >= 0)
                {
                    PreviousTrackPlayers.RemoveRange(outOfScreenIndex, PreviousTrackPlayers.Count - outOfScreenIndex);
                }

                foreach (TrackPlayer player in QueuedTrackPlayers)
                {
                    Canvas.SetLeft(player, rightCursorX);
                    player.Height = ScreenCanvas.ActualHeight;
                    rightCursorX += player.ActualWidth;
                }
            }
        }

        // ##### Properties
        public double _Volume = 1;
        public double Volume
        {
            get { return _Volume; }
            set
            {
                _Volume = value;
                foreach (TrackPlayer player in FadingTrackPlayers)
                {
                    player.Volume = value;
                }
                if (CurrentTrackPlayer != null)
                {
                    CurrentTrackPlayer.Volume = value;
                }
            }
        }

        // ##### Attributes
        private List<TrackPlayer> PreviousTrackPlayers = new List<TrackPlayer>();
        private List<TrackPlayer> FadingTrackPlayers = new List<TrackPlayer>();
        private TrackPlayer CurrentTrackPlayer = null;
        private readonly int QueuedTrackCount = 3;
        private List<TrackPlayer> QueuedTrackPlayers = new List<TrackPlayer>();
        private List<Track> PendingTracks = new List<Track>();
        private enum EState { Playing, Paused };
        private EState State = EState.Paused;
        // Fading
        private readonly double FadingDurationFactor = 0.05;
        private readonly TimeSpan SkipFadingDuration = TimeSpan.FromSeconds(6);
        // Traveling
        private readonly TimeSpan TravelingTimerInterval;
        private readonly DispatcherTimer TravelingTimer;
        private DateTime PreviousTravelingTickDate;
    }
}
