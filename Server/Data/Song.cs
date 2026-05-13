using SQLite;

namespace Hum.Server.Data;

[Table("Songs")]
public class Song
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public double Duration { get; set; }
}
