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
        void Shuffle();
        void Forward();

        // ##### Attributes
        Track PreviousTrack { get; }
        Track CurrentTrack { get; }
        Track NextTrack { get; }
    }
}
