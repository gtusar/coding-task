using System;
using System.Collections.Generic;
using System.Linq;
using CodingTask.Models;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace CodingTask
{
    /// <summary>
    /// Orderbook persistance class
    /// </summary>
    public static class OrderbookLog
    {
        static MongoClient dbClient = new MongoClient("mongodb://localhost:27017/");
        static string databaseName = "codingTask1";
        static string collectionName = "orderbookLog1";
        static IMongoDatabase database = dbClient.GetDatabase(databaseName);
        static IMongoCollection<Models.BitstampWSResponse> collection = database.GetCollection<Models.BitstampWSResponse>(collectionName);
        
        /// <summary>
        /// Save orderbook data to database
        /// </summary>
        /// <param name="rawOrderbook">Orderbook to save</param>
        public static void InsetOrderbook(CodingTask.Models.BitstampWSResponse rawOrderbook)
        {
            collection.InsertOne(rawOrderbook);
        }

        /// <summary>
        /// Get last 500 saved orderbooks with timestamps and the lowest ask price
        /// </summary>
        /// <returns>List of timestamps and ask prices pairs</returns>
        public static List<AuditLogTimestamp> GetAvailableTimestamps()
        {
            return collection.AsQueryable().Select(doc => new AuditLogTimestamp
            {
                Timestamp = doc.Data.Timestamp,
                Price = doc.Data.Asks.First().ElementAt(0)
            })
                .ToList()
                .TakeLast(500)
                .GroupBy(g => g.Timestamp)
                .Select(g => new AuditLogTimestamp
                {
                    Timestamp = g.Key,
                    Price = g.Average(p => p.Price)
                }).ToList();
        }

        /// <summary>
        /// Get orderbook for selected timestamp
        /// </summary>
        /// <param name="timestamp">Selected timestamp</param>
        /// <returns>Orderbook data</returns>
        public static OrderBookDataModel GetDataAtTimestamps(DateTime timestamp)
        {
            BitstampWSResponse searchResult = collection.AsQueryable().Where(doc => doc.Data.Timestamp == timestamp).FirstOrDefault();
            return new OrderBookDataModel(searchResult);
        }
    }
}
