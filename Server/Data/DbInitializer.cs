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
        using var db = new SQLiteConnection(_connectionString);
        db.Execute("PRAGMA foreign_keys = ON;");

        db.Execute(
            "CREATE TABLE IF NOT EXISTS Songs (" +
            "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "Title TEXT NOT NULL, " +
            "Artist TEXT NOT NULL, " +
            "Duration REAL NOT NULL);");

        db.Execute(
            "CREATE TABLE IF NOT EXISTS Fingerprints (" +
            "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "Hash INTEGER NOT NULL, " +
            "SongId INTEGER NOT NULL, " +
            "TimeOffset INTEGER NOT NULL, " +
            "FOREIGN KEY (SongId) REFERENCES Songs(Id) ON DELETE CASCADE);");

        db.Execute("CREATE INDEX IF NOT EXISTS IX_Fingerprints_Hash ON Fingerprints(Hash);");
        db.Execute("CREATE INDEX IF NOT EXISTS IX_Fingerprints_SongId ON Fingerprints(SongId);");
    }
}
