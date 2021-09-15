# Version 0.1
# Publish bat file for generate the publish folder for the server.

dotnet publish -o ./Publish

echo "Copying Data"
mkdir ./Publish/Data
cp -R ./bin/Data ./Publish/

echo "Copying Settngs"
cp -rf ./bin/settings.ini ./Publish

echo "Copying zlib"
cp -rf ./bin/zlib.dll ./Publish
cp -rf ./bin/zlib32.dll ./Publish
cp -rf ./bin/zlib64.dll ./Publish