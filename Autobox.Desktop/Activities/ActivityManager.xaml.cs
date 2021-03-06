﻿using System;
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

namespace Autobox.Desktop.Activities
{
    /// <summary>
    /// Interaction logic for ActivityManager.xaml
    /// </summary>
    public partial class ActivityManager : UserControl
    {
        public ActivityManager()
        {
            InitializeComponent();
            AddActivity(PlayerButton, new PlayerActivity());
            AddActivity(LibraryButton, new LibraryActivity());
            RibbonButton_Click(PlayerButton, null);
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (CurrentActivityButton != null)
            {
                (CurrentActivity as IActivity).OnDeactivated();
                LayoutGrid.Children.Remove(CurrentActivity);
            }

            CurrentActivityButton = button;
            CurrentActivity = Activities[CurrentActivityButton];
            LayoutGrid.Children.Add(CurrentActivity);
            Grid.SetRow(CurrentActivity, 1);
            (CurrentActivity as IActivity).OnActivated();
        }

        private void AddActivity<TActivity>(Button button, TActivity activity) where TActivity : UserControl, IActivity
        {
            Activities.Add(button, activity);
        }

        // ##### Attributes
        private Button CurrentActivityButton = null;
        private UserControl CurrentActivity = null;
        private Dictionary<Button, UserControl> Activities = new Dictionary<Button, UserControl>();
    }
}
