using System;
using System.Collections.Generic;
using System.Text;

namespace Autobox.Data
{
    public class PlaylistSettings
    {
        public TagCollection NoneOfTagList { get; set; } = new TagCollection();
        public TagCollection AnyOfTagList { get; set; } = new TagCollection();
        public TagCollection AllOfTagList { get; set; } = new TagCollection();
    }
}
