using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using Palmmedia.ReportGenerator.Core.Common;
using UnityEngine.UIElements;
using System.Linq;
using System.Reflection;

using Utils;
using RhythmGameObjects;
using Config;
using UnityEditor;

public class ParseChart : MonoBehaviour
{
    public Chart chart = new Chart();
    public GameObject director;
    public bool chartUpdate = true;

    // Start is called before the first frame update
    void Start()
    {
        chartUpdate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (chartUpdate)
        {
            director = GameObject.Find("Director");
            var path = Path.Combine(Application.dataPath, "Resources/Songs", director.GetComponent<LevelHandler>().folder, "chart.json");

            // read chart
            string jsonString = File.ReadAllText(path);
            chart = JsonConvert.DeserializeObject<Chart>(jsonString);
            chart.Offset /= 1000;
            chart.SampleAll();
            chartUpdate = false;

            // debug
        }
    }
}

// TODO:
//    1) Judgement Tweaking 
//    2) Menu / Result
//    3) !! Charting UI !!
//    4) Tracker Position Adjustment