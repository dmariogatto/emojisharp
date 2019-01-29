using System;
using System.Collections.Generic;
using System.Text;

namespace EmojiSharp
{
    public class SubGroup
    {
        public SubGroup(string name)
        {
            Name = name;
            Emojis = new List<Symbol>();
        }

        public string Name { get; set; }
        public IList<Symbol> Emojis { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
