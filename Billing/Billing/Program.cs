using Grpc.Net.Client;
using System;

namespace Billing
{
    class Program
    {
        static void Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Billing.BillingClient(channel);
            Console.ReadLine();
        }
    }
}
