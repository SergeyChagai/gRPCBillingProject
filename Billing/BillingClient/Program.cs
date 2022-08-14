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

            // Tasks schedule
            client.ListUsers().Wait();

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Start client\n" + "Press any key for exit");
            Console.ReadKey();
        }
    }
}
