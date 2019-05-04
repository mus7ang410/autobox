using System;
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
using System.Windows.Threading;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autobox.Core.Data;
using Autobox.Core.Services;
using Autobox.Desktop.Services;

namespace Autobox.Desktop.Activities.Controls
{
    /// <summary>
    /// Interaction logic for TrackPlayer.xaml
    /// </summary>
    public partial class TrackPlayer : UserControl
    {
        public TrackPlayer()
        {
            InitializeComponent();
            Library = ServiceProvider.GetService<ITrackLibrary>();
            MediaPlayer.Visibility = Visibility.Hidden;
            FadingTimer = new DispatcherTimer();
            FadingTimer.Tick += FadingTimer_Tick;
        }

        public void Play(double fadingDurationFactor = 0)
        {
            if (!IsLoaded && fadingDurationFactor > 0)
            {
                FadingDurationFactor = fadingDurationFactor;
                FadingTrigger = new DispatcherTimer();
                // Will be configured once the media will be loaded
            }
            MediaPlayer.Play();
            if (CurrentFadingStep > 0)
            {
                FadingTimer.Start();
            }
            MediaPlayer.Visibility = Visibility.Visible;
        }

        public void Pause()
        {
            MediaPlayer.Pause();
            if (CurrentFadingStep > 0)
            {
                FadingTimer.Stop();
            }
            if (IsTrackEnded)
            {
                MediaPlayer.Visibility = Visibility.Hidden;
            }
        }

        public void Skip(TimeSpan fadingOutDuration)
        {
            StartFadingOut(fadingOutDuration);
        }

        private void StartFadingOut(TimeSpan fadingOutDuration)
        {
            IsTrackFadingOut = true;
            if (fadingOutDuration > TimeSpan.Zero)
            {
                FadingTimer.Interval = TimeSpan.FromMilliseconds(fadingOutDuration.TotalMilliseconds / FadingStepCount);
                CurrentFadingStep = FadingStepCount;
                FadingTimer.Start();
            }
            else
            {
                CurrentFadingStep = 0;
                FadingTimer_Tick(null, null);
            }
            
        }

        private void CurrentTrack_Changed(Track track)
        {
            IsTrackLoaded = false;
            MediaPlayer.Source = new Uri(Library.GetFilePath(CurrentTrack.VideoFilename));
            MediaPlayer.Visibility = Visibility.Hidden;
            MediaPlayer.Loaded += MediaPlayer_Loaded;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            ThumbnailImage.Source = new BitmapImage(new Uri(Library.GetFilePath(CurrentTrack.ThumbnailFilename)));
        }

        private void MediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            if (FadingTrigger != null)
            {
                FadingDuration = FadingDurationFactor > 0 ?
                    TimeSpan.FromMilliseconds(MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds * FadingDurationFactor) :
                    MediaPlayer.NaturalDuration.TimeSpan;
                FadingTrigger.Interval = FadingDuration;
                FadingTrigger.Tick += FadingTrigger_Tick;
            }
            IsTrackLoaded = true;
            IsTrackEnded = false;
            Width = MediaPlayer.Width;
            TrackLoaded?.Invoke(this, CurrentTrack);
        }

        private void FadingTrigger_Tick(object sender, EventArgs e)
        {
            TrackFadingOut?.Invoke(this, CurrentTrack);
            StartFadingOut(FadingDuration);
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            Pause();
            IsTrackEnded = true;
            if (!IsTrackFadingOut)
            {
                TrackEnded?.Invoke(this, CurrentTrack);
            }
        }

        private static void TrackProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TrackPlayer player = sender as TrackPlayer;
            player.CurrentTrack_Changed(e.NewValue as Track);
        }

        private void FadingTimer_Tick(object sender, EventArgs e)
        {
            --CurrentFadingStep;
            if (CurrentFadingStep > 0)
            {
                VolumeFactor = CurrentFadingStep / FadingStepCount;
            }
            else
            {
                CurrentFadingStep = 0;
                IsTrackEnded = true;
                Pause();
                MediaPlayer.Position = TimeSpan.Zero;
                TrackEnded?.Invoke(this, CurrentTrack);
                FadingTimer.Stop();
                IsTrackFadingOut = false;
            }
        }

        // ##### Events
        public EventHandler<Track> TrackLoaded { get; set; }
        public EventHandler<Track> TrackFadingOut { get; set; }
        public EventHandler<Track> TrackEnded { get; set; }
        // ##### Properties
        private Track _CurrentTrack = null;
        public Track CurrentTrack
        {
            get { return _CurrentTrack; }
            set
            {
                _CurrentTrack = value;
                CurrentTrack_Changed(value);
            }
        }
        public bool IsTrackLoaded { get; private set; } = false;
        public bool IsTrackFadingOut { get; private set; } = false;
        public bool IsTrackEnded { get; private set; } = false;
        private double _VolumeFactor = 1;
        private double VolumeFactor
        {
            get { return _VolumeFactor; }
            set
            {
                _VolumeFactor = value;
                MediaPlayer.Volume = Volume * _VolumeFactor;
            }
        }
        private double _Volume = 1;
        public double Volume
        {
            get { return _Volume; }
            set
            {
                _Volume = value;
                MediaPlayer.Volume = _Volume * VolumeFactor;
            }
        }
        public TimeSpan Position => MediaPlayer.Position;
        public Duration NaturalDuration => MediaPlayer.NaturalDuration;
        
        // ##### Attributes
        private readonly ITrackLibrary Library;
        // Fading
        private double FadingDurationFactor = 0;
        private TimeSpan FadingDuration;
        private DispatcherTimer FadingTrigger = null;
        private DispatcherTimer FadingTimer;
        private readonly double FadingStepCount = 50;
        private double CurrentFadingStep = 0;
    }
}
