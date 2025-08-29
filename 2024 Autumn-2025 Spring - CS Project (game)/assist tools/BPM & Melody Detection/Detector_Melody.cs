using NAudio.Codecs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assist;

public partial class AudioHandler
{

    const int KICK_MIN_FREQ = 30;
    const int KICK_MAX_FREQ = 80;
    const int BASS_MAX_FREQ = 200;
    const int MID_MIN_FREQ = 240;
    const int MID_MAX_FREQ = 2400;
    const int HI_MAX_FREQ = 6000;
    const int HAT_MAX_FREQ = 8000;

    void PickMax(List<float> spectrum, int maxof)
    {
        // compute onsets
        var temp = spectrum.ToList();
        List<int> top_id = [];
        for (int j = 0; j < maxof; j++)
        {
            top_id.Add(PopMax(temp));
        }

        for (int j = 0; j < spectrum.Count; j++)
        {
            if (!top_id.Contains(j))
            {
                spectrum[j] = 0;
            }
        }
    }

    int PopMax(List<float> arr)
    {
        int id = arr.IndexOf(arr.Max());
        arr.Remove(arr.Max());
        return id;
    }

    List<double> GetWeightedEnergy(List<List<float>> spectrum)
    {
        List<double> energy = [];
        foreach (var time in spectrum)
        {
            double sum = 0;
            for (int freq = 0; freq < time.Count; freq++)
            {
                sum += time[freq] * time[freq] * freq;
            }
            energy.Add(sum);
        }
        return energy;
    }

    List<double> GetRatio(List<double> energy)
    {
        List<double> ratio = [];
        Queue<double> history = new(new double[HistoryBufferSize]);
        foreach (var eng in energy)
        {
            // enqueue
            history.Enqueue(eng);
            history.Dequeue();

            // compare average
            double avg = history.Average();
            ratio.Add(eng / avg);
        }
        return ratio;
    }

    void FilterRatio(List<double> ratio, float sensitivity)
    {
        for (int i = 0; i < ratio.Count; i++)
        {
            if (ratio[i] < sensitivity)
            {
                ratio[i] = 0;
            }
        }
        Decluster(ratio);
    }

    List<List<float>> GetSemitoneSpectrum(List<List<float>> spectrum, float min_freq, float max_freq, float freq_gap)
    {
        List<List<float>> semitone_spectrum = [];
        foreach (var time in spectrum)
        {
            // get harmonic sum spectrum
            var harmonics = time;

            int indexShift = (int)(min_freq / freq_gap);
            int minSemitone = ToneIndex(min_freq) + 1;
            int maxSemitone = ToneIndex(max_freq) - 1;
            List<float> s = [];

            for (int i = minSemitone; i < maxSemitone; i++)
            {
                int low_bound = (int)Math.Max(0, (ToneFrequency(i - 1) / freq_gap) - indexShift);
                int hi_bound = (int)Math.Min(harmonics.Count - 1, (ToneFrequency(i + 1) / freq_gap) - indexShift);
                double rms = 0;
                for (int j = low_bound; j <= hi_bound; j++)
                {
                    rms += harmonics[j] * harmonics[j] * SemitoneWindow(i, (j + indexShift) * freq_gap);
                }
                rms = Math.Sqrt(rms);

                s.Add((float)rms);
            }
            semitone_spectrum.Add(s);
        }
        SPECTROGRAM(semitone_spectrum, "DEBUG__SEMI.txt");
        return semitone_spectrum;
    }

    /// <summary>
    /// Find the onsets of a given spectrogram.
    /// </summary>
    /// <param name="band">The target spectrogram</param>
    /// <param name="min_freq">The minimum frequency of the spectrogram</param>
    /// <param name="max_freq">The maximum frequency of the spectrogram</param>
    /// <param name="freq_gap">The difference of two consecutive frequencies</param>
    /// <param name="sensitivity">The threshold of loudness that can be recognized as onset</param>
    /// <returns>The list of onsets</returns>
    List<double> BandOnsets(List<List<float>> band, float min_freq, float max_freq, float freq_gap, float sensitivity = 0.01f)
    {
        var semitone_spectrum = GetSemitoneSpectrum(band, min_freq, max_freq, freq_gap);

        // remove last 2/3 of the frequency components

        for (int i = 0; i < semitone_spectrum.Count; i++)
        {
            List<int> index = new(Enumerable.Range(0, semitone_spectrum[i].Count));
            index = index.OrderByDescending(x => semitone_spectrum[i][x]).ToList()[..(semitone_spectrum[i].Count * 2 / 3)];
            for (int j = 0; j < semitone_spectrum[i].Count; j++)
            {
                if (!index.Contains(j))
                {
                    semitone_spectrum[i][j] = 0;
                }
            }
        }

        // to first order derivative
        List<List<float>> diff_spectrum = [];
        for (int i = 1; i < semitone_spectrum.Count; i++)
        {
            List<float> s = [];
            for (int j = 0; j < semitone_spectrum[i].Count; j++)
            {
                s.Add(Math.Max(semitone_spectrum[i][j] - semitone_spectrum[i - 1][j], 0));
            }
            diff_spectrum.Add(s);
        }

        // sum up band values
        List<float> sum_semitone = [];
        for (int i = 0; i < diff_spectrum.Count; i++)
        {
            var sum = semitone_spectrum[i + 1].Sum();
            sum_semitone.Add(diff_spectrum[i].Sum() / sum);
           if (sum < sensitivity) sum_semitone[^1] = 0;
        }

        var temp = sum_semitone.ConvertAll(x => (double)x);
        //Decluster(temp);

        return temp;
    }

    /// <summary>
    /// Find the onsets of a given spectrogram.
    /// </summary>
    /// <param name="band">The target spectrogram</param>
    /// <param name="min_freq">The minimum frequency of the spectrogram</param>
    /// <param name="max_freq">The maximum frequency of the spectrogram</param>
    /// <param name="freq_gap">The difference of two consecutive frequencies</param>
    /// <param name="sensitivity">The threshold of loudness that can be recognized as onset</param>
    /// <param name="timeframes">The length of samples to measure</param>
    /// <returns>The list of onsets</returns>
    List<double> BandSoftOnsets(List<List<float>> band, float min_freq, float max_freq, float freq_gap, float sensitivity = 0.01f, int timeframes = 2)
    {
        var semitone_spectrum = GetSemitoneSpectrum(band, min_freq, max_freq, freq_gap);

        // to weighted differenceS
        List<List<float>> diff_spectrum = [];
        for (int i = 0; i < semitone_spectrum.Count; i++)
        {
            List<float> s = [];
            for (int j = 0; j < semitone_spectrum[i].Count; j++)
            {
                float sum = 0;
                for (int k = 1; k <= timeframes; k++)
                {
                    float lvalue = 0, rvalue = 0;
                    if (i - k >= 0)
                    {
                        rvalue = semitone_spectrum[i - k][j];
                    }
                    if (i + k < semitone_spectrum.Count)
                    {
                        lvalue = semitone_spectrum[i + k][j];
                    }
                    sum += k * (lvalue - rvalue);
                }
                s.Add(Math.Max(sum, 0));
            }
            diff_spectrum.Add(s);
        }

        // sum up band values
        List<float> sum_semitone = [];
        for (int i = 0; i < diff_spectrum.Count; i++)
        {
            float sum = 0;
            for (int j = 1; j <= timeframes; j++)
            {
                if (i + j < semitone_spectrum.Count)
                {
                    sum += j * semitone_spectrum[i + j].Sum();
                }
            }
            sum_semitone.Add(diff_spectrum[i].Sum() / sum);
            if (sum < sensitivity || sum_semitone[^1] < 0.2) sum_semitone[^1] = 0;
        }

        var temp = sum_semitone.ConvertAll(x => (double)x);
        Decluster(temp);

        return temp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="freq">The frequency spectrum of a sample</param>
    /// <param name="minFreq">The minimum frequency of the spectrum</param>
    List<float> HarmonicSum(List<float> freq, float minFreq, float freqGap, float weightRate = 0.84f)
    {
        // copy a list as the base spectrum
        List<float> baseSpectrum = new List<float>(freq);
        // multiply by human audio sensitivity function
        for (int i = 0; i < baseSpectrum.Count; i++)
        {
            float f = minFreq + i * freqGap;
            baseSpectrum[i] *= HearingSensitivity(f);
        }
        // construct harmonic spectrum
        List<float> harmonicSpectrum = new(new float[freq.Count]);
        for (int i = 0; i < harmonicSpectrum.Count; i++)
        {
            // sum up 15 harmonics
            float sum = 0;
            float base_freq = minFreq + i * freqGap;
            for (int j = 0; j < 15; j++)
            {
                int index = (int)(base_freq * (j + 1) / freqGap - minFreq);
                // check if out of bound
                if (index >= baseSpectrum.Count || index < 0) break;
                sum += (float)(baseSpectrum[index] * Math.Pow(weightRate, j));
            }
            harmonicSpectrum[i] = sum;
        }
        return harmonicSpectrum;
    }

    public void GetMainMelodyOnset()
    {
        var amp = GetMono();
        int rate = SampleRate / 2;
        int window = 2048;
        float freqGap = (float)rate / window / 4;

        amp = Downsample(amp, SampleRate, rate);
        Console.WriteLine("Downsampled");
        // DEBUG
        var spectrum = PerformSTFT(amp.ToList(), window, InstantSize / 2, true, 3);
        Console.WriteLine("Specturm Generated");
        //var processed_spectrum = UnblurSpectrogram(spectrum, window / InstantSize);
        // (assuming this works)
        // (kinda not)

        //SPECTROGRAM(spectrum, "DEBUG__SPECTROGRAM.txt");

        // split into 5 bands (kick, bass, mid, high, hat)
        int kick_min_id = (int)(KICK_MIN_FREQ / freqGap);
        int kick_max_id = (int)(KICK_MAX_FREQ / freqGap);
        int bass_max_id = (int)(BASS_MAX_FREQ / freqGap);
        int mid_min_id = (int)(MID_MIN_FREQ / freqGap);
        int mid_max_id = (int)(MID_MAX_FREQ / freqGap);
        int hi_max_id = (int)(HI_MAX_FREQ / freqGap);
        int hat_max_id = (int)(HAT_MAX_FREQ / freqGap);

        var melody_band = Transpose(Transpose(spectrum)[bass_max_id..mid_max_id]);

        // to semitone band
        var lead_onset = BandOnsets(melody_band, BASS_MAX_FREQ, MID_MAX_FREQ, freqGap);

        //var lead_spectrum = GetSemitoneSpectrum(spectrum, KICK_MAX_FREQ, MID_MAX_FREQ, freqGap);
        //SPECTROGRAM(lead_spectrum, "DEBUG_SPECTROGRAM_SEMITONE.txt");

        // estimate bpm
        Console.WriteLine("Estimating BPM");
        var bpmOffset = BPMDetect_Freq();
        float bpm = bpmOffset[0];
        float offset = bpmOffset[1];

        float timeDiff = 60 / bpm;

        // autocorrelation test
        var corr = Autocorrelation(lead_onset, timeDiff / TimePerInstant);

        // convert to short-time deviation
        List<double> dev = [];
        // (13 is arbitrary, I randomly pick a small prime number)
        Queue<double> historyBuffer = new(new double[13]);
        for (int i = 0; i < corr.Count; i++)
        {
            historyBuffer.Enqueue(corr[i]);
            historyBuffer.Dequeue();
            var avg = historyBuffer.Average();

            dev.Add(corr[i] - avg);
        }
        _DEBUG_(dev, "DEBUG_MELODY_DEV.txt");

        // find the best beat gap
        int len = dev.Count;
        Dictionary<float, double[]> gapWeight = []; // key = gap, value = [score, offset]
        for (int gap = 4; gap <= 16; gap++)
        {
            double score = 0;

            // for every offsets
            double bestOffsetScore = 0;
            int bestOffset = 0;
            for (int o = 0; o < gap; o++)
            {
                int count = 0;
                for (int i = o; i < len; i += gap)
                {
                    score += dev[i];
                    count++;
                }
                score /= count;
                if (score > bestOffsetScore)
                {
                    bestOffsetScore = score;
                    bestOffset = o;
                }
            }
            gapWeight[gap] = [score, bestOffset];
        }

        // pick max gap score
        var bestGap = gapWeight.MaxBy(x => x.Value[0]);
        Console.WriteLine("Best beat gap = " + bestGap.Key);

        foreach (var g in gapWeight)
        {
            stringBuilder.AppendLine(g.Value[0].ToString());
        }
        File.WriteAllText("DEBUG__MELODY_GAP.txt", stringBuilder.ToString());
        stringBuilder.Clear();

        // group onsets
        int offsetIndex = SecondToIndex(offset);
        List<List<double>> melody_chunks = [];
        int chunkID = 0;
        while ((chunkID + 1) * bestGap.Key * timeDiff / TimePerInstant + offsetIndex < lead_onset.Count)
        {
            int lowerChunkID = (int)(chunkID * bestGap.Key * timeDiff / TimePerInstant) + offsetIndex;
            int upperChunkID = (int)((chunkID + 1) * bestGap.Key * timeDiff / TimePerInstant) + offsetIndex;
            melody_chunks.Add(lead_onset[lowerChunkID..upperChunkID]);
            chunkID++;
        }
        Console.WriteLine(melody_chunks.Count + " Chunks");

        // Build correlation matrix
        List<List<double>> correlation_matrix = []; 
        // initialize
        for (int i = 0; i < melody_chunks.Count; i++)
        {
            correlation_matrix.Add(Enumerable.Repeat(0.0, melody_chunks.Count).ToList());
        }
        for (int i = 0; i < melody_chunks.Count; i++)
        {
            for (int j = 0; j < melody_chunks.Count; j++)
            {
                if (correlation_matrix[i][j] == 0 && correlation_matrix[j][i] == 0)
                {
                    var x = melody_chunks[i];
                    var y = melody_chunks[j];

                    if (x.Count != y.Count)
                    {
                        int diff = x.Count - y.Count;
                        if (diff > 0)
                        {
                            x = x[..^diff];
                        }
                        else
                        {
                            y = y[..^(-diff)];
                        }
                    }

                    var x_mean = x.Average();
                    var y_mean = y.Average();

                    // standard deviation
                    double x_stddev = 0;
                    double y_stddev = 0;
                    for (int k = 0; k < x.Count; k++)
                    {
                        x_stddev += (x[k] - x_mean) * (x[k] - x_mean);
                        y_stddev += (y[k] - y_mean) * (y[k] - y_mean);
                    }

                    // covariance
                    double sum = 0;
                    for (int k = 0; k < x.Count; k++)
                    {
                        sum += (x[k] - x_mean) * (y[k] - y_mean);
                    }
                    sum /= Math.Sqrt(x_stddev * y_stddev);

                    correlation_matrix[i][j] = sum;
                    correlation_matrix[j][i] = sum;
                }
            }
        }
        // Test
        SPECTROGRAM(correlation_matrix, "DEBUG__COVMAT.txt");

        // ----------------------------- //
        // Apply Hierarchical Clustering //
        // ----------------------------- //
        Console.WriteLine("Perform Hierarchical Clustering");
        List<List<int>> clusters = [];

        // 1) initialize with N clusters
        for (int i = 0; i < melody_chunks.Count; i++)
        {
            clusters.Add(Enumerable.Repeat(i, 1).ToList());
        }

        double maxDistTolerance = 0.3;
        bool hasMerge = true;
        while (hasMerge && clusters.Count > 1)
        {
            hasMerge = false;
            // 2) apply single linkage to find the closest two clusters
            int targetClusterA = 0, targetClusterB = 1;
            double minDist = double.MaxValue;
            // check every cluster distances (i, j)
            for (int i = 0; i < clusters.Count - 1; i++)
            {
                for (int j = i + 1; j < clusters.Count; j++)
                {
                    // compute shortest distance for two clusters (a, b)
                    double dist = double.MaxValue;
                    for (int a = 0; a < clusters[i].Count; a++)
                    {
                        for (int b = 0; b < clusters[j].Count; b++)
                        {
                            double value = 1 - Math.Abs(correlation_matrix[clusters[i][a]][clusters[j][b]]);
                            if (value < dist)
                            {
                                dist = value;
                            }
                        }
                    }

                    if (dist < minDist)
                    {
                        minDist = dist;
                        targetClusterA = i;
                        targetClusterB = j;
                    }
                }
            }
            // check if the distance of selected clusters is small enough
            if (minDist <= maxDistTolerance)
            {
                Console.WriteLine("Merging Cluster " + targetClusterA + " and " + targetClusterB + " (dist = " + minDist + ")");

                hasMerge = true;
                clusters[targetClusterA].AddRange(clusters[targetClusterB]);
                clusters.RemoveAt(targetClusterB);
            }
        }
        Console.WriteLine("Total Clusters: " + clusters.Count);

        // DEBUG FOR CLUSTER VISUALIZATION
        List<int> order = [];
        foreach (var cluster in clusters)
        {
            order.AddRange(cluster);
        }
        List<List<double>> clustered_cormat = [];
        for (int i = 0; i < melody_chunks.Count; i++)
        {
            List<double> correlations = [];
            for (int j = 0; j < melody_chunks.Count; j++)
            {
                correlations.Add(correlation_matrix[order[i]][order[j]]);
            }
            clustered_cormat.Add(correlations);
        }

        // Test
        SPECTROGRAM(clustered_cormat, "DEBUG__COVMAT_CLUSTER.txt");


        // compare with average
        historyBuffer = new(new double[HistoryBufferSize]);
        for (int i = 0; i < lead_onset.Count; i++)
        {
            // add to history
            historyBuffer.Enqueue(lead_onset[i]);
            historyBuffer.Dequeue();
            var avg = historyBuffer.Average();

            // compare instant & average
            if (lead_onset[i] < avg * 2.0)
            {
                lead_onset[i] = 0;
            }
        }
        Decluster(lead_onset);

        List<List<double>> onset_chunks = [];
        int maxLen = 0;
        chunkID = 0;
        while ((chunkID + 1) * bestGap.Key * timeDiff / TimePerInstant + offsetIndex < lead_onset.Count)
        {
            int lowerChunkID = (int)(chunkID * bestGap.Key * timeDiff / TimePerInstant) + offsetIndex;
            int upperChunkID = (int)((chunkID + 1) * bestGap.Key * timeDiff / TimePerInstant) + offsetIndex;
            onset_chunks.Add(lead_onset[lowerChunkID..upperChunkID]);
            chunkID++;

            if (onset_chunks[^1].Count > maxLen)
            {
                maxLen = onset_chunks[^1].Count;
            }
        }

        // for each cluster, compare the onset overlaps
        List<List<int>> onsets = [];
        int n = 0;
        foreach (var cluster in clusters)
        {
            List<int> counter = new(new int[maxLen]);
            List<int> onset = new(new int[onset_chunks[n].Count]);
            foreach (int id in cluster)
            {
                for (int i = 0; i < onset_chunks[id].Count; i++)
                {
                    if (onset_chunks[id][i] > 0)
                    {
                        counter[i]++;
                    }
                }
            }
            for (int i = 0; i < counter.Count; i++)
            {
                int lower = Math.Max(i - 1, 0);
                int upper = Math.Min(i + 1, counter.Count - 1);
                int sum = 0;
                for (int j = lower; j <= upper; j++)
                {
                    sum += counter[j];
                }
                if (sum > cluster.Count / 1.5)
                {
                    for (int j = lower; j <= upper; j++)
                    {
                       counter[j] = 0;
                    }
                    if (i < onset.Count)
                    {
                        onset[i] = 1;
                    }
                }
            }
            onsets.Add(onset);
            n++;
        }

        // construct new onset list
        List<int> onset_post = [];
        for (int i = 0; i < onset_chunks.Count; i++)
        {
            // find the cluster for the chunk
            for (int j = 0; j < clusters.Count; j++)
            {
                if (clusters[j].Contains(i))
                {
                    onset_post.AddRange(onsets[j]);
                    break;
                }
            }
        }
        _DEBUG_(onset_post, "DEBUG__TEST.txt");

        // contruct potential melody locations
        // this includes every 1/4 and 1/3 of beats 
        
        List<bool> melody_pattern = new(new bool[onset_post.Count]);
        for (float time = offset; SecondToIndex(time) < onset_post.Count; time += timeDiff)
        {
            for (int i = 0; i < 4; i++)
            {
                int id = SecondToIndex(time + timeDiff / 4 * i);
                if (id < onset_post.Count)
                {
                    melody_pattern[id] = true;
                }
            }
            for (int i = 1; i < 3; i++)
            {
                int id = SecondToIndex(time + timeDiff / 3 * i);
                if (id < onset_post.Count)
                {
                    melody_pattern[id] = true;
                }
            }
        }
        for (int i = 0; i < onset_post.Count; i++)
        {
            int id_lower = Math.Max(i - 1, 0);
            int id_upper = Math.Max(i + 1, onset_post.Count - 1);
            for (int j = id_lower; j <= id_upper; j++)
            {
                if (onset_post[i] > 0 && melody_pattern[j]) break;
                if (j == id_upper) onset_post[i] = 0;
            }
        }


        Console.WriteLine("Finished processing. Writing files...");
        //SPECTROGRAM(semitone_spectrum, "DEBUG_SPECTROGRAM_SEMI.txt");
        //SPECTROGRAM(diff_spectrum, "DEBUG_SPECTROGRAM_SEMI_DIFF.txt");

        //SPECTROGRAM(lead_spectrum, "DEBUG_SPECTROGRAM_AVG.txt");

        _DEBUG_(lead_onset, "DEBUG__LEAD_ENG.txt");

        //SPECTROGRAM(spectrum, "DEBUG_SPECTROGRAM_MAX.txt");
    }
}