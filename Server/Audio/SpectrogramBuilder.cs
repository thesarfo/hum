using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace Hum.Server.Audio;

public class SpectrogramBuilder
{
    public const int DefaultFftSize = 1024;
    public const int DefaultHopSize = 512;

    public int FftSize { get; }
    public int HopSize { get; }

    public SpectrogramBuilder(IConfiguration config)
    {
        FftSize = config.GetValue<int>("Fingerprinting:FftSize", DefaultFftSize);
        HopSize = config.GetValue<int>("Fingerprinting:HopSize", DefaultHopSize);
    }

    public float[][] Build(float[] samples)
    {
        int fftSize = FftSize;
        int hopSize = HopSize;

        if (fftSize <= 0 || hopSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(fftSize), "FFT size and hop size must be positive.");

        int numFrames = samples.Length <= fftSize
            ? 1
            : 1 + (samples.Length - fftSize) / hopSize;

        int binCount = fftSize / 2 + 1;
        float[] window = ComputeHammingWindow(fftSize);

        float[][] spectrogram = new float[numFrames][];
        Complex[] frame = new Complex[fftSize];

        for (int frameIdx = 0; frameIdx < numFrames; frameIdx++)
        {
            int start = frameIdx * hopSize;
            Array.Clear(frame, 0, fftSize);

            int copyCount = Math.Min(fftSize, samples.Length - start);
            for (int i = 0; i < copyCount; i++)
                frame[i] = new Complex(samples[start + i] * window[i], 0);

            Fourier.Forward(frame, FourierOptions.NoScaling);

            float[] magnitudes = new float[binCount];
            for (int i = 0; i < binCount; i++)
                magnitudes[i] = (float)Math.Sqrt(frame[i].Real * frame[i].Real + frame[i].Imaginary * frame[i].Imaginary);

            spectrogram[frameIdx] = magnitudes;
        }

        return spectrogram;
    }

    private static float[] ComputeHammingWindow(int size)
    {
        float[] window = new float[size];
        for (int i = 0; i < size; i++)
            window[i] = 0.54f - 0.46f * MathF.Cos(2.0f * MathF.PI * i / (size - 1));
        return window;
    }
}
