using System;

namespace Charting.Source.Assist;

public partial class AudioHandler {

    /// <summary>
    /// Play the song from the given time (in millisecond).
    /// </summary>
    /// <param name="ms"></param>
    public void PlaySong(double ms) {
        long tick = (long)(ms * 10000);
        player = new();
        reader.CurrentTime = new TimeSpan(tick);
        player.Init(reader);
        player.Play();
    }

    /// <summary>
    /// Play the song from the last position.
    /// </summary>
    public void PlaySong() {
        player.Play();
    }

    public void PauseSong() {
        lastPlaybackPosition = CurrentTime;
        player.Stop();
    }

    public void StopSong() {
        lastPlaybackPosition = 0;
        player.Stop();
        reader.CurrentTime = new(0);
    }

    public void SetVolume(float volume) {
        player.Volume = volume;
    }
}