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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autobox.Data;
using Autobox.Services;
using Autobox.Desktop.Services;

namespace Autobox.Desktop.Activities.Panels
{
    /// <summary>
    /// Interaction logic for AddYouTubePanel.xaml
    /// </summary>
    public partial class AddYouTubePanel : UserControl
    {
        public AddYouTubePanel()
        {
            InitializeComponent();
            Downloader = new YouTubeTrackDownloader();
            Tagger = new MusicBrainzTrackTagger();
        }

        private async void LinkTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AddTrackAsync();
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            await AddTrackAsync();
        }

        // ##### AddTrackAsync
        // Track creation business
        private async Task AddTrackAsync()
        {
            Cursor previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                ILibrary library = ServiceProvider.GetService<ILibrary>();
                TrackMetadata track = await Downloader.DownloadTrackAsync(library, LinkTextBox.Text);
                await Tagger.TagTrackAsync(track);
                await library?.AddTrackAsync(track);
                CreateTrack?.Invoke(this, new TrackMetadataEventArgs(track));
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    exception.Message,
                    "Cannot create a new track",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = previousCursor;
            }
            LinkTextBox.Text = string.Empty;
        }

        // ##### Properties
        public static readonly DependencyProperty CreateTrackProperty =
            DependencyProperty.Register("CreateTrack", 
                typeof(EventHandler<TrackMetadataEventArgs>),
                typeof(AddYouTubePanel), 
                new PropertyMetadata());

        public EventHandler<TrackMetadataEventArgs> CreateTrack
        {
            get { return (EventHandler<TrackMetadataEventArgs>)GetValue(CreateTrackProperty); }
            set { SetValue(CreateTrackProperty, value); }
        }

        // ##### Attributes
        private ITrackDownloader Downloader;
        private ITrackTagger Tagger;
    }
}
