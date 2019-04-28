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
using System.Windows.Shapes;

using Autobox.Core.Services;
using Autobox.Core.Services.VideoLibrary;

using Autobox.Desktop.Controls;

namespace Autobox.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Library = new TrackLibrary("Library");
            Library.LoadAllAsync();
            Frames = new Dictionary<string, Control>
            {
                { "Turntable", new TurntableControl() },
                { "Library", new LibraryControl(Library) }
            };

            RibbonButton_Click(TurntableRibbonButton, null);
        }

        private void RibbonButton_Click(object sender, MouseButtonEventArgs e)
        {
            Label ribbon = sender as Label;

            Control frame = Frames[ribbon.Content.ToString()];
            if (frame != CurrentFrame)
            {
                if (CurrentRibbonButton != null)
                {
                    CurrentRibbonButton.Style = FindResource("RibbonButton") as Style;
                }
                CurrentRibbonButton = ribbon;
                CurrentRibbonButton.Style = FindResource("RibbonButtonSelected") as Style;

                if (CurrentFrame != null)
                {
                    LayoutGrid.Children.Remove(CurrentFrame);
                }
                CurrentFrame = frame;
                LayoutGrid.Children.Add(CurrentFrame);
                Grid.SetColumn(CurrentFrame, 0);
                Grid.SetRow(CurrentFrame, 2);
            }
        }

        // ##### Attributes
        private ITrackLibrary Library;
        private Dictionary<string, Control> Frames;
        private Label CurrentRibbonButton = null;
        private Control CurrentFrame = null;
    }
}
