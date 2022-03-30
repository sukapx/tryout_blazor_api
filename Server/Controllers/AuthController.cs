using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using tryout_blazor_api.Server.Models;
using tryout_blazor_api.Shared;
using tryout_blazor_api.Shared.Auth;

// https://www.c-sharpcorner.com/article/jwt-authentication-and-authorization-in-net-6-0-with-identity-framework/
namespace tryout_blazor_api.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly SymmetricSecurityKey _authSigningKey;
        private readonly SigningCredentials _signingCredentials;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;


            _authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            _signingCredentials = new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await GetTokenAsync(user);
                var tokenRefresh = await GetRefreshTokenAsync(user);

                return Ok(new LoginToken
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    TokenRefresh = new JwtSecurityTokenHandler().WriteToken(tokenRefresh),
                    TokenExpiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                string reasons = string.Join(",", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status400BadRequest, new Response {
                    Status = "User creation failed",
                    Message = $"[{reasons}]"
                });
            }
            await EnsureRoleAndAdd(user, UserRoles.User);
            await EnsureRoleAndAdd(user, UserRoles.SightModerator);

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] Refresh request)
        {
            _logger.LogInformation("Called 'Refresh' {}", request.TokenRefresh);
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var claims = tokenHandler.ValidateToken(request.TokenRefresh, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["JWT:ValidIssuer"],
                    ValidAudience = _configuration["JWT:RefreshValidAudience"],
                    IssuerSigningKey = _authSigningKey
                }, out SecurityToken validatedToken);

                foreach (var claim in claims.Claims)
                    _logger.LogInformation("Claim {claim} is {value}", claim.Subject, claim.Value);

                var userID = claims.FindFirst("ID");
                var refreshToken = claims.FindFirst("token");

                _logger.LogInformation("userID '{userID}', refreshToken '{refreshToken}'", userID, refreshToken);
                if (userID is null || refreshToken is null)
                {
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(userID.Value!);
                if (user is null || user.RefreshToken != refreshToken.Value)
                {
                    return Unauthorized();
                }

                var token = await GetTokenAsync(user);
                var tokenRefresh = await GetRefreshTokenAsync(user);

                return Ok(new LoginToken
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    TokenRefresh = new JwtSecurityTokenHandler().WriteToken(tokenRefresh),
                    TokenExpiration = token.ValidTo
                });
            }
            catch
            {
            }
            return Unauthorized();
        }


        private async Task<JwtSecurityToken> GetTokenAsync(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("ID", user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddHours(float.Parse(_configuration["JWT:ValidHours"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private async Task<JwtSecurityToken> GetRefreshTokenAsync(ApplicationUser user)
        {
            user.RefreshToken = Guid.NewGuid().ToString();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(float.Parse(_configuration["JWT:RefreshValidHours"]));
            await _userManager.UpdateAsync(user);

            var authClaims = new List<Claim>()
            {
                new Claim("ID", user.Id),
                new Claim("token", user.RefreshToken),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:RefreshValidAudience"],
                expires: user.RefreshTokenExpiry,
                claims: authClaims,
                signingCredentials: _signingCredentials
                );

            return token;
        }

        /// <summary>
        /// Ensures that role exists and user is added to it
        /// </summary>
        private async Task EnsureRoleAndAdd(ApplicationUser user, string role)
        {
            if(await _roleManager.FindByNameAsync(role) is null)
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            await _userManager.AddToRoleAsync(user, role);
        }
    }
}