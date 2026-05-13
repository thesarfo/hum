using SQLite;

namespace Hum.Server.Data;

[Table("Fingerprints")]
public class FingerprintRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public uint Hash { get; set; }

    public int SongId { get; set; }
    public int TimeOffset { get; set; }
}
