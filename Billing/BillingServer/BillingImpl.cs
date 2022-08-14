using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;

namespace Billing
{
    public class BillingImpl : Billing.BillingBase
    {
        public List<JsonUserProfile> Users { get; private set; }
        private readonly object usersLock = new object();
        public BillingImpl(List<JsonUserProfile> jsonUsers)
        {
            Users = new List<JsonUserProfile>(jsonUsers);
        }
        public override async Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream, ServerCallContext context)
        {
            foreach (JsonUserProfile user in Users)
            {
                var reply = new UserProfile
                {
                    Name = user.name,
                    Amount = user.amount
                };

                await responseStream.WriteAsync(reply);
            }
        }

        public override async Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
        {
            var amount = request.Amount;

            if (amount < Users.Count)
            {
                return new Response
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Coins amount is less of users"
                };
            }
            var response = await Task.Run(() => DistributeAmount(amount));
            return response;
        }

        public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
        {
            return base.MoveCoins(request, context);
        }

        public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
        {
            return base.LongestHistoryCoin(request, context);
        }

        private Response DistributeAmount(long amount)
        {
            lock (usersLock)
            {
                var ratings = Users.Select(user => user.rating).ToArray();
                var summaryRating = ratings.Sum();
                var remainders = new Dictionary<int, long>();

                foreach (JsonUserProfile jsonUser in Users)
                {
                    amount -= 1;
                    jsonUser.amount += 1;
                }

                for (int i = 0; i < Users.Count; i++)
                {
                    var jsonUser = Users[i];
                    var coinsForUser = amount * jsonUser.rating / summaryRating;
                    remainders.Add(i, jsonUser.rating);
                    amount -= coinsForUser;
                    jsonUser.amount += coinsForUser;
                }
                if (amount != 0)
                {
                    var idMax = remainders.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                    Users[idMax].amount += amount;
                }
            }
            return new Response { Status = Response.Types.Status.Ok };
        }
    }
}
