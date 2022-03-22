using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using tryout_blazor_api.Shared;
using tryout_blazor_api.Shared.Auth;

// https://jasonwatmore.com/post/2020/08/13/blazor-webassembly-jwt-authentication-example-tutorial
namespace tryout_blazor_api.Client.Services
{
    public interface IAuthenticationService
    {
        Task Initialize();
        Task Register(RegisterModel regData);
        Task Login(LoginModel loginData);
        Task Logout();
    }

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
            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(resultLogin!.Token);

            _navigationManager.NavigateTo("/");
        }

        public async Task Logout()
        {
            await _localStorageService.RemoveItemAsync("authToken");
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
            _navigationManager.NavigateTo("/");
        }
    }
}