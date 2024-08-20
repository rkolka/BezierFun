using Manifold;
using System.IO;


/// <summary>
/// Everything static goes into one happy class Script
/// 
/// This part implements:
///  * GeomToBytes
///  * GeomFromBytes
///  * GeomBytesToFile
/// </summary>
public partial class Script
{
    

    #region geom bytes

    /// <summary>
    /// Manifold geometry as bytes
    /// One of the few functions in .NET but not in SQL
    /// </summary>
    /// <param name="geom"></param>
    /// <returns></returns>
    public static byte[] GeomToBytes(Geom geom)
    {
        byte[] bytes = geom.GetBytes();
        return bytes;
    }

    /// <summary>
    /// From bytes to Manifold geometry
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static Geom GeomFromBytes(byte[] bytes)
    {
        using (Manifold.TypeConverter converter = Manifold.Application.CreateTypeConverter())
        {
            Geom geom = (Manifold.Geom)converter.Convert(bytes, typeof(Manifold.Geom));
            return geom;
        }

    }


    /// <summary>
    /// Writes Geom bytes to file
    /// </summary>
    /// <param name="geom"></param>
    /// <param name="path">folder path, filename is synthetic</param>
    /// <returns></returns>
    public static int GeomBytesToFile(Geom geom, string path)
    {
        byte[] bytes = geom.GetBytes();
        // write out to file.
        string dirPath = System.IO.Path.GetDirectoryName(path);
        File.WriteAllBytes($"{dirPath}\\{geom.Type}{(geom.HasZ ? "Z" : "")}{(geom.HasCurves ? "C" : "")}_o{geom.Opts}_b{geom.Branches.Count}_c{geom.Coords.Count}_{geom.GetHashCode()}.geom", bytes);
        return bytes.Length;
    }
    #endregion

}

