namespace Hum.Server.Audio;

public class InvalidAudioDataException : Exception
{
    public InvalidAudioDataException(string message) : base(message) { }
}
