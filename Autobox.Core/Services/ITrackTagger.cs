using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Autobox.Core.Data;

namespace Autobox.Core.Services
{
    public interface ITrackTagger
    {
        Task TagTrackAsync(TrackMetadata track);
    }
}
