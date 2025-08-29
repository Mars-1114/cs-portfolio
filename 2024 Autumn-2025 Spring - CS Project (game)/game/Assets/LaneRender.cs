using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Utils;
using RhythmGameObjects;
using Config;

public class LaneRender : MonoBehaviour
{
    public List<Vector3> sample;

    GameObject parser;
    GameObject director;
    Chart chart;
    LineRenderer laneRender;
    int sampleID;
    int laneID;

    // Start is called before the first frame update
    void Start()
    {
        parser = GameObject.Find("Parser");
        director = GameObject.Find("Director");
        chart = parser.GetComponent<ParseChart>().chart;
        laneRender = gameObject.AddComponent<LineRenderer>();
        sampleID = 0;
        laneID = transform.parent.parent.GetComponent<LaneControl>().laneID;

        // set up lane attributes
        laneRender.useWorldSpace = true;
        laneRender.sortingOrder = -1;
        laneRender.material = new Material(Shader.Find("Sprites/Default"));
        laneRender.startWidth = 0.08f;
        laneRender.endWidth = 0.08f;
    }

    // Update is called once per frame
    void Update()
    {
        float time = director.GetComponent<LevelHandler>().timer - chart.Offset;
        float distance = transform.parent.parent.gameObject.GetComponent<LaneControl>().Distance;

        // clear line
        laneRender.SetPositions(new Vector3[] { });

        // delete renderer when reached end of lane
        if (distance > sample[^1].z)
        {
            Destroy(gameObject);
            return;
        }

        // update lane opacity
        float alpha = chart.Alpha(laneID, chart.Beat(time));
        laneRender.startColor = new Color(0.7f, 0.7f, 0.7f, 0.3f * alpha);
        laneRender.endColor = new Color(0.7f, 0.7f, 0.7f, 0);

        // update begin sampleID
        if (sampleID < sample.Count)
        {
            while (sample[sampleID].z < distance)
            {
                sampleID++;
                if (sampleID == sample.Count)
                {
                    break;
                }
            }
        }

        // get end sampleID
        int endSampleID = sampleID;
        while (endSampleID < sample.Count &&
            sample[endSampleID].z - sample[sampleID].z < BasicConfig.renderDistance * 0.5f - BasicConfig.judgelinePos)
        {
            endSampleID++;
        }

        // draw lines
        List<Vector3> renderSamples = new List<Vector3>();
        int n = sampleID;
        if (n < sample.Count)
        {
            while (sample[n].z - BasicConfig.renderDistance + BasicConfig.judgelinePos < distance && n <= endSampleID)
            {
                renderSamples.Add(sample[n]);
                n++;
                if (n == sample.Count)
                {
                    break;
                }
            }
        }
        var line = renderSamples.ToArray();
        for (int j = 0; j < line.Length; j++)
        {
            line[j].z -= distance - BasicConfig.judgelinePos;
        }
        laneRender.positionCount = line.Length;
        laneRender.SetPositions(line);
    }
}
