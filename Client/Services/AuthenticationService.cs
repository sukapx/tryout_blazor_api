using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using tryout_blazor_api.Client.Util;
using tryout_blazor_api.Shared;
using tryout_blazor_api.Shared.Auth;

namespace tryout_blazor_api.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private HttpClient _httpclient;
        private NavigationManager _navigationManager;
        private ILocalStorageService _localStorageService;
        private readonly AuthenticationStateProvider _authStateProvider;


        public AuthenticationService(
            HttpClient httpclient,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService,
            AuthenticationStateProvider authStateProvider
        ) {
            _httpclient = httpclient;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
            _authStateProvider = authStateProvider;
        }

        public async Task Initialize()
        {
            if(_httpclient.DefaultRequestHeaders.Authorization is null) {
                var token = await _localStorageService.GetItemAsStringAsync("authToken");
                if(token is not null) {
                    _httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        public async Task Register(RegisterModel regData) {
            var response = await _httpclient.PostAsJsonAsync("Auth/register", regData);
            try
            {
                response.EnsureSuccessStatusCode();
            }catch(HttpRequestException ex)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
            var responseString = await response.Content.ReadAsStringAsync();
            var resultRegister = JsonSerializer.Deserialize<Response>(responseString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            _navigationManager.NavigateTo("/");
        }

        public async Task Login(LoginModel loginData)
        {
            var responseLogin = await _httpclient.PostAsJsonAsync("Auth/login", loginData);
            responseLogin.EnsureSuccessStatusCode();
            var responseString = await responseLogin.Content.ReadAsStringAsync();
            var resultLogin = JsonSerializer.Deserialize<LoginToken>(responseString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            _httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resultLogin!.Token);

            await _localStorageService.SetItemAsStringAsync("authToken", resultLogin!.Token);
            await _localStorageService.SetItemAsStringAsync("authTokenRefresh", resultLogin!.TokenRefresh);
            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(resultLogin!.Token);

            _navigationManager.NavigateTo("/");
        }

        public async Task<bool> EnsureAuth()
        {
            Console.WriteLine("EnsureAuth");
            var tokenRefresh = await _localStorageService.GetItemAsStringAsync("authToken");
            if (tokenRefresh is null)
            {
                Console.WriteLine("Refresh token is null");
                return false;
            }

            var refreshTime = JwtParser.GetExpiry(tokenRefresh).AddMinutes(-1);

            Console.WriteLine($"refreshTime {refreshTime}, Now {DateTime.UtcNow}");
            if (DateTime.UtcNow.CompareTo(refreshTime) >= 0)
                await Refresh();

            return true;
        }

        public async Task Refresh()
        {
            var request = new Refresh {
                TokenRefresh = await _localStorageService.GetItemAsStringAsync("authTokenRefresh")
            };
            _httpclient.DefaultRequestHeaders.Authorization = null;
            var response = await _httpclient.PostAsJsonAsync("Auth/Refresh", request);
            response.EnsureSuccessStatusCode();
            var responseObject = await response.Content.ReadFromJsonAsync<LoginToken>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            _httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseObject!.Token);

            await _localStorageService.SetItemAsStringAsync("authToken", responseObject!.Token);
            await _localStorageService.SetItemAsStringAsync("authTokenRefresh", responseObject!.TokenRefresh);
            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(responseObject!.Token);
        }

        public async Task Logout()
        {
            _httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "INVALID");
            await _localStorageService.RemoveItemAsync("authToken");
            await _localStorageService.RemoveItemAsync("authTokenRefresh");
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
            _navigationManager.NavigateTo("/");
        }
    }
}