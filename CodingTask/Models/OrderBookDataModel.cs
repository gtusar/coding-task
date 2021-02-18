using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Models
{
    public class OrderBookDataModel
    {
        public OrderBookDataModel(BitstampWSResponse response)
        {
            if (response == null) return;
            this.Timestamp = response.Data.Timestamp;
            this.MarketDepthChartData = new List<PriceVolume>();
            this.OrderBookChartData = new List<PriceVolume>();
            this.OrderBookData = new List<PriceVolume>();

            if (response.Event == "data")
            {
                OrderBookData.AddRange(response.Data.Bids.GroupBy(o => o.ElementAt(0)).Select(o => new PriceVolume { Price = o.Key.ToString(), Bids = o.Select(ord => ord.ElementAt(1)).Sum(), Asks= 0 }).TakeLast(10));
                OrderBookData.AddRange(response.Data.Asks.GroupBy(o => o.ElementAt(0)).Select(o => new PriceVolume { Price = o.Key.ToString(), Bids = 0, Asks = o.Select(ord => ord.ElementAt(1)).Sum() }).Take(10));

                OrderBookChartData.AddRange(ClassifyOrders(response.Data.Bids, OrderType.Bid));
                OrderBookChartData.AddRange(ClassifyOrders(response.Data.Asks, OrderType.Ask));

                MarketDepthChartData.AddRange(ClassifyOrders(response.Data.Bids, OrderType.Bid, true));
                MarketDepthChartData.AddRange(ClassifyOrders(response.Data.Asks, OrderType.Ask, true));
            }
        }

        private List<PriceVolume> ClassifyOrders(List<List<decimal>> orders, OrderType type, bool marketDepth = false)
        {
            List<PriceVolume> result = new List<PriceVolume>();
            int numberOfBars = 10;
            decimal minPrice = orders.OrderBy(o => o[0]).FirstOrDefault().FirstOrDefault();
            decimal maxPrice = orders.OrderByDescending(o => o[0]).FirstOrDefault().FirstOrDefault();
            decimal priceRange = maxPrice - minPrice;
            decimal priceIncrement = priceRange / numberOfBars;
            decimal totalOrdersSum = orders.Select(b => b.ElementAt(1)).Sum();
            decimal loopSumAccumulator = 0;

            for (int i = 0; i < numberOfBars; i++)
            {
                decimal minRange = minPrice + (priceIncrement * i);
                decimal maxRange = minPrice + (priceIncrement * (i + 1));

                // select orders in this range
                var ordersInRange = orders.Where(b =>
                {
                    decimal _price = b.FirstOrDefault();
                    return _price >= minRange && _price < maxRange;
                }
                );

                // range orders sum
                decimal rangeOrdersSum = ordersInRange.Select(b => b.ElementAt(1)).Sum();
                loopSumAccumulator += rangeOrdersSum;

                decimal priceVolumeValue = rangeOrdersSum;
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
