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

namespace Autobox.Desktop.Activities.Panels
{
    /// <summary>
    /// Interaction logic for RatingPanel.xaml
    /// </summary>
    public partial class RatingPanel : UserControl
    {
        public RatingPanel()
        {
            InitializeComponent();
            StarButtons = new List<Button>
            {
                Star1Button,
                Star2Button,
                Star3Button,
                Star4Button,
                Star5Button
            };
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanRate)
            {
                Button starButton = sender as Button;
                int index = StarButtons.IndexOf(starButton);
                if (index >= 0 && index < StarButtons.Count)
                {
                    Rating = index + 1;
                    SetStars(Rating);
                    RatingChanged?.Invoke(this, Rating);
                }
            }
        }

        private void SetStars(int rating)
        {
            for (int i = 0; i < rating; i++)
            {
                StarButtons[i].OpacityMask = FindResource("IconButton.Brushes.StarFilled") as Brush;
            }
            for (int i = rating; i < StarButtons.Count; i++)
            {
                StarButtons[i].OpacityMask = FindResource("IconButton.Brushes.Star") as Brush;
            }
        }

        // ##### Events
        public static readonly DependencyProperty RatingChangedProperty = DependencyProperty.Register(
            "ValueChanged",
            typeof(EventHandler<int>),
            typeof(RatingPanel),
            new PropertyMetadata());

        public EventHandler<int> RatingChanged
        {
            get { return (EventHandler<int>)GetValue(RatingChangedProperty); }
            set { SetValue(RatingChangedProperty, value); }
        }

        // ##### Properties
        // Rating
        public static readonly DependencyProperty RatingProperty = DependencyProperty.Register(
            "Rating",
            typeof(int),
            typeof(RatingPanel),
            new PropertyMetadata());

        public int Rating
        {
            get { return (int)GetValue(RatingProperty); }
            set
            {
                SetValue(RatingProperty, value);
                SetStars(value);
            }
        }

        // CanRate
        public static readonly DependencyProperty CanRateProperty = DependencyProperty.Register(
            "CanRate",
            typeof(bool),
            typeof(RatingPanel),
            new PropertyMetadata());

        public bool CanRate
        {
            get { return (bool)GetValue(CanRateProperty); }
            set
            {
                SetValue(CanRateProperty, value);
                if (value)
                {
                    SetStars(Rating);
                }
                else
                {
                    SetStars(0);
                }
            }
        }

        // ##### Attributes
        private List<Button> StarButtons = new List<Button>();
    }
}
