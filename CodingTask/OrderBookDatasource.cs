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
        private readonly SemaphoreSlim _datasourceConnectionLock = new SemaphoreSlim(1, 1);
        private volatile DatasourceState _datasourceState;
        private BitstampWS client = null;

        private IHubContext<OrderBookHub> Hub { get; set; }
        public DatasourceState DatasourceState
        {
            get { return _datasourceState; }
            private set { _datasourceState = value; }
        }

        public async Task ConnectWS()
        {
            //Hub.Groups.
                // naredi za vsak tip tradingpaira instanco WS potem grupiraj uporabnike po tipu in jim pošiljaj tudi po tipu

            // Connect to Bitstamp web socket
            if (client == null)
            {
                client = new BitstampWS();

                // Add event handlers
                client.Connected += async (sender, EventArgs) => { await Hub.Clients.All.SendAsync("bitstampConnected"); };
                client.ResponseReceived += async (sender, eventArgument) => { await Hub.Clients.All.SendAsync("dataReceived", new OrderBookDataModel(eventArgument), new CancellationToken()); };
                client.Disconnected += async (sender, eventArgument) => { await Hub.Clients.All.SendAsync("bitstampDisconnected"); };
            }
            await client.ConnectAsync();
        }
        public async Task DisconnectWS()
        {
            await client.DisconnectAsync();
        }
        public async Task StartReceivingData(string tradingPair)
        {
            await _datasourceConnectionLock.WaitAsync();
            try
            {
                if (DatasourceState != DatasourceState.Connected)
                {
                    DatasourceState = DatasourceState.Connected;
                    
                    // Subscribe to order book stream
                    await client.Subscribe(tradingPair);

                    await InformClientsStateChange(DatasourceState.Connected);
                }
            }
            finally
            {
                _datasourceConnectionLock.Release();
            }
        }

        public async Task StopReceivingData()
        {
            await _datasourceConnectionLock.WaitAsync();
            try
            {
                if (DatasourceState == DatasourceState.Connected)
                {
                    DatasourceState = DatasourceState.Disconnected;

                    // Unsubscribe
                    await client.Unsubscribe();
                    await InformClientsStateChange(DatasourceState.Disconnected);
                }
            }
            finally
            {
                _datasourceConnectionLock.Release();
            }
        }
        private async Task InformClientsStateChange(DatasourceState dataSourceState)
        {
            switch (dataSourceState)
            {
                case DatasourceState.Connected:
                    {
                        await Hub.Clients.All.SendAsync("connected");
                        break;
                    }
                    
                case DatasourceState.Disconnected:
                    {
                        await Hub.Clients.All.SendAsync("disconnected");
                        break;
                    }                    
                default:
                    break;
            }
        }
    }
    public enum DatasourceState
    {
        Disconnected,
        Connected
    }
}
