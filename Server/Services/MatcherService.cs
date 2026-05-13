using Hum.Server.Data;
using Hum.Shared.Models;

namespace Hum.Server.Services;

public class MatcherService
{
    private readonly FingerprintStore _store;
    private readonly FingerprintService _fingerprintService;

    public MatcherService(FingerprintStore store, FingerprintService fingerprintService)
    {
        _store = store;
        _fingerprintService = fingerprintService;
    }

    public async Task<MatchResultDto?> MatchAsync(byte[] wavBytes, int minConfidence)
    {
        var queryFingerprints = _fingerprintService.GenerateFingerprints(wavBytes);
        if (queryFingerprints.Count == 0)
            return null;

        var histogram = new Dictionary<int, Dictionary<int, int>>();

        foreach (var (hash, queryOffset) in queryFingerprints)
        {
            var matches = await _store.LookupHashAsync(hash);

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

        if (bestCount < minConfidence)
            return null;

        var song = await _store.GetSongAsync(bestSongId);
        if (song == null)
            return null;

        return new MatchResultDto
        {
            Title = song.Title,
            Artist = song.Artist,
            Confidence = (double)bestCount / queryFingerprints.Count
        };
    }
}
