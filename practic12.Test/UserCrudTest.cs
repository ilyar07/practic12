using Microsoft.EntityFrameworkCore;
using Xunit;
using practic12;

namespace practic12.Test;


public class UserCrudTests
{
    private DataContext _db;

    public UserCrudTests()
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

    //  Create

    [Fact]
    public async Task Create_SaveCorrectUserToDataBase()
    {
        ResetDatabase();

        string name = "тест";

        var user = await UserCrud.Create(name);
        var saved = await _db.Users.FirstOrDefaultAsync(x => x.Id == user.Id);

        Assert.NotNull(saved);
        Assert.Equal(name, saved.Name);
        Assert.NotEqual(0, saved.Id);
    }

    //  Read by text

    [Fact]
    public async Task ReadByText_ReturnCorrectUser()
    {
        ResetDatabase();

        await UserCrud.Create("андрей");
        await UserCrud.Create("анна");
        await UserCrud.Create("олег");
        await UserCrud.Create("даша");

        var res = await UserCrud.Read("ан");

        Assert.Equal(2, res.Count);
        Assert.All(res, x => Assert.Contains("ан", x.Name));
    }

    [Fact]
    public async Task ReadByText_EmptyString_ReturnAllUsers()
    {
        ResetDatabase();

        await UserCrud.Create("андрей");
        await UserCrud.Create("анна");
        await UserCrud.Create("олег");
        await UserCrud.Create("даша");

        var res = await UserCrud.Read("");
        Assert.Equal(4, res.Count);
    }

    [Fact]
    public async Task ReadByText_MissingString_ReturnEmptyList()
    {
        ResetDatabase();

        await UserCrud.Create("андрей");
        await UserCrud.Create("анна");
        await UserCrud.Create("олег");
        await UserCrud.Create("даша");

        var res = await UserCrud.Read("ба");
        Assert.Empty(res);
    }

    //  Read by id

    [Fact]
    public async Task ReadById_CorrectId_ReturnCorrectUser()
    {
        ResetDatabase();

        var user = await UserCrud.Create("андрей");
        await UserCrud.Create("анна");
        await UserCrud.Create("олег");
        await UserCrud.Create("даша");

        var res = await UserCrud.Read(user.Id);

        Assert.NotNull(res);
        Assert.Equal(user.Id, res.Id);
        Assert.Equal(user.Name, res.Name);
    }

    [Fact]
    public async Task ReadById_MissingId_ReturnNull()
    {
        ResetDatabase();

        await UserCrud.Create("андрей");
        await UserCrud.Create("анна");
        await UserCrud.Create("олег");
        await UserCrud.Create("даша");

        var res = await UserCrud.Read(10000);
        Assert.Null(res);
    }

    //  Update

    [Fact]
    public async Task Update_CorrectUpdateUser()
    {
        ResetDatabase();

        var user = await UserCrud.Create("андрей");
        var origId = user.Id;

        await UserCrud.Update(user, "миша");
        var res = await UserCrud.Read(origId);

        Assert.NotNull(res);
        Assert.Equal("миша", res.Name);
        Assert.Equal(res.Id, origId);
    }

    //  Delete

    [Fact]
    public async Task Delete_DeleteUserAndHerNotes()
    {
        ResetDatabase();

        var user = await UserCrud.Create("анна");

        await NoteCrud.Create(user.Id, "тест1");
        await NoteCrud.Create(user.Id, "тест2");

        var notesBefore = await _db.Notes.Where(x => x.UserId == user.Id).ToListAsync();
        Assert.Equal(2, notesBefore.Count);

        await UserCrud.Delete(user);

        var notesAfter = await _db.Notes.Where(x => x.UserId == user.Id).ToListAsync();
        Assert.Empty(notesAfter);

        var deletedUser = await UserCrud.Read(user.Id);
        Assert.Null(deletedUser);
    }

    //  Get all notes

    [Fact]
    public async Task GetAllNotes_ReturnOnlyNotesOfThatUser()
    {
        ResetDatabase();

        var user1 = await UserCrud.Create("анна");
        var user2 = await UserCrud.Create("иван");

        await NoteCrud.Create(user1.Id, "заметка анна1");
        await NoteCrud.Create(user1.Id, "заметка анна2");
        await NoteCrud.Create(user2.Id, "заметка иван");

        var user1Notes = await UserCrud.GetAllNotes(user1.Id);

        Assert.Equal(2, user1Notes.Count);
        Assert.All(user1Notes, n => Assert.Equal(user1.Id, n.UserId));
        Assert.All(user1Notes, n => Assert.Contains("анна", n.Text));
    }
}
