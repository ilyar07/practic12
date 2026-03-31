using Microsoft.EntityFrameworkCore;

namespace practic12;

public class NoteCrud
{
    public static async Task<Note> Create(int userId, string text, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        Note newNote = new Note { Text = text, Time = DateTime.Now, UserId = userId };

        db.Notes.Add(newNote);
        await db.SaveChangesAsync(ct);

        return newNote;

    }

    public static async Task<List<Note>> Read(string search, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var res = await db.Notes
            .Where(x => EF.Functions.Like(x.Text, $"%{search}%"))
            .ToListAsync(ct);

        return res;

    }

    public static async Task<Note?> Read(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Notes.FirstOrDefaultAsync(x => x.Id == id, ct);

    }

    public static async Task Update(Note note, string newText, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        note.Text = newText;
        db.Notes.Update(note);
        await db.SaveChangesAsync(ct);
    }

    public static async Task Delete(Note note, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        db.Notes.Remove(note);
        await db.SaveChangesAsync(ct);
    }
}
