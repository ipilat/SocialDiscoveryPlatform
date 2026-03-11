using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Entities;
using API.Services;
using Microsoft.Extensions.Configuration;

namespace API.Tests;

public class TokenServiceTests
{
    // HMAC-SHA512 requires >= 64 bytes; service throws if > 64 — so exactly 64 chars is valid
    private static readonly string ValidTokenKey = new string('x', 64);

    private static IConfiguration BuildConfig(string tokenKey) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["TokenKey"] = tokenKey })
            .Build();

    private static AppUser MakeUser() => new()
    {
        Id = "user-1",
        DisplayName = "Alice",
        Email = "alice@test.com",
        PasswordHash = [],
        PasswordSalt = []
    };

    [Fact]
    public void CreateToken_ReturnsNonEmptyString()
    {
        var service = new TokenService(BuildConfig(ValidTokenKey));
        var token = service.CreateToken(MakeUser());
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void CreateToken_ContainsEmailClaim()
    {
        var service = new TokenService(BuildConfig(ValidTokenKey));
        var token = service.CreateToken(MakeUser());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // JwtSecurityTokenHandler maps ClaimTypes.Email -> "email" short name
        var email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
        Assert.Equal("alice@test.com", email);
    }

    [Fact]
    public void CreateToken_ContainsNameIdentifierClaim()
    {
        var service = new TokenService(BuildConfig(ValidTokenKey));
        var token = service.CreateToken(MakeUser());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // JwtSecurityTokenHandler maps ClaimTypes.NameIdentifier -> "nameid" short name
        var id = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value;
        Assert.Equal("user-1", id);
    }

    [Fact]
    public void CreateToken_ExpiresInApproximatelySevenDays()
    {
        var service = new TokenService(BuildConfig(ValidTokenKey));
        var token = service.CreateToken(MakeUser());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddDays(7);
        Assert.InRange(jwt.ValidTo, expectedExpiry.AddMinutes(-1), expectedExpiry.AddMinutes(1));
    }

    [Fact]
    public void CreateToken_ThrowsWhenTokenKeyIsMissing()
    {
        var service = new TokenService(BuildConfig(null!));
        Assert.Throws<Exception>(() => service.CreateToken(MakeUser()));
    }

    [Fact]
    public void CreateToken_ThrowsWhenTokenKeyExceedsSixtyFourChars()
    {
        var longKey = new string('x', 65);
        var service = new TokenService(BuildConfig(longKey));
        Assert.Throws<Exception>(() => service.CreateToken(MakeUser()));
    }
}
