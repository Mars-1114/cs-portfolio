using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace Charting.Models;

public static class Property {
    public static readonly AttachedProperty<int> IndexProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, int>("Index");
    /// <summary>
    /// Get the index of the BPMNode the radio button is linked to
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static int GetIndex(Control element) {
        return element.GetValue(IndexProperty);
    }
    /// <summary>
    /// Set the index of the BPMNode the radio button is linked to
    /// </summary>
    /// <param name="element"></param>
    /// <param name="value"></param>
    public static void SetIndex(Control element, int value) {
        element.SetValue(IndexProperty, value);
    }

    public static readonly AttachedProperty<int> LaneProperty = 
        AvaloniaProperty.RegisterAttached<Control, Control, int>("Lane");
    public static int GetLane(Control element) {
        return element.GetValue(LaneProperty);
    }
    public static void SetLane(Control element, int value) {
        element.SetValue(LaneProperty, value);
    }

    public static readonly AttachedProperty<Rectangle?> TrackNoteTailProperty = 
        AvaloniaProperty.RegisterAttached<RadioButton, Control, Rectangle?>("TrackNoteTail");
    public static Rectangle? GetTrackNoteTail(Control element) {
        return element.GetValue(TrackNoteTailProperty);
    }
    public static void SetTrackNodeTail(Control element, Rectangle? tailNode) {
        element.SetValue(TrackNoteTailProperty, tailNode);
    }
}