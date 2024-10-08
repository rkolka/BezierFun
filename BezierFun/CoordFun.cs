﻿using System;
// Coord2 is like Manifold FLOAT64X2
using Coord2 = Manifold.Point<double>;
// System.Numerics has Vector2 type which is like FLOAT32x2
using Vector2 = System.Numerics.Vector2;
// System.Numerics.Vector2 has static functions like Dot, Normalize, +, -, etc.
using static System.Numerics.Vector2;


/// <summary>
/// Everything static goes into one happy class Script
/// 
/// This part implements:
///  * Coord2 functions (like TriangleFlip, ...)
///  * Coord2 interactions with Vector2
///  * ...
/// </summary>
public partial class Script
{

    // Vector2 from Origin to Coord2
    public static Vector2 v2(Coord2 c) => new Vector2((float)c.X, (float)c.Y);

    // Vector2 from two doubles
    public static Vector2 v2(double x, double y) => new Vector2((float)x, (float)y);


    // Vector2 from Coord2 to Coord2
    // Subtract first and then cast double to float.
    public static Vector2 ab2(Coord2 c0, Coord2 c1) => new Vector2((float)(c1.X - c0.X), (float)(c1.Y - c0.Y));

    // Coord2 from Vector2
    public static Coord2 c2(Vector2 v) => new Coord2(v.X, v.Y);
    // Coord2 from two floats
    public static Coord2 c2(float x, float y) => new Coord2(x, y);
    // Coord2 from two doubles
    public static Coord2 c2(double x, double y) => new Coord2(x, y);

    public static bool Equals(Coord2 c0, Coord2 c1) =>  (c1.X == c0.X) && (c1.Y == c0.Y);
 

    // Coord2 shifted by Vector2
    public static Coord2 shift2(Coord2 c, Vector2 v) => new Coord2(c.X + (double)v.X, c.Y + (double)v.Y);


    public static double Distance(Coord2 a, Coord2 b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Reflect point C across the perpendicular bisector of segment AB.
    /// In other words finds the tip of triangle `abc` if baseline `ab` is flipped.
    /// </summary>
    /// <param name="c">tip to reflect</param>
    /// <param name="a">first tip of baseline</param>
    /// <param name="b">second tip of baseline </param>
    /// <returns></returns>
    public static Coord2 TriangleFlip(Coord2 c, Coord2 a, Coord2 b)
    {
        Vector2 v = ab2(a, c);
        Vector2 baseline = ab2(a, b);
        Vector2 n = perp2(Normalize(baseline));
        Vector2 v2 = Prod2(Prod2(n, v), n);   // v2 = nvn
        return shift2(b, v2);
    }


    /// <summary>
    /// From Manifold.Geom.CoordSet to array of Manifold.Point<double>
    /// Possibly with extra emtpy array elements at start and end
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="extra"></param>
    /// <returns></returns>
    public static Coord2[] CoordArray(Manifold.Geom.CoordSet coords, bool extra)
    {
        Coord2[] coordsArray;

        int start = 0;

        if (extra)
        {
            coordsArray = new Coord2[coords.Count + 2];
            start = 1;
        }
        else
        {
            coordsArray = new Coord2[coords.Count];
        }

        for (int i = 0; i < coords.Count; ++i)
        {
            coordsArray[start + i] = coords[i];
        }

        return coordsArray;
    }

}

