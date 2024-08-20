using Manifold;
using System;
using System.IO;

public partial class Script
{
    /// <summary>
    /// Add-in name inside Manifold
    /// </summary>
    private static readonly string AddinName = "BezierFun";

    /// <summary>
    /// Add-in folder inside Manifold
    /// </summary>
    private static readonly string AddinCodeFolder = "Code\\BezierFun";

    /// <summary>
    /// Filenames that are imported to Manifold
    /// </summary>
    private static readonly string[] FilesToImport = {
        "BezierFun.sql",
        "BezierFunTest.sql", 
    };

    // even if we try to use it outside of this class. It does not work.
    private static Context Manifold;

    public static void Main()
    {
        // The current application context 
        Application app = Manifold.Application;

        string AddinDir = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

        // Import CodeFiles
        using (Database db = app.GetDatabaseRoot())
        {
            foreach (string fname in FilesToImport)
            {
                if (db.GetComponentType(fname) == "")
                {
                    string text = File.ReadAllText(AddinDir + "\\" + fname);
                    CreateQuery(app, db, fname, text, AddinCodeFolder);
                }
                else
                {
                    string message = $"{db.GetComponentType(fname).ToUpper()} {AddinCodeFolder}\\{fname} already exists.";
                    app.Log(message); 
                    app.MessageBox(message, "Warning");
                }
            }

        }
        app.Log(DisplayHelp());
        app.OpenLog();
    }

    public static string DisplayHelp()
    {
        return "Use include directive:\r\n-- $include$ [BezierFun.sql]";
    }

    public static void CreateQuery(Application app, Database db, string name, string text, string folder = "")
    {
        PropertySet propertyset = app.CreatePropertySet();
        propertyset.SetProperty("Text", text);
        if (folder != "")
        {
            propertyset.SetProperty("Folder", folder);

        }
        db.Insert(name, "query", null, propertyset);

    }
}

