using Grpc.Core;
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
            var result = await _client.CoinsEmissionAsync(new EmissionAmount { Amount = coins });
            Log(result.ToString());
        }

        public async Task MoveCoins(string source, string destination, long coins)
        {
            var result = await _client.MoveCoinsAsync(new MoveCoinsTransaction { SrcUser = source, DstUser = destination, Amount = coins });
            Log(result.ToString());
        }

        public async Task LongestHistoryCoin()
        {
            var result = await _client.LongestHistoryCoinAsync(new None());
            Log(result.ToString());
        }

        private void Log(string s)
        {
            Console.WriteLine(s);
        }
    }
}
