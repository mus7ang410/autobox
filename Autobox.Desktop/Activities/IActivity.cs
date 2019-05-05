using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autobox.Desktop.Activities
{
    public interface IActivity
    {
        void OnActivated();
        void OnDeactivated();
    }
}
