using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EmojiSharp
{
    public static class EmojiAccessor
    {
        public static readonly Emojis Emojis;
        public static readonly IDictionary<string, IList<string>> GroupsAndSubGroups;

        static EmojiAccessor()
        {
            var assembly = Assembly.GetAssembly(typeof(EmojiAccessor));
            var resourceName = "EmojiSharp.emoji.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                Emojis = JsonConvert.DeserializeObject<Emojis>(result);
                GroupsAndSubGroups = new Dictionary<string, IList<string>>();

                foreach (var g in Emojis.Groups)
                    GroupsAndSubGroups.Add(g.Name, g.SubGroups.Select(sg => sg.Name).ToList());
            }
        }
    }
}
