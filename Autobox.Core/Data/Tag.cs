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
        public static TagCollection ExtractTagValues(string text)
        {
            TagCollection tags = new TagCollection();
            string[] tokens = text.Split(' ');
            foreach (string token in tokens)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                string computed = rgx.Replace(token, "").ToUpper();
                if (computed.Length >= 3)
                {
                    tags.Add(computed);
                }
            }
            return tags;
        }

        // ##### Attributes
        public string Value { get; set; }
    }
}
