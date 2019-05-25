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

        public override bool Equals(object obj)
        {
            Tag tag = obj as Tag;
            if (tag == null)
            {
                return false;
            }

            return tag.Category == Category && tag.Value == Value;
        }

        public override string ToString() { return Value; }
        public override int GetHashCode() { return $"{Category}:{Value}".GetHashCode(); }
        private static string Normalize(string value) { return value.ToUpper().TrimStart().TrimEnd(); }

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
