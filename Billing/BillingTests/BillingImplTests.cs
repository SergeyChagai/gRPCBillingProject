using System;
using Xunit;
using Billing;
using Grpc.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BillingTests
{
    public class BillingImplTests
    {
        private BillingImpl sut;

        [Theory]
        [MemberData(nameof(CoinsEmissionTestData))]
        public async Task CoinsEmissionTest(long amount, List<UserProfile> expected, List<Coin> expectedCoins)
        {
            //Arrange
            sut = new BillingImpl(BillingUtil.LoadUsers());
            //Act
            var response = await sut.CoinsEmission(new EmissionAmount { Amount = amount }, null);
            var actual = new List<UserProfile>();
            foreach (JsonUserProfile jsonUser in sut.Users)
            {
                actual.Add(new UserProfile { Name = jsonUser.name, Amount = jsonUser.amount });
            }
            var actualCoins = sut.Coins;

            //Assert
            Assert.True(response.Status == Response.Types.Status.Ok);
            Assert.Equal(expected, actual);
            Assert.Equal(expectedCoins.Count, actualCoins.Count);
            for (int i = 0; i < expectedCoins.Count; i++)
            {
                Assert.Contains(actualCoins, c => c.History == expectedCoins[i].History);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task CoinsEmissionNegativeTest(long amount)
        {
            //Arrange
            sut = new BillingImpl(BillingUtil.LoadUsers());

            //Act
            var response = await sut.CoinsEmission(new EmissionAmount { Amount = amount }, null);

            //Assert
            Assert.True(response.Status == Response.Types.Status.Failed);
        }

        [Theory]
        [MemberData(nameof(MoveCoinsTestData))]
        public async Task MoveCoinsTest(MoveCoinsTransaction request, List<UserProfile> expected)
        {
            //Arrange
            sut = new BillingImpl(BillingUtil.LoadUsers());
            await sut.CoinsEmission(new EmissionAmount { Amount = 1000 }, null);

            //Act
            var response = await sut.MoveCoins(request, null);
            var actual = new List<UserProfile>();
            foreach (JsonUserProfile jsonUser in sut.Users)
            {
                actual.Add(new UserProfile { Name = jsonUser.name, Amount = jsonUser.amount });
            }

            //Assert
            Assert.Equal(Response.Types.Status.Ok, response.Status);
            Assert.Equal(expected, actual);

        }

        [Theory]
        [MemberData(nameof(LongestHistoryCoinTestData))]
        public async Task LongestHistoryCoinTest(MoveCoinsTransaction[] transactions, Coin expected)
        {
            //Arrange
            sut = new BillingImpl(BillingUtil.LoadUsers());
            await sut.CoinsEmission(new EmissionAmount { Amount = 1000 }, null);
            foreach (MoveCoinsTransaction transaction in transactions)
            {
                await sut.MoveCoins(transaction, null);
            }

            //Act
            var actual = await sut.LongestHistoryCoin(new None(), null);

            //Assert
            Assert.Equal(expected, actual);

        }

        public static IEnumerable<object[]> LongestHistoryCoinTestData()
        {
            yield return new object[]
            {
                new MoveCoinsTransaction[]
                {
                    new MoveCoinsTransaction { SrcUser = "boris", DstUser = "maria", Amount = 1 },
                    new MoveCoinsTransaction { SrcUser = "boris", DstUser = "oleg", Amount = 1 },
                    new MoveCoinsTransaction { SrcUser = "maria", DstUser = "oleg", Amount = 1 },
                },
                new Coin{ Id = 1, History = "boris;maria;oleg" }
            };
        }
        public static IEnumerable<object[]> MoveCoinsTestData()
        {
            yield return new object[]
            {
                new MoveCoinsTransaction { SrcUser = "boris", DstUser = "maria", Amount = 100 },
                new List<UserProfile>
                {
                    new UserProfile
                    {
                        Name = "boris",
                        Amount = 834
                    },
                     new UserProfile
                    {
                        Name = "maria",
                        Amount = 139
                    },
                      new UserProfile
                    {
                        Name = "oleg",
                        Amount = 27
                    }
            }
        };
            yield return new object[]
            {
            new MoveCoinsTransaction { SrcUser = "boris", DstUser = "oleg", Amount = 50 },
             new List<UserProfile>
            {
                new UserProfile
                {
                    Name = "boris",
                    Amount = 884
                },
                 new UserProfile
                {
                    Name = "maria",
                    Amount = 39
                },
                  new UserProfile
                {
                    Name = "oleg",
                    Amount = 77
                }
            }
        };
        }

        public static IEnumerable<object[]> CoinsEmissionTestData()
        {
            var users = new List<UserProfile>
            {
                new UserProfile
                {
                    Name = "boris",
                    Amount = 1
                },
                 new UserProfile
                {
                    Name = "maria",
                    Amount = 1
                },
                  new UserProfile
                {
                    Name = "oleg",
                    Amount = 1
                }
            };
            var coins = new List<Coin>
            {
                new Coin{Id = 1, History = "boris"},
                new Coin{Id = 2, History = "maria"},
                new Coin{Id = 3, History = "oleg"},

            };
            yield return new object[] { 3, users, coins };

            users = new List<UserProfile>
            {
                new UserProfile
                {
                    Name = "boris",
                    Amount = 4
                },
                 new UserProfile
                {
                    Name = "maria",
                    Amount = 1
                },
                  new UserProfile
                {
                    Name = "oleg",
                    Amount = 1
                }
            };
            coins = new List<Coin>
            {
                new Coin{Id = 1, History = "boris"},
                new Coin{Id = 2, History = "boris"},
                new Coin{Id = 3, History = "boris"},
                new Coin{Id = 4, History = "boris"},
                new Coin{Id = 5, History = "maria"},
                new Coin{Id = 6, History = "oleg"},

            };
            yield return new object[] { 6, users, coins };

        }
    }
}
