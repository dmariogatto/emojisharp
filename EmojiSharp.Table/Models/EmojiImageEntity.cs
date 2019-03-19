using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace EmojiSharp.Table
{
    public class EmojiImageEntity : TableEntity
    {
        public EmojiImageEntity() { }

        public EmojiImageEntity(string groupId, string id)
        {
            PartitionKey = groupId;
            RowKey = id;
        }

        public string Emoji { get; set; }
        public string ImageBase64 { get; set; }

        public override string ToString()
        {
            return Emoji;
        }
    }
}
