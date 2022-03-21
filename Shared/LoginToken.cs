namespace tryout_blazor_api.Shared.Auth
{
    public class LoginToken
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
