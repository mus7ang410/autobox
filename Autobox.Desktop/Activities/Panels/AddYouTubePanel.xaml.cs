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

using Autobox.Core.Data;
using Autobox.Core.Services;
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
        }

        private async void LinkTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ITrackLibrary library = ServiceProvider.GetService<ITrackLibrary>();
                if (library != null)
                {
                    Cursor previousCursor = Mouse.OverrideCursor;
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        Track track = await library.CreateTrackAsync(LinkTextBox.Text);
                        CreateTrack?.Invoke(this, track);
                    }
                    catch(Exception exception)
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
                }
                LinkTextBox.Text = string.Empty;
            }
        }

        // ##### Properties
        public static readonly DependencyProperty CreateTrackProperty =
            DependencyProperty.Register("CreateTrack", 
                typeof(EventHandler<Track>),
                typeof(AddYouTubePanel), 
                new PropertyMetadata());

        public EventHandler<Track> CreateTrack
        {
            get { return (EventHandler<Track>)GetValue(CreateTrackProperty); }
            set { SetValue(CreateTrackProperty, value); }
        }
    }
}
