@echo off

REM Version 0.1
REM Publish bat file for generate the publish folder for the server.

dotnet publish -o ./Publish

echo "Copying Data"
mkdir .\Publish\Data
xcopy /s .\bin\Data .\Publish\Data /Y

echo "Copying Settngs"
xcopy .\bin\settings.ini .\Publish /Y

echo "Copying zlib"
xcopy .\bin\zlib.dll .\Publish /Y
xcopy .\bin\zlib32.dll .\Publish /Y
xcopy .\bin\zlib64.dll .\Publish /Y