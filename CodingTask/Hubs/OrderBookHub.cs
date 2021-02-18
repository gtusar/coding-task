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
        public static HashSet<string> ConnectedIds = new HashSet<string>();
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
            await _datasource.ConnectWS();
            await _datasource.StartReceivingData(tradingPair);
        }
        public async Task StopReceivingData()
        {
            await _datasource.StopReceivingData();
        }

        public override async Task OnConnectedAsync()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            // The last who exits closes the light :)
            if(UserHandler.ConnectedIds.Count == 0) await _datasource.DisconnectWS(); 
            await base.OnDisconnectedAsync(exception);
        }
    }
}
