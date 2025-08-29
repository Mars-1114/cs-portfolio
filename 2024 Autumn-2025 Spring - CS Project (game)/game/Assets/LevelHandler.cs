using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

using RhythmGameObjects;
using Config;
using UnityEngine.Analytics;
using System;
using System.Linq;

public class LevelHandler : MonoBehaviour
{
    AudioSource songPlayer;
    private GameObject parser;
    public GameObject lanePrefab;
    public GameObject noteGenerator;
    public string folder;
    public float timer;

    public Performance performance;

    string songFile;
    bool isPlaying = false;
    bool updateChart = true;

    Chart chart;

    // Start is called before the first frame update
    void Start()
    {
        folder = BasicConfig.songs[1];
        parser = GameObject.Find("Parser");
        timer = -BasicConfig.countdown;
        songPlayer = GameObject.Find("SongPlayer").GetComponent<AudioSource>();
        songFile = Directory.GetFiles(Path.Combine("Assets/Resources/Songs", folder), "*.mp3")[0].Split('.')[0].Replace("Assets/Resources/", "");
        songPlayer.clip = Resources.Load<AudioClip>(songFile);
        songPlayer.clip.LoadAudioData();
        performance = new Performance();
    }

    // Update is called once per frame
    void Update()
    {
        if (updateChart)
        {
            updateChart = false;
            chart = parser.GetComponent<ParseChart>().chart;

            performance.totalNotes = chart.TotalNotes();
            // update offset
            timer += chart.Offset;

            // spawn lane renderers
            for (int i = 0; i < chart.Lanes.Count; i++)
            {
                GameObject laneRender = Instantiate(lanePrefab);
                laneRender.GetComponent<LaneControl>().laneID = i;
            }
        }
        timer += Time.deltaTime;
        if (!isPlaying && timer >= 0)
        {
            isPlaying = true;
            songPlayer.Play();
        }
        // update score
        if (performance.maxCombo < performance.combo)
        {
            performance.maxCombo = performance.combo;
        }
    }
}
