using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RhythmGameObjects;
using Config;
using Leap.Preview.Locomotion;

public class noteGenerator : MonoBehaviour
{
    public GameObject NotePrefab;
    private GameObject director;
    public Chart chart;
    int laneID;
    int noteID;

    // Start is called before the first frame update
    void Start()
    {
        director = GameObject.Find("Director");

        chart = GameObject.Find("Parser").GetComponent<ParseChart>().chart;
        laneID = transform.parent.gameObject.GetComponent<LaneControl>().laneID;
        noteID = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = transform.parent.gameObject.GetComponent<LaneControl>().Distance;
        if (noteID < chart.Lanes[laneID].Notes.Count)
        {
            if (chart.Lanes[laneID].Notes[noteID].Distance < distance + BasicConfig.renderDistance - BasicConfig.judgelinePos)
            {
                GameObject note = Instantiate(NotePrefab, this.transform);
                note.transform.position = new Vector3(chart.Lanes[laneID].Notes[noteID].Position.x, chart.Lanes[laneID].Notes[noteID].Position.y, BasicConfig.renderDistance);
                note.GetComponent<Movement>().noteID = noteID;
                note.GetComponent<Movement>().laneID = laneID;
                note.GetComponent<Movement>().type = chart.Lanes[laneID].Notes[noteID].Type;

                // set script for different note type
                string meshFile = "";
                if (note.GetComponent<Movement>().type == "tap")
                {
                    meshFile = "tapNote";
                    note.AddComponent<TapNoteHandle>();
                    note.GetComponent<TapNoteHandle>().laneID = laneID;
                    note.GetComponent<TapNoteHandle>().beat = chart.Lanes[laneID].Notes[noteID].Beat;
                }
                else if (note.GetComponent<Movement>().type == "track")
                {
                    meshFile = "trackNote";
                    note.AddComponent<TrackNoteHandle>();
                    note.GetComponent<TrackNoteHandle>().laneID = laneID;
                    note.GetComponent<TrackNoteHandle>().beat = chart.Lanes[laneID].Notes[noteID].Beat;
                    note.GetComponent<TrackNoteHandle>().duration = chart.Lanes[laneID].Notes[noteID].Duration;
                }
                else if (note.GetComponent<Movement>().type == "clap")
                {
                    meshFile = "clapNote";
                    note.AddComponent<ClapNoteHandle>();
                    note.GetComponent<ClapNoteHandle>().laneID = laneID;
                    note.GetComponent<ClapNoteHandle>().beat = chart.Lanes[laneID].Notes[noteID].Beat;

                }
                else if (note.GetComponent<Movement>().type == "punch")
                {
                    meshFile = "punchNote";
                    note.AddComponent<PunchNoteHandle>();
                    note.GetComponent<PunchNoteHandle>().laneID = laneID;
                    note.GetComponent<PunchNoteHandle>().beat = chart.Lanes[laneID].Notes[noteID].Beat;

                }
                else if (note.GetComponent<Movement>().type == "avoid")
                {
                    meshFile = "avoidNote";
                    note.AddComponent<AvoidNoteHandle>();
                    note.GetComponent<AvoidNoteHandle>().laneID = laneID;
                    note.GetComponent<AvoidNoteHandle>().beat = chart.Lanes[laneID].Notes[noteID].Beat;

                }

                // load the model
                GameObject ModelPrefab = Resources.Load<GameObject>("Meshes/" + meshFile);

                // attach note model to note
                GameObject model;
                model = Instantiate(ModelPrefab, note.transform) as GameObject;
                model.transform.localScale *= 0.3f;
                if (note.GetComponent<Movement>().type == "track")
                {
                    // TRACK note needs to generate a row of models
                    ModelPrefab = Resources.Load<GameObject>("Meshes/trackNote_body");
                    foreach (Vector3 sample in chart.Lanes[laneID].Notes[noteID].Samples)
                    {
                        model = Instantiate(ModelPrefab, note.transform) as GameObject;
                        model.transform.localScale *= 0.25f;
                        model.transform.localPosition += sample;
                        model.SetActive(false);
                    }
                }
                noteID++;
            }
        }
    }
}