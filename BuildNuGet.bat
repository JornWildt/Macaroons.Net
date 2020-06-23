REM 1) Update versions in Macaroons.Net AssemblyInfo.cs
REM 2) Update release note in Macaroons.Net.nuspec
REM 3) Run this script

\bin\nuget.exe pack -OutputFileNamesWithoutVersion Macaroons.net
