using EmojiSharp.Table;
using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EmojiSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome Emoji Hunter");
               
            var forceDownload = ReadResponse("Force HTML emoji list update? (Y/N) ");
            
            CreateTableAndPopulate(LoadEmojis(forceDownload)).Wait();

            Console.WriteLine("Complete!");
        }

        private static bool ReadResponse(string question)
        {
            Console.Write(question);
            
            bool isValidResponse(string input)
            {
                return input?.ToUpperInvariant() == "Y" ||
                       input?.ToUpperInvariant() == "N";
            }

            var response = default(string);
            while (!isValidResponse(response = Console.ReadLine()?.FirstOrDefault().ToString()))
                Console.Write(question);

            return response.ToUpperInvariant() == "Y";
        }

        static Emojis LoadEmojis(bool forceDownload = false)
        {         
            var emojiList = new Emojis() { LastUpdate = DateTime.UtcNow };

            var url = "https://unicode.org/emoji/charts/emoji-list.html";
            var htmlFilePath = Path.Combine(AppContext.BaseDirectory, "emoji.html");

            if (forceDownload || !File.Exists(htmlFilePath))
            {
                Console.WriteLine($"Downloading '{url}' (~25MB might take a while!)");
                var sw = new System.Diagnostics.Stopwatch();

                using (var client = new HttpClient())
                {
                    var rawHtml = client.GetStringAsync(url).Result;
                    File.WriteAllText(htmlFilePath, rawHtml);
                }

                sw.Stop();
                Console.WriteLine($"Document downloaded ({sw.ElapsedMilliseconds}ms), processing...");
            }

            var doc = new HtmlDocument();

            using (var fs = new FileStream(htmlFilePath, FileMode.Open, FileAccess.Read))
            {
                doc.Load(fs);
            }

            var header = doc.DocumentNode.SelectSingleNode("//h1");
            emojiList.Version = header?.InnerText ?? string.Empty;

            foreach (var row in doc.DocumentNode.SelectNodes("//tr"))
            {
                if (row.ChildNodes.Count == 1)
                {
                    var categoryNode = row.FirstChild;
                    var categoryName = HtmlEntity.DeEntitize(categoryNode.InnerText);
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        if (categoryNode.HasClass("bighead"))
                        {
                            Console.WriteLine($"#{categoryName}#");
                            emojiList.Groups.Add(new Group(categoryName));
                        }
                        else if (categoryNode.HasClass("mediumhead"))
                        {
                            Console.WriteLine($"  ##{categoryName}##");
                            emojiList.Groups
                                .Last()
                                .SubGroups
                                .Add(new SubGroup(categoryName));
                        }
                    }
                }
                else
                {
                    var emojiRow = row.SelectNodes("td");
                    if (emojiRow?.Count == 5)
                    {
                        var id = int.Parse(emojiRow[0].InnerText);
                        var codes = emojiRow[1].InnerText.Split(" ").Select(c => c.Remove(0, 2));
                        var imgNode = emojiRow[2].SelectSingleNode("a/img");
                        var emoji = imgNode.GetAttributeValue("alt", string.Empty);
                        var imageBase64 = imgNode.GetAttributeValue("src", string.Empty)
                                                 .Replace("data:image/png;base64,", string.Empty);
                        var cldr = HtmlEntity.DeEntitize(emojiRow[3].InnerText).TrimStart("⊛ ".ToArray());
                        var keywords = emojiRow[4].InnerText
                            .Split(" | ")
                            .Select(s => HtmlEntity.DeEntitize(s));
                        var isNew = emojiRow[3].InnerText.StartsWith("⊛");

                        var newEmoji = new Symbol()
                        {
                            Id = id,
                            Codes = codes.ToList(),
                            Emoji = emoji,
                            ImageBase64 = imageBase64,
                            Cldr = cldr,
                            Keywords = keywords.ToList(),
                            IsNew = isNew
                        };
                        Console.WriteLine($"    {newEmoji}");
                        emojiList.Groups.Last().SubGroups.Last().Emojis.Add(newEmoji);
                    }
                }
            }

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var prettySerializerSettings = new JsonSerializerSettings();
            prettySerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            prettySerializerSettings.Formatting = Formatting.Indented;

            var baseDirectory = Path.Combine(AppContext.BaseDirectory, "../../../../EmojiSharp/");
            File.WriteAllText(
                    Path.Combine(baseDirectory, "emoji.json"),
                    JsonConvert.SerializeObject(emojiList, serializerSettings));

            File.WriteAllText(
                Path.Combine(baseDirectory, "emoji.pretty.json"),
                JsonConvert.SerializeObject(emojiList, prettySerializerSettings));

            var moji = emojiList.Groups.Select(g =>
                    string.Join(string.Empty,
                                g.SubGroups.SelectMany(sg => sg.Emojis.Select(e => e.Emoji))));

            File.WriteAllLines(
                Path.Combine(baseDirectory, "emoji.palette.txt"),
                moji);

            var emojiLookup = (from g in emojiList.Groups
                               from sg in g.SubGroups
                               from e in sg.Emojis
                               let groupId = emojiList.Groups.IndexOf(g)
                               let emojiId = e.Id
                               select new { Emoji = e.Emoji, GroupId = groupId, EmojiId = emojiId });

            File.WriteAllLines(
                Path.Combine(baseDirectory, "emojiLookup.txt"),
                emojiLookup.Select(i => $"{{@\"{i.Emoji}\",({i.GroupId}, {i.EmojiId})}},"));

            var groupsAndIdRange = (from g in emojiList.Groups
                                    let ids = g.SubGroups.SelectMany(sg => sg.Emojis.Select(e => e.Id))
                                    let groupName = g.Name.Replace(" & ", "-").ToLower()
                                    let minId = ids.Min()
                                    let maxId = ids.Max()
                                    select new { GroupName = groupName, Range = $"{minId}:{maxId}" });

            File.WriteAllLines(
                Path.Combine(baseDirectory, "groups.txt"),
                groupsAndIdRange.Select(g => g.GroupName));

            File.WriteAllLines(
                Path.Combine(baseDirectory, "groupsWithRange.txt"),
                groupsAndIdRange.Select(g => $"{g.GroupName},{g.Range}"));

            File.WriteAllLines(
                Path.Combine(baseDirectory, "subGroups.txt"),
                emojiList.Groups.SelectMany(g => g.SubGroups.Select(sg => sg.Name)));
            
            return emojiList;
        }

        async static Task CreateTableAndPopulate(Emojis emojisToLoad)
        {
            var table = EmojiTable.Get();
            var imgTable = EmojiTable.GetImg();

            // Drop and recreate table
            await table.DeleteIfExistsAsync();
            await table.CreateIfNotExistsAsync();

            await imgTable.DeleteIfExistsAsync();
            await imgTable.CreateIfNotExistsAsync();

            //Entities
            var emojis = new List<EmojiEntity>();
            //Create the batch operation
            var batchOps = new List<TableBatchOperation>();
            var imgBatchOps = new List<TableBatchOperation>();

            foreach (var g in emojisToLoad.Groups)
            {
                var batchOp = new TableBatchOperation();
                var imgBatchOp = new TableBatchOperation();
                var groupId = emojisToLoad.Groups.IndexOf(g).ToString(EmojiMetadata.IdFormat);

                foreach (var sg in g.SubGroups)
                {
                    foreach (var e in sg.Emojis)
                    {
                        var entity = new EmojiEntity(groupId, e.Id.ToString(EmojiMetadata.IdFormat))
                        {
                            Group = g.Name,
                            SubGroup = sg.Name,
                            Code = e.CodeString,
                            Emoji = e.Emoji,
                            Cldr = e.Cldr,
                            Keywords = e.Haystack,
                            IsNew = e.IsNew
                        };
                        emojis.Add(entity);
                        batchOp.Insert(entity);

                        if (!string.IsNullOrWhiteSpace(e.ImageBase64))
                        {
                            var imgEntity = new EmojiImageEntity(groupId, e.Id.ToString(EmojiMetadata.IdFormat))
                            {
                                Emoji = e.Emoji,
                                ImageBase64 = e.ImageBase64
                            };

                            imgBatchOp.Insert(imgEntity);                   
                        }

                        // Maximum operations in a batch
                        if (batchOp.Count == 100)
                        {
                            batchOps.Add(batchOp);
                            batchOp = new TableBatchOperation();

                            imgBatchOps.Add(imgBatchOp);
                            imgBatchOp = new TableBatchOperation();
                        }
                    }
                }

                // Batch can only contain operations in the same partition
                if (batchOp.Count > 0)
                {
                    batchOps.Add(batchOp);
                }
            }

            var sw = new System.Diagnostics.Stopwatch();
            var processedCount = 0;
            foreach (var bo in batchOps)
            {
                sw.Reset();
                Console.WriteLine($"Inserting batch for group {bo.First().Entity.PartitionKey}, {bo.Count} entries");                
                sw.Start();
                var result = await table.ExecuteBatchAsync(bo);
                processedCount += bo.Count;
                sw.Stop();
                Console.WriteLine($"  Insert complete: {sw.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Processed: {processedCount} emojis");
            }

            processedCount = 0;
            Console.WriteLine($"###IMAGES###");
            foreach (var bo in imgBatchOps)
            {
                sw.Reset();
                Console.WriteLine($"Inserting image batch for group {bo.First().Entity.PartitionKey}, {bo.Count} entries");
                sw.Start();
                var result = await imgTable.ExecuteBatchAsync(bo);
                sw.Stop();
                processedCount += bo.Count;
                Console.WriteLine($"  Insert complete: {sw.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Processed: {processedCount} emojis");
            }
        }
    }
}
