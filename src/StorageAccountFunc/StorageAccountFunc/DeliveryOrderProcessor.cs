using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Configuration;
using env = System.Environment;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Newtonsoft.Json;
using EShopFunc.Models;

namespace EShopFunc;

public static class DeliveryOrderProcessor
{
    [FunctionName("DeliveryOrderProcessor")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        CosmosClient cosmosClient = new CosmosClientBuilder(
            env.GetEnvironmentVariable("CosmosDBConnection"))
            .Build();
        await cosmosClient.CreateDatabaseIfNotExistsAsync(env.GetEnvironmentVariable("CosmosDB"));
        var container = cosmosClient.GetContainer(env.GetEnvironmentVariable("CosmosDB"), env.GetEnvironmentVariable("CosmosDBContainer"));
        using (StreamReader stream = new StreamReader(req.Body))
        {
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            var orderJson = stream.ReadToEnd();
            log.LogInformation($"string from body - {orderJson}");
            var order = JsonConvert.DeserializeObject<Order>(orderJson);
            log.LogInformation($"order.Id - {order.Id}");
            await container.CreateItemAsync(order);
        }
        return new StatusCodeResult(StatusCodes.Status200OK);
    }

    static async Task HandleCosmosDB(Stream body, ILogger log)
    {
        CosmosClient cosmosClient = new CosmosClientBuilder(
            "AccountEndpoint=https://cosmos-1234.documents.azure.com:443/;AccountKey=4R9zRgQu7Q0x7qBiN8fLF7iuM3JY1Q8RtwkDTUpzdCctsN2il2lXdF1jnUCnpRBIlxcFCQPfE0ELucJ9i3B9mg==;")
            .Build();
        await cosmosClient.CreateDatabaseIfNotExistsAsync("eShop");
        var container = cosmosClient.GetContainer("eShop", "Orders");
        using (StreamReader stream = new StreamReader(body))
        {
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            var orderJson = stream.ReadToEnd();
            log.LogInformation($"string from body - {orderJson}");
            var order = JsonConvert.DeserializeObject<Order>(orderJson);
            log.LogInformation($"order.Id - {order.Id}");
            await container.CreateItemAsync(order);
        }
    }

    static async Task HandleAccountStorage(HttpRequest req, ILogger log)
    {
        log.LogInformation(env.GetEnvironmentVariable("StorageAccountConnection"));

        BlobServiceClient blobServiceClient = new BlobServiceClient(env.GetEnvironmentVariable("StorageAccountConnection"));

        // Get the container (folder) the file will be saved in
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(env.GetEnvironmentVariable("BlobContainer"));

        // Get the Blob Client used to interact with (including create) the blob
        BlobClient blobClient = containerClient.GetBlobClient($"order-{DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss")}.json");

        // Upload the blob
        var result = await blobClient.UploadAsync(req.Body);

        //cosmos
        await HandleCosmosDB(req.Body, log);
        //return new StatusCodeResult(result.GetRawResponse().Status);
    }
}
