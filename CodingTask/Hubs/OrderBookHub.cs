using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Hubs
{
    /// <summary>
    /// UserHandler keeps track of which user is subscribed to which trading pair data
    /// </summary>
    public static class UserHandler
    {
        /// <summary>
        /// Dictionary of user id as key and trading pair as value
        /// </summary>
        public static Dictionary<string, string> ConnectedClients = new Dictionary<string, string>();
    }

    public class OrderBookHub : Hub
    {
        private readonly OrderBookDatasource _datasource;

        public OrderBookHub(OrderBookDatasource datasource)
        {
            _datasource = datasource;
        }

        /// <summary>
        /// Assigns a user to a group of desired trading pair and calls datasource to start receiving data
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public async Task StartReceivingData(string tradingPair)
        {
            UserHandler.ConnectedClients.Add(Context.ConnectionId, tradingPair);

            await Groups.AddToGroupAsync(Context.ConnectionId, tradingPair);
            await _datasource.StartReceivingData(tradingPair);
        }
        
        /// <summary>
        /// Removes a user from group and stops receiving data if the last user left
        /// </summary>
        /// <returns></returns>
        public async Task StopReceivingData()
        {
            string tradingPair = UserHandler.ConnectedClients.GetValueOrDefault(Context.ConnectionId);
            UserHandler.ConnectedClients.Remove(Context.ConnectionId);
            if (tradingPair != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, tradingPair);
                // Unsubscribe if noone listens to current trading pair
                int currentPairSubscribers = UserHandler.ConnectedClients.Where(c => c.Value == tradingPair).Count();
                if (currentPairSubscribers == 0) { 
                    await _datasource.StopReceivingData(tradingPair);
                    await _datasource.DisconnectWS(tradingPair);
                }
            }
        }

        /// <summary>
        /// Handles user disconnections 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
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
