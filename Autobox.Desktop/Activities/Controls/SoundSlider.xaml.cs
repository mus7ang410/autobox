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

namespace Autobox.Desktop.Activities.Controls
{
    /// <summary>
    /// Interaction logic for SoundSlider.xaml
    /// </summary>
    public partial class SoundSlider : UserControl
    {
        public SoundSlider()
        {
            InitializeComponent();
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MuteButton != null)
            {
                if (Slider.Value == 0)
                {
                    MuteButton.OpacityMask = FindResource("IconButton.Brushes.Muted") as Brush;
                    IsMuted = true;
                }
                else
                {
                    MuteButton.OpacityMask = FindResource("IconButton.Brushes.Sound") as Brush;
                    IsMuted = false;
                }
                ValueChanged?.Invoke(this, Slider.Value);
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMuted)
            {
                MuteButton.OpacityMask = FindResource("IconButton.Brushes.Sound") as Brush;
                Slider.Value = MutedValue > 0 ? MutedValue : 0.1;
                IsMuted = false;
            }
            else
            {
                MuteButton.OpacityMask = FindResource("IconButton.Brushes.Muted") as Brush;
                MutedValue = Slider.Value;
                Slider.Value = 0;
                IsMuted = true;
            }
            ValueChanged?.Invoke(this, Slider.Value);
        }

        // ##### Events
        public static readonly DependencyProperty ValueChangedProperty = DependencyProperty.Register(
            "ValueChanged",
            typeof(EventHandler<double>),
            typeof(SoundSlider),
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
            typeof(SoundSlider),
            new PropertyMetadata());

        public double Value
        {
            get { return Slider.Value; }
            set
            {
                SetValue(ValueProperty, value);
                Slider.Value = value;
            }
        }

        // ##### Attributes
        public bool IsMuted { get; private set; } = false;
        private double MutedValue = 0;
    }
}
