using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;

namespace Billing
{
    public class BillingImpl : Billing.BillingBase
    {
        private List<JsonUserProfile> _users;
        public BillingImpl(List<JsonUserProfile> jsonUsers)
        {
            _users = new List<JsonUserProfile>(jsonUsers);
        }
        public override async Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream, ServerCallContext context)
        {
            foreach (JsonUserProfile user in _users)
            {
                var reply = new UserProfile
                {
                    Name = user.name,
                    Amount = user.amount
                };

                await responseStream.WriteAsync(reply);
            }
        }

        public override Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
        {
            return base.CoinsEmission(request, context);
        }

        public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
        {
            return base.MoveCoins(request, context);
        }

        public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
        {
            return base.LongestHistoryCoin(request, context);
        }


    }
}
