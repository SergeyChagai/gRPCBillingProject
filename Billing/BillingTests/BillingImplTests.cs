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
        public async Task CoinsEmissionTest(long amount, List<UserProfile> expected)
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

            //Assert
            Assert.True(response.Status == Response.Types.Status.Ok);
            Assert.Equal(expected, actual);
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
            yield return new object[] { 3, users };

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
            yield return new object[] { 6, users };

        }
    }
}
