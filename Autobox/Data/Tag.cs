using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Autobox.Data
{
    public class Tag
    {
        public enum ECategory { Artist, Period, Genre, Country, Custom }

        public Tag(ECategory category, string value)
        {
            Category = category;
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        private static string Normalize(string value)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string computed = rgx.Replace(value, "");
            return computed.ToUpper().TrimStart().TrimEnd();
        }

        // ##### Properties
        public bool Automatic { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ECategory Category;
        [JsonIgnore]
        private string _Value;
        public string Value
        {
            get { return _Value; }
            set { _Value = Normalize(value); }
        }
    }
}
