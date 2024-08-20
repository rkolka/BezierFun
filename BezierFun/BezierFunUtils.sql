-- $manifold$

-- Functions from Script from BezierFun.dll
-- 
--

FUNCTION GeomToBytes(@geom GEOM) VARBINARY
  AS SCRIPT FILE 'BezierFun.dll' ENTRY 'Script.GeomToBytes';

FUNCTION GeomFromBytes(@geombytes VARBINARY) GEOM
  AS SCRIPT FILE 'BezierFun.dll' ENTRY 'Script.GeomFromBytes';

FUNCTION GeomBytesToFile(@geom GEOM, @path VARCHAR) VARBINARY
  AS SCRIPT FILE 'BezierFun.dll' ENTRY 'Script.GeomBytesToFile';
