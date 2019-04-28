using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;

namespace Autobox.Core.Data
{
    public class Tag
    {
        // ##### Helpers
        public static HashSet<string> ExtractTagValues(string text)
        {
            HashSet<string> tags = new HashSet<string>();
            string[] tokens = text.Split(' ');
            foreach (string token in tokens)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                tags.Add(rgx.Replace(token, ""));
            }
            return tags;
        }

        // ##### Attributes
        public string Value { get; set; }
    }
}
