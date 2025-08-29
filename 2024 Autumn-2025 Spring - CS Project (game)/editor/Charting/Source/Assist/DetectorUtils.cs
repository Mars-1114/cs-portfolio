using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Charting.Source.Assist;

// BPM Detector
public partial class AudioHandler
{
    // size
    const int WindowSize = 1 << 12; // 4096
    const int InstantSize = 1 << 9; // 512
    const float HistoryDuration = 0.5f; // in seconds

    // tolerance
    const float OnsetResolution = 1 / 15f;  // in seconds

    public double TimePerInstant => (double)InstantSize / SampleRate;
    int HistoryBufferSize => (int)(HistoryDuration / TimePerInstant);

    StringBuilder stringBuilder = new();

    /// <summary>
    /// Get the amplitude of the audio.
    /// </summary>
    /// <returns>The stereo amplitude of the audio. Index 0 = left, 1 = right</returns>
    List<float[]> GetAmplitude()
    {

        // read samples
        float[] sample = new float[BufferSize];
        reader.Position = 0;
        int bufferCount = reader.Read(sample, 0, BufferSize);

        // split into L/R channels
        float[] left = new float[bufferCount / 2];
        float[] right = new float[bufferCount / 2];
        for (int i = 0; i < left.Length; i++)
        {
            left[i] = sample[2 * i];
            right[i] = sample[2 * i + 1];
        }

        return [left, right];
    }
    /// <summary>
    /// Deep copy an existed sample amplitudes.
    /// </summary>
    /// <returns></returns>
    List<float[]> GetStereo()
    {
        return sampleAmplitudes.Select(x => (float[])x.Clone()).ToList();
    }

    float[] GetMono()
    {
        var amp = GetStereo();
        var mono = new float[amp[0].Length];
        for (int i = 0; i <  mono.Length; i++)
        {
            mono[i] = (amp[0][i] + amp[1][i]) / 2;
        }
        return mono;
    }

    void PerformLowpass(float[] samples, float cutOffFrequency)
    {
        var lowpass = BiQuadFilter.LowPassFilter(reader.WaveFormat.SampleRate, cutOffFrequency, 1);
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = lowpass.Transform(samples[i]);
        }
    }

    float[] Downsample(float[] samples, int fromRate, int toRate)
    {
        int factor = fromRate / toRate;

        if (factor <= 1)
            throw new ArgumentException("fromRate (" + fromRate + ") must be greater than toRate (" + toRate + ").");

        List<float> processed = [];
        PerformLowpass(samples, toRate);
        for (int i = 0; i < samples.Length / factor; i++)
        {
            processed.Add(samples[i * factor]);
        }
        return processed.ToArray();
    }

    List<List<float>> PerformSTFT(List<float> amplitude, int window, int hop, bool buffer_from_empty = false, int zero_padding_len = 0)
    {
        Queue<float> buffer = new(new float[window]);
        List<List<float>> spectrum = [];
        int segmentSize = window / hop;
        int i = 0;
        if (!buffer_from_empty)
        {
            i = segmentSize;
            buffer = new(amplitude[..window]);
        }
        for ( ; i < amplitude.Count / hop; i++)
        {
            var segment = amplitude[(i * hop)..((i + 1) * hop)];
            segment.ForEach(buffer.Enqueue);
            for (int j = 0; j < hop; j++) buffer.Dequeue();

            var temp = buffer.ToList();
            // zero padding
            temp.AddRange([.. new float[window * zero_padding_len]]);
            spectrum.Add(FFT_Abs(temp));
        }
        return spectrum;
    }
    List<float> FFT_Abs(List<float> x)
    {
        int size = x.Count;
        if ((int)Math.Log2(size) != Math.Log2(size))
            throw new NotSupportedException("Wrong Buffer Size (" + size + "). Must be the power of 2.");
        // Convert to complex number
        List<Complex> x_time = [];
        int i = 0;
        foreach (var x_k in x)
        {
            Complex c;
            double w = FastFourierTransform.HannWindow(i, size);
            c.X = (float)(x_k * w);
            c.Y = 0;

            x_time.Add(c);

            i++;
        }

        // FFT
        var x_freq_arr = x_time.ToArray();
        FastFourierTransform.FFT(true, (int)Math.Log2(size), x_freq_arr);

        // Compute absolute value
        List<float> x_freq = [];
        foreach (var freq in x_freq_arr)
        {
            float abs = (float)Math.Sqrt(freq.X * freq.X + freq.Y * freq.Y);
            x_freq.Add(abs);
        }
        return x_freq[..(size / 2)];
    }

    /// <summary>
    /// Clean up clustered onsets.
    /// </summary>
    /// <param name="onsets">The raw detected onsets</param>
    /// <returns></returns>
    void Decluster(List<double> onsets)
    {

        // take the first of the clustered ratio
        // DEFINITION: Onset = the moment when a beat or note occurs
        //             Resolution = the minimal time difference human can differentiate two onsets
        //                          Here we set the interval to 1/15 seconds
        int cluster_start_ID = 0;
        int cluster_last_ID = 0;
        int cluster_max_ID = 0;
        int max_gap = (int)Math.Round(OnsetResolution / TimePerInstant);
        for (int i = 0; i < onsets.Count; i++)
        {
            if (onsets[i] > 0)
            {
                // if next onset is within 0.05 seconds
                if (i - cluster_last_ID <= max_gap)
                {
                    // store the latest id
                    cluster_last_ID = i;
                    // update max value
                    if (onsets[cluster_max_ID] < onsets[i])
                    {
                        cluster_max_ID = i;
                    }
                }
                else
                {
                    onsets[cluster_start_ID] = onsets[cluster_max_ID];
                    for (int j = cluster_start_ID + 1; j <= cluster_last_ID; j++)
                    {
                        onsets[j] = 0;
                    }
                    cluster_start_ID = i;
                    cluster_max_ID = i;
                    cluster_last_ID = i;
                }
            }
        }
    }

    int SecondToIndex(double s, double time_per_instant = -1)
    {
        if (time_per_instant == -1) time_per_instant = TimePerInstant;
        return (int)Math.Round(s / time_per_instant);
    }

    List<List<float>> UnblurSpectrogram(List<List<float>> spectrogram, int windowSize)
    {
        List<List<float>> spectrum = [];
        var spec_transposed = Transpose(spectrogram);
        foreach (var freq in spec_transposed)
        {
            // for every frequency
            List<float> f = [];
            Queue<float> buffer = new(new float[windowSize]); // for new spectrogram

            // build kernel
            List<float> kernel = [];
            for (int i = 0; i < windowSize; i++)
            {
                kernel.Add(1 - (float)i / windowSize);
            }
            float s = kernel.Sum();
            for (int i = 0; i < windowSize; i++)
            {
                kernel[i] /= s;
            }

            foreach (var time in freq)
            {
                // compute the unblurred value
                float sum = 0;
                var temp_buffer = buffer.ToList();
                for (int i = 0; i < kernel.Count - 1; i++)
                {
                    sum += kernel[i] * temp_buffer[i];
                }
                float value = (time - sum) / kernel[^1];

                if (value < 0) value = 0;

                // push to buffer
                buffer.Enqueue(value);
                buffer.Dequeue();

                // add to spectrum
                f.Add(value);
            }
            spectrum.Add(f);
        }
        return Transpose(spectrum);
    }

    /// <summary>
    /// The frequency for a given semitone.
    /// For reference,
    /// <para>
    /// A0 (0) = 27.5 Hz <br/>
    /// E2 (19) = 82.4 Hz <br/>
    /// G3 (34) = 196.0 Hz <br/>
    /// B4 (50) = 493.9 Hz <br/>
    /// D7 (77) = 2439 Hz <br/>
    /// F#8 (93) = 5920 Hz <br/>
    /// </para>
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    float ToneFrequency(int i)
    {
        return (float)(Math.Pow(2, (double)i / 12) * 27.5);
    }
    int ToneIndex(float frequency)
    {
        return (int)(12 * Math.Log2(frequency / 27.5));
    }

    /// <summary>
    /// return a triangular scaled window for a given semitone and a frequency.
    /// </summary>
    /// <param name="semitone"></param>
    /// <param name="freq"></param>
    /// <returns></returns>
    double SemitoneWindow(int semitone, float freq)
    {
        var toneFreq = ToneFrequency(semitone);
        var prevToneFreq = ToneFrequency(semitone - 1);
        var nextToneFreq = ToneFrequency(semitone + 1);
        //prevToneFreq = (prevToneFreq + toneFreq) / 2;
        //nextToneFreq = (nextToneFreq + toneFreq) / 2;

        if (freq > toneFreq)
        {
            return (freq > nextToneFreq) ? 0 : 1 - (freq - toneFreq) / (nextToneFreq - toneFreq);
        }
        else
        {
            return (freq < prevToneFreq) ? 0 : 1 - (toneFreq - freq) / (toneFreq - prevToneFreq);
        }
    }

    List<List<float>> Transpose(List<List<float>> matrix)
    {
        int w = matrix.Count;
        int h = matrix[0].Count;

        List<List<float>> result = [];
        for (int i = 0; i < h; i++)
        {
            result.Add(new(new float[w]));
        }

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                result[j][i] = matrix[i][j];
            }
        }

        return result;
    }

    float HearingSensitivity(float freq)
    {
        if (freq < 20 || freq > 10000)
        {
            return 0;
        }
        else if (freq < 1000)
        {
            return (float)(Math.Atan(2 * Math.Log(freq / 50)) / 0.895 / Math.PI + 0.5);
        }
        else
        {
            double a = 5260;
            double b = 1.198;
            double c = 0.6;
            double x = c * Math.Log(freq / a);
            return (float)(-Math.Pow(x, 3) - 2.2 * Math.Pow(x, 2) - x + b);
        }
    }

    List<double> Autocorrelation(List<double> seq, double hop)
    {
        List<double> corr = [];
        // i = shifted beat
        for (int i = 1; i < seq.Count / hop; i++)
        {
            int shift = (int)Math.Round(i * hop);
            // multiply sample with shifted sample
            double sum = 0;
            // j = index
            for (int j = 0; j < seq.Count; j++)
            {
                if (j + shift >= seq.Count) {
                    sum /= j + 1;
                    break;
                }
                sum += seq[j] * seq[j + shift];
            }
            corr.Add(sum);
        }
        _DEBUG_(corr, "DEBUG__CORR.txt");
        return corr;
    }

    /// <summary>
    /// Write data to a specified text file.
    /// </summary>
    /// <typeparam name="T">float or double</typeparam>
    /// <param name="target">List of data</param>
    /// <param name="fileName"></param>
    void _DEBUG_<T>(List<T> target, string fileName)
    {
        if (DEBUG)
        {
            foreach (var t in target)
            {
                if (t is not null)
                {
                    stringBuilder.AppendLine(t.ToString());
                }
            }

            File.WriteAllText(fileName, stringBuilder.ToString());
            stringBuilder.Clear();
        }
    }
    /// <summary>
    /// Write data to a specified text file.
    /// </summary>
    /// <typeparam name="T">float or double</typeparam>
    /// <param name="target">Dictionary of data</param>
    /// <param name="fileName"></param>
    void _DEBUG_<T>(Dictionary<float, T> target, string fileName)
    {
        if (DEBUG)
        {
            foreach (var t in target)
            {
                if (t.Value is not null)
                {
                    stringBuilder.AppendLine(t.Value.ToString());
                }
            }

            File.WriteAllText(fileName, stringBuilder.ToString());
            stringBuilder.Clear();
        }
    }

    /// <summary>
    /// Write spectrogram data to a specified text file
    /// </summary>
    /// <param name="spectrum">The spectrum for a duration of time</param>
    /// <param name="fileName"></param>
    void SPECTROGRAM(List<List<float>> spectrum, string fileName)
    {
        if (DEBUG)
        {
            foreach (var time in spectrum)
            {
                string str = "";
                foreach (var freq in time)
                {
                    str += freq.ToString() + " ";
                }
                stringBuilder.AppendLine(str);
            }

            File.WriteAllText(fileName, stringBuilder.ToString());
            stringBuilder.Clear();
        }
    }
}