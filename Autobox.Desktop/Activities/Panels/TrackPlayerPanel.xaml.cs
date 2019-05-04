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
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            ScreenPanel.Shuffle();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (State == EState.Pause)
            {
                ScreenPanel.Play();
                PlayButton.OpacityMask = FindResource("IconButton.Brushes.Pause") as Brush;
                State = EState.Play;
            }
            else if (State == EState.Play)
            {
                ScreenPanel.Pause();
                PlayButton.OpacityMask = FindResource("IconButton.Brushes.Play") as Brush;
                State = EState.Pause;
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            ScreenPanel.Skip();
        }

        private void SoundSlider_ValueChanged(object sender, double e)
        {
            if (ScreenPanel != null)
            {
                ScreenPanel.Volume = e;
            }
        }

        // ##### Attributes
        private enum EState { Play, Pause };
        private EState State = EState.Pause;
    }
}
