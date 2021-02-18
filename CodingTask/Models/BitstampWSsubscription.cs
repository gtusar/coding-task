using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Models
{
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
            this.Event = "bts:subscribe";
            this.Data.Channel = $"order_book_{tradingPair}";
        }
    }
    public class Unsubscribe: Subscription
    {
        public Unsubscribe(string tradingPair)
        {
            this.Event = "bts:unsubscribe";
            this.Data.Channel = $"order_book_{tradingPair}";
        }
    }
    public class ChannelData
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }
    }
}
