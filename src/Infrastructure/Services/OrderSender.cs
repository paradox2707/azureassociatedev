using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.Infrastructure.Services;
public class OrderSender : IOrderSender
{
    private readonly HttpClient _httpClient;
    private readonly string _serviceBusConnectionString;
    private readonly string _queueName;

    public OrderSender(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _serviceBusConnectionString = configuration["ServiceBusConnectionString"];
        _queueName = configuration["QueueName"];
    }

    public async Task Run(Order order)
    {
        var orderMessage = JsonConvert.SerializeObject(order);
        var response = await _httpClient.PostAsync("", new StringContent(orderMessage));
        SendToServiceBus(orderMessage);
    }

    private async void SendToServiceBus(string orderMessage)
    {
        await using var client = new ServiceBusClient(_serviceBusConnectionString);
        await using ServiceBusSender sender = client.CreateSender(_queueName);
        try
        {
            var message = new ServiceBusMessage(orderMessage);
            await sender.SendMessageAsync(message);
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
