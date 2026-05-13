using SQLite;

namespace Hum.Server.Data;

public class DbInitializer
{
    private readonly string _connectionString;

    public DbInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Initialize()
    {
        var db = new SQLiteConnection(_connectionString);
        db.CreateTable<Song>();
        db.CreateTable<FingerprintRecord>();
        db.Close();
    }
}
