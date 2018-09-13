set BUILD_BUILDNUMBER=1.1.0-local

SET bindir="%~dp0"
CD Coreddns.Core
dotnet clean -c release

CD %bindir%
CD Coreddns.Web
dotnet clean -c release
dotnet publish -c release --runtime centos.7-x64

REM builded at Coreddns.Web\bin\release\netcoreapp2.1\centos.7-x64\publish

SET sevenzip="C:\Program Files\7-Zip\7z.exe"
SET publishdir="Coreddns.Web\bin\release\netcoreapp2.1\centos.7-x64\publish"

CD %bindir%
CD %publishdir%
%sevenzip% a C:\temp\Coreddns.Web-%BUILD_BUILDNUMBER%.zip -r .

CD %bindir%
