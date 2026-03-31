using Microsoft.EntityFrameworkCore;
using Xunit;
using practic12;

namespace practic12.Test;



[CollectionDefinition("NonParallel", DisableParallelization = true)]
public class NonParallelCollectionDefinition { }


[Collection("NonParallel")]
public class NoteCrudTests 
{
    private DataContext _db;

    public NoteCrudTests()
    {
        ResetDatabase();
    }

    private void ResetDatabase()
    {
        _db?.Dispose();
        _db = new DataContext();
        _db.Database.EnsureCreated();

        _db.Database.ExecuteSqlRaw("DELETE FROM Notes");
        _db.Database.ExecuteSqlRaw("DELETE FROM Users");
        _db.ChangeTracker.Clear();
    }

    private async Task<int> CreateTestUser()
    {
        var user = new User { Name = "тест" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _db.Entry(user).ReloadAsync();
        return user.Id;
    }

    //  Create

    [Fact]
    public async Task Create_SaveCorrectNoteToDataBase()
    {
        ResetDatabase();

        int userId = await CreateTestUser();
        string text = "Тест";

        var note = await NoteCrud.Create(userId, text);
        var saved = await _db.Notes.FirstOrDefaultAsync(x => x.Id == note.Id);

        Assert.NotNull(note);
        Assert.NotEqual(0, note.Id);
        Assert.Equal(text, note.Text);
        Assert.Equal(userId, note.UserId);
        Assert.True(note.Time <= DateTime.Now);
        Assert.NotNull(saved);
        Assert.Equal(text, saved.Text);
        Assert.Equal(userId, saved.UserId);
        Assert.True(saved.Time <= DateTime.Now);
    }

    [Fact]
    public async Task Create_WithoutUser_ShouldThrowException()
    {
        ResetDatabase();

        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await NoteCrud.Create(999, "тест");
        });
    }

    //  Read by Text

    [Fact]
    public async Task ReadByText_ReturnCorectNotes()
    {
        ResetDatabase();

        int userId = await CreateTestUser();

        await NoteCrud.Create(userId, "хлеб черный");
        await NoteCrud.Create(userId, "хлеб белый");
        await NoteCrud.Create(userId, "молоко");
        await NoteCrud.Create(userId, "яйца");

        var res = await NoteCrud.Read("хлеб");

        Assert.Equal(2, res.Count);
        Assert.All(res, x => Assert.Contains("хлеб", x.Text));
    }

    [Fact]
    public async Task ReadByText_EmptyString_ReturnAllNotes()
    {
        ResetDatabase();

        int userId = await CreateTestUser();

        await NoteCrud.Create(userId, "хлеб черный");
        await NoteCrud.Create(userId, "хлеб белый");
        await NoteCrud.Create(userId, "молоко");
        await NoteCrud.Create(userId, "яйца");

        var res = await NoteCrud.Read("");
        Assert.Equal(4, res.Count);
    }

    [Fact]
    public async Task ReadByText_MissingString_ReturnEmptyList()
    {
        ResetDatabase();

        int userId = await CreateTestUser();

        await NoteCrud.Create(userId, "хлеб черный");
        await NoteCrud.Create(userId, "хлеб белый");
        await NoteCrud.Create(userId, "молоко");
        await NoteCrud.Create(userId, "яйца");

        var res = await NoteCrud.Read("горошек");
        Assert.Empty(res);
    }

    //  Read by Id

    [Fact]
    public async Task ReadById_CorrectId_ReturnCorrectNote()
    {
        ResetDatabase();

        int userId = await CreateTestUser();

        var note = await NoteCrud.Create(userId, "хлеб черный");
        await NoteCrud.Create(userId, "хлеб белый");
        await NoteCrud.Create(userId, "молоко");
        await NoteCrud.Create(userId, "яйца");

        var res = await NoteCrud.Read(note.Id);

        Assert.NotNull(res);
        Assert.Equal("хлеб черный", res.Text);
        Assert.Equal(userId, res.UserId);
    }

    [Fact]
    public async Task ReadById_MissingId_ReturnNull()
    {
        ResetDatabase();

        int userId = await CreateTestUser();

        await NoteCrud.Create(userId, "хлеб черный");
        await NoteCrud.Create(userId, "хлеб белый");
        await NoteCrud.Create(userId, "молоко");
        await NoteCrud.Create(userId, "яйца");

        var res = await NoteCrud.Read(100);
        Assert.Null(res);
    }

    //  Update

    [Fact]
    public async Task Update_CorrectUpdateNote()
    {
        ResetDatabase();

        int userId = await CreateTestUser();

        var note = await NoteCrud.Create(userId, "яйца");
        var origId = note.Id;
        var origTime = note.Time;

        await NoteCrud.Update(note, "хлеб");
        var res = await NoteCrud.Read(origId);

        Assert.NotNull(res);
        Assert.Equal("хлеб", res.Text);
        Assert.Equal(origId, res.Id);
        Assert.Equal(origTime, res.Time);
        Assert.Equal(userId, res.UserId);
    }

    //  Delete

    [Fact]
    public async Task Delete_DeleteNoteFromDataBase()
    {
        ResetDatabase();

        int userId = await CreateTestUser();

        var note = await NoteCrud.Create(userId, "хлеб");
        var id = note.Id;
        await NoteCrud.Delete(note);
        var deleted = await NoteCrud.Read(id);
        Assert.Null(deleted);
    }
}