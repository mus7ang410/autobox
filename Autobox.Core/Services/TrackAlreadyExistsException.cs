using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autobox.Core.Services
{
    public class TrackAlreadyExistsException : Exception
    {
        public TrackAlreadyExistsException(string title) : base($"Track {title} is already present into library")
        {

        }
    }
}
