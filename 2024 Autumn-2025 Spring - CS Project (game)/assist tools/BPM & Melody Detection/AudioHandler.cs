using NAudio.Wave;

namespace Assist;

public partial class AudioHandler
{
    AudioFileReader reader;
    WaveOutEvent player = new();
    private long lastPlaybackPosition = 0;

    private List<float[]> sampleAmplitudes = [];

    private const bool DEBUG = true;

    /// <summary>
    /// The audio samples per second.
    /// </summary>
    public int SampleRate => reader.WaveFormat.SampleRate;

    /// <summary>
    /// The sample size (in bits).
    /// </summary>
    public int Depth => reader.WaveFormat.BitsPerSample;

    /// <summary>
    /// The channel count.
    /// </summary>
    public int Channels => reader.WaveFormat.Channels;

    /// <summary>
    /// The duration of the song (in milliseconds).
    /// </summary>
    public long Length => (long)((float)reader.Length / (Depth / 8) / SampleRate / Channels * 1000);

    /// <summary>
    /// The current time of the song (in milliseconds).
    /// </summary>
    public long CurrentTime => PlayingTime + lastPlaybackPosition;

    /// <summary>
    /// The time since the song played (in milliseconds).
    /// </summary>
    private long PlayingTime => (long)((double)player.GetPosition() / player.OutputWaveFormat.AverageBytesPerSecond * 1000);

    public int BufferSize => (int)reader.Length / (Depth / 8);

    public PlaybackState State => player.PlaybackState;

    public long StartTime
    {
        get => lastPlaybackPosition;
        set => lastPlaybackPosition = value;
    }

    public AudioHandler(string filePath)
    {
        reader = new(filePath);
        player.Init(reader);

        sampleAmplitudes = GetAmplitude();
    }
}