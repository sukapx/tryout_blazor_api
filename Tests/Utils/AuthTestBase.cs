using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using tryout_blazor_api.Shared;
using tryout_blazor_api.Shared.Auth;
using Xunit;

namespace Tests.IntegrationTests
{
    public class AuthTestBase : IAsyncLifetime
    {
        protected readonly TestingWebAppFactory<Program> _factory;
        private static bool Initialized = false;

        public AuthTestBase()
        {
            _factory = new TestingWebAppFactory<Program>();
        }

        public async Task InitializeAsync()
        {
            if(Initialized)
                return;

            using (var client = _factory.CreateClient())
            {
                await RegisterAsUser(client);
            }

            Initialized = true;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }



        protected async Task<string> LoginAsUser(HttpClient client)
        {
            return await LoginAs(client, "user", "P4$$w0rd");
        }

        protected async Task<string> LoginAs(HttpClient client, string user, string pass)
        {
            LoginModel loginData = new () {
                Username = user,
                Password = pass
            };

            var responseLogin = await client.PostAsJsonAsync("Auth/login", loginData);
            responseLogin.EnsureSuccessStatusCode();
            var responseString = await responseLogin.Content.ReadAsStringAsync();
            var resultLogin = JsonSerializer.Deserialize<LoginToken>(responseString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resultLogin!.Token);

            return resultLogin!.Token;
        }

        protected async Task<Response?> RegisterAsUser(HttpClient client)
        {
            return await RegisterAs(client, "user", "P4$$w0rd", "user@example.net");
        }

        protected async Task<Response?> RegisterAs(HttpClient client, string user, string pass, string mail)
        {
            RegisterModel regData = new () {
                Username = user,
                Password = pass,
                Email = mail
            };

            var response = await client.PostAsJsonAsync("Auth/register", regData);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resultRegister = JsonSerializer.Deserialize<Response>(responseString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            return resultRegister;
        }
    }
}
