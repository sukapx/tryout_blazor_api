using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using tryout_blazor_api.Shared.Auth;
using tryout_blazor_api.Shared.Play;
using Xunit;
using Xunit.Abstractions;

namespace Tests.IntegrationTests
{
    public class PlayerTests : AuthTestBase
    {

        public PlayerTests(ITestOutputHelper output) : base(output) { }


        [Fact]
        public async Task GetInfo()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await LoginAsUser(client);
            var playerResponse = await client.GetAsync("Player");

            // Assert
            playerResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task TransferToMarket()
        {
            // Arrange
            const int TRANSFER_AMOUNT = 3;
            const int MARKET_ID = 0;

            var client = _factory.CreateClient();
            await LoginAsUser(client);

            var marketInitial = await client.GetFromJsonAsync<Market>($"Market/{MARKET_ID}");
            await client.PostAsJsonAsync($"Player/SetLocation", marketInitial.Location);
            var playerInitial = await client.GetFromJsonAsync<Player>("Player");

            var ironOnShipOnInit = playerInitial!.Cargo.GetCargo(MarketItemType.Iron);
            var ironOnMarketOnInit = marketInitial!.Cargo.GetCargo(MarketItemType.Iron);
            Assert.True(ironOnMarketOnInit >= TRANSFER_AMOUNT);


            // Act
            var transferResponse = await client.GetAsync($"Player/transfer/{MARKET_ID}/{(int)MarketItemType.Iron}/{TRANSFER_AMOUNT}");


            // Assert
            var player = await client.GetFromJsonAsync<Player>("Player");
            var market = await client.GetFromJsonAsync<Market>($"Market/{MARKET_ID}");

            var ironOnShip = player!.Cargo.GetCargo(MarketItemType.Iron);
            var ironOnMarket = market!.Cargo.GetCargo(MarketItemType.Iron);

            Assert.Equal(ironOnShip, ironOnShipOnInit + TRANSFER_AMOUNT);
            Assert.Equal(ironOnMarket, ironOnMarketOnInit - TRANSFER_AMOUNT);
        }
    }
}
