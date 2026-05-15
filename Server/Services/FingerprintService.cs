using Hum.Server.Audio;
using Microsoft.Extensions.Logging;

namespace Hum.Server.Services;

public class FingerprintService
{
    private const int TargetSampleRate = 44100;

    private readonly PcmDecoder _decoder;
    private readonly SpectrogramBuilder _spectrogram;
    private readonly PeakPicker _peakPicker;
    private readonly FingerprintGenerator _generator;
    private readonly ILogger<FingerprintService> _logger;

    public FingerprintService(
        PcmDecoder decoder,
        SpectrogramBuilder spectrogram,
        PeakPicker peakPicker,
        FingerprintGenerator generator,
        ILogger<FingerprintService> logger)
    {
        _decoder = decoder;
        _spectrogram = spectrogram;
        _peakPicker = peakPicker;
        _generator = generator;
        _logger = logger;
    }

    public List<(uint Hash, int TimeOffset)> GenerateFingerprints(byte[] wavBytes)
    {
        var decoded = _decoder.Decode(wavBytes);
        int hopSize = _spectrogram.HopSize;

        float[] samples = decoded.SampleRate == TargetSampleRate
            ? decoded.Samples
            : Resampler.Resample(decoded.Samples, decoded.SampleRate, TargetSampleRate);

        float maxAmp = samples.Length > 0 ? samples.Max(Math.Abs) : 0f;
        if (maxAmp > 1e-6f)
        {
            float scale = 1.0f / maxAmp;
            for (int i = 0; i < samples.Length; i++)
                samples[i] *= scale;
            maxAmp = 1.0f;
        }
        _logger.LogInformation("FP: {Samples} samples, sampleRate={SR}, maxAmp={Amp:F4}",
            samples.Length, decoded.SampleRate, maxAmp);

        var spectrogram = _spectrogram.Build(samples);
        float maxMag = spectrogram.Length > 0 ? spectrogram.Max(f => f.Max()) : 0f;
        _logger.LogInformation("FP: {Frames} spectrogram frames, maxMagnitude={Mag:F4}",
            spectrogram.Length, maxMag);

        var peaks = _peakPicker.Pick(spectrogram, TargetSampleRate, hopSize);
        _logger.LogInformation("FP: {Peaks} peaks picked", peaks.Count);

        var fingerprints = _generator.Generate(peaks);
        _logger.LogInformation("FP: {FP} fingerprints generated", fingerprints.Count);
        return fingerprints;
    }
}
