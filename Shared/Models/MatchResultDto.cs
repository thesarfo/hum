namespace Hum.Shared.Models;

public class MatchResultDto
{
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public double Confidence { get; set; }
}
