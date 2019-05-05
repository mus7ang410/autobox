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
using System.Windows.Threading;

namespace Autobox.Desktop.Activities.Controls
{
    /// <summary>
    /// Interaction logic for DynamicBackground.xaml
    /// </summary>
    public partial class DynamicBackground : UserControl
    {
        public DynamicBackground()
        {
            InitializeComponent();

            StopCollection = new GradientStopCollection();
            StopCollection.Add(new GradientStop(Color.FromArgb(255, 59, 75, 89), 1));
            StopCollection.Add(new GradientStop(Color.FromArgb(255, 71, 97, 97), 0));
            Brush = new LinearGradientBrush(StopCollection)
            {
                StartPoint = new Point(0, 1),
                EndPoint = new Point(1, 0)
            };
            BackgroundRectangle.Fill = Brush;

            TravelingTimer = new DispatcherTimer();
            TravelingTimer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
            TravelingTimer.Tick += TravelingTimer_Tick;
            LastTravelingTickDate = DateTime.UtcNow;
            TravelingTimer.Start();
        }

        public void SetBackgroundTargetColor(Color requestedColor)
        {
            RequestedColor = requestedColor;
            HasRequestedColor = true;
        }

        private void TravelingTimer_Tick(object sender, EventArgs e)
        {
            if (StopCollection.First().Offset >= 1 && HasRequestedColor)
            {
                StopCollection.Add(new GradientStop(RequestedColor, -1));
                HasRequestedColor = false;
            }

            if (StopCollection.Count > 2)
            {
                TimeSpan elapsed = DateTime.UtcNow - LastTravelingTickDate;
                double speed = elapsed.TotalMilliseconds / TravalingDuration.TotalMilliseconds;
                foreach (GradientStop stop in StopCollection)
                {
                    stop.Offset += speed;
                }

                while (StopCollection.Count > 1 && StopCollection[1].Offset >= 1)
                {
                    StopCollection.RemoveAt(0);
                }
            }

            LastTravelingTickDate = DateTime.UtcNow;
        }

        // ##### Attributes
        private readonly LinearGradientBrush Brush;
        private readonly GradientStopCollection StopCollection;
        private bool HasRequestedColor = false;
        private Color RequestedColor;
        // Traveling
        private DispatcherTimer TravelingTimer;
        private DateTime LastTravelingTickDate;
        private readonly TimeSpan TravalingDuration = TimeSpan.FromSeconds(10);
    }
}
