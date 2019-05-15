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
    public class TagAddedEventArgs : EventArgs
    {
        public TagAddedEventArgs(TagCollection tagList)
        {
            TagList = tagList;
        }

        public readonly TagCollection TagList;
    }

    public class TagRemovedEventArgs : EventArgs
    {
        public TagRemovedEventArgs(string tag)
        {
            Tag = tag;
        }

        public readonly string Tag;
    }

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
            TagSource.UnionWith(Autobox.Core.Data.Tag.CreateTagCollection(InputTextBox.Text));
            InputTextBox.Text = string.Empty;
            TagWrapPanel_Populate();
            TagAdded?.Invoke(this, new TagAddedEventArgs(TagSource));
        }

        private void TagWrapPanel_Populate()
        {
            if (TagSource != TagListCache)
            {
                TagWrapPanel.Children.Clear();
                if (TagSource != null)
                {
                    foreach (string tag in TagSource)
                    {
                        Button button = new Button
                        {
                            Content = "  " + tag.ToUpper() + "  ",
                            Style = FindResource("TagButtonStyle") as Style
                        };
                        button.Click += TagButon_Click;
                        TagWrapPanel.Children.Add(button);
                    }
                    TagListCache = new HashSet<string>(TagSource);
                }
                else
                {
                    TagListCache = null;
                }
            }
        }

        private static void TagSourceProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TagPanel).TagWrapPanel_Populate();
        }

        private void TagButon_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            TagWrapPanel.Children.Remove(button);
            string tag = (button.Content as string).TrimStart().TrimEnd().ToUpper();
            TagSource.Remove(tag);
            TagRemoved?.Invoke(this, new TagRemovedEventArgs(tag));
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
                if (IsMultipleSelection)
                {
                    TitleLabel.Content += " (MULTIPLE)";
                }
            }
        }

        // TagSource
        public static readonly DependencyProperty TagSourceProperty = DependencyProperty.Register(
            "TagSource",
            typeof(TagCollection),
            typeof(TagPanel),
            new PropertyMetadata(new PropertyChangedCallback(TagSourceProperty_Changed)));

        public TagCollection TagSource
        {
            get { return (TagCollection)GetValue(TagSourceProperty); }
            set
            {
                SetValue(TagSourceProperty, value);
                TagWrapPanel_Populate();
            }
        }

        private bool _IsMultipleSelection = false;
        public bool IsMultipleSelection
        {
            get { return _IsMultipleSelection; }
            set
            {
                _IsMultipleSelection = value;
                Title = Title;
            }
        }

        // ##### Events
        // TagListChanged
        public EventHandler<TagAddedEventArgs> TagAdded { get; set; }
        public EventHandler<TagRemovedEventArgs> TagRemoved { get; set; }

        // ##### Attributes
        private readonly ITrackLibrary Library;
        private HashSet<string> TagListCache = new HashSet<string>();
    }
}
