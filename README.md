# [:floppy_disk: Click here to download the add-in](https://raw.githubusercontent.com/rkolka/BezierFun/master/BezierFun.zip "Click here to download")
# BezierFun
Inspired by NetTopologySuite. Implemented in C# and exposed as Manifold® SQL functions.

## Why?
Fun.

## Usige
Download add-in [(link)](https://raw.githubusercontent.com/rkolka/BezierFun/master/BezierFun.zip) and unpack it under ~\extras\.

These files are imported into your project if you run it from Tools->Add-ins->BezierFun:
* BezierFun.sql



Include [BezierFunGEOM.sql] in your queries. 
```sql
-- $manifold$
-- $include$ [BezierFunGEOM.sql]

...your query...
```

You can also look at BezierFunTest.sql and BezierFun.map
