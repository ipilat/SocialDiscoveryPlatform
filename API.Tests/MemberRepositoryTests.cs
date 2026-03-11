using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Tests;

public class MemberRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly MemberRepository _repo;

    public MemberRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repo = new MemberRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetMembersAsync_ReturnsAllMembers()
    {
        _context.Members.AddRange(MakeMember("1", "Alice"), MakeMember("2", "Bob"));
        await _context.SaveChangesAsync();

        var result = await _repo.GetMembersAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetMembersAsync_IncludesPhotos()
    {
        var member = MakeMember("m1", "Alice");
        member.Photos.Add(new Photo { Url = "http://photo.jpg", MemberId = "m1" });
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var result = await _repo.GetMembersAsync();

        Assert.Single(result[0].Photos);
    }

    [Fact]
    public async Task GetMemberByIdAsync_ReturnsMember_WhenExists()
    {
        _context.Members.Add(MakeMember("abc", "Carol"));
        await _context.SaveChangesAsync();

        var member = await _repo.GetMemberByIdAsync("abc");

        Assert.NotNull(member);
        Assert.Equal("Carol", member.DisplayName);
    }

    [Fact]
    public async Task GetMemberByIdAsync_ReturnsNull_WhenNotFound()
    {
        var member = await _repo.GetMemberByIdAsync("does-not-exist");
        Assert.Null(member);
    }

    [Fact]
    public async Task GetPhotosForMemberAsync_ReturnsOnlyThatMembersPhotos()
    {
        var m1 = MakeMember("m1", "Dave");
        m1.Photos.Add(new Photo { Url = "http://dave-photo.jpg", MemberId = "m1" });

        var m2 = MakeMember("m2", "Eve");
        m2.Photos.Add(new Photo { Url = "http://eve-photo.jpg", MemberId = "m2" });

        _context.Members.AddRange(m1, m2);
        await _context.SaveChangesAsync();

        var photos = await _repo.GetPhotosForMemberAsync("m1");

        Assert.Single(photos);
        Assert.Equal("http://dave-photo.jpg", photos[0].Url);
    }

    [Fact]
    public async Task GetPhotosForMemberAsync_ReturnsEmpty_WhenMemberHasNoPhotos()
    {
        _context.Members.Add(MakeMember("m3", "Frank"));
        await _context.SaveChangesAsync();

        var photos = await _repo.GetPhotosForMemberAsync("m3");

        Assert.Empty(photos);
    }

    [Fact]
    public async Task SaveAllAsync_ReturnsTrue_WhenChangesSaved()
    {
        _context.Members.Add(MakeMember("m4", "Grace"));

        var result = await _repo.SaveAllAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task SaveAllAsync_ReturnsFalse_WhenNoChanges()
    {
        var result = await _repo.SaveAllAsync();
        Assert.False(result);
    }

    [Fact]
    public async Task Update_MarksEntityAsModified()
    {
        var member = MakeMember("m5", "Heidi");
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        member.DisplayName = "Heidi Updated";
        _repo.Update(member);

        Assert.Equal(EntityState.Modified, _context.Entry(member).State);
    }

    private static Member MakeMember(string id, string displayName) => new()
    {
        Id = id,
        DisplayName = displayName,
        Gender = "female",
        City = "Paris",
        Country = "France",
        Photos = []
    };
}
