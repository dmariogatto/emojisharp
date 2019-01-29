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
    public static class GetEmojiRandom
    {
        private static readonly Random Random = new Random();

        [FunctionName(nameof(GetEmojiRandom))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "emoji/random")] HttpRequest req,
            ILogger log)
        {
            var numOfGroups = EmojiMetadata.Groups.Count;
            var randomPartition = Random.Next(0, numOfGroups - 1);
            var gk = EmojiMetadata.Groups.Keys.ElementAt(randomPartition);
            var randomRowKey = Random.Next(EmojiMetadata.Groups[gk].min, EmojiMetadata.Groups[gk].max);

            var emoji = await EmojiTable.GetEmoji(
                randomPartition.ToString(EmojiMetadata.IdFormat),
                randomRowKey.ToString(EmojiMetadata.IdFormat));

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return new JsonResult(new { emoji.Emoji, emoji.Cldr }, serializerSettings);
        }
    }
}
