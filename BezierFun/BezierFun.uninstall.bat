@echo off
setlocal


set M9=C:\Program Files\Manifold\v9.0\extras\Addins


if exist "%M9%\BezierFun\" GOTO PROMPT
GOTO NOTHINGTOUNINSTALL

:PROMPT
SET /P AREYOUSURE=Are you sure you want to delete BezierFun ([Y]/N)?
IF /I "%AREYOUSURE%" NEQ "N" GOTO DOUNINST
GOTO DONOTWANT


:DOUNINST
cd..
del "%M9%\BezierFun\BezierFun.dll"
if exist "%M9%\BezierFun\BezierFun.dll" GOTO NOLUCK
echo BezierFun.dll deleted. 
rmdir /S /Q "%M9%\BezierFun\"
if exist "%M9%\BezierFun\" GOTO NOLUCK
goto END

:NOLUCK
echo Error: Cannot delete BezierFun.dll. Perhaps you have Manifold running.
echo %M9%\BezierFun\
GOTO END


:DONOTWANT
echo Uninstall skipped.
GOTO END

:NOTHINGTOUNINSTALL
echo Nothing to uninstall
echo %M9%\BezierFun\ directory does not exist
GOTO END

:END
endlocal
pause
