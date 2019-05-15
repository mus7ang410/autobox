using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Autobox.Core.Data
{
    public static class Tag
    {
        // ##### CreateTag
        // Create a formatted tag from raw text
        public static string CreateTag(string tag)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string computed = rgx.Replace(tag, "");
            return computed.ToUpper().TrimStart().TrimEnd();
        }

        // ##### CreateTagCollection
        // Compute an input text line to generate a TagCollection
        public static TagCollection CreateTagCollection(string text)
        {
            TagCollection tags = new TagCollection();
            Regex quoteRgx = new Regex("\"[^\"]*\"");
            MatchCollection quoteMatches = quoteRgx.Matches(text);
            foreach (Match quoteMatch in quoteMatches)
            {
                text = text.Replace(quoteMatch.Value, "");
                string quote = quoteMatch.Value.Replace("\"", "");
                tags.Add(CreateTag(quote));
            }

            string[] tokens = text.Split(' ');
            foreach (string token in tokens)
            {
                string computed = CreateTag(token);
                if (computed.Length >= 3)
                {
                    tags.Add(computed);
                }
            }
            return tags;
        }
    }
}
