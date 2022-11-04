@echo off

set currPath=%~dp0
set parentPath=
:beginFindParent
FOR /F "tokens=1,* delims=\" %%i IN ("%currPath%")  DO (set front=%%i)
FOR /F "tokens=1,* delims=\" %%i IN ("%currPath%")  DO (set currPath=%%j)
if not "%parentPath%" == "" goto gotJpdaOpts
:gotJpdaOpts
if "%parentPath%%front%\"=="%~dp0" goto endFindParent
set parentPath=%parentPath%%front%\
goto beginFindParent
:endFindParent
rem 获得的parentPath路径以\结尾
rem echo %parentPath%

call xcopy %cd%\react-ui-antd\dist %parentPath%AgileConfig.Server.Apisite\wwwroot\ui /s /e /Q /Y /I
pause