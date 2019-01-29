using System;
using System.Collections.Generic;

namespace EmojiSharp.Table
{
    public static class EmojiMetadata
    {
        public static string IdFormat = "0000000000";

        public static readonly IDictionary<string, (int min, int max)> Groups = new Dictionary<string, (int min, int max)>()
        {
            { "smileys-emotion", (1, 146) },
            { "people-body", (147, 437) },
            { "component", (438, 441) },
            { "animals-nature", (442, 561) },
            { "food-drink", (562, 673) },
            { "travel-places", (674, 877) },
            { "activities", (878, 953) },
            { "objects", (954, 1171) },
            { "symbols", (1172, 1376) },
            { "flags", (1377, 1644) }
        };
    }
}
