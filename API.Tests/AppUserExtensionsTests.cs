using API.Entities;
using API.Extensions;
using API.Interfaces;
using Moq;

namespace API.Tests;

public class AppUserExtensionsTests
{
    [Fact]
    public void ToDto_MapsAllFieldsCorrectly()
    {
        var user = new AppUser
        {
            Id = "user-42",
            DisplayName = "Alice",
            Email = "alice@test.com",
            ImageUrl = "http://img.jpg",
            PasswordHash = [],
            PasswordSalt = []
        };

        var tokenMock = new Mock<ITokenService>();
        tokenMock.Setup(t => t.CreateToken(user)).Returns("test-token");

        var dto = user.ToDto(tokenMock.Object);

        Assert.Equal("user-42", dto.Id);
        Assert.Equal("Alice", dto.DisplayName);
        Assert.Equal("alice@test.com", dto.Email);
        Assert.Equal("http://img.jpg", dto.ImageUrl);
        Assert.Equal("test-token", dto.Token);
    }

    [Fact]
    public void ToDto_SetsNullImageUrl_WhenUserHasNoImage()
    {
        var user = new AppUser
        {
            Id = "user-99",
            DisplayName = "Bob",
            Email = "bob@test.com",
            ImageUrl = null,
            PasswordHash = [],
            PasswordSalt = []
        };

        var tokenMock = new Mock<ITokenService>();
        tokenMock.Setup(t => t.CreateToken(user)).Returns("token");

        var dto = user.ToDto(tokenMock.Object);

        Assert.Null(dto.ImageUrl);
    }

    [Fact]
    public void ToDto_CallsCreateToken_ExactlyOnce()
    {
        var user = new AppUser
        {
            Id = "user-1",
            DisplayName = "Carol",
            Email = "carol@test.com",
            PasswordHash = [],
            PasswordSalt = []
        };

        var tokenMock = new Mock<ITokenService>();
        tokenMock.Setup(t => t.CreateToken(user)).Returns("token");

        user.ToDto(tokenMock.Object);

        tokenMock.Verify(t => t.CreateToken(user), Times.Once);
    }
}
