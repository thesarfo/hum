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

    private readonly int _fanOut;
    private readonly int _minTimeDelta;
    private readonly int _maxTimeDelta;

    public FingerprintGenerator(IConfiguration config)
    {
        _fanOut       = config.GetValue<int>("Fingerprinting:FanOut",       FanOut);
        _minTimeDelta = config.GetValue<int>("Fingerprinting:MinTimeDelta", MinTimeDelta);
        _maxTimeDelta = config.GetValue<int>("Fingerprinting:MaxTimeDelta", MaxTimeDelta);
    }

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

            for (int j = i + 1; j < sorted.Count && pairsAdded < _fanOut; j++)
            {
                var target = sorted[j];
                int delta = target.TimeFrame - anchor.TimeFrame;

                if (delta < _minTimeDelta)
                    continue;
                if (delta > _maxTimeDelta)
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
