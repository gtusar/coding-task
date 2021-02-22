using System;
using System.Collections.Generic;
using System.Linq;
using CodingTask.Models;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace CodingTask
{
    public static class OrderbookLog
    {
        static MongoClient dbClient = new MongoClient("mongodb://localhost:27017/");
        static string databaseName = "codingTask1";
        static string collectionName = "orderbookLog1";
        static IMongoDatabase database = dbClient.GetDatabase(databaseName);
        static IMongoCollection<Models.BitstampWSResponse> collection = database.GetCollection<Models.BitstampWSResponse>(collectionName);
        public static void InsetOrderbook(CodingTask.Models.BitstampWSResponse rawOrderbook)
        {
            collection.InsertOne(rawOrderbook);
        }

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
    }
}
