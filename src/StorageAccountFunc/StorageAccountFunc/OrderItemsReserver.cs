using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using env = System.Environment;

namespace EShopFunc;

public static class OrderItemsReserver
{
    [FunctionName("OrderItemsReserver")]
    public static async Task Run([ServiceBusTrigger("%queue-name%")]string myQueueItem, ILogger log)
    {
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        log.LogInformation(env.GetEnvironmentVariable("StorageAccountConnection"));
        try
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(env.GetEnvironmentVariable("StorageAccountConnection"));

            // Get the container (folder) the file will be saved in
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(env.GetEnvironmentVariable("BlobContainer"));

            // Get the Blob Client used to interact with (including create) the blob
            BlobClient blobClient = containerClient.GetBlobClient($"order-{DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss")}.json");

            // Upload the blob
            var result = await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(myQueueItem ?? "")));
        }
        catch (Exception)
        {
            using (var httpClient = new HttpClient())
            {
                await httpClient.PostAsync(env.GetEnvironmentVariable("LogicAppUrl"), new StringContent(myQueueItem, Encoding.UTF8, "application/json"));
            }
        }
        
    }
}
