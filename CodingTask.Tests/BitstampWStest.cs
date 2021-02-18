using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodingTask;

namespace CodingTask.Tests
{
    [TestClass]
    public class BitstampWStest
    {
        [TestMethod]
        public async void TestMethod1()
        {
            var client = new BitstampWS();
            await client.ConnectAsync();

            client.ResponseReceived += (sender, eventArguments) => {
                Assert.Equals(eventArguments.Event, "data");
                Assert.IsNotNull(eventArguments.Data);
                Assert.Equals(eventArguments.Data.Bids.Count, 100);
                Assert.Equals(eventArguments.Data.Asks.Count, 100);
            };

            await client.Subscribe("btceur");
            await client.Unsubscribe();
            await client.DisconnectAsync();
            
        }
    }
}
