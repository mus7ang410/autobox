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
        TagCollection ExcludedTagList { get; set; }
        TagCollection OptionalTagList { get; set; }
        TagCollection MandatoryTagList { get; set; }
        List<Track> TrackList { get; }
    }
}
