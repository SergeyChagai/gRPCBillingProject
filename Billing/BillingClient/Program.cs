using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Billing
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1:30052", ChannelCredentials.Insecure);
            var client = new BillingClient(new Billing.BillingClient(channel));
            Console.WriteLine("Start client\n");

            // Tasks schedule
            client.ListUsers().Wait();
            client.CoinsEmission(1000).Wait();
            client.MoveCoins("boris", "maria", 1).Wait();
            client.LongestHistoryCoin().Wait();
            channel.ShutdownAsync().Wait();
            
            Console.WriteLine("Press any key for exit");
            Console.ReadKey();
        }
    }
}
