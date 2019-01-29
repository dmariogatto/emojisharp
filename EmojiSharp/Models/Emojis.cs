using System;
using System.Collections.Generic;
using System.Text;

namespace EmojiSharp
{
    public class Emojis
    {
        public Emojis()
        {
            Groups = new List<Group>();
        }

        public string Version { get; set; }
        public IList<Group> Groups { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
