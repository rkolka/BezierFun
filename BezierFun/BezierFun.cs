﻿using Manifold;
using System;

// System.Numerics.Vector2 has static functions like Dot, Normalize, +, -, etc.
using static System.Numerics.Vector2;
using Coord2 = Manifold.Point<double>;
// System.Numerics has Vector2 type which is like FLOAT32x2
using Vector2 = System.Numerics.Vector2;


public partial class Script
{

    //-- chosen to make curve at right-angle corners roughly circular
    private const float CIRCLE_LEN_FACTOR = (3.0f / 8.0f);

    private static readonly float default_min_segment_length = 0.0f;
    private static readonly int num_of_bezier_segments = 16;
    private static readonly float[,] interpolation_params = ComputeIterpolationParameters(num_of_bezier_segments);

    private static readonly float default_alpha = 1;
    private static float default_skew = 0;



    /// <summary>
    /// Creates a geometry of linearized Cubic Bezier Curves
    /// defined by the segments of the inputand a parameter
    /// controlling how curved the result should be, with a skew factor
    /// affecting the curve shape at each vertex.
    /// </summary>
    /// <param name="geom">The geometry defining the curve</param>
    /// <param name="alpha">The curvedness parameter (0 is linear, 1 is round, >1 is increasingly curved)</param>
    /// <param name="skew">The skew parameter (0 is none, positive skews towards longer side, negative towards shorter</param>
    /// <returns>The linearized curved geometry</returns>

    public static Geom GeomBezierControls(Geom geom) => GeomBezierControls(geom, false);
    public static Geom GeomBezierControlsRadius(Geom geom) => GeomBezierControls(geom, true);
    public static Geom GeomBezierControlsAlpha(Geom geom, double alpha) => GeomBezierControls(geom, (float)alpha, default_skew);
    public static Geom GeomBezierControlsAlphaSkew(Geom geom, double alpha, double skew) => GeomBezierControls(geom, (float)alpha, (float)skew);

    public static Geom GeomBezier(Geom geom) => CubicBezier(geom, GeomBezierControls(geom, default_alpha, default_skew));
    public static Geom GeomBezierAlpha(Geom geom, double alpha) => CubicBezier(geom, GeomBezierControls(geom, (float)alpha, default_skew));
    public static Geom GeomBezierAlphaSkew(Geom geom, double alpha, double skew) => CubicBezier(geom, GeomBezierControls(geom, (float)alpha, (float)skew));

    public static Geom GeomBezierWithControls(Geom geom, Geom controls) {

        if (CheckControls(geom, controls) == "OK")
        {
            return CubicBezier(geom, controls);
        }
        else
        {
            return geom;
        }
    }



    /// <summary>
    /// assumes controls are OK.
    /// </summary>
    /// <param name="geom">a geom</param>
    /// <param name="index">an integer</param>
    /// <param name="newCoord">a float64x2</param>
    /// <returns>New geom with one coordinate changed, or geom if index out of range.</returns>
    private static Geom CubicBezier(Geom geom, Geom controls)
    {

        if (geom == null || geom.Type == "point")
        {
            return null;
        }

        Manifold.GeomBuilder builder = Manifold.Application.CreateGeomBuilder();

        string type = geom.Type;

        switch (type)
        {
            case "line":
                builder.StartGeomLine();
                break;
            case "area":
                builder.StartGeomArea();
                break;
            default:
                return null;
        }

        int control_index = -1;
        Geom.CoordSet control_coords = controls.Coords;

        for (int branch = 0; branch < geom.Branches.Count; ++branch)
        {
            builder.AddBranch();

            Manifold.Geom.CoordSet coords = geom.Branches[branch].Coords;
            for (int i = 0; i < coords.Count - 1; ++i)
            {
                control_index++;
                Coord2 p0 = coords[i];
                Coord2 p1 = coords[i + 1];
                Coord2 ctrl0 = control_coords[4 * control_index + 1];
                Coord2 ctrl1 = control_coords[4 * control_index + 3];

                AddCoords(builder, BuildBezierCurve(p0, p1, ctrl0, ctrl1));
            }

            Coord2 last = coords[coords.Count - 1];
            builder.AddCoord(last);

            builder.EndBranch();
        }

        return builder.EndGeom();
    }

    /// <summary>
    /// Finds default control points for a Geom
    /// First and last coord of any branch have 1 control point(line)
    /// Middle coords have 2 control points(lines)
    /// Other way to look would be that any segment has 2 control points
    /// And those depend on neighbouring segments.
    /// </summary>
    /// <param name="geom"></param>
    /// <param name="alpha"></param>
    /// <param name="skew"></param>
    /// <returns></returns>
    public static Manifold.Geom GeomBezierControls(Manifold.Geom geom, float alpha, float skew)
    {
        if (geom == null || geom.Type == "point")
        {
            return null;
        }

        Manifold.GeomBuilder builder = Manifold.Application.CreateGeomBuilder();

        string type = geom.Type;
        switch (type)
        {
            case "line":
                builder.StartGeomLine();
                break;
            case "area":
                builder.StartGeomLine();
                break;
            default:
                return null;
        }


        // for every coord (and its prec and succ coord, make a line)
        // for first and last one, there must be another way
        // also depending if isRing
        for (int branch = 0; branch < geom.Branches.Count; ++branch)
        { 
            Manifold.Geom.CoordSet coords = geom.Branches[branch].Coords;
            if (coords.Count >= 3)
            {
                // deterine if this branch makes a ring
                Coord2 first = coords[0];
                Coord2 last = coords[coords.Count - 1];
                bool is_ring = Equals(first, last);

                // first and last are always special but different ways whether is_ring on not
                if (is_ring)
                {
                    // This branch is ring and we can take the previous from the end of the branch (NB! the last one equals the first, so we need next to last)
                    Coord2 prev = coords[coords.Count - 2];
                    Coord2 curr = first;
                    Coord2 next = coords[1];
                    AddBranch(builder, ControlLine(prev, curr, next, alpha, skew, false));
                } 
                else
                {
                    // This branch is no ring and we must fake the "previous" 
                    // we give prev and next in opposite order and ask for reflected coord (last argument == true )
                    Coord2 prev = coords[2];
                    Coord2 curr = coords[1];
                    Coord2 next = first;
                    AddBranch(builder, ControlLine(prev, curr, next, alpha, skew, true));
                }

                // middle ones are "standard"
                for (int i = 1; i < coords.Count - 1; ++i)
                {
                    Coord2 prev = coords[i - 1];
                    Coord2 curr = coords[i];
                    Coord2 next = coords[i + 1];
                    // find both ways
                    
                    // "backwards", no reflection
                    AddBranch(builder, ControlLine(next, curr, prev, alpha, skew, false));
                    // "forwards", no reflection
                    AddBranch(builder, ControlLine(prev, curr, next, alpha, skew, false));
                }

                // first and last are always special but different ways whether is_ring on not
                if (is_ring)
                {
                    // This branch is ring and we can take the "next" from the start of the branch 
                    
                    Coord2 prev = coords[coords.Count - 2];
                    Coord2 curr = last;
                    Coord2 next = coords[1];
                    // we need only "backwards" control, so next, curr, prev
                    AddBranch(builder, ControlLine(next, curr, prev, alpha, skew, false));
                }
                else
                {
                    // We find the "forwards" control of next to last point and ask to reflect it
                    Coord2 prev = coords[coords.Count - 3];
                    Coord2 curr = coords[coords.Count - 2];
                    Coord2 next = last;
                    AddBranch(builder, ControlLine(prev, curr, next, alpha, skew, true));
                }

            }
            else
            {
                return null;
            }
        }

        return builder.EndGeom();

    }


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
        
        for (int i=0; i < coords.Count; ++i)
        {
            coordsArray[start + i] = coords[i];
        }

        return coordsArray;
    }

    /// 
    private static Manifold.Geom GeomBezierControls(Manifold.Geom geom, bool normal_only)
    {
        if (geom == null || geom.Type == "point")
        {
            return null;
        }

        Manifold.GeomBuilder builder = Manifold.Application.CreateGeomBuilder();

        string type = geom.Type;
        switch (type)
        {
            case "line":
                builder.StartGeomLine();
                break;
            case "area":
                builder.StartGeomLine();
                break;
            default:
                return null;
        }

        for (int branch = 0; branch < geom.Branches.Count; ++branch)
        {
            Manifold.Geom.CoordSet coordSet = geom.Branches[branch].Coords;
            if (coordSet.Count >= 3)
            {
                // make an array with extra "fake" coords at the start and end.
                Coord2[] coords = CoordArray(coordSet, true);
 
                int first = 1;
                int last = coordSet.Count;
                bool is_ring = Equals(coords[first], coords[last]);

                // find the extra coords,
                if (is_ring)
                {
                    // if the branch is a ring then extra coords are easy
                    coords[first - 1] = coords[last - 1];
                    coords[last + 1] = coords[first + 1];
                }
                else
                {
                    // we have to make mirror images for extra coords
                    coords[first - 1] = TriangleFlip(coords[first + 2], coords[first], coords[first + 1] );
                    coords[last + 1] = TriangleFlip(coords[last - 2], coords[last], coords[last - 1]);
                }

                // thanks to extra coords there is just a single loop. 
                for (int i = first; i <= last; ++i)
                {
                    Coord2 prev = coords[i - 1];
                    Coord2 curr = coords[i];
                    Coord2 next = coords[i + 1];
                    Vector2 v1 = ab2(curr, prev);
                    Vector2 v2 = ab2(curr, next);
                    Coord2[][] ctl = ControlLines(curr, v1, v2);

                    if (normal_only)
                    {
                        AddBranch(builder, ctl[0]);
                    }
                    else if (i == first)
                    {
                        AddBranch(builder, ctl[2]);
                    }
                    else if (i == last)
                    {
                        AddBranch(builder, ctl[1]);
                    }
                    else
                    {
                        AddBranch(builder, ctl[1]);
                        AddBranch(builder, ctl[2]);
                    }
                }

            }
            else
            {
                return null;
            }
        }

        return builder.EndGeom();

    }



    /// <summary>
    /// Creates a geometry of linearized Cubic Bezier Curves
    /// defined by the segments of the input
    /// and a list (or lists) of control points.
    /// </summary>
    /// <remarks>
    /// Typically the control point geometry
    /// is a <see cref="LineString"/> or <see cref="MultiLineString"/>
    /// containing an element for each line or ring in the input geometry.
    /// The list of control points for each linear element must contain two
    /// vertices for each segment (and thus <code>2 * npts - 2</code>).
    /// </remarks>
    /// <param name="geom">The geometry defining the curve</param>
    /// <param name="controlPoints">A geometry containing the control point elements.</param>
    /// <returns>The linearized curved geometry</returns>


    public static string CheckControls(Geom geom, Geom controls)
    {
        int expected_branches = 0;

        string type = geom.Type;

        for (int bi = 0; bi < geom.Branches.Count; bi++)
        {
            expected_branches += 2 * geom.Branches[bi].Coords.Count - 2;
        }
        if (controls == null )
        {
            return string.Format("Controls is NULL");
        }

        if (!(controls.Type == "line"))
        {
            return string.Format("Wrong type of geometry - expected \"line\", found \"{0}\"", controls.Type);
        }

        if (!(controls.Branches.Count == expected_branches && controls.Coords.Count == 2 * expected_branches))
        {
            return string.Format("Wrong number of control lines / points for {0} - expected {1} / {2}, found {3} / {4}", geom.Type, expected_branches, 2 * expected_branches, controls.Branches.Count, controls.Coords.Count);
        }

        return "OK";
        
    }

    /// <summary>
    /// Given segment endpoints and controlpoints 
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="ctrl0"></param>
    /// <param name="crtl1"></param>
    /// <returns></returns>
    private static Coord2[] BuildBezierCurve(
        Coord2 p0,
        Coord2 p1,
        Coord2 ctrl0,
        Coord2 crtl1
         )
    {

        Coord2[] segment_curve_buffer;

        double len = Distance(p0, p1);
        if (len < default_min_segment_length)
        {
            // segment too short - copy input coordinate
            segment_curve_buffer = new Coord2[1];
            segment_curve_buffer[0] = p1;
        }
        else
        {
            segment_curve_buffer = CubicBezierSegment(p0, p1, ctrl0, crtl1, interpolation_params);
    
        }

        return segment_curve_buffer;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="alpha"></param>
    /// <param name="skew"></param>
    /// <returns></returns>
    public static Coord2[][] ControlLines(Coord2 p0, Vector2 v1, Vector2 v2, float alpha = 1, float skew = 0)
    {

        // vectors from middle point to previous and to next point
        // their lengths
        float len_1 = v1.Length();
        float len_2 = v2.Length();
        float lenBase = Math.Min(len_1, len_2);

        // unitvectors and the "tangent" unitvector
        Vector2 v_1hat = Normalize(v1);
        Vector2 v_2hat = Normalize(v2);
        Vector2 tangent = Normalize(-v_1hat + v_2hat);
        Vector2 normal = Normalize(v_1hat + v_2hat);


        // We are going to scale the tangent 

        //-- make acute corners sharper by shortening tangent vectors
        float intAngAbs = AngleBetweenAbs2(v1, v2);
        float sharpnessFactor = intAngAbs >= TAU_OVER_4 ? 1 : intAngAbs / TAU_OVER_4;

        // 
        float len = alpha * CIRCLE_LEN_FACTOR * sharpnessFactor * lenBase;
        
        normal = len * normal;

        // 
        float stretch1 = 1f;
        float stretch2 = 1f;
        if (skew != 0)
        {
            float stretch = Math.Abs(len_1 - len_2) / Math.Max(len_1, len_2);
            int skewIndex = len_1 > len_2 ? 0 : 1;
            if (skew < 0) skewIndex = 1 - skewIndex;
            if (skewIndex == 0)
            {
                stretch1 += Math.Abs(skew) * stretch;
            }
            else
            {
                stretch2 += Math.Abs(skew) * stretch;
            }
        }

        // find the control vector
        Vector2 tangent1 = -1 * tangent * stretch1 * len;
        Vector2 tangent2 = tangent * stretch2 * len;


        Coord2[][] coords = new Coord2[3][] {
            new Coord2[2] {p0, shift2(p0, normal) },
            new Coord2[2] {p0, shift2(p0, tangent1) },
            new Coord2[2] {p0, shift2(p0, tangent2) },
        };
        return coords;

    }



    /// <summary>
    /// Finds controlpoint for p1 coming from p0 and going to p2
    /// 
    /// Given the middle point p1 with neighbouring points p0 and p2 
    /// and 
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="alpha"></param>
    /// <param name="skew"></param>
    /// <param name="reflected"></param>
    /// <returns></returns>
    public static Coord2[] ControlLine(Coord2 p0, Coord2 p1, Coord2 p2, float alpha, float skew, bool reflected)
    {

        // vectors from middle point to previous and to next point
        // their lengths
        Vector2 v_10 = ab2(p1, p0);
        Vector2 v_12 = ab2(p1, p2);
        float len_10 = v_10.Length();
        float len_12 = v_12.Length();
        float lenBase = Math.Min(len_10, len_12);

        // unitvectors and the "tangent" unitvector
        Vector2 v_10hat = Normalize(v_10);
        Vector2 v_12hat = Normalize(v_12);
        Vector2 tangent = Normalize(-v_10hat + v_12hat);

        // We are going to scale the tangent 

        //-- make acute corners sharper by shortening tangent vectors
        float intAngAbs = AngleBetweenAbs2(v_10, v_12);
        float sharpnessFactor = intAngAbs >= TAU_OVER_4 ? 1 : intAngAbs / TAU_OVER_4;

        // 
        float len = alpha * CIRCLE_LEN_FACTOR * sharpnessFactor * lenBase;

        // 
        float stretch0 = 1f;
        float stretch1 = 1f;
        if (skew != 0)
        {
            float stretch = Math.Abs(len_10 - len_12) / Math.Max(len_10, len_12);
            int skewIndex = len_10 > len_12 ? 0 : 1;
            if (skew < 0) skewIndex = 1 - skewIndex;
            if (skewIndex == 0)
            {
                stretch0 += Math.Abs(skew) * stretch;
            }
            else
            {
                stretch1 += Math.Abs(skew) * stretch;
            }
        }
        
        // find the control vector
        var ctl = tangent * stretch1 * len;

        // find the reflected point
        // used for first/last points not having prev/next 
        if (reflected)
        {
            
            ctl = Reflect(ctl, perp2(v_12hat));
            return new Coord2[] { p2, shift2(p2, -ctl) };
        }
        else
        {
            return new Coord2[] { p1, shift2(p1, ctl) };
        }
    }


    /// <summary>
    /// Calculates vertices along a cubic Bezier curve.
    /// Allocates and returns array of Coord2's
    /// contains p0. 
    /// does not contain p1. 
    /// </summary>
    /// <param name="p0">The start point</param>
    /// <param name="p1">The end point</param>
    /// <param name="ctrl1">The first control point</param>
    /// <param name="ctrl2">The second control point</param>
    /// <param name="param">A set of interpolation parameters</param>
    private static Coord2[] CubicBezierSegment(
        Coord2 p0,
        Coord2 p1,
        Coord2 ctrl1,
        Coord2 ctrl2,
        float[,] param
        )
    {
        Coord2[] curve = new Coord2[num_of_bezier_segments];
        int n = curve.Length;
        curve[0] = p0;

        for (int i = 1; i < n; i++)
        {
            Coord2 c = new Coord2();
            c.X = param[i, 0] * p0.X + param[i, 1] * ctrl1.X + param[i, 2] * ctrl2.X + param[i, 3] * p1.X;
            c.Y = param[i, 0] * p0.Y + param[i, 1] * ctrl1.Y + param[i, 2] * ctrl2.Y + param[i, 3] * p1.Y;

            curve[i] = c;
        }

        return curve;
    }

    /// <summary>
    /// Gets the interpolation parameters for a Bezier curve approximated by a
    /// given number of segments.
    /// </summary>
    /// <param name="n">The number of segments</param>
    /// <returns>An array of double[n + 1, 4] holding the parameter values</returns>
    private static float[,] ComputeIterpolationParameters(int n)
    {
        int iterations = (n >> 1) + 1;
        float[,] param = new float[n + 1, 4];
        for (int i = 0; i < iterations; i++)
        {
            float t = (float)i / n;
            float tc = 1.0f - t;
            float remaining = 1;
            float temp;
            int j = n - i;

            temp = tc * tc * tc;
            param[i, 0] = temp;
            param[j, 3] = temp;
            remaining -= temp;

            temp = 3.0f * tc * tc * t;
            param[i, 1] = temp;
            param[j, 2] = temp;
            remaining -= temp;

            temp = 3.0f * tc * t * t;
            param[i, 2] = temp;
            param[j, 1] = temp;
            remaining -= temp;

            param[i, 3] = remaining;
            param[j, 0] = remaining;

            //double diff = remaining - t * t * t;
            //Console.WriteLine(diff.ToString("R"));
        }
        return param;
    }




}
