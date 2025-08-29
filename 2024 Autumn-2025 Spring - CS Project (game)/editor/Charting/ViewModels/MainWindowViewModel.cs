using ReactiveUI;

using Charting.Source;
using Charting.Source.Assist;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Charting.ViewModels;

// Contains Anything Data Related
public partial class MainWindowViewModel : ViewModelBase
{
    private Chart chart = new();
    private bool isLoaded = false;
    private bool canDelete = true;
    private EditMode editMode = EditMode.Unload;
    private Settings settings = new();
    private NodeType selectedNodeType = NodeType.Position;
    private CurrentNode cache = new();
    private List<double> melodyOnsets = new();

    public EditMode StoredEditMode = EditMode.Song;

    public Chart Chart {
        get => chart;
        set => this.RaiseAndSetIfChanged(ref chart, value);
    }
    public bool IsLoaded {
        get => isLoaded;
        set => this.RaiseAndSetIfChanged(ref isLoaded, value);
    }
    public bool CanDelete {
        get => canDelete;
        set => this.RaiseAndSetIfChanged(ref canDelete, value);
    }
    public EditMode EditMode {
        get => editMode;
        set => this.RaiseAndSetIfChanged(ref editMode, value);
    }
    public Settings Settings {
        get => settings;
        set => this.RaiseAndSetIfChanged(ref settings, value);
    }
    public CurrentNode Cache {
        get => cache;
        set => this.RaiseAndSetIfChanged(ref cache, value);
    }
    public NodeType SelectedNodeType {
        get => selectedNodeType;
        set => this.RaiseAndSetIfChanged(ref selectedNodeType, value);
    }
    public List<double> MelodyOnsets {
        get => melodyOnsets;
        set => melodyOnsets = value;
    }

    public void SyncEditMode() {
        EditMode = StoredEditMode;
    }
    public void CacheReset() {
        Cache.Lane = 0;
        Cache.BPMID = 0;
        Cache.PositionID = [.. new int[Chart.Lanes.Count]];
        Cache.AlphaID = [.. new int[Chart.Lanes.Count]];
        Cache.SpeedID = [.. new int[Chart.Lanes.Count]];
        Cache.EndpointID = [.. new int[Chart.Lanes.Count]];
        Cache.NoteID = [.. new int[Chart.Lanes.Count]];
        Cache.BPMControl = (Chart.BPMControl.Count > 0) ? Chart.BPMControl[0] : throw new Exception("BPM control node must have at least 1.");
        Cache.PositionControl = (Chart.Lanes[0].Nodes.PositionControl.Count > 0) ? Chart.Lanes[0].Nodes.PositionControl[0] : throw new Exception("Position Control must have at least 1.");
        Cache.AlphaControl = (Chart.Lanes[0].Nodes.AlphaControl.Count > 0) ? Chart.Lanes[0].Nodes.AlphaControl[0] : throw new Exception("Alpha Control must have at least 1.");
        Cache.SpeedControl = (Chart.Lanes[0].Nodes.SpeedControl.Count > 0) ? Chart.Lanes[0].Nodes.SpeedControl[0] : throw new Exception("Speed Control must have at least 1.");
        Cache.EndpointControl = (Chart.Lanes[0].Nodes.EndpointControl.Count > 0) ? Chart.Lanes[0].Nodes.EndpointControl[0] : null;
        Cache.Note = (Chart.Lanes[0].Notes.Count > 0) ? Chart.Lanes[0].Notes[0] : null;
    }
    public float BeatToViewDistance(float beat) {
        float second = Chart.Second(GetFirstBeat(), beat);
        return second * Settings.WindowScale * 400 + 200;
    }
    public float SecondToViewDistance(float second) {
        return (second - Chart.Second(GetFirstBeat())) * Settings.WindowScale * 400 + 200;
    }
    public float SecondToViewDistance(double second) {
        return (float)(second - Chart.Second(GetFirstBeat())) * Settings.WindowScale * 400 + 200;
    }
    public float ViewDistanceToBeat(double distance) {
        float second = (float)(distance - 200) / Settings.WindowScale / 400;
        return Chart.Beat(second) + GetFirstBeat();
    }
    public float GetFirstBeat() {
        float firstBeatOfControl = 0;
        switch(EditMode) {
            case EditMode.Unload:
                throw new ArgumentException("Invalid mode \"Unload\".");
            case EditMode.Song:
                if (Chart.BPMControl.Count > 0) {
                    firstBeatOfControl = Chart.BPMControl.OrderBy(x => x.Beat).ToList()[0].Beat;
                }
                break;
            case EditMode.Note:
                int laneID = cache.Lane;
                if (Chart.Lanes[laneID].Notes.Count > 0) {
                    firstBeatOfControl = Chart.Lanes[laneID].Notes.OrderBy(x => x.Beat).ToList()[0].Beat;
                }
                break;
            case EditMode.Control:
                laneID = cache.Lane;
                switch(selectedNodeType) {
                    case NodeType.Position:
                        if (Chart.Lanes[laneID].Nodes.PositionControl.Count > 0) {
                            firstBeatOfControl = Chart.Lanes[laneID].Nodes.PositionControl.OrderBy(x => x.Beat).ToList()[0].Beat;
                        }
                        break;
                    case NodeType.Alpha:
                        if (Chart.Lanes[laneID].Nodes.AlphaControl.Count > 0) {
                            firstBeatOfControl = Chart.Lanes[laneID].Nodes.AlphaControl.OrderBy(x => x.Beat).ToList()[0].Beat;
                        }
                        break;
                    case NodeType.Speed:
                        if (Chart.Lanes[laneID].Nodes.SpeedControl.Count > 0) {
                            firstBeatOfControl = Chart.Lanes[laneID].Nodes.SpeedControl.OrderBy(x => x.Beat).ToList()[0].Beat;
                        }
                        break;
                    case NodeType.Endpoint:
                        if (Chart.Lanes[laneID].Nodes.EndpointControl.Count > 0) {
                            firstBeatOfControl = Chart.Lanes[laneID].Nodes.EndpointControl.OrderBy(x => x.Beat).ToList()[0].Beat;
                        }
                        break;
                }
                break;
        }
        if (firstBeatOfControl > 0) firstBeatOfControl = 0;
        return firstBeatOfControl;
    }
    /// <summary>
    /// find the closest subbeat to a given beat
    /// </summary>
    /// <param name="beat"></param>
    /// <returns></returns>
    public float ToClosest(float beat) {
        int div = settings.Subdivision;
        return (float)Math.Round(beat * div) / div;
    }
    public void NewLane() {
        chart.NewLane();
        cache.Lane = chart.Lanes.Count - 1;
        cache.NoteID.Add(-1);
        cache.PositionID.Add(0);
        cache.AlphaID.Add(0);
        cache.SpeedID.Add(0);
        cache.EndpointID.Add(-1);
        cache.Note = null;
        cache.PositionControl = chart.Lanes[cache.Lane].Nodes.PositionControl[0];
        cache.AlphaControl = chart.Lanes[cache.Lane].Nodes.AlphaControl[0];
        cache.SpeedControl = chart.Lanes[cache.Lane].Nodes.SpeedControl[0];
        cache.EndpointControl = null;
    }
    public void DeleteLane() {
        chart.DeleteLane(cache.Lane);
        cache.NoteID.RemoveAt(cache.Lane);
        cache.PositionID.RemoveAt(cache.Lane);
        cache.AlphaID.RemoveAt(cache.Lane);
        cache.SpeedID.RemoveAt(cache.Lane);
        cache.EndpointID.RemoveAt(cache.Lane);
        if (chart.Lanes.Count > 0) {
            if (cache.Lane == chart.Lanes.Count) {
                cache.Lane--;
            }
            if (chart.Lanes[cache.Lane].Notes.Count > 0) {
                cache.Note = chart.Lanes[cache.Lane].Notes[cache.NoteID[cache.Lane]];
            }
            else {
                cache.Note = null;
            }
            cache.PositionControl = chart.Lanes[cache.Lane].Nodes.PositionControl[cache.PositionID[cache.Lane]];
            cache.AlphaControl = chart.Lanes[cache.Lane].Nodes.AlphaControl[cache.AlphaID[cache.Lane]];
            cache.SpeedControl = chart.Lanes[cache.Lane].Nodes.SpeedControl[cache.SpeedID[cache.Lane]];
            if (chart.Lanes[cache.Lane].Nodes.EndpointControl.Count > 0) {
                cache.PositionControl = chart.Lanes[cache.Lane].Nodes.PositionControl[cache.PositionID[cache.Lane]];
            }
            else {
                cache.PositionControl = null;
            }
        }
        else {
            NewLane();
        }
    }
}