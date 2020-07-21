dotnet tool install --global dotnet-ef --version 3.1.6
dotnet tool update --global dotnet-ef --version 3.1.6
dotnet build
dotnet ef --startup-project ../EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/ database update --context ApplicationDbContext
pause