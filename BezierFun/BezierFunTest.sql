-- $manifold$
-- $include$ [BezierFun.sql]

CREATE TABLE [Results] (
  [mfd_id] INT64,
  [signature] NVARCHAR,
  [resultNumber] FLOAT64,
  [resultbOOLEAN] BOOLEAN,
  [resultGeom] GEOM,
  [resultNVARCHAR] NVARCHAR,
  INDEX [mfd_id_x] BTREE ([mfd_id]),
  INDEX [resultGeom_x] RTREE ([resultGeom])
);
CREATE DRAWING [Results Drawing] (
  PROPERTY 'FieldGeom' 'resultGeom',
  PROPERTY 'StyleArea' '{ "Value": { "Stroke": "0.5", "Style": "line" } }',
  PROPERTY 'StyleAreaColorBack' '{ "Value": -16777216 }',
  PROPERTY 'StyleAreaSize' '{ "Value": 1.25 }',
  PROPERTY 'Table' '[Results]'
);

VALUE @line GEOM = ( SELECT First([Geom]) FROM [Drawing] where GeomIsLine([Geom]) );


VALUE @alpha	          FLOAT64 = 1.2;  -- A curvedness parameter (0 is linear, 1 is round, >1 is increasingly curved)
VALUE @skew		          FLOAT64 = -0.1;  -- The skew parameter (0 is none, positive skews towards longer side, negative towards shorter


DELETE FROM [Results];
-- Control lines with alpha and skew as geometry result
INSERT INTO [Results] ([signature], [resultGeom]) SELECT 'GeomBezierControlsAlphaSkew(@line, @alpha, @skew)', GeomBezierControlsAlphaSkew(@line, @alpha, @skew) FROM (VALUES (1));

VALUE @controls GEOM = ( SELECT Last([resultGeom]) FROM [Results] ); 

-- Bezier curve with custom controls
INSERT INTO [Results] ([signature], [resultGeom]) SELECT 'GeomBezierWithControls(@line, @controls)', GeomBezierWithControls(@line, @controls) FROM (VALUES (1));

-- Default control lines as geometry result
INSERT INTO [Results] ([signature], [resultGeom]) SELECT 'GeomBezierControls(@line)', GeomBezierControls(@line) FROM (VALUES (1));

-- Experimental control line for symmetric result
INSERT INTO [Results] ([signature], [resultGeom]) SELECT 'GeomBezierControlsRadius(@line)', GeomBezierControlsRadius(@line) FROM (VALUES (1));

-- Default bezier curve
INSERT INTO [Results] ([signature], [resultGeom]) SELECT 'GeomBezier(@line)', GeomBezier(@line) FROM (VALUES (1));
