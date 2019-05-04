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
using Autobox.Desktop.Activities.Panels;

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
            ExcludedTagPanel.TagSource = Playlist.ExcludedTagList;
            OptionalTagPanel.TagSource = Playlist.OptionalTagList;
            MandatoryTagPanel.TagSource = Playlist.MandatoryTagList;
        }

        public void OnActivated()
        {
            
        }

        public void OnDeactivated()
        {
            PlayerPanel.Pause();
        }

        // ##### Attributes
        private IPlaylistManager Playlist;
    }
}
