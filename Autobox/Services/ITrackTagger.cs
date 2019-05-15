using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Autobox.Data;

namespace Autobox.Services
{
    public interface ITrackTagger
    {
        Task TagTrackAsync(TrackMetadata track);
    }
}
