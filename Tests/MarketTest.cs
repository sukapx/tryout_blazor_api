using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using tryout_blazor_api.Shared;
using tryout_blazor_api.Shared.Map;
using tryout_blazor_api.Shared.Play;
using Xunit;

namespace Tests.IntegrationTests
{
    public class MarketTest : IClassFixture<TestingWebAppFactory<Program>>
    {
        private readonly HttpClient _client;
        public MarketTest(TestingWebAppFactory<Program> factory) 
            => _client = factory.CreateClient();

        [Fact]
        public async Task Markets()
        {
            // Arrange
            // Act
            var markets = await _client.GetFromJsonAsync<List<Market>>("/Market");

            // Assert
            Assert.NotNull(markets);
            Assert.True(markets!.Count >= 2);
        }

        [Theory]
        [InlineData(0, 0, 0, "to far")]
        [InlineData(1, 0, 0, "to far")]
        [InlineData(1, 52.5374F, 13.2037F, "near")]
        [InlineData(1, 52.5447F, 13.1675F, "to far")]
        [InlineData(1, 52.414444F, 13.35F, "to far")]
        [InlineData(0, 52.5374F, 13.2037F, "to far")]
        [InlineData(0, 52.5447F, 13.1675F, "near")]

        public async Task Market_Distance(int sightId, float lat, float lon, string result)
        {
            // Arrange
            Location loc = new() { Latitude = lat, Longitude = lon };

            // Act
            var response = await _client.PostAsJsonAsync($"/Market/check/{sightId}", loc);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            
            // Assert
            Assert.Contains(result, responseString);
        }
    }
}