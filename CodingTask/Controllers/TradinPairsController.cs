using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodingTask.Models;

namespace CodingTask.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TradingPairsController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<CurrencyPair> Get()
        {
            List<CurrencyPair> availableCurrencyPairs = new List<CurrencyPair>
            {
                new CurrencyPair{ Title = "BTC / EUR", ApiName = "btceur"},
                new CurrencyPair{ Title = "BTC / USD", ApiName = "btcusd"},
            };
            return availableCurrencyPairs;
        }
    }
}