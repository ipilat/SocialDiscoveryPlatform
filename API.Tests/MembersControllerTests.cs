using API.Controllers;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests;

public class MembersControllerTests
{
    private readonly Mock<IMemberRepository> _repoMock;
    private readonly MembersController _controller;

    public MembersControllerTests()
    {
        _repoMock = new Mock<IMemberRepository>();
        _controller = new MembersController(_repoMock.Object);
    }

    [Fact]
    public async Task GetMembers_ReturnsOkWithAllMembers()
    {
        var members = new List<Member>
        {
            MakeMember("1", "Alice"),
            MakeMember("2", "Bob")
        };
        _repoMock.Setup(r => r.GetMembersAsync()).ReturnsAsync(members);

        var result = await _controller.GetMembers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IReadOnlyList<Member>>(ok.Value);
        Assert.Equal(2, returned.Count);
    }

    [Fact]
    public async Task GetMembers_ReturnsEmptyList_WhenNoMembers()
    {
        _repoMock.Setup(r => r.GetMembersAsync()).ReturnsAsync(new List<Member>());

        var result = await _controller.GetMembers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IReadOnlyList<Member>>(ok.Value);
        Assert.Empty(returned);
    }

    [Fact]
    public async Task GetMember_ReturnsMember_WhenFound()
    {
        var member = MakeMember("abc-123", "Carol");
        _repoMock.Setup(r => r.GetMemberByIdAsync("abc-123")).ReturnsAsync(member);

        var result = await _controller.GetMember("abc-123");

        var returned = Assert.IsType<Member>(result.Value);
        Assert.Equal("Carol", returned.DisplayName);
    }

    [Fact]
    public async Task GetMember_ReturnsNotFound_WhenMemberDoesNotExist()
    {
        _repoMock.Setup(r => r.GetMemberByIdAsync("missing-id")).ReturnsAsync((Member?)null);

        var result = await _controller.GetMember("missing-id");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetMemberPhotos_ReturnsOkWithPhotos()
    {
        var photos = new List<Photo>
        {
            new() { Id = 1, Url = "http://photo1.jpg", MemberId = "user-1" },
            new() { Id = 2, Url = "http://photo2.jpg", MemberId = "user-1" }
        };
        _repoMock.Setup(r => r.GetPhotosForMemberAsync("user-1")).ReturnsAsync(photos);

        var result = await _controller.GetMemberPhotos("user-1");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IReadOnlyList<Photo>>(ok.Value);
        Assert.Equal(2, returned.Count);
    }

    [Fact]
    public async Task GetMemberPhotos_ReturnsEmptyList_WhenMemberHasNoPhotos()
    {
        _repoMock.Setup(r => r.GetPhotosForMemberAsync("user-2")).ReturnsAsync(new List<Photo>());

        var result = await _controller.GetMemberPhotos("user-2");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IReadOnlyList<Photo>>(ok.Value);
        Assert.Empty(returned);
    }

    private static Member MakeMember(string id, string displayName) => new()
    {
        Id = id,
        DisplayName = displayName,
        Gender = "female",
        City = "London",
        Country = "UK"
    };
}
