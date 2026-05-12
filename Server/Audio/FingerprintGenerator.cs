namespace Hum.Server.Audio;

/*
 * Hash bit layout (32-bit unsigned):
 *   bits 31-22: anchor frequency bin (10 bits, range 0-1023)
 *   bits 21-12: target frequency bin (10 bits, range 0-1023)
 *   bits 11-0:  time delta (12 bits, range 0-4095)
 */
public class FingerprintGenerator
{
    public const int FanOut = 5;
    public const int MinTimeDelta = 1;
    public const int MaxTimeDelta = 256;

    public List<(uint Hash, int TimeOffset)> Generate(List<Peak> peaks)
    {
        if (peaks.Count < 2)
            return [];

        var sorted = peaks.OrderBy(p => p.TimeFrame).ToList();
        var result = new List<(uint, int)>();

        for (int i = 0; i < sorted.Count; i++)
        {
            var anchor = sorted[i];
            int pairsAdded = 0;

            for (int j = i + 1; j < sorted.Count && pairsAdded < FanOut; j++)
            {
                var target = sorted[j];
                int delta = target.TimeFrame - anchor.TimeFrame;

                if (delta < MinTimeDelta)
                    continue;
                if (delta > MaxTimeDelta)
                    break;

                uint hash = ((uint)anchor.FrequencyBin & 0x3FF) << 22
                          | ((uint)target.FrequencyBin & 0x3FF) << 12
                          | ((uint)delta & 0xFFF);

                result.Add((hash, anchor.TimeFrame));
                pairsAdded++;
            }
        }

        return result;
    }
}
