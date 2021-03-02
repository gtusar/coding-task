using CodingTask.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodingTask
{
    public class BitstampWS : IDisposable
    {
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<BitstampWSResponse> ResponseReceived;

        private string url = "wss://ws.bitstamp.net";
        private string TradingPair { get; set; }
        public int ReceiveBufferSize { get; set; } = 8192;
        private ClientWebSocket WS;
        private CancellationTokenSource CTS;
        private CancellationTokenSource CTSReadLoop;
        private readonly SemaphoreSlim _WSsemaphore = new SemaphoreSlim(1, 1);
        public WebSocketState State
        {
            get
            {
                return this.WS.State;
            }
        }
        private SubscriptionState _subscriptionStatus { get; set; }
        public SubscriptionState SubscriptionStatus
        {
            get
            {
                return _subscriptionStatus;
            }
        }

        /// <summary>
        /// Create and connect web socket instance
        /// Trigger Connected event on success
        /// </summary>
        /// <returns></returns>
        private async Task ConnectAsync()
        {
            if (WS != null)
            {
                if (WS.State == WebSocketState.Open) return;
                else WS.Dispose();
            }
            WS = new ClientWebSocket();
            if (CTS != null) CTS.Dispose();
            CTS = new CancellationTokenSource();
            await WS.ConnectAsync(new Uri(url), CTS.Token);
            if(WS.State == WebSocketState.Open)
            {
                Connected.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Subscribe to selected trading pair channel
        /// On success start ReceivingLoop to handle incoming data
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(string tradingPair = "btceur")
        {
            try
            {
                TradingPair = tradingPair;
                if (WS == null || WS.State != WebSocketState.Open)
                {
                    await ConnectAsync();
                }

                if (_subscriptionStatus == SubscriptionState.Unsubscribed)
                {
                    WebSocketResponse response = await Subscription(new Models.Subscribe(TradingPair));
                    _subscriptionStatus = response == WebSocketResponse.Ok ? SubscriptionState.Subscribed : SubscriptionState.Unsubscribed;
                }

                CTSReadLoop = new CancellationTokenSource();
                await Task.Factory.StartNew(ReceiveLoop, CTSReadLoop.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch
            {
                _subscriptionStatus = SubscriptionState.Unsubscribed;
                return;
            }
        }

        /// <summary>
        /// Stop ReceivingLoop and unsubscribe from channel
        /// </summary>
        /// <returns></returns>
        public async Task UnsubscribeAsync()
        {
            CTSReadLoop.Cancel();
            if(_subscriptionStatus == SubscriptionState.Subscribed)
            {
                WebSocketResponse response = await Subscription(new Models.Subscribe(TradingPair));
                _subscriptionStatus = response == WebSocketResponse.Ok ? SubscriptionState.Unsubscribed : SubscriptionState.Subscribed;
            }
        }

        /// <summary>
        /// Unsubscribe, cancel ReceivingLoop and dispose web socket client
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (WS is null) return;

            if (WS.State == WebSocketState.Open)
            {
                if (_subscriptionStatus != SubscriptionState.Unsubscribed)
                {
                    await Subscription(new Models.Unsubscribe(TradingPair));
                }
                await _WSsemaphore.WaitAsync();
                try
                {
                    CTSReadLoop.Cancel();
                    CTS.Cancel();
                    await WS.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                    await WS.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                finally
                {
                    _WSsemaphore.Release();
                }
            }
            if (WS.State == WebSocketState.Closed)
            {
                Disconnected.Invoke(this, EventArgs.Empty);
            }
            WS.Dispose();
            WS = null;
            CTS.Dispose();
            CTS = null;
            CTSReadLoop.Dispose();
            CTSReadLoop = null;
        }

        /// <summary>
        /// Read data from web socket to memory and call read response
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveLoop()
        {
            var loopToken = CTSReadLoop.Token;
            MemoryStream outputStream = null;
            WebSocketReceiveResult receiveResult = null;
            var buffer = new byte[ReceiveBufferSize];
            
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    outputStream = new MemoryStream(ReceiveBufferSize);
                    do
                    {
                        await _WSsemaphore.WaitAsync();
                        try
                        {
                            receiveResult = await WS.ReceiveAsync(buffer, CTS.Token);
                            if (receiveResult.MessageType != WebSocketMessageType.Close)
                                outputStream.Write(buffer, 0, receiveResult.Count);
                        }
                        catch (WebSocketException)
                        {
                            await DisconnectAsync();
                            await ConnectAsync();
                            await SubscribeAsync();                            
                        }
                        _WSsemaphore.Release();
                    }
                    while (!receiveResult.EndOfMessage);
                    if (receiveResult.MessageType == WebSocketMessageType.Close) break;
                    outputStream.Position = 0;
                    ReadResponse(outputStream);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                outputStream?.Dispose();
            }
        }

        /// <summary>
        /// Method for sending subscription or unsubscription message
        /// </summary>
        /// <typeparam name="RequestType"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<WebSocketResponse> Subscription<RequestType>(RequestType message)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message));
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            if (State != WebSocketState.Open) await ConnectAsync();

            bool succeded = false;
            await _WSsemaphore.WaitAsync();
            
            try
            {
                await WS.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: CTS.Token);
                succeded = true;
            }
            catch (WebSocketException)
            {
                await DisconnectAsync();
                await ConnectAsync();
                await SubscribeAsync();
            }
            _WSsemaphore.Release();
            return succeded ? WebSocketResponse.Ok : WebSocketResponse.Error;
        }

        /// <summary>
        /// Parse received stream of data to object.
        /// Call ResponseReceived event
        /// </summary>
        /// <param name="inputStream"></param>
        private void ReadResponse(Stream inputStream)
        {
            try
            {
                using (var reader = new StreamReader(inputStream, Encoding.UTF8))
                {
                    string value = reader.ReadToEnd();
                    BitstampWSResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<BitstampWSResponse>(value);
                    ResponseReceived.Invoke(this, response);
                }
            }
            finally
            {
                inputStream.Dispose();
            }
            
            
        }

        public void Dispose() => DisconnectAsync().Wait();

        public enum WebSocketResponse
        {
            Ok,
            Error
        }
        public enum SubscriptionState
        {
            Unsubscribed,
            Subscribed
        }

    }

}