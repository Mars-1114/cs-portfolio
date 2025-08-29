namespace Assist;

public partial class AudioHandler
{
    /// <summary>
    /// Auto detect the BPM of the song
    /// </summary>
    /// <returns></returns>
    public float[] BPMDetect_Energy()
    {
        var amp = GetStereo();
        var left = amp[0];
        var right = amp[1];

        // we only take the bass kicks of the song
        // feed into lowpass filter
        int cutoff = 200;
        for (int i = 0; i < 1; i++)
        {
            PerformLowpass(left, cutoff);
            PerformLowpass(right, cutoff);
        }

        List<float> energy = [];
        // compute sample energy
        // FORMULA: E[i] = l[i]^2 + r{i]^2
        for (int i = 0; i < left.Length; i++)
        {
            energy.Add(left[i] * left[i] + right[i] * right[i]);
        }

        List<double> energy_instant = [];
        // compute instant and average energy
        // DEFINITION: Instant = the audio duration that a human can identify a sound
        //                       Here we take 512 samples (~0.01 seconds) as an unit
        //             Average = the audio duration that a human can feel the dynamics of the sound
        //                       Here we set the duration to 0.5 seconds
        for (int i = 0; i < left.Length / InstantSize; i++)
        {
            double instant = energy[(i * InstantSize)..((i + 1) * InstantSize)].Average();
            energy_instant.Add(instant);
        }

        // smooth out energy
        List<double> energy_smooth = [];
        Queue<double> energy_history = new(new double[5]);
        for (int i = 0; i < energy_instant.Count; i++)
        {
            energy_history.Enqueue(energy_instant[i]);
            energy_history.Dequeue();
            energy_smooth.Add(energy_history.Average());
        }

        List<double> energy_ratio = [];
        Queue<double> historyBuffer = new(new double[HistoryBufferSize]);
        for (int i = 0; i < energy_smooth.Count; i++)
        {
            historyBuffer.Enqueue(energy_smooth[i]);
            historyBuffer.Dequeue();

            // get the ratio of instant and average energy
            // this way we can see if the instant energy is relatively high or low in that interval
            var ratio = energy_smooth[i] / historyBuffer.Average();
            energy_ratio.Add(ratio);
        }

        List<double> energy_ratio_processed = [];
        float onsetSensitivity = 1.1f;
        // Here we threshold the ratio to remove the noise below a certain value
        // DEFINITION: Sensitivity = the threshold of the ratio
        //                           in a perfect world, the value should be a variable (TO BE IMPLEMENTED)
        for (int i = 1; i < energy_ratio.Count; i++)
        {
            if (energy_ratio[i] < onsetSensitivity)
            {
                energy_ratio_processed.Add(0);
            }
            else
            {
                energy_ratio_processed.Add(energy_ratio[i]);
            }
        }

        // take the first of the clustered ratio
        Decluster(energy_ratio_processed);

        // Now we have onsets!
        // The next step is to find the best fit BPM to our onsets

        _DEBUG_(energy, "DEBUG__ENG_RAW.txt");
        _DEBUG_(energy_instant, "DEBUG__ENG_INST.txt");
        _DEBUG_(energy_smooth, "DEBUG__ENG_SMOOTH.txt");
        _DEBUG_(energy_ratio, "DEBUG__ENG_RATIO.txt");
        _DEBUG_(energy_ratio_processed, "DEBUG__ENG_RATIO_POST.txt");
        //_DEBUG_(wBPM, "DEBUG__BPM_CHAIN.txt");

        return GetBPM_Autocorrelation(energy_ratio_processed);
    }
    public float[] BPMDetect_WeightedEnergy()
    {
        List<float> amplitude = GetMono().ToList();

        List<List<float>> spectrum = PerformSTFT(amplitude, WindowSize, InstantSize);

        // The sum of energy of each frequency band for all instants
        List<double> energy = [];
        // The weighted sum of energy of each frequency band of all instants
        List<double> energy_weighted = [];
        foreach (var time in spectrum)
        {
            double eng = 0;
            double eng_w = 0;
            for (int i = 2; i < time.Count; i++)
            {
                eng += time[i] * time[i];
                eng_w += time[i] * time[i] * i;
            }
            energy.Add(eng);
            energy_weighted.Add(eng_w);
        }

        // determine onsets
        //  FORMULA: eng_w[i]^2 / (eng_w[i - 1] * eng[i]) > sensitivity
        //  :( Masri you lied to me...
        List<double> energy_processed = [];
        float onset_sensitivity = 1.5f;

        Queue<double> historyBuffer = new(new double[HistoryBufferSize]);
        for (int i = 0; i < energy_weighted.Count; i++)
        {
            historyBuffer.Enqueue(energy_weighted[i]);
            historyBuffer.Dequeue();

            // get the ratio of instant and average energy
            // this way we can see if the instant energy is relatively high or low in that interval
            var ratio = energy_weighted[i] / historyBuffer.Average();

            if (ratio <= onset_sensitivity)
            {
                ratio = 0;
            }
            energy_processed.Add(ratio);
        }

        Decluster(energy_processed);

        _DEBUG_(energy_weighted, "DEBUG__ENG_WEIGHTED.txt");
        _DEBUG_(energy_processed, "DEBUG__ENG_WEIGHTED_POST.txt");
        //_DEBUG_(wBPM, "DEBUG__BPM_WGHT.txt");

        return GetBPM_Autocorrelation(energy_processed);
    }
    public float[] BPMDetect_Freq()
    {

        List<float> amplitude = GetMono().ToList();

        List<List<float>> spectrum = PerformSTFT(amplitude, WindowSize, InstantSize);

        /*
        SPECTROGRAM(spectrum, "DEBUG_SPECTROGRAM.txt");
        */

        List<double> percussivity = [];
        float sensitivity_perc = 5;
        // get to ratio
        for (int i = 1; i < spectrum.Count; i++)
        {
            int count = 0;
            for (int j = 20; j < spectrum[i].Count; j++)
            {
                double ratio = 20 * Math.Log10(spectrum[i][j] / spectrum[i - 1][j]);

                // check threshold
                if (ratio >= sensitivity_perc)
                {
                    count++;
                }
            }
            percussivity.Add(count);
        }

        _DEBUG_(percussivity, "DEBUG__PERC.txt");

        float sensitivity_ratio = 1.5f;
        Queue<double> historyBuffer = new(new double[HistoryBufferSize]);
        // find onsets
        for (int i = 0; i < percussivity.Count; i++)
        {
            historyBuffer.Enqueue(percussivity[i]);
            historyBuffer.Dequeue();

            if (percussivity[i] < historyBuffer.Average() * sensitivity_ratio)
                percussivity[i] = 0;
        }

        // take the first of the clustered ratio
        Decluster(percussivity);

        _DEBUG_(percussivity, "DEBUG__PERC_POST.txt");
        //_DEBUG_(wBPM, "DEBUG__BPM_FREQ.txt");

        return GetBPM_Autocorrelation(percussivity);
    }
}