namespace tryout_blazor_api.Shared.Auth
{
    public class LoginToken
    {
        public string? Token { get; set; }
        public DateTime? TokenExpiration { get; set; }

        public string? TokenRefresh { get; set; }
    }
}
