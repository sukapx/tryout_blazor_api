using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Tests.IntegrationTests
{
    public class AuthTests : AuthTestBase
    {
        [Fact]
        public async Task Unauthorized_Access_is_prevented()
        {
            // Arrange
            var client = _factory.CreateClient();
            
            // Act
            // Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () =>
            {
                var responseAuthorized = await client.GetAsync("WeatherForecast");
                responseAuthorized.EnsureSuccessStatusCode();
            });
        }

        [Fact]
        public async Task Authorized_Access_is_permitted()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            string bearerToken = await LoginAsUser(client);
            var responseAuthorized = await client.GetAsync("WeatherForecast");
            
            // Assert
            responseAuthorized.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Register()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var resultReg = await RegisterAs(client, "User2", "P4$$w0rd", "User2@example.net");
            var resultLogin = await LoginAs(client, "User2", "P4$$w0rd");

            // Assert
            Assert.NotNull(resultReg);
            Assert.Contains("Success", resultReg!.Status);
            Assert.NotEqual("", resultLogin);
        }

        [Fact]
        public async Task RegisterShortPassword()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(async () =>
            {
                var resultReg = await RegisterAs(client, "User3", "pass", "User3@example.net");
            });

            // Assert
            Assert.NotEqual("", exception.Message);
        }

        [Fact]
        public async Task Token_duration()
        {
            // Arrange
            var client = _factory.CreateClient();
            var expiryMin = DateTime.Now + TimeSpan.FromHours(1);
            var expiryMax = DateTime.Now + TimeSpan.FromHours(6);

            // Act
            string bearerToken = await LoginAsUser(client);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(bearerToken) as JwtSecurityToken;

            // Assert
            Assert.NotEqual("", bearerToken);
            Assert.NotNull(token);
            Assert.Equal(1, token!.ValidTo.CompareTo(expiryMin));
            Assert.Equal(-1, token!.ValidTo.CompareTo(expiryMax));
        }
    }
}