using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

using Charting.Source;

namespace Charting.Models;

/// <summary>
/// A converter to numeric values with null and empty string protection
/// </summary>
public class SafeNumericConverter : IValueConverter {
    // Data to Control
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value;
    }
    // Control to Data
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string) {
            if (float.TryParse((string)value, out _)) {
                return value;
            }
            else {
                return 0;
            }
        }
        else {
            if (value != null) {
                return value;
            }
            else {
                return 1;
            }
        }
    }
}

/// <summary>
/// A converter for Beat to numeric values with null and empty string protection
/// </summary>
public class SafeNumericBeatConverter : IValueConverter {
    // Data to Control
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value;
    }
    // Control to Data
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string) {
            float o;
            if (float.TryParse((string)value, out o) && o >=0) {
                return value;
            }
            else {
                return 0;
            }
        }
        else {
            throw new NotSupportedException();
        }
    }
}

/// <summary>
/// A converter for BPM to numeric values with null and empty string protection
/// </summary>
public class SafeNumericConverterForBPM : IValueConverter {
    // Data to Control
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value;
    }
    // Control to Data
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string) {
            float o;
            if (float.TryParse((string)value, out o) && o >= 1) {
                return o;
            }
            else {
                return 1;
            }
        }
        else {
            throw new NotSupportedException();
        }
    }
}

/// <summary>
/// A converter to invert the boolean value
/// <para> READ ONLY </para>
/// </summary>
public class BooleanInverseConverter : IValueConverter {
    // Data to Control
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is bool v) {
            return !v;
        }
        else {
            throw new NotSupportedException();
        }
    }
    // Control to Data
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}

/// <summary>
/// A converter to handle combo box value convertions
/// </summary>
public class ComboBoxSelectionConverter : IValueConverter {
    // Data to Control
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string type && parameter is string mode) {
            switch(mode){
                case "simple":
                    switch(type) {
                        case "hold":
                            return 0;
                        case "linear":
                            return 1;
                        default:
                            throw new NotSupportedException();
                    }
                case "complex":
                    switch(type) {
                        case "hold":
                            return 0;
                        case "linear":
                            return 1;
                        case "easeInOut":
                            return 2;
                        default:
                            throw new NotSupportedException();
                    }
                case "note":
                    switch(type) {
                        case "tap":
                            return 0;
                        case "track":
                            return 1;
                        case "clap":
                            return 2;
                        case "punch":
                            return 3;
                        case "avoid":
                            return 4;
                        default:
                            return BindingOperations.DoNothing;
                    }
                default:
                    throw new NotSupportedException();
            }
        }
        if (value is NodeType control) {
            switch(control) {
                case NodeType.Position:
                    return 0;
                case NodeType.Alpha:
                    return 1;
                case NodeType.Speed:
                    return 2;
                case NodeType.Endpoint:
                    return 3;
                default:
                    throw new NotSupportedException();
            }
        }
        else {
            throw new NotSupportedException();
        }
    }
    // Control to Data
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is int index && parameter is string mode) {
            switch(mode) {
                case "simple":
                    switch(index) {
                        case 0:
                            return "hold";
                        case 1:
                            return "linear";
                        default:
                            throw new NotSupportedException();
                    }
                case "complex":
                    switch(index) {
                        case 0:
                            return "hold";
                        case 1:
                            return "linear";
                        case 2:
                            return "easeInOut";
                        default:
                            throw new NotSupportedException();
                    }
                case "note":
                    switch(index) {
                        case 0:
                            return "tap";
                        case 1:
                            return "track";
                        case 2:
                            return "clap";
                        case 3:
                            return "punch";
                        case 4:
                            return "avoid";
                        default:
                            return BindingOperations.DoNothing;
                    }
                case "control":
                    switch(index) {
                        case 0:
                            return NodeType.Position;
                        case 1:
                            return NodeType.Alpha;
                        case 2:
                            return NodeType.Speed;
                        case 3:
                            return NodeType.Endpoint;
                        default:
                            throw new NotSupportedException();
                    }
                default:
                    throw new NotSupportedException();
            }
        }
        else {
            throw new NotSupportedException();
        }
    }
}

/// <summary>
/// A converter that adds 1 to the number
/// </summary>
public class AddOneConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is float v) {
            return v + 1;
        }
        else {
            return 0;
        }
    }
    // Control to Data
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string) {
            float o;
            if (float.TryParse((string)value, out o)) {
                if (parameter is string mode) {
                    if (mode == "negative") {
                        return o - 1;
                    }
                    else {
                        throw new ArgumentException($"Invalid mode \"{mode}\".");
                    }
                }
                else {
                    return o <= 1 ? 0 : o - 1;
                }
            }
            else {
                return 0;
            }
        }
        else if (value is decimal i) {
            return (i > 1) ? i - 1 : 0;
        }
        else {
            throw new NotSupportedException();
        }
    }
}

/// <summary>
/// A converter that finds the maximal lane ID
/// <para> READ ONLY </para>
/// </summary>
public class MaxLaneIDConverter : IValueConverter {
    // Data to Control
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is List<Lane> lanes) {
            return lanes.Count;
        }
        else {
            throw new NotSupportedException();
        }
    }
    // Control to Data
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}

/// <summary>
/// 
/// <para> READ ONLY </para>
/// </summary>
public class NoteNullCheckConverter : IValueConverter {
    // Data to Control
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is null) {
            return "empty";
        }
        else if (value is Note note) {
            return note.Type;
        }
        else {
            throw new NotSupportedException();
        }
    }
    // Control to Data
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}