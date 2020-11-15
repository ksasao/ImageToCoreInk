SET BASE_NAME=ImageToCoreInk_0.0.2_
SET PUBLISH_PATH=.\bin\Publish\

del /s /q %PUBLISH_PATH%

SET WIN_PATH=%PUBLISH_PATH%%BASE_NAME%win10-x64
SET OSX_PATH=%PUBLISH_PATH%%BASE_NAME%%osx-x64
SET LINUX_PATH=%PUBLISH_PATH%%BASE_NAME%%linux-x64

dotnet publish -r win10-x64 -c Release -p:PublishSingleFile=true -o %WIN_PATH%
dotnet publish -r osx-x64 -c Release -p:PublishSingleFile=true -o %OSX_PATH%
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true -o %LINUX_PATH%

copy ..\..\LICENSE %WIN_PATH%
copy ..\..\LICENSE %OSX_PATH%
copy ..\..\LICENSE %LINUX_PATH%

copy ..\..\README.md %WIN_PATH%
copy ..\..\README.md %OSX_PATH%
copy ..\..\README.md %LINUX_PATH%

pushd %PUBLISH_PATH%
del /s /q *.pdb
popd