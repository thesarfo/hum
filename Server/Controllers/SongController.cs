using Hum.Server.Audio;
using Hum.Server.Data;
using Hum.Server.Models;
using Hum.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hum.Server.Controllers;

[ApiController]
[Route("api/songs")]
public class SongController : ControllerBase
{
    private readonly FingerprintService _fingerprintService;
    private readonly FingerprintStore _store;

    public SongController(FingerprintService fingerprintService, FingerprintStore store)
    {
        _fingerprintService = fingerprintService;
        _store = store;
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest([FromBody] IngestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title is required." });

        if (string.IsNullOrWhiteSpace(request.Artist))
            return BadRequest(new { error = "Artist is required." });

        if (string.IsNullOrWhiteSpace(request.WavBase64))
            return BadRequest(new { error = "WavBase64 is required." });

        byte[] wavBytes;
        try
        {
            wavBytes = Convert.FromBase64String(request.WavBase64);
        }
        catch
        {
            return BadRequest(new { error = "WavBase64 could not be decoded. Provide a valid base64-encoded WAV file." });
        }

        double duration = 0;
        try
        {
            var decoder = new PcmDecoder();
            var result = decoder.Decode(wavBytes);
            duration = (double)result.Samples.Length / result.SampleRate;
        }
        catch (InvalidAudioDataException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        var fingerprints = _fingerprintService.GenerateFingerprints(wavBytes);

        int songId = await _store.InsertSongAsync(request.Title, request.Artist, duration);
        await _store.InsertFingerprintsAsync(songId, fingerprints);

        return Created($"/api/songs/{songId}", new { songId });
    }
}
