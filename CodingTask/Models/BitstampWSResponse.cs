using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Models
{
    public class BitstampWSResponse
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
        [JsonProperty("channel")]
        public string Channel { get; set; }
        [JsonProperty("event")]
        public string Event { get; set; }
    }

    public class Data
    {
        public DateTime Timestamp { get
            {
                // Convert from UNIX timestamp
                DateTime _timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                _timestamp = _timestamp.AddSeconds(timestamp).ToLocalTime();
                return _timestamp;
            } 
        }
        [JsonProperty("timestamp")]
        private long timestamp { get; set; }
        [JsonProperty("microtimestamp")]
        private long Microtimestamp { get; set; }
        [JsonProperty("bids")]
        public List<List<decimal>> Bids { get; set; }
        [JsonProperty("asks")]
        public List<List<decimal>> Asks { get; set; }
    }
}
