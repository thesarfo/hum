namespace Hum.Server.Models;

public class IngestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string WavBase64 { get; set; } = string.Empty;
}
