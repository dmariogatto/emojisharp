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
    public static class GetEmoji
    {
        [FunctionName(nameof(GetEmoji))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "emoji")] HttpRequest req,
            ILogger log)
        {
            var emojiList = await EmojiTable.GetAllEmojis();

            var groupedEmojis =  
                emojiList.GroupBy(e => e.Group)
                         .Select(g => new 
                         { 
                            Group = g.Key,
                            SubGroups = g.GroupBy(e => e.SubGroup)
                                         .Select(sg => new {
                                             SubGroup = sg.Key,
                                             Emojis = sg.Select(e => new {
                                                 Emoji = e.Emoji,
                                                 Cldr = e.Cldr,
                                                 Codes = e.Code.Split('_'),
                                                 Keywords = e.Keywords,
                                             })
                                         })
                         });

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return new JsonResult(groupedEmojis, serializerSettings);
        }
    }
}
