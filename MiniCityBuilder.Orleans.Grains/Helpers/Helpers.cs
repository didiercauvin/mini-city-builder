using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace MiniCityBuilder.Orleans.Grains.Helpers;

public interface IPasswordHashHelper
{
    (string salt, string hashed) CalculateHash(string password, string salt = null);
}

public class PasswordHashHelper : IPasswordHashHelper
{
    private readonly Random _random = new Random();

    public (string salt, string hashed) CalculateHash(string password, string salt = null)
    {
        if (salt == null)
        {
            var saltData = new byte[16];
            _random.NextBytes(saltData);

            salt = Convert.ToBase64String(saltData);
        }

        var sha1 = KeyDerivation.Pbkdf2(password: password, salt: Convert.FromBase64String(salt), prf: KeyDerivationPrf.HMACSHA1, iterationCount: 10000, numBytesRequested: 32);
        var hashed = Convert.ToBase64String(sha1);

        return (salt, hashed);
    }
}

public class JwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string username)
    {
        var secret = _configuration["JwtToken:Secret"];
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("username", username)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(-1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class AddRegisterHelpersService
{
    public static void AddRegisterHelpers(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHashHelper, PasswordHashHelper>();
        services.AddSingleton<JwtTokenGenerator>();
    }
}