using RhythmGameObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Config;
using Leap;
using System.Linq;

public class LaneControl : MonoBehaviour
{
    public float Distance;
    public int laneID;

    Chart chart;
    GameObject director;
    GameObject indicator;
    List<int> endpointsArr = new List<int> { 0 };
    List<Vector3> samples;
    int segmentID;
    int sampleID;

    // Start is called before the first frame update
    void Start()
    {
        chart = GameObject.Find("Parser").GetComponent<ParseChart>().chart;
        director = GameObject.Find("Director");
        indicator = transform.Find("HitIndicator").gameObject;
        segmentID = 0;
        sampleID = 0;
        samples = chart.Lanes[laneID].GetAllSamples();

        // initialize distance
        Distance = -BasicConfig.countdown * chart.Lanes[laneID].Nodes.SpeedControl[0].GetSpeed();

        // find endpoint IDs
        int endpointID = 0;
        while (endpointID < chart.Lanes[laneID].Nodes.EndpointControl.Count)
        {
            // find sample ID
            float endpoint_distance = chart.Distance(laneID, chart.Lanes[laneID].Nodes.EndpointControl[endpointID].Beat);
            endpointsArr.Add(Compute.BinarySearch(samples, endpoint_distance));
            endpointID++;
        }
        if (endpointsArr.Count % 2 == 1)
        {
            endpointsArr.Add(samples.Count - 1);
        }

        // generate line render
        for (int i = 0; i < endpointsArr.Count; i += 2)
        {
            GameObject lineSegment = new GameObject("LineSegment");
            lineSegment.transform.SetParent(transform.Find("LineRender"));
            var script = lineSegment.AddComponent<LaneRender>();
            script.sample = samples.Skip(endpointsArr[i]).Take(endpointsArr[i + 1] - endpointsArr[i] + 1).ToList();
        }

        indicator.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        float time = director.GetComponent<LevelHandler>().timer - chart.Offset;

        // update distance
        Distance = chart.DistanceFromSecond(laneID, time);

        // update sample ID
        while (sampleID < samples.Count && samples[sampleID].z < Distance)
        {
            sampleID++;
        }

        // update segment ID
        if (segmentID < endpointsArr.Count && samples[endpointsArr[segmentID + 1]].z < Distance)
        {
            segmentID += 2;
        }

        // update indicator position
        if (sampleID < samples.Count && segmentID < endpointsArr.Count)
        {
            float closestEndpointDistance = samples[endpointsArr[segmentID]].z - Distance;
            float displayDistance = 20;
            float alpha = chart.AlphaFromSecond(laneID, time);
            float opacity = 0;
            var material = indicator.GetComponent<Renderer>();
            indicator.SetActive(true);
            indicator.transform.position = new Vector3(samples[sampleID].x, samples[sampleID].y, BasicConfig.judgelinePos);
            if (closestEndpointDistance <= 0)
            {
                opacity = 1;
            }
            else if (closestEndpointDistance < displayDistance)
            {
                opacity = 1 - closestEndpointDistance / displayDistance;
            }
            material.material.color = new Color(1, 1, 1, opacity * 0.7f * alpha);
        }
        else
        {
            indicator.SetActive(false);
        }
    }
}
