using Hum.Server.Audio;

namespace Hum.Server.Services;

public class FingerprintService
{
    // The pipeline is calibrated to 44 100 Hz. The JS side requests this via
    // AudioContext { sampleRate: 44100 }, but browsers may use the hardware rate
    // (e.g. 48 000 Hz) instead. Server-side resample catches that case.
    private const int TargetSampleRate = 44100;

    private readonly PcmDecoder _decoder;
    private readonly SpectrogramBuilder _spectrogram;
    private readonly PeakPicker _peakPicker;
    private readonly FingerprintGenerator _generator;

    public FingerprintService(
        PcmDecoder decoder,
        SpectrogramBuilder spectrogram,
        PeakPicker peakPicker,
        FingerprintGenerator generator)
    {
        _decoder = decoder;
        _spectrogram = spectrogram;
        _peakPicker = peakPicker;
        _generator = generator;
    }

    public List<(uint Hash, int TimeOffset)> GenerateFingerprints(byte[] wavBytes)
    {
        var decoded = _decoder.Decode(wavBytes);
        int hopSize = SpectrogramBuilder.DefaultHopSize;

        float[] samples = decoded.SampleRate == TargetSampleRate
            ? decoded.Samples
            : Resampler.Resample(decoded.Samples, decoded.SampleRate, TargetSampleRate);

        var spectrogram = _spectrogram.Build(samples);
        var peaks = _peakPicker.Pick(spectrogram, TargetSampleRate, hopSize);
        return _generator.Generate(peaks);
    }
}
