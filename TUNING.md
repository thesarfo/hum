# Fingerprint Tuning Log

All constants live in `Server/appsettings.json` under `"Fingerprinting"`. Change and restart, no recompile needed.

## Default Values

| Key | Default | Controls | Tune direction |
|-----|---------|----------|----------------|
| `FftSize` | 1024 | Frequency resolution of each frame. Must be a power of 2. | Higher → finer freq bins, slower. Lower → coarser, faster. |
| `HopSize` | 512 | Stride between frames (50% overlap at default). | Lower → more frames, denser fingerprint, slower. |
| `NeighbourhoodSize` | 20 | Time×freq window for local peak detection. | Higher → fewer, more prominent peaks. Lower → noisier peak set. |
| `MagnitudeThreshold` | 0.1 | Minimum FFT magnitude to qualify as a peak candidate. | Higher → fewer peaks (faster, may miss quiet songs). Lower → more peaks (slower, more false positives). |
| `MaxPeaksPerSecond` | 20 | Hard cap on peaks retained per second of audio. | Higher → denser fingerprint, larger DB. Lower → sparser, faster lookup. |
| `BoundaryFrameExclusion` | 5 | Frames excluded from peak picking at start and end. | Prevents edge artefacts; rarely needs changing. |
| `FanOut` | 5 | Number of target peaks paired with each anchor. | Higher → more hashes per anchor, better recall, larger DB. Lower → faster ingest. |
| `MinTimeDelta` | 1 | Minimum frame gap between anchor and target in a pair. | Keep ≥ 1. |
| `MaxTimeDelta` | 256 | Maximum frame gap between anchor and target in a pair. | At HopSize=512, 44100 Hz: 256 frames ≈ 3 s. Increase for longer-range pairs. |

## Constraints

- `FftSize` must be a power of 2 (MathNet.Numerics requirement).
- `HopSize` should be ≤ `FftSize`. 50% overlap (`HopSize = FftSize / 2`) is the standard starting point.
- `MaxTimeDelta` is stored in 12 bits in the hash (max 4095). Keep below 4096.
- `FanOut` affects DB size linearly. A 3-minute song at `FanOut=5` produces ~30k–80k hashes.

## Observations

| Date | FftSize | HopSize | Neighbourhood | Threshold | FanOut | MaxTimeDelta | Result |
|------|---------|---------|---------------|-----------|--------|--------------|--------|
| baseline | 1024 | 512 | 20 | 0.1 | 5 | 256 | defaults, not yet validated against 10+ songs |
