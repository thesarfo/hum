using Hum.Server.Data;
using Hum.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Hum.Server.Services;

public class MatcherService
{
    private readonly FingerprintStore _store;
    private readonly FingerprintService _fingerprintService;
    private readonly ILogger<MatcherService> _logger;

    public MatcherService(FingerprintStore store, FingerprintService fingerprintService, ILogger<MatcherService> logger)
    {
        _store = store;
        _fingerprintService = fingerprintService;
        _logger = logger;
    }

    public async Task<MatchResultDto?> MatchAsync(byte[] wavBytes, int minConfidence)
    {
        var queryFingerprints = _fingerprintService.GenerateFingerprints(wavBytes);
        _logger.LogInformation("Match: {Count} query fingerprints generated", queryFingerprints.Count);
        if (queryFingerprints.Count == 0)
            return null;

        var histogram = new Dictionary<int, Dictionary<int, int>>();
        int totalHits = 0;

        foreach (var (hash, queryOffset) in queryFingerprints)
        {
            var matches = await _store.LookupHashAsync(hash);
            totalHits += matches.Count;

            foreach (var (songId, dbOffset) in matches)
            {
                int delta = dbOffset - queryOffset;

                if (!histogram.TryGetValue(songId, out var deltas))
                {
                    deltas = new Dictionary<int, int>();
                    histogram[songId] = deltas;
                }

                deltas.TryGetValue(delta, out int count);
                deltas[delta] = count + 1;
            }
        }

        int bestSongId = 0;
        int bestCount = 0;

        foreach (var (songId, deltas) in histogram)
        {
            foreach (var (delta, count) in deltas)
            {
                if (count > bestCount)
                {
                    bestCount = count;
                    bestSongId = songId;
                }
            }
        }

        _logger.LogInformation("Match: {Hits} total hash hits, best score={Best} (need {Min})", totalHits, bestCount, minConfidence);

        if (bestCount < minConfidence)
            return null;

        var song = await _store.GetSongAsync(bestSongId);
        if (song == null)
            return null;

        return new MatchResultDto
        {
            Title = song.Title,
            Artist = song.Artist,
            Confidence = bestCount
        };
    }
}
