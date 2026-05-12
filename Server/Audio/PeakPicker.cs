namespace Hum.Server.Audio;

public record Peak(int TimeFrame, int FrequencyBin, float Magnitude);

public class PeakPicker
{
    public const int DefaultNeighbourhoodSize = 20;
    public const float DefaultMagnitudeThreshold = 0.1f;
    public const int DefaultMaxPeaksPerSecond = 20;
    public const int BoundaryFrameExclusion = 5;

    public List<Peak> Pick(
        float[][] spectrogram,
        int sampleRate,
        int hopSize,
        int neighbourhoodSize = DefaultNeighbourhoodSize,
        float magnitudeThreshold = DefaultMagnitudeThreshold,
        int maxPeaksPerSecond = DefaultMaxPeaksPerSecond)
    {
        int numFrames = spectrogram.Length;
        int numBins = spectrogram.Length > 0 ? spectrogram[0].Length : 0;

        if (numFrames == 0 || numBins == 0)
            return [];

        int halfNeighbourhood = neighbourhoodSize / 2;
        List<Peak> peaks = [];

        int startFrame = BoundaryFrameExclusion;
        int endFrame = numFrames - BoundaryFrameExclusion;

        for (int t = startFrame; t < endFrame; t++)
        {
            int tMin = Math.Max(0, t - halfNeighbourhood);
            int tMax = Math.Min(numFrames - 1, t + halfNeighbourhood);

            for (int f = 0; f < numBins; f++)
            {
                float val = spectrogram[t][f];
                if (val < magnitudeThreshold)
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

        int maxPeaks = maxPeaksPerSecond * numFrames * hopSize / sampleRate;
        if (peaks.Count > maxPeaks)
        {
            peaks.Sort((a, b) => b.Magnitude.CompareTo(a.Magnitude));
            peaks = peaks.Take(maxPeaks).ToList();
        }

        return peaks;
    }
}
