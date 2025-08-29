namespace Assist;

public partial class AudioHandler
{
    const float MIN_SONG_BPM = 60;
    const float MAX_SONG_BPM = 250;
    const float MIN_TEST_BPM = 30;
    const float MAX_TEST_BPM = 480;

    /// <summary>
    /// Estimate the BPM of the song.
    /// </summary>
    /// <param name="onsets">The detected onsets</param>
    /// <returns>The score of each BPM</returns>
    float[] GetBPM_Chain(List<double> onsets)
    {
        // METHOD 1: PULSE CHAIN
        //  This is a simple comparison algorithm. For any given BPM:
        //  1) Take the onset as the first beat
        //  2) Check if the next beat has another onset around
        //  3) If true, check the next beat; if false, take the chain length squared as the probability
        //  4) Add up the value for all onsets
        //  5) Repeat for every BPM and pick the highest score
        double differenceTolerance = 0.015; // in seconds
        int len = onsets.Count;
        Dictionary<float, double[]> bpmWeight = []; // key = bpm, value = [score, offset]
        for (float bpm = MIN_TEST_BPM; bpm <= MAX_TEST_BPM; bpm += 0.5f)
        {
            double probability = 0;
            double timeDiff = 60d / bpm;
            int IndexTolerance = (int)Math.Round(differenceTolerance / TimePerInstant);

            // for every offsets
            double bestOffsetScore = 0;
            double bestOffset = 0;
            for (double offset = 0; offset < timeDiff; offset += 0.05)
            {
                int chainLen = 0;
                for (double time = offset; SecondToIndex(time) < onsets.Count; time += timeDiff)
                {
                    int expectedBeatID_lower = Math.Max(SecondToIndex(time - differenceTolerance), 0);
                    int expectedBeatID_upper = Math.Min(SecondToIndex(time + differenceTolerance), len - 1);
                    bool hasMatch = false;
                    for (int checkID = expectedBeatID_lower; checkID < expectedBeatID_upper; checkID++)
                    {
                        if (onsets[checkID] > 0)
                        {
                            hasMatch = true;
                            break;
                        }
                    }
                    if (hasMatch)
                    {
                        chainLen++;
                    }
                    else
                    {
                        probability += Math.Pow((double)chainLen / len, 2);
                        chainLen = 0;
                    }
                }
                probability += Math.Pow((double)chainLen / len, 2);
                if (probability > bestOffsetScore)
                {
                    bestOffsetScore = probability;
                    bestOffset = offset;
                }
            }
            bpmWeight[bpm] = [probability, bestOffset];
        }

        // Pick the BPM with best fit (highest score)
        var candidateBPM = bpmWeight.MaxBy(x => x.Value[0]);

        // Scale the BPM within range
        float b = candidateBPM.Key;
        while (b > MAX_SONG_BPM)
        {
            if ((b == (int)b && b % 2 == 0))
            {
                b /= 2;
            }
            else
            {
                // find
                b = bpmWeight.MaxBy(x => (x.Key < b) ? x.Value[0] : 0).Key;
            }
        }
        if (b < MIN_SONG_BPM || (b != (int)b && b * 2 <= MAX_SONG_BPM))
        {
            b *= 2;
        }
        candidateBPM = new(b, bpmWeight[b]);

        // Compute Confidence Score

        // peak picking
        //  PEAK WIDTH | the distance to ignore other local maxima. Set to 0.03 seconds
        //  THRESHOLD | the minimum value to count as a peak from noise. Set to the mean of all bpm scores
        double threshold = bpmWeight.Average(x => x.Value[0]);
        List<float> peaks = [];

        float candidatePeak = bpmWeight.MinBy(x => x.Key).Key;
        float peakWidth = 0.03f;
        foreach (var bpm in bpmWeight)
        {
            // if above noise
            if (bpm.Value[0] >= threshold)
            {
                // if within current peak width
                if (Math.Abs(60 / candidatePeak - 60 / bpm.Key) <= peakWidth)
                {
                    // update if greater than current peak
                    if (bpmWeight[candidatePeak][0] < bpm.Value[0])
                    {
                        candidatePeak = bpm.Key;
                    }
                }
                else
                {
                    if (bpmWeight[candidatePeak][0] >= threshold)
                    {
                        peaks.Add(candidatePeak);
                    }
                    candidatePeak = bpm.Key;
                }
            }
        }
        if (bpmWeight[candidatePeak][0] >= threshold)
        {
            peaks.Add(candidatePeak);
        }

        // calculate confidence score
        // Method: find harmonics of the candidate bpm,
        //         then get the error squared of them (if exists)
        // Formula: sum(((closest_peak - BPM_harmonic_k) / peakWidth) ^ 2
        //
        //  HARMONICS | The BPM 2^k multiple of the base BPM (30~60)

        // find base BPM
        float bpm_base = candidateBPM.Key;
        while (bpm_base >= MIN_SONG_BPM)
        {
            bpm_base /= 2;
        }

        // compute error
        int harmonic_count = 0;
        double errSum = 0;
        for (int i = 0; i < 3; i++)
        {
            // find closest resolution bpm
            float bpm_resolution = (float)Math.Floor(bpm_base * Math.Pow(2, i));
            if (bpm_base - bpm_resolution >= 0.25)
            {
                bpm_base = bpm_resolution + 0.5f;
            }
            else if (bpm_base - bpm_resolution >= 0.75)
            {
                bpm_base++;
            }

            // find closest peak BPM
            float closest_bpm = 0;
            foreach (var peak in peaks)
            {
                if (Math.Abs(peak - bpm_resolution) < Math.Abs(closest_bpm - bpm_resolution))
                {
                    closest_bpm = peak;
                }
            }

            // get error
            double err = Math.Abs(60 / closest_bpm - 60 / bpm_resolution);
            if (err <= peakWidth)
            {
                errSum += Math.Pow(err / peakWidth, 2);
                harmonic_count++;
            }
        }
        double confidence = (1 - errSum / harmonic_count);
        if (harmonic_count == 0 || (harmonic_count == 1 && peaks.Count > 1))
        {
            confidence = 0;
        }

        Console.WriteLine("Best BPM = " + candidateBPM.Key + " (Confidence: " + Math.Round(confidence * 100, 3) + "%)");

        foreach (var t in bpmWeight)
        {
            stringBuilder.AppendLine(t.Value[0].ToString());
        }

        File.WriteAllText("DEBUG__BPM_CHAIN.txt", stringBuilder.ToString());
        stringBuilder.Clear();

        // find first sound instance
        int firstSoundID = 0;
        for (int i = 0; i < onsets.Count; i++)
        {
            if (onsets[i] > 0)
            {
                firstSoundID = i;
                break;
            }
        }

        Console.WriteLine("Offset = " + candidateBPM.Value[1]);
        Console.WriteLine("");

        return [candidateBPM.Key, (float)candidateBPM.Value[1]];
    }

    float[] GetBPM_Autocorrelation(List<double> onsets)
    {
        // METHOD 2: AUTOCORRELATION
        //  This method shifts the onsets and multiply by itself to check the periodicity.
        //  Then, FFT

        var corr = Autocorrelation(onsets, 1);

        int len = corr.Count;
        Dictionary<float, double[]> bpmWeight = []; // key = bpm, value = [score, offset]
        for (float bpm = MIN_SONG_BPM; bpm <= MAX_SONG_BPM; bpm += 0.5f)
        {
            double probability = 0;
            double timeDiff = 60d / bpm;

            // for every offsets
            double bestOffsetScore = 0;
            double bestOffset = 0;
            for (double offset = 0; offset < timeDiff; offset += 0.01)
            {
                int count = 0;
                for (double time = offset; SecondToIndex(time) < corr.Count; time += timeDiff)
                {
                    int checkID = SecondToIndex(time);
                    probability += corr[checkID];
                    count++;
                }
                probability /= count;
                if (probability > bestOffsetScore)
                {
                    bestOffsetScore = probability;
                    bestOffset = offset;
                }
            }
            bpmWeight[bpm] = [probability, bestOffset];
        }

        // Pick the BPM with best fit (highest score)
        var candidateBPM = bpmWeight.MaxBy(x => x.Value[0]);

        // Scale the BPM within range
        float b = candidateBPM.Key;
        while (b > MAX_SONG_BPM)
        {
            if ((b == (int)b && b % 2 == 0))
            {
                b /= 2;
            }
            else
            {
                // find
                b = bpmWeight.MaxBy(x => (x.Key < b) ? x.Value[0] : 0).Key;
            }
        }
        if (b < MIN_SONG_BPM || (b != (int)b && b * 2 <= MAX_SONG_BPM))
        {
            b *= 2;
        }
        candidateBPM = new(b, bpmWeight[b]);

        foreach (var t in bpmWeight)
        {
            stringBuilder.AppendLine(t.Value[0].ToString());
        }

        Console.WriteLine("Best BPM = " + candidateBPM.Key);

        File.WriteAllText("DEBUG__BPM_CORR.txt", stringBuilder.ToString());
        stringBuilder.Clear();

        return [candidateBPM.Key, (float)candidateBPM.Value[1]];
    }
}