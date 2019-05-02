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
    /// Interaction logic for SoundSliderPanel.xaml
    /// </summary>
    public partial class SoundSliderPanel : UserControl
    {
        public SoundSliderPanel()
        {
            InitializeComponent();
        }

        private void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MuteButton != null)
            {
                if (SoundSlider.Value == 0)
                {
                    MuteButton.OpacityMask = FindResource("Player.Brushes.Muted") as Brush;
                    IsMuted = true;
                }
                else
                {
                    MuteButton.OpacityMask = FindResource("Player.Brushes.Sound") as Brush;
                    IsMuted = false;
                }
                ValueChanged?.Invoke(this, SoundSlider.Value);
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMuted)
            {
                MuteButton.OpacityMask = FindResource("Player.Brushes.Sound") as Brush;
                SoundSlider.Value = MutedValue > 0 ? MutedValue : 0.1;
                IsMuted = false;
            }
            else
            {
                MuteButton.OpacityMask = FindResource("Player.Brushes.Muted") as Brush;
                MutedValue = SoundSlider.Value;
                SoundSlider.Value = 0;
                IsMuted = true;
            }
            ValueChanged?.Invoke(this, SoundSlider.Value);
        }

        // ##### Events
        public static readonly DependencyProperty ValueChangedProperty = DependencyProperty.Register(
            "ValueChanged",
            typeof(EventHandler<double>),
            typeof(SoundSliderPanel),
            new PropertyMetadata());

        public EventHandler<double> ValueChanged
        {
            get { return (EventHandler<double>)GetValue(ValueChangedProperty); }
            set { SetValue(ValueChangedProperty, value); }
        }

        // ##### Properties
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(double),
            typeof(SoundSliderPanel),
            new PropertyMetadata());

        public double Value
        {
            get { return SoundSlider.Value; }
            set
            {
                SetValue(ValueProperty, value);
                SoundSlider.Value = value;
            }
        }

        // ##### Attributes
        public bool IsMuted { get; private set; } = false;
        private double MutedValue = 0;
    }
}
