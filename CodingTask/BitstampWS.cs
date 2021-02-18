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
        public event EventHandler<BitstampWSResponse> ResponseReceived;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        private ClientWebSocket WS;
        private CancellationTokenSource CTS;
        private CancellationTokenSource CTSReadLoop;
        private readonly SemaphoreSlim _WSsemaphore = new SemaphoreSlim(1, 1);

        private string url = "wss://ws.bitstamp.net";
        private string TradingPair { get; set; }
        public int ReceiveBufferSize { get; set; } = 8192;

        public async Task ConnectAsync()
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

        public async Task Subscribe(string tradingPair = "btceur")
        {
            TradingPair = tradingPair;
            await Subscription(new Models.Subscribe(TradingPair));
            CTSReadLoop = new CancellationTokenSource();
            await Task.Factory.StartNew(ReceiveLoop, CTSReadLoop.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        public async Task Unsubscribe()
        {
            CTSReadLoop.Cancel();
            await Subscription(new Models.Unsubscribe(TradingPair));
        }
        public async Task DisconnectAsync()
        {
            if (WS is null) return;

            if (WS.State == WebSocketState.Open)
            {
                await Subscription(new Models.Unsubscribe(TradingPair));
                CTS.Cancel(); // After(TimeSpan.FromSeconds(2));
                await WS.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await WS.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
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
            CTSReadLoop.Dispose();

        }

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
                        receiveResult = await WS.ReceiveAsync(buffer, CTS.Token);
                        _WSsemaphore.Release();
                        if (receiveResult.MessageType != WebSocketMessageType.Close)
                            outputStream.Write(buffer, 0, receiveResult.Count);
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

        private async Task<ResponseType> Subscription<RequestType>(RequestType message)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message));
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            try
            {
                await _WSsemaphore.WaitAsync();
                await WS.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: CTS.Token);
                _WSsemaphore.Release();
                return ResponseType.Ok;
            }
            catch (Exception e)
            {
                return ResponseType.Error;
            }
        }

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

        public enum ResponseType
        {
            Ok,
            Error
        }

    }

}