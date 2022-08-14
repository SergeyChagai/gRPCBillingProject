﻿using Grpc.Core;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Billing
{
    public class BillingClient
    {
        private Billing.BillingClient _client;

        public BillingClient(Billing.BillingClient client)
        {
            _client = client;
        }

        public async Task ListUsers()
        {
            using var call = _client.ListUsers(new None());
            var responseLog = new StringBuilder("Result: ");

            while (await call.ResponseStream.MoveNext())
            {
                responseLog.AppendLine(call.ResponseStream.Current.ToString());
            }
            Log(responseLog.ToString());
        }

        public async Task CoinsEmission(long coins)
        {
            await _client.CoinsEmissionAsync(new EmissionAmount { Amount = coins });
        }

        private void Log(string s)
        {
            Console.WriteLine(s);
        }
    }
}
