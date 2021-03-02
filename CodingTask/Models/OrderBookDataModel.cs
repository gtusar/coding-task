using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Models
{
    public class OrderBookDataModel
    {
        /// <summary>
        /// Prepare raw data ready for display in web UI
        /// 
        /// Creates order book table with 10 rows
        /// Groups (classifies) data for orderbook and market depth charts
        /// </summary>
        /// <param name="response"></param>
        public OrderBookDataModel(BitstampWSResponse response)
        {
            if (response == null) return;
            Timestamp = response.Data.Timestamp;
            MarketDepthChartData = new List<PriceVolume>();
            OrderBookChartData = new List<PriceVolume>();
            OrderBookData = new List<PriceVolume>();

            if (response.Event == "data")
            {
                OrderBookData.AddRange(response.Data.Bids
                    .GroupBy(o => o.ElementAt(0))
                    .OrderBy(g => g.Key)
                    .Select(o => new PriceVolume { 
                        Price = o.Key.ToString(), 
                        Bids = o.Select(ord => ord.ElementAt(1)).Sum(), 
                        Asks = 0 })
                    .TakeLast(10));

                OrderBookData.AddRange(response.Data.Asks
                    .GroupBy(o => o.ElementAt(0))
                    .OrderBy(g => g.Key)
                    .Select(o => new PriceVolume { 
                        Price = o.Key.ToString(), 
                        Bids = 0, 
                        Asks = o.Select(ord => ord.ElementAt(1)).Sum() })
                    .Take(10));

                OrderBookChartData.AddRange(ClassifyOrders(response.Data.Bids, OrderType.Bid));
                OrderBookChartData.AddRange(ClassifyOrders(response.Data.Asks, OrderType.Ask));

                MarketDepthChartData.AddRange(ClassifyOrders(response.Data.Bids, OrderType.Bid, true));
                MarketDepthChartData.AddRange(ClassifyOrders(response.Data.Asks, OrderType.Ask, true));
            }
        }

        /// <summary>
        /// Groups data to defined number of classes and prepares model for client UI charts
        /// </summary>
        /// <param name="orders">List of orders</param>
        /// <param name="type">Bids or Asks</param>
        /// <param name="marketDepth">'Integrate' orders for market depth chart</param>
        /// <returns></returns>
        private List<PriceVolume> ClassifyOrders(List<List<decimal>> orders, OrderType type, bool marketDepth = false)
        {
            List<PriceVolume> result = new List<PriceVolume>();
            int numberOfBars = 30;
            // Calculate price ranges and grouping increment
            decimal minPrice = orders.OrderBy(o => o[0]).FirstOrDefault().FirstOrDefault();
            decimal maxPrice = orders.OrderByDescending(o => o[0]).FirstOrDefault().FirstOrDefault();
            decimal priceRange = maxPrice - minPrice;
            decimal priceIncrement = priceRange / numberOfBars;
            decimal totalOrdersSum = orders.Select(b => b.ElementAt(1)).Sum();
            decimal loopSumAccumulator = 0;

            for (int i = 0; i < numberOfBars; i++)
            {
                // Current group boundaries
                decimal minRange = minPrice + (priceIncrement * i);
                decimal maxRange = minPrice + (priceIncrement * (i + 1));

                // Select orders within current price boundaries
                IEnumerable<List<decimal>> ordersInRange = orders.Where(b =>
                    {
                        decimal _price = b.FirstOrDefault();
                        return _price >= minRange && _price < maxRange;
                    }
                );

                // Sum orders in current price range
                decimal rangeOrdersSum = ordersInRange.Select(b => b.ElementAt(1)).Sum();
                loopSumAccumulator += rangeOrdersSum;

                decimal priceVolumeValue = rangeOrdersSum;
                
                // If marketDepth override value with appropriate value for each type
                if (marketDepth)
                {
                    switch (type)
                    {
                        case OrderType.Bid:
                            {
                                priceVolumeValue = totalOrdersSum - loopSumAccumulator;
                                break;
                            }
                        case OrderType.Ask:
                            {
                                priceVolumeValue = loopSumAccumulator;
                                break;
                            }
                    }
                }

                result.Add(new PriceVolume
                {
                    Price = string.Format("{0:C}", maxRange),
                    Bids = type == OrderType.Bid ? priceVolumeValue : 0,
                    Asks = type == OrderType.Ask ? priceVolumeValue : 0,
                });
            }
            return result;
        }

        public DateTime Timestamp { get; set; }
        public List<PriceVolume> OrderBookData {get;set;}
        public List<PriceVolume> MarketDepthChartData { get; set; }
        public List<PriceVolume> OrderBookChartData { get; set; }
        private enum OrderType
        {
            Bid,
            Ask
        }
    }
    public class PriceVolume
    {
        public string Price { get; set; }
        public decimal Bids { get; set; }
        public decimal Asks { get; set; }
    }
}
