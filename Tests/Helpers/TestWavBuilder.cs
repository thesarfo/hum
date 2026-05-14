namespace Hum.Tests.Helpers;

public static class TestWavBuilder
{
    private static readonly double[] Frequencies = [440.0, 880.0, 1320.0, 2200.0, 3300.0];
    private const double AmplitudePerTone = 0.18;

    public static byte[] GenerateWav(int durationSeconds, int sampleRate = 44100)
    {
        int totalFrames = durationSeconds * sampleRate;
        int dataBytes   = totalFrames * 2;
        byte[] wav      = new byte[44 + dataBytes];
        WriteWavHeader(wav, sampleRate, 1, 16, dataBytes);

        int pos = 44;
        for (int i = 0; i < totalFrames; i++)
        {
            double t = (double)i / sampleRate;
            double sample = 0.0;
            foreach (double f in Frequencies)
                sample += AmplitudePerTone * Math.Sin(2.0 * Math.PI * f * t);
            sample = Math.Max(-1.0, Math.Min(1.0, sample));
            short s16 = (short)(sample * 32767.0);
            wav[pos++] = (byte)(s16 & 0xFF);
            wav[pos++] = (byte)((s16 >> 8) & 0xFF);
        }
        return wav;
    }

    public static byte[] SliceWav(byte[] wav, int startSeconds, int durationSeconds)
    {
        ushort channels      = BitConverter.ToUInt16(wav, 22);
        int    sampleRate    = BitConverter.ToInt32(wav,  24);
        ushort bitsPerSample = BitConverter.ToUInt16(wav, 34);
        int    bytesPerFrame = channels * (bitsPerSample / 8);

        int dataChunkOffset = FindDataChunkOffset(wav);
        int dataStart       = dataChunkOffset + 8;

        int startByte  = dataStart + startSeconds * sampleRate * bytesPerFrame;
        int sliceBytes = Math.Min(
            durationSeconds * sampleRate * bytesPerFrame,
            wav.Length - startByte);

        byte[] result = new byte[44 + sliceBytes];
        WriteWavHeader(result, sampleRate, channels, bitsPerSample, sliceBytes);
        Buffer.BlockCopy(wav, startByte, result, 44, sliceBytes);
        return result;
    }

    private static void WriteWavHeader(byte[] buf, int sampleRate, ushort channels,
        ushort bitsPerSample, int dataBytes)
    {
        int byteRate      = sampleRate * channels * bitsPerSample / 8;
        ushort blockAlign = (ushort)(channels * bitsPerSample / 8);

        buf[0]=(byte)'R'; buf[1]=(byte)'I'; buf[2]=(byte)'F'; buf[3]=(byte)'F';
        WriteI32(buf,  4, 36 + dataBytes);
        buf[8]=(byte)'W'; buf[9]=(byte)'A'; buf[10]=(byte)'V'; buf[11]=(byte)'E';
        buf[12]=(byte)'f'; buf[13]=(byte)'m'; buf[14]=(byte)'t'; buf[15]=(byte)' ';
        WriteI32(buf, 16, 16);
        WriteU16(buf, 20, 1);
        WriteU16(buf, 22, channels);
        WriteI32(buf, 24, sampleRate);
        WriteI32(buf, 28, byteRate);
        WriteU16(buf, 32, blockAlign);
        WriteU16(buf, 34, bitsPerSample);
        buf[36]=(byte)'d'; buf[37]=(byte)'a'; buf[38]=(byte)'t'; buf[39]=(byte)'a';
        WriteI32(buf, 40, dataBytes);
    }

    private static int FindDataChunkOffset(byte[] wav)
    {
        int offset = 12;
        while (offset + 8 <= wav.Length)
        {
            if (wav[offset]=='d' && wav[offset+1]=='a' &&
                wav[offset+2]=='t' && wav[offset+3]=='a') return offset;
            int chunkSize = BitConverter.ToInt32(wav, offset + 4);
            offset += 8 + chunkSize;
        }
        throw new InvalidOperationException("No data chunk in source WAV.");
    }

    private static void WriteI32(byte[] b, int o, int v)
    {
        b[o]=(byte)(v&0xFF); b[o+1]=(byte)((v>>8)&0xFF);
        b[o+2]=(byte)((v>>16)&0xFF); b[o+3]=(byte)((v>>24)&0xFF);
    }

    private static void WriteU16(byte[] b, int o, ushort v)
    { b[o]=(byte)(v&0xFF); b[o+1]=(byte)((v>>8)&0xFF); }
}
