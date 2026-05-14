using System.Net.Http.Json;
using Hum.Shared.Models;

namespace Hum.Client.Services;

public class SongService
{
    private readonly HttpClient _http;

    public SongService(HttpClient http)
    {
        _http = http;
    }

    public async Task<int> IngestSongAsync(string title, string artist, string wavBase64)
    {
        var response = await _http.PostAsJsonAsync("/api/songs/ingest", new
        {
            title,
            artist,
            wavBase64
        });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new HttpRequestException(error?.error ?? "Ingest failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<IngestResponse>();
        return result?.songId ?? throw new HttpRequestException("Invalid ingest response.");
    }

    public async Task<MatchResultDto?> MatchSnippetAsync(string wavBase64)
    {
        var response = await _http.PostAsJsonAsync("/api/fingerprint/match", new
        {
            wavBase64
        });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new HttpRequestException(error?.error ?? "Match request failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<MatchResponse>();
        if (result == null || !result.matched)
            return null;

        return result.result;
    }

    private record IngestResponse(int songId);
    private record MatchResponse(bool matched, MatchResultDto? result);
    private record ErrorResponse(string error);
}
