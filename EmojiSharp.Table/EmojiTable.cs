using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace EmojiSharp.Table
{
    public static class EmojiTable
    {
        private static IConfigurationRoot Configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
               .Build();

        public static CloudTable Get()
        {
            var storageConnString = Configuration.GetConnectionString("StorageConnectionString") ?? "UseDevelopmentStorage=true;";

            var storageAccount = CloudStorageAccount.Parse(storageConnString);
            var tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference("Emoji");
        }

        public static async Task<List<EmojiEntity>> GetAllEmojis(string partitionKey = "")
        {
            var emojiTable = EmojiTable.Get();
            // Base query to get all entitys
            var emojiQuery = new TableQuery<EmojiEntity>();

            if (!string.IsNullOrEmpty(partitionKey))
            {
                emojiQuery = emojiQuery.Where(
                    TableQuery.GenerateFilterCondition(
                        nameof(TableEntity.PartitionKey),
                        QueryComparisons.Equal,
                        partitionKey));
            }

            var emojiList = new List<EmojiEntity>();
            // Initialize continuation token to start from the beginning of the table.
            var continuationToken = default(TableContinuationToken);

            do
            {
                // Retrieve a segment (1000 entities)
                var tableQueryResult =
                    await emojiTable.ExecuteQuerySegmentedAsync(emojiQuery, continuationToken);

                // Assign the new continuation token to tell the service where to
                // continue on the next iteration (or null if it has reached the end)
                continuationToken = tableQueryResult.ContinuationToken;

                emojiList.AddRange(tableQueryResult.Results);
            } while (continuationToken != null);

            return emojiList;
        }

        public static async Task<EmojiEntity> GetEmoji(string partitionKey, string rowKey)
        {
            var emojiTable = EmojiTable.Get();
            var retrieveOperation = TableOperation.Retrieve<EmojiEntity>(partitionKey, rowKey);
            var result = await emojiTable.ExecuteAsync(retrieveOperation);

            return result.Result as EmojiEntity;
        }
    }
}