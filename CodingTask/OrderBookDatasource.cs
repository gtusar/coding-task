using CodingTask.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CodingTask.Models;

namespace CodingTask
{
    public class OrderBookDatasource
    {
        public OrderBookDatasource(IHubContext<OrderBookHub> hub)
        {
            Hub = hub;
        }

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private IHubContext<OrderBookHub> Hub { get; set; }

        // Bitstamp Web Socket clients for each trading pair
        private Dictionary<string, BitstampWS> clients = new Dictionary<string, BitstampWS>();

        /// <summary>
        /// Create a Web Socket client, connect to datasource and subscribe to channel with selected trading pair
        /// </summary>
        /// <param name="tradingPair">String of selected trading pair</param>
        /// <returns></returns>
        public async Task StartReceivingData(string tradingPair)
        {

            await semaphore.WaitAsync();
            try
            {
                BitstampWS client = null;
                if (!clients.ContainsKey(tradingPair))
                {
                    client = new BitstampWS();
                    client.Connected += async (sender, eventArguments) => { await InformClientsStateChange(client.State); };
                    client.Disconnected += async (sender, eventArguments) => { await InformClientsStateChange(client.State); };
                    client.ResponseReceived += async (sender, eventArgument) =>
                    {
                        if (eventArgument != null && eventArgument.Event == "data")
                        {
                            OrderbookLog.InsetOrderbook(eventArgument);
                            await Hub.Clients.Group(tradingPair).SendAsync("dataReceived",
                            new OrderBookDataModel(eventArgument),
                            new CancellationToken());
                        }
                    };
                    clients.Add(tradingPair, client);
                    await client.SubscribeAsync(tradingPair);
                }
                else
                {
                    client = clients.GetValueOrDefault(tradingPair);
                    if (client.SubscriptionStatus == BitstampWS.SubscriptionState.Unsubscribed)
                    {
                        await client.SubscribeAsync(tradingPair);
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Unsubscribe from selected tradingPair channel
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public async Task StopReceivingData(string tradingPair)
        {
            BitstampWS client = clients.GetValueOrDefault(tradingPair);
            if (client != null)
            {
                await client.UnsubscribeAsync();
            }
        }

        /// <summary>
        /// Unsubscribe, disconnect WebSocket and dispose it.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public async Task DisconnectWS(string tradingPair)
        {
            await semaphore.WaitAsync();
            try
            {
                BitstampWS client = clients.GetValueOrDefault(tradingPair);
                if (client != null)
                {
                    await client.UnsubscribeAsync();
                    await client.DisconnectAsync();
                    client.Dispose();
                    clients.Remove(tradingPair);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Inform clients about datasource state change
        /// </summary>
        /// <param name="dataSourceState"></param>
        /// <returns></returns>
        private async Task InformClientsStateChange(WebSocketState dataSourceState)
        {
            switch (dataSourceState)
            {
                case WebSocketState.Open:
                    {
                        await Hub.Clients.All.SendAsync("open");
                        break;
                    }

                case WebSocketState.Closed:
                    {
                        await Hub.Clients.All.SendAsync("closed");
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
