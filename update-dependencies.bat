dotnet restore
setx DOTNET_HOST_PATH "%ProgramFiles%\dotnet\dotnet.exe" /M
dotnet tool uninstall --global dotnet-outdated
dotnet tool update --global dotnet-outdated-tool
dotnet outdated -u
dotnet restore
pause