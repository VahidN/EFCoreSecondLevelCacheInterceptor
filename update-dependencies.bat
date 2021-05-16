dotnet restore
setx DOTNET_HOST_PATH "%ProgramFiles%\dotnet\dotnet.exe" /M
dotnet tool update --global dotnet-outdated-tool
dotnet outdated
rem dotnet outdated --pre-release Always
dotnet restore
pause