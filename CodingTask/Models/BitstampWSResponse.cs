using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Models
{
    /// <summary>
    /// Classes to comply with Bitstamp WebSocket described here: https://www.bitstamp.net/websocket/v2/
    /// JSON response from Web Socket will be parsed to object of type BitstampWSResponse 
    /// </summary>
    [BsonIgnoreExtraElements]
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
        /// <summary>
        /// Convert to DateTime from UNIX timestamp of kind UTC
        /// </summary>
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Timestamp
        {
            get
            {
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
