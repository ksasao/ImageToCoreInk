SET BASE_NAME=ImageToCoreInk_0.0.2_
dotnet publish -r win10-x64 -c Release -p:PublishSingleFile=true -o ./bin/Publish/%BASE_NAME%win10-x64
dotnet publish -r osx-x64 -c Release -p:PublishSingleFile=true -o ./bin/Publish/%BASE_NAME%osx-x64
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true -o ./bin/Publish/%BASE_NAME%linux-x64
cd ./bin/Publish
del /s /q *.pdb