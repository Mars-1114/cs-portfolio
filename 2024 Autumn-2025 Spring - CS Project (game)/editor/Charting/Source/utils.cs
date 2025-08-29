using System;
using System.Numerics;
using Avalonia.Media;

namespace Charting.Source;

public static class Compute {
    /// <summary>
    /// Compute the integral of the line function
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static float Integral(float yA, float yB, float dx)
    {
        if (yA * yB < 0)
        {
            float lenA = Math.Abs(yA) / (Math.Abs(yA) + Math.Abs(yB)) * dx;
            return (yA * lenA + yB * (dx - lenA)) / 2;
        }
        else
        {
            return (yA + yB) * dx / 2;
        }
    }
    /// <summary>
    /// Transform a 3D point to 2D int preview.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static Vector2 ToDisplayCoordinates(Vector3 point, float distance) {
        // 1. scale x and y from chart to canvas
        // (Scale: Canvas = Chart * 22.5)
        Vector2 transformedPoint = new Vector2(point.X, -point.Y) * 22.5f * 1.5f;
        // 2. scale the x-y plane according to dz (z - distance) to simulate perspective
        // (define vanishing point at (0, 0))
        float camera = 1;
        transformedPoint *=  camera / (camera + point.Z - distance);
        // 3. tranlate x-y plane to canvas coordinates
        // (Chart Coords: (0, 0) at center)
        // (Canvas Coords: (0, 0) at top-left)
        return transformedPoint += new Vector2(480, 250);
    }
    /// <summary>
    /// Transform a 3D point in preview to 2D in chart
    /// </summary>
    /// <param name="point"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static Vector2 ToChartCoordinates(Vector3 point, float distance) {
        float camera = 1;
        Vector2 transformedPoint = new Vector2(point.X, point.Y) - new Vector2(480, 250);
        transformedPoint *= (camera + point.Z - distance) / camera;
        transformedPoint.Y *= -1;
        return transformedPoint / 22.5f / 1.5f;
    }
}

public static class Helper {
    public static Color GetColor(string noteType) {
        switch(noteType) {
            case "tap":
                return Colors.SkyBlue;
            case "track":
                return Colors.Yellow;
            case "clap":
                return Colors.LimeGreen;
            case "punch":
                return Colors.Red;
            case "avoid":
                return Colors.Gray;
            default:
                return Colors.Purple;
        }
    }
    public static Color GetTransparentColor(Color color, float alpha) {
        return Color.FromArgb((byte)Math.Max(0, Math.Min(255, (int)Math.Floor(alpha * 256.0))), color.R, color.G, color.B);
    }
}

public static class Texts {
    public static string ToTitle(string str) {
        if (str.Length > 0) {
            return str[..1].ToUpper() + str[1..];
        }
        else {
            return "";
        }
    }
    public static string GetTimeString(float sec) {
        int minute = (int)Math.Floor(sec / 60);
        float second = (float)Math.Round(sec - minute * 60, 2);
        return $"{minute}:" + string.Format("{0:0.00}", second);
    }
}