-- $manifold$

FUNCTION GeomBezierControlsAlphaSkew(@geom GEOM, @alpha FLOAT64, @skew FLOAT64) GEOM 
  AS SCRIPT FILE 'BezierFun.dll' ENTRY 'Script.GeomBezierControlsAlphaSkew';

FUNCTION GeomBezierWithControls(@geom GEOM, @controls GEOM) GEOM
  AS SCRIPT FILE 'BezierFun.dll' ENTRY 'Script.GeomBezierWithControls';

FUNCTION GeomBezierControls(@geom GEOM) GEOM
  AS SCRIPT FILE 'BezierFun.dll' ENTRY 'Script.GeomBezierControls';


FUNCTION GeomBezierControlsRadius(@geom GEOM) GEOM
  AS SCRIPT FILE 'BezierFun.dll' ENTRY 'Script.GeomBezierControlsRadius';

FUNCTION GeomBezier(@geom GEOM) GEOM
  AS SCRIPT FILE 'BezierFun.dll' ENTRY 'Script.GeomBezier';