using Hum.Server.Audio;

namespace Hum.Server.Services;

public class FingerprintService
{
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

        var spectrogram = _spectrogram.Build(decoded.Samples);
        var peaks = _peakPicker.Pick(spectrogram, decoded.SampleRate, hopSize);
        return _generator.Generate(peaks);
    }
}
