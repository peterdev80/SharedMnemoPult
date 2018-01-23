@echo off

set PATH=%PATH%;%windir%\Microsoft.NET\Framework\v4.0.30319

rem set FMSObjDir=c:\zzu
rem set FMSReleaseDir=c:\zzur

msbuild s700.sln /p:Configuration=ReleaseClean /p:Platform=x86 /v:m

:end
pause