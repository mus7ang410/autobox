using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Autobox.Core.Data
{
    public class TrackCollection : ObservableCollection<Track>
    {
        public void SetTrackRange(List<Track> tracks)
        {
            CheckReentrancy();
            Items.Clear();
            foreach (Track track in tracks)
            {
                Items.Add(track);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
