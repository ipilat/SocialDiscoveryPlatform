using System.Security.Cryptography;
using System.Text;
using API.Controllers;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests;

public class AccountControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _tokenServiceMock = new Mock<ITokenService>();
        _tokenServiceMock.Setup(t => t.CreateToken(It.IsAny<AppUser>())).Returns("fake-jwt-token");
        _controller = new AccountController(_context, _tokenServiceMock.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Register_ReturnsUserDto_WhenValid()
    {
        var dto = new RegisterDto
        {
            DisplayName = "Alice",
            Email = "alice@test.com",
            Password = "Password1!"
        };

        var result = await _controller.Register(dto);

        var ok = Assert.IsType<ActionResult<UserDto>>(result);
        var userDto = Assert.IsType<UserDto>(ok.Value);
        Assert.Equal("alice@test.com", userDto.Email);
        Assert.Equal("Alice", userDto.DisplayName);
        Assert.Equal("fake-jwt-token", userDto.Token);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenEmailAlreadyTaken()
    {
        var existingUser = CreateUserWithPassword("bob@test.com", "Password1!");
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var dto = new RegisterDto
        {
            DisplayName = "Bob",
            Email = "bob@test.com",
            Password = "Password1!"
        };

        var result = await _controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Email taken", badRequest.Value);
    }

    [Fact]
    public async Task Register_EmailCheck_IsCaseInsensitive()
    {
        var existingUser = CreateUserWithPassword("carol@test.com", "Password1!");
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var dto = new RegisterDto
        {
            DisplayName = "Carol",
            Email = "CAROL@TEST.COM",
            Password = "Password1!"
        };

        var result = await _controller.Register(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ReturnsUserDto_WithValidCredentials()
    {
        var user = CreateUserWithPassword("dave@test.com", "MyPassword!");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _controller.Login(new LoginDto
        {
            Email = "dave@test.com",
            Password = "MyPassword!"
        });

        var userDto = Assert.IsType<UserDto>(result.Value);
        Assert.Equal("dave@test.com", userDto.Email);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenEmailNotFound()
    {
        var result = await _controller.Login(new LoginDto
        {
            Email = "nobody@test.com",
            Password = "anything"
        });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        var user = CreateUserWithPassword("eve@test.com", "CorrectPassword!");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _controller.Login(new LoginDto
        {
            Email = "eve@test.com",
            Password = "WrongPassword!"
        });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_PersistsUserToDatabase()
    {
        var dto = new RegisterDto
        {
            DisplayName = "Frank",
            Email = "frank@test.com",
            Password = "Password1!"
        };

        await _controller.Register(dto);

        Assert.Equal(1, await _context.Users.CountAsync());
        Assert.True(await _context.Users.AnyAsync(u => u.Email == "frank@test.com"));
    }

    private static AppUser CreateUserWithPassword(string email, string password)
    {
        using var hmac = new HMACSHA512();
        return new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            DisplayName = email.Split('@')[0],
            Email = email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
            PasswordSalt = hmac.Key
        };
    }
}
