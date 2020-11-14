dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
