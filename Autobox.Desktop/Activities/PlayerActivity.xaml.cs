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

namespace Autobox.Desktop.Activities
{
    /// <summary>
    /// Interaction logic for PlayerActivity.xaml
    /// </summary>
    public partial class PlayerActivity : UserControl, IActivity
    {
        public PlayerActivity()
        {
            Playlist = ServiceProvider.GetService<IPlaylistManager>();
            InitializeComponent();
            NoneOfTagPanel.TagSource = Playlist.Settings.NoneOfTagList;
            AnyOfTagPanel.TagSource = Playlist.Settings.AnyOfTagList;
            AllOfTagPanel.TagSource = Playlist.Settings.AllOfTagList;
            PlayerPanel.CurrentTrackChanged += delegate (object sender, TrackMetadataEventArgs e)
            {
                Random r = new Random();
                ActivityBackgroundChanged?.Invoke(this, new ActivityBackgroundChangedEventArgs(
                    Color.FromArgb(255, (byte)r.Next(1, 255), (byte)r.Next(1, 255), (byte)r.Next(1, 255))
                    ));
            };
        }

        public void OnActivated()
        {
            
        }

        public void OnDeactivated()
        {
            PlayerPanel.Pause();
        }

        // ##### Events
        public EventHandler<ActivityBackgroundChangedEventArgs> ActivityBackgroundChanged { get; set; }
        // ##### Attributes
        private IPlaylistManager Playlist;
    }
}
