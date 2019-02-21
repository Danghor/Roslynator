@echo off

set _roslynator="..\src\CommandLine\bin\Debug\net461\roslynator"

set _msbuildPath="C:\Program Files\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"
set _rootDirectoryUrl="../../docs/api/"

%_msbuildPath%\msbuild "..\src\CommandLine.sln" /t:Build /p:Configuration=Debug /v:m /m

%_roslynator% generate-doc "..\src\Core.sln" ^
 --msbuild-path %_msbuildPath% ^
 -o "..\docs\api" ^
 -h "Roslynator API Reference"

%_roslynator% list-symbols "..\src\Core.sln" ^
 --msbuild-path %_msbuildPath% ^
 --visibility public ^
 --depth member ^
 --ignored-parts containing-namespace ^
 --output "..\docs\api.txt"

%_roslynator% generate-doc-root "..\src\Core.sln" ^
 --msbuild-path %_msbuildPath% ^
 --projects Core ^
 -o "..\src\Core\README.md" ^
 -h "Roslynator.Core" ^
 --root-directory-url %_rootDirectoryUrl%

%_roslynator% generate-doc-root "..\src\Core.sln" ^
 --msbuild-path %_msbuildPath% ^
 --projects CSharp ^
 -o "..\src\CSharp\README.md" ^
 -h "Roslynator.CSharp" ^
 --root-directory-url %_rootDirectoryUrl%

%_roslynator% generate-doc-root "..\src\Core.sln" ^
 --msbuild-path %_msbuildPath% ^
 --projects Workspaces.Core ^
 -o "..\src\Workspaces.Core\README.md" ^
 -h "Roslynator.CSharp.Workspaces" ^
 --root-directory-url %_rootDirectoryUrl%

%_roslynator% generate-doc-root "..\src\Core.sln" ^
 --msbuild-path %_msbuildPath% ^
 --projects CSharp.Workspaces ^
 -o "..\src\CSharp.Workspaces\README.md" ^
 -h "Roslynator.CSharp.Workspaces" ^
 --root-directory-url %_rootDirectoryUrl%

pause
