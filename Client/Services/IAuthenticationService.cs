using tryout_blazor_api.Shared.Auth;

namespace tryout_blazor_api.Client.Services
{
	public interface IAuthenticationService
	{
		Task Initialize();
		Task Register(RegisterModel regData);
		Task Login(LoginModel loginData);
		Task Logout();
	}
}
