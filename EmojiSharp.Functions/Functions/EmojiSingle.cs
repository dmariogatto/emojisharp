using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EmojiSharp.Table;

namespace EmojiSharp.Functions
{
    public static class GetEmojiSingle
    {
        [FunctionName(nameof(GetEmojiSingle))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "emoji/{emoji}")] HttpRequest req,
            string emoji,
            ILogger log)
        {
            if (!string.IsNullOrWhiteSpace(emoji) && EmojiMetadata.Lookup.ContainsKey(emoji))
            {
                var emojiEntity = await EmojiTable.GetEmoji(
                    EmojiMetadata.Lookup[emoji].groupId.ToString(EmojiMetadata.IdFormat),
                    EmojiMetadata.Lookup[emoji].emojiId.ToString(EmojiMetadata.IdFormat));

                if (emojiEntity != null)
                {
                    var jsonObject = new
                    {
                        Group = emojiEntity.Group,
                        SubGroup = emojiEntity.SubGroup,
                        Emoji = emojiEntity.Emoji,
                        Cldr = emojiEntity.Cldr,
                        Codes = emojiEntity.Code.Split('_'),
                        Keywords = emojiEntity.Keywords
                    };
                    var serializerSettings = new JsonSerializerSettings();
                    serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    return new JsonResult(jsonObject, serializerSettings);
                }
            }

            return new NotFoundResult();            
        }
    }
}
