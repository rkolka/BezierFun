﻿using System;
using System.IO;
// Coord2 is like Manifold FLOAT64X2
using Coord2 = Manifold.Point<double>;
using static Script;

namespace BezierFunTest
{
    class Program
    {

        // [STAThread] // not needed since 9.0.180 (?)
        static void Main(string[] args)
        {

            String extdll = @"C:\Program Files\Manifold\v9.0\ext.dll";
            using (Manifold.Root root = new Manifold.Root(extdll))
            {
                Manifold.Application app = root.Application;
                Console.WriteLine(app.Name);
                String mapfile = Path.GetFullPath(@"m9_BezierFunTest.map");

                using (Manifold.Database db = app.CreateDatabaseForFile(mapfile, true))
                {
                    Console.WriteLine(db.Technology);

                    Script.DisplayHelp();
                    Console.WriteLine();

                    using (Manifold.Table table = db.Search("mfd_root"))
                    {
                        Console.WriteLine("Fields in mfd_root:");
                        Manifold.Schema schema = table.GetSchema();
                        foreach (Manifold.Schema.Field field in schema.Fields)
                            Console.WriteLine(field.Name);

                        Coord2 a = c2(3, 4);
                        Coord2 b = c2(5, 7);
                        Coord2 c = c2(2, 4);
                        Coord2 d = TriangleFlip(c, a, b);

                    }
                }
            }
        }
    }
}
