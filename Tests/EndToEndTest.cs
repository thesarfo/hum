using Hum.Server.Audio;
using Hum.Server.Data;
using Hum.Server.Services;
using Hum.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Hum.Tests;

public class EndToEndTest
{
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fingerprinting:FftSize"]                = "1024",
                ["Fingerprinting:HopSize"]                = "512",
                ["Fingerprinting:NeighbourhoodSize"]      = "20",
                ["Fingerprinting:MagnitudeThreshold"]     = "0.1",
                ["Fingerprinting:MaxPeaksPerSecond"]      = "20",
                ["Fingerprinting:BoundaryFrameExclusion"] = "5",
                ["Fingerprinting:FanOut"]                 = "5",
                ["Fingerprinting:MinTimeDelta"]           = "1",
                ["Fingerprinting:MaxTimeDelta"]           = "256",
            })
            .Build();

    [Theory]
    [InlineData(5)]
    [InlineData(12)]
    [InlineData(20)]
    public async Task MatchAsync_ReturnsCorrectSong_ForSliceAtOffset(int offsetSeconds)
    {
        string dbPath     = Path.Combine(Path.GetTempPath(), $"hum-test-{Guid.NewGuid()}.db");
        string connString = dbPath;

        try
        {
            new DbInitializer(connString).Initialize();

            var config      = BuildConfig();
            var decoder     = new PcmDecoder();
            var spectrogram = new SpectrogramBuilder(config);
            var peakPicker  = new PeakPicker(config);
            var generator   = new FingerprintGenerator(config);
            var fpService   = new FingerprintService(decoder, spectrogram, peakPicker, generator, NullLogger<FingerprintService>.Instance);
            var store       = new FingerprintStore(connString);
            var matcher     = new MatcherService(store, fpService, NullLogger<MatcherService>.Instance);

            const string title  = "Synthetic Test Song";
            const string artist = "Test Artist";

            byte[] fullWav = TestWavBuilder.GenerateWav(30);

            var fingerprints = fpService.GenerateFingerprints(fullWav);
            Assert.NotEmpty(fingerprints);

            int songId = await store.InsertSongAsync(title, artist, duration: 30.0);
            await store.InsertFingerprintsAsync(songId, fingerprints);

            byte[] slice = TestWavBuilder.SliceWav(fullWav, offsetSeconds, 5);
            var result   = await matcher.MatchAsync(slice, minConfidence: 5);

            Assert.NotNull(result);
            Assert.Equal(title,  result.Title);
            Assert.Equal(artist, result.Artist);
        }
        finally
        {
            TryDelete(dbPath);
            TryDelete(dbPath + "-wal");
            TryDelete(dbPath + "-shm");
        }
    }

    private static void TryDelete(string p)
    { try { if (File.Exists(p)) File.Delete(p); } catch { } }
}
