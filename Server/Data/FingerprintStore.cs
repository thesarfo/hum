using SQLite;

namespace Hum.Server.Data;

public class FingerprintStore
{
    private readonly string _connectionString;

    public FingerprintStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InsertFingerprintsAsync(int songId, List<(uint Hash, int TimeOffset)> fingerprints)
    {
        if (fingerprints.Count == 0)
            return;

        var db = new SQLiteAsyncConnection(_connectionString);

        await db.RunInTransactionAsync(conn =>
        {
            conn.Execute("PRAGMA foreign_keys = ON;");

            foreach (var fp in fingerprints)
            {
                conn.Execute(
                    "INSERT INTO Fingerprints (Hash, SongId, TimeOffset) VALUES (?, ?, ?)",
                    fp.Hash, songId, fp.TimeOffset);
            }
        });
    }

    public async Task<IReadOnlyList<(int SongId, int TimeOffset)>> LookupHashAsync(uint hash)
    {
        var db = new SQLiteAsyncConnection(_connectionString);
        var rows = await db.QueryAsync<FingerprintRecord>(
            "SELECT SongId, TimeOffset FROM Fingerprints WHERE Hash = ?", hash);

        return rows.Select(r => (r.SongId, r.TimeOffset)).ToList();
    }

    public async Task<Song?> GetSongAsync(int songId)
    {
        var db = new SQLiteAsyncConnection(_connectionString);
        return await db.FindAsync<Song>(songId);
    }
}
