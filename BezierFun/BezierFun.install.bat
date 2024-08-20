@echo off
setlocal

set M9=C:\Program Files\Manifold\v9.0\extras\Addins

if exist "%M9%\BezierFun\" GOTO ALREADYINSTALLED
GOTO DOINST


:DOINST
echo ------- Creating directory BezierFun 
echo ------- under %M9%\ 
mkdir "%M9%\BezierFun"
if exist "%M9%\BezierFun\" GOTO COPYFILES 
GOTO CANNOTCREATEDIR

:COPYFILES
echo ------- Copying add-in Files
copy System.Numerics.Vectors.dll "%M9%\BezierFun\"

copy BezierFun.dll "%M9%\BezierFun\"
copy BezierFun.dll.addin "%M9%\BezierFun\"
copy BezierFun.map "%M9%\BezierFun\"

copy BezierFun.sql "%M9%\BezierFun\"
copy BezierFunTest.sql "%M9%\BezierFun\"

copy BezierFun.uninstall.bat "%M9%\BezierFun\"
goto END

:CANNOTCREATEDIR
echo Error: Cannot create Addin directory.
echo You must have write permission for %M9%
goto END

:ALREADYINSTALLED
echo Error: Cannot install
echo BezierFun directory already exists
echo Try running %M9%\BezierFun\BezierFun.uninstall.bat first
GOTO END

:END
endlocal
pause
