using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace EmojiSharp.Table
{
    public class EmojiEntity : TableEntity
    {
        public EmojiEntity() { }

        public EmojiEntity(string groupId, string id)
        {
            PartitionKey = groupId;
            RowKey = id;
        }

        public string Group { get; set; }
        public string SubGroup { get; set; }
        public string Code { get; set; }
        public string Emoji { get; set; }
        public string Cldr { get; set; }
        public string Keywords { get; set; }
        public bool IsNew { get; set; }

        public override string ToString()
        {
            return Cldr;
        }
    }
}
