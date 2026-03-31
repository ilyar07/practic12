using Microsoft.EntityFrameworkCore;

namespace practic12;

public class UserCrud
{
    public static async Task<User> Create(string name, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        User user = new User { Name = name };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }

    public static async Task<List<User>> Read(string search, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var res = await db.Users
            .Where(x => EF.Functions.Like(x.Name, $"%{search}%"))
            .ToListAsync(ct);

        return res;

    }

    public static async Task<User?> Read(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public static async Task Update(User user, string name, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        user.Name = name;
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }

    public static async Task Delete(User user, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
    }

    public static async Task<List<Note>> GetAllNotes(int userId, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var res = await db.Notes
             .Where(x => x.UserId == userId)
             .ToListAsync(ct);

        return res;

    }
}
