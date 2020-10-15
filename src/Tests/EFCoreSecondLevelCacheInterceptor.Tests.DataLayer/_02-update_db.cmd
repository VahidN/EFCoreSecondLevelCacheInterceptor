dotnet tool install --global dotnet-ef --version 5.0.0-rc.2.20475.6
dotnet tool update --global dotnet-ef --version 5.0.0-rc.2.20475.6
dotnet build
dotnet ef --startup-project ../EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/ database update --context ApplicationDbContext
pause