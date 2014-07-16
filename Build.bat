@echo off

set SLN="DryIoc.v12.sln"
set OUTDIR="..\bin\Release"

echo:
echo:Building %SLN% into %OUTDIR% ..
echo:

rem MSBuild 32-bit operating systems:
rem HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\12.0

for /f "tokens=2*" %%S in ('reg query HKLM\SOFTWARE\Wow6432Node\Microsoft\MSBuild\ToolsVersions\12.0 /v MSBuildToolsPath') do (
if exist "%%T" (

echo:
echo:Using MSBuild from "%%T"
echo:

"%%T\MSBuild.exe" DryIoc.v12.sln /t:Rebuild /p:OutDir=%OUTDIR% /p:Configuration=Release /m:4 /p:BuildInParallel=true
))

if not "%1"=="-nopause" pause