using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Autobox.Core.Data
{
    public class TrackCollection : ObservableCollection<TrackMetadata>
    {
        public void SetTrackRange(List<TrackMetadata> tracks)
        {
            CheckReentrancy();
            Items.Clear();
            foreach (TrackMetadata track in tracks)
            {
                Items.Add(track);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
