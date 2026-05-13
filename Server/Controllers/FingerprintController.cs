using Hum.Server.Models;
using Hum.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hum.Server.Controllers;

[ApiController]
[Route("api/fingerprint")]
public class FingerprintController : ControllerBase
{
    private readonly MatcherService _matcherService;
    private readonly IConfiguration _configuration;

    public FingerprintController(MatcherService matcherService, IConfiguration configuration)
    {
        _matcherService = matcherService;
        _configuration = configuration;
    }

    [HttpPost("match")]
    public async Task<IActionResult> Match([FromBody] MatchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.WavBase64))
            return BadRequest(new { error = "WavBase64 is required." });

        byte[] wavBytes;
        try
        {
            wavBytes = Convert.FromBase64String(request.WavBase64);
        }
        catch
        {
            return BadRequest(new { error = "WavBase64 could not be decoded." });
        }

        int minConfidence = _configuration.GetValue<int>("Matching:MinConfidence", 5);
        var result = await _matcherService.MatchAsync(wavBytes, minConfidence);

        if (result == null)
            return Ok(new { matched = false });

        return Ok(new
        {
            matched = true,
            result = new
            {
                title = result.Title,
                artist = result.Artist,
                confidence = result.Confidence
            }
        });
    }
}
