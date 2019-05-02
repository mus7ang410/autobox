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
            Playlist = ServiceProvider.GetService<IPlaylistManager>();

            TravelingTimer = new DispatcherTimer
            {
                Interval = TravelingTickInterval
            };
            TravelingTimer.Tick += TravelingTimer_Tick;
        }

        private void ScreenCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecomputeContainerPositions();
        }

        public void Shuffle()
        {
            Playlist.Shuffle();
            if (Playlist.Count > 0)
            {
                ScreenCanvas.Children.Clear();
                CurrentTrackContainer = new TrackContainer(Playlist.CurrentTrack, EndTransitionDuration);
                ScreenCanvas.Children.Add(CurrentTrackContainer.LayoutGrid);
                if (Playlist.NextTracks.Count > 0)
                {
                    NextTrackContainer = new TrackContainer(Playlist.NextTracks.First(), EndTransitionDuration);
                    ScreenCanvas.Children.Add(NextTrackContainer.LayoutGrid);
                }
                CurrentOffset = 0;

                RecomputeContainerPositions();
            }
            else
            {
                PreviousTrackContainer = null;
                CurrentTrackContainer = null;
                NextTrackContainer = null;
            }

            if (State == EState.Play)
            {
                State = EState.Pause;
                Play();
            }
        }

        public void Play()
        {
            if (State == EState.Pause)
            {
                if (CurrentTrackContainer == null)
                {
                    Shuffle();
                }

                if (CurrentTrackContainer != null)
                {
                    MediaElement player = CurrentTrackContainer.MediaPlayer;
                    CurrentTrackContainer.Play(delegate (TimeSpan duration)
                    {
                        ExpectedTerminatedTime = DateTime.UtcNow.Add(duration - player.Position);
                        LastTravelingTickDate = DateTime.UtcNow;

                        TravelingTimer.Start();
                        State = EState.Play;
                    });
                }
            }
        }

        public void Pause()
        {
            if (State == EState.Play)
            {
                CurrentTrackContainer.Pause();
                TravelingTimer.Stop();
                State = EState.Pause;
            }
        }

        public void Skip()
        {

        }

        private void TravelingTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan remainingTime = ExpectedTerminatedTime - DateTime.UtcNow;
            double remainingWidth = (1 - CurrentOffset);
            TimeSpan dt = DateTime.UtcNow - LastTravelingTickDate;
            double dx = dt.TotalMilliseconds * remainingWidth / remainingTime.TotalMilliseconds;
            CurrentOffset += dx;
            LastTravelingTickDate = DateTime.UtcNow;
            RecomputeContainerPositions();
        }

        private void RecomputeContainerPositions()
        {
            if (CurrentTrackContainer.LayoutGrid.ActualWidth != 0)
            {
                double totalWidth = ScreenCanvas.ActualWidth;
                double currentX = (totalWidth / 2) - (CurrentTrackContainer.LayoutGrid.ActualWidth * CurrentOffset);
                CurrentTrackContainer.LayoutGrid.Height = ScreenCanvas.ActualHeight;
                Canvas.SetLeft(CurrentTrackContainer.LayoutGrid, currentX);

                if (PreviousTrackContainer != null)
                {
                    PreviousTrackContainer.LayoutGrid.Height = ScreenCanvas.ActualHeight;
                    double previousX = currentX - PreviousTrackContainer.LayoutGrid.ActualWidth;
                    Canvas.SetLeft(PreviousTrackContainer.LayoutGrid, previousX);
                }

                if (NextTrackContainer != null)
                {
                    NextTrackContainer.LayoutGrid.Height = ScreenCanvas.ActualHeight;
                    double nextX = currentX + CurrentTrackContainer.LayoutGrid.ActualWidth;
                    Canvas.SetLeft(NextTrackContainer.LayoutGrid, nextX);
                }
            }
        }

        // ##### Properties
        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register(
            "Volume",
            typeof(double),
            typeof(ScreenPanel),
            new PropertyMetadata());

        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set
            {
                SetValue(VolumeProperty, value);
                CurrentTrackContainer.Volume = value;
            }
        }
        // ##### Configuration
        private readonly TimeSpan TravelingTickInterval = TimeSpan.FromMilliseconds(16);
        private readonly TimeSpan EndTransitionDuration = TimeSpan.FromSeconds(5);
        // ##### Attributes
        private class TrackContainer
        {
            public TrackContainer(Track track, TimeSpan endTrensitionDuration)
            {
                ITrackLibrary library = ServiceProvider.GetService<ITrackLibrary>();
                ContainedTrack = track;
                LayoutGrid = new Grid();
                ThumbnailImage = new Image
                {
                    Source = new BitmapImage(new Uri(library.GetFilePath(ContainedTrack.ThumbnailFilename)))
                };
                Panel.SetZIndex(ThumbnailImage, -1);
                LayoutGrid.Children.Add(ThumbnailImage);
                Canvas.SetTop(LayoutGrid, 0);
                MediaPlayer = new MediaElement();
                MediaPlayer.Source = new Uri(library.GetFilePath(ContainedTrack.VideoFilename));
                MediaPlayer.LoadedBehavior = MediaState.Manual;
                MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
                MediaPlayer.Pause();
                MediaPlayer.Visibility = Visibility.Hidden;
                LayoutGrid.Children.Add(MediaPlayer);

                // Transition
                TransitionVolume = 1.0;
                TransitionTimer = new DispatcherTimer
                {
                    Interval = endTrensitionDuration
                };
            }

            public delegate void OnMediaLoaded(TimeSpan duration);
            public void Play(OnMediaLoaded onMediaLoaded)
            {
                MediaPlayer.Visibility = Visibility.Visible;
                MediaPlayer.Play();
                if (TrackTimer != null)
                {
                    TrackTimer.Start();
                }
                
                if (TransitionStep > 0)
                {
                    TransitionTimer.Start();
                }

                if (MediaPlayer.NaturalDuration != Duration.Automatic)
                {
                    onMediaLoaded(MediaPlayer.NaturalDuration.TimeSpan);
                }
                else
                {
                    MediaLoadedCallback = onMediaLoaded;
                }
            }

            public void Pause()
            {
                MediaPlayer.Visibility = Visibility.Hidden;
                MediaPlayer.Pause();
                TrackTimer.Stop();
                if (TransitionStep > 0)
                {
                    TransitionTimer.Stop();
                }
            }

            private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
            {
                LayoutGrid.Width = MediaPlayer.Width;
                TrackTimer = new DispatcherTimer
                {
                    Interval = MediaPlayer.NaturalDuration.TimeSpan
                };
                TrackTimer.Tick += Timer_Tick;
                if (State == EState.Play)
                {
                    TrackTimer.Start();
                }

                MediaLoadedCallback?.Invoke(MediaPlayer.NaturalDuration.TimeSpan);
                MediaLoadedCallback = null;
            }

            private void Timer_Tick(object sender, EventArgs e)
            {
                TrackTimer.Stop();
                TrackAlmostFinished?.Invoke(this, ContainedTrack);
                TransitionTimer.Start();
                TransitionTimer.Tick += TransitionTimer_Tick;
                TransitionStep = TransitionStepCount;
            }

            private void TransitionTimer_Tick(object sender, EventArgs e)
            {
                if (TransitionStep > 0)
                {
                    --TransitionStep;
                    TransitionVolume = (double)TransitionStepCount / (double)TransitionStep;
                }
                else
                {
                    TransitionTimer.Stop(); ;
                }
            }

            // ##### Events
            public EventHandler<Track> TrackAlmostFinished;
            // ##### Configuration
            private readonly int TransitionStepCount = 50;
            // ##### Properties
            public enum EState { Play, Pause };
            public EState State = EState.Pause;
            public double _Volume;
            public double Volume
            {
                get { return _Volume; }
                set
                {
                    _Volume = value;
                    MediaPlayer.Volume = value * TransitionVolume;
                }
            }
            // ##### Attributes
            private readonly Track ContainedTrack;
            public readonly Grid LayoutGrid;
            private readonly Image ThumbnailImage;
            public readonly MediaElement MediaPlayer;
            private OnMediaLoaded MediaLoadedCallback = null;
            private DispatcherTimer TrackTimer;
            // Final transition
            private DispatcherTimer TransitionTimer;
            private double TransitionVolume = 1.0;
            private int TransitionStep = 0;
        }

        private ITrackLibrary Library;
        private IPlaylistManager Playlist;
        public enum EState { Play, Pause };
        public EState State { get; private set; } = EState.Pause;
        // Container
        private TrackContainer PreviousTrackContainer = null;
        private TrackContainer CurrentTrackContainer;
        private TrackContainer NextTrackContainer;
        // Traveling
        private double CurrentOffset = 0;
        private DateTime ExpectedTerminatedTime;
        private DispatcherTimer TravelingTimer;
        private DateTime LastTravelingTickDate;
    }
}
