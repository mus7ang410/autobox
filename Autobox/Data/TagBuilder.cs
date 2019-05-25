using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Autobox.Data
{
    public static class TagBuilder
    {
        // ##### ParseText
        // Parse an input text line to generate a TagCollection
        public static TagCollection ParseText(string text)
        {
            TagCollection tags = new TagCollection();

            List<string> tokens = ExtractTokens(text);
            for (int i = 0; i < tokens.Count;)
            {
                if (tokens[i].StartsWith("$"))
                {
                    if (i + 1 < tokens.Count)
                    {
                        tags.Add(new Tag(CategoryFromString(tokens[i]), tokens[i + 1]));
                        i += 2;
                    }
                    else
                    {
                        i += 1;
                    }
                }
                else
                {
                    tags.Add(new Tag(Tag.ECategory.Custom, tokens[i]));
                    i += 1;
                }
            }
            return tags;
        }

        private static List<string> ExtractTokens(string text)
        {
            List<string> tokens = new List<string>();

            // extract quoted strings
            Regex quoteRgx = new Regex("\"[^\"]*\"");
            MatchCollection quoteMatches = quoteRgx.Matches(text);
            foreach (Match quoteMatch in quoteMatches)
            {
                text = text.Replace(quoteMatch.Value, "");
                string quote = quoteMatch.Value.Replace("\"", "");
                tokens.Add(quote);
            }

            tokens.AddRange(text.Split(' '));

            return tokens;
        }

        private static Tag.ECategory CategoryFromString(string value)
        {
            string key = value.ToLower();
            if (Categories.ContainsKey(key))
            {
                return Categories[key];
            }
            return Tag.ECategory.Custom;
        }

        // ##### Attributes
        private static Dictionary<string, Tag.ECategory> Categories = new Dictionary<string, Tag.ECategory>
        {
            { "artist", Tag.ECategory.Artist }, { "a", Tag.ECategory.Artist },
            { "period", Tag.ECategory.Period }, { "p", Tag.ECategory.Period},
            { "genre", Tag.ECategory.Genre }, { "g", Tag.ECategory.Genre},
            { "country", Tag.ECategory.Country }, { "c", Tag.ECategory.Country}
        };
    }
}
