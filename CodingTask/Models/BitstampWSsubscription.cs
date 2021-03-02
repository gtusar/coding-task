using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Models
{
    /// <summary>
    /// Classes to comply with Bitstamp WebSocket described here: https://www.bitstamp.net/websocket/v2/
    /// Subscription class stringifies to valid JSON message that the websocket accepts
    /// </summary>
    public class Subscription
    {
        public Subscription()
        {
            this.Data = new ChannelData();
        }
        
        [JsonProperty("event")]
        public string Event  { get; set; }
        
        [JsonProperty("data")]
        public ChannelData Data { get; set; }
    }
    public class Subscribe : Subscription
    {
        public Subscribe(string tradingPair)
        {
            Event = "bts:subscribe";
            Data.Channel = $"order_book_{tradingPair}";
        }
    }
    public class Unsubscribe: Subscription
    {
        public Unsubscribe(string tradingPair)
        {
            Event = "bts:unsubscribe";
            Data.Channel = $"order_book_{tradingPair}";
        }
    }
    public class ChannelData
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }
    }
}
