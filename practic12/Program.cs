namespace practic12
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            await using var db = new DataContext();
            await db.Database.EnsureCreatedAsync();
        }
    }
}