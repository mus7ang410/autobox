using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autobox.Core.Data;

namespace Autobox.Core.Services
{
    public interface IPlaylistManager
    {
        Task Shuffle();

        // ##### Attributes
        HashSet<string> ExcludedTagList { get; set; }
        HashSet<string> OptionalTagList { get; set; }
        HashSet<string> MandatoryTagList { get; set; }
        List<Track> TrackList { get; }
    }
}
