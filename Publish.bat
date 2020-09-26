@echo off

REM Version 0.1
REM Publish bat file for generate the publish folder for the server.

dotnet publish -o ./Publish

echo "Copying Data"
mkdir .\Publish\Data
xcopy /s .\Binaries\Data .\Publish\Data /Y

echo "Copying Settngs"
xcopy .\Binaries\settings.ini .\Publish /Y

echo "Copying zlib"
xcopy .\Binaries\zlib.dll .\Publish /Y
xcopy .\Binaries\zlib32.dll .\Publish /Y
xcopy .\Binaries\zlib64.dll .\Publish /Y