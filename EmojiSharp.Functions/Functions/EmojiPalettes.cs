using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Serialization;
using EmojiSharp.Table;

namespace EmojiSharp.Functions
{
    public static class GetEmojiPalettes
    {
        [FunctionName(nameof(GetEmojiPalettes))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "emoji/palettes")] HttpRequest req,
            ILogger log)
        {
            var emojiList = await EmojiTable.GetAllEmojis();
            
            var groupedEmojis =  
                emojiList.GroupBy(e => e.Group)
                         .Select(g => new 
                         { 
                            Group = g.Key,
                            Emoji = g.Select(e => e.Emoji)
                         });

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return new JsonResult(groupedEmojis, serializerSettings);
        }
    }
}
