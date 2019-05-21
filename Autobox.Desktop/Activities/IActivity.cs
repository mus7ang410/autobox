using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Autobox.Desktop.Activities
{
    public class ActivityBackgroundChangedEventArgs : EventArgs
    {
        public ActivityBackgroundChangedEventArgs(Color requestedColor)
        {
            RequestedColor = requestedColor;
        }

        public readonly Color RequestedColor;
    }

    public interface IActivity
    {
        void OnActivated();
        void OnDeactivated();
    }
}
