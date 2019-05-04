using System;
using System.Collections.Generic;
using System.Collections;
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
    /// Interaction logic for TagPanel.xaml
    /// </summary>
    public partial class TagPanel : UserControl
    {
        public TagPanel()
        {
            InitializeComponent();
            Library = ServiceProvider.GetService<ITrackLibrary>();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddTags();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddTags();
        }

        // ##### AddTags
        // Tags creation and adding business
        private void AddTags()
        {
            TagList.UnionWith(Library.CreateTagList(InputTextBox.Text));
            TagWrapPanel_Populate();
            InputTextBox.Text = string.Empty;
        }

        private void TagWrapPanel_Populate()
        {
            if (TagList != TagListCache)
            {
                TagWrapPanel.Children.Clear();
                if (TagList != null)
                {
                    foreach (string tag in TagList)
                    {
                        Button button = new Button
                        {
                            Content = "  " + tag.ToUpper() + "  ",
                            Style = FindResource("TagButtonStyle") as Style
                        };
                        button.Click += TagButon_Click;
                        TagWrapPanel.Children.Add(button);
                    }
                    TagListCache = new HashSet<string>(TagList);
                }
                else
                {
                    TagListCache = null;
                }
                
                TagListChanged?.Invoke(this, TagList);
            }
        }

        private static void TagSourceProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TagPanel).TagSource_Changed((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        private void TagSource_Changed(IEnumerable oldValue, IEnumerable newValue)
        {
            TagList = newValue as HashSet<string>;
            TagWrapPanel_Populate();
        }

        private void TagButon_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            TagWrapPanel.Children.Remove(button);
            string tag = button.Content as string;
            TagList.Remove(tag.TrimStart().TrimEnd().ToUpper());
            TagListChanged?.Invoke(this, TagList);
        }

        // ##### Properties
        // Title
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                TitleLabel.Content = value;
                if (IsMultiple)
                {
                    TitleLabel.Content += " (MULTIPLE)";
                }
            }
        }

        // TagSource
        public static readonly DependencyProperty TagSourceProperty = DependencyProperty.Register(
            "TagSource",
            typeof(IEnumerable),
            typeof(TagPanel),
            new PropertyMetadata(new PropertyChangedCallback(TagSourceProperty_Changed)));

        public IEnumerable TagSource
        {
            get { return (IEnumerable)GetValue(TagSourceProperty); }
            set { SetValue(TagSourceProperty, value); }
        }

        private bool _IsMultiple = false;
        public bool IsMultiple
        {
            get { return _IsMultiple; }
            set
            {
                _IsMultiple = value;
                Title = Title;
            }
        }

        // ##### Events
        // TagListChanged
        public static readonly DependencyProperty TagListChangedProperty = DependencyProperty.Register(
            "TagListChanged",
            typeof(EventHandler<HashSet<string>>),
            typeof(TagPanel),
            new PropertyMetadata());

        public EventHandler<HashSet<string>> TagListChanged
        {
            get { return (EventHandler<HashSet<string>>)GetValue(TagListChangedProperty); }
            set { SetValue(TagListChangedProperty, value); }
        }

        // ##### Attributes
        private readonly ITrackLibrary Library;
        private HashSet<string> TagList = new HashSet<string>();
        private HashSet<string> TagListCache = new HashSet<string>();
    }
}
