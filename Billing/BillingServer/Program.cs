using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Billing
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 5001;

            var users = BillingUtil.LoadUsers();

            Server server = new Server
            {
                Services = { Billing.BindService(new BillingImpl(users)) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Billing server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
