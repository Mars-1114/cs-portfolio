using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Collections;
using Avalonia.LogicalTree;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ReactiveUI;
using System.Linq;

namespace Charting.Source;

// FOR EDITOR USE

/// <summary>
/// A structure used to cache a specific control node
/// </summary>
public class CurrentNode : ReactiveObject {
    // Referenced
    private BPMControl? bpmControl = null;
    private PositionControl? positionControl = null;
    private AlphaControl? alphaControl = null;
    private SpeedControl? speedControl = null;
    private EndpointControl? endpointControl = null;
    private Note? note;

    // index
    private int lane = 0;
    private int bpmID = 0;
    private List<int> positionID = [];
    private List<int> alphaID = [];
    private List<int> speedID = [];
    private List<int> endpointID = [];
    private List<int> noteID = [];

    public BPMControl? BPMControl {
        get => bpmControl;
        set => this.RaiseAndSetIfChanged(ref bpmControl, value);
    }
    public PositionControl? PositionControl {
        get => positionControl;
        set => this.RaiseAndSetIfChanged(ref positionControl, value);
    }
    public AlphaControl? AlphaControl {
        get => alphaControl;
        set => this.RaiseAndSetIfChanged(ref alphaControl, value);
    }
    public SpeedControl? SpeedControl {
        get => speedControl;
        set => this.RaiseAndSetIfChanged(ref speedControl, value);
    }
    public Note? Note {
        get => note;
        set => this.RaiseAndSetIfChanged(ref note, value);
    }
    public EndpointControl? EndpointControl {
        get => endpointControl;
        set => this.RaiseAndSetIfChanged(ref endpointControl, value);
    }
    public int Lane {
        get => lane;
        set => lane = value;
    }
    public int BPMID {
        get => bpmID;
        set => bpmID = value;
    }
    public List<int> PositionID {
        get => positionID;
        set => positionID = value;
    }
    public List<int> AlphaID {
        get => alphaID;
        set => alphaID = value;
    }
    public List<int> SpeedID {
        get => speedID;
        set => speedID = value;
    }
    public List<int> EndpointID {
        get => endpointID;
        set => endpointID = value;
    }
    public List<int> NoteID {
        get => noteID;
        set => noteID = value;
    }
}

public class Settings : ReactiveObject {
    private int subdivision = 4;
    private float windowScale = 1;
    private bool isBeatSnap = true;
    private bool isGridSnap = true;
    private float timer = 0;
    private float volume = 0.5f;
    
    /// <summary>
    /// The subdivision of a beat. (Default = 4)
    /// </summary>
    public int Subdivision {
        get => subdivision;
        set => this.RaiseAndSetIfChanged(ref subdivision, value);
    }
    /// <summary>
    /// <para> The scale of the timeline window. (Default = 1) </para>
    /// <para> Used to determine the distance between each second. </para>
    /// <para> Defined as [value]*400 </para>
    /// </summary>
    public float WindowScale {
        get => windowScale;
        set => this.RaiseAndSetIfChanged(ref windowScale, value);
    }
    /// <summary>
    /// Snap the nodes and notes to the beat or subdivisions. (Default = true)
    /// </summary>
    public bool IsBeatSnap {
        get => isBeatSnap;
        set => this.RaiseAndSetIfChanged(ref isBeatSnap, value);
    }
    /// <summary>
    /// Snap the notes to the lattice point. (Default = true)
    /// </summary>
    public bool IsGridSnap {
        get => isGridSnap;
        set => this.RaiseAndSetIfChanged(ref isGridSnap, value);
    }
    public float Timer {
        get => timer;
        set => this.RaiseAndSetIfChanged(ref timer, value);
    }
    public float Volume {
        get => volume;
        set => this.RaiseAndSetIfChanged(ref volume, value);
    }
}

public enum EditMode {
    Unload,
    Song,
    Note,
    Control
}

public enum NodeType {
    Position,
    Alpha,
    Speed,
    Endpoint
}

class Renderer : Control {
    public List<LinePoint> lines = [];
    public List<LinePoint> grid = [];
    public List<Circle> circles = [];
    public List<RectProperty> rectangles = [];
    public sealed override void Render(DrawingContext context)
    {
        foreach (var line in lines.OrderBy(x => x.Property.Thickness)) {
            context.DrawLine(line.Property.GetPen(), line.StartPos.ToPoint(), line.EndPos.ToPoint());
        }
        foreach (var line in grid) {
            context.DrawLine(line.Property.GetPen(), line.StartPos.ToPoint(), line.EndPos.ToPoint());
        }
        foreach (var circle in circles.OrderBy(x => x.Border.Thickness)) {
            context.DrawEllipse(new SolidColorBrush(circle.Fill), circle.Border.GetPen(), circle.GetBound());
        }
        foreach (var rectangle in rectangles) {
            context.DrawRectangle(new SolidColorBrush(rectangle.Fill), rectangle.Border.GetPen(), rectangle.Rectangle);
        }
        base.Render(context);
    }
    public void RemoveAll() {
        RemoveLane();
        RemoveGrid();
        RemoveOnsets();
    }
    public void RemoveLane() {
        lines = [];
        circles = [];
        InvalidateVisual();
    }
    public void RemoveGrid() {
        grid = [];
        InvalidateVisual();
    }
    public void RemoveOnsets() {
        rectangles = [];
        InvalidateVisual();
    }
}

class LinePoint {
    public Vector2 StartPos;
    public Vector2 EndPos;
    public LineProperty Property;
    public LinePoint(Vector2 start, Vector2 end, Color color, double thickness) {
        StartPos = start;
        EndPos = end;
        Property = new(new SolidColorBrush(color), thickness);
    }
    public LinePoint(float startX, float startY, float endX, float endY, Color color, double thickness) {
        StartPos = new Vector2(startX, startY);
        EndPos = new Vector2(endX, endY);
        Property = new(new SolidColorBrush(color), thickness);
    }
}
class Circle {
    public Vector2 Center;
    public double Radius;
    public Color Fill;
    public LineProperty Border;
    public Circle(Vector2 center, double radius, Color fill, Color border, double borderThickness) {
        Center = center;
        Radius = radius;
        Fill = fill;
        Border = new(new SolidColorBrush(border), borderThickness);
    }
    public Circle(float x, float y, double r, Color fill, Color border, double borderThickness) {
        Center = new Vector2(x, y);
        Radius = r;
        Fill = fill;
        Border = new(new SolidColorBrush(border), borderThickness);
    }
    public Rect GetBound() {
        return new Rect(Center.X - Radius, Center.Y - Radius, 2 * Radius, 2 * Radius);
    }
}
class RectProperty {
    public Rect Rectangle;
    public Color Fill;
    public LineProperty Border;

    public RectProperty(Rect rectangle, Color fill, Color border, double borderThickness) {
        Rectangle = rectangle;
        Fill = fill;
        Border = new(new SolidColorBrush(border), borderThickness);
    }
}
public class LineProperty {
    public Brush Brush;
    public double Thickness;
    public LineProperty(Brush brush, double thickness) {
        Brush = brush;
        Thickness = thickness;
    }
    public Pen GetPen() {
        return new Pen(Brush, Thickness);
    }
}