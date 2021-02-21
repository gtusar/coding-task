using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Hubs
{
    public static class UserHandler
    {
        public static Dictionary<string, string> ConnectedClients = new Dictionary<string, string>();
    }

    public class OrderBookHub : Hub
    {
        private readonly OrderBookDatasource _datasource;

        public OrderBookHub(OrderBookDatasource datasource)
        {
            _datasource = datasource;
        }

        public async Task StartReceivingData(string tradingPair)
        {
            UserHandler.ConnectedClients.Add(Context.ConnectionId, tradingPair);

            await Groups.AddToGroupAsync(Context.ConnectionId, tradingPair);
            await _datasource.StartReceivingData(tradingPair);
        }
        public async Task StopReceivingData()
        {
            string tradingPair = UserHandler.ConnectedClients.GetValueOrDefault(Context.ConnectionId);
            UserHandler.ConnectedClients.Remove(Context.ConnectionId);
            if (tradingPair != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, tradingPair);
                // Unsubscribe in noone listens to current trading pair
                int currentPairSubscribers = UserHandler.ConnectedClients.Where(c => c.Value == tradingPair).Count();
                if (currentPairSubscribers == 0) await _datasource.StopReceivingData(tradingPair);
            }
        }

        //public override async Task OnConnectedAsync()
        //{
        //    UserHandler.ConnectedIds.Add(Context.ConnectionId);
        //    await base.OnConnectedAsync();
        //}
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string tradingPair = UserHandler.ConnectedClients.GetValueOrDefault(Context.ConnectionId);
            UserHandler.ConnectedClients.Remove(Context.ConnectionId);

            if (tradingPair != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, tradingPair);
                // The last who exits closes the light :)
                int currentPairSubscribers = UserHandler.ConnectedClients.Where(c => c.Value == tradingPair).Count();
                if (currentPairSubscribers == 0) await _datasource.DisconnectWS(tradingPair);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
