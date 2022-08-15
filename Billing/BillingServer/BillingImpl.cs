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
        public List<Coin> Coins { get; private set; }

        private const char Separator = ';';

        private readonly object usersLock = new object();
        public BillingImpl(List<JsonUserProfile> jsonUsers)
        {
            Users = new List<JsonUserProfile>(jsonUsers);
            Coins = new List<Coin>();
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

        public override Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
        {
            var amount = request.Amount;

            if (amount < Users.Count)
            {
                return Task.FromResult(new Response
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Coins amount is less of users"
                });
            }
            var response = Task.Run(() => DistributeAmount(amount));
            return response;
        }

        public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                lock (usersLock)
                {
                    var source = (Users.Find(i => i.name == request.SrcUser));
                    var destination = (Users.Find(i => i.name == request.DstUser));
                    var amount = request.Amount;

                    if (source != null && destination != null && source.amount >= amount)
                    {
                        var sourceCoins = Coins.FindAll(c =>
                        {
                            var owners = c.History.Split(Separator);
                            return owners.Last() == source.name;
                        });
                        ChangeOwnerOfCoins(sourceCoins, destination);
                        source.amount -= amount;
                        destination.amount += amount;
                        return new Response { Status = Response.Types.Status.Ok };
                    }
                    return new Response { Status = Response.Types.Status.Failed };
                }
            });
        }

        public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var longestHistoryCoin = Coins.Aggregate((x, y) => x.History.Count(c => c == ';') < y.History.Count(c => c == ';') ? y : x);
                return longestHistoryCoin;
            });
        }

        private void ChangeOwnerOfCoins(List<Coin> sourceCoins, JsonUserProfile destination)
        {
            foreach (Coin coin in sourceCoins)
            {
                coin.History = String.Join(Separator, coin.History, destination.name);
            }
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
                    CreateCoins(1, jsonUser.name);
                }

                for (int i = 0; i < Users.Count; i++)
                {
                    var jsonUser = Users[i];
                    var coinsForUser = amount * jsonUser.rating / summaryRating;
                    CreateCoins(coinsForUser, jsonUser.name);
                    remainders.Add(i, jsonUser.rating);
                    amount -= coinsForUser;
                    jsonUser.amount += coinsForUser;
                }
                if (amount != 0)
                {
                    var idMax = remainders.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                    CreateCoins(amount, Users[idMax].name);
                    Users[idMax].amount += amount;
                }
            }
            return new Response { Status = Response.Types.Status.Ok };
        }

        private void CreateCoins(long amount, string owner)
        {
            var lastUser = Coins.LastOrDefault();
            var lastIndex = lastUser == null ? 0 : lastUser.Id;
            for (int i = 0; i < amount; i++)
            {
                Coins.Add(new Coin
                {
                    Id = lastIndex + i + 1,
                    History = owner
                });
            }
        }
    }
}
