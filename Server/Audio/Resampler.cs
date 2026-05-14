namespace Hum.Server.Audio;

public static class Resampler
{
    public static float[] Resample(float[] samples, int sourceSampleRate, int targetSampleRate)
    {
        if (sourceSampleRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(sourceSampleRate));
        if (targetSampleRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetSampleRate));
        if (samples.Length == 0 || sourceSampleRate == targetSampleRate)
            return samples;

        double ratio = (double)sourceSampleRate / targetSampleRate;
        int outputLength = (int)(samples.Length / ratio);
        float[] output = new float[outputLength];

        for (int i = 0; i < outputLength; i++)
        {
            double srcPos = i * ratio;
            int lo = (int)srcPos;
            int hi = lo + 1;
            double frac = srcPos - lo;
            float hiSample = hi < samples.Length ? samples[hi] : samples[samples.Length - 1];
            output[i] = (float)(samples[lo] + frac * (hiSample - samples[lo]));
        }

        return output;
    }
}
