using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
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
}
