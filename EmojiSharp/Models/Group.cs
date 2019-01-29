using System;
using System.Collections.Generic;
using System.Text;

namespace EmojiSharp
{
    public class Group
    {
        public Group(string name)
        {
            Name = name;
            SubGroups = new List<SubGroup>();
        }

        public string Name { get; set; }
        public IList<SubGroup> SubGroups { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
