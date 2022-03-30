using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using tryout_blazor_api.Shared.Auth;
using Xunit;
using Xunit.Abstractions;

namespace Tests.IntegrationTests
{
    public class AuthTests : AuthTestBase
    {

        public AuthTests(ITestOutputHelper output) : base(output) { }


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
            var bearerToken = await LoginAsUser(client);
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
            Assert.NotEqual("", resultLogin.Token);
        }

        [Fact]
        public async Task RefreshAuthToken()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var resultLogin = await LoginAsUser(client);
            var resultRefresh = await client.PostAsJsonAsync("Auth/Refresh", new Refresh { TokenRefresh = resultLogin!.TokenRefresh });
            var resultRefreshString = await resultRefresh.Content.ReadAsStringAsync();

            // Assert
            Assert.NotEqual("", resultLogin.Token);
        }

        [Fact]
        public async Task RegisterShortPassword()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                var resultReg = await RegisterAs(client, "User3", "pass", "User3@example.net");
            });

            // Assert
            Assert.Contains("Passwords must be at least", exception.Message);
        }

        [Fact]
        public async Task RegisterNoPassword()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                var resultReg = await RegisterAs(client, "User4", "", "User4@example.net");
            });

            // Assert
            Assert.Contains("Password is required", exception.Message);
        }

        [Fact]
        public async Task RegisterExistingUser()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await RegisterAs(client, "User5", "P4$$w0rd", "User5@example.net");
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                var resultReg = await RegisterAs(client, "User5", "P4$$w0rd", "User5@example.net");
            });

            // Assert
            Assert.Contains("User already exists", exception.Message);
        }

        [Fact]
        public async Task Token_validity()
        {
            // Arrange
            var client = _factory.CreateClient();
            var expiryMin = DateTime.UtcNow + TimeSpan.FromMinutes(5);
            var expiryMax = DateTime.UtcNow + TimeSpan.FromMinutes(15);

            // Act
            var bearerToken = await LoginAsUser(client);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(bearerToken.Token) as JwtSecurityToken;

            // Assert
            Assert.NotEqual("", bearerToken.Token);
            Assert.NotNull(token);
            Assert.Equal(1, token!.ValidTo.CompareTo(expiryMin));
            Assert.Equal(-1, token!.ValidTo.CompareTo(expiryMax));
        }

        [Fact]
        public async Task Token_refresh_validity()
        {
            // Arrange
            var client = _factory.CreateClient();
            var expiryMin = DateTime.UtcNow + TimeSpan.FromHours(12);
            var expiryMax = DateTime.UtcNow + TimeSpan.FromHours(25);

            // Act
            var bearerToken = await LoginAsUser(client);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(bearerToken.TokenRefresh) as JwtSecurityToken;

            // Assert
            Assert.NotEqual("", bearerToken.TokenRefresh);
            Assert.NotNull(token);
            Assert.Equal(1, token!.ValidTo.CompareTo(expiryMin));
            Assert.Equal(-1, token!.ValidTo.CompareTo(expiryMax));
        }
    }
}