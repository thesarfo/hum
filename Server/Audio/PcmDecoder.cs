using System.Buffers;

namespace Hum.Server.Audio;

public class PcmDecoder
{
    public float[] Decode(byte[] wavBytes)
    {
        if (wavBytes.Length < 44)
            throw new InvalidAudioDataException("WAV data is too short (minimum 44 bytes for header).");

        if (wavBytes[0] != 'R' || wavBytes[1] != 'I' || wavBytes[2] != 'F' || wavBytes[3] != 'F')
            throw new InvalidAudioDataException("Missing RIFF header.");

        if (wavBytes[8] != 'W' || wavBytes[9] != 'A' || wavBytes[10] != 'V' || wavBytes[11] != 'E')
            throw new InvalidAudioDataException("Missing WAVE format identifier.");

        ushort audioFormat = BitConverter.ToUInt16(wavBytes, 20);
        if (audioFormat != 1)
            throw new InvalidAudioDataException($"Unsupported audio format {audioFormat}. Only PCM (1) is supported.");

        ushort channels = BitConverter.ToUInt16(wavBytes, 22);
        if (channels != 1 && channels != 2)
            throw new InvalidAudioDataException($"Unsupported channel count {channels}. Only mono (1) and stereo (2) are supported.");

        ushort bitsPerSample = BitConverter.ToUInt16(wavBytes, 34);
        if (bitsPerSample != 16)
            throw new InvalidAudioDataException($"Unsupported bits per sample {bitsPerSample}. Only 16-bit PCM is supported.");

        int dataOffset = FindDataChunk(wavBytes);
        int dataSize = BitConverter.ToInt32(wavBytes, dataOffset + 4);
        int dataStart = dataOffset + 8;

        if (dataSize <= 0)
            return [];

        int availableBytes = wavBytes.Length - dataStart;
        int actualDataSize = Math.Min(dataSize, availableBytes);
        int sampleCount = actualDataSize / (bitsPerSample / 8);
        int frameCount = sampleCount / channels;

        float[] result = new float[frameCount];
        int sampleIndex = 0;

        if (channels == 1)
        {
            for (int i = 0; i < frameCount; i++)
            {
                short sample = BitConverter.ToInt16(wavBytes, dataStart + sampleIndex * 2);
                result[i] = sample / 32768.0f;
                sampleIndex++;
            }
        }
        else
        {
            for (int i = 0; i < frameCount; i++)
            {
                short left = BitConverter.ToInt16(wavBytes, dataStart + sampleIndex * 2);
                short right = BitConverter.ToInt16(wavBytes, dataStart + (sampleIndex + 1) * 2);
                result[i] = (left + right) / 65536.0f;
                sampleIndex += 2;
            }
        }

        return result;
    }

    private static int FindDataChunk(byte[] wavBytes)
    {
        int offset = 12;
        while (offset + 8 <= wavBytes.Length)
        {
            if (wavBytes[offset] == 'd' && wavBytes[offset + 1] == 'a'
                && wavBytes[offset + 2] == 't' && wavBytes[offset + 3] == 'a')
            {
                return offset;
            }
            int chunkSize = BitConverter.ToInt32(wavBytes, offset + 4);
            offset += 8 + chunkSize;
        }
        throw new InvalidAudioDataException("No data chunk found in WAV file.");
    }
}
