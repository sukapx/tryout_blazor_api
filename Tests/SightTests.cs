using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using tryout_blazor_api.Shared;
using Xunit;

namespace Tests.IntegrationTests
{
    public class EmployeesControllerIntegrationTests : IClassFixture<TestingWebAppFactory<Program>>
    {
        private readonly HttpClient _client;
        public EmployeesControllerIntegrationTests(TestingWebAppFactory<Program> factory) 
            => _client = factory.CreateClient();

        [Fact]
        public async Task Sights()
        {
            // Arrange
            // Act
            var response = await _client.GetAsync("/Sight");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var sights = JsonSerializer.Deserialize<List<Sight>>(responseString);

            // Assert
            Assert.Equal(sights.Count, 2);
        }

        [Theory]
        [InlineData(1, 52.514444F, 13.35F, "guessed it")]
        [InlineData(1, 52.514444F, 13.05F, "Wrong guess")]
        [InlineData(1, 52.414444F, 13.35F, "Wrong guess")]

        public async Task Sight_Distance(int sightId, float lat, float lon, string result)
        {
            // Arrange
            Location loc = new() { Latitude = lat, Longitude = lon };

            // Act
            var response = await _client.PostAsJsonAsync($"/Sight/check/{sightId}", loc);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            
            // Assert
            Assert.Contains(result, responseString);
        }
    }
}