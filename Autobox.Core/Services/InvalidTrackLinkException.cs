using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autobox.Core.Services
{
    public class InvalidTrackLinkException : Exception
    {
        public InvalidTrackLinkException(string link) : base ($"<{link}> is not a valid track link")
        {

        }
    }
}
