using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EmojiSharp.Table;
using Microsoft.Net.Http.Headers;

namespace EmojiSharp.Functions
{
    public static class GetEmojiImg
    {
        private static readonly Random Random = new Random();

        [FunctionName(nameof(GetEmojiImg))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "img/{emoji}")] HttpRequest req,
            string emoji,
            ILogger log)
        {
            if (!string.IsNullOrWhiteSpace(emoji) && EmojiMetadata.Lookup.ContainsKey(emoji))
            {
                var emojiImg = await EmojiTable.GetEmojiImg(
                    EmojiMetadata.Lookup[emoji].groupId.ToString(EmojiMetadata.IdFormat),
                    EmojiMetadata.Lookup[emoji].emojiId.ToString(EmojiMetadata.IdFormat));

                if (!string.IsNullOrWhiteSpace(emojiImg?.ImageBase64))
                {
                    var etag = new EntityTagHeaderValue($"\"{Convert.ToString(emojiImg.Timestamp.ToFileTime() ^ emojiImg.ImageBase64.Length, 16)}\"");
                    return new FileContentResult(Convert.FromBase64String(emojiImg.ImageBase64), "image/png")
                    {
                        EntityTag = etag,
                        LastModified = emojiImg.Timestamp
                    };
                }
            }

            return new NotFoundResult();
        }
    }
}
