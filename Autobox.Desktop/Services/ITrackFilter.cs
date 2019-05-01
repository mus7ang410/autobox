using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autobox.Core.Data;

namespace Autobox.Desktop.Services
{
    public interface ITrackFilter
    {
        void UpdateFilter(HashSet<string> toExclude, HashSet<string> toInclude);
        void AddFilteredTrack(Track track);
        ObservableCollection<Track> FilteredTrackList { get; }
    }
}
