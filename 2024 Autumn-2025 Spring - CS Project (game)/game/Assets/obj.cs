using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

using Utils;

// Data Structures For Game Objects
namespace RhythmGameObjects
{
    [Serializable]
    public class Chart
    {
        // JSON Parse
        private string name = "";
        private string artist = "";
        private float difficulty;
        private float offset;
        private List<BPMControl> bpmControl = new List<BPMControl>();
        private List<Lane> lanes = new List<Lane>();

        // Properties
        public string Name
        {
            get => name;
            set => name = value;
        }
        public string Artist
        {
            get => artist;
            set => artist = value;
        }
        public float Difficulty
        {
            get => difficulty;
            set => difficulty = value;
        }
        public float Offset
        {
            get => offset;
            set => offset = value;
        }
        public List<BPMControl> BPMControl
        {
            get => bpmControl;
            set => bpmControl = value;
        }
        public List<Lane> Lanes
        {
            get => lanes;
            set => lanes = value;
        }

        public float Second(float beat)
        {
            // sort by beat
            var BPMControl = bpmControl.OrderBy(x => x.Beat).ToList();
            // case 1: beat before the first node
            if (beat <= BPMControl[0].Beat)
            {
                return 60 / BPMControl[0].BPM * (beat - BPMControl[0].Beat);
            }
            else
            {
                BPMControl startNode, endNode;
                float s = 0;
                int i = 0;
                for (; i + 1 < BPMControl.Count && BPMControl[i].Beat < beat; i++)
                {
                    startNode = BPMControl[i];
                    endNode = BPMControl[i + 1];
                    float startSPB = 60 / startNode.BPM;
                    float endSPB = 60 / endNode.BPM;
                    if (startNode.Easing == "hold")
                    {
                        s += Compute.Integral(startSPB, startSPB, endNode.Beat - startNode.Beat);
                    }
                    else
                    {
                        s += Compute.Integral(startSPB, endSPB, endNode.Beat - startNode.Beat);
                    }
                }
                // beats after last node
                if (beat >= BPMControl[^1].Beat)
                {
                    s += 60 / BPMControl[^1].BPM * (beat - BPMControl[^1].Beat);
                }
                // remove excessive part
                else
                {
                    startNode = BPMControl[i - 1];
                    endNode = BPMControl[i];
                    float startSPB = 60 / startNode.BPM;
                    float endSPB = 60 / endNode.BPM;
                    float SPB = startSPB + (endSPB - startSPB) * (beat - startNode.Beat) / (endNode.Beat - startNode.Beat);
                    if (startNode.Easing == "hold")
                    {
                        s -= Compute.Integral(startSPB, startSPB, endNode.Beat - beat);
                    }
                    else
                    {
                        s -= Compute.Integral(SPB, endSPB, endNode.Beat - beat);
                    }
                }
                return s;
            }
        }
        public float Second(float startBeat, float endBeat)
        {
            return Second(endBeat) - Second(startBeat);
        }
        /// <summary>
        /// Convert second to beat
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public float Beat(float second)
        {
            // sort by beat
            var sortedBPMControl = BPMControl.OrderBy(x => x.Beat).ToList();
            if (second <= Second(sortedBPMControl[0].Beat))
            {
                return sortedBPMControl[0].BPM / 60 * (second - Second(sortedBPMControl[0].Beat));
            }
            else
            {
                BPMControl startNode, endNode;
                float b = 0;
                int i = 0;
                for (; i + 1 < sortedBPMControl.Count && Second(sortedBPMControl[i].Beat) < second; i++)
                {
                    startNode = sortedBPMControl[i];
                    endNode = sortedBPMControl[i + 1];
                    float startBPS = startNode.BPM / 60;
                    float endBPS = endNode.BPM / 60;
                    if (startNode.Easing == "hold")
                    {
                        b += Compute.Integral(startBPS, startBPS, Second(startNode.Beat, endNode.Beat));
                    }
                    else
                    {
                        b += Compute.Integral(startBPS, endBPS, Second(startNode.Beat, endNode.Beat));
                    }
                }
                // second after last node
                if (second > Second(sortedBPMControl[^1].Beat))
                {
                    b += sortedBPMControl[^1].BPM / 60 * (second - Second(sortedBPMControl[^1].Beat));
                }
                // remove excessive part
                else
                {
                    startNode = sortedBPMControl[i - 1];
                    endNode = sortedBPMControl[i];
                    float startBPS = startNode.BPM / 60;
                    float endBPS = endNode.BPM / 60;
                    float BPS = startBPS + (endBPS - startBPS) * (second - Second(startNode.Beat)) / Second(startNode.Beat, endNode.Beat);
                    if (startNode.Easing == "hold")
                    {
                        b -= Compute.Integral(startBPS, startBPS, Second(endNode.Beat) - second);
                    }
                    else
                    {
                        b -= Compute.Integral(BPS, endBPS, Second(endNode.Beat) - second);
                    }
                }
                return b;
            }
        }
        /// <summary>
        /// Get the distance of a given beat
        /// </summary>
        /// <param name="laneID"></param>
        /// <param name="beat"></param>
        /// <returns></returns>
        public float Distance(int laneID, float beat)
        {
            // offset
            var SpeedControl = Lanes[laneID].Nodes.SpeedControl.OrderBy(x => x.Beat).ToList();
            float distance = Compute.Integral(SpeedControl[0].GetSpeed(), SpeedControl[0].GetSpeed(), SpeedControl[0].Beat);
            if (beat <= SpeedControl[0].Beat)
            {
                return distance + SpeedControl[0].GetSpeed() * Second(beat - SpeedControl[0].Beat);
            }
            else
            {
                // integrate each line segment
                SpeedControl startNode, endNode;
                int i = 0;
                for (; i + 1 < SpeedControl.Count && SpeedControl[i].Beat < beat; i++)
                {
                    startNode = SpeedControl[i];
                    endNode = SpeedControl[i + 1];
                    if (startNode.Easing == "hold")
                    {
                        distance += Compute.Integral(startNode.GetSpeed(), startNode.GetSpeed(), Second(startNode.Beat, endNode.Beat));
                    }
                    else if (startNode.Easing == "linear")
                    {
                        distance += Compute.Integral(startNode.GetSpeed(), endNode.GetSpeed(), Second(startNode.Beat, endNode.Beat));
                    }
                }
                if (beat >= SpeedControl[^1].Beat)
                {
                    // add to beat
                    // for beat beyond the last node, easing would be "hold" no matter the actual value
                    distance += SpeedControl[^1].GetSpeed() * Second(SpeedControl[^1].Beat, beat);
                }
                else
                {
                    startNode = SpeedControl[i - 1];
                    endNode = SpeedControl[i];
                    // subtract excessive part
                    float speed = startNode.GetSpeed() + (endNode.GetSpeed() - startNode.GetSpeed()) * Second(startNode.Beat, beat) / Second(startNode.Beat, endNode.Beat);
                    if (startNode.Easing == "hold")
                    {
                        distance -= Compute.Integral(startNode.GetSpeed(), startNode.GetSpeed(), Second(beat, endNode.Beat));
                    }
                    else
                    {
                        distance -= Compute.Integral(speed, endNode.GetSpeed(), Second(beat, endNode.Beat));
                    }
                }
            }
            return distance;
        }
        public float Distance(int laneID, float startBeat, float endBeat)
        {
            return Distance(laneID, endBeat) - Distance(laneID, startBeat);
        }
        public float DistanceFromSecond(int laneID, float second)
        {
            return Distance(laneID, Beat(second));
        }
        /// <summary>
        /// Get the opacity of a given beat
        /// </summary>
        /// <param name="laneID"></param>
        /// <param name="beat"></param>
        /// <returns></returns>
        public float Alpha(int laneID, float beat)
        {
            List<AlphaControl> AlphaControl = Lanes[laneID].Nodes.AlphaControl.OrderBy(x => x.Beat).ToList();
            // case 0: no nodes
            if (AlphaControl.Count == 0) return 1;
            // case 1: beat before the first node
            if (beat <= AlphaControl[0].Beat) return AlphaControl[0].Alpha;
            // case 2: beat after the last node
            if (beat >= AlphaControl[^1].Beat) return AlphaControl[^1].Alpha;
            // case 3: beat inbetween
            // find the segment
            int index = 0;
            while (beat >= AlphaControl[index].Beat) index++;
            // compute alpha
            switch (AlphaControl[index - 1].Easing)
            {
                case "hold":
                    return AlphaControl[index - 1].Alpha;
                case "linear":
                    var startNode = AlphaControl[index - 1];
                    var endNode = AlphaControl[index];
                    return startNode.Alpha + (endNode.Alpha - startNode.Alpha) * Second(startNode.Beat, beat) / Second(startNode.Beat, endNode.Beat);
                default:
                    throw new NotSupportedException("Alpha Control: Invalid Easing Type at Beat = " + AlphaControl[index].Beat);
            }
        }
        public float AlphaFromSecond(int laneID, float second)
        {
            return Alpha(laneID, Beat(second));
        }
        /// <summary>
        /// Get BPM of a given beat.
        /// </summary>
        /// <param name="beat"></param>
        /// <returns></returns>
        public float BPM(float beat)
        {
            var BPMControl = bpmControl.OrderBy(x => x.Beat).ToList();
            // case 1: beat before the first node
            if (beat <= BPMControl[0].Beat) return BPMControl[0].BPM;
            // case 2: beat after the last node
            if (beat >= BPMControl[^1].Beat) return BPMControl[^1].BPM;
            // case 3: beat inbetween
            int index = 0;
            while (beat >= BPMControl[index].Beat) index++;
            switch (BPMControl[index - 1].Easing)
            {
                case "hold":
                    return BPMControl[index - 1].BPM;
                case "linear":
                    var startNode = BPMControl[index - 1];
                    var endNode = BPMControl[index];
                    float startSPB = 60 / startNode.BPM;
                    float endSPB = 60 / endNode.BPM;
                    return startNode.BPM + (endNode.BPM - startNode.BPM) * Second(startNode.Beat, beat) / Second(startNode.Beat, endNode.Beat);
                default:
                    throw new NotSupportedException("BPM Control: Invalid Easing Type at Beat = " + BPMControl[index].Beat);
            }
        }
        public void SampleAll()
        {
            for (int i = 0; i < Lanes.Count; i++)
            {
                Sample(i);
            }
        }
        public void Sample(int laneID)
        {
            var lane = Lanes[laneID];
            // compute distance of each notes
            foreach (var note in lane.Notes)
            {
                note.Distance = Distance(lane.ID, note.Beat);
                if (note.Type == "track")
                {
                    note.EndDistance = Distance(lane.ID, note.Beat + note.Duration);
                }
            }
            foreach (var node in lane.Nodes.PositionControl)
            {
                node.Position = Compute.TransformCoords(node.Position.x, node.Position.y);
            }
            // clear old curves
            lane.Curves.Clear();
            // generate lane curves
            var nodes = lane.Nodes.PositionControl.OrderBy(x => x.Beat).ToList();
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                // compute distance
                float startBeat = nodes[i].Beat;
                float endBeat = nodes[i + 1].Beat;
                float distance = Distance(lane.ID, startBeat);
                float length = Distance(lane.ID, endBeat) - distance;
                nodes[i].Distance = distance;
                nodes[i + 1].Distance = distance + length;

                // generate curves
                Vector2 startPos = nodes[i].Position;
                Vector2 endPos = nodes[i + 1].Position;
                Vector3 startTangent = new(0, 0, 0);
                Vector3 endTangent = new(1, 1, 1);
                switch (nodes[i].Easing)
                {
                    case "hold":
                        endPos = startPos;
                        break;
                    case "linear":
                        break;
                    case "easeInOut":
                        startTangent = new(0, 0, 0.45f);
                        endTangent = new(1, 1, 0.55f);
                        break;
                    case "quintInOut":
                        startTangent = new(0, 0, 0.83f);
                        endTangent = new(1, 1, 0.17f);
                        break;
                    case "expoIn":
                        startTangent = new(0.7f, 0, 0);
                        endTangent = new(0.84f, 1, 0);
                        break;
                    case "circIn":
                        startTangent = new(0.55f, 0, 0);
                        endTangent = new(1, 0.45f, 0.45f);
                        break;
                    case "circOut":
                        startTangent = new(0, 0.55f, 0.55f);
                        endTangent = new(0.45f, 1, 1);
                        break;
                    default:
                        throw new Exception("Invalid Easing Type \"" + nodes[i].Easing + "\"");
                }
                lane.Curves.Add(new Curve(startPos, startTangent, endPos, endTangent, length, startBeat, distance));
                distance += length;
            }

            // compute note position
            int curveID = 0;
            foreach (var note in lane.Notes.OrderBy(x => x.Beat))
            {
                if (lane.Curves.Count > 0)
                {
                    // find curve section
                    while (curveID < lane.Curves.Count - 1 && !(lane.Curves[curveID].Beat <= note.Beat && note.Beat < lane.Curves[curveID + 1].Beat))
                    {
                        curveID++;
                    }

                    // find the distance from the start of the curve
                    float curveDistance = lane.Curves[curveID].Distance;
                    Vector3 notePos = lane.Curves[curveID].ApproxPoint(note.Distance - curveDistance);
                    note.Position = new(notePos.x, notePos.y);

                    // get the position & distance of the end of track note
                    if (note.Type == "track")
                    {
                        note.Samples.Clear();
                        int tempCurveID = curveID;

                        // find curve section
                        while (curveID + 1 < lane.Curves.Count && !(lane.Curves[curveID].Beat <= (note.Beat + note.Duration) && (note.Beat + note.Duration) < lane.Curves[curveID + 1].Beat))
                        {
                            curveID++;
                        }

                        // find the distance from the start of the curve
                        curveDistance = lane.Curves[curveID].Distance;
                        Vector3 noteEndPos = lane.Curves[curveID].ApproxPoint(note.EndDistance - curveDistance);
                        note.EndPosition = new(noteEndPos.x, noteEndPos.y);

                        curveID = tempCurveID;

                        // sample the track notes
                        float currentDistance = note.Distance;
                        while (currentDistance <= note.EndDistance)
                        {
                            // find curve section
                            while (curveID < lane.Curves.Count - 1 && !(lane.Curves[curveID].Distance <= currentDistance && currentDistance < lane.Curves[curveID + 1].Distance))
                            {
                                curveID++;
                            }

                            Vector3 samplePos = lane.Curves[curveID].ApproxPoint(currentDistance - lane.Curves[curveID].Distance);
                            Vector2 pos = new(samplePos.x, samplePos.y);
                            pos -= note.Position;
                            float dist = currentDistance - note.Distance;
                            note.Samples.Add(new Vector3(pos.x, pos.y, dist));
                            currentDistance += 1.5f;
                        }
                        curveID = tempCurveID;
                    }
                }
                else if (lane.Nodes.PositionControl.Count > 0)
                {
                    note.Position = lane.Nodes.PositionControl[0].Position;
                    if (note.Type == "track")
                    {
                        note.EndPosition = note.Position;
                    }
                }
                else
                {
                    throw new NullReferenceException("No Position Control Is Found");
                }
            }

            // Compute Endpoint Distance
            foreach (var endpoint in lane.Nodes.EndpointControl)
            {
                endpoint.Distance = Distance(laneID, endpoint.Beat);
            }
        }
        public Chart GetSortedChart()
        {
            Chart output = new()
            {
                Name = name,
                Artist = artist,
                Difficulty = difficulty,
                Offset = offset,
                BPMControl = bpmControl.OrderBy(x => x.Beat).ToList(),
                Lanes = lanes
            };
            foreach (var lane in lanes)
            {
                lane.Notes = lane.Notes.OrderBy(x => x.Beat).ToList();
                lane.Nodes.PositionControl = lane.Nodes.PositionControl.OrderBy(x => x.Beat).ToList();
                lane.Nodes.AlphaControl = lane.Nodes.AlphaControl.OrderBy(x => x.Beat).ToList();
                lane.Nodes.SpeedControl = lane.Nodes.SpeedControl.OrderBy(x => x.Beat).ToList();
                lane.Nodes.PositionControl = lane.Nodes.PositionControl.OrderBy(x => x.Beat).ToList();
            }
            return output;
        }

        public int EndpointAttr(int laneID, float beat)
        {
            var endpoints = Lanes[laneID].Nodes.EndpointControl.OrderBy(x => x.Beat).ToList();
            int index = 0;
            for (; index < endpoints.Count && beat > endpoints[index].Beat; index++) { }
            return 1 - index % 2;
        }

        public bool EndpointAttrFromDistance(int laneID, float distance)
        {
            var endpoints = Lanes[laneID].Nodes.EndpointControl.OrderBy(x => x.Beat).ToList();
            int index = 0;
            for (; index < endpoints.Count && distance > endpoints[index].Distance; index++) { }
            return index % 2 == 0;
        }
        public int TotalNotes()
        {
            int s = 0;
            foreach (var lane in lanes)
            {
                s += lane.Notes.Count;
            }
            return s;
        }
    }

    [Serializable]
    public class Lane
    {
        private int id;
        private List<Note> notes = new List<Note>();
        private ControlNode nodes = new();
        private List<Curve> curves = new List<Curve>();

        public int ID
        {
            get => id;
            set => id = value;
        }
        public List<Note> Notes
        {
            get => notes;
            set => notes = value;
        }
        public ControlNode Nodes
        {
            get => nodes;
            set => nodes = value;
        }
        public List<Curve> Curves
        {
            get => curves;
            set => curves = value;
        }

        public List<Vector3> GetAllSamples()
        {
            List<Vector3> laneSamples = new List<Vector3>();
            foreach (var curve in Curves)
            {
                laneSamples.AddRange(curve.Samples);
            }
            return laneSamples;
        }
    }

    [Serializable]
    public class Note
    {
        private float beat;
        private string type = "";
        private float duration;
        private Vector2 position = new(0, 0);
        private Vector2 end_position = new(0, 0);
        private float distance;
        private float end_distance;
        private List<Vector3> samples = new List<Vector3>();

        public float Beat
        {
            get => beat;
            set => beat = value;
        }
        public string Type
        {
            get => type;
            set => type = value;
        }
        public float Duration
        {
            get => duration;
            set => duration = value;
        }
        public Vector2 Position
        {
            get => position;
            set => position = value;
        }
        public Vector2 EndPosition
        {
            get => end_position;
            set => end_position = value;
        }
        public float Distance
        {
            get => distance;
            set => distance = value;
        }
        public float EndDistance
        {
            get => end_distance;
            set => end_distance = value;
        }
        public List<Vector3> Samples
        {
            get => samples;
            set => samples = value;
        }
    }

    [Serializable]
    public class ControlNode
    {
        private List<PositionControl> positionControl = new List<PositionControl>();
        private List<AlphaControl> alphaControl = new List<AlphaControl>();
        private List<SpeedControl> speedControl = new List<SpeedControl>();
        private List<EndpointControl> endpointControl = new List<EndpointControl>();

        public List<PositionControl> PositionControl
        {
            get => positionControl;
            set => positionControl = value;
        }
        public List<AlphaControl> AlphaControl
        {
            get => alphaControl;
            set => alphaControl = value;
        }
        public List<SpeedControl> SpeedControl
        {
            get => speedControl;
            set => speedControl = value;
        }
        public List<EndpointControl> EndpointControl
        {
            get => endpointControl;
            set => endpointControl = value;
        }
    }

    public class Curve
    {
        private float beat;
        private Vector3 start;
        private Vector3 end;
        private Vector3 startTangent;
        private Vector3 endTangent;
        private float length;
        private float distance;
        private List<Vector3> samples = new List<Vector3>();

        public float Beat
        {
            get => beat;
            set => beat = value;
        }
        public Vector3 Start
        {
            get => start;
            set => start = value;
        }
        public Vector3 End
        {
            get => end;
            set => end = value;
        }
        public Vector3 StartTangent
        {
            get => startTangent;
            set => startTangent = value;
        }
        public Vector3 EndTangent
        {
            get => endTangent;
            set => endTangent = value;
        }
        public float Length
        {
            get => length;
            set => length = value;
        }
        public float Distance
        {
            get => distance;
            set => distance = value;
        }
        public List<Vector3> Samples
        {
            get => samples;
            set => samples = value;
        }

        public Curve(Vector2 start, Vector3 startTangent, Vector2 end, Vector3 endTangent, float length, float beat, float distance)
        {
            this.start = new(start.x, start.y, 0);
            this.end = new(end.x, end.y, length);
            Vector3 size = End - Start;
            startTangent = Vector3.Scale(startTangent, size);
            endTangent = Vector3.Scale(endTangent, size);
            this.startTangent = startTangent + Start;
            this.endTangent = endTangent + Start;
            this.length = length;
            this.beat = beat;
            this.distance = distance;

            for (float t = 0; t < 1; t += 1 / Math.Abs(Length) / 20)
            {
                Samples.Add(GetCurvePointGlobal(t));
            }
        }

        public Vector3 GetCurvePoint(float t)
        {
            float it = 1 - t;
            float it2 = it * it;
            float t2 = t * t;
            return Start * (it2 * it) + StartTangent * (3 * it2 * t) + EndTangent * (3 * it * t2) + End * (t2 * t);
        }
        /// <summary>
        /// get the global position of a point on the curve (0 <= t <= 1)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetCurvePointGlobal(float t)
        {
            return GetCurvePoint(t) + new Vector3(0, 0, Distance);
        }
        /// <summary>
        /// Approximate the position for a given z-axis distance
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public Vector3 ApproxPoint(float d)
        {
            // bisection method
            float a = 0;
            float b = 1;
            while (b - a > 1e-5f)
            {
                if (GetCurvePoint((a + b) / 2).z < d)
                {
                    a = (a + b) / 2;
                }
                else
                {
                    b = (b + a) / 2;
                }
            }
            return GetCurvePoint((a + b) / 2);
        }
    }

    [Serializable]
    public class BPMControl
    {
        private float beat;
        private float bpm;
        private string easing = "";

        public float Beat
        {
            get => beat;
            set => beat = value;
        }
        public float BPM
        {
            get => bpm;
            set => bpm = value;
        }
        public string Easing
        {
            get => easing;
            set => easing = value;
        }
    }

    [Serializable]
    public class PositionControl
    {
        private float beat;
        private Vector2 position = new(0, 0);
        private string easing = "";
        private float distance;

        public float Beat
        {
            get => beat;
            set => beat = value;
        }
        public Vector2 Position
        {
            get => position;
            set => position = value;
        }
        public string Easing
        {
            get => easing;
            set => easing = value;
        }
        public float Distance
        {
            get => distance;
            set => distance = value;
        }

        public PositionControl()
        {
            beat = 0;
            position = new(0, 0);
            easing = "easeInOut";
        }
    }

    [Serializable]
    public class AlphaControl
    {
        private float beat;
        private float alpha;
        private string easing = "";

        public float Beat
        {
            get => beat;
            set => beat = value;
        }
        public float Alpha
        {
            get => alpha;
            set => alpha = value;
        }
        public string Easing
        {
            get => easing;
            set => easing = value;
        }

        public AlphaControl()
        {
            beat = 0;
            alpha = 1;
            easing = "linear";
        }
    }

    [Serializable]
    public class SpeedControl
    {
        private float beat;
        private float speed;
        private string easing = "";

        public float Beat
        {
            get => beat;
            set => beat = value;
        }
        public float Speed
        {
            get => speed;
            set => speed = value;
        }
        public string Easing
        {
            get => easing;
            set => easing = value;
        }

        public SpeedControl()
        {
            beat = 0;
            speed = 1;
            easing = "hold";
        }

        public float GetSpeed()
        {
            return speed * 20;
        }
    }

    [Serializable]
    public class EndpointControl
    {
        private float beat;
        private float distance;

        public float Beat
        {
            get => beat;
            set => beat = value;
        }
        public float Distance
        {
            get => distance;
            set => distance = value;
        }
    }

    // This object handles the judgement of the note hit based on the attributes provided
    public class JudgeNote
    {
        // The targeted note
        public GameObject note;
        // Whether the hand is in front of the note
        public bool leftDetect, rightDetect;
        // The hand that triggers the hit detection. Depends on the type of note
        public int hitBy;
        // The timing to hit the note
        public float beat;

        // for tap
        public JudgeNote(GameObject note, int hitBy, bool leftDetect, bool rightDetect, float beat)
        {
            this.note = note;
            this.hitBy = hitBy;
            this.leftDetect = leftDetect;
            this.rightDetect = rightDetect;
            this.beat = beat;
        }
        // for clap
        public JudgeNote(GameObject note, float beat)
        {
            this.note = note;
            this.beat = beat;

            leftDetect = false;
            rightDetect = false;
            hitBy = 0;
        }
        // for punch
        public JudgeNote(GameObject note, bool leftDetect, bool rightDetect, float beat)
        {
            this.note = note;
            this.beat = beat;
            this.leftDetect = leftDetect;
            this.rightDetect = rightDetect;

            hitBy = 0;
        }
    }

    public struct HandPosRecord
    {
        public Vector2 Position;
        public readonly float time;

        public HandPosRecord(Vector2 pos, float time)
        {
            Position = pos;
            this.time = time;
        }
    }

    public class Performance
    {
        public int perfect;
        public int good;
        public int bad;
        public int miss;

        public int combo;
        public int maxCombo;

        public float difficulty;
        public float totalNotes;

        public int bestScore;
        public float bestAcc;

        public Performance()
        {
            perfect = 0;
            good = 0;
            bad = 0;
            miss = 0;
            combo = 0;
            maxCombo = 0;
        }

        public int Score()
        {
            float judge = (perfect + good * 0.6f) / totalNotes;
            float endure = maxCombo / totalNotes;
            float score = judge * 900000 + endure * 100000;
            return score >= 0 ? (int)Math.Round(score) : 0;
        }

        public float Accuracy()
        {
            return 0;
        }

        public float PerformanceScore()
        {
            return 0;
        }
    }
}