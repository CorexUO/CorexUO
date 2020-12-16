# Version 0.1
# Publish bat file for generate the publish folder for the server.

dotnet publish -o ./Publish

echo "Copying Data"
mkdir ./Publish/Data
cp -R ./Binaries/Data ./Publish/

echo "Copying Settngs"
cp -rf ./Binaries/settings.ini ./Publish

echo "Copying zlib"
cp -rf ./Binaries/zlib.dll ./Publish
cp -rf ./Binaries/zlib32.dll ./Publish
cp -rf ./Binaries/zlib64.dll ./Publish