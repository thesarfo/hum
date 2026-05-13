using Microsoft.JSInterop;

namespace Hum.Client.Services;

public class AudioCapture
{
    private const string JsFunctionName = "captureAudio";
    private readonly IJSRuntime _js;

    public AudioCapture(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<string> RecordAsync(int durationMs = 5000)
    {
        try
        {
            return await _js.InvokeAsync<string>(JsFunctionName, durationMs);
        }
        catch (JSException ex)
        {
            throw new AudioCaptureException(ex.Message);
        }
    }
}
