using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using tryout_blazor_api.Shared;
using tryout_blazor_api.Shared.Auth;
using Xunit;

namespace Tests.IntegrationTests
{
    public class AuthTests : IClassFixture<TestingWebAppFactory<Program>>
    {
        private readonly HttpClient _client;
        public AuthTests(TestingWebAppFactory<Program> factory) 
            => _client = factory.CreateClient();



        [Fact]
        public async Task Unauthorized_Access_is_prevented()
        {
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () =>
            {
                var responseAuthorized = await _client.GetAsync("WeatherForecast");
                responseAuthorized.EnsureSuccessStatusCode();
            });
        }

        [Fact]
        public async Task Register()
        {
            // Arrange
            RegisterModel regData = new() {
                Username = "Testuser1",
                Email = "Testuser1@ad.de",
                Password = "$Testuser1"
            };

            // Act
            var response = await _client.PostAsJsonAsync("Auth/register-admin", regData);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resultRegister = JsonSerializer.Deserialize<Response>(responseString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            
            // Assert
            Assert.Contains("Success", resultRegister!.Status);

            // Arrange
            LoginModel loginData = new LoginModel() {
                Username = "Testuser1",
                Password = "$Testuser1"
            };
            var expiryMin = DateTime.Now + TimeSpan.FromHours(1);
            var expiryMax = DateTime.Now + TimeSpan.FromHours(6);

            // Act
            var responseLogin = await _client.PostAsJsonAsync("Auth/login", loginData);
            responseLogin.EnsureSuccessStatusCode();
            responseString = await responseLogin.Content.ReadAsStringAsync();
            var resultLogin = JsonSerializer.Deserialize<LoginToken>(responseString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.NotEqual("", resultLogin!.Token);
            Assert.Equal(1, resultLogin!.Expiration.CompareTo(expiryMin));
            Assert.Equal(-1, resultLogin!.Expiration.CompareTo(expiryMax));

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resultLogin!.Token);
            var responseAuthorized = await _client.GetAsync("WeatherForecast");
            responseAuthorized.EnsureSuccessStatusCode();
        }
    }
}