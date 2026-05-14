namespace Hum.Server.Audio;

public record Peak(int TimeFrame, int FrequencyBin, float Magnitude);

public class PeakPicker
{
    public const int DefaultNeighbourhoodSize = 20;
    public const float DefaultMagnitudeThreshold = 0.1f;
    public const int DefaultMaxPeaksPerSecond = 20;
    public const int BoundaryFrameExclusion = 5;

    private readonly int _neighbourhoodSize;
    private readonly float _magnitudeThreshold;
    private readonly int _maxPeaksPerSecond;
    private readonly int _boundaryFrameExclusion;

    public PeakPicker(IConfiguration config)
    {
        _neighbourhoodSize      = config.GetValue<int>  ("Fingerprinting:NeighbourhoodSize",      DefaultNeighbourhoodSize);
        _magnitudeThreshold     = config.GetValue<float>("Fingerprinting:MagnitudeThreshold",     DefaultMagnitudeThreshold);
        _maxPeaksPerSecond      = config.GetValue<int>  ("Fingerprinting:MaxPeaksPerSecond",      DefaultMaxPeaksPerSecond);
        _boundaryFrameExclusion = config.GetValue<int>  ("Fingerprinting:BoundaryFrameExclusion", BoundaryFrameExclusion);
    }

    public List<Peak> Pick(float[][] spectrogram, int sampleRate, int hopSize)
    {
        int numFrames = spectrogram.Length;
        int numBins = spectrogram.Length > 0 ? spectrogram[0].Length : 0;

        if (numFrames == 0 || numBins == 0)
            return [];

        int halfNeighbourhood = _neighbourhoodSize / 2;
        List<Peak> peaks = [];

        int startFrame = _boundaryFrameExclusion;
        int endFrame = numFrames - _boundaryFrameExclusion;

        for (int t = startFrame; t < endFrame; t++)
        {
            int tMin = Math.Max(0, t - halfNeighbourhood);
            int tMax = Math.Min(numFrames - 1, t + halfNeighbourhood);

            for (int f = 0; f < numBins; f++)
            {
                float val = spectrogram[t][f];
                if (val < _magnitudeThreshold)
                    continue;

                int fMin = Math.Max(0, f - halfNeighbourhood);
                int fMax = Math.Min(numBins - 1, f + halfNeighbourhood);

                bool isPeak = true;
                for (int nt = tMin; nt <= tMax && isPeak; nt++)
                {
                    for (int nf = fMin; nf <= fMax && isPeak; nf++)
                    {
                        if (spectrogram[nt][nf] > val)
                            isPeak = false;
                    }
                }

                if (isPeak)
                    peaks.Add(new Peak(t, f, val));
            }
        }

        int maxPeaks = _maxPeaksPerSecond * numFrames * hopSize / sampleRate;
        if (peaks.Count > maxPeaks)
        {
            peaks.Sort((a, b) => b.Magnitude.CompareTo(a.Magnitude));
            peaks = peaks.Take(maxPeaks).ToList();
        }

        return peaks;
    }
}
