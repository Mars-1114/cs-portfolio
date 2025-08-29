using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Platform.Storage;
using Avalonia.LogicalTree;

using Charting.ViewModels;
using Charting.Source;
using Charting.Models;
using Charting.Source.Assist;

namespace Charting.Views;

// Contains Anything Control Related
public partial class MainWindow : Window
{
    enum NotifType
    {
        SUCCESS,
        WARNING,
        ERROR
    }
    private readonly string directory = Environment.CurrentDirectory;
    private string folderPath = "";
    private string folderName = "";
    private Point lastMousePosition = new(0, 0);
    private Point rawPosition = new(0, 0);
    /// <summary>
    /// The duration of the song (in seconds)
    /// </summary>
    private double songLength = 0;
    private float storedDuration = 0;
    private bool mouseDown = false;
    private List<Key> pressedKey = [];

    // song play variables
    AudioHandler? audioHandler = null;
    bool isPlaying = false;

    int notifTask = 0;
    MainWindowViewModel editor => DataContext as MainWindowViewModel ?? throw new NullReferenceException("Cannot find view model.");
    public MainWindow()
    {
        InitializeComponent();
        SongPlayer_Timeline.AddHandler(PointerPressedEvent, StopPlaying, RoutingStrategies.Tunnel);
        preview.IsHitTestVisible = false;
        // set hot keys
        if (GetTopLevel(this) is TopLevel topLevel) {
            topLevel.AddHandler(KeyDownEvent, HotKeyEventManager, RoutingStrategies.Tunnel);
            topLevel.AddHandler(KeyUpEvent, RemoveKey, RoutingStrategies.Tunnel);
        }
        AutoSaveHandler();
    }

    public async void LoadChart(object? sender, RoutedEventArgs args)
    {
        // call the file opener
        if (GetTopLevel(this) is TopLevel topLevel)
        {
            // create folder
            Directory.CreateDirectory(directory);
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = false,
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(new Uri(directory + "\\Charts"))
            });

            // check if select any file
            if (folder.Count == 1)
            {
                folderPath = HttpUtility.UrlDecode(folder[0].Path.AbsolutePath);
                folderName = folder[0].Name;
                if (File.Exists(folderPath + "\\chart.json"))
                {
                    // read chart
                    var json = File.ReadAllText(folderPath + "\\chart.json");
                    if (JsonConvert.DeserializeObject<Chart>(json) is Chart chart)
                    {
                        if (File.Exists(folderPath + "\\song.mp3"))
                        {
                            editor.Settings.Timer = 0;
                            // delete controls (if any)
                            if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
                            {
                                RemoveAll<RadioButton>(canvas);
                                RemoveAll<Rectangle>(canvas);
                            }
                            editor.Chart = chart;
                            audioHandler = new(folderPath + "\\song.mp3");

                            // read song info
                            SongPlayer_Timeline.Maximum = (audioHandler.Length - editor.Chart.Offset) / 1000;
                            songLength = SongPlayer_Timeline.Maximum;
                            editor.Chart.SampleAll();
                            // Set current nodes to default
                            editor.CacheReset();
                            editor.MelodyOnsets = [];
                            editor.IsLoaded = true;
                            editor.SyncEditMode();
                            if (GetControl<Canvas>("timelineDisplayContainer") is not null)
                            {
                                Initialize();
                                GenerateLines();
                                preview.RemoveGrid();
                                if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
                                {
                                    GeneratePositionNodeOnPreview();
                                }
                                else
                                {
                                    RemoveAll<Rectangle>(previewContainer);
                                }
                            }
                            Notif("Chart loaded successfully!");
                        }
                        else
                        {
                            Notif("\"song.mp3\" does not exist in the folder", NotifType.ERROR);
                        }
                    }
                    else
                    {
                        Notif("Failed to parse chart. Check the format of the selected json", NotifType.ERROR);
                    }
                }
                else
                {
                    Notif("\"chart.json\" does not exist in the folder", NotifType.ERROR);
                }
            }
        }
        else
        {
            throw new NullReferenceException("Editor failed to find main window.");
        }
    }
    public void SaveChart(object? sender, RoutedEventArgs args)
    {
        Save(folderPath);
    }
    private void Save(string path)
    {
        if (editor.IsLoaded)
        {
            Directory.CreateDirectory(path);
            Chart output = editor.Chart.GetSortedChart();
            string json = JsonConvert.SerializeObject(output, Formatting.Indented);
            try
            {
                File.WriteAllText(path + "\\chart.json", json);
                Notif("Chart Saved!");
            }
            catch
            {
                Notif("Save failed. Please try again", NotifType.ERROR);
            }
        }
    }
    public async void NewChart(object? sender, RoutedEventArgs args)
    {
        // call the file opener
        var topLevel = GetTopLevel(this);
        if (topLevel != null)
        {
            var song = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Song",
                AllowMultiple = false,
                FileTypeFilter = [
                    new("mp3") {
                        MimeTypes = ["audio/mpeg"],
                        Patterns = ["*.mp3"]
                    }
                ]
            });

            // check if select any file
            if (song.Count == 1)
            {
                editor.Settings.Timer = 0;
                editor.IsLoaded = false;
                // delete controls (if any)
                if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
                {
                    RemoveAll<RadioButton>(canvas);
                    RemoveAll<Rectangle>(canvas);
                }
                // create folders
                Directory.CreateDirectory(directory);
                folderPath = directory + "\\Charts\\" + song[0].Name[..^4];
                folderName = song[0].Name[..^4];
                Directory.CreateDirectory(folderPath);
                // copy song to directory
                File.Copy(HttpUtility.UrlDecode(song[0].Path.AbsolutePath), folderPath + "\\song.mp3");
                // prepare song player
                audioHandler = new(folderPath + "\\song.mp3");
                // read song info
                SongPlayer_Timeline.Maximum = audioHandler.Length / 1000;
                songLength = SongPlayer_Timeline.Maximum;

                // create chart
                editor.Chart = new()
                {
                    Name = song[0].Name[..^4],
                    Artist = "",
                    Offset = 0,
                    Difficulty = 0,
                    BPMControl = [new() {
                        Beat = 0,
                        BPM = 128,
                        Easing = "hold"
                    }],
                    Lanes = []
                };
                editor.Chart.NewLane();

                // save to json
                Chart output = editor.Chart.GetSortedChart();
                string json = JsonConvert.SerializeObject(output, Formatting.Indented);
                File.WriteAllText(folderPath + "\\chart.json", json);
                editor.Chart.SampleAll();

                // Set current nodes to default
                editor.CacheReset();
                editor.MelodyOnsets = [];
                editor.IsLoaded = true;
                editor.SyncEditMode();
                if (GetControl<Canvas>("timelineDisplayContainer") is not null)
                {
                    Initialize();
                    GenerateLines();
                    if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
                    {
                        GeneratePositionNodeOnPreview();
                    }
                    else
                    {
                        preview.RemoveGrid();
                        RemoveAll<Rectangle>(previewContainer);
                    }
                }
                Notif("Chart created successfully!");
            }
        }
        else
        {
            throw new NullReferenceException("Editor failed to find main window.");
        }
    }
    public void ChangeEditMode(object sender, RoutedEventArgs e)
    {
        if (sender is Control src)
        {
            switch (src.Name)
            {
                case "EditSong":
                    editor.StoredEditMode = EditMode.Song;
                    break;
                case "EditNote":
                    editor.StoredEditMode = EditMode.Note;
                    break;
                case "EditControl":
                    editor.StoredEditMode = EditMode.Control;
                    break;
                default:
                    throw new ArgumentException($"Invalid source of function caller: \"{src}\"");
            }
            if (editor.IsLoaded)
            {
                editor.SyncEditMode();
            }
        }
        else
        {
            throw new NullReferenceException("Function caller must be a control.");
        }
    }

    // Load Event (Equivalent to AttachedToVisualTree Event)
    public void InitializeGraph(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (sender is Canvas)
        {
            // Initialize Window
            if (editor.IsLoaded)
            {
                SetTimelineWidth();
                Initialize();
                // draw lines
                GenerateLines();
                Render();
                if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
                {
                    GeneratePositionNodeOnPreview();
                }
                else
                {
                    preview.RemoveGrid();
                    RemoveAll<Rectangle>(previewContainer);
                }
                SetNoteDetailWindow();
                SetScrollerValue("timeline", editor.BeatToViewDistance(editor.Chart.Beat((float)GetSliderValue())) - 200);
                if (GetControl<NumericUpDown>("LaneSwitch") is NumericUpDown laneControl) {
                    laneControl.Value = Math.Max(editor.Cache.Lane, 1);
                }
                // add wheel handler to timeline
                if (GetControl<ScrollViewer>("timeline") is ScrollViewer scrollViewer) {
                    scrollViewer.AddHandler(PointerWheelChangedEvent, ScrollHorizontally, RoutingStrategies.Tunnel);
                }
            }
        }
        else
        {
            throw new NullReferenceException("Function caller must be a canvas.");
        }
    }
    private void Initialize()
    {
        if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
        {
            RemoveAll<RadioButton>(canvas);
            RemoveAll<Rectangle>(canvas);
            var settings = editor.Settings;
            switch (editor.EditMode)
            {
                case EditMode.Song:
                    // create radio buttons
                    for (int i = 0; i < editor.Chart.BPMControl.Count; i++)
                    {
                        var bpmControl = editor.Chart.BPMControl[i];
                        var bpmNode = new RadioButton
                        {
                            GroupName = "BPM",
                            Width = 20,
                            Height = 32,
                            CornerRadius = CornerRadius.Parse("10")
                        };
                        // Set Position
                        Canvas.SetLeft(bpmNode, editor.BeatToViewDistance(bpmControl.Beat));
                        Canvas.SetTop(bpmNode, 10);
                        // Set Index of The Referenced BPMNode in Chart
                        Property.SetIndex(bpmNode, i);
                        if (i == editor.Cache.BPMID)
                        {
                            bpmNode.IsChecked = true;
                        }
                        // Attach DragDrop Events
                        bpmNode.AddHandler(PointerMovedEvent, EventMouseMove, RoutingStrategies.Tunnel);
                        bpmNode.AddHandler(PointerPressedEvent, EventMouseDown, RoutingStrategies.Tunnel);
                        bpmNode.AddHandler(PointerReleasedEvent, EventMouseUp, RoutingStrategies.Tunnel);
                        // Attach to Canvas
                        canvas.Children.Add(bpmNode);
                    }
                    break;
                case EditMode.Note:
                    // create radio buttons
                    int laneID = editor.Cache.Lane;
                    for (int i = 0; i < editor.Chart.Lanes[laneID].Notes.Count; i++)
                    {
                        var noteControl = editor.Chart.Lanes[laneID].Notes[i];
                        var noteNode = new RadioButton
                        {
                            GroupName = "Note",
                            Width = 20,
                            Height = 32,
                            CornerRadius = CornerRadius.Parse("10"),
                            Background = new SolidColorBrush(Helper.GetColor(noteControl.Type))
                        };
                        // Set Position
                        Canvas.SetLeft(noteNode, editor.BeatToViewDistance(noteControl.Beat));
                        Canvas.SetTop(noteNode, 10);

                        // Set Index of The Referenced BPMNode in Chart
                        Property.SetIndex(noteNode, i);
                        if (i == editor.Cache.NoteID[laneID])
                        {
                            noteNode.IsChecked = true;
                        }
                        // Attach DragDrop Events
                        noteNode.AddHandler(PointerPressedEvent, EventMouseDown, RoutingStrategies.Tunnel);
                        noteNode.AddHandler(PointerMovedEvent, EventMouseMove, RoutingStrategies.Tunnel);
                        noteNode.AddHandler(PointerReleasedEvent, EventMouseUp, RoutingStrategies.Tunnel);
                        // Attach to Canvas
                        canvas.Children.Add(noteNode);
                        // if note is track
                        if (noteControl.Type == "track")
                        {
                            var trackTrail = new Rectangle
                            {
                                Fill = new SolidColorBrush(Helper.GetColor("track")),
                                Height = 15,
                                Width = editor.BeatToViewDistance(noteControl.Beat + noteControl.Duration) - editor.BeatToViewDistance(noteControl.Beat),
                                StrokeJoin = PenLineJoin.Round,
                                ZIndex = -1
                            };
                            Canvas.SetLeft(trackTrail, editor.BeatToViewDistance(noteControl.Beat) + 10);
                            Canvas.SetTop(trackTrail, 17);
                            Property.SetTrackNodeTail(noteNode, trackTrail);
                            canvas.Children.Add(trackTrail);
                        }
                    }
                    break;
                case EditMode.Control:
                    switch (editor.SelectedNodeType)
                    {
                        case NodeType.Position:
                            // create radio buttons
                            laneID = editor.Cache.Lane;
                            for (int i = 0; i < editor.Chart.Lanes[laneID].Nodes.PositionControl.Count; i++)
                            {
                                var nodeControl = editor.Chart.Lanes[laneID].Nodes.PositionControl[i];
                                var positionNode = new RadioButton
                                {
                                    GroupName = "Position",
                                    Width = 20,
                                    Height = 32,
                                    CornerRadius = CornerRadius.Parse("10")
                                };
                                // Set Position
                                Canvas.SetLeft(positionNode, editor.BeatToViewDistance(nodeControl.Beat));
                                Canvas.SetTop(positionNode, 10);

                                // Set Index of The Referenced BPMNode in Chart
                                Property.SetIndex(positionNode, i);
                                if (i == editor.Cache.PositionID[laneID])
                                {
                                    positionNode.IsChecked = true;
                                }
                                // Attach DragDrop Events & Set Immovable (First Node)
                                positionNode.AddHandler(PointerPressedEvent, EventMouseDown, RoutingStrategies.Tunnel);
                                positionNode.AddHandler(PointerMovedEvent, EventMouseMove, RoutingStrategies.Tunnel);
                                positionNode.AddHandler(PointerReleasedEvent, EventMouseUp, RoutingStrategies.Tunnel);
                                // Attach to Canvas
                                canvas.Children.Add(positionNode);
                            }
                            break;
                        case NodeType.Alpha:
                            // create radio buttons
                            laneID = editor.Cache.Lane;
                            for (int i = 0; i < editor.Chart.Lanes[laneID].Nodes.AlphaControl.Count; i++)
                            {
                                var nodeControl = editor.Chart.Lanes[laneID].Nodes.AlphaControl[i];
                                var alphaNode = new RadioButton
                                {
                                    GroupName = "Alpha",
                                    Width = 20,
                                    Height = 32,
                                    CornerRadius = CornerRadius.Parse("10")
                                };
                                // Set Position
                                Canvas.SetLeft(alphaNode, editor.BeatToViewDistance(nodeControl.Beat));
                                Canvas.SetTop(alphaNode, 10);

                                // Set Index of The Referenced BPMNode in Chart
                                Property.SetIndex(alphaNode, i);
                                if (i == editor.Cache.AlphaID[laneID])
                                {
                                    alphaNode.IsChecked = true;
                                }
                                // Attach DragDrop Events
                                alphaNode.AddHandler(PointerPressedEvent, EventMouseDown, RoutingStrategies.Tunnel);
                                alphaNode.AddHandler(PointerMovedEvent, EventMouseMove, RoutingStrategies.Tunnel);
                                alphaNode.AddHandler(PointerReleasedEvent, EventMouseUp, RoutingStrategies.Tunnel);
                                // Attach to Canvas
                                canvas.Children.Add(alphaNode);
                            }
                            break;
                        case NodeType.Speed:
                            // create radio buttons
                            laneID = editor.Cache.Lane;
                            for (int i = 0; i < editor.Chart.Lanes[laneID].Nodes.SpeedControl.Count; i++)
                            {
                                var nodeControl = editor.Chart.Lanes[laneID].Nodes.SpeedControl[i];
                                var speedNode = new RadioButton
                                {
                                    GroupName = "Speed",
                                    Width = 20,
                                    Height = 32,
                                    CornerRadius = CornerRadius.Parse("10")
                                };
                                // Set Position
                                Canvas.SetLeft(speedNode, editor.BeatToViewDistance(nodeControl.Beat));
                                Canvas.SetTop(speedNode, 10);

                                // Set Index of The Referenced BPMNode in Chart
                                Property.SetIndex(speedNode, i);
                                if (i == editor.Cache.SpeedID[laneID])
                                {
                                    speedNode.IsChecked = true;
                                }
                                // Attach DragDrop Events
                                speedNode.AddHandler(PointerPressedEvent, EventMouseDown, RoutingStrategies.Tunnel);
                                speedNode.AddHandler(PointerMovedEvent, EventMouseMove, RoutingStrategies.Tunnel);
                                speedNode.AddHandler(PointerReleasedEvent, EventMouseUp, RoutingStrategies.Tunnel);
                                // Attach to Canvas
                                canvas.Children.Add(speedNode);
                            }
                            break;
                        case NodeType.Endpoint:
                            // create radio buttons
                            laneID = editor.Cache.Lane;
                            for (int i = 0; i < editor.Chart.Lanes[laneID].Nodes.EndpointControl.Count; i++)
                            {
                                var nodeControl = editor.Chart.Lanes[laneID].Nodes.EndpointControl[i];
                                var endpointNode = new RadioButton
                                {
                                    GroupName = "Endpoint",
                                    Width = 20,
                                    Height = 32,
                                    CornerRadius = CornerRadius.Parse("10")
                                };
                                // Set Position
                                Canvas.SetLeft(endpointNode, editor.BeatToViewDistance(nodeControl.Beat));
                                Canvas.SetTop(endpointNode, 10);

                                // Set Index of The Referenced BPMNode in Chart
                                Property.SetIndex(endpointNode, i);
                                if (i == editor.Cache.EndpointID[laneID])
                                {
                                    endpointNode.IsChecked = true;
                                }
                                endpointNode.AddHandler(PointerPressedEvent, EventMouseDown, RoutingStrategies.Tunnel);
                                endpointNode.AddHandler(PointerMovedEvent, EventMouseMove, RoutingStrategies.Tunnel);
                                endpointNode.AddHandler(PointerReleasedEvent, EventMouseUp, RoutingStrategies.Tunnel);
                                // Attach to Canvas
                                canvas.Children.Add(endpointNode);
                            }
                            break;
                            throw new NotImplementedException();
                    }
                    break;
            }
            EnableDeletion();
        }
    }

    //
    private void EventMouseDown(object? sender, PointerPressedEventArgs args)
    {
        if (sender is RadioButton src)
        {
            if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
            {
                src.IsChecked = true;
                mouseDown = true;
                lastMousePosition = args.GetCurrentPoint(canvas).Position;
                rawPosition = new(Canvas.GetLeft(src), 0);

                // Change Cache Reference
                UpdateCache(src);

                if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
                {
                    GeneratePositionNodeOnPreview();
                }
            }
        }
        else
        {
            throw new NullReferenceException("Function caller must be a radio button.");
        }
    }
    private void EventMouseMove(object? sender, PointerEventArgs args)
    {
        if (sender is RadioButton src)
        {
            if (mouseDown && GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
            {
                Settings settings = editor.Settings;
                var point = args.GetCurrentPoint(canvas).Position;
                rawPosition = rawPosition + point - lastMousePosition;

                // beat snap
                var setBeat = editor.ViewDistanceToBeat(rawPosition.X - 10);
                if (settings.IsBeatSnap)
                {
                    setBeat = (float)Math.Round(setBeat * settings.Subdivision) / settings.Subdivision;
                }
                var expectedPositionX = editor.BeatToViewDistance(setBeat);
                if (setBeat < editor.GetFirstBeat())
                {
                    setBeat = editor.GetFirstBeat();
                }
                // out of bounds
                if (expectedPositionX < 200)
                {
                    expectedPositionX = 200;
                }
                else if (expectedPositionX > editor.SecondToViewDistance(songLength))
                {
                    expectedPositionX = editor.SecondToViewDistance(songLength);
                }
                if (args.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed)
                {
                    Canvas.SetLeft(src, expectedPositionX);
                    switch (editor.EditMode)
                    {
                        case EditMode.Song:
                            if (editor.Cache.BPMControl is BPMControl bpm)
                            {
                                if (bpm.Beat != setBeat)
                                {
                                    bpm.Beat = setBeat;
                                    SetTimelineWidth();
                                }
                                break;
                            }
                            else
                            {
                                throw new NullReferenceException("Selected node has no corresponding data");
                            }
                        case EditMode.Note:
                            if (editor.Cache.Note is Note note)
                            {
                                // if track note
                                if (note.Beat != setBeat)
                                {
                                    if (note.Type == "track")
                                    {
                                        if (Property.GetTrackNoteTail(src) is Rectangle noteTail)
                                        {
                                            Canvas.SetLeft(noteTail, expectedPositionX + 10);
                                        }
                                        else
                                        {
                                            throw new Exception("No attached rectangle found for the selected track note.");
                                        }
                                    }
                                    note.Beat = setBeat;
                                }
                                break;
                            }
                            else
                            {
                                throw new NullReferenceException("Selected note has no corresponding data");
                            }
                        case EditMode.Control:
                            switch (editor.SelectedNodeType)
                            {
                                case NodeType.Position:
                                    if (editor.Cache.PositionControl is PositionControl pos)
                                    {
                                        if (pos.Beat != setBeat)
                                        {
                                            pos.Beat = setBeat;
                                            GeneratePositionNodeOnPreview();
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        throw new NullReferenceException("Selected node has no corresponding data");
                                    }
                                case NodeType.Alpha:
                                    if (editor.Cache.AlphaControl is AlphaControl alpha)
                                    {
                                        if (alpha.Beat != setBeat)
                                        {
                                            alpha.Beat = setBeat;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        throw new NullReferenceException("Selected node has no corresponding data");
                                    }
                                case NodeType.Speed:
                                    if (editor.Cache.SpeedControl is SpeedControl speed)
                                    {
                                        if (speed.Beat != setBeat)
                                        {
                                            speed.Beat = setBeat;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        throw new NullReferenceException("Selected node has no corresponding data");
                                    }
                                case NodeType.Endpoint:
                                    if (editor.Cache.EndpointControl is EndpointControl endpoint)
                                    {
                                        if (endpoint.Beat != setBeat)
                                        {
                                            endpoint.Beat = setBeat;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        throw new NullReferenceException("Selected node has no corresponding data");
                                    }
                            }
                            break;
                    }
                }
                else
                {
                    if (editor.EditMode == EditMode.Note)
                    {
                        if (editor.Cache.Note is Note note)
                        {
                            if (note.Type == "track")
                            {
                                if (Property.GetTrackNoteTail(src) is Rectangle noteTail)
                                {
                                    editor.Cache.Note.Duration = setBeat - editor.Cache.Note.Beat + storedDuration;
                                    if (editor.Cache.Note.Duration < 0) editor.Cache.Note.Duration = 0;
                                    editor.Cache.Note.Duration = editor.ToClosest(editor.Cache.Note.Duration + editor.Cache.Note.Beat) - editor.Cache.Note.Beat;
                                    noteTail.Width = editor.BeatToViewDistance(editor.Cache.Note.Beat + editor.Cache.Note.Duration) - editor.BeatToViewDistance(editor.Cache.Note.Beat);
                                }
                                else
                                {
                                    throw new NullReferenceException("No attached rectangle found for the selected track note.");
                                }
                            }
                        }
                        else
                        {
                            throw new NullReferenceException("Selected note has no correspoinding data");
                        }
                    }
                }
                lastMousePosition = point;
                UpdateDisplay();
            }
        }
        else
        {
            throw new NullReferenceException("Function caller must be a radio button.");
        }
    }
    private void EventMouseUp(object? sender, PointerReleasedEventArgs args)
    {
        mouseDown = false;
    }

    // Set Node Position
    private void SetNodePositions()
    {
        if (IsLoaded)
        {
            if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
            {
                var nodes = canvas.Children.OfType<RadioButton>().ToList();
                int laneID = editor.Cache.Lane;
                switch (editor.EditMode)
                {
                    case EditMode.Song:
                        foreach (var bpmNode in nodes)
                        {
                            Canvas.SetLeft(bpmNode, editor.BeatToViewDistance(editor.Chart.BPMControl[Property.GetIndex(bpmNode)].Beat));
                        }
                        break;
                    case EditMode.Note:
                        foreach (var noteNode in nodes)
                        {
                            var note = editor.Chart.Lanes[laneID].Notes[Property.GetIndex(noteNode)];
                            Canvas.SetLeft(noteNode, editor.BeatToViewDistance(note.Beat));
                            // if track note
                            if (Property.GetTrackNoteTail(noteNode) is Rectangle noteTail)
                            {
                                noteTail.Width = editor.BeatToViewDistance(note.Beat + note.Duration) - editor.BeatToViewDistance(note.Beat);
                                Canvas.SetLeft(noteTail, Canvas.GetLeft(noteNode) + 10);
                            }
                        }
                        break;
                    case EditMode.Control:
                        switch (editor.SelectedNodeType)
                        {
                            case NodeType.Position:
                                foreach (var positionNode in nodes)
                                {
                                    Canvas.SetLeft(positionNode, editor.BeatToViewDistance(editor.Chart.Lanes[laneID].Nodes.PositionControl[Property.GetIndex(positionNode)].Beat));
                                }
                                break;
                            case NodeType.Alpha:
                                foreach (var alphaNode in nodes)
                                {
                                    Canvas.SetLeft(alphaNode, editor.BeatToViewDistance(editor.Chart.Lanes[laneID].Nodes.AlphaControl[Property.GetIndex(alphaNode)].Beat));
                                }
                                break;
                            case NodeType.Speed:
                                foreach (var speedNode in nodes)
                                {
                                    Canvas.SetLeft(speedNode, editor.BeatToViewDistance(editor.Chart.Lanes[laneID].Nodes.SpeedControl[Property.GetIndex(speedNode)].Beat));
                                }
                                break;
                            case NodeType.Endpoint:
                                foreach (var endpointNode in nodes)
                                {
                                    Canvas.SetLeft(endpointNode, editor.BeatToViewDistance(editor.Chart.Lanes[laneID].Nodes.EndpointControl[Property.GetIndex(endpointNode)].Beat));
                                }
                                break;
                        }
                        break;
                }
            }
        }
    }
    public void UpdateWindow(object? sender, TextChangedEventArgs args)
    {
        if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas && sender is TextBox src)
        {
            if (canvas.Children.OfType<RadioButton>().ToList().Count > 0)
            {
                if (src.IsFocused) {
                    if (editor.EditMode == EditMode.Song)
                    {
                        SetTimelineWidth();
                    }
                    else
                    {
                        SetNodePositions();
                    }
                    UpdateDisplay();
                    if (GetControl<TextBox>("PosBeat") is TextBox posBeat)
                    {
                        if (posBeat.IsFocused)
                        {
                            GeneratePositionNodeOnPreview();
                        }
                    }
                }
            }

        }
    }
    public void UpdateWindow(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (GetControl<Canvas>("timelineDisplayContainer") is Canvas)
        {
            if (args.Property.Name == "Value")
            {
                if (((Control)args.Sender).Name == "LaneSwitch")
                {
                    UpdateCache();
                    Initialize();
                }
                else
                {
                    GenerateLines();
                }
            }
        }
    }

    public void GenerateLines()
    {
        if (GetControl<Renderer>("timelineDisplay") is Renderer timeline && GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas && audioHandler is not null)
        {
            // Clear Lines
            timeline.RemoveAll();
            RemoveAll<TextBlock>(canvas);
            var drawToBeat = editor.Chart.Beat((float)songLength);
            var subdivision = editor.Settings.Subdivision;
            for (int i = (int)Math.Floor(editor.GetFirstBeat()) * subdivision; i < (int)Math.Ceiling(drawToBeat) * subdivision; i++)
            {
                // add lines
                var beatLine = new LinePoint(
                    new Source.Vector2(10 + editor.BeatToViewDistance((float)i / subdivision), 0),
                    new Source.Vector2(10 + editor.BeatToViewDistance((float)i / subdivision), 50),
                    (i % subdivision == 0) ? Colors.Cyan : Colors.LightGray,
                    1
                );
                timeline.lines.Add(beatLine);
                // add labels
                if (i % subdivision == 0)
                {
                    var beatLabel = new TextBlock
                    {
                        Text = (i / subdivision).ToString(),
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Colors.Cyan),
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    Canvas.SetLeft(beatLabel, 10 + editor.BeatToViewDistance((float)i / subdivision));
                    Canvas.SetTop(beatLabel, 50);
                    canvas.Children.Add(beatLabel);
                }
            }
            // add onsets
            int id = 0;
            foreach (var onsets in editor.MelodyOnsets) {
                if (onsets > 0) {
                    double second = id * audioHandler.TimePerInstant;
                    float view_distance = editor.SecondToViewDistance(second - editor.Chart.Offset / 1000);
                    var rect = new RectProperty(
                        new Rect(10 + view_distance, 40, 20, 10),
                        Colors.OrangeRed,
                        Colors.DarkOrange,
                        1
                    );
                    timeline.rectangles.Add(rect);
                }
                id++;
            }
        }
        else
        {
            throw new NullReferenceException("Timeline display is not set to a canvas.");
        }
    }
    public void WindowZoomIn(object? sender, RoutedEventArgs args)
    {
        Zoom(editor.Settings.WindowScale / 0.8f);
    }
    public void WindowZoomOut(object? sender, RoutedEventArgs args)
    {
        Zoom(editor.Settings.WindowScale * 0.8f);
    }
    public void WindowZoomReset(object? sender, RoutedEventArgs args)
    {
        Zoom(1);
    }
    private void Zoom(float scale)
    {
        if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
        {
            if (canvas.Parent is ScrollViewer scroller)
            {
                var offset = scroller.Offset.X + 200;
                var beat = offset == 0 ? editor.GetFirstBeat() : editor.ViewDistanceToBeat(offset);
                editor.Settings.WindowScale = scale;
                SetTimelineWidth();
                scroller.Offset = scroller.Offset.WithX(editor.BeatToViewDistance(beat) - 200);
            }
            else
            {
                throw new NullReferenceException("Timeline display is not set inside a ScrollViewer.");
            }
        }
    }
    public void SetOffset(object? sender, ScrollChangedEventArgs args)
    {
        if (sender is ScrollViewer scroller)
        {
            if (!isPlaying && audioHandler is not null)
            {
                SetSliderValue(editor.Chart.Second(editor.ViewDistanceToBeat(scroller.Offset.X + 200)));
                audioHandler.StartTime = (long)(editor.Chart.Second(editor.ViewDistanceToBeat(scroller.Offset.X + 200)) * 1000);
                Render();
                GeneratePositionNodeOnPreview();
            }
        }
        else
        {
            throw new NullReferenceException("Function caller must be a scroll viewer.");
        }
    }
    public void AddNode(object? sender, RoutedEventArgs args)
    {
        if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
        {
            if (canvas.Parent is ScrollViewer scroller)
            {
                var offset = scroller.Offset.X;
                var beat = editor.ToClosest(editor.ViewDistanceToBeat(scroller.Offset.X + 200));
                // insert control node to chart
                string groupName = "";
                int index = 0;
                switch (editor.EditMode)
                {
                    case EditMode.Song:
                        groupName = "BPM";
                        var newNode = new BPMControl
                        {
                            Beat = beat,
                            BPM = editor.Chart.BPM(beat),
                            Easing = ((BPMControl)GetSelectedNode()).Easing
                        };
                        editor.Chart.BPMControl.Add(newNode);
                        // update cache
                        editor.Cache.BPMID = editor.Chart.BPMControl.Count - 1;
                        editor.Cache.BPMControl = editor.Chart.BPMControl[^1];
                        index = editor.Chart.BPMControl.Count - 1;
                        SetTimelineWidth();
                        break;
                    case EditMode.Note:
                        groupName = "Note";
                        int laneID = editor.Cache.Lane;
                        var newNote = new Note
                        {
                            Beat = beat,
                            Type = "tap",
                            Duration = 1
                        };
                        editor.Chart.Lanes[laneID].Notes.Add(newNote);
                        // update cache
                        editor.Cache.NoteID[laneID] = editor.Chart.Lanes[laneID].Notes.Count - 1;
                        editor.Cache.Note = editor.Chart.Lanes[laneID].Notes[^1];
                        index = editor.Chart.Lanes[laneID].Notes.Count - 1;
                        break;
                    case EditMode.Control:
                        laneID = editor.Cache.Lane;
                        switch (editor.SelectedNodeType)
                        {
                            case NodeType.Position:
                                groupName = "Position";
                                var newPositionNode = new PositionControl
                                {
                                    Beat = beat,
                                    Position = ((PositionControl)GetSelectedNode()).Position,
                                    Easing = "easeInOut",
                                    Distance = editor.Chart.Distance(laneID, beat)
                                };
                                editor.Chart.Lanes[laneID].Nodes.PositionControl.Add(newPositionNode);
                                // update cache
                                editor.Cache.PositionID[laneID] = editor.Chart.Lanes[laneID].Nodes.PositionControl.Count - 1;
                                editor.Cache.PositionControl = editor.Chart.Lanes[laneID].Nodes.PositionControl[^1];
                                index = editor.Chart.Lanes[laneID].Nodes.PositionControl.Count - 1;
                                GeneratePositionNodeOnPreview();
                                break;
                            case NodeType.Alpha:
                                groupName = "Alpha";
                                var newAlphaNode = new AlphaControl
                                {
                                    Beat = beat,
                                    Alpha = editor.Chart.Alpha(laneID, beat),
                                    Easing = ((AlphaControl)GetSelectedNode()).Easing
                                };
                                editor.Chart.Lanes[laneID].Nodes.AlphaControl.Add(newAlphaNode);
                                // update cache
                                editor.Cache.AlphaID[laneID] = editor.Chart.Lanes[laneID].Nodes.AlphaControl.Count - 1;
                                editor.Cache.AlphaControl = editor.Chart.Lanes[laneID].Nodes.AlphaControl[^1];
                                index = editor.Chart.Lanes[laneID].Nodes.AlphaControl.Count - 1;
                                break;
                            case NodeType.Speed:
                                groupName = "Speed";
                                var newSpeedNode = new SpeedControl
                                {
                                    Beat = beat,
                                    Speed = ((SpeedControl)GetSelectedNode()).Speed,
                                    Easing = ((SpeedControl)GetSelectedNode()).Easing
                                };
                                editor.Chart.Lanes[laneID].Nodes.SpeedControl.Add(newSpeedNode);
                                // update cache
                                editor.Cache.SpeedID[laneID] = editor.Chart.Lanes[laneID].Nodes.SpeedControl.Count - 1;
                                editor.Cache.SpeedControl = editor.Chart.Lanes[laneID].Nodes.SpeedControl[^1];
                                index = editor.Chart.Lanes[laneID].Nodes.SpeedControl.Count - 1;
                                break;
                            case NodeType.Endpoint:
                                groupName = "Endpoint";
                                var newEndpointNode = new EndpointControl
                                {
                                    Beat = beat,
                                    Distance = editor.Chart.Distance(laneID, beat)
                                };
                                editor.Chart.Lanes[laneID].Nodes.EndpointControl.Add(newEndpointNode);
                                // update cache
                                editor.Cache.EndpointID[laneID] = editor.Chart.Lanes[laneID].Nodes.EndpointControl.Count - 1;
                                editor.Cache.EndpointControl = editor.Chart.Lanes[laneID].Nodes.EndpointControl[^1];
                                index = editor.Chart.Lanes[laneID].Nodes.EndpointControl.Count - 1;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                }
                // create radio button
                RadioButton newNodeControl = new RadioButton
                {
                    GroupName = groupName,
                    Width = 20,
                    Height = 32,
                    CornerRadius = CornerRadius.Parse("10")
                };
                if (editor.EditMode == EditMode.Note && editor.Cache.Note is Note note)
                {
                    newNodeControl.Background = new SolidColorBrush(Helper.GetColor(note.Type));
                }
                Canvas.SetTop(newNodeControl, 10);
                Canvas.SetLeft(newNodeControl, editor.BeatToViewDistance(beat));
                Property.SetIndex(newNodeControl, index);
                // Attach DragDrop Events
                newNodeControl.AddHandler(PointerPressedEvent, EventMouseDown, RoutingStrategies.Tunnel);
                newNodeControl.AddHandler(PointerMovedEvent, EventMouseMove, RoutingStrategies.Tunnel);
                newNodeControl.AddHandler(PointerReleasedEvent, EventMouseUp, RoutingStrategies.Tunnel);
                // attach to canvas
                canvas.Children.Add(newNodeControl);
                // set to checked
                newNodeControl.IsChecked = true;

                EnableDeletion();
                UpdateDisplay();
            }
            else
            {
                throw new NullReferenceException("Timeline display is not set inside a ScrollViewer.");
            }
        }
    }
    public void DeleteNode(object? sender, RoutedEventArgs args)
    {
        if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
        {
            int id = 0;
            switch (editor.EditMode)
            {
                case EditMode.Song:
                    id = editor.Cache.BPMID;
                    break;
                case EditMode.Note:
                    id = editor.Cache.NoteID[editor.Cache.Lane];
                    break;
                case EditMode.Control:
                    switch (editor.SelectedNodeType)
                    {
                        case NodeType.Position:
                            id = editor.Cache.PositionID[editor.Cache.Lane];
                            break;
                        case NodeType.Alpha:
                            id = editor.Cache.AlphaID[editor.Cache.Lane];
                            break;
                        case NodeType.Speed:
                            id = editor.Cache.SpeedID[editor.Cache.Lane];
                            break;
                        case NodeType.Endpoint:
                            id = editor.Cache.SpeedID[editor.Cache.Lane];
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
            }
            // find node
            var nodes = canvas.Children.OfType<RadioButton>().OrderBy(Canvas.GetLeft).ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                int targetID = Property.GetIndex(nodes[i]);
                if (targetID == id)
                {
                    RadioButton newSelectedControl = nodes[i];
                    switch (editor.EditMode)
                    {
                        case EditMode.Song:
                            editor.Chart.BPMControl.RemoveAt(id);
                            canvas.Children.Remove(nodes[i]);
                            if (editor.Chart.BPMControl.Count > 0)
                            {
                                if (i == 0)
                                {
                                    newSelectedControl = nodes[i + 1];
                                }
                                else
                                {
                                    newSelectedControl = nodes[i - 1];
                                }
                                newSelectedControl.IsChecked = true;
                                // update index
                                foreach (var node in nodes)
                                {
                                    int oldID = Property.GetIndex(node);
                                    if (oldID > targetID)
                                    {
                                        Property.SetIndex(node, oldID - 1);
                                    }
                                }
                            }
                            // remove reference of control
                            nodes.RemoveAt(i);
                            SetTimelineWidth();
                            break;
                        case EditMode.Note:
                            int laneID = editor.Cache.Lane;
                            // if track note
                            if (Property.GetTrackNoteTail(nodes[i]) is Rectangle noteTail)
                            {
                                canvas.Children.Remove(noteTail);
                            }
                            editor.Chart.Lanes[laneID].Notes.RemoveAt(id);
                            canvas.Children.Remove(nodes[i]);
                            if (editor.Chart.Lanes[laneID].Notes.Count > 0)
                            {
                                // update cache
                                if (i == 0)
                                {
                                    newSelectedControl = nodes[i + 1];
                                }
                                else
                                {
                                    newSelectedControl = nodes[i - 1];
                                }
                                newSelectedControl.IsChecked = true;
                                // update index
                                foreach (var node in nodes)
                                {
                                    int oldID = Property.GetIndex(node);
                                    if (oldID > targetID)
                                    {
                                        Property.SetIndex(node, oldID - 1);
                                    }
                                }
                            }
                            // remove reference of control
                            nodes.RemoveAt(i);
                            break;
                        case EditMode.Control:
                            laneID = editor.Cache.Lane;
                            switch (editor.SelectedNodeType)
                            {
                                case NodeType.Position:
                                    editor.Chart.Lanes[laneID].Nodes.PositionControl.RemoveAt(id);
                                    canvas.Children.Remove(nodes[i]);
                                    if (editor.Chart.Lanes[laneID].Nodes.PositionControl.Count > 0)
                                    {
                                        // update cache
                                        if (i == 0)
                                        {
                                            newSelectedControl = nodes[i + 1];
                                        }
                                        else
                                        {
                                            newSelectedControl = nodes[i - 1];
                                        }
                                        newSelectedControl.IsChecked = true;
                                        // update index
                                        foreach (var node in nodes)
                                        {
                                            int oldID = Property.GetIndex(node);
                                            if (oldID > targetID)
                                            {
                                                Property.SetIndex(node, oldID - 1);
                                            }
                                        }
                                    }
                                    // remove reference of control
                                    nodes.RemoveAt(i);
                                    break;
                                case NodeType.Alpha:
                                    editor.Chart.Lanes[laneID].Nodes.AlphaControl.RemoveAt(id);
                                    canvas.Children.Remove(nodes[i]);
                                    if (editor.Chart.Lanes[laneID].Nodes.AlphaControl.Count > 0)
                                    {
                                        // update cache
                                        if (i == 0)
                                        {
                                            newSelectedControl = nodes[i + 1];
                                        }
                                        else
                                        {
                                            newSelectedControl = nodes[i - 1];
                                        }
                                        newSelectedControl.IsChecked = true;
                                        // update index
                                        foreach (var node in nodes)
                                        {
                                            int oldID = Property.GetIndex(node);
                                            if (oldID > targetID)
                                            {
                                                Property.SetIndex(node, oldID - 1);
                                            }
                                        }
                                    }
                                    // remove reference of control
                                    nodes.RemoveAt(i);
                                    break;
                                case NodeType.Speed:
                                    editor.Chart.Lanes[laneID].Nodes.SpeedControl.RemoveAt(id);
                                    canvas.Children.Remove(nodes[i]);
                                    if (editor.Chart.Lanes[laneID].Nodes.SpeedControl.Count > 0)
                                    {
                                        // update cache
                                        if (i == 0)
                                        {
                                            newSelectedControl = nodes[i + 1];
                                        }
                                        else
                                        {
                                            newSelectedControl = nodes[i - 1];
                                        }
                                        newSelectedControl.IsChecked = true;
                                        // update index
                                        foreach (var node in nodes)
                                        {
                                            int oldID = Property.GetIndex(node);
                                            if (oldID > targetID)
                                            {
                                                Property.SetIndex(node, oldID - 1);
                                            }
                                        }
                                    }
                                    // remove reference of control
                                    nodes.RemoveAt(i);
                                    break;
                                case NodeType.Endpoint:
                                    editor.Chart.Lanes[laneID].Nodes.EndpointControl.RemoveAt(id);
                                    canvas.Children.Remove(nodes[i]);
                                    if (editor.Chart.Lanes[laneID].Nodes.EndpointControl.Count > 0)
                                    {
                                        // update cache
                                        if (i == 0)
                                        {
                                            newSelectedControl = nodes[i + 1];
                                        }
                                        else
                                        {
                                            newSelectedControl = nodes[i - 1];
                                        }
                                        newSelectedControl.IsChecked = true;
                                        // update index
                                        foreach (var node in nodes)
                                        {
                                            int oldID = Property.GetIndex(node);
                                            if (oldID > targetID)
                                            {
                                                Property.SetIndex(node, oldID - 1);
                                            }
                                        }
                                    }
                                    // remove reference of control
                                    nodes.RemoveAt(i);
                                    break;
                            }
                            break;
                    }
                    UpdateCache(newSelectedControl);
                    if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position) GeneratePositionNodeOnPreview();
                    UpdateDisplay();
                    break;
                }
            }
            EnableDeletion();
        }
    }
    public void NodeUpdate(object? sender, SelectionChangedEventArgs args)
    {
        if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas && sender is ComboBox src)
        {
            switch (editor.EditMode)
            {
                // the easing changed
                case EditMode.Song:
                    // rerender
                    SetTimelineWidth();
                    UpdateDisplay();
                    break;
                // note type update
                case EditMode.Note:
                    if (editor.Chart.Lanes[editor.Cache.Lane].Notes.Count > 0)
                    {
                        var nodes = canvas.Children.OfType<RadioButton>().ToList();
                        // update cache
                        int id = editor.Cache.NoteID[editor.Cache.Lane];
                        editor.Cache.Note = editor.Chart.Lanes[editor.Cache.Lane].Notes[id];
                        // find selected note
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            int targetID = Property.GetIndex(nodes[i]);
                            if (targetID == id)
                            {
                                if (editor.Cache.Note is Note note)
                                {
                                    nodes[i].Background = new SolidColorBrush(Helper.GetColor(note.Type));
                                    // if track note
                                    if (note.Type == "track")
                                    {
                                        // was not track
                                        if (Property.GetTrackNoteTail(nodes[i]) is not Rectangle)
                                        {
                                            // generate one
                                            var trackTail = new Rectangle
                                            {
                                                Fill = new SolidColorBrush(Helper.GetColor("track")),
                                                Height = 15,
                                                Width = editor.BeatToViewDistance(note.Beat + note.Duration) - editor.BeatToViewDistance(note.Beat),
                                                StrokeJoin = PenLineJoin.Round,
                                                ZIndex = -1
                                            };
                                            Canvas.SetLeft(trackTail, editor.BeatToViewDistance(note.Beat) + 10);
                                            Canvas.SetTop(trackTail, 17);
                                            Property.SetTrackNodeTail(nodes[i], trackTail);
                                            canvas.Children.Add(trackTail);
                                        }
                                    }
                                    else
                                    {
                                        // was track
                                        if (Property.GetTrackNoteTail(nodes[i]) is Rectangle noteTail)
                                        {
                                            // remove it
                                            Property.SetTrackNodeTail(nodes[i], null);
                                            canvas.Children.Remove(noteTail);
                                        }
                                    }
                                    SetNoteDetailWindow();
                                    editor.Chart.Sample(editor.Cache.Lane);
                                }
                                break;
                            }
                        }
                        Render();
                    }
                    break;
                // easing update
                case EditMode.Control:
                    if (src.Name == "nodeType")
                    {
                        if (editor.SelectedNodeType == NodeType.Position)
                        {
                            GeneratePositionNodeOnPreview();
                        }
                        else
                        {
                            preview.RemoveGrid();
                            RemoveAll<Rectangle>(previewContainer);
                        }
                    }
                    else
                    {
                        mouseDown = false;
                    }
                    UpdateCache();
                    Initialize();
                    UpdateDisplay();
                    break;
            }
        }
    }
    private void SetNoteDetailWindow()
    {
        if (GetControl<TextBlock>("NoteDetail_Desc") is TextBlock desc &&
            GetControl<TextBlock>("NoteDetail_DurationText") is TextBlock durText &&
            GetControl<TextBox>("NoteDetail_Duration") is TextBox dur &&
            editor.Cache.Note is Note note)
        {
            desc.Text = Texts.ToTitle(note.Type) + " Note Detail";
            durText.IsVisible = note.Type == "track";
            dur.IsVisible = note.Type == "track";
        }
    }
    private void RemoveAll<T>(Canvas parent, string? className = null) where T : Control
    {
        var controls = parent.Children.OfType<T>();
        if (className is not null)
        {
            List<T> toRemove = [];
            foreach (var control in controls)
            {
                if (control.Classes.Contains(className))
                {
                    toRemove.Add(control);
                }
            }
            parent.Children.RemoveAll(toRemove);
        }
        else
        {
            parent.Children.RemoveAll(controls);
        }
    }

    private T? GetControl<T>(string name) where T : Control
    {
        if (GetTopLevel(this) is TopLevel topLevel)
        {
            var panels = topLevel.GetLogicalDescendants().OfType<T>();
            foreach (var panel in panels)
            {
                if (panel.Name == name)
                {
                    return panel;
                }
            }
        }
        return null;
    }

    public void UpdateSample(object? sender, TextChangedEventArgs args)
    {
        UpdateDisplay();
        if (GetControl<TextBox>("PosX") is TextBox posX && GetControl<TextBox>("PosY") is TextBox posY)
        {
            if (posX.IsFocused || posY.IsFocused)
            {
                GeneratePositionNodeOnPreview();
            }
        }
    }
    public void UpdateSample(object? sender, RangeBaseValueChangedEventArgs args)
    {
        if (sender is Slider src)
        {
            switch (src.Name)
            {
                case "Detail_Alpha":
                    UpdateDisplay();
                    break;
                case "SongPlayer_Timeline":
                    if (editor.IsLoaded)
                    {
                        if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
                        {
                            if (!isPlaying)
                            {
                                GeneratePositionNodeOnPreview();
                            }
                            else
                            {
                                preview.RemoveGrid();
                                RemoveAll<Rectangle>(previewContainer);
                            }
                        }
                        Render();
                        timeDisplay_Beat.Text = string.Format("{0:0.00}", editor.Chart.Beat(editor.Settings.Timer));
                        timeDisplay_Time.Text = Texts.GetTimeString(editor.Settings.Timer);
                        if ((SongPlayer_Timeline.IsFocused || (!isPlaying && GetControl<ScrollViewer>("timeline") is ScrollViewer scrollViewer && !scrollViewer.IsPointerOver)) && audioHandler is not null)
                        {
                            SetScrollerValue("timeline", editor.BeatToViewDistance(editor.Chart.Beat(editor.Settings.Timer)) - 200);
                            audioHandler.StartTime = (long)(editor.Settings.Timer * 1000);
                        }
                    }
                    break;
            }
        }
        else
        {
            throw new NotSupportedException("Function caller must be a slider");
        }
    }

    private void UpdateDisplay()
    {
        int laneID = editor.Cache.Lane;
        switch (editor.EditMode)
        {
            case EditMode.Song:
                // resample all curves
                editor.Chart.SampleAll();
                break;
            case EditMode.Note:
                // resample curve
                editor.Chart.Sample(laneID);
                break;
            case EditMode.Control:
                switch (editor.SelectedNodeType)
                {
                    case NodeType.Position:
                        // resample curve
                        editor.Chart.Sample(laneID);
                        break;
                    case NodeType.Alpha:
                        // do nothing
                        break;
                    case NodeType.Speed:
                        // resample curve
                        editor.Chart.Sample(laneID);
                        break;
                    case NodeType.Endpoint:
                        // do nothing
                        break;
                }
                break;
        }
        Render();
    }

    private void Render()
    {
        float currentBeat = editor.Chart.Beat(editor.Settings.Timer);
        // Clear Display
        preview.RemoveLane();
        RemoveAll<TextBlock>(previewContainer, "LaneLabel");
        // Generate Lines
        foreach (var lane in editor.Chart.Lanes)
        {
            float currentDistance = editor.Chart.Distance(lane.ID, currentBeat);
            var samples = lane.GetAllSamples();
            // generate line segments
            bool isVisible = false;
            int firstSampleID = int.MaxValue;
            int gap = 1;
            float alpha = editor.Chart.Alpha(lane.ID, currentBeat);
            for (int i = 0; i + gap + 1 < samples.Count; i += gap)
            {
                if (samples[i].Z >= currentDistance + 8)
                {
                    // finish render if outside of view distance
                    break;
                }
                else if (samples[i].Z >= currentDistance && editor.Chart.EndpointAttrFromDistance(lane.ID, samples[i].Z) && alpha != 0)
                {
                    var dz = samples[i].Z - currentDistance;
                    gap = (int)Math.Round(dz / 8 * 10) + 1;
                    if (i + gap < samples.Count)
                    {
                        // in range
                        LinePoint segment = new(
                            Compute.ToDisplayCoordinates(samples[i], currentDistance),
                            Compute.ToDisplayCoordinates(samples[i + gap], currentDistance),
                            Helper.GetTransparentColor(Colors.LightGray, (8 - dz) / 8 * alpha),
                            5 / (1 + dz)
                        );
                        preview.lines.Add(segment);
                        isVisible = true;
                        if (firstSampleID > i)
                        {
                            firstSampleID = i;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                // ignore if passed
            }
            if (isVisible)
            {
                // add lane ID
                var label = new TextBlock
                {
                    Text = (lane.ID + 1).ToString()
                };
                label.Classes.Add("LaneLabel");
                var coords = Compute.ToDisplayCoordinates(samples[firstSampleID], samples[firstSampleID].Z);
                Canvas.SetLeft(label, coords.X);
                Canvas.SetTop(label, coords.Y + 10);
                previewContainer.Children.Add(label);
            }
            // Generate Note
            var notes = lane.Notes.OrderBy(x => x.Beat).ToList();
            for (int i = 0; i < notes.Count; i++)
            {
                var note = notes[i];
                if (note.Distance >= currentDistance + 8)
                {
                    // finish render if outside of view distance
                    break;
                }
                else if (note.Distance >= currentDistance)
                {
                    // in range
                    Circle n = new(
                        Compute.ToDisplayCoordinates(note.Get3DPosition(), currentDistance),
                        25 / (1 + note.Distance - currentDistance),
                        Helper.GetColor(note.Type),
                        Colors.White,
                        5 / (1 + note.Distance - currentDistance)
                    );
                    preview.circles.Add(n);
                }
                // ignore if passed

                // if track note
                if (note.Type == "track")
                {
                    foreach (var sample in note.Samples)
                    {
                        var samplePos = note.Get3DPosition() + sample;
                        if (samplePos.Z >= currentDistance + 8)
                        {
                            break;
                        }
                        else if (samplePos.Z >= currentDistance)
                        {
                            Circle n = new(
                                Compute.ToDisplayCoordinates(samplePos, currentDistance),
                                14 / (1 + samplePos.Z - currentDistance),
                                Color.FromArgb(0, 0, 0, 0),
                                Helper.GetColor(note.Type),
                                5 / (1 + samplePos.Z - currentDistance)
                            );
                            preview.circles.Add(n);
                        }
                    }
                }
            }
        }
    }
    public void AddLane(object? sender, RoutedEventArgs args)
    {
        editor.NewLane();
        if (GetControl<NumericUpDown>("LaneSwitch") is NumericUpDown laneSwitch)
        {
            laneSwitch.Maximum = editor.Chart.Lanes.Count;
            laneSwitch.Value = laneSwitch.Maximum;
        }
        else
        {
            throw new NullReferenceException("Cannot find lane switch.");
        }
        Initialize();
        Render();
        if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
        {
            GeneratePositionNodeOnPreview();
        }
    }
    public void DeleteLane(object? sender, RoutedEventArgs args)
    {
        editor.DeleteLane();
        if (GetControl<NumericUpDown>("LaneSwitch") is NumericUpDown laneSwitch)
        {
            laneSwitch.Maximum = editor.Chart.Lanes.Count;
            laneSwitch.Value = editor.Cache.Lane;
        }
        else
        {
            throw new NullReferenceException("Cannot find lane switch.");
        }
        Initialize();
        Render();
        if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
        {
            GeneratePositionNodeOnPreview();
        }
    }
    public async void PlaySong(object? sender, RoutedEventArgs args)
    {
        if (sender is Button src)
        {
            if (audioHandler is not null)
            {
                if (!isPlaying)
                {
                    isPlaying = true;
                    await Task.Run(SliderControl);
                }
                else
                {
                    isPlaying = false;
                    audioHandler.PauseSong();

                    if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
                    {
                        GeneratePositionNodeOnPreview();
                    }
                }
                UpdatePlayButtonIcon(src);
            }
        }
        else
        {
            throw new ArgumentException("Function caller must be a button.");
        }
    }
    public void StopSong(object? sender, RoutedEventArgs args)
    {
        if (audioHandler is not null && GetControl<ScrollViewer>("timeline") is ScrollViewer scroller)
        {
            isPlaying = false;
            audioHandler.StopSong();
            UpdatePlayButtonIcon(PlayButton);
            SongPlayer_Timeline.Value = 0;
            scroller.Offset = scroller.Offset.WithX(0);
            timeDisplay_Beat.Text = "0.00";
            timeDisplay_Time.Text = "0:00.00";
            Render();
            if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position) GeneratePositionNodeOnPreview();
        }
    }
    private void UpdatePlayButtonIcon(Button target)
    {
        if (target.Content is Image icon)
        {
            if (isPlaying)
            {
                icon.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Charting/Assets/pause.png")));
            }
            else
            {
                icon.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Charting/Assets/play.png")));
            }
        }
        else
        {
            throw new NullReferenceException("Cannot find image inside button.");
        }
    }
    private async void SliderControl()
    {
        var editor = Dispatcher.UIThread.Invoke(GetDataContext);
        bool songPlayed = false;
        if (audioHandler is not null)
        {
            while (isPlaying)
            {
                // ~ 30 fps
                await Task.Delay(30);
                if (isPlaying)
                {
                    double value = Dispatcher.UIThread.Invoke(GetSliderValue);
                    // Update slider
                    if (value < -editor.Chart.Offset / 1000) {
                        value += 0.03;
                    }
                    else {
                        if (!songPlayed) {
                            audioHandler.PlaySong(editor.Settings.Timer * 1000 + editor.Chart.Offset);
                            songPlayed = true;
                        }
                        value = ((double)audioHandler.CurrentTime - Math.Min(0, editor.Chart.Offset)) / 1000;
                        if (audioHandler.State != NAudio.Wave.PlaybackState.Playing) {
                            audioHandler.PlaySong(editor.Settings.Timer * 1000 + editor.Chart.Offset);
                        }
                    }
                    audioHandler.SetVolume(editor.Settings.Volume);
                    Dispatcher.UIThread.Post(() => SetSliderValue(value));
                    Dispatcher.UIThread.Post(() => SetScrollerValue("timeline", editor.BeatToViewDistance(editor.Chart.Beat((float)value)) - 200));
                }
            }
        }
        else
        {
            throw new NullReferenceException("Cannot find song player.");
        }
    }
    private void SetScrollerValue(string target, double val)
    {
        if (GetControl<ScrollViewer>(target) is ScrollViewer scroll)
        {
            scroll.Offset = scroll.Offset.WithX(val);
        }
        else
        {
            throw new NullReferenceException("Cannot find scroller.");
        }
    }

    private void SetSliderValue(double val)
    {
        SongPlayer_Timeline.Value = val;
    }
    private double GetSliderValue()
    {
        return SongPlayer_Timeline.Value;
    }
    private MainWindowViewModel GetDataContext()
    {
        return editor;
    }
    private Border GetNotifBox()
    {
        return Notification;
    }
    private void SetNotifBox(string property, object value)
    {
        if (Notification.Child is TextBlock content)
        {
            switch (property)
            {
                case "Opacity":
                    if (value is int)
                    {
                        Notification.Opacity = (int)value;
                        break;
                    }
                    else if (value is float)
                    {
                        Notification.Opacity = (float)value;
                    }
                    else if (value is double)
                    {
                        Notification.Opacity = (double)value;
                    }
                    else
                    {
                        throw new ArgumentException("\"value\" is not float.");
                    }
                    break;
                case "Text":
                    if (value is not null)
                    {
                        content.Text = value.ToString();
                    }
                    break;
                case "Color":
                    if (value is Color color)
                    {
                        content.Foreground = new SolidColorBrush(color);
                    }
                    break;
            }
        }
    }
    public void UpdateOffset(object? sender, TextChangedEventArgs args)
    {
        if (audioHandler is not null)
        {
            SongPlayer_Timeline.Maximum = (audioHandler.Length - editor.Chart.Offset) / 1000;
            songLength = SongPlayer_Timeline.Maximum;
            SetTimelineWidth();
        }
        else
        {
            throw new NullReferenceException("Cannot find song player.");
        }
    }
    private void UpdateCache(RadioButton? control = null)
    {
        int id;
        if (control is not null)
        {
            id = Property.GetIndex(control);
        }
        else
        {
            id = -1;
        }
        switch (editor.EditMode)
        {
            case EditMode.Song:
                if (editor.Chart.BPMControl.Count > 0)
                {
                    if (id >= 0) editor.Cache.BPMID = id;
                    editor.Cache.BPMControl = editor.Chart.BPMControl[editor.Cache.BPMID];
                }
                else
                {
                    editor.Cache.BPMID = -1;
                    editor.Cache.BPMControl = null;
                }
                break;
            case EditMode.Note:
                int laneID = editor.Cache.Lane;
                if (editor.Chart.Lanes[laneID].Notes.Count > 0)
                {
                    if (id >= 0) editor.Cache.NoteID[laneID] = id;
                    editor.Cache.Note = editor.Chart.Lanes[laneID].Notes[editor.Cache.NoteID[laneID]];
                    storedDuration = editor.Cache.Note.Duration;
                }
                else
                {
                    editor.Cache.NoteID[laneID] = -1;
                    editor.Cache.Note = null;
                    storedDuration = 0;
                }
                break;
            case EditMode.Control:
                laneID = editor.Cache.Lane;
                switch (editor.SelectedNodeType)
                {
                    case NodeType.Position:
                        if (editor.Chart.Lanes[laneID].Nodes.PositionControl.Count > 0)
                        {
                            if (id >= 0) editor.Cache.PositionID[laneID] = id;
                            editor.Cache.PositionControl = editor.Chart.Lanes[laneID].Nodes.PositionControl[editor.Cache.PositionID[laneID]];
                        }
                        else
                        {
                            editor.Cache.PositionID[laneID] = -1;
                            editor.Cache.PositionControl = null;
                        }
                        break;
                    case NodeType.Alpha:
                        if (editor.Chart.Lanes[laneID].Nodes.AlphaControl.Count > 0)
                        {
                            if (id >= 0) editor.Cache.AlphaID[laneID] = id;
                            editor.Cache.AlphaControl = editor.Chart.Lanes[laneID].Nodes.AlphaControl[editor.Cache.AlphaID[laneID]];
                        }
                        else
                        {
                            editor.Cache.AlphaID[laneID] = -1;
                            editor.Cache.AlphaControl = null;
                        }
                        break;
                    case NodeType.Speed:
                        if (editor.Chart.Lanes[laneID].Nodes.SpeedControl.Count > 0)
                        {
                            if (id >= 0) editor.Cache.SpeedID[laneID] = id;
                            editor.Cache.SpeedControl = editor.Chart.Lanes[laneID].Nodes.SpeedControl[editor.Cache.SpeedID[laneID]];
                        }
                        else
                        {
                            editor.Cache.SpeedID[laneID] = -1;
                            editor.Cache.SpeedControl = null;
                        }
                        break;
                    case NodeType.Endpoint:
                        if (editor.Chart.Lanes[laneID].Nodes.EndpointControl.Count > 0)
                        {
                            if (id >= 0) editor.Cache.EndpointID[laneID] = id;
                            editor.Cache.EndpointControl = editor.Chart.Lanes[laneID].Nodes.EndpointControl[editor.Cache.EndpointID[laneID]];
                        }
                        else
                        {
                            editor.Cache.EndpointID[laneID] = -1;
                            editor.Cache.EndpointControl = null;
                        }
                        break;
                }
                break;
        }
    }
    public RadioButton GetControlButton()
    {
        if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canva)
        {
            int laneID = editor.Cache.Lane;
            int selectedID = 0;
            var controls = canva.Children.OfType<RadioButton>().ToList();
            switch (editor.EditMode)
            {
                case EditMode.Song:
                    selectedID = editor.Cache.BPMID;
                    break;
                case EditMode.Note:
                    selectedID = editor.Cache.NoteID[laneID];
                    break;
                case EditMode.Control:
                    switch (editor.SelectedNodeType)
                    {
                        case NodeType.Position:
                            selectedID = editor.Cache.PositionID[laneID];
                            break;
                        case NodeType.Alpha:
                            selectedID = editor.Cache.AlphaID[laneID];
                            break;
                        case NodeType.Speed:
                            selectedID = editor.Cache.SpeedID[laneID];
                            break;
                        case NodeType.Endpoint:
                            selectedID = editor.Cache.EndpointID[laneID];
                            break;
                    }
                    break;
            }
            foreach (var control in controls)
            {
                if (Property.GetIndex(control) == selectedID)
                {
                    return control;
                }
            }
            throw new NullReferenceException("Cannot find control node.");
        }
        else
        {
            throw new NullReferenceException("Cannot find canvas.");
        }
    }
    public float? GetSelectedBeat()
    {
        switch (editor.EditMode)
        {
            case EditMode.Song:
                if (editor.Cache.BPMControl is BPMControl bpm)
                {
                    return bpm.Beat;
                }
                break;
            case EditMode.Note:
                if (editor.Cache.Note is Note note)
                {
                    return note.Beat;
                }
                break;
            case EditMode.Control:
                switch (editor.SelectedNodeType)
                {
                    case NodeType.Position:
                        if (editor.Cache.PositionControl is PositionControl pos)
                        {
                            return pos.Beat;
                        }
                        break;
                    case NodeType.Alpha:
                        if (editor.Cache.AlphaControl is AlphaControl alpha)
                        {
                            return alpha.Beat;
                        }
                        break;
                    case NodeType.Speed:
                        if (editor.Cache.SpeedControl is SpeedControl speed)
                        {
                            return speed.Beat;
                        }
                        break;
                    case NodeType.Endpoint:
                        if (editor.Cache.EndpointControl is EndpointControl endpoint)
                        {
                            return endpoint.Beat;
                        }
                        break;
                }
                break;
        }
        return null;
    }
    public object GetSelectedNode()
    {
        switch (editor.EditMode)
        {
            case EditMode.Song:
                if (editor.Cache.BPMControl is BPMControl bpm)
                {
                    return bpm;
                }
                break;
            case EditMode.Note:
                if (editor.Cache.Note is Note note)
                {
                    return note;
                }
                break;
            case EditMode.Control:
                switch (editor.SelectedNodeType)
                {
                    case NodeType.Position:
                        if (editor.Cache.PositionControl is PositionControl pos)
                        {
                            return pos;
                        }
                        break;
                    case NodeType.Alpha:
                        if (editor.Cache.AlphaControl is AlphaControl alpha)
                        {
                            return alpha;
                        }
                        break;
                    case NodeType.Speed:
                        if (editor.Cache.SpeedControl is SpeedControl speed)
                        {
                            return speed;
                        }
                        break;
                    case NodeType.Endpoint:
                        if (editor.Cache.EndpointControl is EndpointControl endpoint)
                        {
                            return endpoint;
                        }
                        break;
                }
                break;
        }
        throw new NullReferenceException("Cannot find node reference.");
    }
    private void EnableDeletion()
    {
        int laneID = editor.Cache.Lane;
        switch (editor.EditMode)
        {
            case EditMode.Song:
                editor.CanDelete = editor.Chart.BPMControl.Count > 1;
                break;
            case EditMode.Note:
                editor.CanDelete = editor.Chart.BPMControl.Count > 0;
                break;
            case EditMode.Control:
                switch (editor.SelectedNodeType)
                {
                    case NodeType.Position:
                        editor.CanDelete = editor.Chart.Lanes[laneID].Nodes.PositionControl.Count > 1;
                        break;
                    case NodeType.Alpha:
                        editor.CanDelete = editor.Chart.Lanes[laneID].Nodes.AlphaControl.Count > 1;
                        break;
                    case NodeType.Speed:
                        editor.CanDelete = editor.Chart.Lanes[laneID].Nodes.SpeedControl.Count > 1;
                        break;
                    case NodeType.Endpoint:
                        editor.CanDelete = editor.Chart.BPMControl.Count > 0;
                        break;
                }
                break;
        }
    }
    private void GeneratePositionNodeOnPreview()
    {
        RemoveAll<Rectangle>(previewContainer);
        preview.RemoveGrid();
        if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
        {
            float currentBeat = editor.Chart.Beat(editor.Settings.Timer);
            foreach (var lane in editor.Chart.Lanes)
            {
                float currentDistance = editor.Chart.Distance(lane.ID, currentBeat);
                var positionControl = lane.Nodes.PositionControl;
                for (int i = 0; i < positionControl.Count; i++)
                {
                    var pos = positionControl[i];
                    if (pos.Distance < currentDistance + 5 && pos.Distance >= currentDistance)
                    {
                        // in range
                        Rectangle control = new()
                        {
                            ZIndex = -(int)(pos.Distance * 10),
                            Width = 40 / (1 + pos.Distance - currentDistance),
                            Height = 40 / (1 + pos.Distance - currentDistance),
                            Fill = new SolidColorBrush(Colors.DarkMagenta),
                            Stroke = new SolidColorBrush(Colors.White),
                            StrokeThickness = 0
                        };
                        var coords = Compute.ToDisplayCoordinates(pos.Get3DCoords() + new Vector3(-20f, 20f, 0) / 22.5f / 1.5f, currentDistance);
                        Canvas.SetTop(control, coords.Y);
                        Canvas.SetLeft(control, coords.X);
                        Property.SetIndex(control, i);
                        Property.SetLane(control, lane.ID);
                        // attach properties
                        control.AddHandler(PointerPressedEvent, PositionNodeOnMouseDown, RoutingStrategies.Tunnel);
                        control.AddHandler(PointerMovedEvent, PositionNodeOnMouseMove, RoutingStrategies.Tunnel);
                        control.AddHandler(PointerReleasedEvent, EventMouseUp, RoutingStrategies.Tunnel);
                        previewContainer.Children.Add(control);

                        // if is selected
                        if (lane.ID == editor.Cache.Lane && i == editor.Cache.PositionID[lane.ID])
                        {
                            control.StrokeThickness = 5 / (1 + pos.Distance - currentDistance);
                            UpdateCache();
                            GenerateGridline(currentDistance, pos.Distance);
                        }
                    }
                }
            }
        }
    }
    private void PositionNodeOnMouseDown(object? sender, PointerPressedEventArgs args)
    {
        if (sender is Rectangle src)
        {
            if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position)
            {
                int lane = Property.GetLane(src);
                int id = Property.GetIndex(src);
                var nodeDistance = editor.Chart.Lanes[lane].Nodes.PositionControl[id].Distance;
                var currentDistance = editor.Chart.Distance(lane, editor.Chart.Beat(editor.Settings.Timer));
                // set control node border
                foreach (var node in previewContainer.Children.OfType<Rectangle>())
                {
                    if (node.StrokeThickness > 0) node.StrokeThickness = 0;
                    if (node == src) node.StrokeThickness = 5 / (1 + nodeDistance - currentDistance);
                }
                GenerateGridline(currentDistance, nodeDistance);
                // set lane to selected
                editor.Cache.Lane = lane;
                editor.Cache.PositionID[lane] = id;
                editor.Cache.PositionControl = editor.Chart.Lanes[lane].Nodes.PositionControl[id];
                // set selected node
                if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canva)
                {
                    var nodes = canva.Children.OfType<RadioButton>().ToList();
                    foreach (var node in nodes)
                    {
                        if (Property.GetIndex(node) == id)
                        {
                            node.IsChecked = true;
                            break;
                        }
                    }
                }
                if (GetControl<NumericUpDown>("LaneSwitch") is NumericUpDown laneSwitch)
                {
                    laneSwitch.Value = editor.Cache.Lane + 1;
                }
                else
                {
                    throw new NullReferenceException("Cannot find lane switch.");
                }
                lastMousePosition = args.GetCurrentPoint(preview).Position;
                rawPosition = new(Canvas.GetLeft(src), Canvas.GetTop(src));
                mouseDown = true;
            }
        }
        else
        {
            throw new NotSupportedException("Function caller must be a rectangle.");
        }
    }
    private void GenerateGridline(float currentDistance, float targetDistance)
    {
        preview.RemoveGrid();
        for (int j = -8; j <= 8; j++)
        {
            LinePoint gridLine = new(
                Compute.ToDisplayCoordinates(new Vector3(-15, j, targetDistance), currentDistance),
                Compute.ToDisplayCoordinates(new Vector3(15, j, targetDistance), currentDistance),
                Helper.GetTransparentColor(Colors.LightYellow, 0.3f),
                2 / (1 + targetDistance - currentDistance)
            );
            if (j == 0)
            {
                gridLine.Property.Thickness *= 1.5;
                gridLine.Property.Brush = new SolidColorBrush(Colors.LightGreen)
                {
                    Opacity = 0.5
                };
            }
            preview.grid.Add(gridLine);
        }
        for (int k = -15; k <= 15; k++)
        {
            LinePoint gridLine = new(
                Compute.ToDisplayCoordinates(new Vector3(k, -8, targetDistance), currentDistance),
                Compute.ToDisplayCoordinates(new Vector3(k, 8, targetDistance), currentDistance),
                Helper.GetTransparentColor(Colors.LightYellow, 0.3f),
                2 / (1 + targetDistance - currentDistance)
            );
            if (k == 0)
            {
                gridLine.Property.Thickness *= 1.5;
                gridLine.Property.Brush = new SolidColorBrush(Colors.LightGreen)
                {
                    Opacity = 0.5
                };
            }
            preview.grid.Add(gridLine);
        }
    }
    private void PositionNodeOnMouseMove(object? sender, PointerEventArgs args)
    {
        if (sender is Rectangle src)
        {
            if (editor.EditMode == EditMode.Control && editor.SelectedNodeType == NodeType.Position && mouseDown)
            {
                Settings settings = editor.Settings;
                var point = args.GetCurrentPoint(preview).Position;

                // get node distance and current distance
                int lane = Property.GetLane(src);
                int id = Property.GetIndex(src);
                var nodeDistance = editor.Chart.Lanes[lane].Nodes.PositionControl[id].Distance;
                var currentDistance = editor.Chart.Distance(lane, editor.Chart.Beat(editor.Settings.Timer));

                rawPosition = rawPosition + point - lastMousePosition;
                var expectedChartPosition = Compute.ToChartCoordinates(new((float)rawPosition.X, (float)rawPosition.Y, nodeDistance), currentDistance);
                expectedChartPosition += new Source.Vector2(0.5f, -0.5f);

                // check out of bounds
                // x
                if (expectedChartPosition.X < -20) expectedChartPosition.X = -20;
                if (expectedChartPosition.X > 20) expectedChartPosition.X = 20;
                // y
                if (expectedChartPosition.Y < -12) expectedChartPosition.Y = -12;
                if (expectedChartPosition.Y > 12) expectedChartPosition.Y = 12;

                if (settings.IsGridSnap)
                {
                    expectedChartPosition.X = (int)Math.Round(expectedChartPosition.X);
                    expectedChartPosition.Y = (int)Math.Round(expectedChartPosition.Y);
                }

                // move node
                var expectedPosition = Compute.ToDisplayCoordinates(new((expectedChartPosition + new Source.Vector2(-0.5f, 0.5f)).ToWindowsVector(), nodeDistance), currentDistance);
                Canvas.SetLeft(src, expectedPosition.X);
                Canvas.SetTop(src, expectedPosition.Y);

                if (editor.Chart.Lanes[lane].Nodes.PositionControl[id].Position != expectedChartPosition)
                {
                    editor.Chart.Lanes[lane].Nodes.PositionControl[id].Position = expectedChartPosition;
                }
                // update cache
                UpdateCache();
                Render();

                lastMousePosition = point;
            }
        }
        else
        {
            throw new NotSupportedException("Function caller must be a rectangle.");
        }
    }
    private void StopPlaying(object? sender, PointerPressedEventArgs args)
    {
        if (sender is Slider)
        {
            if (audioHandler is not null)
            {
                isPlaying = false;
                audioHandler.StopSong();
                UpdatePlayButtonIcon(PlayButton);
            }
        }
        else
        {
            throw new NotSupportedException("Function caller must be a slider.");
        }
    }
    private async void AutoSaveHandler()
    {
        await Task.Run(DoAutoSave);
    }
    private async void DoAutoSave()
    {
        while (true)
        {
            await Task.Delay(5 * 60 * 1000);
            if (Dispatcher.UIThread.Invoke(GetDataContext).IsLoaded)
            {
                Dispatcher.UIThread.Post(() => Save(directory + "\\Autosave\\" + folderName));
                Dispatcher.UIThread.Post(() => Notif("Chart Autosaved!"));
            }
        }
    }
    private async void Notif(string message, NotifType type = NotifType.SUCCESS)
    {
        Dispatcher.UIThread.Post(() => SetNotifBox("Opacity", 1));
        Dispatcher.UIThread.Post(() => SetNotifBox("Text", message));
        switch (type)
        {
            case NotifType.SUCCESS:
                Dispatcher.UIThread.Post(() => SetNotifBox("Color", Colors.White));
                break;
            case NotifType.WARNING:
                Dispatcher.UIThread.Post(() => SetNotifBox("Color", Colors.Yellow));
                break;
            case NotifType.ERROR:
                Dispatcher.UIThread.Post(() => SetNotifBox("Color", Colors.Red));
                break;
        }
        int taskID = notifTask;
        notifTask++;
        await Task.Delay(1000);
        while (true)
        {
            await Task.Delay(40);
            var notif = Dispatcher.UIThread.Invoke(GetNotifBox);
            if (notif.Opacity > 0 && (notif.Opacity != 1 || taskID + 1 == notifTask))
            {
                Dispatcher.UIThread.Post(() => SetNotifBox("Opacity", notif.Opacity - 0.05));
            }
            else
            {
                break;
            }
        }
    }
    public void SetProperties(object? sender, VisualTreeAttachmentEventArgs args)
    {
        SetNoteDetailWindow();
    }
    private void SetTimelineWidth()
    {
        if (GetControl<Canvas>("timelineDisplayContainer") is Canvas canvas)
        {
            canvas.Width = editor.SecondToViewDistance(songLength) + 1000;
            SetNodePositions();
            GenerateLines();
        }
    }
    private void HotKeyEventManager(object? sender, KeyEventArgs args) {
        if (!pressedKey.Contains(args.Key)) {
            pressedKey.Add(args.Key);
            switch(args.Key) {
                case Key.Space:
                    // check if the focused control is not a text box
                    if (FocusManager is not null && FocusManager.GetFocusedElement() is not TextBox) {
                        Focus();
                        PlaySong(PlayButton, new RoutedEventArgs());
                    }
                    break;
                case Key.Delete:
                    if (editor.CanDelete) {
                        DeleteNode(null, new RoutedEventArgs());
                    }
                    break;
            }
        }
    }
    private void RemoveKey(object? sender, KeyEventArgs args) {
        if (pressedKey.Contains(args.Key)) {
            pressedKey.Remove(args.Key);
        }
    }
    public void ScrollHorizontally(object? sender, PointerWheelEventArgs args) {
        args.Handled = true;
        if (sender is ScrollViewer scrollViewer && audioHandler is not null && !isPlaying) {
            var setTime = editor.Settings.Timer + args.Delta.Y * -0.15;
            scrollViewer.Offset = new Avalonia.Vector(editor.SecondToViewDistance(setTime) - 200, 0);
            SetSliderValue(setTime);
        }
    }
    public async void Test(object? sender, RoutedEventArgs args) {
        if (audioHandler is not null) {
            var melodyData = await Task.Run(audioHandler.GetMainMelodyOnset);

            editor.Chart.BPMControl[0].BPM = melodyData.BPM;
            editor.Chart.Offset = (float)Math.Round(melodyData.Offset * 1000);
            editor.MelodyOnsets = melodyData.MelodyOnsets;
            await Dispatcher.UIThread.InvokeAsync(GenerateLines);
        }
    }
}

// TODO
// 1. bpm detect
// 3. Better converter
// 4. keybind
//      COPY + PASTE
//      LEFT ARROW: backward
//      RIGHT ARROW: forward
// 5. screen resolution display bug

// OPTIONAL
// 1. undo / redo
// 2. multilane display
// 3. auto update (?
// 4. editor icon