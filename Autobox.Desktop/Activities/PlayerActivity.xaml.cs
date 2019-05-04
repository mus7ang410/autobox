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

using Autobox.Core.Services;
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
            InitializeComponent();
            Playlist = ServiceProvider.GetService<IPlaylistManager>();
        }

        public void OnActivated()
        {
            
        }

        public void OnDeactivated()
        {
            PlayerPanel.Pause();
        }

        private void ExcludedTagPanel_TagListChanged(object sender, HashSet<string> e)
        {
            Playlist.ExcludedTagList = e;
        }

        private void OptionalTagPanel_TagListChanged(object sender, HashSet<string> e)
        {
            Playlist.OptionalTagList = e;
        }

        private void MandatoryTagPanel_TagListChanged(object sender, HashSet<string> e)
        {
            Playlist.MandatoryTagList = e;
        }

        // ##### Attributes
        private IPlaylistManager Playlist;
    }
}
