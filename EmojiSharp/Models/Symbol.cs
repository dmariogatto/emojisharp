using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmojiSharp
{
    public class Symbol
    {
        public int Id { get; set; }

        public IList<string> Codes { get; set; }

        private string _codeString;
        [JsonIgnore]
        public string CodeString =>
            _codeString ?? (_codeString = string.Join("_", Codes ?? Enumerable.Empty<string>()));

        public string Emoji { get; set; }

        [JsonIgnore]
        public string ImageBase64 { get; set; }

        public string Cldr { get; set; }
        public IList<string> Keywords { get; set; }

        public bool IsNew { get; set; }
        
        private string _haystack;
        [JsonIgnore]
        public string Haystack => 
            _haystack ?? (_haystack = string.Join("|", new List<string>(Keywords) { Emoji } ?? Enumerable.Empty<string>()));

        public override string ToString()
        {
            return Cldr;
        }
    }
}
