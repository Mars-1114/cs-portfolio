using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RhythmGameObjects;
using Utils;
using Config;
using UnityEditor.Experimental.GraphView;

public class Movement : MonoBehaviour
{
    public int noteID;
    public int laneID;
    public string type;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // move
        Note note = GameObject.Find("Parser").GetComponent<ParseChart>().chart.Lanes[laneID].Notes[noteID];
        float currentJudgePos = transform.parent.parent.gameObject.GetComponent<LaneControl>().Distance;
        transform.position = new Vector3(transform.position.x, transform.position.y, note.Distance - currentJudgePos + BasicConfig.judgelinePos);
    }

}
